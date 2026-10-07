using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Config;

namespace LastEpoch_Hud.Tests.Core.CustomItems.Headhunter.Config;

public sealed class HeadhunterGrowthCurveTests
{
    private static readonly HeadhunterGrowthCurve _curve = new(10f, 25f);

    [Fact]
    public void Factor_ZeroTotal_IsOne()
    {
        Assert.Equal(1f, _curve.Factor(0));
    }

    [Fact]
    public void Factor_OneStack_IsExactlyOne()
    {
        Assert.Equal(1f, _curve.Factor(1));
    }

    [Fact]
    public void Factor_TwoStacks_AddsOnePerStack()
    {
        Assert.Equal(1.1f, _curve.Factor(2), 0.0001f);
    }

    [Fact]
    public void Factor_ThreeStacks_AddsTwoPerStack()
    {
        Assert.Equal(1.2f, _curve.Factor(3), 0.0001f);
    }

    [Theory]
    [InlineData(4)]
    [InlineData(100)]
    public void Factor_OverCap_IsCapped(int total)
    {
        Assert.Equal(1.25f, _curve.Factor(total), 0.0001f);
    }

    [Fact]
    public void Factor_None_IsOne()
    {
        Assert.Equal(1f, HeadhunterGrowthCurve.None.Factor(50));
    }

    [Fact]
    public void Factor_NegativeTotal_IsOne()
    {
        Assert.Equal(1f, _curve.Factor(-3));
    }
}
