using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Config;
using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Config.Defaults;
using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Config.Json;
using Code = LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Config.HeadhunterConfigProblemCode;

namespace LastEpoch_Hud.Tests.Core.CustomItems.Headhunter.Config.Json;

public sealed class HeadhunterConfigParserModelSizeTests
{
    [Fact]
    public void Parse_NoModelSize_Defaults()
    {
        HeadhunterConfigParseResult result = Parse("{}");

        Assert.Empty(result.Problems);
        Assert.Equal(HeadhunterConfigDefaults.ModelSize, result.Config.ModelSize);
    }

    [Fact]
    public void Parse_ModelSize_Read()
    {
        HeadhunterConfigParseResult result = Parse("""{"modelSize":{"perBuff":3,"cap":15}}""");

        Assert.Empty(result.Problems);
        Assert.Equal(new HeadhunterSizeCurve(3f, 15f), result.Config.ModelSize);
    }

    [Fact]
    public void Parse_ModelSizeNotObject_Problem()
    {
        HeadhunterConfigParseResult result = Parse("""{"modelSize":5}""");

        Assert.Equal(
            new[] { (Code.NotObject, "modelSize") },
            HeadhunterTestData.Problems(result.Problems)
        );
        Assert.Equal(HeadhunterConfigDefaults.ModelSize, result.Config.ModelSize);
    }

    [Theory]
    [InlineData("-1")]
    [InlineData("101")]
    [InlineData("\"x\"")]
    public void Parse_BadPerBuff_ProblemAndFallback(string value)
    {
        HeadhunterConfigParseResult result = Parse(
            "{\"modelSize\":{\"perBuff\":" + value + ",\"cap\":15}}"
        );

        Assert.Equal(
            new[] { (Code.NotPercent, "modelSize.perBuff") },
            HeadhunterTestData.Problems(result.Problems)
        );
        Assert.Equal(
            new HeadhunterSizeCurve(HeadhunterConfigDefaults.ModelSize.PerBuffPercent, 15f),
            result.Config.ModelSize
        );
    }

    [Theory]
    [InlineData("-1")]
    [InlineData("101")]
    [InlineData("\"x\"")]
    public void Parse_BadCap_ProblemAndFallback(string value)
    {
        HeadhunterConfigParseResult result = Parse(
            "{\"modelSize\":{\"perBuff\":3,\"cap\":" + value + "}}"
        );

        Assert.Equal(
            new[] { (Code.NotPercent, "modelSize.cap") },
            HeadhunterTestData.Problems(result.Problems)
        );
        Assert.Equal(
            new HeadhunterSizeCurve(3f, HeadhunterConfigDefaults.ModelSize.CapPercent),
            result.Config.ModelSize
        );
    }

    [Theory]
    [InlineData("0", "100", 0f, 100f)]
    [InlineData("2.5", "0", 2.5f, 0f)]
    public void Parse_EdgePercent_Read(
        string perBuff,
        string cap,
        float expectedPerBuff,
        float expectedCap
    )
    {
        HeadhunterConfigParseResult result = Parse(
            "{\"modelSize\":{\"perBuff\":" + perBuff + ",\"cap\":" + cap + "}}"
        );

        Assert.Empty(result.Problems);
        Assert.Equal(
            new HeadhunterSizeCurve(expectedPerBuff, expectedCap),
            result.Config.ModelSize
        );
    }

    [Fact]
    public void Parse_ModelSizeMissingKey_DefaultsThatKey()
    {
        HeadhunterConfigParseResult result = Parse("""{"modelSize":{"cap":15}}""");

        Assert.Empty(result.Problems);
        Assert.Equal(
            new HeadhunterSizeCurve(HeadhunterConfigDefaults.ModelSize.PerBuffPercent, 15f),
            result.Config.ModelSize
        );
    }

    private static HeadhunterConfigParseResult Parse(string json)
    {
        return HeadhunterConfigParser.Parse(json, new HashSet<string>(StringComparer.Ordinal));
    }
}
