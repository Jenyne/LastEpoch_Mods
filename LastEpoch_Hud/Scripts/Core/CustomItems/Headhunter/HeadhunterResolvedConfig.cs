using System.Collections.Generic;

namespace LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter;

/// <summary>Config ready for per-kill use: game stat ids, fractions, buff names.</summary>
public sealed class HeadhunterResolvedConfig
{
    private readonly Dictionary<HeadhunterStatKey, int> _rowByKey = new();

    public HeadhunterResolvedConfig(
        string mechanic,
        float durationSeconds,
        int maxStacks,
        HeadhunterTriggers triggers,
        IReadOnlyList<HeadhunterBuffStat> stats
    )
    {
        Mechanic = mechanic;
        DurationSeconds = durationSeconds;
        MaxStacks = maxStacks;
        Triggers = triggers;
        Stats = stats;
        for (int i = 0; i < stats.Count; i++)
        {
            _rowByKey.Add(stats[i].Key, i);
        }
    }

    public string Mechanic { get; }
    public float DurationSeconds { get; }
    public int MaxStacks { get; }
    public HeadhunterTriggers Triggers { get; }
    public IReadOnlyList<HeadhunterBuffStat> Stats { get; }

    public bool TryGetRow(HeadhunterStatKey key, out int row)
    {
        return _rowByKey.TryGetValue(key, out row);
    }
}
