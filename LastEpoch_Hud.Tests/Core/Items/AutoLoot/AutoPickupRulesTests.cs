using LastEpoch_Hud.Scripts.Core.Items.AutoLoot;

namespace LastEpoch_Hud.Tests.Core.Items.AutoLoot;

public sealed class AutoPickupRulesTests
{
    [Theory]
    [InlineData(0, 3)]
    [InlineData(2, 3)]
    public void PotionHasRoom_BelowMax_True(int current, int max)
    {
        Assert.True(AutoPickupRules.PotionHasRoom(current, max));
    }

    [Fact]
    public void PotionHasRoom_AtMax_False()
    {
        Assert.False(AutoPickupRules.PotionHasRoom(3, 3));
    }

    [Fact]
    public void PotionHasRoom_AboveMax_False()
    {
        Assert.False(AutoPickupRules.PotionHasRoom(4, 3));
    }

    [Fact]
    public void PotionHasRoom_NoCharges_False()
    {
        Assert.False(AutoPickupRules.PotionHasRoom(0, 0));
    }
}
