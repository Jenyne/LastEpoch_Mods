using LastEpoch_Hud.Scripts.Core.Monoliths;

namespace LastEpoch_Hud.Tests.Core.Monoliths;

public sealed class MonolithEditPlanTests
{
    [Fact]
    public void ForValue_TargetsFirstMatchingRun()
    {
        List<MonolithRunSnapshot> runs = new()
        {
            MonolithRunFactory.Run(1, 0),
            MonolithRunFactory.Run(1, 1, hasEchoWeb: true),
            MonolithRunFactory.Run(2, 0),
        };

        IReadOnlyList<MonolithEdit> edits = MonolithEditPlan.ForValue(
            runs,
            1,
            MonolithField.EmpoweredStability,
            50
        );

        Assert.Equal(new[] { new MonolithEdit(1, 1, MonolithField.EmpoweredStability, 50) }, edits);
    }

    [Fact]
    public void ForValue_DuplicateRuns_TargetsFirst()
    {
        List<MonolithRunSnapshot> runs = new()
        {
            MonolithRunFactory.Run(1, 1, 10),
            MonolithRunFactory.Run(1, 1, 20),
        };

        IReadOnlyList<MonolithEdit> edits = MonolithEditPlan.ForValue(
            runs,
            1,
            MonolithField.EmpoweredStability,
            50
        );

        Assert.Equal(new[] { new MonolithEdit(0, 1, MonolithField.EmpoweredStability, 50) }, edits);
    }

    [Fact]
    public void ForValue_Corruption_SkipsRunWithoutWeb()
    {
        List<MonolithRunSnapshot> runs = new()
        {
            MonolithRunFactory.Run(1, 1),
            MonolithRunFactory.Run(1, 1, hasEchoWeb: true),
        };

        IReadOnlyList<MonolithEdit> edits = MonolithEditPlan.ForValue(
            runs,
            1,
            MonolithField.Corruption,
            7
        );

        Assert.Equal(new[] { new MonolithEdit(1, 1, MonolithField.Corruption, 7) }, edits);
    }

    [Fact]
    public void ForValue_NoMatchingRun_ReturnsEmpty()
    {
        List<MonolithRunSnapshot> runs = new() { MonolithRunFactory.Run(1, 0) };

        Assert.Empty(MonolithEditPlan.ForValue(runs, 2, MonolithField.BasicStability, 5));
    }

    [Fact]
    public void ForValue_CorruptionWithoutWeb_ReturnsEmpty()
    {
        List<MonolithRunSnapshot> runs = new() { MonolithRunFactory.Run(1, 1) };

        Assert.Empty(MonolithEditPlan.ForValue(runs, 1, MonolithField.Corruption, 5));
    }

    [Fact]
    public void ForValue_TimelineBelowOne_ReturnsEmpty()
    {
        List<MonolithRunSnapshot> runs = new() { MonolithRunFactory.Run(0, 0) };

        Assert.Empty(MonolithEditPlan.ForValue(runs, 0, MonolithField.BasicStability, 5));
    }

    [Fact]
    public void CopyToAll_CopiesShownFieldsToMatchingRuns()
    {
        List<MonolithRunSnapshot> runs = new()
        {
            MonolithRunFactory.Run(1, 0, 100),
            MonolithRunFactory.Run(1, 1, 200, true, 300, 4),
            MonolithRunFactory.Run(2, 0, 5),
            MonolithRunFactory.Run(2, 1, 6),
            MonolithRunFactory.Run(3, 1, 7, true, 8, 9),
        };

        IReadOnlyList<MonolithEdit> edits = MonolithEditPlan.CopyToAll(runs, 1);

        MonolithEdit[] expected =
        {
            new(0, 1, MonolithField.BasicStability, 100),
            new(1, 1, MonolithField.EmpoweredStability, 200),
            new(1, 1, MonolithField.Corruption, 300),
            new(1, 1, MonolithField.Gaze, 4),
            new(2, 2, MonolithField.BasicStability, 100),
            new(3, 2, MonolithField.EmpoweredStability, 200),
            new(4, 3, MonolithField.EmpoweredStability, 200),
            new(4, 3, MonolithField.Corruption, 300),
            new(4, 3, MonolithField.Gaze, 4),
        };
        Assert.Equal(expected, edits);
    }

    [Fact]
    public void CopyToAll_SourceBasicOnly_LeavesEmpoweredRuns()
    {
        List<MonolithRunSnapshot> runs = new()
        {
            MonolithRunFactory.Run(1, 0, 100),
            MonolithRunFactory.Run(2, 1, 6, true, 1, 2),
        };

        IReadOnlyList<MonolithEdit> edits = MonolithEditPlan.CopyToAll(runs, 1);

        Assert.Equal(new[] { new MonolithEdit(0, 1, MonolithField.BasicStability, 100) }, edits);
    }

    [Fact]
    public void CopyToAll_SourceWithoutWeb_CopiesOnlyEmpoweredStability()
    {
        List<MonolithRunSnapshot> runs = new()
        {
            MonolithRunFactory.Run(1, 1, 200, false, 300, 4),
            MonolithRunFactory.Run(2, 1, 6, true, 1, 2),
        };

        IReadOnlyList<MonolithEdit> edits = MonolithEditPlan.CopyToAll(runs, 1);

        MonolithEdit[] expected =
        {
            new(0, 1, MonolithField.EmpoweredStability, 200),
            new(1, 2, MonolithField.EmpoweredStability, 200),
        };
        Assert.Equal(expected, edits);
    }

    [Fact]
    public void CopyToAll_SourceWithoutRuns_ReturnsEmpty()
    {
        List<MonolithRunSnapshot> runs = new() { MonolithRunFactory.Run(2, 0, 5) };

        Assert.Empty(MonolithEditPlan.CopyToAll(runs, 1));
    }

    [Fact]
    public void CopyToAll_SourceBelowOne_ReturnsEmpty()
    {
        List<MonolithRunSnapshot> runs = new()
        {
            MonolithRunFactory.Run(0, 0, 5),
            MonolithRunFactory.Run(1, 0, 5),
        };

        Assert.Empty(MonolithEditPlan.CopyToAll(runs, 0));
    }

    [Fact]
    public void RaisedMaxCorruption_NoCorruptionEdits_ReturnsCurrent()
    {
        MonolithEdit[] edits =
        {
            new(0, 1, MonolithField.Gaze, 900),
            new(0, 1, MonolithField.BasicStability, 900),
        };

        Assert.Equal(400, MonolithEditPlan.RaisedMaxCorruption(400, edits));
    }

    [Theory]
    [InlineData(400, 500)]
    [InlineData(600, 600)]
    public void RaisedMaxCorruption_CorruptionEdits_ReturnsMax(int current, int expected)
    {
        MonolithEdit[] edits =
        {
            new(0, 1, MonolithField.Corruption, 300),
            new(1, 2, MonolithField.Corruption, 500),
        };

        Assert.Equal(expected, MonolithEditPlan.RaisedMaxCorruption(current, edits));
    }

    [Theory]
    [InlineData(2, 1, true)]
    [InlineData(2, 0, false)]
    [InlineData(3, 1, false)]
    public void Touches_TimelineAndDifficulty(int timelineId, int difficulty, bool expected)
    {
        MonolithEdit[] edits = { new(0, 2, MonolithField.EmpoweredStability, 5) };

        Assert.Equal(expected, MonolithEditPlan.Touches(edits, timelineId, difficulty));
    }
}
