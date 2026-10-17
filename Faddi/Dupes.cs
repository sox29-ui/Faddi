using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace Faddi;

public sealed record DupeFile(string Path, long Modified);
public sealed record DupeGroup(long Size, List<DupeFile> Files)
{
    public long Wasted => Size * (Files.Count - 1);
}

public sealed class DupeState
{
    long done;
    public bool Running { get; set; }
    public string Phase { get; set; } = "sample"; // sample: quick sort-out, full: comparing whole files
    public long Done => Volatile.Read(ref done);  // bytes compared in the full pass
    public long Total { get; set; }
    public List<DupeGroup> Groups { get; set; } = [];
    public void Add(long bytes) => Interlocked.Add(ref done, bytes);
}

/// <summary>Finds big files with identical content: same size, then a sampled hash, then a full SHA-256.</summary>
public static class Dupes
{
    /// <param name="junk">Folders the cleanup page already offers whole (caches, venvs, node_modules): a copy inside one is not worth listing.</param>
    public static void Find(Scanner scan, DupeState st, IEnumerable<string> junk)
    {
        // app data is mostly caches the cleanup page already handles; Windows and programs keep their own copies on purpose
        string[] skip = [Cleanup.W, Cleanup.PF, Cleanup.PF86, Cleanup.PD, Cleanup.L, Cleanup.R, Cleanup.LL, Path.Join(Cleanup.Sys, "$Recycle.Bin"), .. junk];
        List<IGrouping<long, (BigFile File, string Path)>> sameSize;
        lock (scan.BigFiles)
            sameSize = [.. scan.BigFiles.Select(f => (File: f, Path: f.FullPath))
                .Where(x => !skip.Any(s => Cleanup.IsUnder(x.Path, s)) && !x.Path.Contains(@"\node_modules\") && !x.Path.Contains(@"\.git\"))
                .GroupBy(x => x.File.Size).Where(g => g.Count() > 1)];

        // quick pass: drop hard links (one file under two names frees nothing), then compare three 64 KB samples
        var suspects = new List<(long Size, List<DupeFile> Files)>();
        foreach (var g in sameSize)
        {
            var files = g.Select(x => (x.Path, x.File.Modified, Id: Identity(x.Path))).Where(x => x.Id != null).DistinctBy(x => x.Id);
            suspects.AddRange(files.GroupBy(x => Hash(x.Path, sample: true)).Where(q => q.Key != null && q.Count() > 1)
                .Select(q => (g.Key, q.Select(x => new DupeFile(x.Path, x.Modified)).ToList())));
        }

        // full pass: only whole-file hashes are trusted before anyone deletes a copy
        st.Total = suspects.Sum(s => s.Size * s.Files.Count);
        st.Phase = "full";
        var full = new ConcurrentDictionary<string, string?>();
        Parallel.ForEach(suspects.SelectMany(s => s.Files.Select(f => (s.Size, f.Path))), new ParallelOptions { MaxDegreeOfParallelism = 3 }, f =>
        {
            full[f.Path] = Hash(f.Path, sample: false);
            st.Add(f.Size);
        });
        st.Groups = [.. suspects
            .SelectMany(s => s.Files.GroupBy(f => full[f.Path]).Where(q => q.Key != null && q.Count() > 1).Select(q => new DupeGroup(s.Size, [.. q])))
            .OrderByDescending(d => d.Wasted)];
    }

    static string? Hash(string path, bool sample)
    {
        try
        {
            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete, 1 << 20, FileOptions.SequentialScan);
            if (!sample) return Convert.ToHexString(SHA256.HashData(fs));
            using var sha = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
            var buf = new byte[64 << 10];
            foreach (long pos in new[] { 0, fs.Length / 2, Math.Max(0, fs.Length - buf.Length) })
            {
                fs.Position = pos;
                sha.AppendData(buf, 0, fs.ReadAtLeast(buf, buf.Length, throwOnEndOfStream: false));
            }
            return Convert.ToHexString(sha.GetHashAndReset());
        }
        catch { return null; } // locked or gone: not a candidate
    }

    static string? Identity(string path)
    {
        try
        {
            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
            return GetFileInformationByHandle(fs.SafeFileHandle, out var i) ? $"{i.Volume}:{i.IndexHigh}:{i.IndexLow}" : null;
        }
        catch { return null; }
    }

    [StructLayout(LayoutKind.Sequential)]
    struct HandleInfo // BY_HANDLE_FILE_INFORMATION
    {
        public uint Attributes, CreatedLow, CreatedHigh, AccessedLow, AccessedHigh, WrittenLow, WrittenHigh, Volume, SizeHigh, SizeLow, Links, IndexHigh, IndexLow;
    }

    [DllImport("kernel32", SetLastError = true)]
    static extern bool GetFileInformationByHandle(Microsoft.Win32.SafeHandles.SafeFileHandle h, out HandleInfo info);
}
