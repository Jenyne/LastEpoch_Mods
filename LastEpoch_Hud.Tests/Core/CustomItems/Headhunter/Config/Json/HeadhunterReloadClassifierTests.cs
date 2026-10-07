using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Config.Json;

namespace LastEpoch_Hud.Tests.Core.CustomItems.Headhunter.Config.Json;

public sealed class HeadhunterReloadClassifierTests
{
    private const string Base = """{"a":1,"b":{"c":2},"bar":{"perRow":3}}""";

    [Fact]
    public void Classify_SameText_VisualOnly()
    {
        Assert.Equal(
            HeadhunterReloadKind.VisualOnly,
            HeadhunterReloadClassifier.Classify(Base, Base)
        );
    }

    [Fact]
    public void Classify_ReformattedOrReordered_VisualOnly()
    {
        const string next = """
            {
              "bar": { "perRow": 3 },
              "b": { "c": 2 },
              "a": 1
            }
            """;

        Assert.Equal(
            HeadhunterReloadKind.VisualOnly,
            HeadhunterReloadClassifier.Classify(Base, next)
        );
    }

    [Fact]
    public void Classify_OnlyBarChanged_VisualOnly()
    {
        const string next = """{"a":1,"b":{"c":2},"bar":{"perRow":7}}""";

        Assert.Equal(
            HeadhunterReloadKind.VisualOnly,
            HeadhunterReloadClassifier.Classify(Base, next)
        );
    }

    [Fact]
    public void Classify_OnlyAuraChanged_VisualOnly()
    {
        const string previous = """{"a":1,"b":{"c":2},"aura":{"cap":1}}""";
        const string next = """{"a":1,"b":{"c":2},"aura":{"cap":2}}""";

        Assert.Equal(
            HeadhunterReloadKind.VisualOnly,
            HeadhunterReloadClassifier.Classify(previous, next)
        );
    }

    [Fact]
    public void Classify_BarAndAuraChanged_VisualOnly()
    {
        const string previous = """{"a":1,"bar":{"perRow":3},"aura":{"cap":1}}""";
        const string next = """{"a":1,"bar":{"perRow":7},"aura":{"cap":2}}""";

        Assert.Equal(
            HeadhunterReloadKind.VisualOnly,
            HeadhunterReloadClassifier.Classify(previous, next)
        );
    }

    [Fact]
    public void Classify_AuraAndOtherChanged_Full()
    {
        const string previous = """{"a":1,"aura":{"cap":1}}""";
        const string next = """{"a":2,"aura":{"cap":2}}""";

        Assert.Equal(
            HeadhunterReloadKind.Full,
            HeadhunterReloadClassifier.Classify(previous, next)
        );
    }

    [Theory]
    [InlineData("""{"a":1,"b":{"c":2},"bar":{"perRow":3}}""", """{"a":1,"b":{"c":2}}""")]
    [InlineData("""{"a":1,"b":{"c":2}}""", """{"a":1,"b":{"c":2},"bar":{"perRow":3}}""")]
    public void Classify_BarAddedOrRemoved_VisualOnly(string previous, string next)
    {
        Assert.Equal(
            HeadhunterReloadKind.VisualOnly,
            HeadhunterReloadClassifier.Classify(previous, next)
        );
    }

    [Theory]
    [InlineData("""{"a":1,"aura":{"cap":1}}""", """{"a":1}""")]
    [InlineData("""{"a":1}""", """{"a":1,"aura":{"cap":1}}""")]
    public void Classify_AuraAddedOrRemoved_VisualOnly(string previous, string next)
    {
        Assert.Equal(
            HeadhunterReloadKind.VisualOnly,
            HeadhunterReloadClassifier.Classify(previous, next)
        );
    }

    [Fact]
    public void Classify_OtherValueChanged_Full()
    {
        const string next = """{"a":2,"b":{"c":2},"bar":{"perRow":3}}""";

        Assert.Equal(HeadhunterReloadKind.Full, HeadhunterReloadClassifier.Classify(Base, next));
    }

    [Fact]
    public void Classify_NestedValueChanged_Full()
    {
        const string next = """{"a":1,"b":{"c":3},"bar":{"perRow":3}}""";

        Assert.Equal(HeadhunterReloadKind.Full, HeadhunterReloadClassifier.Classify(Base, next));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("{")]
    [InlineData("[]")]
    public void Classify_InvalidOrEmptyPrevious_Full(string previous)
    {
        Assert.Equal(
            HeadhunterReloadKind.Full,
            HeadhunterReloadClassifier.Classify(previous, Base)
        );
    }

    [Fact]
    public void Classify_InvalidNext_Full()
    {
        Assert.Equal(HeadhunterReloadKind.Full, HeadhunterReloadClassifier.Classify(Base, "{"));
    }
}
