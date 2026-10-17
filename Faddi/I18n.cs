using System.Globalization;
using System.Text.Json;

namespace Faddi;

/// <summary>
/// Arabic is written straight in the code; every other language is an embedded lang\xx.json that maps the
/// Arabic text to its translation. Adding a language is adding one file.
/// </summary>
public static class I18n
{
    public static string Lang { get; private set; } = "ar";
    public static bool Rtl => Lang == "ar";
    static Dictionary<string, string> map = [];

    public static IEnumerable<string> Available =>
        ["ar", .. typeof(I18n).Assembly.GetManifestResourceNames().Where(n => n.StartsWith("lang.")).Select(n => n[5..^5])];

    public static void Set(string? lang)
    {
        lang ??= CultureInfo.CurrentUICulture.TwoLetterISOLanguageName; // until the user picks one: the language Windows is in
        var m = lang == "ar" ? [] : Read(lang);
        (Lang, map) = m == null ? ("en", Read("en") ?? []) : (lang, m); // no file for it: English
    }

    public static Dictionary<string, string>? Read(string lang)
    {
        using var s = typeof(I18n).Assembly.GetManifestResourceStream($"lang.{lang}.json");
        return s == null ? null : JsonSerializer.Deserialize<Dictionary<string, string>>(s);
    }

    public static Dictionary<string, string> Map => map;

    public static string T(string ar) => map.GetValueOrDefault(ar, ar);

    public static string T(string ar, params object?[] args) => string.Format(T(ar), args);
}
