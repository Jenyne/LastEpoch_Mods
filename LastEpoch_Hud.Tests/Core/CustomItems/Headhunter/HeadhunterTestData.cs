using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter;

namespace LastEpoch_Hud.Tests.Core.CustomItems.Headhunter;

/// <summary>Fake configs and id maps shared by the Headhunter rule tests.</summary>
internal static class HeadhunterTestData
{
    public const float Duration = 7f;

    public static readonly IReadOnlyDictionary<string, int> StatIds = new Dictionary<string, int>
    {
        ["FakeA"] = 1,
        ["FakeB"] = 2,
        ["FakeC"] = 3,
    };

    public static HeadhunterTriggers AllTriggers => new(true, true, true, true);

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
            Triggers = triggers,
            Stats = stats,
        };
    }

    public static HeadhunterStatEntry Entry(string stat, float added = 0f, float increased = 0f)
    {
        return new HeadhunterStatEntry(stat, added, increased, true);
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
            new List<HeadhunterConfigProblem>()
        );
    }
}
