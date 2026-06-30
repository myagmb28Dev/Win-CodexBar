using System.Globalization;
using WindexBar.Core.Config;
using WindexBar.Core.Models;

namespace WindexBar.Core.Formatting;

public static class RateLimitResetCreditFormatter
{
    public static string Format(
        RateLimitResetCreditsSnapshot? resetCredits,
        string? language,
        DateTimeOffset? now = null)
    {
        if (resetCredits is null)
        {
            return Unknown(language);
        }

        var isKorean = IsKorean(language);
        var parts = new List<string>
        {
            isKorean
                ? $"{resetCredits.AvailableCount:N0}\uAC1C \uB0A8\uC74C"
                : $"{resetCredits.AvailableCount:N0} available"
        };

        if (resetCredits.AvailableCount <= 0)
        {
            return parts[0];
        }

        var referenceTime = now ?? DateTimeOffset.Now;
        if (resetCredits.NextEstimatedExpiresAt is { } nextEstimatedExpiresAt)
        {
            parts.Add(isKorean
                ? $"\uB2E4\uC74C \uCD94\uC815 \uB9CC\uB8CC {FormatRelative(nextEstimatedExpiresAt, referenceTime, language)}"
                : $"next estimated expiry {FormatRelative(nextEstimatedExpiresAt, referenceTime, language)}");
        }

        var unknownCount = resetCredits.UnknownExpirationCount;
        if (unknownCount > 0)
        {
            parts.Add(isKorean
                ? FormatUnknownKorean(unknownCount, resetCredits.AvailableCount)
                : FormatUnknownEnglish(unknownCount, resetCredits.AvailableCount));
        }

        return string.Join(", ", parts);
    }

    public static string FormatCompact(
        RateLimitResetCreditsSnapshot resetCredits,
        string? language,
        DateTimeOffset? now = null)
    {
        var isKorean = IsKorean(language);
        var baseText = resetCredits.AvailableCount.ToString("N0", CultureInfo.InvariantCulture);
        if (resetCredits.AvailableCount <= 0)
        {
            return baseText;
        }

        var referenceTime = now ?? DateTimeOffset.Now;
        if (resetCredits.NextEstimatedExpiresAt is { } nextEstimatedExpiresAt)
        {
            var relative = FormatRelative(nextEstimatedExpiresAt, referenceTime, language);
            return isKorean
                ? $"{baseText}, \uB9CC\uB8CC ~{relative}"
                : $"{baseText}, exp ~{relative}";
        }

        if (resetCredits.UnknownExpirationCount > 0)
        {
            return isKorean
                ? $"{baseText}, \uB9CC\uB8CC ?"
                : $"{baseText}, exp ?";
        }

        return baseText;
    }

    public static string FormatRelative(DateTimeOffset target, DateTimeOffset now, string? language)
    {
        var isKorean = IsKorean(language);
        var delta = target - now;
        if (delta.TotalSeconds <= 0)
        {
            return isKorean ? "\uC9C0\uAE08" : "now";
        }

        if (delta.TotalHours >= 24)
        {
            var days = delta.TotalDays >= 10 ? Math.Round(delta.TotalDays) : Math.Round(delta.TotalDays, 1);
            var daysText = days.ToString("0.#", CultureInfo.InvariantCulture);
            return isKorean ? $"{daysText}\uC77C \uD6C4" : $"in {daysText}d";
        }

        if (delta.TotalHours >= 1)
        {
            var hours = Math.Max(1, (int)Math.Round(delta.TotalHours));
            return isKorean ? $"{hours}\uC2DC\uAC04 \uD6C4" : $"in {hours}h";
        }

        var minutes = Math.Max(1, (int)Math.Round(delta.TotalMinutes));
        return isKorean ? $"{minutes}\uBD84 \uD6C4" : $"in {minutes}m";
    }

    private static string FormatUnknownEnglish(long unknownCount, long availableCount) =>
        unknownCount >= availableCount
            ? "expiry unknown"
            : $"{unknownCount:N0} expiry unknown";

    private static string FormatUnknownKorean(long unknownCount, long availableCount) =>
        unknownCount >= availableCount
            ? "\uB9CC\uB8CC \uD655\uC778 \uBD88\uAC00"
            : $"{unknownCount:N0}\uAC1C \uB9CC\uB8CC \uD655\uC778 \uBD88\uAC00";

    private static string Unknown(string? language) => IsKorean(language) ? "\uC54C \uC218 \uC5C6\uC74C" : "unknown";

    private static bool IsKorean(string? language) => WindexBarConfig.NormalizeLanguage(language) == "ko";
}
