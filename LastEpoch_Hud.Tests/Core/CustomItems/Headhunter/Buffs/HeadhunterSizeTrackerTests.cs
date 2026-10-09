using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Buffs;

namespace LastEpoch_Hud.Tests.Core.CustomItems.Headhunter.Buffs;

public sealed class HeadhunterSizeTrackerTests
{
    [Fact]
    public void Next_Initial_NormalNoModel_None()
    {
        Assert.Equal(HeadhunterSizeAction.None, new HeadhunterSizeTracker().Next(0, 1f, true));
    }

    [Fact]
    public void Next_NewFactor_RescaleThenNone()
    {
        var tracker = new HeadhunterSizeTracker();

        HeadhunterSizeAction first = tracker.Next(7, 1.02f, true);
        HeadhunterSizeAction second = tracker.Next(7, 1.02f, true);

        Assert.Equal(HeadhunterSizeAction.Rescale, first);
        Assert.Equal(HeadhunterSizeAction.None, second);
    }

    [Fact]
    public void Next_ModelChanges_RestoreThenRescale()
    {
        HeadhunterSizeTracker tracker = Applied();

        Assert.Equal(HeadhunterSizeAction.RestoreThenRescale, tracker.Next(8, 1.02f, true));
    }

    [Fact]
    public void Next_FactorBackToOne_RestoreThenRescale()
    {
        HeadhunterSizeTracker tracker = Applied();

        Assert.Equal(HeadhunterSizeAction.RestoreThenRescale, tracker.Next(7, 1f, true));
    }

    [Fact]
    public void Next_ScaleNotIntact_Rescale()
    {
        HeadhunterSizeTracker tracker = Applied();

        HeadhunterSizeAction broken = tracker.Next(7, 1.02f, false);
        HeadhunterSizeAction after = tracker.Next(7, 1.02f, true);

        Assert.Equal(HeadhunterSizeAction.Rescale, broken);
        Assert.Equal(HeadhunterSizeAction.None, after);
    }

    [Fact]
    public void Next_NotIntactOnModelChange_NoRestore()
    {
        HeadhunterSizeTracker tracker = Applied();

        Assert.Equal(HeadhunterSizeAction.Rescale, tracker.Next(8, 1.02f, false));
    }

    [Fact]
    public void ShouldRestore_AppliedAndIntact_True()
    {
        Assert.True(Applied().ShouldRestore(true));
    }

    [Fact]
    public void ShouldRestore_NotIntact_False()
    {
        Assert.False(Applied().ShouldRestore(false));
    }

    [Fact]
    public void ShouldRestore_Fresh_False()
    {
        Assert.False(new HeadhunterSizeTracker().ShouldRestore(true));
    }

    [Fact]
    public void ShouldRestore_FactorOne_False()
    {
        var tracker = new HeadhunterSizeTracker();
        tracker.Next(7, 1f, true);

        Assert.False(tracker.ShouldRestore(true));
    }

    [Fact]
    public void Reset_ThenSameValues_Rescale()
    {
        HeadhunterSizeTracker tracker = Applied();
        tracker.Reset();

        Assert.Equal(HeadhunterSizeAction.Rescale, tracker.Next(7, 1.02f, true));
    }

    [Fact]
    public void Reset_ThenNormalNoModel_None()
    {
        HeadhunterSizeTracker tracker = Applied();
        tracker.Reset();

        Assert.Equal(HeadhunterSizeAction.None, tracker.Next(0, 1f, true));
    }

    [Fact]
    public void Reset_ThenShouldRestore_False()
    {
        HeadhunterSizeTracker tracker = Applied();
        tracker.Reset();

        Assert.False(tracker.ShouldRestore(true));
    }

    [Fact]
    public void Next_ModelAppears_RescaleNoRestore()
    {
        var tracker = new HeadhunterSizeTracker();
        tracker.Next(0, 1.02f, true);

        Assert.Equal(HeadhunterSizeAction.Rescale, tracker.Next(7, 1.02f, true));
    }

    [Fact]
    public void Next_FactorGrows_RestoreThenRescale()
    {
        HeadhunterSizeTracker tracker = Applied();

        Assert.Equal(HeadhunterSizeAction.RestoreThenRescale, tracker.Next(7, 1.04f, true));
    }

    private static HeadhunterSizeTracker Applied()
    {
        var tracker = new HeadhunterSizeTracker();
        tracker.Next(7, 1.02f, true);
        return tracker;
    }
}
