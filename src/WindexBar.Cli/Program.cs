using System.Text.Json;
using WindexBar.Core.Models;
using WindexBar.Core.Formatting;
using WindexBar.Core.Config;
using WindexBar.Core.Refresh;

var output = CliOutputPreferences.From(args);
var command = args.FirstOrDefault(a => !a.StartsWith("-", StringComparison.Ordinal)) ?? "usage";
if (command is "-h" or "--help" or "help")
{
    PrintHelp();
    return 0;
}

if (!string.Equals(command, "usage", StringComparison.OrdinalIgnoreCase))
{
    Console.Error.WriteLine($"Unknown command: {command}");
    PrintHelp();
    return 2;
}

var provider = OptionValue(args, "--provider") ?? "codex";
var resolvedProvider = NormalizeProvider(provider);
if (resolvedProvider is null)
{
    return WriteError(output, provider, "Only the codex provider is implemented in v1.");
}

var settings = new SettingsStore(new WindexBarConfigStore());
var store = new UsageStore(settings);
await store.RefreshAsync();

if (store.LastError is not null && store.Snapshot is null)
{
    return WriteError(output, "codex", store.LastError);
}

var payload = new ProviderPayload(
    resolvedProvider,
    store.LastSourceLabel ?? "codex-cli",
    store.Snapshot?.UpdatedAt,
    store.Snapshot?.Primary,
    store.Snapshot?.Secondary,
    store.Snapshot?.TokenUsage,
    store.Snapshot?.RateLimitResetCredits,
    store.Credits?.Remaining,
    store.Snapshot?.Identity?.AccountEmail,
    store.Snapshot?.Identity?.LoginMethod,
    store.LastError);

if (output.Json)
{
    Console.WriteLine(JsonSerializer.Serialize(payload, JsonOptions()));
}
else
{
    Console.WriteLine("Codex");
    Console.WriteLine($"  Source: {payload.Source}");
    Console.WriteLine($"  Session: {FormatWindow(payload.Primary)}");
    Console.WriteLine($"  Weekly:  {FormatWindow(payload.Secondary)}");
    if (payload.RateLimitResetCredits is not null)
    {
        Console.WriteLine($"  Limit resets: {payload.RateLimitResetCredits.AvailableCount:N0} available");
    }

    if (payload.CreditsRemaining is not null)
    {
        Console.WriteLine($"  Credits: {payload.CreditsRemaining:0.##}");
    }

    if (payload.TokenUsage is not null)
    {
        Console.WriteLine($"  Tokens:  {FormatTokenUsage(payload.TokenUsage, settings.Config.Language)}");
    }

    if (!string.IsNullOrWhiteSpace(payload.Error))
    {
        Console.WriteLine($"  Warning: {payload.Error}");
    }
}

return store.LastError is null ? 0 : 1;

static int WriteError(CliOutputPreferences output, string provider, string error)
{
    if (output.Json)
    {
        Console.WriteLine(JsonSerializer.Serialize(new ProviderPayload(provider, "auto", null, null, null, null, null, null, null, null, error), JsonOptions()));
    }
    else
    {
        Console.Error.WriteLine($"Error: {error}");
    }

    return 1;
}

static string? OptionValue(string[] args, string name)
{
    for (var index = 0; index < args.Length - 1; index++)
    {
        if (string.Equals(args[index], name, StringComparison.OrdinalIgnoreCase))
        {
            return args[index + 1];
        }
    }

    return null;
}

static string Format(double? percent) => percent is null ? "unknown" : $"{percent:0.#}%";

static string FormatWindow(RateWindow? window)
{
    if (window is null)
    {
        return "unknown";
    }

    var reset = FormatReset(window.ResetsAt);
    return reset is null
        ? $"{Format(window.RemainingPercent)} left"
        : $"{Format(window.RemainingPercent)} left, resets {reset}";
}

static string? FormatReset(DateTimeOffset? resetsAt)
{
    if (resetsAt is null)
    {
        return null;
    }

    var delta = resetsAt.Value - DateTimeOffset.Now;
    if (delta.TotalSeconds <= 0)
    {
        return "now";
    }

    if (delta.TotalHours >= 24)
    {
        var days = delta.TotalDays >= 10 ? Math.Round(delta.TotalDays) : Math.Round(delta.TotalDays, 1);
        return $"in {days:0.#}d";
    }

    if (delta.TotalHours >= 1)
    {
        return $"in {(int)Math.Round(delta.TotalHours)}h";
    }

    return $"in {Math.Max(1, (int)Math.Round(delta.TotalMinutes))}m";
}

static string FormatTokenUsage(TokenUsageSnapshot tokenUsage, string language)
{
    var values = new List<string>();
    var current = tokenUsage.Last ?? tokenUsage.Total;
    if (current is not null && tokenUsage.ModelContextWindow is { } window)
    {
        values.Add($"context {TokenCountFormatter.Format(current.TotalTokens, language)} / {TokenCountFormatter.Format(window, language)}");
    }
    else if (current is not null)
    {
        values.Add($"context {TokenCountFormatter.Format(current.TotalTokens, language)}");
    }

    if (tokenUsage.Total is not null)
    {
        values.Add($"session total {TokenCountFormatter.Format(tokenUsage.Total.TotalTokens, language)}");
    }

    return values.Count == 0 ? "unknown" : string.Join(", ", values);
}


static JsonSerializerOptions JsonOptions() => new(JsonSerializerDefaults.Web) { WriteIndented = true };

static void PrintHelp()
{
    Console.WriteLine("Usage:");
    Console.WriteLine("  windexbar usage --provider codex|codex-spark [--json]");
}

static string? NormalizeProvider(string? provider)
{
    return provider?.Trim().ToLowerInvariant() switch
    {
        "codex" => "codex",
        "codex-spark" => "codex",
        "gpt-5.3-codex-spark" => "codex",
        "gpt-5-3-codex-spark" => "codex",
        "codexspark" => "codex",
        _ => null
    };
}

internal sealed record CliOutputPreferences(bool Json)
{
    public static CliOutputPreferences From(string[] args) => new(args.Any(a => a is "--json" or "--json-output"));
}

internal sealed record ProviderPayload(
    string Provider,
    string Source,
    DateTimeOffset? UpdatedAt,
    RateWindow? Primary,
    RateWindow? Secondary,
    TokenUsageSnapshot? TokenUsage,
    RateLimitResetCreditsSnapshot? RateLimitResetCredits,
    double? CreditsRemaining,
    string? AccountEmail,
    string? Plan,
    string? Error);
