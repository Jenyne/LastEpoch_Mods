using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Buffs;

namespace LastEpoch_Hud.Tests.Core.CustomItems.Headhunter.Buffs;

public sealed class HeadhunterAuraTrackerTests
{
    [Fact]
    public void Next_Initial_Off_None()
    {
        Assert.Equal(HeadhunterAuraAction.None, new HeadhunterAuraTracker().Next(0f, true, 0));
    }

    [Fact]
    public void Next_FirstStrength_ApplyThenNone()
    {
        var tracker = new HeadhunterAuraTracker();

        HeadhunterAuraAction first = tracker.Next(0.1f, true, 7);
        HeadhunterAuraAction second = tracker.Next(0.1f, true, 7);

        Assert.Equal(HeadhunterAuraAction.Apply, first);
        Assert.Equal(HeadhunterAuraAction.None, second);
    }

    [Fact]
    public void Next_StrengthChanges_Retint()
    {
        HeadhunterAuraTracker tracker = Applied();

        Assert.Equal(HeadhunterAuraAction.Retint, tracker.Next(0.2f, true, 7));
    }

    [Fact]
    public void Next_KeyChanges_Rebuild()
    {
        HeadhunterAuraTracker tracker = Applied();

        Assert.Equal(HeadhunterAuraAction.Rebuild, tracker.Next(0.1f, true, 8));
    }

    [Fact]
    public void Next_KeyAndStrengthChange_Rebuild()
    {
        HeadhunterAuraTracker tracker = Applied();

        Assert.Equal(HeadhunterAuraAction.Rebuild, tracker.Next(0.2f, true, 8));
    }

    [Fact]
    public void Next_StrengthZero_RemoveThenNone()
    {
        HeadhunterAuraTracker tracker = Applied();

        HeadhunterAuraAction first = tracker.Next(0f, true, 7);
        HeadhunterAuraAction second = tracker.Next(0f, true, 7);

        Assert.Equal(HeadhunterAuraAction.Remove, first);
        Assert.Equal(HeadhunterAuraAction.None, second);
    }

    [Fact]
    public void Next_NotReadyWhileOn_RemoveThenApply()
    {
        HeadhunterAuraTracker tracker = Applied();

        HeadhunterAuraAction removed = tracker.Next(0.1f, false, 7);
        HeadhunterAuraAction back = tracker.Next(0.1f, true, 9);

        Assert.Equal(HeadhunterAuraAction.Remove, removed);
        Assert.Equal(HeadhunterAuraAction.Apply, back);
    }

    [Fact]
    public void Next_NotReadyWhileOff_None()
    {
        Assert.Equal(HeadhunterAuraAction.None, new HeadhunterAuraTracker().Next(0.1f, false, 7));
    }

    [Fact]
    public void Reset_ThenSameValues_Apply()
    {
        HeadhunterAuraTracker tracker = Applied();

        tracker.Reset();

        Assert.Equal(HeadhunterAuraAction.Apply, tracker.Next(0.1f, true, 7));
    }

    private static HeadhunterAuraTracker Applied()
    {
        var tracker = new HeadhunterAuraTracker();
        tracker.Next(0.1f, true, 7);
        return tracker;
    }
}
