using System.Collections.Generic;

namespace LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter;

/// <summary>Config ready for per-kill use: game stat ids, fractions, buff names.</summary>
public sealed class HeadhunterResolvedConfig
{
    private readonly Dictionary<HeadhunterStatKey, int> _rowByKey = new();
    private readonly IReadOnlyDictionary<int, int[]> _affixRows;

    public HeadhunterResolvedConfig(
        string mechanic,
        float durationSeconds,
        int maxStacks,
        HeadhunterTriggers triggers,
        IReadOnlyList<HeadhunterBuffStat> stats,
        IReadOnlyDictionary<int, int[]> affixRows = null
    )
    {
        Mechanic = mechanic;
        DurationSeconds = durationSeconds;
        MaxStacks = maxStacks;
        Triggers = triggers;
        Stats = stats;
        _affixRows = affixRows ?? new Dictionary<int, int[]>();
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
    public int AffixCount => _affixRows.Count;

    public bool TryGetRow(HeadhunterStatKey key, out int row)
    {
        return _rowByKey.TryGetValue(key, out row);
    }

    public bool TryGetAffixRows(int modKey, out IReadOnlyList<int> rows)
    {
        bool found = _affixRows.TryGetValue(modKey, out int[] mapped);
        rows = mapped;
        return found;
    }
}
