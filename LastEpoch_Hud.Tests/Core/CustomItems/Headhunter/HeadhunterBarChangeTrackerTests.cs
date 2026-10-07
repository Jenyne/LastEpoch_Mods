using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter;

namespace LastEpoch_Hud.Tests.Core.CustomItems.Headhunter;

public sealed class HeadhunterBarChangeTrackerTests
{
    [Fact]
    public void Update_FirstEmpty_False()
    {
        HeadhunterBarChangeTracker tracker = new();

        Assert.False(tracker.Update(Entries()));
    }

    [Fact]
    public void Update_FirstNonEmpty_True()
    {
        HeadhunterBarChangeTracker tracker = new();

        Assert.True(tracker.Update(Entries(1)));
    }

    [Fact]
    public void Update_SameIdsNewSeconds_False()
    {
        HeadhunterBarChangeTracker tracker = new();
        tracker.Update(Entries(1, 2));

        HeadhunterBarEntry[] later = [new(1, 2, 0.9f, 0, 1), new(2, 1, 0.99f, 0, 1)];

        Assert.False(tracker.Update(later));
    }

    [Fact]
    public void Update_OrderChange_True()
    {
        HeadhunterBarChangeTracker tracker = new();
        tracker.Update(Entries(1, 2));

        Assert.True(tracker.Update(Entries(2, 1)));
    }

    [Fact]
    public void Update_Shrink_True()
    {
        HeadhunterBarChangeTracker tracker = new();
        tracker.Update(Entries(1, 2));

        Assert.True(tracker.Update(Entries(1)));
    }

    [Fact]
    public void Update_BackToEmpty_True()
    {
        HeadhunterBarChangeTracker tracker = new();
        tracker.Update(Entries(1));

        Assert.True(tracker.Update(Entries()));
    }

    private static HeadhunterBarEntry[] Entries(params int[] ids)
    {
        return ids.Select(id => new HeadhunterBarEntry(id, 5, 0.5f, 0, 1)).ToArray();
    }
}
