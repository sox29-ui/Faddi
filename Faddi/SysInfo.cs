using System.Diagnostics;
using System.Management;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using static Faddi.I18n;

namespace Faddi;

public sealed record InstalledApp(string Key, string Name, string? Publisher, string? Version, long Size, string? Date, string? Location, string? Uninstall);
public sealed record Row(string Label, string Value, string? Hint = null);
public sealed record Card(string Name, List<Row> Rows);
public sealed record Section(string Id, string Title, string Icon, List<Card> Cards);

/// <summary>Everything Windows will tell about this machine, as labelled rows for the UI.</summary>
public static class SysInfo
{
    public static List<Section> Collect()
    {
        var all = new List<Section>();
        foreach (var f in new Func<Section>[] { Os, Cpu, Ram, Gpu, Board, Disks, Volumes, Network, Displays, Audio, Battery, Startup })
            try { if (f() is { Cards.Count: > 0 } s) all.Add(s); } catch { }
        return all;
    }

    static Section Os()
    {
        var o = Q("SELECT Caption, OSArchitecture, InstallDate, LastBootUpTime, CSName FROM Win32_OperatingSystem").FirstOrDefault();
        using var cv = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
        var boot = WmiDate(V(o, "LastBootUpTime"));
        var up = DateTime.Now - (boot ?? DateTime.Now);
        GetFirmwareType(out int fw);
        var secure = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Control\SecureBoot\State", "UEFISecureBootEnabled", null);
        return new("os", T("النظام"), "🪟", [C("Windows",
            R(T("النظام"), V(o, "Caption").Replace("Microsoft ", "")),
            R(T("الإصدار"), $"{cv?.GetValue("DisplayVersion")} (Build {cv?.GetValue("CurrentBuild")}.{cv?.GetValue("UBR")})", T("رقم البناء يتغيّر مع كل تحديث تراكمي من ويندوز")),
            R(T("المعمارية"), V(o, "OSArchitecture")),
            R(T("اسم الجهاز"), V(o, "CSName")),
            R(T("اسم المستخدم"), Environment.UserName),
            R(T("تاريخ تثبيت ويندوز"), Day(WmiDate(V(o, "InstallDate")))),
            R(T("آخر تشغيل"), T("{0}، والجهاز شغّال من {1}", Day(boot), string.Join(T(" و"), new[] { Count((int)up.TotalDays, T("يوم"), T("يومين"), T("أيام")), Count(up.Hours, T("ساعة"), T("ساعتين"), T("ساعات")) }.Where(x => x != ""))), T("إعادة التشغيل كل كم يوم تفضّي الرام وتطبّق التحديثات")),
            R(T("نوع الإقلاع"), fw switch { 2 => "UEFI", 1 => T("BIOS قديم (Legacy)"), _ => "" }),
            R("Secure Boot", secure is int sb ? (sb == 1 ? T("مفعّل") : T("مقفل")) : T("غير مدعوم"), T("حماية تمنع البرامج الخبيثة من التشغيل قبل ويندوز")),
            R(T("صلاحية البرنامج"), Api.IsAdmin ? T("مسؤول (Administrator)") : T("مستخدم عادي"), T("كمسؤول نقدر نقرأ معلومات أكثر مثل حرارة القرص وعمره")))]);
    }

    static Section Cpu()
    {
        bool hyperV = V(Q("SELECT HypervisorPresent FROM Win32_ComputerSystem").FirstOrDefault(), "HypervisorPresent") == "True";
        return new("cpu", T("المعالج"), "🧠", [.. Q("SELECT * FROM Win32_Processor").Select(p => C(V(p, "Name"),
            R(T("الشركة"), V(p, "Manufacturer")),
            R(T("الأنوية"), V(p, "NumberOfCores"), T("كل نواة تقدر تشتغل على مهمة لحالها")),
            R(T("المسارات (Threads)"), V(p, "NumberOfLogicalProcessors"), T("كل نواة تشتغل كأنها نواتين بفضل Hyper-Threading")),
            R(T("السرعة الأساسية"), $"{N(p, "MaxClockSpeed") / 1000.0:0.00} GHz", T("وقت الضغط يسرّع أكثر (Turbo)؛ السرعة الحالية تشوفها فوق")),
            R(T("ذاكرة L2"), Kb(N(p, "L2CacheSize"))),
            R(T("ذاكرة L3"), Kb(N(p, "L3CacheSize")), T("ذاكرة سريعة جداً داخل المعالج؛ كل ما زادت كان أفضل")),
            R(T("المقبس (Socket)"), V(p, "SocketDesignation")),
            R(T("المحاكاة الافتراضية"), hyperV ? T("مفعّلة (Hyper-V شغّال)") : V(p, "VirtualizationFirmwareEnabled") == "True" ? T("مفعّلة") : T("مقفلة من البايوس"),
                T("لازم تكون مفعّلة عشان WSL وDocker ومحاكي أندرويد")),
            R(T("المعرّف"), V(p, "Description"))))]);
    }

    static Section Ram()
    {
        var mods = Q("SELECT * FROM Win32_PhysicalMemory");
        var arr = Q("SELECT MemoryDevices, MaxCapacityEx FROM Win32_PhysicalMemoryArray").FirstOrDefault();
        var cards = new List<Card> { C(T("الملخص"),
            R(T("الحجم الكلي"), Gb(mods.Sum(m => N(m, "Capacity")))),
            R(T("المنافذ المستخدمة"), T("{0} من {1}", mods.Count, V(arr, "MemoryDevices")), T("لو فيه منافذ فاضية تقدر تضيف رام")),
            R(T("أقصى حجم تدعمه اللوحة"), Gb(N(arr, "MaxCapacityEx") * 1024)),
            R(T("وضع القنوات"), mods.Count >= 2 ? T("غالباً Dual Channel") : "Single Channel", T("شريحتين بنفس الحجم أسرع من شريحة وحدة"))) };
        cards.AddRange(mods.Select(m => C(V(m, "DeviceLocator"),
            R(T("الحجم"), Gb(N(m, "Capacity"))),
            R(T("النوع"), N(m, "SMBIOSMemoryType") switch { 20 => "DDR", 21 => "DDR2", 24 => "DDR3", 26 => "DDR4", 34 => "DDR5", 35 => "LPDDR5", _ => "" }),
            R(T("السرعة"), T("{0} MHz (أقصى {1} MHz)", V(m, "ConfiguredClockSpeed"), V(m, "Speed")), T("لو السرعة أقل من الأقصى، فعّل XMP من البايوس")),
            R(T("الشركة"), V(m, "Manufacturer")),
            R(T("الموديل"), V(m, "PartNumber")),
            R(T("الشكل"), N(m, "FormFactor") == 12 ? T("SODIMM (لابتوب)") : T("DIMM (مكتبي)")))));
        return new("ram", T("الرام"), "🧮", cards);
    }

    static Section Gpu()
    {
        var smi = Smi("vbios_version,power.limit,clocks.max.graphics,clocks.max.memory,pcie.link.gen.max,pcie.link.width.max");
        return new("gpu", T("كرت الشاشة"), "🎮", [.. Q("SELECT * FROM Win32_VideoController").Select(g =>
        {
            var name = V(g, "Name");
            bool nv = smi != null && name.Contains("NVIDIA", StringComparison.OrdinalIgnoreCase);
            return C(name,
                R(T("الذاكرة (VRAM)"), Gb(Vram(name) ?? N(g, "AdapterRAM")), T("ذاكرة كرت الشاشة الخاصة؛ الألعاب الثقيلة تحتاج أكثر")),
                R(T("إصدار التعريف"), V(g, "DriverVersion")),
                R(T("تاريخ التعريف"), Day(WmiDate(V(g, "DriverDate"))), T("التعريف الأحدث يحسّن أداء الألعاب")),
                R(T("الدقة الحالية"), N(g, "CurrentHorizontalResolution") > 0 ? $"{V(g, "CurrentHorizontalResolution")} × {V(g, "CurrentVerticalResolution")} @ {V(g, "CurrentRefreshRate")}Hz" : ""),
                R("VBIOS", nv ? smi![0] : ""),
                R(T("حد الطاقة"), nv ? smi![1] + " W" : ""),
                R(T("أقصى سرعة للمعالج الرسومي"), nv ? smi![2] + " MHz" : ""),
                R(T("أقصى سرعة للذاكرة"), nv ? smi![3] + " MHz" : ""),
                R("PCIe", nv ? $"Gen {smi![4]} x{smi![5]}" : "", T("المنفذ اللي ركّبت فيه الكرت")));
        })]);
    }

    static Section Board()
    {
        var b = Q("SELECT Manufacturer, Product, Version FROM Win32_BaseBoard").FirstOrDefault();
        var bios = Q("SELECT Manufacturer, SMBIOSBIOSVersion, ReleaseDate FROM Win32_BIOS").FirstOrDefault();
        var cs = Q("SELECT Manufacturer, Model, SystemType FROM Win32_ComputerSystem").FirstOrDefault();
        return new("board", T("اللوحة الأم والبايوس"), "🧩", [
            C(T("اللوحة الأم"), R(T("الشركة"), V(b, "Manufacturer")), R(T("الموديل"), V(b, "Product")), R(T("الإصدار"), V(b, "Version")),
                R(T("الجهاز"), $"{V(cs, "Manufacturer")} {V(cs, "Model")}"), R(T("نوع النظام"), V(cs, "SystemType"))),
            C(T("البايوس"), R(T("الشركة"), V(bios, "Manufacturer")), R(T("الإصدار"), V(bios, "SMBIOSBIOSVersion"), T("تحديث البايوس يصلح مشاكل ويدعم قطع أحدث")),
                R(T("التاريخ"), Day(WmiDate(V(bios, "ReleaseDate")))))]);
    }

    static Section Disks()
    {
        const string ns = @"root\Microsoft\Windows\Storage";
        var rel = Q("SELECT * FROM MSFT_StorageReliabilityCounter", ns); // admin only
        return new("disks", T("الأقراص"), "💽", [.. Q("SELECT * FROM MSFT_PhysicalDisk", ns).Select(d =>
        {
            var r = rel.FirstOrDefault(x => V(x, "DeviceId") == V(d, "DeviceId"));
            return C(V(d, "FriendlyName"),
                R(T("الحجم"), Gb(N(d, "Size"))),
                R(T("النوع"), N(d, "MediaType") switch { 3 => T("HDD (قرص ميكانيكي)"), 4 => "SSD", 5 => "SCM", _ => T("غير محدد") }),
                R(T("الواجهة"), N(d, "BusType") switch { 17 => T("NVMe (الأسرع)"), 11 => "SATA", 7 => "USB", 8 => "RAID", 10 => "SAS", 12 => "SD", _ => V(d, "BusType") }),
                R(T("الحالة"), N(d, "HealthStatus") switch { 0 => T("سليم ✅"), 1 => T("تحذير ⚠️"), 2 => T("غير سليم ❌"), _ => T("غير معروف") }, T("تقييم ويندوز لصحة القرص")),
                R(T("الفيرموير"), V(d, "FirmwareVersion")),
                R(T("الرقم التسلسلي"), V(d, "SerialNumber")),
                R(T("الحرارة والعمر"), r != null ? "" : Api.IsAdmin ? T("غير متاحة لهذا القرص") : T("شغّل البرنامج كمسؤول عشان تشوفها")),
                R(T("الحرارة"), r != null && N(r, "Temperature") > 0 ? T("{0}°C (أعلى قراءة {1}°C)", N(r, "Temperature"), N(r, "TemperatureMax")) : "", T("فوق 70° لفترات طويلة تضر الـSSD")),
                R(T("الاستهلاك"), r != null ? $"{N(r, "Wear")}%" : "", T("كم استهلكت من عمر الـSSD؛ 100% يعني خلص عمره الافتراضي")),
                R(T("ساعات التشغيل"), r != null ? T("{0} ساعة", N(r, "PowerOnHours").ToString("N0")) : ""),
                R(T("أخطاء القراءة / الكتابة"), r != null ? $"{N(r, "ReadErrorsTotal")} / {N(r, "WriteErrorsTotal")}" : ""));
        })]);
    }

    static Section Volumes() => new("volumes", T("البارتشنات"), "🗂️", [.. DriveInfo.GetDrives().Where(d => d.IsReady).Select(d => C($"{d.Name} {d.VolumeLabel}",
        R(T("الحجم"), Gb(d.TotalSize)),
        R(T("المستخدم"), $"{Gb(d.TotalSize - d.TotalFreeSpace)} ({100.0 * (d.TotalSize - d.TotalFreeSpace) / d.TotalSize:0}%)"),
        R(T("الفاضي"), Gb(d.TotalFreeSpace), T("خلّ 10% على الأقل فاضية عشان ويندوز يشتغل بسلاسة")),
        R(T("نظام الملفات"), d.DriveFormat),
        R(T("النوع"), d.DriveType switch { DriveType.Fixed => T("قرص داخلي"), DriveType.Removable => T("قرص خارجي أو فلاش"), DriveType.Network => T("شبكة"), _ => d.DriveType.ToString() })))]);

    static Section Network() => new("net", T("الشبكة"), "🌐", [.. NetworkInterface.GetAllNetworkInterfaces()
        .Where(n => n.OperationalStatus == OperationalStatus.Up && n.NetworkInterfaceType != NetworkInterfaceType.Loopback)
        .OrderBy(n => n.NetworkInterfaceType is NetworkInterfaceType.Ethernet or NetworkInterfaceType.Wireless80211 ? 0 : 1)
        .Select(n =>
        {
            var ip = n.GetIPProperties();
            var st = n.GetIPStatistics();
            return C(n.Name,
                R(T("الوصف"), n.Description),
                R(T("النوع"), n.NetworkInterfaceType switch { NetworkInterfaceType.Wireless80211 => "Wi-Fi", NetworkInterfaceType.Ethernet or NetworkInterfaceType.GigabitEthernet => T("سلكي (Ethernet)"), _ => T("شبكة وهمية (VPN أو آلة افتراضية)") }),
                R(T("السرعة"), n.Speed >= 1_000_000_000 ? $"{n.Speed / 1e9:0.#} Gbps" : $"{n.Speed / 1e6:0} Mbps", T("سرعة الاتصال بالراوتر، مو سرعة الإنترنت")),
                R(T("عنوان IPv4"), string.Join(T("، "), ip.UnicastAddresses.Where(a => a.Address.AddressFamily == AddressFamily.InterNetwork).Select(a => a.Address))),
                R(T("البوابة"), string.Join(T("، "), ip.GatewayAddresses.Select(g => g.Address))),
                R("DNS", string.Join(T("، "), ip.DnsAddresses)),
                R("MAC", string.Join("-", n.GetPhysicalAddress().GetAddressBytes().Select(b => b.ToString("X2")))),
                R(T("المستلم / المرسل"), $"{Gb(st.BytesReceived)} / {Gb(st.BytesSent)}", T("من آخر تشغيل للجهاز")));
        })]);

    static Section Displays()
    {
        var models = Q("SELECT UserFriendlyName FROM WmiMonitorID", @"root\wmi").Select(m => m["UserFriendlyName"] is ushort[] a ? new string([.. a.TakeWhile(c => c != 0).Select(c => (char)c)]) : "").ToList();
        return new("display", T("الشاشات"), "🖥️", [.. Screen.AllScreens.Select((s, i) => C(i < models.Count && models[i] != "" ? models[i] : T("شاشة {0}", i + 1),
            R(T("الدقة"), $"{s.Bounds.Width} × {s.Bounds.Height}"),
            R(T("أساسية"), s.Primary ? T("نعم") : T("لا")),
            R(T("عمق الألوان"), s.BitsPerPixel + " bit")))]);
    }

    static Section Audio() => new("audio", T("الصوت"), "🔊",
        [C(T("أجهزة الصوت"), [.. Q("SELECT Name FROM Win32_SoundDevice").Select((d, i) => R(T("جهاز {0}", i + 1), V(d, "Name")))])]);

    static Section Battery() => new("battery", T("البطارية"), "🔋", [.. Q("SELECT Name, EstimatedChargeRemaining, BatteryStatus FROM Win32_Battery").Select(b => C(V(b, "Name"),
        R(T("الشحن"), V(b, "EstimatedChargeRemaining") + "%"),
        R(T("الحالة"), N(b, "BatteryStatus") == 2 ? T("على الشاحن") : T("على البطارية"))))]);

    static Section Startup()
    {
        var rows = new List<Row?>();
        void Reg(RegistryKey root, string run, string approved)
        {
            using var k = root.OpenSubKey(run);
            using var ok = root.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Explorer\StartupApproved\" + approved);
            foreach (var name in k?.GetValueNames() ?? [])
                rows.Add(R(name, ok?.GetValue(name) is byte[] b && b.Length > 0 && b[0] % 2 == 1 ? T("معطّل") : T("مفعّل"), k!.GetValue(name)?.ToString()));
        }
        Reg(Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Run", "Run");
        Reg(Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", "Run");
        Reg(Registry.LocalMachine, @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Run", "Run32");
        foreach (var dir in new[] { Environment.SpecialFolder.Startup, Environment.SpecialFolder.CommonStartup }.Select(Environment.GetFolderPath).Where(Directory.Exists))
            foreach (var f in Directory.GetFiles(dir, "*.lnk"))
                rows.Add(R(Path.GetFileNameWithoutExtension(f), T("مفعّل"), f));
        return new("startup", T("برامج تشتغل مع ويندوز"), "🚀", [C(T("كل برنامج هنا ياخذ رام من أول ما يشتغل الجهاز"), [.. rows])]);
    }

    public static List<InstalledApp> Apps()
    {
        var list = new List<InstalledApp>();
        foreach (var (root, path) in new[] {
            (Registry.LocalMachine, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall"),
            (Registry.LocalMachine, @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall"),
            (Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Uninstall") })
        {
            using var k = root.OpenSubKey(path);
            foreach (var id in k?.GetSubKeyNames() ?? [])
            {
                using var a = k!.OpenSubKey(id);
                if (a?.GetValue("DisplayName") is not string name || a.GetValue("SystemComponent") is 1 || a.GetValue("ParentKeyName") != null) continue;
                long kb = a.GetValue("EstimatedSize") is int n ? (uint)n : 0;
                list.Add(new(root.Name + "\\" + path + "\\" + id, name.Trim(), a.GetValue("Publisher") as string, a.GetValue("DisplayVersion") as string,
                    kb * 1024, a.GetValue("InstallDate") as string, (a.GetValue("InstallLocation") as string)?.Trim('"', ' ').TrimEnd('\\') is { Length: > 0 } loc ? loc : null,
                    a.GetValue("UninstallString") as string));
            }
        }
        return [.. list.Where(a => a.Uninstall != null).DistinctBy(a => a.Name + a.Version)];
    }

    // by full path: a bare name would also run an nvidia-smi.exe that sits next to Faddi.exe, in Downloads say
    static readonly string? SmiPath = new[] { Environment.SystemDirectory, Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "NVIDIA Corporation", "NVSMI") }
        .Select(d => Path.Join(d, "nvidia-smi.exe")).FirstOrDefault(File.Exists);

    /// <summary>One line of nvidia-smi values, or null when there is no NVIDIA card.</summary>
    public static string[]? Smi(string fields)
    {
        if (SmiPath == null) return null;
        try
        {
            using var p = Process.Start(new ProcessStartInfo(SmiPath, $"--query-gpu={fields} --format=csv,noheader,nounits")
            { RedirectStandardOutput = true, CreateNoWindow = true, UseShellExecute = false })!;
            var line = p.StandardOutput.ReadLine();
            p.WaitForExit(3000);
            return line?.Split(',').Select(x => x.Trim()).ToArray();
        }
        catch { return null; }
    }

    static long? Vram(string adapter)
    {
        using var cls = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\Class\{4d36e968-e325-11ce-bfc1-08002be10318}");
        foreach (var sub in cls?.GetSubKeyNames() ?? [])
            try
            {
                using var k = cls!.OpenSubKey(sub);
                if (k?.GetValue("DriverDesc") as string != adapter) continue;
                return k.GetValue("HardwareInformation.qwMemorySize") switch
                {
                    long l => l,
                    byte[] b when b.Length >= 8 => BitConverter.ToInt64(b),
                    _ => k.GetValue("HardwareInformation.MemorySize") switch { int i => (uint)i, byte[] b when b.Length >= 4 => BitConverter.ToUInt32(b), _ => null },
                };
            }
            catch { } // the "Properties" subkey is locked
        return null;
    }

    // ---------- helpers ----------

    static List<ManagementBaseObject> Q(string wql, string ns = @"root\cimv2")
    {
        try
        {
            using var s = new ManagementObjectSearcher(ns, wql);
            return [.. s.Get().Cast<ManagementBaseObject>()];
        }
        catch { return []; }
    }

    static string V(ManagementBaseObject? o, string prop)
    {
        try { return o?[prop]?.ToString()?.Trim() ?? ""; }
        catch { return ""; }
    }

    static long N(ManagementBaseObject? o, string prop) => long.TryParse(V(o, prop), out var n) ? n : 0;

    static Row? R(string label, string value, string? hint = null) => string.IsNullOrWhiteSpace(value) ? null : new(label, value, hint);

    static Card C(string name, params Row?[] rows) => new(name, [.. rows.OfType<Row>()]);

    static DateTime? WmiDate(string s)
    {
        try { return s == "" ? null : ManagementDateTimeConverter.ToDateTime(s); }
        catch { return null; }
    }

    static string Day(DateTime? d) => d?.ToString("yyyy-MM-dd") ?? "";

    // Arabic counts have dual and plural forms; other languages get "n word(s)" from the singular's translation
    static string Count(int n, string one, string two, string few) => n <= 0 ? "" : Rtl
        ? n switch { 1 => one, 2 => two, <= 10 => $"{n} {few}", _ => $"{n} {one}" }
        : $"{n} {(n == 1 ? one : few)}";

    static string Kb(long kb) => kb <= 0 ? "" : kb >= 1024 ? $"{kb / 1024.0:0.#} MB" : $"{kb} KB";

    public static string Gb(long bytes) => bytes <= 0 ? "" : bytes >= 1L << 30 ? $"{bytes / (double)(1L << 30):0.#} GB" : $"{bytes / (double)(1 << 20):0} MB";

    [DllImport("kernel32")]
    static extern bool GetFirmwareType(out int type);
}
