using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using static Faddi.I18n;

namespace Faddi;

/// <summary>One thing that can be cleaned. Mode says how: contents | folder | recycle | bin | uninstall | cmd | tool | info.</summary>
public sealed record SugItem(string Key, string Path, long Size, long Newest, string Mode, bool Admin, string? Note = null)
{
    [JsonIgnore] public string? Command { get; init; }
    public int KeepDays { get; init; } // contents mode: files changed in the last N days stay
}

public sealed class Suggestion(string id, string group, string title, string desc)
{
    public string Id { get; } = id;
    public string Group { get; } = group;
    public string Title { get; } = title;
    public string Desc { get; } = desc;
    public List<SugItem> Items { get; } = [];
    public long Size => Items.Sum(i => i.Size);
}

public sealed class CleanState
{
    public bool Running { get; set; }
    public string Current { get; set; } = "";
    public int Done { get; set; }
    public int Total { get; set; }
    public long Freed { get; set; }      // gone for good
    public long Recycled { get; set; }   // in the recycle bin: frees space once the bin is emptied
    public List<string> Failed { get; } = [];
    public bool NeedAdmin { get; set; }
}

/// <summary>Turns a finished scan into cleanup suggestions and carries them out.</summary>
public sealed class Cleanup(Scanner scan)
{
    static string F(Environment.SpecialFolder f) => Environment.GetFolderPath(f);
    public static readonly string L = F(Environment.SpecialFolder.LocalApplicationData), R = F(Environment.SpecialFolder.ApplicationData),
        U = F(Environment.SpecialFolder.UserProfile), W = F(Environment.SpecialFolder.Windows),
        PD = F(Environment.SpecialFolder.CommonApplicationData), PF = F(Environment.SpecialFolder.ProgramFiles),
        PF86 = F(Environment.SpecialFolder.ProgramFilesX86), Sys = Path.GetPathRoot(W)!.TrimEnd('\\'),
        LL = Path.Join(U, "AppData", "LocalLow"), Tmp = Path.GetTempPath().TrimEnd('\\');

    readonly List<Suggestion> list = [];
    readonly Dictionary<string, SugItem> byKey = [];
    readonly HashSet<string> claimed = new(StringComparer.OrdinalIgnoreCase);

    public SugItem? Get(string key) => byKey.GetValueOrDefault(key);

    public List<Suggestion> Build(IReadOnlyList<InstalledApp> apps, long driveUsed)
    {
        list.Clear(); byKey.Clear(); claimed.Clear();
        claimed.Add(Path.Join(Sys, "$Recycle.Bin"));
        List<BigFile> big;
        lock (scan.BigFiles) big = [.. scan.BigFiles]; // the map's trash button may change the list meanwhile

        // ---------- safe: caches and leftovers that rebuild themselves ----------
        // a week's grace: files opened straight from a zip or a download in progress live in Temp too
        var temp = S("temp", "safe", T("الملفات المؤقتة"),
            T("ملفات تحطها البرامج مؤقتاً وتنساها. نحذف بس اللي له أكثر من أسبوع، واللي يستخدمه برنامج شغال نتخطاه."));
        foreach (var n in new[] { Tmp, $@"{W}\Temp" }.SelectMany(Match)) Item(temp, n.FullPath, n.Size, n.Newest, "contents", keepDays: 7);
        Paths(S("winupdate", "safe", T("ملفات تحديثات ويندوز"),
            T("نسخ التحديثات بعد ما تثبّتت، وكاش التوصيل اللي يشاركه ويندوز مع أجهزة ثانية. ويندوز ينزّلها من جديد لو احتاجها.")),
            "contents", $@"{W}\SoftwareDistribution\Download", $@"{W}\SoftwareDistribution\DeliveryOptimization",
            $@"{W}\ServiceProfiles\NetworkService\AppData\Local\Microsoft\Windows\DeliveryOptimization\Cache");
        var dumps = S("dumps", "safe", T("تقارير الأعطال (Crash dumps)"),
            T("ملفات يسويها ويندوز والبرامج لما يصير كراش. تفيد المطورين بس، وحذفها آمن."));
        Paths(dumps, "contents", $@"{L}\CrashDumps", $@"{W}\Minidump", $@"{W}\LiveKernelReports",
            $@"{PD}\Microsoft\Windows\WER\ReportArchive", $@"{PD}\Microsoft\Windows\WER\ReportQueue", $@"{L}\Microsoft\Windows\WER");
        if (big.FirstOrDefault(f => f.Name.Equals("MEMORY.DMP", StringComparison.OrdinalIgnoreCase) && f.Dir.FullPath.Equals(W, StringComparison.OrdinalIgnoreCase)) is { } md)
            Item(dumps, md.FullPath, md.Size, md.Modified, "folder");
        Paths(S("shaders", "safe", T("كاش كرت الشاشة (Shaders)"),
            T("يسرّع تحميل الألعاب ويرجع ينبني لحاله. أول تشغيل للعبة بعد الحذف بيكون أبطأ شوي.")),
            "contents", $@"{L}\D3DSCache", $@"{L}\NVIDIA\DXCache", $@"{L}\NVIDIA\GLCache", $@"{L}\NVIDIA\OptixCache",
            $@"{LL}\NVIDIA\PerDriverVersion", $@"{L}\AMD\DxCache", $@"{L}\AMD\DxcCache", $@"{L}\AMD\GLCache", $@"{L}\Intel\ShaderCache");
        Paths(S("drivers", "safe", T("ملفات تثبيت التعريفات"),
            T("بقايا تثبيت تعريف كرت الشاشة. ما لها داعي بعد ما يخلص التثبيت.")),
            "contents", $@"{Sys}\NVIDIA", $@"{Sys}\AMD", $@"{PD}\NVIDIA Corporation\Downloader");
        var chromium = new[] { $@"{L}\Google\Chrome\User Data", $@"{L}\Microsoft\Edge\User Data", $@"{L}\BraveSoftware\Brave-Browser\User Data", $@"{L}\Vivaldi\User Data" };
        Paths(S("browsers", "safe", T("كاش المتصفحات"),
            T("صور وصفحات محفوظة عشان المواقع تفتح أسرع. ما يمسح كلمات المرور ولا السجل ولا تسجيل دخولك. سكّر المتصفح قبل التنظيف عشان ينحذف كله.")),
            "contents", [.. chromium.SelectMany(b => new[] { "Cache", "Code Cache", "GPUCache", @"Service Worker\CacheStorage", @"Service Worker\ScriptCache" }.Select(k => $@"{b}\*\{k}")),
                .. chromium.SelectMany(b => new[] { $@"{b}\ShaderCache", $@"{b}\GrShaderCache" }),
                $@"{L}\Opera Software\*\Cache", $@"{L}\Mozilla\Firefox\Profiles\*\cache2"]); // not INetCache: Outlook keeps edited attachments in it
        Paths(S("appcache", "safe", T("كاش البرامج"),
            T("كاش Discord وVS Code وSpotify وTeams وSlack وغيرها. يرجع يتعبّى لحاله مع الاستخدام.")),
            "contents", [.. new[] { "discord", "Code", "Cursor", "Slack", @"Microsoft\Teams" }.SelectMany(a => new[] { "Cache", "Code Cache", "GPUCache", "CachedData", "CachedExtensionVSIXs", @"Service Worker\CacheStorage" }.Select(k => $@"{R}\{a}\{k}")),
                $@"{L}\Spotify\Data", $@"{L}\Packages\SpotifyAB.SpotifyMusic_*\LocalCache\Spotify\Data", $@"{L}\Steam\htmlcache",
                $@"{L}\EpicGamesLauncher\Saved\webcache*", $@"{L}\Microsoft\Terminal Server Client\Cache"]);
        var gameCache = S("gamecache", "safe", T("كاش الألعاب"),
            T("ملفات تنزّلها الألعاب مؤقتاً، مثل كاش سيرفرات FiveM وكاش Roblox وSteam. تنزل من جديد وقت ما تحتاجها."));
        Paths(gameCache, "contents", $@"{L}\FiveM\FiveM.app\data\cache", $@"{L}\FiveM\FiveM.app\data\server-cache",
            $@"{L}\FiveM\FiveM.app\data\server-cache-priv", $@"{L}\FiveM\FiveM.app\crashes", $@"{L}\FiveM\FiveM.app\logs",
            $@"{L}\Roblox\rbx-storage", $@"{L}\Roblox\logs", $@"{PF86}\Steam\steamapps\shadercache", $@"{PF86}\Steam\depotcache",
            $@"{PF86}\Steam\appcache\httpcache", $@"{PF86}\Steam\dumps", $@"{PF86}\Steam\logs");
        if (Files.BinSize() is > 0 and var bin)
            Item(S("bin", "safe", T("سلة المحذوفات"), T("الملفات اللي حذفتها قبل لسه تاخذ مساحة لين تفضّي السلة.")), "::bin", bin, 0, "bin");
        Paths(S("upgrade", "safe", T("بقايا ترقيات ويندوز"),
            T("ملفات من ترقية أو إعادة ضبط ويندوز سابقة. ما لها فايدة بعد ما تخلص الترقية.")),
            "folder", $@"{Sys}\$Windows.~WS", $@"{Sys}\$Windows.~BT", $@"{Sys}\$GetCurrent", $@"{Sys}\$WinREAgent", $@"{Sys}\$SysReset", $@"{Sys}\ESD");
        // usually update extractions, but a hash-named folder could be someone's own: recycle, never preselected
        var leftovers = S("leftovers", "review", T("مجلدات بأسماء عشوائية في جذر القرص"),
            T("غالباً بقايا تحديثات أو برامج تثبيت فكّت ملفاتها هنا ونستها (مثل ملفات KB.cab). شيّك عليها قبل الحذف؛ تروح لسلة المحذوفات."));
        if (IsDriveScan)
            foreach (var c in scan.Root.Children.Where(c => Regex.IsMatch(c.Name, "^[0-9a-f]{20,40}$")))
                Item(leftovers, c.FullPath, c.Size, c.Newest, "recycle");
        if (scan.Find($@"{Sys}\Windows.old") is { } old)
            Item(S("winold", "safe", T("نسخة ويندوز القديمة (Windows.old)"),
                T("ويندوز حفظها عشان ترجع للنسخة القديمة لو احتجت. تنحذف بأداة تنظيف القرص من ويندوز (اختار: Previous Windows installation).")),
                old.FullPath, old.Size, old.Newest, "tool", command: $"cleanmgr.exe /d {Sys[0]}");

        // ---------- dev: package caches and build output ----------
        Paths(S("npm", "dev", T("كاش npm وYarn وpnpm"),
            T("نسخ من الحزم اللي نزّلتها قبل. أي مشروع يحتاجها ينزّلها من جديد تلقائياً.")),
            "contents", $@"{L}\npm-cache", $@"{R}\npm-cache", $@"{L}\Yarn\Cache", $@"{L}\pnpm-cache", $@"{L}\pnpm\store",
            $@"{U}\.bun\install\cache", $@"{L}\node-gyp\Cache", $@"{L}\electron\Cache", $@"{L}\electron-builder\Cache");
        Paths(S("python", "dev", T("كاش بايثون (pip و uv و conda)"),
            T("نسخ من مكتبات بايثون المنزّلة. pip وuv ينزّلونها من جديد وقت الحاجة.")),
            "contents", $@"{L}\pip\cache", $@"{L}\uv\cache", $@"{L}\pypoetry\Cache", $@"{U}\anaconda3\pkgs", $@"{U}\miniconda3\pkgs", $@"{U}\.conda\pkgs");
        Paths(S("jvm", "dev", T("كاش Gradle وMaven"),
            T("مكتبات Java وAndroid المنزّلة. Gradle ينزّل اللي يحتاجه في البناء الجاي (أول بناء أبطأ).")),
            "contents", $@"{U}\.gradle\caches", $@"{U}\.gradle\wrapper\dists", $@"{U}\.gradle\daemon", $@"{U}\.m2\repository", $@"{U}\.android\cache");
        Paths(S("nuget", "dev", T("كاش NuGet (.NET)"),
            T("حزم .NET المنزّلة. dotnet restore يرجعها وقت الحاجة.")),
            "contents", $@"{U}\.nuget\packages", $@"{L}\NuGet\v3-cache", $@"{L}\NuGet\plugins-cache", $@"{L}\NuGet\Cache");
        Paths(S("devtools", "dev", T("كاش أدوات برمجة ثانية"),
            T("Go وRust وDart وComposer وPlaywright وJetBrains وكاش مثبّت Visual Studio. كلها تنزل من جديد وقت الحاجة.")),
            "contents", $@"{L}\go-build", $@"{U}\go\pkg\mod", $@"{U}\.cargo\registry", $@"{L}\Pub\Cache", $@"{R}\Pub\Cache",
            $@"{L}\Composer", $@"{L}\ms-playwright", $@"{U}\.cache\puppeteer", $@"{U}\.cache\selenium", $@"{L}\JetBrains\*\caches",
            $@"{PD}\Microsoft\VisualStudio\Packages", $@"{L}\UnrealEngine\Common\DerivedDataCache");
        Paths(S("unitycache", "dev", T("كاش Unity وحزم Asset Store"),
            T("حزم نزّلتها من Asset Store وكاش الحزم. تقدر تنزلها من جديد من داخل Unity.")),
            "contents", $@"{L}\Unity\cache", $@"{R}\Unity\Asset Store-5.x");
        var kinds = new Dictionary<string, Suggestion>
        {
            ["node"] = S("node_modules", "dev", T("مجلدات node_modules"),
                T("مكتبات مشاريع JavaScript. ترجع بأمر npm install داخل المشروع. ابدأ بالمشاريع القديمة.")),
            ["venv"] = S("venv", "dev", T("بيئات بايثون (venv)"),
                T("ترجع بأمر pip install -r requirements.txt. احذف بيئات المشاريع اللي ما تشتغل عليها.")),
            ["unity"] = S("unitylib", "dev", T("مجلدات Library في مشاريع Unity"),
                T("Unity يعيد بناءها لحاله لما تفتح المشروع (أول فتحة تطول شوي).")),
            ["unreal"] = S("unreal", "dev", T("كاش مشاريع Unreal"),
                T("مجلدات DerivedDataCache وIntermediate. Unreal يعيد بناءها لما تفتح المشروع.")),
            ["dotnet"] = S("dotnetout", "dev", T("مجلدات bin وobj (.NET)"), T("مخرجات البناء. ترجع لما تسوي Build.")),
            ["build"] = S("buildout", "dev", T("مجلدات البناء (Flutter وGradle وRust)"),
                T("مخرجات البناء مثل build وtarget و.dart_tool. ترجع لما تبني المشروع.")),
            ["web"] = S("webcache", "dev", T("كاش أدوات الويب"), T("مجلدات .next و.turbo و.angular وغيرها. ترجع لحالها مع أول تشغيل.")),
            ["vs"] = S("vscache", "dev", T("مجلدات .vs (Visual Studio)"), T("كاش IntelliSense والإعدادات المؤقتة. Visual Studio يعيد إنشاءها.")),
            ["fivem"] = gameCache,
        };
        // on a whole drive, user profiles are walked without their dot-folders (tool caches handled above)
        var users = IsDriveScan ? scan.Find(Path.Join(Sys, "Users")) : null;
        foreach (var top in IsDriveScan ? scan.Root.Children.Where(c => !SkipNames.Contains(c.Name) && c != users) : [scan.Root])
            Walk(top, kinds, false);
        foreach (var profile in users?.Children ?? [])
            Walk(profile, kinds, true);

        // ---------- games and big programs ----------
        var programs = S("programs", "games", T("الألعاب والبرامج الكبيرة"),
            T("الألعاب ما تغيّر ملفاتها وقت اللعب، فالتاريخ هنا تاريخ التثبيت أو آخر تحديث. لو ما تلعبها، إلغاء تثبيتها يفضّي مساحة ضخمة."));
        var steam = SteamIds();
        var launchers = new (string Path, string Note)[]
        {
            ($@"{PF86}\Steam\steamapps\common", T("الأفضل تحذفه من Steam")), ($@"{PF}\Epic Games", T("الأفضل تحذفه من Epic Games Launcher")),
            ($@"{PF}\Unity\Hub\Editor", T("الأفضل تحذفه من Unity Hub")), ($@"{Sys}\XboxGames", T("الأفضل تحذفه من تطبيق Xbox")),
            ($@"{Sys}\Riot Games", T("الأفضل تحذفه من Riot Client")), ($@"{PF}\EA Games", T("الأفضل تحذفه من تطبيق EA")),
            ($@"{PF86}\Ubisoft\Ubisoft Game Launcher\games", T("الأفضل تحذفه من Ubisoft Connect")), ($@"{PF}\Rockstar Games", T("الأفضل تحذفه من Rockstar Launcher")),
        };
        var candidates = new List<(DirNode Node, string? Note)>();
        foreach (var (path, note) in launchers) if (scan.Find(path) is { } n) candidates.AddRange(n.Children.Select(c => (c, (string?)note)));
        foreach (var p in new[] { PF, PF86 }) if (scan.Find(p) is { } n) candidates.AddRange(n.Children.Select(c => (c, (string?)null)));
        if (IsDriveScan) candidates.AddRange(scan.Root.Children.Where(c => !SkipNames.Contains(c.Name)).Select(c => (c, (string?)null)));
        foreach (var (n, note) in candidates.Where(c => c.Node.Size >= 3L << 30).OrderByDescending(c => c.Node.Size))
        {
            var path = n.FullPath;
            if (Containers.Contains(n.Name) || claimed.Contains(path) || Covered(path)) continue;
            var owners = MatchApps(apps, path);
            if (n.Parent == scan.Root && owners.Count == 0) continue; // a root folder is only a "program" when Windows knows its uninstaller
            if (owners.Count > 1) // e.g. C:\Program Files\Adobe: one uninstaller would not empty it, and deleting it would break the others
            {
                Item(programs, path, n.Size, n.Newest, "tool", T("فيه {0} برامج داخله؛ احذفها من إعدادات ويندوز", owners.Count), "ms-settings:appsfeatures");
                continue;
            }
            var cmd = steam.TryGetValue(n.Name, out var id) && n.Parent?.Name == "common" ? "steam://uninstall/" + id
                : owners.Count == 1 ? UninstallCommand(owners[0]) : null;
            Item(programs, path, n.Size, n.Newest, cmd != null ? "uninstall" : "folder", owners.FirstOrDefault()?.Name ?? note, cmd);
        }
        Paths(S("android", "games", T("محاكيات أندرويد وصور النظام"),
            T("الأجهزة الوهمية (AVD) وصور أندرويد. تقدر تسويها من جديد من Android Studio ← Device Manager.")),
            "folder", $@"{U}\.android\avd\*.avd", $@"{L}\Android\Sdk\system-images\*");
        Paths(S("ai", "games", T("موديلات الذكاء الاصطناعي"),
            T("موديلات نزّلتها (Hugging Face وOllama وLM Studio). تنزل من جديد لو احتجتها، بس حجمها كبير.")),
            "folder", $@"{U}\.cache\huggingface\hub\models--*", $@"{U}\.cache\huggingface\hub\datasets--*", $@"{U}\.ollama\models",
            $@"{U}\.lmstudio\models", $@"{U}\.cache\lm-studio\models", $@"{U}\.cache\torch\hub", $@"{U}\.cache\whisper");
        Paths(S("recordings", "games", T("تسجيلات الألعاب والشاشة"),
            T("مقاطع سجّلتها (Steam وXbox Game Bar). شيّك عليها قبل لا تحذفها؛ تروح لسلة المحذوفات.")),
            "recycle", $@"{PF86}\Steam\userdata\*\gamerecordings", $@"{U}\Videos\Captures", $@"{U}\Videos\Radeon ReLive");

        // ---------- review: big and old stuff only you can judge ----------
        var stale = S("stale", "review", T("مجلدات كبيرة ما انلمست من 6 شهور"),
            T("ما تعدّل فيها أي ملف من أكثر من 6 شهور، فغالباً ما تستخدمها. تروح لسلة المحذوفات."));
        long cutoff = DateTime.Now.AddMonths(-6).ToFileTime();
        var usersDir = Path.Join(Sys, "Users");
        void Old(DirNode n)
        {
            foreach (var c in n.Children)
            {
                if (c.Size < 2L << 30) continue;
                var p = c.FullPath;
                // app data (phone backups, WSL disks) and other people's profiles are not ours to call "unused"
                if (c.Name is ".git" or "AppData" || new[] { W, PD, PF, PF86 }.Any(r => IsUnder(p, r)) || claimed.Contains(p) || Covered(p)) continue;
                if (IsUnder(p, usersDir) && !IsUnder(p, U) && !IsUnder(U, p)) continue;
                if (c.Newest > 0 && c.Newest < cutoff && !Files.IsProtected(p)) Item(stale, p, c.Size, c.Newest, "recycle");
                else Old(c);
            }
        }
        Old(scan.Root);
        var downloads = $@"{U}\Downloads";
        var dl = S("downloads", "review", T("ملفات كبيرة في التنزيلات"), T("غالباً ملفات تثبيت أو أرشيفات خلص دورها. تروح لسلة المحذوفات."));
        var archives = S("archives", "review", T("أرشيفات وملفات تثبيت وأقراص وهمية"), T("ملفات zip وrar وiso وmsi وأقراص أجهزة وهمية كبيرة برا مجلدات البرامج. تروح لسلة المحذوفات."));
        var videos = S("videos", "review", T("فيديوهات كبيرة"), T("فيديوهات حجمها كبير. شيّك عليها قبل الحذف؛ تروح لسلة المحذوفات."));
        string[] appData = [W, PD, PF, PF86, L, R, LL];
        foreach (var f in big)
        {
            var p = f.FullPath;
            if (Covered(p)) continue;
            var cat = Kinds.Of(Path.GetExtension(f.Name));
            if (IsUnder(p, downloads)) { if (f.Size >= 50L << 20) Item(dl, p, f.Size, f.Modified, "recycle"); }
            else if (appData.Any(r => IsUnder(p, r))) continue;
            // a big .exe outside Downloads is usually a tool someone runs, not an installer to throw away
            else if ((cat is "archive" or "disk" || cat == "installer" && !f.Name.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)) && f.Size >= 200L << 20) Item(archives, p, f.Size, f.Modified, "recycle");
            else if (cat == "video" && f.Size >= 300L << 20) Item(videos, p, f.Size, f.Modified, "recycle");
        }

        // ---------- system files: Windows' own tools do these ----------
        BigFile? RootFile(string name) => big.FirstOrDefault(f => f.Dir == scan.Root && f.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        if (IsDriveScan && RootFile("hiberfil.sys") is { } hib)
            Item(S("hiberfil", "system", T("ملف السبات (hiberfil.sys)"),
                T("ويندوز يحفظ فيه الرام لما تسوي سبات أو إقلاع سريع. لو قفلته توفّر حجمه كامل، بس يوقف السبات والإقلاع السريع. ترجّعه بأمر powercfg /h on.")),
                hib.FullPath, hib.Size, 0, "cmd", command: "powercfg /h off");
        if (IsDriveScan && RootFile("pagefile.sys") is { } page)
            Item(S("pagefile", "system", T("الذاكرة الافتراضية (pagefile.sys)"),
                T("ويندوز يستخدمه لما تمتلئ الرام، ويكبر كل ما ضغطت الرام. لا تحذفه؛ لو خففت استهلاك الرام يصغر لحاله.")),
                page.FullPath, page.Size, 0, "info");
        if (scan.Find($@"{W}\WinSxS") is { } sxs)
            Item(S("winsxs", "system", T("مكونات ويندوز (WinSxS)"),
                T("نسخ قديمة من ملفات النظام بعد التحديثات (الرقم أكبر من الحقيقي لأن ملفات كثيرة مشتركة). لا تلمسه يدوياً؛ نظّفه بأداة ويندوز الرسمية DISM، وتاخذ كم دقيقة.")),
                sxs.FullPath, sxs.Size, sxs.Newest, "cmd", command: "Dism.exe /Online /Cleanup-Image /StartComponentCleanup");
        Item(S("restore", "system", T("نقاط الاستعادة (System Restore)"),
            T("نسخ احتياطية للنظام. من «إعدادات حماية النظام» ← Configure تقدر تحدد مساحتها أو تحذف القديمة.")),
            "::restore", ShadowBytes(), 0, "tool", command: "SystemPropertiesProtection.exe");
        Item(S("cleanmgr", "system", T("أداة تنظيف القرص من ويندوز"),
            T("أداة ويندوز الرسمية. اضغط «Clean up system files» عشان تشوف تحديثات ويندوز القديمة وغيرها.")),
            "::cleanmgr", 0, 0, "tool", command: $"cleanmgr.exe /d {Sys[0]}");
        if (IsDriveScan && driveUsed - scan.Root.Size is > 1L << 30 and var hidden)
            Item(S("hidden", "system", T("مساحة ما قدرنا نقراها"),
                T("مجلدات محمية مثل نقاط الاستعادة وملفات النظام وتطبيقات المتجر. شغّل البرنامج كمسؤول عشان نقرا أكثر.")),
                "::hidden", hidden, 0, "info");

        list.RemoveAll(s => s.Items.Count == 0);
        foreach (var s in list) s.Items.Sort((a, b) => b.Size.CompareTo(a.Size));
        return list;
    }

    /// <summary>
    /// Bytes per group with every byte counted once: an item inside one already counted is skipped, and one that
    /// contains counted items only adds the rest. "system" is Windows plus the root files (pagefile...) plus what
    /// could not be read; "rest" is whatever is left of the used space.
    /// </summary>
    public Dictionary<string, long> Totals(long used)
    {
        var counted = new List<SugItem>();
        var res = new Dictionary<string, long>();
        foreach (var g in new[] { "safe", "dev", "games", "review" })
        {
            long sum = 0;
            foreach (var it in list.Where(s => s.Group == g).SelectMany(s => s.Items).Where(i => i.Mode is not ("info" or "tool")).OrderBy(i => i.Path.Length).ToList())
            {
                if (counted.Any(c => IsUnder(it.Path, c.Path))) continue;
                sum += it.Size - counted.Where(c => IsUnder(c.Path, it.Path)).Sum(c => c.Size);
                counted.Add(it);
            }
            res[g] = Math.Max(0, sum);
        }
        if (IsDriveScan)
            res["system"] = Math.Max(0, (scan.Find(W)?.Size ?? 0) + scan.Root.OwnSize + Math.Max(0, used - scan.Root.Size)
                - counted.Where(c => IsUnder(c.Path, W)).Sum(c => c.Size));
        res["rest"] = Math.Max(0, used - res.Values.Sum());
        return res;
    }

    public void Clean(IReadOnlyList<string> keys, CleanState st)
    {
        st.Total = keys.Count;
        // empty the bin before anything is moved into it, or one run would destroy what it just "recycled"
        foreach (var key in keys.OrderBy(k => byKey.GetValueOrDefault(k)?.Mode == "bin" ? 0 : 1))
        {
            if (byKey.GetValueOrDefault(key) is not { } it) { st.Done++; continue; }
            st.Current = it.Path;
            try
            {
                switch (it.Mode)
                {
                    case "contents":
                        long n = Files.DeleteTree(it.Path, false, it.KeepDays > 0 ? DateTime.UtcNow.AddDays(-it.KeepDays) : null);
                        st.Freed += n;
                        // with an age limit, nothing old enough is a fine outcome; without one, nothing deleted means it failed
                        if (n == 0 && (it.KeepDays == 0 || it.Admin && !Api.IsAdmin)) Fail(it, st);
                        scan.Refresh(it.Path);
                        break;
                    case "folder":
                        st.Freed += Files.DeleteTree(it.Path, true);
                        if (Path.Exists(it.Path)) { Fail(it, st); scan.Refresh(it.Path); break; }
                        scan.Removed(it.Path, it.Size);
                        // an emulator's .ini only goes once its .avd folder is fully gone
                        if (it.Path.EndsWith(".avd", StringComparison.OrdinalIgnoreCase)) File.Delete(it.Path[..^4] + ".ini");
                        break;
                    case "recycle":
                        if (Files.Recycle(it.Path)) { st.Recycled += it.Size; scan.Removed(it.Path, it.Size); }
                        else Fail(it, st);
                        break;
                    case "bin":
                        st.Freed += Files.BinSize();
                        Files.EmptyBin();
                        break;
                }
            }
            catch (Exception ex) { Fail(it, st); st.NeedAdmin |= ex is UnauthorizedAccessException; }
            st.Done++;
        }
    }

    static void Fail(SugItem it, CleanState st)
    {
        st.Failed.Add(it.Path);
        st.NeedAdmin |= it.Admin && !Api.IsAdmin;
    }

    // ---------- helpers ----------

    bool IsDriveScan => scan.Root.Name.Length <= 3;

    Suggestion S(string id, string group, string title, string desc)
    {
        var s = new Suggestion(id, group, title, desc);
        list.Add(s);
        return s;
    }

    void Item(Suggestion s, string path, long size, long newest, string mode, string? note = null, string? command = null, int keepDays = 0)
    {
        if (size <= 0 && mode is not ("tool" or "info" or "cmd")) return;
        if (!path.StartsWith("::") && (Covered(path) || !claimed.Add(path))) return;
        bool admin = mode == "cmd" || new[] { W, PD, PF, PF86 }.Any(r => IsUnder(path, r)) || path.StartsWith(Sys + @"\$");
        var it = new SugItem(s.Id + "|" + path, path, size, newest, mode, admin, note) { Command = command, KeepDays = keepDays };
        s.Items.Add(it);
        byKey[it.Key] = it;
    }

    void Paths(Suggestion s, string mode, params string[] patterns)
    {
        foreach (var p in patterns) foreach (var n in Match(p)) Item(s, n.FullPath, n.Size, n.Newest, mode);
    }

    bool Covered(string path) => claimed.Any(c => c.Length < path.Length && IsUnder(path, c));

    /// <summary>Resolves a path whose segments may be "*", "prefix*" or "*suffix" against the scanned tree.</summary>
    IEnumerable<DirNode> Match(string pattern)
    {
        if (!pattern.StartsWith(scan.Root.Name, StringComparison.OrdinalIgnoreCase)) return [];
        IEnumerable<DirNode> cur = [scan.Root];
        foreach (var part in pattern[scan.Root.Name.Length..].Split('\\', StringSplitOptions.RemoveEmptyEntries))
            cur = cur.SelectMany(n => n.Children.Where(c => Glob(c.Name, part))).ToList();
        return cur;
    }

    static bool Glob(string name, string pat) =>
        pat == "*" || (pat.StartsWith('*') ? name.EndsWith(pat[1..], StringComparison.OrdinalIgnoreCase)
            : pat.EndsWith('*') ? name.StartsWith(pat[..^1], StringComparison.OrdinalIgnoreCase)
            : name.Equals(pat, StringComparison.OrdinalIgnoreCase));

    public static bool IsUnder(string path, string dir) =>
        path.Length > dir.Length
            ? path.StartsWith(dir, StringComparison.OrdinalIgnoreCase) && (dir.EndsWith('\\') || path[dir.Length] == '\\')
            : path.Equals(dir, StringComparison.OrdinalIgnoreCase);

    // Never look for project junk inside Windows, installed programs or tool installs.
    static readonly HashSet<string> SkipNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "Windows", "Program Files", "Program Files (x86)", "ProgramData", "$Recycle.Bin", "System Volume Information", "Recovery",
        "Users", "AppData", "nvm4w", "msys64", "MinGW", "xampp", "Intel", "AMD", "NVIDIA", "PerfLogs", "inetpub", "WindowsApps",
        "$Windows.~WS", "$Windows.~BT", "$SysReset", "$WinREAgent", "Windows.old",
    };

    static readonly HashSet<string> Containers = new(StringComparer.OrdinalIgnoreCase)
    {
        "Steam", "Epic Games", "Unity", "XboxGames", "Riot Games", "EA Games", "Ubisoft", "Rockstar Games", "WindowsApps", "Common Files",
        "Microsoft", "Windows Defender", "Windows Kits", "dotnet", "Windows NT", "Reference Assemblies", "Microsoft SDKs", "ModifiableWindowsApps",
        "Windows Defender Advanced Threat Protection", "WindowsPowerShell", "Internet Explorer", "Windows Mail", "Windows Media Player", "Windows Photo Viewer",
    };

    void Walk(DirNode n, Dictionary<string, Suggestion> kinds, bool profile)
    {
        foreach (var c in n.Children)
        {
            if (c.Size < 1 << 20 || SkipNames.Contains(c.Name) || (profile && c.Name.StartsWith('.'))) continue;
            var kind = Kind(n, c);
            if (kind != null) Item(kinds[kind], c.FullPath, c.Size, c.Newest, kind == "fivem" ? "contents" : "folder");
            else if (!c.Name.Equals("node_modules", StringComparison.OrdinalIgnoreCase)) Walk(c, kinds, false);
        }
    }

    static string? Kind(DirNode parent, DirNode c) => c.Name.ToLowerInvariant() switch
    {
        // a lockfile next to it means a project npm install can restore; an app's bundled node_modules has none
        "node_modules" when AnyFile(parent, "package-lock.json", "yarn.lock", "pnpm-lock.yaml", "bun.lockb", "bun.lock", "npm-shrinkwrap.json") => "node",
        ".venv" or "venv" or "env" or ".env" when File.Exists(Path.Join(c.FullPath, "pyvenv.cfg")) => "venv",
        "library" when Has(parent, "Assets") && Has(parent, "ProjectSettings") => "unity",
        "deriveddatacache" => "unreal",
        "intermediate" when Has(parent, "Content") && Has(parent, "Config") => "unreal",
        "obj" or "bin" when AnyFile(parent, "*.csproj", "*.fsproj", "*.vbproj") => "dotnet",
        "target" when AnyFile(parent, "Cargo.toml", "pom.xml") => "build",
        "build" when AnyFile(parent, "pubspec.yaml", "build.gradle", "build.gradle.kts") => "build",
        ".dart_tool" or ".gradle" or "cmake-build-debug" or "cmake-build-release" => "build",
        ".next" or ".nuxt" or ".turbo" or ".parcel-cache" or ".angular" or ".svelte-kit" or ".expo" => "web",
        ".vs" => "vs",
        "server-cache-priv" or "server-cache" when parent.Name.Equals("data", StringComparison.OrdinalIgnoreCase) => "fivem",
        _ => null,
    };

    static bool Has(DirNode n, string child) => Array.Exists(n.Children, c => c.Name.Equals(child, StringComparison.OrdinalIgnoreCase));

    static bool AnyFile(DirNode n, params string[] patterns)
    {
        try { return patterns.Any(p => Directory.EnumerateFiles(n.FullPath, p).Any()); }
        catch { return false; }
    }

    // Apps living in the folder itself or at most two levels down (VS sits in "...\Microsoft Visual Studio\2022\Community");
    // deeper matches would let a tool installed somewhere under C:\Users claim the whole folder.
    static List<InstalledApp> MatchApps(IReadOnlyList<InstalledApp> apps, string path)
    {
        static int Depth(string p) => p.TrimEnd('\\').Count(ch => ch == '\\');
        var inside = apps.Where(a => a.Location is { Length: > 3 } l && IsUnder(l.TrimEnd('\\'), path) && Depth(l) - Depth(path) <= 2).DistinctBy(a => a.Uninstall).ToList();
        return inside.Count > 0 ? inside
            : [.. apps.Where(a => a.Uninstall?.Contains(path + "\\", StringComparison.OrdinalIgnoreCase) == true).DistinctBy(a => a.Uninstall)];
    }

    // Anything can write the per-user uninstall list; from an elevated app, send those to Windows Settings instead of running them as admin.
    public static string? UninstallCommand(InstalledApp a) =>
        Api.IsAdmin && a.Key.StartsWith("HKEY_CURRENT_USER", StringComparison.OrdinalIgnoreCase) ? "ms-settings:appsfeatures" : a.Uninstall;

    static Dictionary<string, string> SteamIds()
    {
        var ids = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        try
        {
            foreach (var acf in Directory.EnumerateFiles(Path.Join(PF86, "Steam", "steamapps"), "appmanifest_*.acf"))
            {
                var t = File.ReadAllText(acf);
                var id = Regex.Match(t, "\"appid\"\\s+\"(\\d+)\"").Groups[1].Value;
                var dir = Regex.Match(t, "\"installdir\"\\s+\"([^\"]+)\"").Groups[1].Value;
                if (id != "" && dir != "") ids[dir] = id;
            }
        }
        catch { }
        return ids;
    }

    // Restore points live in System Volume Information; only an admin may ask their size.
    static long ShadowBytes()
    {
        try
        {
            using var q = new System.Management.ManagementObjectSearcher("SELECT UsedSpace FROM Win32_ShadowStorage");
            return q.Get().Cast<System.Management.ManagementBaseObject>().Sum(o => Convert.ToInt64(o["UsedSpace"]));
        }
        catch { return 0; }
    }
}

/// <summary>Deleting, recycling and launching, with a guard against touching anything Windows needs.</summary>
public static class Files
{
    static readonly string W = Cleanup.W, Users = Path.Join(Cleanup.Sys, "Users");
    // never deleted, never emptied
    static readonly string[] Exact =
    [
        W, Cleanup.PF, Cleanup.PF86, Cleanup.PD, Users, Path.Join(Cleanup.PF, "WindowsApps"), Path.Join(Cleanup.PF, "Common Files"),
        Path.Join(Cleanup.PF86, "Common Files"), Path.Join(Cleanup.PD, "Package Cache"),
    ];
    // in every profile, not only the current one: the profile itself, these folders, and AppData\Local|Roaming|LocalLow
    static readonly HashSet<string> ProfileDirs = new(StringComparer.OrdinalIgnoreCase)
    {
        "AppData", "Desktop", "Documents", "Downloads", "Pictures", "Videos", "Music", "OneDrive", "Favorites", "Contacts", "Links",
        "Saved Games", "Searches", "3D Objects",
    };
    // Inside Windows and ProgramData\Microsoft only these junk folders may be emptied; the folders themselves stay.
    static readonly string[] Allowed =
    [
        Path.Join(W, "Temp"), Path.Join(W, "SoftwareDistribution", "Download"), Path.Join(W, "SoftwareDistribution", "DeliveryOptimization"),
        Path.Join(W, "ServiceProfiles", "NetworkService", "AppData", "Local", "Microsoft", "Windows", "DeliveryOptimization"),
        Path.Join(W, "Minidump"), Path.Join(W, "LiveKernelReports"),
        Path.Join(Cleanup.PD, "Microsoft", "Windows", "WER"), Path.Join(Cleanup.PD, "Microsoft", "VisualStudio", "Packages"),
    ];

    /// <param name="whole">true when the path itself goes away (delete, recycle); false when only what is inside is emptied.</param>
    public static bool IsProtected(string path, bool whole = true)
    {
        var p = LongName(Path.GetFullPath(path)).TrimEnd('\\');
        if (p.Length <= 2 || Exact.Any(e => e.Equals(p, StringComparison.OrdinalIgnoreCase))) return true;
        if (whole && p.Equals(Cleanup.Tmp, StringComparison.OrdinalIgnoreCase)) return true; // empty Temp, never remove it
        if (Cleanup.IsUnder(p, Users))
        {
            var parts = p[(Users.Length + 1)..].Split('\\');
            if (parts.Length == 1 || parts.Length == 2 && ProfileDirs.Contains(parts[1])
                || parts.Length == 3 && parts[1].Equals("AppData", StringComparison.OrdinalIgnoreCase)) return true;
        }
        if (p.Equals(Path.Join(W, "MEMORY.DMP"), StringComparison.OrdinalIgnoreCase)) return false;
        bool walled = Cleanup.IsUnder(p, W) || Cleanup.IsUnder(p, Path.Join(Cleanup.PD, "Microsoft"));
        return walled && !Allowed.Any(a => whole ? p.Length > a.Length && Cleanup.IsUnder(p, a) : Cleanup.IsUnder(p, a));
    }

    // "C:\PROGRA~1" and friends: compare real names only
    static string LongName(string p)
    {
        var buf = new char[1024];
        uint n = GetLongPathNameW(p, buf, (uint)buf.Length);
        return n > 0 && n < buf.Length ? new string(buf, 0, (int)n) : p;
    }

    /// <summary>
    /// Deletes for good. Links are removed as links and never followed; files in use are skipped.
    /// With <paramref name="olderThanUtc"/>, files changed after it stay.
    /// </summary>
    public static long DeleteTree(string path, bool self, DateTime? olderThanUtc = null)
    {
        if (IsProtected(path, whole: self)) throw new UnauthorizedAccessException(T("مسار محمي: {0}", path));
        if (File.Exists(path))
        {
            var f = new FileInfo(path);
            long n = f.Length;
            f.Attributes = FileAttributes.Normal;
            f.Delete();
            return n;
        }
        return Directory.Exists(path) ? Tree(new DirectoryInfo(path), self, olderThanUtc) : 0;

        static long Tree(DirectoryInfo dir, bool self, DateTime? cutoff)
        {
            long freed = 0;
            List<FileSystemInfo> entries;
            try { entries = [.. dir.EnumerateFileSystemInfos("*", new EnumerationOptions { AttributesToSkip = 0, IgnoreInaccessible = true })]; }
            catch { return 0; }
            foreach (var e in entries)
            {
                try
                {
                    bool recent = cutoff != null && e.LastWriteTimeUtc > cutoff;
                    if (e is DirectoryInfo d)
                    {
                        if (d.Attributes.HasFlag(FileAttributes.ReparsePoint)) { if (!recent) d.Delete(); }
                        else freed += Tree(d, true, cutoff); // a folder only goes once nothing recent is left inside
                        continue;
                    }
                    if (recent) continue;
                    long n = ((FileInfo)e).Length;
                    if (e.Attributes.HasFlag(FileAttributes.ReadOnly)) e.Attributes = FileAttributes.Normal;
                    e.Delete();
                    freed += n;
                }
                catch { /* in use or locked: leave it */ }
            }
            if (self)
                try
                {
                    if (dir.Attributes.HasFlag(FileAttributes.ReadOnly)) dir.Attributes &= ~FileAttributes.ReadOnly;
                    dir.Delete();
                }
                catch { }
            return freed;
        }
    }

    /// <summary>Moves to the recycle bin. Windows warns before anything too big for the bin is deleted for good.</summary>
    public static bool Recycle(string path)
    {
        if (IsProtected(path)) throw new UnauthorizedAccessException(T("مسار محمي: {0}", path));
        bool ok = false;
        var t = new Thread(() =>
        {
            var op = new SHFILEOPSTRUCT { wFunc = 3 /*FO_DELETE*/, pFrom = path + "\0", fFlags = 0x40 | 0x10 | 0x4000 /*ALLOWUNDO|NOCONFIRMATION|WANTNUKEWARNING*/ };
            ok = SHFileOperationW(ref op) == 0 && op.fAnyOperationsAborted == 0;
        });
        t.SetApartmentState(ApartmentState.STA);
        t.Start();
        t.Join();
        return ok && !Path.Exists(path);
    }

    public static long BinSize()
    {
        var info = new SHQUERYRBINFO { cbSize = Marshal.SizeOf<SHQUERYRBINFO>() };
        return SHQueryRecycleBinW(null, ref info) == 0 ? info.i64Size : 0;
    }

    public static void EmptyBin() => SHEmptyRecycleBinW(0, null, 7 /*no confirm, progress or sound*/);

    /// <summary>Starts an uninstaller, a ms-settings: page or a Windows tool the way Explorer would.</summary>
    public static void Shell(string cmd)
    {
        if (cmd.Contains("://") || cmd.StartsWith("ms-settings:")) { Process.Start(new ProcessStartInfo(cmd) { UseShellExecute = true }); return; }
        string file, args;
        if (cmd.StartsWith('"')) { int q = cmd.IndexOf('"', 1); file = cmd[1..q]; args = cmd[(q + 1)..]; }
        else
        {
            int x = cmd.IndexOf(".exe", StringComparison.OrdinalIgnoreCase);
            (file, args) = x > 0 ? (cmd[..(x + 4)], cmd[(x + 4)..]) : (cmd, "");
        }
        Process.Start(new ProcessStartInfo(file, args.Trim()) { UseShellExecute = true });
    }

    /// <summary>Runs a command as admin in a console that stays open so the result is visible.</summary>
    public static void Elevated(string cmd) =>
        Process.Start(new ProcessStartInfo("cmd.exe", "/k " + cmd) { UseShellExecute = true, Verb = "runas" });

    public static void Reveal(string path)
    {
        if (path.StartsWith(@"\\")) return; // local disks only: a network path would make Explorer connect out
        if (File.Exists(path)) Process.Start("explorer.exe", $"/select,\"{path}\"");
        else if (Directory.Exists(path)) Process.Start("explorer.exe", $"\"{path}\"");
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    struct SHFILEOPSTRUCT
    {
        public nint hwnd;
        public uint wFunc;
        public string pFrom;
        public string? pTo;
        public ushort fFlags;
        public int fAnyOperationsAborted;
        public nint hNameMappings;
        public string? lpszProgressTitle;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct SHQUERYRBINFO
    {
        public int cbSize;
        public long i64Size;
        public long i64NumItems;
    }

    [DllImport("shell32", CharSet = CharSet.Unicode)]
    static extern int SHFileOperationW(ref SHFILEOPSTRUCT op);

    [DllImport("shell32", CharSet = CharSet.Unicode)]
    static extern int SHQueryRecycleBinW(string? root, ref SHQUERYRBINFO info);

    [DllImport("shell32", CharSet = CharSet.Unicode)]
    static extern int SHEmptyRecycleBinW(nint hwnd, string? root, uint flags);

    [DllImport("kernel32", CharSet = CharSet.Unicode)]
    static extern uint GetLongPathNameW(string shortPath, [Out] char[] longPath, uint size);
}

/// <summary>File-type families used by the "largest files" and "file types" views.</summary>
public static class Kinds
{
    static readonly Dictionary<string, string> Map = new(StringComparer.OrdinalIgnoreCase);

    static Kinds()
    {
        void Add(string kind, string exts) { foreach (var e in exts.Split(' ')) Map[e] = kind; }
        Add("video", "mp4 mkv mov avi wmv flv webm m4v mpg mpeg ts m4s 3gp");
        Add("image", "jpg jpeg png gif bmp webp tif tiff psd raw heic svg ico tga dds exr hdr");
        Add("audio", "mp3 wav flac aac ogg m4a wma opus");
        Add("archive", "zip rar 7z tar gz tgz bz2 xz zst cab");
        Add("installer", "exe msi msix appx appxbundle msixbundle apk");
        Add("disk", "iso img vhd vhdx vmdk qcow2 vdi wim esd dmg");
        Add("game", "pak dat forge ucas utoc bundle assets ress resource arc vpk big wad rpf bsa ba2 uasset umap");
        Add("code", "js mjs cjs ts tsx jsx py pyc cs cpp c h hpp java kt go rs rb php json xml yml yaml lock map class html css scss md");
        Add("binary", "dll sys lib a so node jar obj o pdb wasm bin");
        Add("doc", "pdf doc docx xls xlsx ppt pptx txt csv rtf odt epub");
        Add("data", "db db3 sqlite sqlite3 mdf ldf ibd sql log ldb");
        Add("model", "safetensors ckpt gguf onnx pt pth h5 tflite");
    }

    public static string Of(string ext) => Map.GetValueOrDefault(ext.TrimStart('.'), ext == "" ? "none" : "other");
}
