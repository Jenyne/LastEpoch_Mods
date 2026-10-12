using LastEpoch_Hud.Scripts.Core.Monoliths;
using LastEpoch_Hud.Tests.Support;

namespace LastEpoch_Hud.Tests.Core.Monoliths;

/// <summary>The timeline enum must mirror the game's TimelineID.</summary>
public sealed class MonolithTimelineGameTests
{
    [Fact]
    public void Timelines_MatchGameTimelineId()
    {
        GameEnvironment.SkipWithoutGame();
        Dictionary<string, int> ids = GameEnumIds.Read("TimelineID");

        foreach (MonolithTimeline timeline in Enum.GetValues<MonolithTimeline>())
        {
            Assert.Contains(timeline.ToString(), ids.Keys);
            Assert.Equal((int)timeline, ids[timeline.ToString()]);
        }
    }
}
