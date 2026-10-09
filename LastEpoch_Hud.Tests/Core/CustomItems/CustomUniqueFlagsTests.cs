using LastEpoch_Hud.Scripts.Core.CustomItems;

namespace LastEpoch_Hud.Tests.Core.CustomItems;

public sealed class CustomUniqueFlagsTests
{
    [Fact]
    public void NoSettings_IsLegendaryPotentialAndDropsRandomly()
    {
        Assert.Equal(new CustomUniqueFlags(false, true, true), CustomUniqueFlags.NoSettings);
    }
}
