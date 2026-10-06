using LastEpoch_Hud.Scripts.Core.CustomItems;

namespace LastEpoch_Hud.Tests.Core.CustomItems;

public sealed class CustomItemKeysTests
{
    [Fact]
    public void SubtypeName_FormatsTypeAndId()
    {
        Assert.Equal("Item_SubType_Name_2_13", CustomItemKeys.SubtypeName(2, 13));
    }

    [Fact]
    public void UniqueName_Formats()
    {
        Assert.Equal("Unique_Name_500", CustomItemKeys.UniqueName(500));
    }

    [Fact]
    public void UniqueTooltip_UsesLineZero()
    {
        Assert.Equal("Unique_Tooltip_0_500", CustomItemKeys.UniqueTooltip(500));
    }

    [Fact]
    public void UniqueLore_Formats()
    {
        Assert.Equal("Unique_Lore_500", CustomItemKeys.UniqueLore(500));
    }
}
