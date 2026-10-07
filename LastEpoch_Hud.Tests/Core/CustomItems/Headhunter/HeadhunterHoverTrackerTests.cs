using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter;

namespace LastEpoch_Hud.Tests.Core.CustomItems.Headhunter;

public sealed class HeadhunterHoverTrackerTests
{
    [Fact]
    public void Changed_Initial_None_False()
    {
        HeadhunterHoverTracker tracker = new();

        Assert.False(tracker.Changed(-1, 0));
    }

    [Fact]
    public void Changed_NewStat_True()
    {
        HeadhunterHoverTracker tracker = new();

        Assert.True(tracker.Changed(3, 0));
    }

    [Fact]
    public void Changed_Same_False()
    {
        HeadhunterHoverTracker tracker = new();
        tracker.Changed(3, 1);

        Assert.False(tracker.Changed(3, 1));
    }

    [Fact]
    public void Changed_NewLayoutVersion_True()
    {
        HeadhunterHoverTracker tracker = new();
        tracker.Changed(3, 1);

        Assert.True(tracker.Changed(3, 2));
    }

    [Fact]
    public void Changed_BackToNone_True()
    {
        HeadhunterHoverTracker tracker = new();
        tracker.Changed(3, 1);

        Assert.True(tracker.Changed(-1, 1));
    }
}
