using LastEpoch_Hud.Scripts.Core.Items;

namespace LastEpoch_Hud.Tests.Core.Items;

/// <summary>Materials are stored before a pickup only when enabled, cursor empty and no free spot.</summary>
public sealed class FullInventoryStoreRuleTests
{
    [Fact]
    public void ShouldStoreBeforePickup_EnabledCursorEmptyNoSpace_ReturnsTrue()
    {
        Assert.True(FullInventoryStoreRule.ShouldStoreBeforePickup(true, true, false));
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(false, false)]
    public void ShouldStoreBeforePickup_Disabled_ReturnsFalse(bool cursorEmpty, bool hasSpace)
    {
        Assert.False(FullInventoryStoreRule.ShouldStoreBeforePickup(false, cursorEmpty, hasSpace));
    }

    [Fact]
    public void ShouldStoreBeforePickup_HasSpace_ReturnsFalse()
    {
        Assert.False(FullInventoryStoreRule.ShouldStoreBeforePickup(true, true, true));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ShouldStoreBeforePickup_CursorHoldsItem_ReturnsFalse(bool hasSpace)
    {
        Assert.False(FullInventoryStoreRule.ShouldStoreBeforePickup(true, false, hasSpace));
    }
}
