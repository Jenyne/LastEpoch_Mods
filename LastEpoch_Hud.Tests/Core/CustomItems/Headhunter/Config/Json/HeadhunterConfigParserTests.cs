using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Config;
using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Config.Defaults;
using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Config.Json;
using Code = LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Config.HeadhunterConfigProblemCode;

namespace LastEpoch_Hud.Tests.Core.CustomItems.Headhunter.Config.Json;

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

    public static TheoryData<string, Code> UnusableTextProblems { get; } =
        new()
        {
            { (string)null, Code.EmptyFile },
            { "", Code.EmptyFile },
            { "  ", Code.EmptyFile },
            { "not json", Code.InvalidJson },
            { "{", Code.InvalidJson },
            { "[]", Code.RootNotObject },
        };

    [Fact]
    public void Parse_ReturnsDefaults_WhenObjectEmpty()
    {
        HeadhunterConfigParseResult result = Parse("{}");

        Assert.Empty(result.Problems);
        Assert.Equivalent(HeadhunterConfigDefaults.Config, result.Config, strict: true);
    }

    [Theory]
    [MemberData(nameof(UnusableTextProblems))]
    public void Parse_ReturnsDefaultsWithRootProblem_WhenTextUnusable(string json, Code code)
    {
        HeadhunterConfigParseResult result = Parse(json);

        Assert.Equal(new[] { (code, "") }, Problems(result));
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
        Assert.Equal(new HeadhunterTriggers(true, false, true, true, true), result.Config.Triggers);
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
        Assert.Equal(new[] { (Code.NotWholeNumber, "version") }, Problems(result));
    }

    [Fact]
    public void Parse_Version_IsKeptWithProblem_WhenInRangeAndNotOne()
    {
        HeadhunterConfigParseResult result = Parse("{\"version\":2,\"durationSeconds\":30}");

        Assert.Equal(2, result.Config.Version);
        Assert.Equal(30f, result.Config.DurationSeconds);
        Assert.Equal(new[] { (Code.UnsupportedVersion, "version") }, Problems(result));
    }

    [Theory]
    [InlineData("\"rare_mods\"")]
    [InlineData("\"fake_id\"")]
    [InlineData("\"\"")]
    [InlineData("5")]
    public void Parse_IgnoresLegacyMechanicKey(string value)
    {
        HeadhunterConfigParseResult result = Parse(
            "{\"mechanic\":" + value + ",\"durationSeconds\":30}"
        );

        Assert.Empty(result.Problems);
        Assert.Equivalent(Parse("{\"durationSeconds\":30}").Config, result.Config, strict: true);
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
        Assert.Equal(new[] { (Code.NotPositiveNumber, "durationSeconds") }, Problems(result));
    }

    [Fact]
    public void MaxStacks_Missing_Defaults10()
    {
        HeadhunterConfigParseResult result = Parse("{}");

        Assert.Equal(10, result.Config.MaxStacks);
        Assert.Empty(result.Problems);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-1")]
    [InlineData("2.5")]
    [InlineData("\"x\"")]
    [InlineData("true")]
    public void MaxStacks_Invalid_DefaultsWithProblem(string value)
    {
        HeadhunterConfigParseResult result = Parse("{\"maxStacks\":" + value + "}");

        Assert.Equal(10, result.Config.MaxStacks);
        Assert.Equal(new[] { (Code.NotPositiveWholeNumber, "maxStacks") }, Problems(result));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(7)]
    public void MaxStacks_ReadsValue(int value)
    {
        HeadhunterConfigParseResult result = Parse("{\"maxStacks\":" + value + "}");

        Assert.Equal(value, result.Config.MaxStacks);
        Assert.Empty(result.Problems);
    }

    [Fact]
    public void Parse_Triggers_AllOnWithProblem_WhenNotObject()
    {
        HeadhunterConfigParseResult result = Parse("{\"triggers\":true}");

        Assert.Equal(HeadhunterConfigDefaults.Triggers, result.Config.Triggers);
        Assert.Equal(new[] { (Code.NotObject, "triggers") }, Problems(result));
    }

    [Fact]
    public void Parse_Triggers_FieldOnWithProblem_WhenNotBool()
    {
        HeadhunterConfigParseResult result = Parse("{\"triggers\":{\"rare\":1,\"boss\":false}}");

        Assert.Equal(new HeadhunterTriggers(true, false, true, true, true), result.Config.Triggers);
        Assert.Equal(new[] { (Code.NotBool, "triggers.rare") }, Problems(result));
    }

    [Fact]
    public void Parse_MagicTrigger_DefaultsOn_WhenMissing()
    {
        HeadhunterConfigParseResult result = Parse("{\"triggers\":{}}");

        Assert.True(result.Config.Triggers.Magic);
        Assert.Empty(result.Problems);
    }

    [Fact]
    public void Parse_MagicTrigger_ReadsFalse()
    {
        HeadhunterConfigParseResult result = Parse("{\"triggers\":{\"magic\":false}}");

        Assert.False(result.Config.Triggers.Magic);
        Assert.Empty(result.Problems);
    }

    [Fact]
    public void Parse_MagicTrigger_Invalid_ReportsProblem()
    {
        HeadhunterConfigParseResult result = Parse("{\"triggers\":{\"magic\":5}}");

        Assert.True(result.Config.Triggers.Magic);
        Assert.Equal(new[] { (Code.NotBool, "triggers.magic") }, Problems(result));
    }

    [Fact]
    public void Parse_Stats_DefaultTableWithProblem_WhenNotArray()
    {
        HeadhunterConfigParseResult result = Parse("{\"stats\":{}}");

        Assert.Equivalent(HeadhunterConfigDefaults.Stats, result.Config.Stats, strict: true);
        Assert.Equal(new[] { (Code.NotList, "stats") }, Problems(result));
    }

    [Fact]
    public void Parse_Stats_EmptyTableWithoutProblem_WhenEmptyArray()
    {
        HeadhunterConfigParseResult result = ParseStats("[]");

        Assert.Empty(result.Config.Stats);
        Assert.Empty(result.Problems);
    }

    [Theory]
    [InlineData("1", "stats[0]", Code.NotObject)]
    [InlineData("{\"added\":1}", "stats[0]", Code.MissingStat)]
    [InlineData("{\"stat\":5}", "stats[0].stat", Code.UnknownStat)]
    [InlineData("{\"stat\":\"Nope\"}", "stats[0].stat", Code.UnknownStat)]
    [InlineData("{\"stat\":\"FakeA\",\"added\":\"x\"}", "stats[0].added", Code.NotFiniteNumber)]
    [InlineData(
        "{\"stat\":\"FakeA\",\"increased\":true}",
        "stats[0].increased",
        Code.NotFiniteNumber
    )]
    [InlineData("{\"stat\":\"FakeA\",\"enabled\":\"yes\"}", "stats[0].enabled", Code.NotBool)]
    [InlineData("{\"stat\":\"FakeA\",\"added\":1e300}", "stats[0].added", Code.NotFiniteNumber)]
    [InlineData(
        "{\"stat\":\"FakeA\",\"increased\":NaN}",
        "stats[0].increased",
        Code.NotFiniteNumber
    )]
    [InlineData(
        "{\"stat\":\"FakeA\",\"added\":99999999999999999999}",
        "stats[0].added",
        Code.NotFiniteNumber
    )]
    [InlineData(
        "{\"stat\":\"FakeA\",\"added\":\"x\",\"enabled\":\"yes\"}",
        "stats[0].added",
        Code.NotFiniteNumber
    )]
    public void Parse_Stats_SkipsBadEntryWithProblem(string entry, string path, Code code)
    {
        HeadhunterConfigParseResult result = ParseStats("[" + entry + "]");

        Assert.Empty(result.Config.Stats);
        Assert.Equal(new[] { (code, path) }, Problems(result));
    }

    [Fact]
    public void Parse_Stats_SkipsUnknownStatAndKeepsOrder()
    {
        HeadhunterConfigParseResult result = ParseStats(
            "[{\"stat\":\"FakeA\"},{\"stat\":\"Nope\"},{\"stat\":\"FakeB\"}]"
        );

        Assert.Equal(new[] { "FakeA", "FakeB" }, result.Config.Stats.Select(entry => entry.Stat));
        Assert.Equal(new[] { (Code.UnknownStat, "stats[1].stat") }, Problems(result));
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
        Assert.Equal(new[] { (Code.DuplicateStat, "stats[1].stat") }, Problems(result));
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
        Assert.Equal(new[] { (Code.DuplicateStat, "stats[1].stat") }, Problems(result));
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
        Assert.Equal(new[] { (Code.NotFiniteNumber, "stats[0].added") }, Problems(result));
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
            "{\"durationSeconds\":0,\"triggers\":{\"rare\":1},\"stats\":[{\"stat\":\"Nope\"},{\"stat\":\"FakeA\",\"enabled\":\"yes\"},{\"stat\":\"FakeB\"}]}"
        );

        Assert.Equal(
            new[]
            {
                (Code.NotPositiveNumber, "durationSeconds"),
                (Code.NotBool, "triggers.rare"),
                (Code.UnknownStat, "stats[0].stat"),
                (Code.NotBool, "stats[1].enabled"),
            },
            Problems(result)
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

    [Fact]
    public void Parse_Stats_ReadsTag()
    {
        HeadhunterConfigParseResult result = ParseStats(
            "[{\"stat\":\"FakeA\",\"tag\":\"FakeTag\"}]"
        );

        Assert.Equal("FakeTag", Assert.Single(result.Config.Stats).Tag);
    }

    [Theory]
    [InlineData("[{\"stat\":\"FakeA\"}]")]
    [InlineData("[{\"stat\":\"FakeA\",\"tag\":null}]")]
    public void Parse_Stats_MissingOrNullTag_IsNull(string statsJson)
    {
        HeadhunterConfigParseResult result = ParseStats(statsJson);

        Assert.Empty(result.Problems);
        Assert.Null(Assert.Single(result.Config.Stats).Tag);
    }

    [Theory]
    [InlineData("5")]
    [InlineData("\"\"")]
    public void Parse_Stats_InvalidTag_SkipsRowWithProblem(string tagJson)
    {
        HeadhunterConfigParseResult result = ParseStats(
            "[{\"stat\":\"FakeA\",\"tag\":" + tagJson + "}]"
        );

        Assert.Empty(result.Config.Stats);
        Assert.Equal(new[] { (Code.EmptyOrNotText, "stats[0].tag") }, Problems(result));
    }

    [Fact]
    public void Parse_Stats_SameStatDifferentTags_KeepsBoth()
    {
        HeadhunterConfigParseResult result = ParseStats(
            "[{\"stat\":\"FakeA\"},{\"stat\":\"FakeA\",\"tag\":\"FakeTag\"},{\"stat\":\"FakeA\",\"tag\":\"OtherTag\"}]"
        );

        Assert.Empty(result.Problems);
        Assert.Equal(
            new string[] { null, "FakeTag", "OtherTag" },
            result.Config.Stats.Select(entry => entry.Tag)
        );
    }

    [Fact]
    public void Parse_Stats_DuplicateStatAndTag_KeepsFirst()
    {
        HeadhunterConfigParseResult result = ParseStats(
            "[{\"stat\":\"FakeA\",\"tag\":\"FakeTag\",\"added\":1},{\"stat\":\"FakeA\",\"tag\":\"FakeTag\",\"added\":2}]"
        );

        Assert.Equal(
            new HeadhunterStatEntry("FakeA", 1f, 0f, true, "FakeTag"),
            Assert.Single(result.Config.Stats)
        );
        Assert.Equal(new[] { (Code.DuplicateStat, "stats[1].stat") }, Problems(result));
    }

    private static HeadhunterConfigParseResult Parse(string json)
    {
        return HeadhunterConfigParser.Parse(json, _known);
    }

    private static HeadhunterConfigParseResult ParseStats(string statsJson)
    {
        return Parse("{\"stats\":" + statsJson + "}");
    }

    private static (Code Code, string Path)[] Problems(HeadhunterConfigParseResult result)
    {
        return HeadhunterTestData.Problems(result.Problems);
    }
}
