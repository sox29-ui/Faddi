using System.Text.Json;
using Microsoft.Win32;

namespace Faddi;

/// <summary>What the user chose, kept in %LOCALAPPDATA%\Faddi\settings.json.</summary>
public sealed class Settings
{
    public string? Lang { get; set; }
    public bool Trim { get; set; } = true;      // empty what programs hold in RAM but are not using
    public bool Standby { get; set; }           // also empty Windows' standby cache (admin only)
    public bool Clipboard { get; set; }         // also clear the clipboard
    public bool AutoLow { get; set; }           // optimize when usage reaches LowPercent
    public int LowPercent { get; set; } = 85;
    public bool AutoEvery { get; set; }         // optimize every EveryMinutes
    public int EveryMinutes { get; set; } = 10;
    public bool IdleOnly { get; set; } = true;  // auto runs only while the CPU is quiet
    public bool CloseToTray { get; set; } = true;
    public bool Notify { get; set; }

    static readonly string FilePath = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Faddi", "settings.json");

    public static Settings Current { get; set; } = Load().Clamped(); // a hand-edited file gets the same limits as the page

    static Settings Load()
    {
        try { return JsonSerializer.Deserialize<Settings>(File.ReadAllText(FilePath)) ?? new(); }
        catch { return new(); }
    }

    public void Save()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
            File.WriteAllText(FilePath, JsonSerializer.Serialize(this));
        }
        catch { }
    }

    /// <summary>Values from the page are only trusted inside these ranges.</summary>
    public Settings Clamped()
    {
        LowPercent = Math.Clamp(LowPercent, 50, 95);
        EveryMinutes = new[] { 3, 5, 10, 30, 60 }.MinBy(m => Math.Abs(m - EveryMinutes));
        Lang = Lang is { Length: >= 2 and <= 5 } ? Lang : null;
        return this;
    }
}

/// <summary>"Run at Windows startup" lives in the per-user Run key; the registry is the only source of truth.</summary>
public static class Startup
{
    const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";

    public static bool Enabled
    {
        get
        {
            using var k = Registry.CurrentUser.OpenSubKey(RunKey);
            return k?.GetValue("Faddi") is string v && v.Contains(Environment.ProcessPath!, StringComparison.OrdinalIgnoreCase);
        }
        set
        {
            using var k = Registry.CurrentUser.CreateSubKey(RunKey);
            if (value) k.SetValue("Faddi", $"\"{Environment.ProcessPath}\" --tray");
            else k.DeleteValue("Faddi", false);
        }
    }
}
