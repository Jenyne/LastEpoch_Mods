using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter;

namespace LastEpoch_Hud.Tests.Core.CustomItems.Headhunter;

public sealed class HeadhunterConfigParserTests
{
    private static readonly IReadOnlySet<string> _known = new HashSet<string>(
        StringComparer.Ordinal
    )
    {
        "FakeA",
        "FakeB",
    };

    public static TheoryData<string> UnusableTexts { get; } =
        new() { (string)null, "", "  ", "not json", "[]", "{" };

    [Fact]
    public void Parse_ReturnsDefaults_WhenObjectEmpty()
    {
        HeadhunterConfigParseResult result = Parse("{}");

        Assert.Empty(result.Problems);
        Assert.Equivalent(HeadhunterConfigDefaults.Config, result.Config, strict: true);
    }

    [Theory]
    [MemberData(nameof(UnusableTexts))]
    public void Parse_ReturnsDefaultsWithRootProblem_WhenTextUnusable(string json)
    {
        HeadhunterConfigParseResult result = Parse(json);

        Assert.Equal(new[] { "" }, Paths(result));
        Assert.Equivalent(HeadhunterConfigDefaults.Config, result.Config, strict: true);
    }

    [Fact]
    public void Parse_KeepsGivenFields_WhenFilePartial()
    {
        HeadhunterConfigParseResult result = Parse(
            "{\"durationSeconds\":30,\"triggers\":{\"boss\":false}}"
        );

        Assert.Empty(result.Problems);
        Assert.Equal(30f, result.Config.DurationSeconds);
        Assert.Equal(new HeadhunterTriggers(true, false, true, true), result.Config.Triggers);
        Assert.Equal(HeadhunterConfigDefaults.Mechanic, result.Config.Mechanic);
        Assert.Equivalent(HeadhunterConfigDefaults.Stats, result.Config.Stats, strict: true);
    }

    [Fact]
    public void Parse_IgnoresUnknownFields()
    {
        HeadhunterConfigParseResult result = Parse(
            "{\"extra\":1,\"triggers\":{\"other\":true},\"stats\":[{\"stat\":\"FakeA\",\"note\":\"x\"}]}"
        );

        Assert.Empty(result.Problems);
        Assert.Single(result.Config.Stats);
    }

    [Fact]
    public void Parse_Version_IsOne_WhenMissing()
    {
        Assert.Equal(1, Parse("{}").Config.Version);
    }

    [Theory]
    [InlineData("\"x\"")]
    [InlineData("1.5")]
    [InlineData("3000000000")]
    [InlineData("99999999999999999999")]
    public void Parse_Version_FallsBackToOneWithProblem_WhenInvalid(string value)
    {
        HeadhunterConfigParseResult result = Parse("{\"version\":" + value + "}");

        Assert.Equal(1, result.Config.Version);
        Assert.Equal(new[] { "version" }, Paths(result));
    }

    [Fact]
    public void Parse_Version_IsKeptWithProblem_WhenInRangeAndNotOne()
    {
        HeadhunterConfigParseResult result = Parse("{\"version\":2,\"durationSeconds\":30}");

        Assert.Equal(2, result.Config.Version);
        Assert.Equal(30f, result.Config.DurationSeconds);
        Assert.Equal(new[] { "version" }, Paths(result));
    }

    [Fact]
    public void Parse_Mechanic_IsKept_WhenNonBlankString()
    {
        HeadhunterConfigParseResult result = Parse("{\"mechanic\":\"fake_id\"}");

        Assert.Equal("fake_id", result.Config.Mechanic);
        Assert.Empty(result.Problems);
    }

    [Theory]
    [InlineData("\"\"")]
    [InlineData("\"  \"")]
    [InlineData("5")]
    public void Parse_Mechanic_FallsBackToDefaultWithProblem_WhenInvalid(string value)
    {
        HeadhunterConfigParseResult result = Parse("{\"mechanic\":" + value + "}");

        Assert.Equal(HeadhunterConfigDefaults.Mechanic, result.Config.Mechanic);
        Assert.Equal(new[] { "mechanic" }, Paths(result));
    }

    [Theory]
    [InlineData("\"abc\"")]
    [InlineData("0")]
    [InlineData("-5")]
    [InlineData("NaN")]
    [InlineData("1e300")]
    [InlineData("99999999999999999999")]
    public void Parse_Duration_FallsBackToDefaultWithProblem_WhenInvalid(string value)
    {
        HeadhunterConfigParseResult result = Parse("{\"durationSeconds\":" + value + "}");

        Assert.Equal(HeadhunterConfigDefaults.DurationSeconds, result.Config.DurationSeconds);
        Assert.Equal(new[] { "durationSeconds" }, Paths(result));
    }

    [Fact]
    public void Parse_Triggers_AllOnWithProblem_WhenNotObject()
    {
        HeadhunterConfigParseResult result = Parse("{\"triggers\":true}");

        Assert.Equal(HeadhunterConfigDefaults.Triggers, result.Config.Triggers);
        Assert.Equal(new[] { "triggers" }, Paths(result));
    }

    [Fact]
    public void Parse_Triggers_FieldOnWithProblem_WhenNotBool()
    {
        HeadhunterConfigParseResult result = Parse("{\"triggers\":{\"rare\":1,\"boss\":false}}");

        Assert.Equal(new HeadhunterTriggers(true, false, true, true), result.Config.Triggers);
        Assert.Equal(new[] { "triggers.rare" }, Paths(result));
    }

    [Fact]
    public void Parse_Stats_DefaultTableWithProblem_WhenNotArray()
    {
        HeadhunterConfigParseResult result = Parse("{\"stats\":{}}");

        Assert.Equivalent(HeadhunterConfigDefaults.Stats, result.Config.Stats, strict: true);
        Assert.Equal(new[] { "stats" }, Paths(result));
    }

    [Fact]
    public void Parse_Stats_EmptyTableWithoutProblem_WhenEmptyArray()
    {
        HeadhunterConfigParseResult result = ParseStats("[]");

        Assert.Empty(result.Config.Stats);
        Assert.Empty(result.Problems);
    }

    [Theory]
    [InlineData("1", "stats[0]")]
    [InlineData("{\"added\":1}", "stats[0]")]
    [InlineData("{\"stat\":5}", "stats[0].stat")]
    [InlineData("{\"stat\":\"Nope\"}", "stats[0].stat")]
    [InlineData("{\"stat\":\"FakeA\",\"added\":\"x\"}", "stats[0].added")]
    [InlineData("{\"stat\":\"FakeA\",\"increased\":true}", "stats[0].increased")]
    [InlineData("{\"stat\":\"FakeA\",\"enabled\":\"yes\"}", "stats[0].enabled")]
    [InlineData("{\"stat\":\"FakeA\",\"added\":1e300}", "stats[0].added")]
    [InlineData("{\"stat\":\"FakeA\",\"increased\":NaN}", "stats[0].increased")]
    [InlineData("{\"stat\":\"FakeA\",\"added\":99999999999999999999}", "stats[0].added")]
    [InlineData("{\"stat\":\"FakeA\",\"added\":\"x\",\"enabled\":\"yes\"}", "stats[0].added")]
    public void Parse_Stats_SkipsBadEntryWithProblem(string entry, string path)
    {
        HeadhunterConfigParseResult result = ParseStats("[" + entry + "]");

        Assert.Empty(result.Config.Stats);
        Assert.Equal(new[] { path }, Paths(result));
    }

    [Fact]
    public void Parse_Stats_SkipsUnknownStatAndKeepsOrder()
    {
        HeadhunterConfigParseResult result = ParseStats(
            "[{\"stat\":\"FakeA\"},{\"stat\":\"Nope\"},{\"stat\":\"FakeB\"}]"
        );

        Assert.Equal(new[] { "FakeA", "FakeB" }, result.Config.Stats.Select(entry => entry.Stat));
        Assert.Equal(new[] { "stats[1].stat" }, Paths(result));
    }

    [Fact]
    public void Parse_Stats_SkipsDuplicateStat_KeepsFirst()
    {
        HeadhunterConfigParseResult result = ParseStats(
            "[{\"stat\":\"FakeA\",\"added\":1},{\"stat\":\"FakeA\",\"added\":2}]"
        );

        Assert.Equal(
            new HeadhunterStatEntry("FakeA", 1f, 0f, true),
            Assert.Single(result.Config.Stats)
        );
        Assert.Equal(new[] { "stats[1].stat" }, Paths(result));
    }

    [Fact]
    public void Parse_Stats_SkipsDuplicateStat_WhenFirstDisabled()
    {
        HeadhunterConfigParseResult result = ParseStats(
            "[{\"stat\":\"FakeA\",\"added\":1,\"enabled\":false},{\"stat\":\"FakeA\",\"added\":2}]"
        );

        Assert.Equal(
            new HeadhunterStatEntry("FakeA", 1f, 0f, false),
            Assert.Single(result.Config.Stats)
        );
        Assert.Equal(new[] { "stats[1].stat" }, Paths(result));
    }

    [Fact]
    public void Parse_Stats_KeepsLaterDuplicate_WhenEarlierSkipped()
    {
        HeadhunterConfigParseResult result = ParseStats(
            "[{\"stat\":\"FakeA\",\"added\":\"x\"},{\"stat\":\"FakeA\",\"added\":2}]"
        );

        Assert.Equal(
            new HeadhunterStatEntry("FakeA", 2f, 0f, true),
            Assert.Single(result.Config.Stats)
        );
        Assert.Equal(new[] { "stats[0].added" }, Paths(result));
    }

    [Fact]
    public void Parse_Stats_DefaultsEntryFields_WhenMissing()
    {
        HeadhunterConfigParseResult result = ParseStats("[{\"stat\":\"FakeA\"}]");

        Assert.Equal(
            new HeadhunterStatEntry("FakeA", 0f, 0f, true),
            Assert.Single(result.Config.Stats)
        );
        Assert.Empty(result.Problems);
    }

    [Fact]
    public void Parse_Stats_KeepsDisabledEntry()
    {
        HeadhunterConfigParseResult result = ParseStats("[{\"stat\":\"FakeA\",\"enabled\":false}]");

        Assert.False(Assert.Single(result.Config.Stats).Enabled);
        Assert.Empty(result.Problems);
    }

    [Fact]
    public void Parse_ReportsEveryProblemInFileOrder()
    {
        HeadhunterConfigParseResult result = Parse(
            "{\"mechanic\":5,\"durationSeconds\":0,\"triggers\":{\"rare\":1},\"stats\":[{\"stat\":\"Nope\"},{\"stat\":\"FakeA\",\"enabled\":\"yes\"},{\"stat\":\"FakeB\"}]}"
        );

        Assert.Equal(
            new[]
            {
                "mechanic",
                "durationSeconds",
                "triggers.rare",
                "stats[0].stat",
                "stats[1].enabled",
            },
            Paths(result)
        );
        Assert.Equal("FakeB", Assert.Single(result.Config.Stats).Stat);
    }

    [Theory]
    [MemberData(nameof(UnusableTexts))]
    public void Parse_UnusableText_NotReadable(string json)
    {
        Assert.False(Parse(json).IsReadable);
    }

    [Fact]
    public void Parse_ObjectWithBadEntries_Readable()
    {
        HeadhunterConfigParseResult result = Parse(
            "{\"durationSeconds\":-1,\"stats\":[{\"stat\":\"NotAStat\"}]}"
        );

        Assert.NotEmpty(result.Problems);
        Assert.True(result.IsReadable);
    }

    [Fact]
    public void Parse_Object_Readable()
    {
        Assert.True(Parse("{}").IsReadable);
    }

    private static HeadhunterConfigParseResult Parse(string json)
    {
        return HeadhunterConfigParser.Parse(json, _known);
    }

    private static HeadhunterConfigParseResult ParseStats(string statsJson)
    {
        return Parse("{\"stats\":" + statsJson + "}");
    }

    private static string[] Paths(HeadhunterConfigParseResult result)
    {
        return result.Problems.Select(problem => problem.Path).ToArray();
    }
}
