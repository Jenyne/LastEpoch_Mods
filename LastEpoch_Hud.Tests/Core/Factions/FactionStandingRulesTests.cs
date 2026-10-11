using LastEpoch_Hud.Scripts.Core.Factions;

namespace LastEpoch_Hud.Tests.Core.Factions;

/// <summary>Limits for favor, rank and reputation edits.</summary>
public sealed class FactionStandingRulesTests
{
    [Theory]
    [InlineData(FactionKind.CircleOfFortune, 12)]
    [InlineData(FactionKind.MerchantsGuild, 12)]
    [InlineData(FactionKind.ForgottenKnights, 10)]
    [InlineData(FactionKind.TheWeaver, 10)]
    public void MaxRank_PerFaction(FactionKind kind, int expected)
    {
        Assert.Equal(expected, FactionStandingRules.MaxRank(kind));
    }

    [Fact]
    public void ClampRank_WithinRange_ReturnsRequested()
    {
        Assert.Equal(7, FactionStandingRules.ClampRank(FactionKind.ForgottenKnights, 7));
    }

    [Theory]
    [InlineData(FactionKind.CircleOfFortune, 13, 12)]
    [InlineData(FactionKind.TheWeaver, 12, 10)]
    public void ClampRank_AboveMax_ReturnsFactionMax(FactionKind kind, int requested, int expected)
    {
        Assert.Equal(expected, FactionStandingRules.ClampRank(kind, requested));
    }

    [Fact]
    public void ClampRank_Negative_ReturnsZero()
    {
        Assert.Equal(0, FactionStandingRules.ClampRank(FactionKind.MerchantsGuild, -1));
    }

    [Fact]
    public void ClampFavorGain_WithinRoom_ReturnsRequested()
    {
        Assert.Equal(100, FactionStandingRules.ClampFavorGain(100, 0));
    }

    [Theory]
    [InlineData(500, 999800, 199)]
    [InlineData(int.MaxValue, 0, FactionStandingRules.MaxFavor)]
    public void ClampFavorGain_AboveRoom_ReturnsRoom(int requested, int current, int expected)
    {
        Assert.Equal(expected, FactionStandingRules.ClampFavorGain(requested, current));
    }

    [Theory]
    [InlineData(5, FactionStandingRules.MaxFavor)]
    [InlineData(5, FactionStandingRules.MaxFavor + 500)]
    public void ClampFavorGain_AtOrAboveMax_ReturnsZero(int requested, int current)
    {
        Assert.Equal(0, FactionStandingRules.ClampFavorGain(requested, current));
    }

    [Fact]
    public void ClampFavorGain_NegativeRequest_ReturnsZero()
    {
        Assert.Equal(0, FactionStandingRules.ClampFavorGain(-5, 0));
    }

    [Theory]
    [InlineData(-50)]
    [InlineData(int.MinValue)]
    public void ClampFavorGain_NegativeCurrent_CountsAsZero(int current)
    {
        Assert.Equal(
            FactionStandingRules.MaxFavor,
            FactionStandingRules.ClampFavorGain(int.MaxValue, current)
        );
    }

    [Fact]
    public void ClampReputation_WithinLimit_ReturnsRequested()
    {
        Assert.Equal(50, FactionStandingRules.ClampReputation(50, 100));
    }

    [Fact]
    public void ClampReputation_AboveLimit_ReturnsLimit()
    {
        Assert.Equal(100, FactionStandingRules.ClampReputation(150, 100));
    }

    [Theory]
    [InlineData(-1, 100)]
    [InlineData(10, -5)]
    public void ClampReputation_Negative_ReturnsZero(int requested, int total)
    {
        Assert.Equal(0, FactionStandingRules.ClampReputation(requested, total));
    }
}
