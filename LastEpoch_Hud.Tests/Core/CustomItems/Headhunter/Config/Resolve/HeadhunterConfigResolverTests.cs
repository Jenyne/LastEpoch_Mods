using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter;
using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Config;
using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Config.Resolve;
using Code = LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Config.HeadhunterConfigProblemCode;

namespace LastEpoch_Hud.Tests.Core.CustomItems.Headhunter.Config.Resolve;

public sealed class HeadhunterConfigResolverTests
{
    [Fact]
    public void Resolve_BuildsBuffStats_FromEnabledEntries()
    {
        var problems = new List<HeadhunterConfigProblem>();
        HeadhunterConfig config = HeadhunterTestData.Config(
            HeadhunterTestData.AllTriggers,
            HeadhunterTestData.Entry("FakeA", 5f, 10f),
            HeadhunterTestData.Disabled("FakeB"),
            HeadhunterTestData.Entry("FakeC", 0f, 0f)
        );

        HeadhunterResolvedConfig result = HeadhunterConfigResolver.Resolve(
            config,
            HeadhunterTestData.StatIds,
            HeadhunterTestData.TagIds,
            problems
        );

        Assert.Empty(problems);
        Assert.Equal(2, result.Stats.Count);
        Assert.Equal(new HeadhunterBuffStat(1, "HH_FakeA", 5f, 0.1f), result.Stats[0]);
        Assert.Equal(new HeadhunterBuffStat(3, "HH_FakeC", 0f, 0f), result.Stats[1]);
    }

    [Theory]
    [InlineData(10f, 0.1f)]
    [InlineData(0f, 0f)]
    [InlineData(250f, 2.5f)]
    public void Resolve_ConvertsIncreasedPercentToFraction(float percent, float fraction)
    {
        HeadhunterResolvedConfig result = HeadhunterTestData.Resolve(
            HeadhunterTestData.Config(
                HeadhunterTestData.AllTriggers,
                HeadhunterTestData.Entry("FakeA", 0f, percent)
            )
        );

        Assert.Equal(fraction, result.Stats[0].Increased, 5);
    }

    [Fact]
    public void Resolve_SkipsStatMissingFromMap()
    {
        var problems = new List<HeadhunterConfigProblem>();
        HeadhunterConfig config = HeadhunterTestData.Config(
            HeadhunterTestData.AllTriggers,
            HeadhunterTestData.Entry("FakeA"),
            HeadhunterTestData.Entry("FakeZ")
        );

        HeadhunterResolvedConfig result = HeadhunterConfigResolver.Resolve(
            config,
            HeadhunterTestData.StatIds,
            HeadhunterTestData.TagIds,
            problems
        );

        Assert.Equal("HH_FakeA", Assert.Single(result.Stats).BuffName);
        Assert.Equal([(Code.UnknownStat, "stats[1].stat")], HeadhunterTestData.Problems(problems));
    }

    [Fact]
    public void Resolve_ProblemPath_UsesIndexInConfigStats()
    {
        var problems = new List<HeadhunterConfigProblem>();
        HeadhunterConfig config = HeadhunterTestData.Config(
            HeadhunterTestData.AllTriggers,
            HeadhunterTestData.Disabled("FakeZ"),
            HeadhunterTestData.Entry("FakeA"),
            HeadhunterTestData.Entry("FakeY")
        );

        HeadhunterConfigResolver.Resolve(
            config,
            HeadhunterTestData.StatIds,
            HeadhunterTestData.TagIds,
            problems
        );

        Assert.Equal([(Code.UnknownStat, "stats[2].stat")], HeadhunterTestData.Problems(problems));
    }

    [Fact]
    public void Resolve_KeepsFirst_WhenTwoNamesShareId()
    {
        var problems = new List<HeadhunterConfigProblem>();
        var sharedIds = new Dictionary<string, int> { ["FakeA"] = 1, ["FakeB"] = 1 };
        HeadhunterConfig config = HeadhunterTestData.Config(
            HeadhunterTestData.AllTriggers,
            HeadhunterTestData.Entry("FakeA"),
            HeadhunterTestData.Entry("FakeB")
        );

        HeadhunterResolvedConfig result = HeadhunterConfigResolver.Resolve(
            config,
            sharedIds,
            HeadhunterTestData.TagIds,
            problems
        );

        Assert.Equal("HH_FakeA", Assert.Single(result.Stats).BuffName);
        Assert.True(result.TryGetRow(new HeadhunterStatKey(1, 0), out int row));
        Assert.Equal("HH_FakeA", result.Stats[row].BuffName);
        Assert.Equal(
            [(Code.DuplicateStatId, "stats[1].stat")],
            HeadhunterTestData.Problems(problems)
        );
    }

    [Fact]
    public void Resolve_CopiesDurationTriggers()
    {
        var triggers = new HeadhunterTriggers(true, false, true, false, true);
        HeadhunterConfig config = HeadhunterTestData.Config(
            triggers,
            HeadhunterTestData.Entry("FakeA")
        );

        HeadhunterResolvedConfig result = HeadhunterTestData.Resolve(config);

        Assert.Equal(config.DurationSeconds, result.DurationSeconds);
        Assert.Equal(triggers, result.Triggers);
    }

    [Fact]
    public void Resolve_CopiesMaxStacks()
    {
        HeadhunterConfig config = HeadhunterTestData.Config(
            HeadhunterTestData.AllTriggers,
            HeadhunterTestData.Entry("FakeA")
        );

        HeadhunterResolvedConfig result = HeadhunterTestData.Resolve(config);

        Assert.Equal(HeadhunterTestData.MaxStacks, result.MaxStacks);
    }

    [Theory]
    [InlineData(2, 0)]
    [InlineData(3, 1)]
    [InlineData(1, -1)]
    [InlineData(99, -1)]
    public void TryGetRow_ReturnsTableRow(int statId, int expectedRow)
    {
        HeadhunterResolvedConfig result = HeadhunterTestData.Resolve(
            HeadhunterTestData.Config(
                HeadhunterTestData.AllTriggers,
                HeadhunterTestData.Disabled("FakeA"),
                HeadhunterTestData.Entry("FakeB"),
                HeadhunterTestData.Entry("FakeC")
            )
        );

        bool found = result.TryGetRow(new HeadhunterStatKey(statId, 0), out int row);

        Assert.Equal(expectedRow, found ? row : -1);
    }

    [Fact]
    public void Resolve_TaggedRow_SetsTagsAndName()
    {
        HeadhunterResolvedConfig result = HeadhunterTestData.Resolve(
            HeadhunterTestData.Config(
                HeadhunterTestData.AllTriggers,
                HeadhunterTestData.Tagged("FakeA", "FakeTag")
            )
        );

        HeadhunterBuffStat stat = Assert.Single(result.Stats);
        Assert.Equal(8, stat.Tags);
        Assert.Equal("HH_FakeA_FakeTag", stat.BuffName);
    }

    [Fact]
    public void Resolve_UntaggedRow_NameHasNoSuffix()
    {
        HeadhunterResolvedConfig result = HeadhunterTestData.Resolve(
            HeadhunterTestData.Config(
                HeadhunterTestData.AllTriggers,
                HeadhunterTestData.Entry("FakeA")
            )
        );

        HeadhunterBuffStat stat = Assert.Single(result.Stats);
        Assert.Equal(0, stat.Tags);
        Assert.Equal("HH_FakeA", stat.BuffName);
    }

    [Fact]
    public void Resolve_UnknownTag_SkipsWithProblem()
    {
        var problems = new List<HeadhunterConfigProblem>();
        HeadhunterConfig config = HeadhunterTestData.Config(
            HeadhunterTestData.AllTriggers,
            HeadhunterTestData.Tagged("FakeA", "NoSuchTag")
        );

        HeadhunterResolvedConfig result = HeadhunterConfigResolver.Resolve(
            config,
            HeadhunterTestData.StatIds,
            HeadhunterTestData.TagIds,
            problems
        );

        Assert.Empty(result.Stats);
        Assert.Equal([(Code.UnknownTag, "stats[0].tag")], HeadhunterTestData.Problems(problems));
    }

    [Fact]
    public void Resolve_SameStatTwoTags_TwoRows()
    {
        HeadhunterResolvedConfig result = HeadhunterTestData.Resolve(
            HeadhunterTestData.Config(
                HeadhunterTestData.AllTriggers,
                HeadhunterTestData.Entry("FakeA"),
                HeadhunterTestData.Tagged("FakeA", "FakeTag")
            )
        );

        Assert.True(result.TryGetRow(new HeadhunterStatKey(1, 0), out int untagged));
        Assert.True(result.TryGetRow(new HeadhunterStatKey(1, 8), out int tagged));
        Assert.Equal(0, untagged);
        Assert.Equal(1, tagged);
    }

    [Fact]
    public void Resolve_ZeroIdTag_DuplicatesUntagged_Skipped()
    {
        var problems = new List<HeadhunterConfigProblem>();
        HeadhunterConfig config = HeadhunterTestData.Config(
            HeadhunterTestData.AllTriggers,
            HeadhunterTestData.Entry("FakeA"),
            HeadhunterTestData.Tagged("FakeA", "ZeroTag")
        );

        HeadhunterResolvedConfig result = HeadhunterConfigResolver.Resolve(
            config,
            HeadhunterTestData.StatIds,
            HeadhunterTestData.TagIds,
            problems
        );

        Assert.Equal("HH_FakeA", Assert.Single(result.Stats).BuffName);
        Assert.Equal(
            [(Code.DuplicateStatId, "stats[1].stat")],
            HeadhunterTestData.Problems(problems)
        );
    }
}
