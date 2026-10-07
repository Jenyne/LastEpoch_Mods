using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Buffs;
using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Config.Resolve;

namespace LastEpoch_Hud.Tests.Core.CustomItems.Headhunter.Buffs;

public sealed class HeadhunterLiveRowsTests
{
    private static readonly HeadhunterBuffStat[] _stats =
    [
        new(2, "HH_FakeB", 0f, 0f),
        new(3, "HH_FakeC", 0f, 0f),
    ];

    [Fact]
    public void Fill_AddsRowIndices_NotStatIds()
    {
        HashSet<int> rows = [];

        HeadhunterLiveRows.Fill(_stats, name => name == "HH_FakeC", rows);

        Assert.Equal([1], rows);
    }

    [Fact]
    public void Fill_ClearsPrevious()
    {
        HashSet<int> rows = [0, 5];

        HeadhunterLiveRows.Fill(_stats, name => name == "HH_FakeC", rows);

        Assert.Equal([1], rows);
    }

    [Fact]
    public void Fill_Empty_WhenNothingLive()
    {
        HashSet<int> rows = [0];

        HeadhunterLiveRows.Fill(_stats, _ => false, rows);

        Assert.Empty(rows);
    }
}
