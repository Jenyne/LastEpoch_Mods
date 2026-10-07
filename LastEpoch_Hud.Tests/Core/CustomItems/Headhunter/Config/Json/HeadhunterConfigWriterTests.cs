using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Config;
using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Config.Defaults;
using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Config.Json;
using Newtonsoft.Json.Linq;

namespace LastEpoch_Hud.Tests.Core.CustomItems.Headhunter.Config.Json;

public sealed class HeadhunterConfigWriterTests
{
    [Fact]
    public void Write_RoundTrips_Defaults()
    {
        var known = HeadhunterConfigDefaults
            .Stats.Select(entry => entry.Stat)
            .ToHashSet(StringComparer.Ordinal);

        HeadhunterConfigParseResult result = HeadhunterConfigParser.Parse(
            HeadhunterConfigWriter.Write(HeadhunterConfigDefaults.Config),
            known
        );

        Assert.Empty(result.Problems);
        Assert.Equivalent(HeadhunterConfigDefaults.Config, result.Config, strict: true);
    }

    [Fact]
    public void Write_RoundTrips_CustomConfig()
    {
        var config = new HeadhunterConfig
        {
            Version = 1,
            DurationSeconds = 12.5f,
            MaxStacks = 7,
            Triggers = new HeadhunterTriggers(true, false, true, false, false),
            Stats = new List<HeadhunterStatEntry>
            {
                new("FakeA", 1.5f, 0f, true),
                new("FakeB", 0f, 7.25f, false),
            },
        };
        var known = new HashSet<string>(StringComparer.Ordinal) { "FakeA", "FakeB" };

        HeadhunterConfigParseResult result = HeadhunterConfigParser.Parse(
            HeadhunterConfigWriter.Write(config),
            known
        );

        Assert.Empty(result.Problems);
        Assert.Equivalent(config, result.Config, strict: true);
    }

    [Fact]
    public void Write_ContainsMaxStacks()
    {
        var root = JObject.Parse(HeadhunterConfigWriter.Write(HeadhunterConfigDefaults.Config));

        Assert.Equal(HeadhunterConfigDefaults.MaxStacks, (int)root["maxStacks"]);
    }

    [Fact]
    public void Write_OmitsMechanicKey()
    {
        var root = JObject.Parse(HeadhunterConfigWriter.Write(HeadhunterConfigDefaults.Config));

        Assert.False(root.ContainsKey("mechanic"));
    }

    [Fact]
    public void Write_StampsDefaultsVersion()
    {
        var root = JObject.Parse(HeadhunterConfigWriter.Write(HeadhunterConfigDefaults.Config));

        Assert.Equal(HeadhunterConfigDefaults.DefaultsVersion, (int)root["defaultsVersion"]);
    }

    [Fact]
    public void Write_IsIndented()
    {
        Assert.Contains("\n", HeadhunterConfigWriter.Write(HeadhunterConfigDefaults.Config));
    }

    [Fact]
    public void Write_TaggedRow_WritesTag()
    {
        HeadhunterConfig config = HeadhunterTestData.Config(
            HeadhunterTestData.AllTriggers,
            HeadhunterTestData.Tagged("FakeA", "FakeTag")
        );

        var root = JObject.Parse(HeadhunterConfigWriter.Write(config));

        Assert.Equal("FakeTag", (string)root["stats"][0]["tag"]);
    }

    [Fact]
    public void Write_UntaggedRow_OmitsTag()
    {
        HeadhunterConfig config = HeadhunterTestData.Config(
            HeadhunterTestData.AllTriggers,
            HeadhunterTestData.Entry("FakeA")
        );

        var root = JObject.Parse(HeadhunterConfigWriter.Write(config));

        Assert.Null(((JObject)root["stats"][0]).Property("tag"));
    }

    [Fact]
    public void Write_Parse_RoundTrip_KeepsTag()
    {
        HeadhunterConfig config = HeadhunterTestData.Config(
            HeadhunterTestData.AllTriggers,
            HeadhunterTestData.Entry("FakeA"),
            HeadhunterTestData.Tagged("FakeA", "FakeTag", 1f, 2f)
        );
        var known = new HashSet<string>(StringComparer.Ordinal) { "FakeA" };

        HeadhunterConfigParseResult result = HeadhunterConfigParser.Parse(
            HeadhunterConfigWriter.Write(config),
            known
        );

        Assert.Empty(result.Problems);
        Assert.Equal(config.Stats, result.Config.Stats);
    }

    [Fact]
    public void Write_AffixMap_WritesEntriesAfterStats()
    {
        HeadhunterConfig config = HeadhunterTestData.ConfigWithMap(
            [HeadhunterTestData.Affix(100, "FakeA", "FakeB_FakeTag")],
            HeadhunterTestData.Entry("FakeA")
        );

        var root = JObject.Parse(HeadhunterConfigWriter.Write(config));

        var names = root.Properties().Select(property => property.Name).ToList();
        Assert.Equal(names.IndexOf("stats") + 1, names.IndexOf("affixMap"));
        Assert.Equal(100, (int)root["affixMap"][0]["modKey"]);
        Assert.Equal("FakeNote", (string)root["affixMap"][0]["note"]);
        Assert.Equal(
            new[] { "FakeA", "FakeB_FakeTag" },
            root["affixMap"][0]["rows"].Select(row => (string)row)
        );
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Write_AffixEntryWithoutNote_OmitsNote(string note)
    {
        HeadhunterConfig config = HeadhunterTestData.ConfigWithMap(
            [new HeadhunterAffixEntry(100, note, ["FakeA"])],
            HeadhunterTestData.Entry("FakeA")
        );

        var root = JObject.Parse(HeadhunterConfigWriter.Write(config));

        Assert.Null(((JObject)root["affixMap"][0]).Property("note"));
    }

    [Fact]
    public void Write_Parse_RoundTrip_KeepsAffixMap()
    {
        HeadhunterAffixEntry[] map =
        [
            HeadhunterTestData.Affix(100, "FakeA", "FakeB_FakeTag"),
            new HeadhunterAffixEntry(-200, null, []),
        ];
        HeadhunterConfig config = HeadhunterTestData.ConfigWithMap(
            map,
            HeadhunterTestData.Entry("FakeA")
        );
        var known = new HashSet<string>(StringComparer.Ordinal) { "FakeA" };

        HeadhunterConfigParseResult result = HeadhunterConfigParser.Parse(
            HeadhunterConfigWriter.Write(config),
            known
        );

        Assert.Empty(result.Problems);
        Assert.Equivalent(map, result.Config.AffixMap, strict: true);
    }

    [Fact]
    public void Write_Defaults_HasNoScaling()
    {
        var written = JObject.Parse(HeadhunterConfigWriter.Write(HeadhunterConfigDefaults.Config));

        Assert.Null(written["scaling"]);
    }

    [Fact]
    public void Write_RoundTrips_ModelSize()
    {
        HeadhunterConfig config = new()
        {
            Version = 1,
            DurationSeconds = 10f,
            Triggers = HeadhunterTestData.AllTriggers,
            Stats = new List<HeadhunterStatEntry>(),
            ModelSize = new HeadhunterSizeCurve(3f, 15f),
        };

        HeadhunterConfigParseResult result = HeadhunterConfigParser.Parse(
            HeadhunterConfigWriter.Write(config),
            new HashSet<string>(StringComparer.Ordinal)
        );

        Assert.Empty(result.Problems);
        Assert.Equal(config.ModelSize, result.Config.ModelSize);
    }

    [Fact]
    public void Write_Defaults_HasModelSize()
    {
        var root = JObject.Parse(HeadhunterConfigWriter.Write(HeadhunterConfigDefaults.Config));

        Assert.Equal(2f, (float)root["modelSize"]["perBuff"]);
        Assert.Equal(20f, (float)root["modelSize"]["cap"]);
    }

    [Fact]
    public void Write_RoundTrips_Bar()
    {
        HeadhunterConfig config = new()
        {
            Version = 1,
            DurationSeconds = 10f,
            Triggers = HeadhunterTestData.AllTriggers,
            Stats = new List<HeadhunterStatEntry>(),
            Bar = new HeadhunterBarSettings(-0.5f, 2f, 0.7f, 5),
        };

        HeadhunterConfigParseResult result = HeadhunterConfigParser.Parse(
            HeadhunterConfigWriter.Write(config),
            new HashSet<string>(StringComparer.Ordinal)
        );

        Assert.Empty(result.Problems);
        Assert.Equal(config.Bar, result.Config.Bar);
    }

    [Fact]
    public void Write_Defaults_HasBar()
    {
        var root = JObject.Parse(HeadhunterConfigWriter.Write(HeadhunterConfigDefaults.Config));

        Assert.True(
            JToken.DeepEquals(
                HeadhunterConfigWriter.BuildBar(HeadhunterConfigDefaults.Bar),
                root["bar"]
            )
        );
    }

    [Fact]
    public void Write_RoundTrips_Aura()
    {
        HeadhunterConfig config = new()
        {
            Version = 1,
            DurationSeconds = 10f,
            Triggers = HeadhunterTestData.AllTriggers,
            Stats = new List<HeadhunterStatEntry>(),
            Aura = new HeadhunterAuraCurve(false, 0.25f, 2f),
        };

        HeadhunterConfigParseResult result = HeadhunterConfigParser.Parse(
            HeadhunterConfigWriter.Write(config),
            new HashSet<string>(StringComparer.Ordinal)
        );

        Assert.Empty(result.Problems);
        Assert.Equal(config.Aura, result.Config.Aura);
    }

    [Fact]
    public void Write_Defaults_HasAura()
    {
        var root = JObject.Parse(HeadhunterConfigWriter.Write(HeadhunterConfigDefaults.Config));

        Assert.True(
            JToken.DeepEquals(
                HeadhunterConfigWriter.BuildAura(HeadhunterConfigDefaults.Aura),
                root["aura"]
            )
        );
    }
}
