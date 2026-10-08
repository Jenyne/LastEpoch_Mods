using LastEpoch_Hud.Scripts.Core;

namespace LastEpoch_Hud.Tests.Core;

public class MasteryLimitOverrideTests
{
    [Fact]
    public void DisableRestoresTheActualNativeLimit()
    {
        var limit = new MasteryLimitOverride();
        Assert.Equal((byte)80, limit.Sync(25, 80, true));
        Assert.Equal((byte)80, limit.Sync(80, 80, true));
        Assert.Equal((byte)25, limit.Sync(80, 80, false));
        Assert.False(limit.Active);
    }

    [Fact]
    public void NativeReinitializationProvidesANewBaseline()
    {
        var limit = new MasteryLimitOverride();
        limit.Sync(25, 80, true);
        Assert.Equal((byte)80, limit.Sync(30, 80, true));
        Assert.Equal((byte)30, limit.Sync(80, 80, false));
    }

    [Fact]
    public void RestoreDoesNotOverwriteAnExternalChange()
    {
        var limit = new MasteryLimitOverride();
        limit.Sync(25, 80, true);
        Assert.Equal((byte)40, limit.Sync(40, 80, false));
    }

    [Theory]
    [InlineData(0, 80)]
    [InlineData(25, 0)]
    [InlineData(80, 80)]
    [InlineData(100, 80)]
    public void DoesNotOverrideUninitializedOrAlreadyHigherLimits(byte current, byte full)
    {
        var limit = new MasteryLimitOverride();
        Assert.Equal(current, limit.Sync(current, full, true));
        Assert.False(limit.Active);
    }

    [Fact]
    public void ChangingTheFullLimitPreservesTheOriginalForRestoration()
    {
        var limit = new MasteryLimitOverride();
        limit.Sync(25, 80, true);
        Assert.Equal((byte)100, limit.Sync(80, 100, true));
        Assert.Equal((byte)25, limit.Sync(100, 100, false));
    }
}
