using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using static Faddi.I18n;

namespace Faddi;

static class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        // One copy at a time: a second launch just brings the first one forward. Everyone signed in to this session may
        // open both, or a copy started as admin would make them admin-only and a normal launch would fail on them.
        var users = new SecurityIdentifier(WellKnownSidType.AuthenticatedUserSid, null);
        var ms = new MutexSecurity();
        ms.AddAccessRule(new MutexAccessRule(users, MutexRights.FullControl, AccessControlType.Allow));
        var es = new EventWaitHandleSecurity();
        es.AddAccessRule(new EventWaitHandleAccessRule(users, EventWaitHandleRights.FullControl, AccessControlType.Allow));
        Mutex single;
        EventWaitHandle showSignal;
        bool first;
        try
        {
            single = MutexAcl.Create(true, "Faddi.SingleInstance", out first, ms);
            showSignal = EventWaitHandleAcl.Create(false, EventResetMode.AutoReset, "Faddi.Show", out _, es);
        }
        catch (UnauthorizedAccessException) { return; } // made by an older Faddi running as admin: that one stays the one copy
        if (!first && !(args.Contains("--restarted") && Acquire(single)))
        {
            AllowSetForegroundWindow(-1); // the first copy may come to the front instead of just flashing in the taskbar
            showSignal.Set();
            return;
        }
        ApplicationConfiguration.Initialize();
        I18n.Set(Settings.Current.Lang);
        var form = new MainForm(showSignal);
        if (!args.Contains("--tray")) form.ShowWindow(); // started with Windows: stay next to the clock, no page, no scan
        Application.Run();
        GC.KeepAlive(single); // owning it until the end is what makes this the one copy
    }

    [DllImport("user32")]
    static extern bool AllowSetForegroundWindow(int processId);

    // the copy that asked to restart as admin is still closing; wait for it to let go
    static bool Acquire(Mutex m)
    {
        try { return m.WaitOne(TimeSpan.FromSeconds(15)); }
        catch (AbandonedMutexException) { return true; }
    }
}

sealed class MainForm : Form
{
    static readonly Color Bg = Color.FromArgb(23, 25, 28);
    readonly Api api = new();
    readonly NotifyIcon tray = new();
    WebView2? web;
    bool quitting, trayHintShown;

    public MainForm(EventWaitHandle showSignal)
    {
        RightToLeft = Rtl ? RightToLeft.Yes : RightToLeft.No;
        RightToLeftLayout = Rtl;
        // sizes here are physical pixels: scale them for the display, and never past the screen
        float k = DeviceDpi / 96f;
        var area = Screen.FromPoint(Cursor.Position).WorkingArea.Size;
        MinimumSize = new Size(Math.Min((int)(1024 * k), area.Width), Math.Min((int)(680 * k), area.Height));
        Size = new Size(Math.Min((int)(1376 * k), area.Width), Math.Min((int)(900 * k), area.Height));
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Bg;
        Icon = Icon.ExtractAssociatedIcon(Environment.ProcessPath!);
        tray.Icon = Icon;
        tray.Visible = true;
        tray.DoubleClick += (_, _) => ShowWindow();
        Retitle();
        FormClosing += (_, e) =>
        {
            if (quitting || e.CloseReason != CloseReason.UserClosing) return;
            e.Cancel = true;
            if (Settings.Current.CloseToTray) HideToTray();
            else Quit();
        };
        FormClosed += (_, _) => { if (!quitting) Quit(); }; // closed some other way (Task Manager, sign-out): leave no tray icon behind
        Optimizer.Done += r =>
        {
            if (r.Auto && Settings.Current.Notify) Post(() => Balloon(T("حرّرنا {0} من الرام", Optimizer.Pretty(r.Freed))));
        };
        var tick = new System.Windows.Forms.Timer { Interval = 3000 };
        tick.Tick += (_, _) => tray.Text = $"Faddi | {T("الرام")} {api.Watcher.UsedPercent:0}%";
        tick.Start();
        _ = Handle; // so a second launch can reach this window even before it was ever shown
        new Thread(() => { while (showSignal.WaitOne()) Post(ShowWindow); }) { IsBackground = true }.Start();
    }

    // from other threads; while the app quits the form may already be gone
    void Post(Action a)
    {
        try { BeginInvoke(a); }
        catch (InvalidOperationException) { }
    }

    /// <summary>Title and tray menu in the current language.</summary>
    public void Retitle()
    {
        Text = T("فضّي — حلّل جهازك وفضّي مساحتك") + (Api.IsAdmin ? T(" (مسؤول)") : "");
        var menu = new ContextMenuStrip { RightToLeft = Rtl ? RightToLeft.Yes : RightToLeft.No };
        menu.Items.Add(T("افتح فضّي"), null, (_, _) => ShowWindow());
        menu.Items.Add(T("حسّن الرام الحين"), null, (_, _) => Task.Run(() =>
        {
            if (Optimizer.Run(auto: false) is { } r) Post(() => Balloon(T("حرّرنا {0} من الرام", Optimizer.Pretty(r.Freed))));
        }));
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(T("خروج"), null, (_, _) => Quit());
        tray.ContextMenuStrip?.Dispose();
        tray.ContextMenuStrip = menu;
    }

    void Balloon(string text) => tray.ShowBalloonTip(3500, T("فضّي"), text, ToolTipIcon.Info);

    public void ShowWindow()
    {
        if (!Visible) Show();
        if (WindowState == FormWindowState.Minimized) WindowState = FormWindowState.Normal;
        Activate();
        if (web != null) return;
        web = new WebView2 { Dock = DockStyle.Fill, DefaultBackgroundColor = Bg };
        Controls.Add(web);
        _ = Start(web);
    }

    void HideToTray()
    {
        Hide();
        // the page is the heavy part; the scan results stay, so reopening shows them right away
        if (web != null)
        {
            var w = web;
            web = null;
            Controls.Remove(w);
            w.Dispose();
        }
        GC.Collect(2, GCCollectionMode.Aggressive, blocking: true, compacting: true);
        if (!trayHintShown) Balloon(T("فضّي شغّال جنب الساعة ويراقب الرام. افتحه من الأيقونة."));
        trayHintShown = true;
    }

    public void Quit()
    {
        quitting = true;
        tray.Visible = false;
        Application.Exit();
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        int dark = 1; // dark title bar to match the page (Windows 11)
        DwmSetWindowAttribute(Handle, 20 /*DWMWA_USE_IMMERSIVE_DARK_MODE*/, ref dark, sizeof(int));
    }

    [DllImport("dwmapi")]
    static extern int DwmSetWindowAttribute(nint hwnd, int attribute, ref int value, int size);

    async Task Start(WebView2 w)
    {
        try
        {
            // an elevated copy cannot share the browser profile of a normal one
            var data = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Faddi", Api.IsAdmin ? "WebView2-admin" : "WebView2");
            await w.EnsureCoreWebView2Async(await CoreWebView2Environment.CreateAsync(null, data));
        }
        catch (Exception ex)
        {
            if (w != web) return; // closed to the tray while it was starting
            MessageBox.Show(T("البرنامج يحتاج Microsoft Edge WebView2 Runtime.") + "\n\n" + ex.Message, T("فضّي"), MessageBoxButtons.OK, MessageBoxIcon.Error);
            Quit();
            return;
        }
        if (w != web) return; // closed to the tray while it was starting
        var core = w.CoreWebView2;
        core.Settings.IsStatusBarEnabled = false;
        core.Settings.AreDefaultContextMenusEnabled = false;
        core.Settings.IsPasswordAutosaveEnabled = false;
        core.Settings.IsGeneralAutofillEnabled = false;
        core.Settings.AreDevToolsEnabled = Environment.GetEnvironmentVariable("FADDI_DEVTOOLS") == "1";
        bool loaded = false;
        core.NavigationStarting += (_, e) => { e.Cancel = loaded; loaded = true; }; // the app page only, never anything else
        core.NewWindowRequested += (_, e) => e.Handled = true;
        core.WebMessageReceived += OnMessage;
        using var page = typeof(MainForm).Assembly.GetManifestResourceStream("index.html")!;
        core.NavigateToString(new StreamReader(page).ReadToEnd());
    }

    async void OnMessage(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
    {
        int id = 0;
        object? data = null;
        string? error = null;
        try
        {
            var msg = JsonNode.Parse(e.WebMessageAsJson)!.AsObject();
            id = (int)msg["id"]!;
            var cmd = (string)msg["cmd"]!;
            var args = msg["args"] as JsonObject ?? [];
            data = await Task.Run(() => api.Handle(cmd, args, this));
        }
        catch (Exception ex) { error = ex.Message; }
        try { (sender as CoreWebView2)?.PostWebMessageAsJson(JsonSerializer.Serialize(new { id, ok = error == null, data, error }, Api.Json)); }
        catch (Exception) { } // the page went away (closed to the tray) before the answer
    }
}

/// <summary>Everything the page can ask for. Long jobs run on their own threads and the page polls their status.</summary>
sealed class Api
{
    public static readonly JsonSerializerOptions Json = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
    public static readonly bool IsAdmin = new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);

    public Watcher Watcher { get; } = new();
    readonly object gate = new();
    readonly Dictionary<int, DirNode> handles = [];
    Scanner? scan;
    CancellationTokenSource? scanStop;
    string scanPhase = "idle"; // idle | scanning | analyzing | done | stopped
    readonly Stopwatch scanClock = new();
    Cleanup? cleanup;
    List<Suggestion>? suggestions;
    CleanState clean = new();
    DupeState dupes = new();
    List<InstalledApp>? apps;
    List<Section>? sys;

    public object? Handle(string cmd, JsonObject a, MainForm form) => cmd switch
    {
        "init" => Init(),
        "scan" => StartScan(Str(a, "root")),
        "stopScan" => StopScan(),
        "scanStatus" => ScanStatus(),
        "children" => Children((int?)a["id"] ?? 0),
        "bigFiles" => BigFiles(),
        "types" => Types(),
        "suggestions" => Suggestions(),
        "clean" => StartClean([.. a["keys"]!.AsArray().Select(k => (string)k!)]),
        "cleanStatus" => clean,
        "run" => RunItem(Str(a, "key")),
        "recycle" => Recycle(Str(a, "path")),
        "reveal" => Reveal(Str(a, "path")),
        "emptyBin" => EmptyBin(),
        "dupes" => StartDupes(),
        "dupesStatus" => dupes,
        "apps" => Apps(),
        "uninstall" => Uninstall(Str(a, "key")),
        "sys" => sys ??= SysInfo.Collect(),
        "live" => Watcher.Live(),
        "ram" => Watcher.Ram(),
        "kill" => Watcher.Kill(Str(a, "name")),
        "optimize" => Optimizer.Run(auto: false) ?? throw new InvalidOperationException(T("فيه تحسين شغّال الحين، انتظر ثواني")),
        "optState" => OptState(),
        "saveSettings" => SaveSettings(a),
        "lang" => LangInfo(),
        "setLang" => SetLang(Str(a, "lang"), form),
        "open" => Open(Str(a, "what")),
        "admin" => Elevate(form),
        _ => throw new ArgumentException(T("أمر غير معروف: {0}", cmd)),
    };

    static string Str(JsonObject a, string key) => (string?)a[key] ?? throw new ArgumentException(T("ناقص: {0}", key));

    object Init()
    {
        using var cpu = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"HARDWARE\DESCRIPTION\System\CentralProcessor\0");
        return new
        {
            admin = IsAdmin,
            system = Cleanup.Sys + "\\",
            drives = DriveInfo.GetDrives().Where(d => d.IsReady && d.DriveType is DriveType.Fixed or DriveType.Removable)
                .Select(d => new { name = d.Name, label = d.VolumeLabel, fs = d.DriveFormat, total = d.TotalSize, free = d.TotalFreeSpace }).ToList(),
            machine = Environment.MachineName,
            cpu = (cpu?.GetValue("ProcessorNameString") as string)?.Trim(),
        };
    }

    object StartScan(string root)
    {
        lock (gate)
        {
            if (scanPhase is "scanning" or "analyzing") return ScanStatus();
            var s = scan = new Scanner(root);
            var stop = scanStop = new CancellationTokenSource();
            scanPhase = "scanning";
            cleanup = null;
            suggestions = null;
            dupes = new();
            lock (handles) handles.Clear();
            scanClock.Restart();
            new Thread(() =>
            {
                s.Run(stop.Token);
                if (stop.IsCancellationRequested) { scanPhase = "stopped"; return; }
                scanPhase = "analyzing";
                try { BuildSuggestions(); } catch { }
                s.Prune(1 << 20); // after the rules ran: they never look inside folders under 1 MB
                scanPhase = "done";
                scanClock.Stop();
                // the walk leaves a lot of short-lived garbage; hand it back so the app stays light next to the user's programs
                GC.Collect(2, GCCollectionMode.Aggressive, blocking: true, compacting: true);
            }) { IsBackground = true }.Start();
        }
        return ScanStatus();
    }

    object StopScan()
    {
        scanStop?.Cancel();
        return ScanStatus();
    }

    object ScanStatus()
    {
        var s = scan;
        return new
        {
            phase = scanPhase,
            root = s?.Root.Name,
            files = s?.FilesSeen ?? 0,
            dirs = s?.DirsSeen ?? 0,
            size = s?.Root.Size ?? 0,
            current = s?.Current,
            seconds = (int)scanClock.Elapsed.TotalSeconds,
            denied = s?.DeniedDirs.Count ?? 0,
            used = s == null ? 0 : Used(s.Root.Name),
        };
    }

    static long Used(string path)
    {
        var d = new DriveInfo(Path.GetPathRoot(path)!);
        return d.TotalSize - d.TotalFreeSpace;
    }

    void BuildSuggestions()
    {
        var s = scan!;
        apps ??= SysInfo.Apps();
        var c = new Cleanup(s);
        var list = c.Build(apps, Used(s.Root.Name));
        lock (gate) { cleanup = c; suggestions = list; }
    }

    int Handle(DirNode n)
    {
        lock (handles)
        {
            if (!handles.TryGetValue(n.Id, out var known) || known != n) { n.Id = handles.Count + 1; handles[n.Id] = n; }
            return n.Id;
        }
    }

    sealed record Entry(int Id, string Name, long Size, long Files, long Newest, bool Dir, bool Denied);

    object Children(int id)
    {
        var s = scan ?? throw new InvalidOperationException(T("سوّ فحص أول"));
        DirNode node;
        lock (handles) node = handles.GetValueOrDefault(id) ?? s.Root;
        var path = node.FullPath;
        if (node.Pruned) s.Refresh(path);
        var items = node.Children.Select(d => new Entry(Handle(d), d.Name, d.Size, d.Files, d.Newest, true, d.Denied)).ToList();
        try
        {
            foreach (var f in new DirectoryInfo(path).EnumerateFiles("*", new EnumerationOptions { AttributesToSkip = 0, IgnoreInaccessible = true }))
                items.Add(new Entry(0, f.Name, s.Alloc((uint)f.Attributes, f.Length, path, f.Name), 1, f.LastWriteTime.ToFileTime(), false, false));
        }
        catch { }
        items.Sort((x, y) => y.Size.CompareTo(x.Size));
        var crumbs = new List<object>();
        for (var n = node; n != null; n = n.Parent) crumbs.Insert(0, new { id = Handle(n), name = n.Parent == null ? n.Name.TrimEnd('\\') : n.Name });
        const int max = 400;
        return new
        {
            id = Handle(node), path, size = node.Size, files = node.Files, newest = node.Newest, denied = node.Denied, crumbs,
            items = items.Take(max),
            more = items.Count > max ? new { count = items.Count - max, size = items.Skip(max).Sum(i => i.Size) } : null,
        };
    }

    object BigFiles()
    {
        var s = scan ?? throw new InvalidOperationException(T("سوّ فحص أول"));
        lock (s.BigFiles)
            return s.BigFiles.Take(1500).Select(f => new { path = f.FullPath, name = f.Name, size = f.Size, modified = f.Modified, kind = Kinds.Of(Path.GetExtension(f.Name)) }).ToList();
    }

    object Types() => (scan?.Extensions ?? []).GroupBy(e => Kinds.Of(e.Key))
        .Select(g => new
        {
            kind = g.Key, size = g.Sum(e => e.Value.Size), count = g.Sum(e => e.Value.Count),
            exts = g.OrderByDescending(e => e.Value.Size).Take(8).Select(e => new { ext = e.Key, size = e.Value.Size, count = e.Value.Count }).ToList(),
        })
        .OrderByDescending(x => x.size).ToList();

    object? Suggestions()
    {
        Cleanup? c;
        List<Suggestion>? list;
        lock (gate) (c, list) = (cleanup, suggestions);
        if (c == null || list == null) return null;
        var d = new DriveInfo(Path.GetPathRoot(scan!.Root.Name)!);
        return new { groups = list, totals = c.Totals(d.TotalSize - d.TotalFreeSpace), total = d.TotalSize, free = d.TotalFreeSpace };
    }

    object StartClean(List<string> keys)
    {
        lock (gate)
        {
            if (clean.Running || cleanup == null) return clean;
            var st = clean = new CleanState { Running = true };
            var c = cleanup;
            new Thread(() =>
            {
                try { c.Clean(keys, st); BuildSuggestions(); }
                catch { }
                finally { st.Running = false; }
            }) { IsBackground = true }.Start();
            return st;
        }
    }

    object RunItem(string key)
    {
        var it = cleanup?.Get(key) ?? throw new ArgumentException(T("العنصر ما عاد موجود، حدّث الصفحة"));
        if (it.Mode == "cmd") Files.Elevated(it.Command!);
        else if (it.Mode is "uninstall" or "tool") Files.Shell(it.Command!);
        else throw new ArgumentException(T("هذا العنصر يتنظف بزر التنظيف"));
        return true;
    }

    object Recycle(string path)
    {
        var s = scan ?? throw new InvalidOperationException(T("سوّ فحص أول"));
        if (!Cleanup.IsUnder(path, s.Root.Name)) throw new UnauthorizedAccessException(T("المسار برا الفحص"));
        long size = File.Exists(path) ? new FileInfo(path).Length : s.Find(path)?.Size ?? 0;
        if (!Files.Recycle(path)) return false;
        s.Removed(path, size);
        return true;
    }

    static object Reveal(string path)
    {
        Files.Reveal(path);
        return true;
    }

    object EmptyBin()
    {
        Files.EmptyBin();
        if (scan != null && scanPhase == "done") BuildSuggestions();
        return true;
    }

    object StartDupes()
    {
        lock (gate)
        {
            if (dupes.Running || scan == null || scanPhase != "done") return dupes;
            var st = dupes = new DupeState { Running = true };
            var s = scan;
            var junk = (suggestions ?? []).Where(g => g.Group is "safe" or "dev").SelectMany(g => g.Items).Select(i => i.Path).Where(p => !p.StartsWith("::")).ToList();
            new Thread(() =>
            {
                try { Dupes.Find(s, st, junk); }
                catch { }
                finally
                {
                    st.Running = false;
                    GC.Collect(2, GCCollectionMode.Aggressive, blocking: true, compacting: true);
                }
            }) { IsBackground = true, Priority = ThreadPriority.BelowNormal }.Start();
            return st;
        }
    }

    object Apps()
    {
        apps ??= SysInfo.Apps();
        var s = scanPhase == "done" ? scan : null;
        return apps.Select(a =>
        {
            // a finished scan knows the real size of the install folder
            var node = !string.IsNullOrEmpty(a.Location) && !Files.IsProtected(a.Location) ? s?.Find(a.Location) : null;
            return new { a.Key, a.Name, a.Publisher, a.Version, a.Date, a.Location, size = node?.Size ?? a.Size, measured = node != null };
        }).OrderByDescending(x => x.size).ToList();
    }

    object Uninstall(string key)
    {
        var app = apps?.FirstOrDefault(a => a.Key == key) ?? throw new ArgumentException(T("البرنامج غير موجود"));
        Files.Shell(Cleanup.UninstallCommand(app)!);
        return true;
    }

    static object OptState() => new
    {
        settings = Settings.Current,
        startup = Startup.Enabled,
        admin = IsAdmin,
        running = Optimizer.Running,
        history = Optimizer.History.AsEnumerable().Reverse().Take(8).ToList(),
    };

    static object SaveSettings(JsonObject a)
    {
        var s = a["settings"]!.Deserialize<Settings>(Json)!.Clamped();
        s.Lang = Settings.Current.Lang; // the language has its own command
        Settings.Current = s;
        s.Save();
        if ((bool?)a["startup"] is bool on && on != Startup.Enabled) Startup.Enabled = on;
        return OptState();
    }

    static object LangInfo() => new
    {
        lang = I18n.Lang,
        rtl = Rtl,
        dict = I18n.Map,
        available = I18n.Available.Select(c => new { code = c, name = c == "ar" ? "العربية" : I18n.Read(c)?.GetValueOrDefault("@name") ?? c }).ToList(),
    };

    object SetLang(string lang, MainForm form)
    {
        if (!I18n.Available.Contains(lang)) throw new ArgumentException(T("اللغة غير موجودة"));
        I18n.Set(lang);
        Settings.Current.Lang = lang;
        Settings.Current.Save();
        sys = null; // rebuilt in the new language next time it is asked for
        if (scan != null && scanPhase == "done") BuildSuggestions();
        form.BeginInvoke(form.Retitle);
        return LangInfo();
    }

    static readonly HashSet<string> Openable = ["ms-settings:startupapps", "ms-settings:appsfeatures", "ms-settings:storagesense", "taskmgr", "cleanmgr.exe"];

    static object Open(string what)
    {
        if (!Openable.Contains(what)) throw new ArgumentException(T("غير مسموح"));
        Files.Shell(what);
        return true;
    }

    static object Elevate(MainForm form)
    {
        // --restarted: the new copy waits for this one to close instead of just waking it up
        try { Process.Start(new ProcessStartInfo(Environment.ProcessPath!, "--restarted") { UseShellExecute = true, Verb = "runas" }); }
        catch (System.ComponentModel.Win32Exception) { return false; } // the user said no
        form.BeginInvoke(form.Quit);
        return true;
    }
}
