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
        Assert.True(result.TryGetRow(1, out int row));
        Assert.Equal("HH_FakeA", result.Stats[row].BuffName);
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

        bool found = result.TryGetRow(statId, out int row);

        Assert.Equal(expectedRow, found ? row : -1);
    }
}
