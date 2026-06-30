using System.Text.Json;
using WindexBar.Core.Config;
using WindexBar.Core.Models;

namespace WindexBar.Core.Refresh;

public interface IRateLimitResetCreditStateStore
{
    RateLimitResetCreditState Load();
    void Save(RateLimitResetCreditState state);
}

public sealed class RateLimitResetCreditState
{
    public const int CurrentVersion = 1;

    public int Version { get; set; } = CurrentVersion;
    public bool HasObserved { get; set; }
    public List<RateLimitResetCreditObservation> Credits { get; set; } = [];
}

public sealed class RateLimitResetCreditTracker
{
    private static readonly TimeSpan EstimatedLifetime = TimeSpan.FromDays(30);
    private readonly IRateLimitResetCreditStateStore _store;

    public RateLimitResetCreditTracker(IRateLimitResetCreditStateStore store)
    {
        _store = store;
    }

    public RateLimitResetCreditsSnapshot Track(RateLimitResetCreditsSnapshot snapshot)
    {
        try
        {
            var state = Update(_store.Load(), snapshot.AvailableCount, snapshot.UpdatedAt);
            _store.Save(state);
            return snapshot with { Credits = state.Credits };
        }
        catch
        {
            return snapshot;
        }
    }

    public static RateLimitResetCreditState Update(
        RateLimitResetCreditState? state,
        long availableCount,
        DateTimeOffset observedAt)
    {
        var safeCount = ToSafeCount(availableCount);
        if (state?.HasObserved != true)
        {
            return new RateLimitResetCreditState
            {
                HasObserved = true,
                Credits = Enumerable
                    .Range(0, safeCount)
                    .Select(_ => new RateLimitResetCreditObservation(observedAt, null))
                    .ToList()
            };
        }

        var credits = (state.Credits ?? [])
            .Where(credit => credit.FirstSeenAt != default)
            .ToList();

        if (credits.Count > safeCount)
        {
            var removeCount = credits.Count - safeCount;
            credits = credits
                .OrderBy(RemovalSortKey)
                .ThenBy(credit => credit.FirstSeenAt)
                .Skip(removeCount)
                .ToList();
        }

        while (credits.Count < safeCount)
        {
            credits.Add(new RateLimitResetCreditObservation(observedAt, observedAt.Add(EstimatedLifetime)));
        }

        return new RateLimitResetCreditState
        {
            HasObserved = true,
            Credits = credits
                .OrderBy(DisplaySortKey)
                .ThenBy(credit => credit.FirstSeenAt)
                .ToList()
        };
    }

    private static int ToSafeCount(long availableCount)
    {
        if (availableCount <= 0)
        {
            return 0;
        }

        return availableCount > int.MaxValue ? int.MaxValue : (int)availableCount;
    }

    private static DateTimeOffset RemovalSortKey(RateLimitResetCreditObservation credit) =>
        credit.EstimatedExpiresAt ?? DateTimeOffset.MaxValue;

    private static DateTimeOffset DisplaySortKey(RateLimitResetCreditObservation credit) =>
        credit.EstimatedExpiresAt ?? DateTimeOffset.MaxValue;
}

public sealed class FileRateLimitResetCreditStateStore : IRateLimitResetCreditStateStore
{
    public FileRateLimitResetCreditStateStore(string? filePath = null)
    {
        FilePath = filePath ?? DefaultPath();
    }

    public string FilePath { get; }

    public RateLimitResetCreditState Load()
    {
        if (!File.Exists(FilePath))
        {
            return new RateLimitResetCreditState();
        }

        try
        {
            var json = File.ReadAllText(FilePath);
            var state = JsonSerializer.Deserialize(json, WindexBarJsonContext.Default.RateLimitResetCreditState);
            return state is null ? new RateLimitResetCreditState() : Normalize(state);
        }
        catch
        {
            return new RateLimitResetCreditState();
        }
    }

    public void Save(RateLimitResetCreditState state)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
        var json = JsonSerializer.Serialize(Normalize(state), WindexBarJsonContext.Default.RateLimitResetCreditState);
        File.WriteAllText(FilePath, json);
    }

    public static string DefaultPath()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        return Path.Combine(appData, "WindexBar", "codex-reset-credits.json");
    }

    private static RateLimitResetCreditState Normalize(RateLimitResetCreditState state) => new()
    {
        Version = RateLimitResetCreditState.CurrentVersion,
        HasObserved = state.HasObserved,
        Credits = (state.Credits ?? [])
            .Where(credit => credit.FirstSeenAt != default)
            .ToList()
    };
}
