using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Config;
using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Config.Defaults;
using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Config.Json;
using Code = LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Config.HeadhunterConfigProblemCode;

namespace LastEpoch_Hud.Tests.Core.CustomItems.Headhunter.Config.Json;

public sealed class HeadhunterConfigParserBarTests
{
    private static HeadhunterBarSettings Defaults => HeadhunterConfigDefaults.Bar;

    [Fact]
    public void Parse_NoBar_Defaults()
    {
        HeadhunterConfigParseResult result = Parse("{}");

        Assert.Empty(result.Problems);
        Assert.Equal(Defaults, result.Config.Bar);
    }

    [Fact]
    public void Parse_Bar_Read()
    {
        HeadhunterConfigParseResult result = Parse(
            """{"bar":{"offsetX":-0.5,"offsetY":2,"iconSize":0.7,"perRow":5}}"""
        );

        Assert.Empty(result.Problems);
        Assert.Equal(new HeadhunterBarSettings(-0.5f, 2f, 0.7f, 5), result.Config.Bar);
    }

    [Fact]
    public void Parse_BarNotObject_Problem()
    {
        HeadhunterConfigParseResult result = Parse("""{"bar":5}""");

        Assert.Equal(
            new[] { (Code.NotObject, "bar") },
            HeadhunterTestData.Problems(result.Problems)
        );
        Assert.Equal(Defaults, result.Config.Bar);
    }

    [Theory]
    [InlineData("\"x\"")]
    [InlineData("true")]
    [InlineData("NaN")]
    [InlineData("Infinity")]
    public void Parse_BadOffsetX_ProblemAndKeyDefault(string value)
    {
        HeadhunterConfigParseResult result = Parse(
            "{\"bar\":{\"offsetX\":" + value + ",\"offsetY\":3,\"iconSize\":2,\"perRow\":4}}"
        );

        Assert.Equal(
            new[] { (Code.NotFiniteNumber, "bar.offsetX") },
            HeadhunterTestData.Problems(result.Problems)
        );
        Assert.Equal(new HeadhunterBarSettings(Defaults.OffsetX, 3f, 2f, 4), result.Config.Bar);
    }

    [Theory]
    [InlineData("\"x\"")]
    [InlineData("true")]
    [InlineData("NaN")]
    [InlineData("Infinity")]
    public void Parse_BadOffsetY_ProblemAndKeyDefault(string value)
    {
        HeadhunterConfigParseResult result = Parse(
            "{\"bar\":{\"offsetX\":3,\"offsetY\":" + value + ",\"iconSize\":2,\"perRow\":4}}"
        );

        Assert.Equal(
            new[] { (Code.NotFiniteNumber, "bar.offsetY") },
            HeadhunterTestData.Problems(result.Problems)
        );
        Assert.Equal(new HeadhunterBarSettings(3f, Defaults.OffsetY, 2f, 4), result.Config.Bar);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-1")]
    [InlineData("NaN")]
    [InlineData("\"x\"")]
    public void Parse_BadIconSize_Problem(string value)
    {
        HeadhunterConfigParseResult result = Parse("{\"bar\":{\"iconSize\":" + value + "}}");

        Assert.Equal(
            new[] { (Code.NotPositiveNumber, "bar.iconSize") },
            HeadhunterTestData.Problems(result.Problems)
        );
        Assert.Equal(Defaults, result.Config.Bar);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-2")]
    [InlineData("1.5")]
    [InlineData("\"x\"")]
    public void Parse_BadPerRow_Problem(string value)
    {
        HeadhunterConfigParseResult result = Parse("{\"bar\":{\"perRow\":" + value + "}}");

        Assert.Equal(
            new[] { (Code.NotPositiveWholeNumber, "bar.perRow") },
            HeadhunterTestData.Problems(result.Problems)
        );
        Assert.Equal(Defaults, result.Config.Bar);
    }

    [Fact]
    public void Parse_BarMissingKey_DefaultsThatKey()
    {
        HeadhunterConfigParseResult result = Parse("""{"bar":{"perRow":5}}""");

        Assert.Empty(result.Problems);
        Assert.Equal(Defaults with { PerRow = 5 }, result.Config.Bar);
    }

    private static HeadhunterConfigParseResult Parse(string json)
    {
        return HeadhunterConfigParser.Parse(json, new HashSet<string>(StringComparer.Ordinal));
    }
}
