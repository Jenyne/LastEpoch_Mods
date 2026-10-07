using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Config;

namespace LastEpoch_Hud.Tests.Core.CustomItems.Headhunter.Config;

public sealed class HeadhunterSizeCurveTests
{
    private static readonly HeadhunterSizeCurve _curve = new(2f, 20f);

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Factor_ZeroOrNegative_IsOne(int buffs)
    {
        Assert.Equal(1f, _curve.Factor(buffs));
    }

    [Theory]
    [InlineData(1, 1.02f)]
    [InlineData(3, 1.06f)]
    public void Factor_ScalesPerBuff(int buffs, float expected)
    {
        Assert.Equal(expected, _curve.Factor(buffs), 5);
    }

    [Theory]
    [InlineData(10)]
    [InlineData(11)]
    public void Factor_Capped(int buffs)
    {
        Assert.Equal(1.2f, _curve.Factor(buffs), 5);
    }

    [Fact]
    public void Factor_ZeroPerBuff_IsOne()
    {
        Assert.Equal(1f, new HeadhunterSizeCurve(0f, 20f).Factor(5));
    }
}
