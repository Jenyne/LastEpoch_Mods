using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Config;
using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Config.Defaults;
using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Config.Json;
using Code = LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Config.HeadhunterConfigProblemCode;

namespace LastEpoch_Hud.Tests.Core.CustomItems.Headhunter.Config.Json;

public sealed class HeadhunterConfigParserScalingTests
{
    private static readonly IReadOnlySet<string> _known = new HashSet<string>(
        StringComparer.Ordinal
    );

    private static readonly HeadhunterGrowthCurve _defaults = HeadhunterConfigDefaults.ValueGrowth;

    [Fact]
    public void Parse_ScalingMissing_UsesDefaults()
    {
        HeadhunterConfigParseResult result = Parse("{}");

        Assert.Empty(result.Problems);
        Assert.Equal(_defaults, result.Config.ValueGrowth);
    }

    [Fact]
    public void Parse_ScalingValid_Read()
    {
        HeadhunterConfigParseResult result = Parse(Values("""{"perStack":4,"cap":40}"""));

        Assert.Empty(result.Problems);
        Assert.Equal(new HeadhunterGrowthCurve(4f, 40f), result.Config.ValueGrowth);
    }

    [Fact]
    public void Parse_NegativePerStack_ProblemAndDefault()
    {
        HeadhunterConfigParseResult result = Parse(Values("""{"perStack":-1,"cap":40}"""));

        Assert.Equal(
            new[] { (Code.NotNonNegativeNumber, "scaling.values.perStack") },
            Problems(result)
        );
        Assert.Equal(
            new HeadhunterGrowthCurve(_defaults.PerStackPercent, 40f),
            result.Config.ValueGrowth
        );
    }

    [Fact]
    public void Parse_TextCap_ProblemAndDefault()
    {
        HeadhunterConfigParseResult result = Parse(Values("""{"perStack":4,"cap":"x"}"""));

        Assert.Equal(new[] { (Code.NotNonNegativeNumber, "scaling.values.cap") }, Problems(result));
        Assert.Equal(
            new HeadhunterGrowthCurve(4f, _defaults.CapPercent),
            result.Config.ValueGrowth
        );
    }

    [Fact]
    public void Parse_PerStackMissing_DefaultNoProblem()
    {
        HeadhunterConfigParseResult result = Parse(Values("""{"cap":40}"""));

        Assert.Empty(result.Problems);
        Assert.Equal(
            new HeadhunterGrowthCurve(_defaults.PerStackPercent, 40f),
            result.Config.ValueGrowth
        );
    }

    [Fact]
    public void Parse_HugeCap_ProblemAndDefault()
    {
        HeadhunterConfigParseResult result = Parse(Values("""{"perStack":4,"cap":1e40}"""));

        Assert.Equal(new[] { (Code.NotNonNegativeNumber, "scaling.values.cap") }, Problems(result));
        Assert.Equal(
            new HeadhunterGrowthCurve(4f, _defaults.CapPercent),
            result.Config.ValueGrowth
        );
    }

    [Fact]
    public void Parse_ZeroPerStack_NoProblem()
    {
        HeadhunterConfigParseResult result = Parse(Values("""{"perStack":0,"cap":40}"""));

        Assert.Empty(result.Problems);
        Assert.Equal(new HeadhunterGrowthCurve(0f, 40f), result.Config.ValueGrowth);
    }

    [Fact]
    public void Parse_ScalingNotObject_ProblemAndDefaults()
    {
        HeadhunterConfigParseResult result = Parse("""{"scaling":5}""");

        Assert.Equal(new[] { (Code.NotObject, "scaling") }, Problems(result));
        Assert.Equal(_defaults, result.Config.ValueGrowth);
    }

    [Fact]
    public void Parse_ValuesNotObject_ProblemAndDefaults()
    {
        HeadhunterConfigParseResult result = Parse("""{"scaling":{"values":5}}""");

        Assert.Equal(new[] { (Code.NotObject, "scaling.values") }, Problems(result));
        Assert.Equal(_defaults, result.Config.ValueGrowth);
    }

    private static string Values(string valuesJson)
    {
        return "{\"scaling\":{\"values\":" + valuesJson + "}}";
    }

    private static HeadhunterConfigParseResult Parse(string json)
    {
        return HeadhunterConfigParser.Parse(json, _known);
    }

    private static (Code Code, string Path)[] Problems(HeadhunterConfigParseResult result)
    {
        return HeadhunterTestData.Problems(result.Problems);
    }
}
