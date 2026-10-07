using LastEpoch_Hud.Scripts.Core.CustomItems;

namespace LastEpoch_Hud.Tests.Core.CustomItems;

public sealed class RefreshGateTests
{
    [Fact]
    public void ShouldRefresh_FirstCall_True()
    {
        Assert.True(new RefreshGate(1.0).ShouldRefresh(0));
    }

    [Fact]
    public void ShouldRefresh_DirtyBeforeInterval_True()
    {
        var gate = new RefreshGate(1.0);
        gate.ShouldRefresh(0);
        gate.MarkDirty();

        Assert.True(gate.ShouldRefresh(0.2));
    }

    [Fact]
    public void ShouldRefresh_DirtyClearsAfterOneRefresh()
    {
        var gate = new RefreshGate(1.0);
        gate.ShouldRefresh(0);
        gate.MarkDirty();
        gate.ShouldRefresh(0.2);

        Assert.False(gate.ShouldRefresh(0.3));
    }

    [Fact]
    public void ShouldRefresh_NoDirty_PollsAtInterval()
    {
        var gate = new RefreshGate(1.0);
        gate.ShouldRefresh(0);

        Assert.False(gate.ShouldRefresh(0.5));
        Assert.True(gate.ShouldRefresh(1.0));
    }
}
