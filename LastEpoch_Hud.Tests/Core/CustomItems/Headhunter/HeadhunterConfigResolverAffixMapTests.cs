using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter;

namespace LastEpoch_Hud.Tests.Core.CustomItems.Headhunter;

public sealed class HeadhunterConfigResolverAffixMapTests
{
    [Fact]
    public void Resolve_AffixMap_MapsRowTexts()
    {
        var problems = new List<HeadhunterConfigProblem>();

        HeadhunterResolvedConfig result = Resolve(
            problems,
            [HeadhunterTestData.Affix(100, "FakeB", "FakeA_FakeTag")],
            ThreeStats()
        );

        Assert.Empty(problems);
        Assert.Equal(1, result.AffixCount);
        Assert.True(result.TryGetAffixRows(100, out IReadOnlyList<int> rows));
        Assert.Equal(new[] { 2, 1 }, rows);
    }

    [Fact]
    public void Resolve_AffixMap_UnknownRow_ProblemAndSkipped()
    {
        var problems = new List<HeadhunterConfigProblem>();

        HeadhunterResolvedConfig result = Resolve(
            problems,
            [HeadhunterTestData.Affix(100, "Nope", "FakeA")],
            ThreeStats()
        );

        Assert.Equal(new[] { "affixMap[0].rows[0]" }, problems.Select(problem => problem.Path));
        Assert.True(result.TryGetAffixRows(100, out IReadOnlyList<int> rows));
        Assert.Equal(new[] { 0 }, rows);
    }

    [Fact]
    public void Resolve_AffixMap_UnknownRow_MessageNamesKeyAndText()
    {
        var problems = new List<HeadhunterConfigProblem>();

        Resolve(problems, [HeadhunterTestData.Affix(100, "Nope")], ThreeStats());

        string message = Assert.Single(problems).Message;
        Assert.Contains("100", message);
        Assert.Contains("Nope", message);
    }

    [Fact]
    public void Resolve_AffixMap_UnknownRow_PathUsesEntryPosition()
    {
        var problems = new List<HeadhunterConfigProblem>();

        Resolve(
            problems,
            [
                HeadhunterTestData.Affix(100, "FakeA"),
                HeadhunterTestData.Affix(200, "FakeA", "Nope"),
            ],
            ThreeStats()
        );

        Assert.Equal(new[] { "affixMap[1].rows[1]" }, problems.Select(problem => problem.Path));
    }

    [Fact]
    public void Resolve_AffixMap_DisabledRow_SkippedNoProblem()
    {
        var problems = new List<HeadhunterConfigProblem>();

        HeadhunterResolvedConfig result = Resolve(
            problems,
            [HeadhunterTestData.Affix(100, "FakeB", "FakeA")],
            HeadhunterTestData.Entry("FakeA"),
            HeadhunterTestData.Disabled("FakeB")
        );

        Assert.Empty(problems);
        Assert.True(result.TryGetAffixRows(100, out IReadOnlyList<int> rows));
        Assert.Equal(new[] { 0 }, rows);
    }

    [Fact]
    public void Resolve_AffixMap_UnresolvedStatRow_NoExtraProblem()
    {
        var problems = new List<HeadhunterConfigProblem>();

        HeadhunterResolvedConfig result = Resolve(
            problems,
            [HeadhunterTestData.Affix(100, "FakeZ", "FakeA")],
            HeadhunterTestData.Entry("FakeA"),
            HeadhunterTestData.Entry("FakeZ")
        );

        Assert.Equal(new[] { "stats[1].stat" }, problems.Select(problem => problem.Path));
        Assert.True(result.TryGetAffixRows(100, out IReadOnlyList<int> rows));
        Assert.Equal(new[] { 0 }, rows);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Resolve_AffixMap_SkippedRowBefore_UsesResolvedIndex(bool disabled)
    {
        HeadhunterStatEntry skipped = disabled
            ? HeadhunterTestData.Disabled("FakeB")
            : HeadhunterTestData.Entry("FakeZ");

        HeadhunterResolvedConfig result = Resolve(
            new List<HeadhunterConfigProblem>(),
            [HeadhunterTestData.Affix(100, "FakeC", "FakeA")],
            skipped,
            HeadhunterTestData.Entry("FakeA"),
            HeadhunterTestData.Entry("FakeC")
        );

        Assert.True(result.TryGetAffixRows(100, out IReadOnlyList<int> rows));
        Assert.Equal(new[] { 1, 0 }, rows);
    }

    [Fact]
    public void Resolve_AffixMap_UnmappedKey_False()
    {
        HeadhunterResolvedConfig result = Resolve(
            new List<HeadhunterConfigProblem>(),
            [HeadhunterTestData.Affix(100, "FakeA")],
            ThreeStats()
        );

        Assert.False(result.TryGetAffixRows(200, out _));
    }

    [Fact]
    public void Resolve_AffixMap_EmptyRows_MappedToNothing()
    {
        HeadhunterResolvedConfig result = Resolve(
            new List<HeadhunterConfigProblem>(),
            [HeadhunterTestData.Affix(100)],
            ThreeStats()
        );

        Assert.True(result.TryGetAffixRows(100, out IReadOnlyList<int> rows));
        Assert.Empty(rows);
        Assert.Equal(1, result.AffixCount);
    }

    [Fact]
    public void Resolve_NoAffixMap_AffixCountZero()
    {
        HeadhunterResolvedConfig result = HeadhunterTestData.Resolve(
            HeadhunterTestData.Config(HeadhunterTestData.AllTriggers, ThreeStats())
        );

        Assert.Equal(0, result.AffixCount);
        Assert.False(result.TryGetAffixRows(100, out _));
    }

    private static HeadhunterStatEntry[] ThreeStats()
    {
        return
        [
            HeadhunterTestData.Entry("FakeA"),
            HeadhunterTestData.Tagged("FakeA", "FakeTag"),
            HeadhunterTestData.Entry("FakeB"),
        ];
    }

    private static HeadhunterResolvedConfig Resolve(
        List<HeadhunterConfigProblem> problems,
        HeadhunterAffixEntry[] map,
        params HeadhunterStatEntry[] stats
    )
    {
        return HeadhunterConfigResolver.Resolve(
            HeadhunterTestData.ConfigWithMap(map, stats),
            HeadhunterTestData.StatIds,
            HeadhunterTestData.TagIds,
            problems
        );
    }
}
