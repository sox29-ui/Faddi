using System.Diagnostics;
using Faddi;

// Self-checks on throwaway folders: scanner totals, junk detection, and that cleaning never crosses a junction.
var tmp = Directory.CreateTempSubdirectory("faddi-check-").FullName;
var outside = Directory.CreateTempSubdirectory("faddi-outside-").FullName;
try
{
    Directory.CreateDirectory(Path.Join(tmp, "a", "b"));
    File.WriteAllBytes(Path.Join(tmp, "a", "one.bin"), new byte[5000]);
    File.WriteAllBytes(Path.Join(tmp, "a", "b", "big.MP4"), new byte[12_000_000]);
    File.WriteAllBytes(Path.Join(tmp, "tiny.txt"), new byte[10]);
    Junction(Path.Join(tmp, "loop"), tmp);

    var s = new Scanner(tmp);
    s.Run();
    long R(long n) => (n + s.ClusterSize - 1) / s.ClusterSize * s.ClusterSize;
    Check(Directory.Exists(Path.Join(tmp, "loop")), "junction was created");
    Check(s.Root.Size == R(5000) + R(12_000_000), $"total size {s.Root.Size}");
    Check(s.Root.Files == 3, "file count");
    Check(s.Root.Children is [{ Name: "a" }], "junction not followed");
    Check(s.BigFiles is [{ Name: "big.MP4" }], "big files");
    Check(s.Extensions["mp4"].Count == 1, "extension stats");
    Check(s.Find(Path.Join(tmp, "A", "b"))?.Size == R(12_000_000), "find by path");
    long before = s.Root.Size;
    Check(s.Prune(13_000_000) == 1 && s.Find(Path.Join(tmp, "a", "b")) == null && s.Root.Size == before, "prune drops the inside of small folders");
    s.Refresh(Path.Join(tmp, "a"));
    Check(s.Find(Path.Join(tmp, "a", "b"))?.Size == R(12_000_000) && s.Root.Size == before, "refresh brings the inside back");
    Directory.Delete(Path.Join(tmp, "loop"));

    // project junk next to real work
    Directory.CreateDirectory(Path.Join(tmp, "web", "node_modules", "left-pad"));
    File.WriteAllBytes(Path.Join(tmp, "web", "node_modules", "left-pad", "index.js"), new byte[2_000_000]);
    File.WriteAllText(Path.Join(tmp, "web", "app.js"), "real work");
    File.WriteAllText(Path.Join(tmp, "web", "package-lock.json"), "{}");
    // an app that ships its own node_modules (no lockfile): npm install could not bring it back
    Directory.CreateDirectory(Path.Join(tmp, "someapp", "resources", "node_modules", "native"));
    File.WriteAllBytes(Path.Join(tmp, "someapp", "resources", "node_modules", "native", "addon.node"), new byte[2_000_000]);
    Directory.CreateDirectory(Path.Join(tmp, "py", ".venv", "Lib"));
    File.WriteAllText(Path.Join(tmp, "py", ".venv", "pyvenv.cfg"), "home = x");
    File.WriteAllBytes(Path.Join(tmp, "py", ".venv", "Lib", "big.pyd"), new byte[2_000_000]);
    foreach (var d in new[] { "Assets", "ProjectSettings", "Library" }) Directory.CreateDirectory(Path.Join(tmp, "game", d));
    File.WriteAllBytes(Path.Join(tmp, "game", "Library", "cache.bin"), new byte[2_000_000]);
    File.WriteAllText(Path.Join(outside, "keep.txt"), "must survive");
    Junction(Path.Join(tmp, "web", "node_modules", "linked"), outside); // like pnpm / npm link

    s = new Scanner(tmp);
    s.Run();
    var cleanup = new Cleanup(s);
    var found = cleanup.Build([], 0);
    Check(found.Any(x => x.Id == "node_modules" && x.Items is [{ Path: var p }] && p.EndsWith(@"web\node_modules")), "only the project's node_modules found");
    Check(found.Any(x => x.Id == "venv" && x.Items.Count == 1), "venv found");
    Check(found.Any(x => x.Id == "unitylib" && x.Items.Count == 1), "unity Library found");
    var st = new CleanState();
    cleanup.Clean([.. found.Where(x => x.Group == "dev").SelectMany(x => x.Items).Select(i => i.Key)], st);
    Check(!Directory.Exists(Path.Join(tmp, "web", "node_modules")) && File.Exists(Path.Join(tmp, "web", "app.js")), "node_modules gone, project kept");
    Check(File.Exists(Path.Join(tmp, "someapp", "resources", "node_modules", "native", "addon.node")), "an app's bundled node_modules untouched");
    Check(File.Exists(Path.Join(outside, "keep.txt")), "junction target untouched");
    Check(st.Freed >= 6_000_000 && st.Failed.Count == 0, $"freed {st.Freed}, failed {st.Failed.Count}");
    Check(s.Root.Size < R(12_000_000) + R(5000) + R(2_000_000) + 4096 * 4, $"tree updated after clean ({s.Root.Size})");

    // duplicates: same bytes group up; same size with one byte off outside the sampled spots does not; a hard link is one file
    // (lives next to the exe because the duplicate search skips AppData, where Temp is)
    var dd = Path.Join(AppContext.BaseDirectory, "dupes-check");
    Directory.CreateDirectory(dd);
    try
    {
        var data = new byte[12_000_000];
        new Random(1).NextBytes(data);
        File.WriteAllBytes(Path.Join(dd, "a.bin"), data);
        File.WriteAllBytes(Path.Join(dd, "b.bin"), data);
        data[3_000_000] ^= 1;
        File.WriteAllBytes(Path.Join(dd, "c.bin"), data);
        Process.Start(new ProcessStartInfo("cmd", $"/c mklink /H \"{Path.Join(dd, "a-link.bin")}\" \"{Path.Join(dd, "a.bin")}\"") { CreateNoWindow = true, UseShellExecute = false })!.WaitForExit();
        var ds = new Scanner(dd);
        ds.Run();
        var dupes = new DupeState();
        Dupes.Find(ds, dupes, []);
        Check(ds.BigFiles.Count == 4, "hard link was created");
        Check(dupes.Groups is [{ Files.Count: 2 } g] && g.Files.Any(f => f.Path.EndsWith("b.bin")) && !g.Files.Any(f => f.Path.EndsWith("c.bin")),
            "duplicates: " + string.Join(" | ", dupes.Groups.Select(x => string.Join(",", x.Files.Select(f => Path.GetFileName(f.Path))))));
    }
    finally { Directory.Delete(dd, true); }

    Check(Files.IsProtected(@"C:\") && Files.IsProtected(Cleanup.W) && Files.IsProtected(Path.Join(Cleanup.W, "System32", "drivers"))
        && Files.IsProtected(Cleanup.U) && Files.IsProtected(Cleanup.L) && Files.IsProtected(Environment.GetFolderPath(Environment.SpecialFolder.Desktop)), "protected paths");
    Check(!Files.IsProtected(Path.Join(Cleanup.W, "Temp"), whole: false) && !Files.IsProtected(Path.Join(Cleanup.L, "npm-cache")), "cleanable paths");
    Check(Files.IsProtected(Path.Join(Cleanup.W, "Temp")) && Files.IsProtected(Cleanup.Tmp) && !Files.IsProtected(Cleanup.Tmp, whole: false), "Temp folders are emptied, never removed");
    Check(Files.IsProtected(@"C:\Users\Public") && Files.IsProtected(@"C:\Users\Public\Documents") && Files.IsProtected(@"C:\Users\Someone\AppData\Roaming")
        && Files.IsProtected(Path.Join(Cleanup.PD, "Microsoft", "Crypto")) && !Files.IsProtected(Path.Join(Cleanup.PD, "Microsoft", "Windows", "WER", "ReportArchive"), whole: false),
        "every profile and ProgramData\\Microsoft are walled");
    if (Directory.Exists(@"C:\PROGRA~1")) Check(Files.IsProtected(@"C:\PROGRA~1"), "short names are resolved");

    // Temp keeps anything changed in the last week (a file opened straight from a zip lives there)
    var aged = Directory.CreateTempSubdirectory("faddi-age-").FullName;
    File.WriteAllText(Path.Join(aged, "old.tmp"), "old");
    File.SetLastWriteTimeUtc(Path.Join(aged, "old.tmp"), DateTime.UtcNow.AddDays(-30));
    Directory.CreateDirectory(Path.Join(aged, "Temp1_photos.zip"));
    File.WriteAllText(Path.Join(aged, "Temp1_photos.zip", "edited.jpg"), "new");
    Files.DeleteTree(aged, false, DateTime.UtcNow.AddDays(-7));
    Check(!File.Exists(Path.Join(aged, "old.tmp")) && File.Exists(Path.Join(aged, "Temp1_photos.zip", "edited.jpg")), "only week-old temp files go");
    Directory.Delete(aged, true);
    Console.WriteLine("self-check OK");
}
finally
{
    foreach (var j in new[] { Path.Join(tmp, "loop"), Path.Join(tmp, "web", "node_modules", "linked") })
        if (Directory.Exists(j)) Directory.Delete(j); // .NET 8's recursive delete throws on junctions without admin
    Directory.Delete(tmp, true);
    Directory.Delete(outside, true);
}

if (args.Length == 0) return;

// Report mode: Faddi.Check C:\
var scan = new Scanner(args[0]);
var th = new Thread(() => scan.Run());
th.Start();
while (!th.Join(10_000)) Console.Error.WriteLine($"... {scan.FilesSeen:N0} files");
const double GB = 1 << 30;
var drive = new DriveInfo(args[0]);
long used = drive.TotalSize - drive.TotalFreeSpace;
Console.WriteLine($"used {used / GB:F1} GB | scanned {scan.Root.Size / GB:F1} GB | {scan.FilesSeen:N0} files | {scan.Elapsed.TotalSeconds:F0}s");
var sw = Stopwatch.StartNew();
var c = new Cleanup(scan);
var list = c.Build(SysInfo.Apps(), used);
Console.WriteLine($"suggestions built in {sw.ElapsedMilliseconds} ms");
var totals = c.Totals(used);
Console.WriteLine(string.Join(" | ", totals.Select(t => $"{t.Key} {t.Value / GB:F1}")));
foreach (var g in list.GroupBy(x => x.Group))
{
    Console.WriteLine($"== {g.Key}: {totals.GetValueOrDefault(g.Key) / GB:F1} GB");
    foreach (var x in g)
    {
        Console.WriteLine($"  {x.Size / GB,6:F1} GB  {x.Id} ({x.Items.Count})");
        foreach (var i in x.Items.Take(6)) Console.WriteLine($"      {i.Size / GB,6:F2}  {i.Mode,-9} {i.Path}{(i.Note != null ? "  [" + i.Note + "]" : "")}");
    }
}

static void Junction(string link, string target) =>
    Process.Start(new ProcessStartInfo("cmd", $"/c mklink /J \"{link}\" \"{target}\"") { CreateNoWindow = true, UseShellExecute = false })!.WaitForExit();

static void Check(bool ok, string what)
{
    if (!ok) throw new Exception("CHECK FAILED: " + what);
}
