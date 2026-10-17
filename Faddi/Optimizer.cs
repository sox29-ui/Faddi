using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Faddi;

public sealed record OptResult(DateTime Time, long Freed, long AvailableBefore, long AvailableAfter, int Trimmed, bool Standby, bool Auto);

/// <summary>
/// Frees RAM the way memory optimizers do: asks every program to hand back the pages it holds but is not using
/// (they come back on their own when needed), and as admin also empties Windows' standby cache.
/// </summary>
public static class Optimizer
{
    static readonly object gate = new();
    static readonly List<OptResult> history = [];
    static readonly DateTime started = DateTime.Now;
    public static DateTime? Last { get; private set; }
    public static bool Running => busy;
    static volatile bool busy;
    public static event Action<OptResult>? Done;

    public static List<OptResult> History { get { lock (history) return [.. history]; } }

    /// <summary>Returns null when a run is already going.</summary>
    public static OptResult? Run(bool auto)
    {
        if (!Monitor.TryEnter(gate)) return null;
        busy = true;
        try
        {
            var s = Settings.Current;
            long before = Available();
            int trimmed = s.Trim || !(s.Standby && Api.IsAdmin) ? TrimAll() : 0; // with nothing else that can run, trimming is the default
            bool standby = s.Standby && Api.IsAdmin && PurgeStandby();
            if (s.Clipboard) ClearClipboard();
            Thread.Sleep(1200); // let Windows move the pages before measuring
            long after = Available();
            var r = new OptResult(DateTime.Now, Math.Max(0, after - before), before, after, trimmed, standby, auto);
            lock (history)
            {
                history.Add(r);
                if (history.Count > 30) history.RemoveAt(0);
            }
            Last = r.Time;
            Done?.Invoke(r);
            return r;
        }
        finally
        {
            busy = false;
            Monitor.Exit(gate);
        }
    }

    /// <summary>Called by the RAM sampler every couple of seconds.</summary>
    public static void Maybe(double usedPercent, double cpu)
    {
        var s = Settings.Current;
        if (busy || !(s.AutoLow || s.AutoEvery) || s.IdleOnly && cpu > 30) return;
        var since = DateTime.Now - (Last ?? started); // the first timed run waits a full period
        bool due = s.AutoLow && usedPercent >= s.LowPercent && since > TimeSpan.FromMinutes(2)
            || s.AutoEvery && since >= TimeSpan.FromMinutes(s.EveryMinutes);
        if (due) ThreadPool.QueueUserWorkItem(_ => Run(auto: true));
    }

    static int TrimAll()
    {
        int n = 0;
        foreach (var p in Process.GetProcesses())
            using (p)
            {
                if (p.Id is 0 or 4) continue;
                var h = OpenProcess(0x0100 | 0x1000 /*SET_QUOTA | QUERY_LIMITED_INFORMATION*/, false, p.Id);
                if (h == 0) continue;
                if (EmptyWorkingSet(h)) n++;
                CloseHandle(h);
            }
        return n;
    }

    static bool PurgeStandby()
    {
        if (!EnablePrivilege("SeProfileSingleProcessPrivilege")) return false;
        int cmd = 3; // MemoryFlushModifiedList: write pending pages out first so they can be freed too
        NtSetSystemInformation(80 /*SystemMemoryListInformation*/, ref cmd, sizeof(int));
        cmd = 4;     // MemoryPurgeStandbyList
        return NtSetSystemInformation(80, ref cmd, sizeof(int)) == 0;
    }

    static void ClearClipboard()
    {
        var t = new Thread(() => { try { System.Windows.Forms.Clipboard.Clear(); } catch { } });
        t.SetApartmentState(ApartmentState.STA);
        t.Start();
        t.Join();
    }

    public static string Pretty(long bytes) => bytes >= 1L << 30 ? $"{bytes / (double)(1L << 30):0.0} GB" : $"{bytes >> 20} MB";

    public static long Available()
    {
        var m = new MEMORYSTATUSEX { dwLength = (uint)Marshal.SizeOf<MEMORYSTATUSEX>() };
        return GlobalMemoryStatusEx(ref m) ? (long)m.ullAvailPhys : 0;
    }

    static bool EnablePrivilege(string name)
    {
        if (!OpenProcessToken(-1 /*this process*/, 0x20 | 0x8 /*ADJUST_PRIVILEGES | QUERY*/, out var token)) return false;
        try
        {
            if (!LookupPrivilegeValueW(null, name, out long luid)) return false;
            var tp = new TOKEN_PRIVILEGES { Count = 1, Luid = luid, Attributes = 2 /*SE_PRIVILEGE_ENABLED*/ };
            return AdjustTokenPrivileges(token, false, ref tp, 0, 0, 0) && Marshal.GetLastWin32Error() == 0;
        }
        finally { CloseHandle(token); }
    }

    [StructLayout(LayoutKind.Sequential)]
    struct MEMORYSTATUSEX
    {
        public uint dwLength, dwMemoryLoad;
        public ulong ullTotalPhys, ullAvailPhys, ullTotalPageFile, ullAvailPageFile, ullTotalVirtual, ullAvailVirtual, ullAvailExtendedVirtual;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    struct TOKEN_PRIVILEGES
    {
        public uint Count;
        public long Luid;
        public uint Attributes;
    }

    [DllImport("kernel32", SetLastError = true)]
    static extern bool GlobalMemoryStatusEx(ref MEMORYSTATUSEX m);

    [DllImport("kernel32")]
    static extern nint OpenProcess(uint access, bool inherit, int pid);

    [DllImport("kernel32")]
    static extern bool CloseHandle(nint h);

    [DllImport("kernel32", EntryPoint = "K32EmptyWorkingSet")]
    static extern bool EmptyWorkingSet(nint h);

    [DllImport("ntdll")]
    static extern int NtSetSystemInformation(int infoClass, ref int info, int length);

    [DllImport("advapi32", SetLastError = true)]
    static extern bool OpenProcessToken(nint process, uint access, out nint token);

    [DllImport("advapi32", CharSet = CharSet.Unicode, SetLastError = true)]
    static extern bool LookupPrivilegeValueW(string? system, string name, out long luid);

    [DllImport("advapi32", SetLastError = true)]
    static extern bool AdjustTokenPrivileges(nint token, bool disableAll, ref TOKEN_PRIVILEGES state, int bufferLength, nint previous, nint returnLength);
}
