using System.Collections.Generic;

namespace LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter;

/// <summary>Config ready for per-kill use: game stat ids, fractions, buff names.</summary>
public sealed class HeadhunterResolvedConfig
{
    private readonly Dictionary<int, HeadhunterBuffStat> _byId = new();

    public HeadhunterResolvedConfig(
        string mechanic,
        float durationSeconds,
        HeadhunterTriggers triggers,
        IReadOnlyList<HeadhunterBuffStat> stats
    )
    {
        Mechanic = mechanic;
        DurationSeconds = durationSeconds;
        Triggers = triggers;
        Stats = stats;
        for (int i = 0; i < stats.Count; i++)
        {
            _byId.Add(stats[i].StatId, stats[i]);
        }
    }

    public string Mechanic { get; }
    public float DurationSeconds { get; }
    public HeadhunterTriggers Triggers { get; }
    public IReadOnlyList<HeadhunterBuffStat> Stats { get; }

    public bool TryGetStat(int statId, out HeadhunterBuffStat stat)
    {
        return _byId.TryGetValue(statId, out stat);
    }
}
