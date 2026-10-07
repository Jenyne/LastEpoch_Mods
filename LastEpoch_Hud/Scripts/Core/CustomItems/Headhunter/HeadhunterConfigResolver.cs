using System.Collections.Generic;

namespace LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter;

/// <summary>Turns a parsed config into the per-kill form. Runs once per config load.</summary>
public static class HeadhunterConfigResolver
{
    private const string BuffPrefix = "HH_";
    private const float PercentPerFraction = 100f;

    public static HeadhunterResolvedConfig Resolve(
        HeadhunterConfig config,
        IReadOnlyDictionary<string, int> statIds,
        ICollection<HeadhunterConfigProblem> problems
    )
    {
        return new HeadhunterResolvedConfig(
            config.Mechanic,
            config.DurationSeconds,
            config.MaxStacks,
            config.Triggers,
            ResolveStats(config.Stats, statIds, problems)
        );
    }

    private static List<HeadhunterBuffStat> ResolveStats(
        IReadOnlyList<HeadhunterStatEntry> entries,
        IReadOnlyDictionary<string, int> statIds,
        ICollection<HeadhunterConfigProblem> problems
    )
    {
        var result = new List<HeadhunterBuffStat>();
        var seen = new HashSet<int>();
        for (int i = 0; i < entries.Count; i++)
        {
            HeadhunterStatEntry entry = entries[i];
            if (!entry.Enabled)
            {
                continue;
            }

            if (!statIds.TryGetValue(entry.Stat, out int statId))
            {
                AddProblem(problems, i, "Unknown stat: " + entry.Stat);
                continue;
            }

            if (!seen.Add(statId))
            {
                AddProblem(problems, i, "Stat shares a game id with an earlier row: " + entry.Stat);
                continue;
            }

            result.Add(ToBuffStat(entry, statId));
        }

        return result;
    }

    private static void AddProblem(
        ICollection<HeadhunterConfigProblem> problems,
        int index,
        string message
    )
    {
        string path = HeadhunterConfigKeys.Stats + "[" + index + "]." + HeadhunterConfigKeys.Stat;
        problems.Add(new HeadhunterConfigProblem(path, message));
    }

    private static HeadhunterBuffStat ToBuffStat(HeadhunterStatEntry entry, int statId)
    {
        return new HeadhunterBuffStat(
            statId,
            BuffPrefix + entry.Stat,
            entry.Added,
            entry.Increased / PercentPerFraction
        );
    }
}
