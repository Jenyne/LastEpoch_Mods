using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter;

namespace LastEpoch_Hud.Tests.Core.CustomItems.Headhunter;

public sealed class HeadhunterAssetLoadGateTests
{
    private const long BundleA = 11;
    private const long BundleB = 12;

    [Fact]
    public void ShouldLoad_FirstCallWithBundle_True()
    {
        var gate = new HeadhunterAssetLoadGate();

        Assert.True(gate.ShouldLoad(BundleA));
    }

    [Fact]
    public void ShouldLoad_WithoutMissingMark_StaysTrue()
    {
        var gate = new HeadhunterAssetLoadGate();

        Assert.True(gate.ShouldLoad(BundleA));
        Assert.True(gate.ShouldLoad(BundleA));
    }

    [Fact]
    public void ShouldLoad_BundleLacksPrefabs_FalseOnSameBundle()
    {
        var gate = new HeadhunterAssetLoadGate();
        gate.MarkMissing(BundleA);

        Assert.False(gate.ShouldLoad(BundleA));
        Assert.False(gate.ShouldLoad(BundleA));
    }

    [Fact]
    public void ShouldLoad_BundleChangedAfterMissing_True()
    {
        var gate = new HeadhunterAssetLoadGate();
        gate.MarkMissing(BundleA);

        Assert.True(gate.ShouldLoad(BundleB));
    }

    [Fact]
    public void ShouldLoad_NoBundle_FalseAndNotAnAttempt()
    {
        var gate = new HeadhunterAssetLoadGate();

        Assert.False(gate.ShouldLoad(0));
        Assert.True(gate.ShouldLoad(BundleA));
    }
}
