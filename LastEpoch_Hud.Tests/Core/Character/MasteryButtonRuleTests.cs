using LastEpoch_Hud.Scripts.Core.Character;

namespace LastEpoch_Hud.Tests.Core.Character;

public sealed class MasteryButtonRuleTests
{
    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(255)]
    public void ShouldReset_ChosenMastery_ReturnsTrue(int chosenMastery)
    {
        Assert.True(MasteryButtonRule.ShouldReset(chosenMastery));
    }

    [Fact]
    public void ShouldReset_NoMastery_ReturnsFalse()
    {
        Assert.False(MasteryButtonRule.ShouldReset(0));
    }

    [Fact]
    public void ShouldReset_Negative_ReturnsFalse()
    {
        Assert.False(MasteryButtonRule.ShouldReset(-1));
    }
}
