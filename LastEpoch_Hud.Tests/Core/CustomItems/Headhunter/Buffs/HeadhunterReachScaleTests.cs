using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Buffs;

namespace LastEpoch_Hud.Tests.Core.CustomItems.Headhunter.Buffs;

public sealed class HeadhunterReachScaleTests
{
    [Fact]
    public void Fresh_IsNormal_ScaleUnchanged()
    {
        var scale = new HeadhunterReachScale();

        Assert.True(scale.IsNormal);
        Assert.Equal(3.7f, scale.Scale(3.7f));
    }

    [Theory]
    [InlineData(1.1f, 2f, 2.2f)]
    [InlineData(1.2f, 5f, 6f)]
    public void Set_Grown_ScalesProportionally(float factor, float range, float expected)
    {
        var scale = new HeadhunterReachScale();

        scale.Set(factor);

        Assert.False(scale.IsNormal);
        Assert.Equal(expected, scale.Scale(range), 5);
    }

    [Fact]
    public void Set_One_IsNormal()
    {
        var scale = new HeadhunterReachScale();
        scale.Set(1.1f);

        scale.Set(1f);

        Assert.True(scale.IsNormal);
        Assert.Equal(2f, scale.Scale(2f));
    }

    [Fact]
    public void ScaleFor_Player_Scaled()
    {
        var scale = new HeadhunterReachScale();
        scale.Set(1.1f);

        float result = scale.ScaleFor((IntPtr)42, (IntPtr)42, 2f);

        Assert.Equal(2.2f, result, 5);
    }

    [Fact]
    public void ScaleFor_OtherActor_Unchanged()
    {
        var scale = new HeadhunterReachScale();
        scale.Set(1.1f);

        float result = scale.ScaleFor((IntPtr)7, (IntPtr)42, 2f);

        Assert.Equal(2f, result);
    }

    [Fact]
    public void ScaleFor_NoPlayer_Unchanged()
    {
        var scale = new HeadhunterReachScale();
        scale.Set(1.1f);

        float result = scale.ScaleFor(IntPtr.Zero, IntPtr.Zero, 2f);

        Assert.Equal(2f, result);
    }

    [Fact]
    public void ScaleFor_PlayerAtNormal_Unchanged()
    {
        var scale = new HeadhunterReachScale();

        float result = scale.ScaleFor((IntPtr)42, (IntPtr)42, 3.7f);

        Assert.Equal(3.7f, result);
    }

    [Fact]
    public void Reset_BackToNormal()
    {
        var scale = new HeadhunterReachScale();
        scale.Set(1.2f);

        scale.Reset();

        Assert.True(scale.IsNormal);
        Assert.Equal(1f, scale.Factor);
    }
}
