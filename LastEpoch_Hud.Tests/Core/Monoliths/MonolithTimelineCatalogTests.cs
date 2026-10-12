using LastEpoch_Hud.Scripts.Core.Monoliths;

namespace LastEpoch_Hud.Tests.Core.Monoliths;

public sealed class MonolithTimelineCatalogTests
{
    [Fact]
    public void All_IsEveryDefinedTimelineOnceInIdOrder()
    {
        Assert.Equal(
            Enum.GetValues<MonolithTimeline>(),
            MonolithTimelineCatalog.All.Select(o => o.Timeline)
        );
    }

    [Fact]
    public void All_LabelsNonEmptyAndDistinct()
    {
        Assert.All(
            MonolithTimelineCatalog.All,
            o => Assert.False(string.IsNullOrWhiteSpace(o.Label))
        );
        Assert.Equal(
            MonolithTimelineCatalog.All.Count,
            MonolithTimelineCatalog.All.Select(o => o.Label).Distinct().Count()
        );
    }

    [Fact]
    public void TryGet_EveryOption_ReturnsIt()
    {
        foreach (MonolithTimelineOption option in MonolithTimelineCatalog.All)
        {
            Assert.True(
                MonolithTimelineCatalog.TryGet(option.TimelineId, out MonolithTimelineOption found)
            );
            Assert.Equal(option, found);
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(11)]
    [InlineData(99)]
    public void TryGet_Unknown_ReturnsFalse(int id)
    {
        Assert.False(MonolithTimelineCatalog.TryGet(id, out _));
    }
}
