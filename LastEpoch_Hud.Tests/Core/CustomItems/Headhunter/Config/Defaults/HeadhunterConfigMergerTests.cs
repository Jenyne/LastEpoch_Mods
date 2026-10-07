using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Config;
using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Config.Defaults;
using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Config.Json;
using Newtonsoft.Json.Linq;

namespace LastEpoch_Hud.Tests.Core.CustomItems.Headhunter.Config.Defaults;

public sealed class HeadhunterConfigMergerTests
{
    private static readonly HeadhunterMergeDefaults _defaults = new()
    {
        Version = 3,
        Stats = new List<HeadhunterVersionedStat>
        {
            new(new HeadhunterStatEntry("FakeA", 1f, 2f, true), 1),
            new(new HeadhunterStatEntry("FakeB", 3f, 4f, true), 2),
            new(new HeadhunterStatEntry("FakeC", 5f, 6f, true), 3),
        },
        Fields = new List<HeadhunterVersionedField>
        {
            new("triggers", "fakeFlag", true, 3),
            new("", "fakeSection", new JObject { ["x"] = 1 }, 3),
        },
    };

    private static readonly HashSet<string> _fakeNames = new(StringComparer.Ordinal)
    {
        "FakeA",
        "FakeB",
        "FakeC",
    };

    [Fact]
    public void Merge_Unstamped_CountsAsVersion1()
    {
        HeadhunterMergeResult result = Merge("""{"stats":[]}""");

        var root = JObject.Parse(result.Text);
        Assert.Equal(new[] { "FakeB", "FakeC" }, StatNames(result));
        Assert.Equal(1, (int)root["fakeSection"]["x"]);
        Assert.Equal(3, (int)root["defaultsVersion"]);
        Assert.Equal(3, result.Added);
    }

    [Fact]
    public void Merge_InvalidStamp_CountsAsVersion1()
    {
        HeadhunterMergeResult result = Merge("""{"defaultsVersion":"x","stats":[]}""");

        Assert.Equal(new[] { "FakeB", "FakeC" }, StatNames(result));
        Assert.Equal(3, result.Added);
    }

    [Fact]
    public void Merge_NewerRows_AppendedInDefaultsOrder()
    {
        HeadhunterMergeResult result = Merge(
            """{"defaultsVersion":1,"stats":[{"stat":"FakeA"}]}"""
        );

        JToken added = JObject.Parse(result.Text)["stats"][1];
        Assert.Equal(new[] { "FakeA", "FakeB", "FakeC" }, StatNames(result));
        Assert.Equal(3f, (float)added["added"]);
        Assert.Equal(4f, (float)added["increased"]);
        Assert.True((bool)added["enabled"]);
    }

    [Fact]
    public void Merge_RowDeletedAtOldVersion_NotReAdded()
    {
        HeadhunterMergeResult result = Merge("""{"defaultsVersion":2,"stats":[]}""");

        Assert.Equal(new[] { "FakeC" }, StatNames(result));
    }

    [Fact]
    public void Merge_ExistingDisabledRow_StaysDisabled()
    {
        HeadhunterMergeResult result = Merge(
            """{"defaultsVersion":1,"stats":[{"stat":"FakeB","enabled":false,"added":7.5}]}"""
        );

        JToken row = JObject.Parse(result.Text)["stats"][0];
        Assert.Equal(1, StatNames(result).Count(name => name == "FakeB"));
        Assert.False((bool)row["enabled"]);
        Assert.Equal(7.5f, (float)row["added"]);
    }

    [Fact]
    public void Merge_NewerField_AddedWhenParentExists()
    {
        HeadhunterMergeResult result = Merge("""{"defaultsVersion":2,"triggers":{},"stats":[]}""");

        var root = JObject.Parse(result.Text);
        Assert.True((bool)root["triggers"]["fakeFlag"]);
        Assert.Equal(1, (int)root["fakeSection"]["x"]);
    }

    [Fact]
    public void Merge_ExistingField_KeepsPlayerValue()
    {
        HeadhunterMergeResult result = Merge(
            """{"defaultsVersion":2,"triggers":{"fakeFlag":false},"stats":[]}"""
        );

        Assert.False((bool)JObject.Parse(result.Text)["triggers"]["fakeFlag"]);
    }

    [Fact]
    public void Merge_MissingParent_FieldSkipped()
    {
        HeadhunterMergeResult result = Merge("""{"defaultsVersion":2,"stats":[]}""");

        Assert.Null(JObject.Parse(result.Text)["triggers"]);
        Assert.True(result.Changed);
        Assert.Equal(2, result.Added);
        Assert.Equal(_defaults.Version, (int)JObject.Parse(result.Text)["defaultsVersion"]);
    }

    [Theory]
    [InlineData("5")]
    [InlineData("\"FakeC\"")]
    [InlineData("""{"stat":{}}""")]
    [InlineData("""{"stat":["FakeC"]}""")]
    [InlineData("""{"stat":null}""")]
    [InlineData("{}")]
    public void Merge_MalformedStatRows_SkippedAndMerged(string row)
    {
        HeadhunterMergeResult result = Merge("{\"defaultsVersion\":2,\"stats\":[" + row + "]}");

        JToken first = JObject.Parse(result.Text)["stats"][0];
        Assert.True(result.Changed);
        Assert.True(JToken.DeepEquals(JToken.Parse(row), first));
        Assert.Equal(1, StatNamesOrNull(result).Count(name => name == "FakeC"));
        Assert.Equal(2, result.Added);
    }

    [Fact]
    public void Merge_StatMatch_IsCaseSensitive()
    {
        HeadhunterMergeResult result = Merge(
            """{"defaultsVersion":2,"stats":[{"stat":"fakec"}]}"""
        );

        Assert.Equal(new[] { "fakec", "FakeC" }, StatNames(result));
    }

    [Fact]
    public void Merge_ParentNotObject_FieldSkipped()
    {
        HeadhunterMergeResult result = Merge(
            """{"defaultsVersion":2,"triggers":5,"stats":[{"stat":"FakeC"}]}"""
        );

        var root = JObject.Parse(result.Text);
        Assert.Equal(5, (int)root["triggers"]);
        Assert.NotNull(root["fakeSection"]);
        Assert.Equal(1, result.Added);
    }

    [Fact]
    public void Merge_UnknownFieldsAndOrder_Kept()
    {
        const string input =
            """{"custom":"c","defaultsVersion":1,"durationSeconds":33,"stats":[{"stat":"FakeA","note":"n"}]}""";

        HeadhunterMergeResult result = Merge(input);

        var root = JObject.Parse(result.Text);
        IEnumerable<string> inputNames = JObject
            .Parse(input)
            .Properties()
            .Select(property => property.Name);
        IEnumerable<string> outputNames = root.Properties().Select(property => property.Name);
        Assert.Equal(inputNames, outputNames.Take(4));
        Assert.Equal("c", (string)root["custom"]);
        Assert.Equal(33, (int)root["durationSeconds"]);
        Assert.Equal("n", (string)root["stats"][0]["note"]);
    }

    [Fact]
    public void Merge_Stamped_SetsCurrentVersion()
    {
        HeadhunterMergeResult result = Merge("""{"defaultsVersion":1,"stats":[]}""");

        Assert.True(result.Changed);
        Assert.Equal(_defaults.Version, (int)JObject.Parse(result.Text)["defaultsVersion"]);
    }

    [Theory]
    [InlineData(3)]
    [InlineData(9)]
    public void Merge_UpToDate_Unchanged(int stamp)
    {
        string input = "{\"defaultsVersion\":" + stamp + ",\"stats\":[]}";

        HeadhunterMergeResult result = Merge(input);

        Assert.False(result.Changed);
        Assert.Same(input, result.Text);
        Assert.Equal(0, result.Added);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("{ broken")]
    [InlineData("[1]")]
    public void Merge_Unreadable_Unchanged(string input)
    {
        HeadhunterMergeResult result = Merge(input);

        Assert.False(result.Changed);
        Assert.Equal(input, result.Text);
    }

    [Fact]
    public void Merge_StatsNotList_Unchanged()
    {
        const string input = """{"defaultsVersion":1,"stats":5}""";

        HeadhunterMergeResult result = Merge(input);

        Assert.False(result.Changed);
        Assert.Equal(input, result.Text);
        Assert.Equal(0, result.Added);
    }

    [Fact]
    public void Merge_StatsMissing_FieldsAddedAndStamped()
    {
        HeadhunterMergeResult result = Merge("""{"defaultsVersion":2}""");

        var root = JObject.Parse(result.Text);
        Assert.Null(root["stats"]);
        Assert.NotNull(root["fakeSection"]);
        Assert.Equal(3, (int)root["defaultsVersion"]);
        Assert.Equal(1, result.Added);
    }

    [Fact]
    public void Merge_AddedRows_ParseBackWithoutProblems()
    {
        HeadhunterMergeResult result = Merge(
            """{"defaultsVersion":1,"stats":[{"stat":"FakeA"}]}"""
        );

        HeadhunterConfigParseResult parsed = HeadhunterConfigParser.Parse(result.Text, _fakeNames);

        Assert.Empty(parsed.Problems);
        Assert.Contains(new HeadhunterStatEntry("FakeC", 5f, 6f, true), parsed.Config.Stats);
    }

    [Fact]
    public void Merge_FreshDefaultsFile_Unchanged()
    {
        string text = HeadhunterConfigWriter.Write(HeadhunterConfigDefaults.Config);

        HeadhunterMergeResult result = HeadhunterConfigMerger.Merge(
            text,
            HeadhunterConfigDefaults.MergeDefaults
        );

        Assert.False(result.Changed);
    }

    [Fact]
    public void Merge_Version1File_GetsFullDefaultTable()
    {
        var config = new HeadhunterConfig
        {
            Version = HeadhunterConfigDefaults.CurrentVersion,
            DurationSeconds = HeadhunterConfigDefaults.DurationSeconds,
            Triggers = HeadhunterConfigDefaults.Triggers,
            Stats = HeadhunterConfigDefaults
                .VersionedStats.Where(row => row.Since == 1)
                .Select(row => row.Entry)
                .ToList(),
        };
        var file = JObject.Parse(HeadhunterConfigWriter.Write(config));
        file.Remove("defaultsVersion");
        var known = HeadhunterConfigDefaults
            .Stats.Select(entry => entry.Stat)
            .ToHashSet(StringComparer.Ordinal);

        HeadhunterMergeResult result = HeadhunterConfigMerger.Merge(
            file.ToString(),
            HeadhunterConfigDefaults.MergeDefaults
        );

        HeadhunterConfigParseResult parsed = HeadhunterConfigParser.Parse(result.Text, known);
        Assert.Empty(parsed.Problems);
        Assert.Equal(HeadhunterConfigDefaults.Stats, parsed.Config.Stats);
    }

    [Fact]
    public void Merge_Stamp2File_AddsMagicTrigger_KeepsPlayerTriggers()
    {
        var config = new HeadhunterConfig
        {
            Version = HeadhunterConfigDefaults.CurrentVersion,
            DurationSeconds = HeadhunterConfigDefaults.DurationSeconds,
            Triggers = new HeadhunterTriggers(false, true, true, true, true),
            Stats = HeadhunterConfigDefaults.Stats,
            AffixMap = HeadhunterAffixDefaults.AffixMap,
        };
        var file = JObject.Parse(HeadhunterConfigWriter.Write(config));
        ((JObject)file["triggers"]).Remove("magic");
        file["defaultsVersion"] = 2;
        var known = HeadhunterConfigDefaults
            .Stats.Select(entry => entry.Stat)
            .ToHashSet(StringComparer.Ordinal);

        HeadhunterMergeResult result = HeadhunterConfigMerger.Merge(
            file.ToString(),
            HeadhunterConfigDefaults.MergeDefaults
        );

        HeadhunterConfigParseResult parsed = HeadhunterConfigParser.Parse(result.Text, known);
        Assert.Empty(parsed.Problems);
        Assert.True(parsed.Config.Triggers.Magic);
        Assert.False(parsed.Config.Triggers.Rare);
        Assert.Equal(
            HeadhunterConfigDefaults.DefaultsVersion,
            (int)JObject.Parse(result.Text)["defaultsVersion"]
        );
        Assert.Equal(1, result.Added);
    }

    [Fact]
    public void Merge_Stamp3File_AddsMaxStacks()
    {
        var file = JObject.Parse(HeadhunterConfigWriter.Write(HeadhunterConfigDefaults.Config));
        file.Remove("maxStacks");
        file["defaultsVersion"] = 3;

        HeadhunterMergeResult result = HeadhunterConfigMerger.Merge(
            file.ToString(),
            HeadhunterConfigDefaults.MergeDefaults
        );

        var merged = JObject.Parse(result.Text);
        Assert.Equal(10, (int)merged["maxStacks"]);
        Assert.Equal(HeadhunterConfigDefaults.DefaultsVersion, (int)merged["defaultsVersion"]);
        Assert.Equal(1, result.Added);
    }

    [Fact]
    public void Merge_Stamp4File_AppendsVersion5Rows_KeepsPlayerRows()
    {
        var config = new HeadhunterConfig
        {
            Version = HeadhunterConfigDefaults.CurrentVersion,
            DurationSeconds = HeadhunterConfigDefaults.DurationSeconds,
            MaxStacks = HeadhunterConfigDefaults.MaxStacks,
            Triggers = HeadhunterConfigDefaults.Triggers,
            Stats = HeadhunterConfigDefaults
                .VersionedStats.Where(row => row.Since <= 4)
                .Select(row => row.Entry)
                .ToList(),
            AffixMap = HeadhunterAffixDefaults.AffixMap,
        };
        var file = JObject.Parse(HeadhunterConfigWriter.Write(config));
        file["stats"][0]["added"] = 123;
        file["defaultsVersion"] = 4;

        HeadhunterMergeResult result = HeadhunterConfigMerger.Merge(
            file.ToString(),
            HeadhunterConfigDefaults.MergeDefaults
        );

        var rows = (JArray)JObject.Parse(result.Text)["stats"];
        var keys = rows.Select(row => ((string)row["stat"], (string)row["tag"])).ToList();
        Assert.NotEqual(0, CountAfter4());
        Assert.Equal(CountAfter4(), result.Added);
        Assert.All(
            HeadhunterConfigDefaults.Stats,
            entry => Assert.Contains((entry.Stat, entry.Tag), keys)
        );
        Assert.Equal(123f, (float)rows[0]["added"]);
        Assert.Equal(
            HeadhunterConfigDefaults.DefaultsVersion,
            (int)JObject.Parse(result.Text)["defaultsVersion"]
        );
    }

    [Fact]
    public void Merge_Stamp3File_KeepsPlayerMaxStacks()
    {
        var file = JObject.Parse(HeadhunterConfigWriter.Write(HeadhunterConfigDefaults.Config));
        file["maxStacks"] = 5;
        file["defaultsVersion"] = 3;

        HeadhunterMergeResult result = HeadhunterConfigMerger.Merge(
            file.ToString(),
            HeadhunterConfigDefaults.MergeDefaults
        );

        Assert.Equal(5, (int)JObject.Parse(result.Text)["maxStacks"]);
    }

    [Fact]
    public void Merge_TaggedDefault_FileHasUntaggedSameStat_Appends()
    {
        HeadhunterMergeResult result = MergeTagged(
            """{"defaultsVersion":1,"stats":[{"stat":"FakeA"}]}"""
        );

        JToken added = JObject.Parse(result.Text)["stats"][1];
        Assert.Equal(1, result.Added);
        Assert.Equal("FakeTag", (string)added["tag"]);
    }

    [Fact]
    public void Merge_TaggedDefault_FileHasSameStatAndTag_NotAppended()
    {
        HeadhunterMergeResult result = MergeTagged(
            """{"defaultsVersion":1,"stats":[{"stat":"FakeA","tag":"FakeTag"}]}"""
        );

        Assert.True(result.Changed);
        Assert.Equal(0, result.Added);
        Assert.Single((JArray)JObject.Parse(result.Text)["stats"]);
    }

    [Fact]
    public void Merge_UntaggedDefault_FileHasOnlyTaggedSameStat_Appends()
    {
        HeadhunterMergeResult result = HeadhunterConfigMerger.Merge(
            """{"defaultsVersion":1,"stats":[{"stat":"FakeA","tag":"FakeTag"}]}""",
            TaggedDefaults(new HeadhunterStatEntry("FakeA", 1f, 2f, true))
        );

        var rows = (JArray)JObject.Parse(result.Text)["stats"];
        Assert.Equal(1, result.Added);
        Assert.Equal(2, rows.Count);
        Assert.Null(((JObject)rows[1]).Property("tag"));
    }

    [Fact]
    public void Merge_NoAffixMap_CreatesSection()
    {
        HeadhunterMergeResult result = MergeAffix("""{"defaultsVersion":1}""");

        Assert.Equal(new[] { 100 }, ModKeys(result));
        Assert.Equal(1, result.Added);
    }

    [Fact]
    public void Merge_FileHasModKey_KeepsPlayerRows()
    {
        HeadhunterMergeResult result = MergeAffix(
            """{"defaultsVersion":1,"affixMap":[{"modKey":100,"rows":["FakeZ"]}]}"""
        );

        var entry = (JObject)JObject.Parse(result.Text)["affixMap"][0];
        Assert.Equal(0, result.Added);
        Assert.Equal("FakeZ", (string)entry["rows"][0]);
        Assert.Single((JArray)JObject.Parse(result.Text)["affixMap"]);
    }

    [Fact]
    public void Merge_NewerAffix_AppendedAfterPlayerEntries()
    {
        HeadhunterMergeResult result = MergeAffix(
            """{"defaultsVersion":1,"affixMap":[{"modKey":7,"rows":[]}]}"""
        );

        Assert.Equal(new[] { 7, 100 }, ModKeys(result));
        Assert.Equal(1, result.Added);
    }

    [Fact]
    public void Merge_AppendedAffix_CarriesNoteAndRows()
    {
        HeadhunterMergeResult result = MergeAffix("""{"defaultsVersion":1}""");

        var entry = (JObject)JObject.Parse(result.Text)["affixMap"][0];
        Assert.Equal("FakeNote", (string)entry["note"]);
        Assert.Equal("FakeA", (string)entry["rows"][0]);
    }

    [Fact]
    public void Merge_StampAtSince_DeletedKeyNotReAdded()
    {
        HeadhunterMergeResult result = HeadhunterConfigMerger.Merge(
            """{"defaultsVersion":2,"affixMap":[]}""",
            AffixDefaults(3, HeadhunterTestData.Affix(100, "FakeA"), 2)
        );

        Assert.Empty(ModKeys(result));
        Assert.Equal(0, result.Added);
    }

    [Fact]
    public void Merge_AffixMapNotList_NoMerge()
    {
        const string json = """{"defaultsVersion":1,"affixMap":5}""";

        HeadhunterMergeResult result = MergeAffix(json);

        Assert.False(result.Changed);
        Assert.Equal(json, result.Text);
        Assert.Equal(0, result.Added);
    }

    [Fact]
    public void Merge_MalformedAffixEntries_Ignored()
    {
        HeadhunterMergeResult result = MergeAffix(
            """{"defaultsVersion":1,"affixMap":[5,{"modKey":"x"},{"rows":[]}]}"""
        );

        var entries = (JArray)JObject.Parse(result.Text)["affixMap"];
        Assert.Equal(4, entries.Count);
        Assert.Equal(100, (int)entries[3]["modKey"]);
        Assert.Equal(1, result.Added);
    }

    [Fact]
    public void Merge_Stamp5File_AddsDefaultAffixMap()
    {
        var config = new HeadhunterConfig
        {
            Version = HeadhunterConfigDefaults.CurrentVersion,
            DurationSeconds = HeadhunterConfigDefaults.DurationSeconds,
            MaxStacks = HeadhunterConfigDefaults.MaxStacks,
            Triggers = HeadhunterConfigDefaults.Triggers,
            Stats = HeadhunterConfigDefaults.Stats,
        };
        var file = JObject.Parse(HeadhunterConfigWriter.Write(config));
        file.Remove("affixMap");
        file["defaultsVersion"] = 5;
        var known = HeadhunterConfigDefaults
            .Stats.Select(entry => entry.Stat)
            .ToHashSet(StringComparer.Ordinal);

        HeadhunterMergeResult result = HeadhunterConfigMerger.Merge(
            file.ToString(),
            HeadhunterConfigDefaults.MergeDefaults
        );

        HeadhunterConfigParseResult parsed = HeadhunterConfigParser.Parse(result.Text, known);
        Assert.Empty(parsed.Problems);
        Assert.Equal(HeadhunterAffixDefaults.VersionedAffixes.Count, result.Added);
        Assert.Equal(
            HeadhunterAffixDefaults.AffixMap.Select(entry => entry.ModKey),
            parsed.Config.AffixMap.Select(entry => entry.ModKey)
        );
    }

    [Fact]
    public void Merge_V6File_NoScalingAdded()
    {
        var file = JObject.Parse(HeadhunterConfigWriter.Write(HeadhunterConfigDefaults.Config));
        file["defaultsVersion"] = 6;

        HeadhunterMergeResult result = HeadhunterConfigMerger.Merge(
            file.ToString(),
            HeadhunterConfigDefaults.MergeDefaults
        );

        var merged = JObject.Parse(result.Text);
        Assert.Null(merged["scaling"]);
        Assert.Equal(HeadhunterConfigDefaults.DefaultsVersion, (int)merged["defaultsVersion"]);
    }

    [Fact]
    public void Merge_V8File_AddsModelSize()
    {
        var file = JObject.Parse(HeadhunterConfigWriter.Write(HeadhunterConfigDefaults.Config));
        file.Remove("modelSize");
        file["defaultsVersion"] = 8;

        HeadhunterMergeResult result = HeadhunterConfigMerger.Merge(
            file.ToString(),
            HeadhunterConfigDefaults.MergeDefaults
        );

        var merged = JObject.Parse(result.Text);
        Assert.True(
            JToken.DeepEquals(
                HeadhunterConfigWriter.BuildModelSize(HeadhunterConfigDefaults.ModelSize),
                merged["modelSize"]
            )
        );
        Assert.Equal(1, result.Added);
        Assert.Equal(HeadhunterConfigDefaults.DefaultsVersion, (int)merged["defaultsVersion"]);
    }

    [Fact]
    public void Merge_PlayerModelSize_Kept()
    {
        var file = JObject.Parse(HeadhunterConfigWriter.Write(HeadhunterConfigDefaults.Config));
        file["modelSize"] = new JObject { ["perBuff"] = 1, ["cap"] = 5 };
        file["defaultsVersion"] = 8;

        HeadhunterMergeResult result = HeadhunterConfigMerger.Merge(
            file.ToString(),
            HeadhunterConfigDefaults.MergeDefaults
        );

        var merged = JObject.Parse(result.Text);
        Assert.Equal(1f, (float)merged["modelSize"]["perBuff"]);
        Assert.Equal(5f, (float)merged["modelSize"]["cap"]);
    }

    [Fact]
    public void Merge_V9File_AddsBar()
    {
        var file = JObject.Parse(HeadhunterConfigWriter.Write(HeadhunterConfigDefaults.Config));
        file.Remove("bar");
        file["defaultsVersion"] = 9;

        HeadhunterMergeResult result = HeadhunterConfigMerger.Merge(
            file.ToString(),
            HeadhunterConfigDefaults.MergeDefaults
        );

        var merged = JObject.Parse(result.Text);
        Assert.True(
            JToken.DeepEquals(
                HeadhunterConfigWriter.BuildBar(HeadhunterConfigDefaults.Bar),
                merged["bar"]
            )
        );
        Assert.Equal(1, result.Added);
        Assert.Equal(HeadhunterConfigDefaults.DefaultsVersion, (int)merged["defaultsVersion"]);
    }

    [Fact]
    public void Merge_PlayerBar_Kept()
    {
        var file = JObject.Parse(HeadhunterConfigWriter.Write(HeadhunterConfigDefaults.Config));
        file["bar"] = new JObject { ["offsetY"] = 3, ["perRow"] = 4 };
        file["defaultsVersion"] = 9;

        HeadhunterMergeResult result = HeadhunterConfigMerger.Merge(
            file.ToString(),
            HeadhunterConfigDefaults.MergeDefaults
        );

        var merged = JObject.Parse(result.Text);
        Assert.Equal(3f, (float)merged["bar"]["offsetY"]);
        Assert.Equal(4, (int)merged["bar"]["perRow"]);
    }

    [Fact]
    public void Merge_V10File_AddsAura()
    {
        var file = JObject.Parse(HeadhunterConfigWriter.Write(HeadhunterConfigDefaults.Config));
        file.Remove("aura");
        file["defaultsVersion"] = 10;

        HeadhunterMergeResult result = HeadhunterConfigMerger.Merge(
            file.ToString(),
            HeadhunterConfigDefaults.MergeDefaults
        );

        var merged = JObject.Parse(result.Text);
        Assert.True(
            JToken.DeepEquals(
                HeadhunterConfigWriter.BuildAura(HeadhunterConfigDefaults.Aura),
                merged["aura"]
            )
        );
        Assert.Equal(1, result.Added);
        Assert.Equal(HeadhunterConfigDefaults.DefaultsVersion, (int)merged["defaultsVersion"]);
    }

    [Fact]
    public void Merge_PlayerAura_Kept()
    {
        var file = JObject.Parse(HeadhunterConfigWriter.Write(HeadhunterConfigDefaults.Config));
        file["aura"] = new JObject { ["perBuff"] = 0.4, ["cap"] = 2 };
        file["defaultsVersion"] = 10;

        HeadhunterMergeResult result = HeadhunterConfigMerger.Merge(
            file.ToString(),
            HeadhunterConfigDefaults.MergeDefaults
        );

        var merged = JObject.Parse(result.Text);
        Assert.Equal(0.4f, (float)merged["aura"]["perBuff"]);
        Assert.Equal(2, (int)merged["aura"]["cap"]);
        Assert.Equal(0, result.Added);
        Assert.True(
            JToken.DeepEquals(new JObject { ["perBuff"] = 0.4, ["cap"] = 2 }, merged["aura"])
        );
    }

    [Fact]
    public void Merge_AffixRow_AppendedToPlayerEntry()
    {
        HeadhunterMergeResult result = MergeRow(
            """{"defaultsVersion":1,"affixMap":[{"modKey":100,"rows":["FakeZ"]}]}"""
        );

        Assert.Equal(new[] { "FakeZ", "FakeB" }, RowsOf(result, 0));
        Assert.Equal(1, result.Added);
    }

    [Fact]
    public void Merge_AffixRow_AlreadyPresent_NotDuplicated()
    {
        HeadhunterMergeResult result = MergeRow(
            """{"defaultsVersion":1,"affixMap":[{"modKey":100,"rows":["FakeB","FakeZ"]}]}"""
        );

        Assert.Equal(new[] { "FakeB", "FakeZ" }, RowsOf(result, 0));
        Assert.Equal(0, result.Added);
    }

    [Fact]
    public void Merge_AffixRow_EntryMissing_NothingCreated()
    {
        HeadhunterMergeResult result = MergeRow("""{"defaultsVersion":1,"affixMap":[]}""");

        Assert.Empty((JArray)JObject.Parse(result.Text)["affixMap"]);
        Assert.Equal(0, result.Added);
    }

    [Fact]
    public void Merge_AffixRow_NoAffixMap_NothingCreated()
    {
        HeadhunterMergeResult result = MergeRow("""{"defaultsVersion":1}""");

        Assert.Null(JObject.Parse(result.Text)["affixMap"]);
        Assert.Equal(0, result.Added);
    }

    [Fact]
    public void Merge_AffixRow_StampAtSince_NotReAdded()
    {
        HeadhunterMergeResult result = HeadhunterConfigMerger.Merge(
            """{"defaultsVersion":2,"affixMap":[{"modKey":100,"rows":["FakeZ"]}]}""",
            RowDefaults(3)
        );

        Assert.Equal(new[] { "FakeZ" }, RowsOf(result, 0));
        Assert.Equal(0, result.Added);
    }

    [Theory]
    [InlineData("""{"modKey":100,"rows":5}""")]
    [InlineData("5")]
    [InlineData("""{"modKey":"x"}""")]
    public void Merge_AffixRow_RowsNotListOrMalformedEntries_Ignored(string entry)
    {
        HeadhunterMergeResult result = MergeRow(
            "{\"defaultsVersion\":1,\"affixMap\":[" + entry + "]}"
        );

        var entries = (JArray)JObject.Parse(result.Text)["affixMap"];
        Assert.Single(entries);
        Assert.True(JToken.DeepEquals(JToken.Parse(entry), entries[0]));
        Assert.Equal(0, result.Added);
    }

    [Fact]
    public void Merge_V7File_AddsDefaultAffixRows()
    {
        var file = JObject.Parse(HeadhunterConfigWriter.Write(HeadhunterConfigDefaults.Config));
        foreach (HeadhunterVersionedAffixRow row in HeadhunterAffixDefaults.VersionedRows)
        {
            JToken entry = file["affixMap"].Single(item => (int)item["modKey"] == row.ModKey);
            entry["rows"].Single(item => (string)item == row.Row).Remove();
        }
        file["stats"].Single(stat => (string)stat["stat"] == "Mana").Remove();
        file["defaultsVersion"] = 7;
        var known = HeadhunterConfigDefaults
            .Stats.Select(entry => entry.Stat)
            .ToHashSet(StringComparer.Ordinal);

        HeadhunterMergeResult result = HeadhunterConfigMerger.Merge(
            file.ToString(),
            HeadhunterConfigDefaults.MergeDefaults
        );

        HeadhunterConfigParseResult parsed = HeadhunterConfigParser.Parse(result.Text, known);
        Assert.Empty(parsed.Problems);
        Assert.Equal(
            HeadhunterAffixDefaults.AffixMap.Select(entry => (entry.ModKey, entry.Rows)),
            parsed.Config.AffixMap.Select(entry => (entry.ModKey, entry.Rows))
        );
        Assert.Equal(HeadhunterAffixDefaults.VersionedRows.Count + 1, result.Added);
    }

    [Fact]
    public void Merge_V7File_KeepsPlayerHealthLeechValue()
    {
        var file = JObject.Parse(HeadhunterConfigWriter.Write(HeadhunterConfigDefaults.Config));
        file["stats"].Single(stat => (string)stat["stat"] == "HealthLeech")["added"] = 0.01f;
        file["defaultsVersion"] = 7;

        HeadhunterMergeResult result = HeadhunterConfigMerger.Merge(
            file.ToString(),
            HeadhunterConfigDefaults.MergeDefaults
        );

        JToken leech = JObject
            .Parse(result.Text)["stats"]
            .Single(stat => (string)stat["stat"] == "HealthLeech");
        Assert.Equal(0.01f, (float)leech["added"]);
    }

    private static HeadhunterMergeResult MergeRow(string json)
    {
        return HeadhunterConfigMerger.Merge(json, RowDefaults(2));
    }

    private static HeadhunterMergeDefaults RowDefaults(int version)
    {
        return new HeadhunterMergeDefaults
        {
            Version = version,
            Stats = new List<HeadhunterVersionedStat>(),
            Fields = new List<HeadhunterVersionedField>(),
            AffixRows = new List<HeadhunterVersionedAffixRow> { new(100, "FakeB", 2) },
        };
    }

    private static List<string> RowsOf(HeadhunterMergeResult result, int index)
    {
        return ((JArray)JObject.Parse(result.Text)["affixMap"][index]["rows"])
            .Select(item => (string)item)
            .ToList();
    }

    private static HeadhunterMergeResult MergeAffix(string json)
    {
        return HeadhunterConfigMerger.Merge(
            json,
            AffixDefaults(2, HeadhunterTestData.Affix(100, "FakeA"), 2)
        );
    }

    private static HeadhunterMergeDefaults AffixDefaults(
        int version,
        HeadhunterAffixEntry entry,
        int since
    )
    {
        return new HeadhunterMergeDefaults
        {
            Version = version,
            Stats = new List<HeadhunterVersionedStat>(),
            Fields = new List<HeadhunterVersionedField>(),
            Affixes = new List<HeadhunterVersionedAffix> { new(entry, since) },
        };
    }

    private static List<int> ModKeys(HeadhunterMergeResult result)
    {
        return ((JArray)JObject.Parse(result.Text)["affixMap"])
            .Select(row => (int)row["modKey"])
            .ToList();
    }

    private static int CountAfter4()
    {
        return HeadhunterConfigDefaults.VersionedStats.Count(row => row.Since > 4);
    }

    private static HeadhunterMergeResult Merge(string json)
    {
        return HeadhunterConfigMerger.Merge(json, _defaults);
    }

    private static List<string> StatNamesOrNull(HeadhunterMergeResult result)
    {
        return ((JArray)JObject.Parse(result.Text)["stats"])
            .Select(row => row is JObject obj ? obj["stat"] as JValue : null)
            .Select(value => value?.Value as string)
            .ToList();
    }

    private static List<string> StatNames(HeadhunterMergeResult result)
    {
        return ((JArray)JObject.Parse(result.Text)["stats"])
            .Select(row => (string)row["stat"])
            .ToList();
    }

    private static HeadhunterMergeResult MergeTagged(string json)
    {
        return HeadhunterConfigMerger.Merge(
            json,
            TaggedDefaults(new HeadhunterStatEntry("FakeA", 1f, 2f, true, "FakeTag"))
        );
    }

    private static HeadhunterMergeDefaults TaggedDefaults(HeadhunterStatEntry entry)
    {
        return new HeadhunterMergeDefaults
        {
            Version = 2,
            Stats = new List<HeadhunterVersionedStat> { new(entry, 2) },
            Fields = new List<HeadhunterVersionedField>(),
        };
    }
}
