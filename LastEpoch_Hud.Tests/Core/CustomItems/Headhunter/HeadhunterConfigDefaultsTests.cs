using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter;

namespace LastEpoch_Hud.Tests.Core.CustomItems.Headhunter;

public sealed class HeadhunterConfigDefaultsTests
{
    [Fact]
    public void Config_Version_IsCurrentVersion()
    {
        Assert.Equal(
            HeadhunterConfigDefaults.CurrentVersion,
            HeadhunterConfigDefaults.Config.Version
        );
    }

    [Fact]
    public void Config_Mechanic_IsRareMods()
    {
        Assert.Equal("rare_mods", HeadhunterConfigDefaults.Config.Mechanic);
    }

    [Fact]
    public void Config_Duration_IsSixtySeconds()
    {
        Assert.Equal(60f, HeadhunterConfigDefaults.Config.DurationSeconds);
    }

    [Fact]
    public void Config_Triggers_AreAllOn()
    {
        Assert.Equal(
            new HeadhunterTriggers(true, true, true, true),
            HeadhunterConfigDefaults.Config.Triggers
        );
    }

    [Fact]
    public void Config_Stats_AreNotEmpty()
    {
        Assert.NotEmpty(HeadhunterConfigDefaults.Config.Stats);
    }

    [Fact]
    public void Config_Stats_HaveUniqueNames()
    {
        var names = HeadhunterConfigDefaults.Config.Stats.Select(entry => entry.Stat).ToList();

        Assert.Equal(names.Count, names.Distinct(StringComparer.Ordinal).Count());
    }

    [Fact]
    public void Config_Stats_AreAllEnabled()
    {
        Assert.All(HeadhunterConfigDefaults.Config.Stats, entry => Assert.True(entry.Enabled));
    }
}
