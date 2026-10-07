using LastEpoch_Hud.Scripts.Core.CustomItems;

namespace LastEpoch_Hud.Tests.Core.CustomItems;

public sealed class IntervalGateTests
{
    [Fact]
    public void IsDue_FirstCall_True()
    {
        Assert.True(new IntervalGate(1.0).IsDue(0));
    }

    [Fact]
    public void IsDue_BeforeInterval_False()
    {
        var gate = new IntervalGate(1.0);
        gate.IsDue(0);

        Assert.False(gate.IsDue(0.5));
    }

    [Fact]
    public void IsDue_AtInterval_True()
    {
        var gate = new IntervalGate(1.0);
        gate.IsDue(0);

        Assert.True(gate.IsDue(1.0));
    }

    [Fact]
    public void IsDue_AfterSkip_SchedulesFromNow()
    {
        var gate = new IntervalGate(1.0);
        gate.IsDue(0);

        Assert.True(gate.IsDue(5));
        Assert.False(gate.IsDue(5.5));
    }
}
