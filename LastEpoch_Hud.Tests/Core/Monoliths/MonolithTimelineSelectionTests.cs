using LastEpoch_Hud.Scripts.Core.Monoliths;

namespace LastEpoch_Hud.Tests.Core.Monoliths;

public sealed class MonolithTimelineSelectionTests
{
    [Fact]
    public void Initial_CurrentSet_ReturnsCurrent()
    {
        List<MonolithRunSnapshot> runs = new() { MonolithRunFactory.Run(3, 0) };

        Assert.Equal(4, MonolithTimelineSelection.Initial(4, runs, 10));
    }

    [Fact]
    public void Initial_NoCurrent_FirstSavedInRange()
    {
        List<MonolithRunSnapshot> runs = new()
        {
            MonolithRunFactory.Run(0, 0),
            MonolithRunFactory.Run(12, 0),
            MonolithRunFactory.Run(3, 0),
            MonolithRunFactory.Run(5, 0),
        };

        Assert.Equal(3, MonolithTimelineSelection.Initial(0, runs, 10));
    }

    [Theory]
    [InlineData(10, 3, 10)]
    [InlineData(1, 5, 1)]
    [InlineData(11, 5, 5)]
    public void Initial_NoCurrent_RangeBoundsInclusive(int first, int second, int expected)
    {
        List<MonolithRunSnapshot> runs = new()
        {
            MonolithRunFactory.Run(first, 0),
            MonolithRunFactory.Run(second, 0),
        };

        Assert.Equal(expected, MonolithTimelineSelection.Initial(0, runs, 10));
    }

    [Fact]
    public void Initial_NoRunInRange_ReturnsOne()
    {
        List<MonolithRunSnapshot> runs = new()
        {
            MonolithRunFactory.Run(0, 0),
            MonolithRunFactory.Run(12, 0),
        };

        Assert.Equal(1, MonolithTimelineSelection.Initial(0, runs, 10));
    }
}
