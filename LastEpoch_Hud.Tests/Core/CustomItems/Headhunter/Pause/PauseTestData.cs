using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Buffs;
using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Config;
using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Config.Resolve;

namespace LastEpoch_Hud.Tests.Core.CustomItems.Headhunter.Pause;

/// <summary>Fake configs and kill actions for the pause tests.</summary>
internal static class PauseTestData
{
    public static HeadhunterResolvedConfig Config(int rows)
    {
        string[] names = ["FakeA", "FakeB", "FakeC"];
        HeadhunterStatEntry[] entries = names
            .Take(rows)
            .Select(name => HeadhunterTestData.Entry(name))
            .ToArray();
        return HeadhunterTestData.Resolve(
            HeadhunterTestData.Config(HeadhunterTestData.AllTriggers, entries)
        );
    }

    public static BuffAction Action(
        HeadhunterResolvedConfig config,
        int row,
        BuffActionKind kind,
        float seconds
    )
    {
        HeadhunterBuffStat stat = config.Stats[row];
        return new BuffAction(kind, stat.BuffName, stat.StatId, 0.5f, 0.25f, seconds, 2, stat.Tags);
    }

    public static BuffAction SetRemaining(HeadhunterResolvedConfig config, int row, float seconds)
    {
        HeadhunterBuffStat stat = config.Stats[row];
        return new BuffAction(
            BuffActionKind.SetRemaining,
            stat.BuffName,
            stat.StatId,
            0f,
            0f,
            seconds,
            0,
            stat.Tags
        );
    }
}
