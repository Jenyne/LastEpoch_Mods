using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Config;

namespace LastEpoch_Hud.Tests.Core.CustomItems.Headhunter.Config;

public sealed class HeadhunterAuraCurveTests
{
    private static readonly HeadhunterAuraCurve _curve = new(true, 0.1f, 1f);

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Strength_ZeroOrNegative_Off(int buffs)
    {
        Assert.Equal(HeadhunterAuraCurve.Off, _curve.Strength(buffs));
    }

    [Fact]
    public void Strength_Disabled_Off()
    {
        var curve = new HeadhunterAuraCurve(false, 0.1f, 1f);

        Assert.Equal(HeadhunterAuraCurve.Off, curve.Strength(3));
    }

    [Theory]
    [InlineData(1, 0.1f)]
    [InlineData(3, 0.3f)]
    public void Strength_ScalesPerBuff(int buffs, float expected)
    {
        Assert.Equal(expected, _curve.Strength(buffs), 5);
    }

    [Theory]
    [InlineData(10)]
    [InlineData(12)]
    public void Strength_Capped(int buffs)
    {
        Assert.Equal(1f, _curve.Strength(buffs), 5);
    }
}
