using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter;
using Newtonsoft.Json.Linq;

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
            new HeadhunterTriggers(true, true, true, true, true),
            HeadhunterConfigDefaults.Config.Triggers
        );
    }

    [Fact]
    public void VersionedFields_AddMagicTrigger_AtCurrentVersion()
    {
        Assert.Contains(
            HeadhunterConfigDefaults.VersionedFields,
            field =>
                field.Parent == "triggers"
                && field.Key == "magic"
                && field.Value.Type == JTokenType.Boolean
                && (bool)field.Value
                && field.Since == 3
        );
    }

    [Fact]
    public void DefaultsVersion_EqualsNewestSince()
    {
        int newest = HeadhunterConfigDefaults
            .VersionedStats.Select(row => row.Since)
            .Concat(HeadhunterConfigDefaults.VersionedFields.Select(field => field.Since))
            .Max();

        Assert.Equal(HeadhunterConfigDefaults.DefaultsVersion, newest);
    }

    [Fact]
    public void VersionedFields_SinceWithinRange()
    {
        Assert.All(
            HeadhunterConfigDefaults.VersionedFields,
            field => Assert.InRange(field.Since, 1, HeadhunterConfigDefaults.DefaultsVersion)
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
    public void VersionedStats_SinceWithinRange()
    {
        Assert.All(
            HeadhunterConfigDefaults.VersionedStats,
            row => Assert.InRange(row.Since, 1, HeadhunterConfigDefaults.DefaultsVersion)
        );
    }

    [Fact]
    public void Stats_EqualVersionedStatEntries_InOrder()
    {
        Assert.Equal(
            HeadhunterConfigDefaults.VersionedStats.Select(row => row.Entry),
            HeadhunterConfigDefaults.Stats
        );
    }

    [Fact]
    public void Config_Stats_AreAllEnabled()
    {
        Assert.All(HeadhunterConfigDefaults.Config.Stats, entry => Assert.True(entry.Enabled));
    }
}
