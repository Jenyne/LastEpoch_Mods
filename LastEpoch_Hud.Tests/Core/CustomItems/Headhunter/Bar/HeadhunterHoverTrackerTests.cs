using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Bar;

namespace LastEpoch_Hud.Tests.Core.CustomItems.Headhunter.Bar;

public sealed class HeadhunterHoverTrackerTests
{
    [Fact]
    public void Changed_Initial_None_False()
    {
        HeadhunterHoverTracker tracker = new();

        Assert.False(tracker.Changed(-1, 0, 0, 0));
    }

    [Fact]
    public void Changed_NewStat_True()
    {
        HeadhunterHoverTracker tracker = new();

        Assert.True(tracker.Changed(3, 1, 0, 0));
    }

    [Fact]
    public void Changed_Same_False()
    {
        HeadhunterHoverTracker tracker = new();
        tracker.Changed(3, 1, 1, 0);

        Assert.False(tracker.Changed(3, 1, 1, 0));
    }

    [Fact]
    public void Changed_NewLayoutVersion_True()
    {
        HeadhunterHoverTracker tracker = new();
        tracker.Changed(3, 1, 1, 0);

        Assert.True(tracker.Changed(3, 1, 2, 0));
    }

    [Fact]
    public void Changed_BackToNone_True()
    {
        HeadhunterHoverTracker tracker = new();
        tracker.Changed(3, 1, 1, 0);

        Assert.True(tracker.Changed(-1, 0, 1, 0));
    }

    [Fact]
    public void Changed_True_WhenStacksChange()
    {
        HeadhunterHoverTracker tracker = new();
        tracker.Changed(3, 1, 1, 0);

        Assert.True(tracker.Changed(3, 2, 1, 0));
    }

    [Fact]
    public void Changed_GrowthTotalDiffers_True()
    {
        HeadhunterHoverTracker tracker = new();
        tracker.Changed(3, 1, 1, 2);

        Assert.True(tracker.Changed(3, 1, 1, 3));
    }
}
