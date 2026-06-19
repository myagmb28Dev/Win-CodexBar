using System.Globalization;
using WindexBar.Core.Config;

namespace WindexBar.Core.Formatting;

public static class TokenCountFormatter
{
    public static string Format(long tokens, string? language) =>
        WindexBarConfig.NormalizeLanguage(language) == "ko"
            ? FormatKorean(tokens)
            : FormatEnglish(tokens);

    public static string FormatEnglish(long tokens)
    {
        var magnitude = Math.Abs(tokens);
        if (magnitude >= 1_000_000)
        {
            return (tokens / 1_000_000d).ToString("0.#", CultureInfo.InvariantCulture) + "M";
        }

        if (magnitude >= 1_000)
        {
            return (tokens / 1_000d).ToString("0.#", CultureInfo.InvariantCulture) + "K";
        }

        return tokens.ToString(CultureInfo.InvariantCulture);
    }

    public static string FormatKorean(long tokens)
    {
        var sign = tokens < 0 ? "-" : string.Empty;
        var value = Math.Abs(tokens);
        if (value < 1_000)
        {
            return sign + value.ToString(CultureInfo.InvariantCulture);
        }

        var parts = new List<string>();
        AppendUnit(parts, ref value, 100_000_000, "\uC5B5");
        AppendUnit(parts, ref value, 10_000, "\uB9CC");
        AppendUnit(parts, ref value, 1_000, "\uCC9C");

        return sign + string.Join(" ", parts);
    }

    private static void AppendUnit(List<string> parts, ref long value, long unit, string suffix)
    {
        var count = value / unit;
        if (count <= 0)
        {
            return;
        }

        parts.Add(count.ToString(CultureInfo.InvariantCulture) + suffix);
        value %= unit;
    }
}
