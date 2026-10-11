using LastEpoch_Hud.Scripts.Core.Items.AutoLoot;

namespace LastEpoch_Hud.Tests.Core.Items.AutoLoot;

public sealed class FilterLootPlanTests
{
    [Fact]
    public void IsEmpty_Default_True()
    {
        Assert.True(default(FilterLootPlan).IsEmpty);
    }

    [Theory]
    [InlineData(true, false, false)]
    [InlineData(false, true, false)]
    [InlineData(false, false, true)]
    public void IsEmpty_AnyAction_False(bool pickUp, bool shatter, bool sell)
    {
        Assert.False(new FilterLootPlan(pickUp, shatter, sell).IsEmpty);
    }
}
