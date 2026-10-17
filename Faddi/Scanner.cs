using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Faddi;

/// <summary>One folder in the scanned tree. Sizes cover everything below it.</summary>
public sealed class DirNode(string name, DirNode? parent)
{
    public readonly string Name = name;
    public DirNode? Parent = parent;
    public DirNode[] Children = [];
    public long Size;       // bytes on disk, whole subtree (grows live while scanning)
    public long Files;      // file count, whole subtree
    public long OwnSize;    // bytes of the files directly inside
    public int OwnFiles;
    public long Newest;     // newest file write time in the subtree (FILETIME)
    public bool Denied;     // Windows refused to list it
    public bool Pruned;     // children dropped to save memory; rescan before showing them
    public int Id;          // handle given to the UI, 0 until needed

    public string FullPath => Parent == null ? Name : Path.Join(Parent.FullPath, Name);

    public bool IsUnder(DirNode ancestor)
    {
        for (var n = this; n != null; n = n.Parent) if (n == ancestor) return true;
        return false;
    }
}

public sealed record BigFile(DirNode Dir, string Name, long Size, long Modified)
{
    public string FullPath => Path.Join(Dir.FullPath, Name);
}

/// <summary>Parallel FindFirstFileEx walk of a drive or folder.</summary>
public sealed unsafe class Scanner
{
    public const long BigFileMin = 10L << 20;

    public readonly DirNode Root;
    public readonly long ClusterSize;
    public long FilesSeen, DirsSeen;
    public string Current = "";
    public TimeSpan Elapsed;
    public List<BigFile> BigFiles = [];
    public Dictionary<string, (long Size, long Count)> Extensions = [];
    public readonly ConcurrentBag<DirNode> DeniedDirs = [];

    public Scanner(string root)
    {
        root = Path.GetFullPath(root);
        Root = new DirNode(root, null);
        ClusterSize = GetDiskFreeSpaceW(Path.GetPathRoot(root)!, out uint spc, out uint bps, out _, out _) ? spc * bps : 4096;
    }

    public void Run(CancellationToken ct = default)
    {
        var sw = Stopwatch.StartNew();
        var work = new ConcurrentStack<(DirNode Node, string Path)>();
        work.Push((Root, Root.Name.StartsWith(@"\\") ? Root.Name : @"\\?\" + Root.Name));
        int pending = 1;
        var locals = new ConcurrentBag<Local>();
        var threads = Enumerable.Range(0, Math.Clamp(Environment.ProcessorCount, 4, 16)).Select(_ => new Thread(() =>
        {
            var local = new Local();
            locals.Add(local);
            var kids = new List<(DirNode, string)>();
            while (Volatile.Read(ref pending) > 0 && !ct.IsCancellationRequested)
            {
                if (!work.TryPop(out var item)) { Thread.Sleep(1); continue; }
                kids.Clear();
                ScanDir(item.Node, item.Path, local, kids);
                // count the children before releasing the parent so pending never touches 0 early
                if (kids.Count > 0) { Interlocked.Add(ref pending, kids.Count); work.PushRange([.. kids]); }
                Interlocked.Decrement(ref pending);
            }
        }) { IsBackground = true }).ToList();
        threads.ForEach(t => t.Start());
        threads.ForEach(t => t.Join());

        var big = new List<BigFile>();
        var ext = new Dictionary<ulong, (long Size, long Count)>();
        foreach (var l in locals)
        {
            big.AddRange(l.Big);
            foreach (var (k, v) in l.Ext)
            {
                ref var e = ref CollectionsMarshal.GetValueRefOrAddDefault(ext, k, out _);
                e.Size += v.Size; e.Count += v.Count;
            }
        }
        big.Sort((a, b) => b.Size.CompareTo(a.Size));
        BigFiles = big;
        Extensions = ext.ToDictionary(kv => ExtName(kv.Key), kv => kv.Value);
        Elapsed = sw.Elapsed;
    }

    /// <summary>Walks a display path ("C:\Users\x") down the tree.</summary>
    public DirNode? Find(string path)
    {
        if (!path.StartsWith(Root.Name, StringComparison.OrdinalIgnoreCase)) return null;
        DirNode? node = Root;
        foreach (var part in path[Root.Name.Length..].Split('\\', StringSplitOptions.RemoveEmptyEntries))
            if ((node = Array.Find(node.Children, c => c.Name.Equals(part, StringComparison.OrdinalIgnoreCase))) == null) return null;
        return node;
    }

    /// <summary>Bytes a file really takes on this disk.</summary>
    public long Alloc(uint attrs, long size, string dir, ReadOnlySpan<char> name) =>
        (attrs & 0x401000) != 0 ? 0                               // cloud-only placeholder (OneDrive): nothing stored locally
        : (attrs & 0xA00) != 0 ? CompressedSize(dir, name, size)  // NTFS-compressed or sparse
        : size < 700 ? 0                                          // shortcut: tiny files sit inside the MFT record; exact needs raw MFT reads
        : (size + ClusterSize - 1) / ClusterSize * ClusterSize;

    /// <summary>
    /// Forgets the inside of folders smaller than <paramref name="below"/>: a drive full of node_modules holds most of
    /// its folders there. Their totals stay; Refresh() brings the inside back when someone opens one.
    /// </summary>
    public int Prune(long below)
    {
        int dropped = 0;
        var stack = new Stack<DirNode>([Root]);
        while (stack.TryPop(out var n))
            foreach (var c in n.Children)
                if (c.Size >= below) stack.Push(c);
                else if (c.Children.Length > 0) { c.Children = []; c.Pruned = true; dropped++; }
        return dropped;
    }

    /// <summary>Takes a deleted file or folder out of the tree so totals stay right without a rescan.</summary>
    public void Removed(string path, long fileSize = 0)
    {
        var dir = Find(path);
        if (dir is { Parent: { } parent })
        {
            parent.Children = Array.FindAll(parent.Children, c => c != dir);
            for (var p = parent; p != null; p = p.Parent) { p.Size -= dir.Size; p.Files -= dir.Files; }
            lock (BigFiles) BigFiles.RemoveAll(f => f.Dir.IsUnder(dir));
            return;
        }
        var owner = Find(Path.GetDirectoryName(path) ?? "");
        if (owner == null) return;
        lock (BigFiles)
        {
            int i = BigFiles.FindIndex(f => f.Dir == owner && f.Name.Equals(Path.GetFileName(path), StringComparison.OrdinalIgnoreCase));
            if (i >= 0) { fileSize = BigFiles[i].Size; BigFiles.RemoveAt(i); }
        }
        owner.OwnSize -= fileSize;
        owner.OwnFiles--;
        for (var p = owner; p != null; p = p.Parent) { p.Size -= fileSize; p.Files--; }
    }

    /// <summary>Rescans one folder (after cleaning inside it) and grafts the result into the tree.</summary>
    public void Refresh(string path)
    {
        var node = Find(path);
        if (node == null) return;
        var fresh = new Scanner(node.FullPath);
        fresh.Run();
        if (fresh.Root.Denied) return; // could not look inside: keep the old numbers
        foreach (var c in fresh.Root.Children) c.Parent = node;
        long dSize = fresh.Root.Size - node.Size, dFiles = fresh.Root.Files - node.Files;
        node.Children = fresh.Root.Children;
        node.Pruned = false;
        node.OwnSize = fresh.Root.OwnSize;
        node.OwnFiles = fresh.Root.OwnFiles;
        for (var p = node; p != null; p = p.Parent) { p.Size += dSize; p.Files += dFiles; }
        lock (BigFiles)
        {
            BigFiles.RemoveAll(f => f.Dir.IsUnder(node));
            BigFiles.AddRange(fresh.BigFiles.Select(f => f.Dir == fresh.Root ? f with { Dir = node } : f));
            BigFiles.Sort((a, b) => b.Size.CompareTo(a.Size));
        }
    }

    sealed class Local
    {
        public readonly Dictionary<ulong, (long Size, long Count)> Ext = [];
        public readonly List<BigFile> Big = [];
    }

    void ScanDir(DirNode node, string path, Local local, List<(DirNode, string)> kids)
    {
        var sep = path.EndsWith('\\') ? "" : @"\";
        var h = FindFirstFileExW(path + sep + "*", 1 /*FindExInfoBasic*/, out FindData fd, 0, 0, 2 /*LARGE_FETCH*/);
        if (h == -1)
        {
            if (Marshal.GetLastWin32Error() != 2 /*FILE_NOT_FOUND = empty*/) { node.Denied = true; DeniedDirs.Add(node); }
            return;
        }
        long own = 0, newest = 0;
        int files = 0;
        var dirs = new List<DirNode>();
        try
        {
            do
            {
                var name = MemoryMarshal.CreateReadOnlySpanFromNullTerminated((char*)fd.FileName);
                uint a = fd.Attributes;
                if ((a & 0x10) != 0) // directory
                {
                    if (name is "." or "..") continue;
                    // junctions and symlinks point at data counted elsewhere (or loop back): never follow
                    if ((a & 0x400) != 0 && fd.Reserved0 is 0xA0000003 or 0xA000000C) continue;
                    var child = new DirNode(new string(name), node);
                    dirs.Add(child);
                    kids.Add((child, path + sep + child.Name));
                    continue;
                }
                long alloc = Alloc(a, (long)fd.SizeHigh << 32 | fd.SizeLow, path, name);
                long write = (long)fd.WriteHigh << 32 | fd.WriteLow;
                own += alloc;
                files++;
                if (write > newest) newest = write;
                ref var e = ref CollectionsMarshal.GetValueRefOrAddDefault(local.Ext, ExtKey(name), out _);
                e.Size += alloc; e.Count++;
                if (alloc >= BigFileMin) local.Big.Add(new BigFile(node, new string(name), alloc, write));
            } while (FindNextFileW(h, out fd));
        }
        finally { FindClose(h); }

        node.Children = dirs.Count == 0 ? [] : [.. dirs];
        node.OwnSize = own;
        node.OwnFiles = files;
        for (var p = node; p != null; p = p.Parent)
        {
            Interlocked.Add(ref p.Size, own);
            Interlocked.Add(ref p.Files, files);
            for (long cur; (cur = Volatile.Read(ref p.Newest)) < newest && Interlocked.CompareExchange(ref p.Newest, newest, cur) != cur;) { }
        }
        Interlocked.Add(ref FilesSeen, files);
        if ((Interlocked.Increment(ref DirsSeen) & 255) == 0) Current = node.FullPath;
    }

    static long CompressedSize(string dir, ReadOnlySpan<char> name, long fallback)
    {
        uint lo = GetCompressedFileSizeW(Path.Join(dir, name), out uint hi);
        return lo == uint.MaxValue && Marshal.GetLastWin32Error() != 0 ? fallback : (long)hi << 32 | lo;
    }

    // Extensions up to 8 ASCII chars packed into a ulong: per-file stats without allocating strings.
    static ulong ExtKey(ReadOnlySpan<char> name)
    {
        int dot = name.LastIndexOf('.');
        if (dot <= 0 || name.Length - dot - 1 is 0 or > 8) return 0;
        ulong k = 0;
        foreach (char c in name[(dot + 1)..])
        {
            if (c > 0x7F) return 0;
            k = k << 8 | (byte)(c is >= 'A' and <= 'Z' ? c + 32 : c);
        }
        return k;
    }

    static string ExtName(ulong k)
    {
        var s = "";
        for (; k != 0; k >>= 8) s = (char)(k & 0xFF) + s;
        return s;
    }

    [StructLayout(LayoutKind.Sequential)]
    struct FindData // WIN32_FIND_DATAW; ushort buffers keep it blittable
    {
        public uint Attributes;
        public uint CreationLow, CreationHigh, AccessLow, AccessHigh, WriteLow, WriteHigh;
        public uint SizeHigh, SizeLow, Reserved0, Reserved1;
        public fixed ushort FileName[260];
        public fixed ushort AltName[14];
    }

    [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
    static extern nint FindFirstFileExW(string name, int infoLevel, out FindData data, int searchOp, nint filter, int flags);

    [DllImport("kernel32", SetLastError = true)]
    static extern bool FindNextFileW(nint h, out FindData data);

    [DllImport("kernel32")]
    static extern bool FindClose(nint h);

    [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
    static extern uint GetCompressedFileSizeW(string name, out uint high);

    [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
    static extern bool GetDiskFreeSpaceW(string root, out uint sectorsPerCluster, out uint bytesPerSector, out uint freeClusters, out uint totalClusters);
}
