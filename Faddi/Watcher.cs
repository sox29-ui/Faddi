using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace Faddi;

public sealed record ProcGroup(string Name, int Count, long Ram, double Cpu);
public sealed record Grower(string Name, long Delta);
public sealed record Spike(DateTime Time, long From, long To, List<Grower> Culprits, long Kernel);

/// <summary>Samples CPU, RAM, GPU, disk and network every 2 s and keeps a log of sudden RAM jumps with their culprits.</summary>
public sealed class Watcher
{
    static readonly string LogPath = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Faddi", "spikes.json");
    static readonly HashSet<string> Untouchable = new(StringComparer.OrdinalIgnoreCase)
    {
        "System", "Idle", "Registry", "smss", "csrss", "wininit", "winlogon", "services", "lsass", "LsaIso", "svchost", "dwm", "fontdrvhost",
        "Memory Compression", "MsMpEng", "Secure System", "sihost", "ctfmon", "WmiPrvSE", "audiodg", "Faddi", "msedgewebview2", "explorer",
        "vmmem", "vmmemWSL", "vmcompute", "vmwp", // a WSL/Docker VM is stopped with "wsl --shutdown", not killed
    };

    public object? Latest { get; private set; }
    public double UsedPercent { get; private set; }

    readonly object gate = new();
    readonly List<long[]> history = [];   // [unix ms, used, commit], last 30 minutes
    readonly Queue<Snap> recent = new();  // last ~70 s: who grew during a jump
    readonly Queue<Snap> minutes = new(); // one per minute for 30 minutes: slow leaks
    readonly List<Spike> spikes = Load();
    readonly Dictionary<int, long> cpuTimes = [];
    List<ProcGroup> groups = [];
    long lastStamp, asked;

    sealed record Snap(DateTime T, long Used, long Kernel, Dictionary<int, (string Name, long Ram)> Procs);

    public Watcher() => new Thread(Loop) { IsBackground = true, Priority = ThreadPriority.BelowNormal }.Start();

    /// <summary>The latest numbers for the page.</summary>
    public object? Live()
    {
        asked = Environment.TickCount64;
        return Latest;
    }

    void Loop()
    {
        var pdh = new Pdh();
        string[]? smi = null;
        for (int tick = 0; ; tick++)
        {
            try
            {
                // only while the page shows it: nvidia-smi is a new process each time and wakes a sleeping laptop GPU
                if (tick % 3 == 0) smi = Environment.TickCount64 - asked < 10_000 ? SysInfo.Smi("temperature.gpu,fan.speed,power.draw,clocks.gr,memory.total") : null;
                Tick(pdh, smi);
            }
            catch { }
            Thread.Sleep(2000);
        }
    }

    void Tick(Pdh pdh, string[]? smi)
    {
        pdh.Collect();
        var pi = new PERFORMANCE_INFORMATION { cb = (uint)Marshal.SizeOf<PERFORMANCE_INFORMATION>() };
        GetPerformanceInfo(ref pi, pi.cb);
        long pg = (long)pi.PageSize, total = (long)pi.PhysicalTotal * pg, used = total - (long)pi.PhysicalAvailable * pg;
        long paged = (long)pi.KernelPaged * pg, nonpaged = (long)pi.KernelNonpaged * pg, commit = (long)pi.CommitTotal * pg;
        var procs = ReadProcs();
        var now = DateTime.Now;
        var snap = new Snap(now, used, paged + nonpaged, procs.GroupBy(p => p.Pid).ToDictionary(g => g.Key, g => (g.First().Name, g.First().Ram)));
        // programs page back in what an optimize took for a while after it; that is neither a jump nor a leak
        bool refilling = Optimizer.Last is { } opt && now - opt < TimeSpan.FromMinutes(2);
        lock (gate)
        {
            history.Add([new DateTimeOffset(now).ToUnixTimeMilliseconds(), used, commit]);
            if (history.Count > 900) history.RemoveAt(0);
            recent.Enqueue(snap);
            while (recent.Peek().T < now.AddSeconds(-70)) recent.Dequeue();
            if (refilling) minutes.Clear();
            else if (minutes.Count == 0 || now - minutes.Last().T >= TimeSpan.FromMinutes(1)) minutes.Enqueue(snap);
            if (minutes.Count > 31) minutes.Dequeue();
            groups = [.. procs.GroupBy(p => p.Name, StringComparer.OrdinalIgnoreCase)
                .Select(g => new ProcGroup(g.Key, g.Count(), g.Sum(p => p.Ram), Math.Round(g.Sum(p => p.Cpu), 1)))
                .OrderByDescending(g => g.Ram)];
            if (!refilling) DetectSpike(snap, total);
        }
        double cpu = Math.Min(100, pdh.One(Pdh.Cpu));
        UsedPercent = used * 100.0 / total;
        Optimizer.Maybe(UsedPercent, cpu);
        Latest = new
        {
            t = new DateTimeOffset(now).ToUnixTimeMilliseconds(),
            cpu,
            cores = pdh.Many(Pdh.Cores).Where(x => !x.Name.Contains("_Total")).OrderBy(x => int.TryParse(x.Name.Split(',').Last(), out var i) ? i : 0)
                .Select(x => Math.Round(Math.Min(100, x.Value))).ToList(),
            mhz = Math.Round(pdh.One(Pdh.Freq) * pdh.One(Pdh.Perf) / 100),
            gpu = Math.Min(100, pdh.Many(Pdh.Gpu).Sum(x => x.Value)),
            vram = pdh.Many(Pdh.Vram).Sum(x => x.Value),
            vramTotal = Num(smi, 4) * (1 << 20),
            gpuTemp = Num(smi, 0), gpuFan = Num(smi, 1), gpuPower = Num(smi, 2), gpuClock = Num(smi, 3),
            diskRead = pdh.One(Pdh.DiskRead), diskWrite = pdh.One(Pdh.DiskWrite), diskActive = Math.Clamp(100 - pdh.One(Pdh.DiskIdle), 0, 100),
            netDown = pdh.Many(Pdh.NetDown).Sum(x => x.Value), netUp = pdh.Many(Pdh.NetUp).Sum(x => x.Value),
            mem = new
            {
                total, used, commit, commitLimit = (long)pi.CommitLimit * pg, paged, nonpaged,
                standby = pdh.One(Pdh.StandbyCore) + pdh.One(Pdh.StandbyNormal) + pdh.One(Pdh.StandbyReserve),
                modified = pdh.One(Pdh.Modified), free = pdh.One(Pdh.Free),
                compressed = procs.Where(p => p.Name == "Memory Compression").Sum(p => p.Ram),
            },
            procs = pi.ProcessCount, threads = pi.ThreadCount, handles = pi.HandleCount,
            uptime = Environment.TickCount64 / 1000,
        };
    }

    static double? Num(string[]? a, int i) => a != null && i < a.Length && double.TryParse(a[i], out var v) ? v : null;

    void DetectSpike(Snap now, long total)
    {
        var low = recent.Where(s => s.T >= now.T.AddSeconds(-60)).MinBy(s => s.Used)!;
        if (now.Used - low.Used < Math.Max(1L << 30, total / 12)) return;
        if (spikes.Count > 0 && now.T - spikes[^1].Time < TimeSpan.FromMinutes(2)) return;
        spikes.Add(new Spike(now.T, low.Used, now.Used, [.. Growth(low.Procs, now.Procs).Take(5)], now.Kernel - low.Kernel));
        if (spikes.Count > 50) spikes.RemoveAt(0);
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(LogPath)!);
            File.WriteAllText(LogPath, JsonSerializer.Serialize(spikes));
        }
        catch { }
    }

    // New processes count fully: they were 0 before.
    static IEnumerable<Grower> Growth(Dictionary<int, (string Name, long Ram)> before, Dictionary<int, (string Name, long Ram)> after) =>
        after.Select(kv => (kv.Value.Name, Delta: kv.Value.Ram - (before.TryGetValue(kv.Key, out var b) && b.Name == kv.Value.Name ? b.Ram : 0)))
            .GroupBy(x => x.Name, StringComparer.OrdinalIgnoreCase)
            .Select(g => new Grower(g.Key, g.Sum(x => x.Delta)))
            .Where(g => g.Delta > 50L << 20)
            .OrderByDescending(g => g.Delta);

    public object Ram()
    {
        lock (gate)
            return new
            {
                history = history.ToList(),
                spikes = spikes.AsEnumerable().Reverse().Take(30).ToList(),
                growers = minutes.Count > 1 ? Growth(minutes.Peek().Procs, recent.Last().Procs).Take(8).ToList() : [],
                growMinutes = minutes.Count > 1 ? (int)(recent.Last().T - minutes.Peek().T).TotalMinutes : 0,
                procs = groups.Take(60).ToList(),
            };
    }

    public static int Kill(string name)
    {
        if (Untouchable.Contains(name)) throw new InvalidOperationException(I18n.T("هذا من أساسيات ويندوز، قفله ممكن يطيّح الجهاز"));
        int n = 0;
        foreach (var p in Process.GetProcessesByName(name))
            using (p)
                try { p.Kill(); n++; } catch { }
        return n;
    }

    static List<Spike> Load()
    {
        try { return JsonSerializer.Deserialize<List<Spike>>(File.ReadAllText(LogPath)) ?? []; }
        catch { return []; }
    }

    List<(int Pid, string Name, long Ram, double Cpu)> ReadProcs()
    {
        long stamp = Stopwatch.GetTimestamp();
        double secs = lastStamp == 0 ? 0 : (stamp - lastStamp) / (double)Stopwatch.Frequency;
        lastStamp = stamp;
        var list = new List<(int, string, long, double)>();
        var times = new Dictionary<int, long>();
        foreach (var p in Process.GetProcesses())
            using (p)
            {
                if (p.Id == 0) continue;
                long ram = 0;
                double cpu = 0;
                var h = OpenProcess(0x1000 /*QUERY_LIMITED_INFORMATION*/, false, p.Id);
                if (h != 0)
                {
                    var m = new MEMORY_EX2 { cb = (uint)Marshal.SizeOf<MEMORY_EX2>() };
                    if (GetProcessMemoryInfo(h, ref m, m.cb)) ram = (long)m.PrivateWorkingSetSize; // what Task Manager calls "Memory"
                    if (GetProcessTimes(h, out _, out _, out long k, out long u))
                    {
                        times[p.Id] = k + u;
                        if (secs > 0 && cpuTimes.TryGetValue(p.Id, out var prev)) cpu = (k + u - prev) / 1e7 / secs / Environment.ProcessorCount * 100;
                    }
                    CloseHandle(h);
                }
                if (ram == 0) try { ram = p.WorkingSet64; } catch { }
                list.Add((p.Id, p.ProcessName, ram, Math.Max(0, cpu)));
            }
        cpuTimes.Clear();
        foreach (var (pid, t) in times) cpuTimes[pid] = t;
        return list;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct PERFORMANCE_INFORMATION
    {
        public uint cb;
        public nuint CommitTotal, CommitLimit, CommitPeak, PhysicalTotal, PhysicalAvailable, SystemCache, KernelTotal, KernelPaged, KernelNonpaged, PageSize;
        public uint HandleCount, ProcessCount, ThreadCount;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct MEMORY_EX2 // PROCESS_MEMORY_COUNTERS_EX2
    {
        public uint cb, PageFaultCount;
        public nuint PeakWorkingSetSize, WorkingSetSize, QuotaPeakPagedPoolUsage, QuotaPagedPoolUsage, QuotaPeakNonPagedPoolUsage,
            QuotaNonPagedPoolUsage, PagefileUsage, PeakPagefileUsage, PrivateUsage, PrivateWorkingSetSize;
        public ulong SharedCommitUsage;
    }

    [DllImport("psapi")]
    static extern bool GetPerformanceInfo(ref PERFORMANCE_INFORMATION pi, uint cb);

    [DllImport("kernel32")]
    static extern nint OpenProcess(uint access, bool inherit, int pid);

    [DllImport("kernel32")]
    static extern bool CloseHandle(nint h);

    [DllImport("kernel32", EntryPoint = "K32GetProcessMemoryInfo")]
    static extern bool GetProcessMemoryInfo(nint h, ref MEMORY_EX2 m, uint cb);

    [DllImport("kernel32")]
    static extern bool GetProcessTimes(nint h, out long created, out long exited, out long kernel, out long user);
}

/// <summary>Windows performance counters (the same numbers Task Manager shows).</summary>
sealed class Pdh
{
    public const string Cpu = @"\Processor Information(_Total)\% Processor Utility", Cores = @"\Processor Information(*)\% Processor Utility",
        Perf = @"\Processor Information(_Total)\% Processor Performance", Freq = @"\Processor Information(_Total)\Processor Frequency",
        Gpu = @"\GPU Engine(*engtype_3D)\Utilization Percentage", Vram = @"\GPU Adapter Memory(*)\Dedicated Usage",
        DiskRead = @"\PhysicalDisk(_Total)\Disk Read Bytes/sec", DiskWrite = @"\PhysicalDisk(_Total)\Disk Write Bytes/sec",
        DiskIdle = @"\PhysicalDisk(_Total)\% Idle Time", NetDown = @"\Network Interface(*)\Bytes Received/sec", NetUp = @"\Network Interface(*)\Bytes Sent/sec",
        StandbyCore = @"\Memory\Standby Cache Core Bytes", StandbyNormal = @"\Memory\Standby Cache Normal Priority Bytes",
        StandbyReserve = @"\Memory\Standby Cache Reserve Bytes", Modified = @"\Memory\Modified Page List Bytes", Free = @"\Memory\Free & Zero Page List Bytes";

    const uint Double = 0x200 | 0x8000; // PDH_FMT_DOUBLE | PDH_FMT_NOCAP100
    readonly nint query;
    readonly Dictionary<string, nint> counters = [];

    public Pdh()
    {
        PdhOpenQueryW(null, 0, out query);
        foreach (var p in new[] { Cpu, Cores, Perf, Freq, Gpu, Vram, DiskRead, DiskWrite, DiskIdle, NetDown, NetUp, StandbyCore, StandbyNormal, StandbyReserve, Modified, Free })
            if (PdhAddEnglishCounterW(query, p, 0, out var h) == 0) counters[p] = h;
        PdhCollectQueryData(query);
    }

    public void Collect() => PdhCollectQueryData(query);

    public double One(string path) =>
        counters.TryGetValue(path, out var h) && PdhGetFormattedCounterValue(h, Double, out _, out var v) == 0 && v.Status is 0 or 1 ? v.Value : 0;

    public List<(string Name, double Value)> Many(string path)
    {
        var res = new List<(string, double)>();
        if (!counters.TryGetValue(path, out var h)) return res;
        uint size = 0;
        if (PdhGetFormattedCounterArrayW(h, Double, ref size, out _, 0) != unchecked((int)0x800007D2) /*PDH_MORE_DATA*/) return res;
        var buf = Marshal.AllocHGlobal((int)size);
        try
        {
            if (PdhGetFormattedCounterArrayW(h, Double, ref size, out uint count, buf) != 0) return res;
            for (int i = 0; i < count; i++)
            {
                nint item = buf + i * 24; // PDH_FMT_COUNTERVALUE_ITEM_W: name pointer, status, padding, double
                if (Marshal.ReadInt32(item + 8) is 0 or 1)
                    res.Add((Marshal.PtrToStringUni(Marshal.ReadIntPtr(item)) ?? "", BitConverter.Int64BitsToDouble(Marshal.ReadInt64(item + 16))));
            }
        }
        finally { Marshal.FreeHGlobal(buf); }
        return res;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct FmtValue
    {
        public uint Status;
        public double Value;
    }

    [DllImport("pdh", CharSet = CharSet.Unicode)]
    static extern int PdhOpenQueryW(string? source, nint user, out nint query);

    [DllImport("pdh", CharSet = CharSet.Unicode)]
    static extern int PdhAddEnglishCounterW(nint query, string path, nint user, out nint counter);

    [DllImport("pdh")]
    static extern int PdhCollectQueryData(nint query);

    [DllImport("pdh")]
    static extern int PdhGetFormattedCounterValue(nint counter, uint format, out uint type, out FmtValue value);

    [DllImport("pdh", CharSet = CharSet.Unicode)]
    static extern int PdhGetFormattedCounterArrayW(nint counter, uint format, ref uint size, out uint count, nint buffer);
}
