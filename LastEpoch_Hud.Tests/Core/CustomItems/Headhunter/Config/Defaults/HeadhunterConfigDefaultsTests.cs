using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Config;
using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Config.Defaults;
using Newtonsoft.Json.Linq;

namespace LastEpoch_Hud.Tests.Core.CustomItems.Headhunter.Config.Defaults;

public sealed class HeadhunterConfigDefaultsTests
{
    private static readonly (string Stat, string Tag)[] _approvedPairs =
    {
        ("DodgeRating", null),
        ("HealthRegen", null),
        ("StunAvoidance", null),
        ("IncreasedCooldownRecoverySpeed", null),
        ("Damage", "Fire"),
        ("Damage", "Cold"),
        ("Damage", "Lightning"),
        ("Damage", "Necrotic"),
        ("Damage", "Poison"),
        ("Damage", "Void"),
        ("Damage", "Physical"),
        ("Damage", "Minion"),
        ("FireResistance", null),
        ("ColdResistance", null),
        ("LightningResistance", null),
        ("NecroticResistance", null),
        ("PoisonResistance", null),
        ("VoidResistance", null),
    };

    public static TheoryData<string, string> ApprovedMapRows => BuildApprovedMapRows();

    [Fact]
    public void Config_Version_IsCurrentVersion()
    {
        Assert.Equal(
            HeadhunterConfigDefaults.CurrentVersion,
            HeadhunterConfigDefaults.Config.Version
        );
    }

    [Fact]
    public void Config_Duration_IsSixtySeconds()
    {
        Assert.Equal(60f, HeadhunterConfigDefaults.Config.DurationSeconds);
    }

    [Fact]
    public void Config_MaxStacks_Is10()
    {
        Assert.Equal(10, HeadhunterConfigDefaults.Config.MaxStacks);
    }

    [Fact]
    public void VersionedFields_AddMaxStacks_AtVersion4()
    {
        Assert.Contains(
            HeadhunterConfigDefaults.VersionedFields,
            field =>
                field.Parent == ""
                && field.Key == "maxStacks"
                && field.Value.Type == JTokenType.Integer
                && (int)field.Value == 10
                && field.Since == 4
        );
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
            .Concat(HeadhunterAffixDefaults.VersionedAffixes.Select(affix => affix.Since))
            .Concat(HeadhunterAffixDefaults.VersionedRows.Select(row => row.Since))
            .Max();

        Assert.Equal(HeadhunterConfigDefaults.DefaultsVersion, newest);
    }

    [Fact]
    public void VersionedStats_Mana_AddedAtVersion8()
    {
        HeadhunterVersionedStat row = HeadhunterConfigDefaults.VersionedStats.Single(candidate =>
            candidate.Entry.Stat == "Mana"
        );

        Assert.Equal(new HeadhunterStatEntry("Mana", 0f, 5f, true), row.Entry);
        Assert.Equal(8, row.Since);
    }

    [Fact]
    public void VersionedStats_HealthLeech_DefaultHalfPercent()
    {
        HeadhunterVersionedStat row = HeadhunterConfigDefaults.VersionedStats.Single(candidate =>
            candidate.Entry.Stat == "HealthLeech"
        );

        Assert.Equal(new HeadhunterStatEntry("HealthLeech", 0.005f, 0f, true), row.Entry);
        Assert.Equal(2, row.Since);
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
    public void Config_Stats_HaveUniqueRowKeys()
    {
        var keys = HeadhunterConfigDefaults
            .Config.Stats.Select(entry => (entry.Stat, entry.Tag))
            .ToList();

        Assert.Equal(keys.Count, keys.Distinct().Count());
    }

    [Theory]
    [MemberData(nameof(ApprovedMapRows))]
    public void Stats_ContainApprovedMapRows(string stat, string tag)
    {
        Assert.Contains(
            HeadhunterConfigDefaults.Stats,
            entry => entry.Stat == stat && entry.Tag == tag
        );
    }

    [Theory]
    [MemberData(nameof(ApprovedMapRows))]
    public void VersionedStats_ApprovedMapRows_AddedAtVersion5(string stat, string tag)
    {
        HeadhunterVersionedStat row = HeadhunterConfigDefaults.VersionedStats.Single(candidate =>
            candidate.Entry.Stat == stat && candidate.Entry.Tag == tag
        );

        Assert.Equal(5, row.Since);
    }

    [Fact]
    public void VersionedStats_Since5Rows_AreExactlyApprovedMap()
    {
        var actual = HeadhunterConfigDefaults
            .VersionedStats.Where(row => row.Since == 5)
            .Select(row => (row.Entry.Stat, row.Entry.Tag))
            .OrderBy(key => key.Stat)
            .ThenBy(key => key.Tag)
            .ToList();
        var expected = _approvedPairs.OrderBy(key => key.Stat).ThenBy(key => key.Tag).ToList();

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void VersionedStats_Since_IsNonDecreasing()
    {
        IReadOnlyList<HeadhunterVersionedStat> rows = HeadhunterConfigDefaults.VersionedStats;

        for (int i = 1; i < rows.Count; i++)
        {
            Assert.True(rows[i].Since >= rows[i - 1].Since);
        }
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

    private static TheoryData<string, string> BuildApprovedMapRows()
    {
        var data = new TheoryData<string, string>();
        foreach ((string stat, string tag) in _approvedPairs)
        {
            data.Add(stat, tag);
        }

        return data;
    }
}
