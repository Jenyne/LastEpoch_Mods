using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter;

/// <summary>Turns a config into indented JSON text.</summary>
public static class HeadhunterConfigWriter
{
    public static string Write(HeadhunterConfig config)
    {
        var root = new JObject
        {
            [HeadhunterConfigKeys.Version] = config.Version,
            [HeadhunterConfigKeys.DefaultsVersion] = HeadhunterConfigDefaults.DefaultsVersion,
            [HeadhunterConfigKeys.Mechanic] = config.Mechanic,
            [HeadhunterConfigKeys.DurationSeconds] = config.DurationSeconds,
            [HeadhunterConfigKeys.Triggers] = BuildTriggers(config.Triggers),
            [HeadhunterConfigKeys.Stats] = BuildStats(config.Stats),
        };
        return root.ToString(Formatting.Indented);
    }

    internal static JObject BuildStat(HeadhunterStatEntry entry)
    {
        return new JObject
        {
            [HeadhunterConfigKeys.Stat] = entry.Stat,
            [HeadhunterConfigKeys.Added] = entry.Added,
            [HeadhunterConfigKeys.Increased] = entry.Increased,
            [HeadhunterConfigKeys.Enabled] = entry.Enabled,
        };
    }

    private static JObject BuildTriggers(HeadhunterTriggers triggers)
    {
        return new JObject
        {
            [HeadhunterConfigKeys.Magic] = triggers.Magic,
            [HeadhunterConfigKeys.Rare] = triggers.Rare,
            [HeadhunterConfigKeys.Boss] = triggers.Boss,
            [HeadhunterConfigKeys.Miniboss] = triggers.Miniboss,
            [HeadhunterConfigKeys.MinionKills] = triggers.MinionKills,
        };
    }

    private static JArray BuildStats(IReadOnlyList<HeadhunterStatEntry> stats)
    {
        var array = new JArray();
        foreach (HeadhunterStatEntry entry in stats)
        {
            array.Add(BuildStat(entry));
        }
        return array;
    }
}
