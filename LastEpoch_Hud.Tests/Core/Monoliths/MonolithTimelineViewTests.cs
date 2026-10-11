using LastEpoch_Hud.Scripts.Core.Monoliths;

namespace LastEpoch_Hud.Tests.Core.Monoliths;

public sealed class MonolithTimelineViewTests
{
    private static readonly MonolithField[] _allFields =
    {
        MonolithField.BasicStability,
        MonolithField.EmpoweredStability,
        MonolithField.Corruption,
        MonolithField.Gaze,
    };

    [Fact]
    public void Of_NoRuns_ShowsNothing()
    {
        AssertShowsNothing(MonolithTimelineView.Of(new List<MonolithRunSnapshot>(), 1));
    }

    [Fact]
    public void Of_TimelineBelowOne_ShowsNothing()
    {
        List<MonolithRunSnapshot> runs = new()
        {
            MonolithRunFactory.Run(0, 0, 10),
            MonolithRunFactory.Run(0, 1, 20, true, 3, 4),
        };

        AssertShowsNothing(MonolithTimelineView.Of(runs, 0));
    }

    [Fact]
    public void Of_BasicOnly()
    {
        List<MonolithRunSnapshot> runs = new() { MonolithRunFactory.Run(2, 0, 10) };

        var view = MonolithTimelineView.Of(runs, 2);

        Assert.Equal(new MonolithTimelineView(true, 10, false, 0, false, 0, 0), view);
        Assert.Equal(MonolithTimelineView.Hidden, view.ValueOf(MonolithField.EmpoweredStability));
    }

    [Fact]
    public void Of_EmpoweredWithoutWeb_HidesCorruptionAndGaze()
    {
        List<MonolithRunSnapshot> runs = new() { MonolithRunFactory.Run(2, 1, 20, false, 5, 6) };

        var view = MonolithTimelineView.Of(runs, 2);

        Assert.True(view.Shows(MonolithField.EmpoweredStability));
        Assert.Equal(20, view.ValueOf(MonolithField.EmpoweredStability));
        Assert.False(view.Shows(MonolithField.Corruption));
        Assert.False(view.Shows(MonolithField.Gaze));
        Assert.Equal(MonolithTimelineView.Hidden, view.ValueOf(MonolithField.Corruption));
        Assert.Equal(MonolithTimelineView.Hidden, view.ValueOf(MonolithField.Gaze));
    }

    [Fact]
    public void Of_EmpoweredWithWeb_ShowsAll()
    {
        List<MonolithRunSnapshot> runs = new()
        {
            MonolithRunFactory.Run(2, 0, 10),
            MonolithRunFactory.Run(2, 1, 20, true, 5, 6),
        };

        var view = MonolithTimelineView.Of(runs, 2);

        Assert.Equal(new MonolithTimelineView(true, 10, true, 20, true, 5, 6), view);
        Assert.Equal(5, view.ValueOf(MonolithField.Corruption));
        Assert.Equal(6, view.ValueOf(MonolithField.Gaze));
    }

    [Fact]
    public void Of_OtherTimelinesIgnored()
    {
        List<MonolithRunSnapshot> runs = new()
        {
            MonolithRunFactory.Run(1, 0, 10),
            MonolithRunFactory.Run(3, 1, 20, true, 5, 6),
        };

        AssertShowsNothing(MonolithTimelineView.Of(runs, 2));
    }

    [Fact]
    public void Of_DuplicateRuns_FirstWins()
    {
        List<MonolithRunSnapshot> runs = new()
        {
            MonolithRunFactory.Run(1, 0, 10),
            MonolithRunFactory.Run(1, 0, 99),
        };

        Assert.Equal(10, MonolithTimelineView.Of(runs, 1).ValueOf(MonolithField.BasicStability));
    }

    [Fact]
    public void Of_DuplicateEmpoweredRuns_FirstWins()
    {
        List<MonolithRunSnapshot> runs = new()
        {
            MonolithRunFactory.Run(1, 1, 20, false, 5, 6),
            MonolithRunFactory.Run(1, 1, 99, true, 7, 8),
        };

        var view = MonolithTimelineView.Of(runs, 1);

        Assert.Equal(new MonolithTimelineView(false, 0, true, 20, false, 5, 6), view);
    }

    [Fact]
    public void Of_UnknownDifficulty_Ignored()
    {
        List<MonolithRunSnapshot> runs = new() { MonolithRunFactory.Run(1, 2, 10, true, 5, 6) };

        AssertShowsNothing(MonolithTimelineView.Of(runs, 1));
    }

    [Fact]
    public void Default_ShowsNothing()
    {
        AssertShowsNothing(default);
    }

    private static void AssertShowsNothing(MonolithTimelineView view)
    {
        foreach (MonolithField field in _allFields)
        {
            Assert.False(view.Shows(field));
            Assert.Equal(MonolithTimelineView.Hidden, view.ValueOf(field));
        }
    }
}
