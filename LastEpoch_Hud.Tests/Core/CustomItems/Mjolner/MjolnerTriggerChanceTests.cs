using LastEpoch_Hud.Scripts.Core.CustomItems.Mjolner;

namespace LastEpoch_Hud.Tests.Core.CustomItems.Mjolner;

public sealed class MjolnerTriggerChanceTests
{
    [Fact]
    public void Probability_InRange_Unchanged()
    {
        Assert.Equal(0.25f, MjolnerTriggerChance.Probability(0.25f));
    }

    [Fact]
    public void Probability_BelowZero_ClampedToZero()
    {
        Assert.Equal(0f, MjolnerTriggerChance.Probability(-0.5f));
    }

    [Fact]
    public void Probability_AboveOne_ClampedToOne()
    {
        Assert.Equal(1f, MjolnerTriggerChance.Probability(1.5f));
    }

    [Fact]
    public void Percent_Fraction_TimesHundred()
    {
        Assert.Equal(25, MjolnerTriggerChance.Percent(0.25f));
    }

    [Theory]
    [InlineData(0.53f, 53)]
    [InlineData(0.59f, 59)]
    public void Percent_FloatError_RoundedNotTruncated(float fraction, int expected)
    {
        Assert.Equal(expected, MjolnerTriggerChance.Percent(fraction));
    }

    [Theory]
    [InlineData(-1f, 0)]
    [InlineData(2f, 100)]
    public void Percent_OutOfRange_Clamped(float fraction, int expected)
    {
        Assert.Equal(expected, MjolnerTriggerChance.Percent(fraction));
    }

    [Fact]
    public void Procs_RollBelowChance_True()
    {
        Assert.True(MjolnerTriggerChance.Procs(0.2f, 0.6f, 0.5f, 0.39f));
    }

    [Fact]
    public void Procs_RollAtOrAboveChance_False()
    {
        Assert.False(MjolnerTriggerChance.Procs(0.2f, 0.6f, 0.5f, 0.4f));
    }

    [Fact]
    public void Procs_ZeroChance_NeverProcs()
    {
        Assert.False(MjolnerTriggerChance.Procs(0f, 0f, 1f, 0f));
    }

    [Fact]
    public void Procs_ChanceTZero_UsesMin()
    {
        Assert.False(MjolnerTriggerChance.Procs(0.2f, 0.6f, 0f, 0.3f));
    }

    [Fact]
    public void Procs_ChanceTOne_UsesMax()
    {
        Assert.True(MjolnerTriggerChance.Procs(0.2f, 0.6f, 1f, 0.5f));
    }

    [Fact]
    public void Procs_MinBelowZero_ClampedBeforeLerp()
    {
        Assert.True(MjolnerTriggerChance.Procs(-1f, 1f, 0.5f, 0.25f));
    }

    [Fact]
    public void Procs_MaxAboveOne_ClampedBeforeLerp()
    {
        Assert.False(MjolnerTriggerChance.Procs(0f, 3f, 0.5f, 0.6f));
    }
}
