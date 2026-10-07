using System.Collections.Generic;

namespace LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter;

/// <summary>Config ready for per-kill use: game stat ids, fractions, buff names.</summary>
public sealed class HeadhunterResolvedConfig
{
    private readonly Dictionary<int, int> _rowById = new();

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
            _rowById.Add(stats[i].StatId, i);
        }
    }

    public string Mechanic { get; }
    public float DurationSeconds { get; }
    public int MaxStacks { get; }
    public HeadhunterTriggers Triggers { get; }
    public IReadOnlyList<HeadhunterBuffStat> Stats { get; }

    public bool TryGetRow(int statId, out int row)
    {
        return _rowById.TryGetValue(statId, out row);
    }
}
