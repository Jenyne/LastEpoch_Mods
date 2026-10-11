using LastEpoch_Hud.Scripts.Core.Monoliths;

namespace LastEpoch_Hud.Tests.Core.Monoliths;

public sealed class MonolithFieldsTests
{
    [Theory]
    [InlineData(MonolithField.BasicStability, 0)]
    [InlineData(MonolithField.EmpoweredStability, 1)]
    [InlineData(MonolithField.Corruption, 1)]
    [InlineData(MonolithField.Gaze, 1)]
    public void DifficultyOf_PerField(MonolithField field, int expected)
    {
        Assert.Equal(expected, MonolithFields.DifficultyOf(field));
    }

    [Theory]
    [InlineData(MonolithField.BasicStability, 0, false, true)]
    [InlineData(MonolithField.BasicStability, 1, false, false)]
    [InlineData(MonolithField.EmpoweredStability, 1, false, true)]
    [InlineData(MonolithField.Corruption, 1, false, false)]
    [InlineData(MonolithField.Corruption, 1, true, true)]
    [InlineData(MonolithField.Gaze, 1, false, false)]
    [InlineData(MonolithField.Gaze, 1, true, true)]
    [InlineData(MonolithField.BasicStability, 2, true, false)]
    [InlineData(MonolithField.EmpoweredStability, 2, true, false)]
    [InlineData(MonolithField.Corruption, 2, true, false)]
    [InlineData(MonolithField.Gaze, 2, true, false)]
    public void AppliesTo_Run(MonolithField field, int difficulty, bool hasEchoWeb, bool expected)
    {
        MonolithRunSnapshot run = MonolithRunFactory.Run(1, difficulty, hasEchoWeb: hasEchoWeb);

        Assert.Equal(expected, MonolithFields.AppliesTo(field, run));
    }
}
