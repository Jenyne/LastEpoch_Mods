using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Config;
using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Config.Defaults;
using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Config.Json;
using Code = LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Config.HeadhunterConfigProblemCode;

namespace LastEpoch_Hud.Tests.Core.CustomItems.Headhunter.Config.Json;

public sealed class HeadhunterConfigParserAuraTests
{
    private static HeadhunterAuraCurve Defaults => HeadhunterConfigDefaults.Aura;

    [Fact]
    public void Parse_NoAura_Defaults()
    {
        HeadhunterConfigParseResult result = Parse("{}");

        Assert.Empty(result.Problems);
        Assert.Equal(Defaults, result.Config.Aura);
    }

    [Fact]
    public void Parse_Aura_Read()
    {
        HeadhunterConfigParseResult result = Parse(
            """{"aura":{"enabled":false,"perBuff":0.25,"cap":2}}"""
        );

        Assert.Empty(result.Problems);
        Assert.Equal(new HeadhunterAuraCurve(false, 0.25f, 2f), result.Config.Aura);
    }

    [Theory]
    [InlineData("5")]
    [InlineData("[]")]
    public void Parse_AuraNotObject_Problem(string value)
    {
        HeadhunterConfigParseResult result = Parse("{\"aura\":" + value + "}");

        Assert.Equal(
            new[] { (Code.NotObject, "aura") },
            HeadhunterTestData.Problems(result.Problems)
        );
        Assert.Equal(Defaults, result.Config.Aura);
    }

    [Theory]
    [InlineData("\"yes\"")]
    [InlineData("1")]
    public void Parse_BadEnabled_ProblemAndDefault(string value)
    {
        HeadhunterConfigParseResult result = Parse(
            "{\"aura\":{\"enabled\":" + value + ",\"perBuff\":0.25,\"cap\":2}}"
        );

        Assert.Equal(
            new[] { (Code.NotBool, "aura.enabled") },
            HeadhunterTestData.Problems(result.Problems)
        );
        Assert.Equal(new HeadhunterAuraCurve(Defaults.Enabled, 0.25f, 2f), result.Config.Aura);
    }

    [Theory]
    [InlineData("-0.1")]
    [InlineData("3.5")]
    [InlineData("\"x\"")]
    [InlineData("true")]
    public void Parse_BadPerBuff_ProblemAndKeyDefault(string value)
    {
        HeadhunterConfigParseResult result = Parse(
            "{\"aura\":{\"perBuff\":" + value + ",\"cap\":0.5}}"
        );

        Assert.Equal(
            new[] { (Code.NotAuraStrength, "aura.perBuff") },
            HeadhunterTestData.Problems(result.Problems)
        );
        Assert.Equal(
            new HeadhunterAuraCurve(Defaults.Enabled, Defaults.PerBuff, 0.5f),
            result.Config.Aura
        );
    }

    [Theory]
    [InlineData("-0.1")]
    [InlineData("3.5")]
    [InlineData("\"x\"")]
    [InlineData("true")]
    public void Parse_BadCap_ProblemAndKeyDefault(string value)
    {
        HeadhunterConfigParseResult result = Parse(
            "{\"aura\":{\"perBuff\":0.5,\"cap\":" + value + "}}"
        );

        Assert.Equal(
            new[] { (Code.NotAuraStrength, "aura.cap") },
            HeadhunterTestData.Problems(result.Problems)
        );
        Assert.Equal(
            new HeadhunterAuraCurve(Defaults.Enabled, 0.5f, Defaults.Cap),
            result.Config.Aura
        );
    }

    [Theory]
    [InlineData("0", "3", 0f, 3f)]
    [InlineData("3", "0", 3f, 0f)]
    public void Parse_StrengthBounds_Accepted(
        string perBuff,
        string cap,
        float expectedPerBuff,
        float expectedCap
    )
    {
        HeadhunterConfigParseResult result = Parse(
            "{\"aura\":{\"perBuff\":" + perBuff + ",\"cap\":" + cap + "}}"
        );

        Assert.Empty(result.Problems);
        Assert.Equal(
            new HeadhunterAuraCurve(Defaults.Enabled, expectedPerBuff, expectedCap),
            result.Config.Aura
        );
    }

    [Fact]
    public void Parse_AuraMissingKey_DefaultsThatKey()
    {
        HeadhunterConfigParseResult result = Parse("""{"aura":{"cap":2}}""");

        Assert.Empty(result.Problems);
        Assert.Equal(
            new HeadhunterAuraCurve(Defaults.Enabled, Defaults.PerBuff, 2f),
            result.Config.Aura
        );
    }

    private static HeadhunterConfigParseResult Parse(string json)
    {
        return HeadhunterConfigParser.Parse(json, new HashSet<string>(StringComparer.Ordinal));
    }
}
