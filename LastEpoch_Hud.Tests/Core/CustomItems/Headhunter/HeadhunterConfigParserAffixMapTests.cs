using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter;

namespace LastEpoch_Hud.Tests.Core.CustomItems.Headhunter;

public sealed class HeadhunterConfigParserAffixMapTests
{
    private static readonly IReadOnlySet<string> _known = new HashSet<string>(
        StringComparer.Ordinal
    )
    {
        "FakeA",
    };

    [Fact]
    public void Parse_AffixMap_ReadsEntries()
    {
        HeadhunterConfigParseResult result = ParseMap(
            """[{"modKey":100,"note":"n","rows":["FakeA","FakeB_FakeTag"]},{"modKey":-200,"rows":[]}]"""
        );

        Assert.Empty(result.Problems);
        Assert.Equal(2, result.Config.AffixMap.Count);
        Assert.Equal(100, result.Config.AffixMap[0].ModKey);
        Assert.Equal("n", result.Config.AffixMap[0].Note);
        Assert.Equal(new[] { "FakeA", "FakeB_FakeTag" }, result.Config.AffixMap[0].Rows);
        Assert.Equal(-200, result.Config.AffixMap[1].ModKey);
    }

    [Fact]
    public void Parse_AffixMap_Missing_UsesDefaults()
    {
        HeadhunterConfigParseResult result = Parse("{}");

        Assert.Empty(result.Problems);
        Assert.Same(HeadhunterAffixDefaults.AffixMap, result.Config.AffixMap);
    }

    [Fact]
    public void Parse_AffixMap_NotList_ProblemAndDefaults()
    {
        HeadhunterConfigParseResult result = ParseMap("5");

        Assert.Equal(new[] { "affixMap" }, Paths(result));
        Assert.Same(HeadhunterAffixDefaults.AffixMap, result.Config.AffixMap);
    }

    [Fact]
    public void Parse_AffixMap_EntryNotObject_Skipped()
    {
        HeadhunterConfigParseResult result = ParseMap("""[5,{"modKey":1,"rows":[]}]""");

        Assert.Equal(new[] { "affixMap[0]" }, Paths(result));
        Assert.Equal(1, Assert.Single(result.Config.AffixMap).ModKey);
    }

    [Theory]
    [InlineData("""{"rows":[]}""")]
    [InlineData("""{"modKey":"x","rows":[]}""")]
    [InlineData("""{"modKey":1.5,"rows":[]}""")]
    [InlineData("""{"modKey":3000000000,"rows":[]}""")]
    public void Parse_AffixMap_InvalidModKey_Skipped(string entry)
    {
        HeadhunterConfigParseResult result = ParseMap("[" + entry + "]");

        Assert.Equal(new[] { "affixMap[0].modKey" }, Paths(result));
        Assert.Empty(result.Config.AffixMap);
    }

    [Theory]
    [InlineData("""{"modKey":1}""")]
    [InlineData("""{"modKey":1,"rows":"x"}""")]
    [InlineData("""{"modKey":1,"rows":null}""")]
    [InlineData("""{"modKey":1,"rows":{}}""")]
    public void Parse_AffixMap_RowsMissingOrNotList_Skipped(string entry)
    {
        HeadhunterConfigParseResult result = ParseMap("[" + entry + "]");

        Assert.Equal(new[] { "affixMap[0].rows" }, Paths(result));
        Assert.Empty(result.Config.AffixMap);
    }

    [Theory]
    [InlineData("5")]
    [InlineData("\"\"")]
    [InlineData("null")]
    public void Parse_AffixMap_BadRowItem_SkipsItemOnly(string item)
    {
        HeadhunterConfigParseResult result = ParseMap(
            "[{\"modKey\":1,\"rows\":[\"FakeA\"," + item + ",\"FakeB\"]}]"
        );

        Assert.Equal(new[] { "affixMap[0].rows[1]" }, Paths(result));
        Assert.Equal(new[] { "FakeA", "FakeB" }, Assert.Single(result.Config.AffixMap).Rows);
    }

    [Fact]
    public void Parse_AffixMap_DuplicateModKey_KeepsFirst()
    {
        HeadhunterConfigParseResult result = ParseMap(
            """[{"modKey":1,"rows":["FakeA"]},{"modKey":1,"rows":["FakeB"]}]"""
        );

        Assert.Equal(new[] { "affixMap[1].modKey" }, Paths(result));
        Assert.Equal(new[] { "FakeA" }, Assert.Single(result.Config.AffixMap).Rows);
    }

    [Fact]
    public void Parse_AffixMap_EmptyRows_Kept()
    {
        HeadhunterConfigParseResult result = ParseMap("""[{"modKey":1,"rows":[]}]""");

        Assert.Empty(result.Problems);
        Assert.Empty(Assert.Single(result.Config.AffixMap).Rows);
    }

    [Fact]
    public void Parse_AffixMap_EmptyList_GivesEmptyMap()
    {
        HeadhunterConfigParseResult result = ParseMap("[]");

        Assert.Empty(result.Problems);
        Assert.Empty(result.Config.AffixMap);
    }

    [Theory]
    [InlineData("""{"modKey":1,"rows":[]}""")]
    [InlineData("""{"modKey":1,"note":5,"rows":[]}""")]
    public void Parse_AffixMap_NoteMissingOrNotText_IsNull(string entry)
    {
        HeadhunterConfigParseResult result = ParseMap("[" + entry + "]");

        Assert.Empty(result.Problems);
        Assert.Null(Assert.Single(result.Config.AffixMap).Note);
    }

    [Fact]
    public void Parse_AffixMap_ProblemPathUsesFileIndex()
    {
        HeadhunterConfigParseResult result = ParseMap(
            """[5,{"modKey":1,"rows":[]},{"modKey":1,"rows":[]}]"""
        );

        Assert.Equal(new[] { "affixMap[0]", "affixMap[2].modKey" }, Paths(result));
    }

    private static HeadhunterConfigParseResult Parse(string json)
    {
        return HeadhunterConfigParser.Parse(json, _known);
    }

    private static HeadhunterConfigParseResult ParseMap(string mapJson)
    {
        return Parse("{\"affixMap\":" + mapJson + "}");
    }

    private static string[] Paths(HeadhunterConfigParseResult result)
    {
        return result.Problems.Select(problem => problem.Path).ToArray();
    }
}
