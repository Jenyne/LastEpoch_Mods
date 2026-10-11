using LastEpoch_Hud.Scripts.Core.ForceDrop;

namespace LastEpoch_Hud.Tests.Core.ForceDrop;

public sealed class ForceDropBuildGateTests
{
    [Fact]
    public void ShouldBuild_ViewNotReady_ReturnsTrue()
    {
        var gate = new ForceDropBuildGate();

        Assert.True(gate.ShouldBuild(false));
    }

    [Fact]
    public void ShouldBuild_ViewReady_ReturnsFalse()
    {
        var gate = new ForceDropBuildGate();

        Assert.False(gate.ShouldBuild(true));
    }

    [Fact]
    public void ShouldBuild_NotReadyTwice_StaysTrue()
    {
        var gate = new ForceDropBuildGate();

        Assert.True(gate.ShouldBuild(false));
        Assert.True(gate.ShouldBuild(false));
    }

    [Fact]
    public void ShouldBuild_AfterMarkFailed_ReturnsFalseOnLaterFrames()
    {
        var gate = new ForceDropBuildGate();
        gate.MarkFailed();

        Assert.False(gate.ShouldBuild(false));
        Assert.False(gate.ShouldBuild(false));
        Assert.False(gate.ShouldBuild(false));
    }
}
