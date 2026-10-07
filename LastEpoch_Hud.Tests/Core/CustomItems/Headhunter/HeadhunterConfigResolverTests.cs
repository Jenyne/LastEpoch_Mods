using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter;

namespace LastEpoch_Hud.Tests.Core.CustomItems.Headhunter;

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
            problems
        );

        Assert.Equal("HH_FakeA", Assert.Single(result.Stats).BuffName);
        Assert.Equal(["stats[1].stat"], problems.Select(problem => problem.Path));
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

        HeadhunterConfigResolver.Resolve(config, HeadhunterTestData.StatIds, problems);

        Assert.Equal(["stats[2].stat"], problems.Select(problem => problem.Path));
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
            problems
        );

        Assert.Equal("HH_FakeA", Assert.Single(result.Stats).BuffName);
        Assert.True(result.TryGetStat(1, out HeadhunterBuffStat stat));
        Assert.Equal("HH_FakeA", stat.BuffName);
        Assert.Equal(["stats[1].stat"], problems.Select(problem => problem.Path));
    }

    [Fact]
    public void Resolve_CopiesMechanicDurationTriggers()
    {
        var triggers = new HeadhunterTriggers(true, false, true, false, true);
        HeadhunterConfig config = HeadhunterTestData.Config(
            triggers,
            HeadhunterTestData.Entry("FakeA")
        );

        HeadhunterResolvedConfig result = HeadhunterTestData.Resolve(config);

        Assert.Equal(config.Mechanic, result.Mechanic);
        Assert.Equal(config.DurationSeconds, result.DurationSeconds);
        Assert.Equal(triggers, result.Triggers);
    }

    [Theory]
    [InlineData(1, true)]
    [InlineData(2, false)]
    [InlineData(99, false)]
    public void TryGetStat_FindsKeptStatsOnly(int statId, bool expected)
    {
        HeadhunterResolvedConfig result = HeadhunterTestData.Resolve(
            HeadhunterTestData.Config(
                HeadhunterTestData.AllTriggers,
                HeadhunterTestData.Entry("FakeA"),
                HeadhunterTestData.Disabled("FakeB")
            )
        );

        Assert.Equal(expected, result.TryGetStat(statId, out _));
    }
}
