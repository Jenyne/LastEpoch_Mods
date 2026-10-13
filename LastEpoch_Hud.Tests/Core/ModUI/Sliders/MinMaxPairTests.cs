using LastEpoch_Hud.Scripts.Core.ModUI.Sliders;

namespace LastEpoch_Hud.Tests.Core.ModUI.Sliders;

public sealed class MinMaxPairTests
{
    [Fact]
    public void WithMin_AboveMax_RaisesMax() =>
        Assert.Equal(new MinMaxPair(7, 7), new MinMaxPair(2, 5).WithMin(7));

    [Fact]
    public void WithMin_BelowMax_KeepsMax() =>
        Assert.Equal(new MinMaxPair(3, 5), new MinMaxPair(2, 5).WithMin(3));

    [Fact]
    public void WithMax_BelowMin_LowersMin() =>
        Assert.Equal(new MinMaxPair(1, 1), new MinMaxPair(2, 5).WithMax(1));

    [Fact]
    public void WithMax_AboveMin_KeepsMin() =>
        Assert.Equal(new MinMaxPair(2, 4), new MinMaxPair(2, 5).WithMax(4));

    [Fact]
    public void WithMin_EqualToMax_KeepsPair() =>
        Assert.Equal(new MinMaxPair(5, 5), new MinMaxPair(2, 5).WithMin(5));
}
