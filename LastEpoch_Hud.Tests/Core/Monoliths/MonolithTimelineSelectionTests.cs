using LastEpoch_Hud.Scripts.Core.Monoliths;

namespace LastEpoch_Hud.Tests.Core.Monoliths;

public sealed class MonolithTimelineSelectionTests
{
    [Fact]
    public void Initial_FirstSavedRunInCatalog()
    {
        List<MonolithRunSnapshot> runs = new()
        {
            MonolithRunFactory.Run(0, 0),
            MonolithRunFactory.Run(99, 0),
            MonolithRunFactory.Run(5, 0),
            MonolithRunFactory.Run(3, 0),
        };

        Assert.Equal(5, MonolithTimelineSelection.Initial(runs));
    }

    [Fact]
    public void Initial_EveryCatalogTimeline_Accepted()
    {
        foreach (MonolithTimelineOption option in MonolithTimelineCatalog.All)
        {
            List<MonolithRunSnapshot> runs = new()
            {
                MonolithRunFactory.Run(0, 0),
                MonolithRunFactory.Run(option.TimelineId, 0),
            };

            Assert.Equal(option.TimelineId, MonolithTimelineSelection.Initial(runs));
        }
    }

    [Fact]
    public void Initial_NoRunInCatalog_ReturnsFirstOption()
    {
        List<MonolithRunSnapshot> runs = new()
        {
            MonolithRunFactory.Run(0, 0),
            MonolithRunFactory.Run(12, 0),
        };

        Assert.Equal(
            MonolithTimelineCatalog.All[0].TimelineId,
            MonolithTimelineSelection.Initial(runs)
        );
    }

    [Fact]
    public void Initial_NoRuns_ReturnsFirstOption()
    {
        Assert.Equal(
            MonolithTimelineCatalog.All[0].TimelineId,
            MonolithTimelineSelection.Initial(new List<MonolithRunSnapshot>())
        );
    }
}
