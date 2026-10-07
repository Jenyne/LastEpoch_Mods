using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Buffs;
using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Config;

namespace LastEpoch_Hud.Tests.Core.CustomItems.Headhunter.Buffs;

public sealed class HeadhunterValueGrowthTests
{
    private static readonly HeadhunterGrowthCurve _curve = new(10f, 25f);

    [Fact]
    public void Update_FirstStack_FalseFactorOne()
    {
        var growth = new HeadhunterValueGrowth(_curve);

        Assert.False(growth.Update(1));
        Assert.Equal(1, growth.AppliedTotal);
        Assert.Equal(1f, growth.Factor);
    }

    [Fact]
    public void Update_SameTotal_False()
    {
        var growth = new HeadhunterValueGrowth(_curve);
        growth.Update(3);

        Assert.False(growth.Update(3));
    }

    [Fact]
    public void Update_FactorChanges_TrueAndStores()
    {
        var growth = new HeadhunterValueGrowth(_curve);
        growth.Update(1);

        Assert.True(growth.Update(2));
        Assert.Equal(2, growth.AppliedTotal);
        Assert.Equal(1.1f, growth.Factor, 0.0001f);
    }

    [Fact]
    public void Update_OverCap_FalseButTotalStored()
    {
        var growth = new HeadhunterValueGrowth(new HeadhunterGrowthCurve(10f, 10f));
        growth.Update(2);

        Assert.False(growth.Update(3));
        Assert.Equal(3, growth.AppliedTotal);
    }

    [Fact]
    public void Update_GrowthOff_False()
    {
        var growth = new HeadhunterValueGrowth(HeadhunterGrowthCurve.None);
        growth.Update(1);

        Assert.False(growth.Update(5));
    }

    [Fact]
    public void Reset_BackToOne()
    {
        var growth = new HeadhunterValueGrowth(_curve);
        growth.Update(3);

        growth.Reset();

        Assert.Equal(0, growth.AppliedTotal);
        Assert.Equal(1f, growth.Factor);
    }
}
