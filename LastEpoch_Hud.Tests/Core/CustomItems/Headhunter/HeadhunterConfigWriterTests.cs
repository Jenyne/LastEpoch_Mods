using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter;
using Newtonsoft.Json.Linq;

namespace LastEpoch_Hud.Tests.Core.CustomItems.Headhunter;

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
            Mechanic = "fake_id",
            DurationSeconds = 12.5f,
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
}
