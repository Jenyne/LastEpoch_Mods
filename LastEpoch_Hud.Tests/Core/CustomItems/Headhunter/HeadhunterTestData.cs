using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter;
using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Buffs;
using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Config;
using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Kills;
using LastEpoch_Hud.Tests.Core.CustomItems.Headhunter.Buffs;

namespace LastEpoch_Hud.Tests.Core.CustomItems.Headhunter;

/// <summary>Fake configs and id maps shared by the Headhunter rule tests.</summary>
internal static class HeadhunterTestData
{
    public const float Duration = 7f;
    public const int MaxStacks = 3;

    public static readonly IReadOnlyDictionary<string, int> StatIds = new Dictionary<string, int>
    {
        ["FakeA"] = 1,
        ["FakeB"] = 2,
        ["FakeC"] = 3,
    };

    public static readonly IReadOnlyDictionary<string, int> TagIds = new Dictionary<string, int>
    {
        ["FakeTag"] = 8,
        ["OtherTag"] = 16,
        ["ZeroTag"] = 0,
    };

    public static HeadhunterTriggers AllTriggers => new(true, true, true, true, true);

    public static HeadhunterConfig Config(
        HeadhunterTriggers triggers,
        params HeadhunterStatEntry[] stats
    )
    {
        return new HeadhunterConfig
        {
            Version = 1,
            Mechanic = "fake_mechanic",
            DurationSeconds = Duration,
            MaxStacks = MaxStacks,
            Triggers = triggers,
            Stats = stats,
        };
    }

    public static HeadhunterConfig ConfigWithMap(
        HeadhunterAffixEntry[] map,
        params HeadhunterStatEntry[] stats
    )
    {
        return new HeadhunterConfig
        {
            Version = 1,
            Mechanic = "fake_mechanic",
            DurationSeconds = Duration,
            MaxStacks = MaxStacks,
            Triggers = AllTriggers,
            Stats = stats,
            AffixMap = map,
        };
    }

    public static HeadhunterAffixEntry Affix(int modKey, params string[] rows)
    {
        return new HeadhunterAffixEntry(modKey, "FakeNote", rows);
    }

    public static KillInfo Kill(KillKind kind, params (int Key, HeadhunterStatKey[] Stats)[] mods)
    {
        var flat = new List<HeadhunterStatKey>();
        var ranges = new List<KillMod>();
        foreach ((int key, HeadhunterStatKey[] stats) in mods)
        {
            ranges.Add(new KillMod(key, flat.Count, stats.Length));
            flat.AddRange(stats);
        }

        return new KillInfo(kind, false, flat, ranges);
    }

    public static HeadhunterStatEntry Entry(string stat, float added = 0f, float increased = 0f)
    {
        return new HeadhunterStatEntry(stat, added, increased, true);
    }

    public static HeadhunterStatEntry Tagged(
        string stat,
        string tag,
        float added = 0f,
        float increased = 0f
    )
    {
        return new HeadhunterStatEntry(stat, added, increased, true, tag);
    }

    public static HeadhunterStatKey[] Mods(params int[] statIds)
    {
        return statIds.Select(id => new HeadhunterStatKey(id, 0)).ToArray();
    }

    public static HeadhunterStatEntry Disabled(string stat)
    {
        return new HeadhunterStatEntry(stat, 0f, 0f, false);
    }

    public static HeadhunterResolvedConfig Resolve(HeadhunterConfig config)
    {
        return HeadhunterConfigResolver.Resolve(
            config,
            StatIds,
            TagIds,
            new List<HeadhunterConfigProblem>()
        );
    }

    public static RareModsMechanic Mechanic(HeadhunterResolvedConfig config)
    {
        return Mechanic(config, new HeadhunterStackState(config.Stats.Count));
    }

    public static RareModsMechanic Mechanic(
        HeadhunterResolvedConfig config,
        HeadhunterStackState stacks
    )
    {
        return Mechanic(config, stacks, new FakeHeadhunterRandom(0));
    }

    public static RareModsMechanic Mechanic(
        HeadhunterResolvedConfig config,
        HeadhunterStackState stacks,
        IHeadhunterRandom random
    )
    {
        return new RareModsMechanic(config, stacks, random);
    }
}
