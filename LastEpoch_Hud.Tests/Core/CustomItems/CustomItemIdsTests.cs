using LastEpoch_Hud.Scripts.Core.CustomItems;

namespace LastEpoch_Hud.Tests.Core.CustomItems;

public sealed class CustomItemIdsTests
{
    [Fact]
    public void PickBaseId_FixedFree_ReturnsFixed()
    {
        Assert.Equal(15, CustomItemIds.PickBaseId(15, 15, Range(15)));
    }

    [Fact]
    public void PickBaseId_FixedTaken_ReturnsNone()
    {
        Assert.Equal(CustomItemIds.None, CustomItemIds.PickBaseId(15, 20, Range(20)));
    }

    [Fact]
    public void PickBaseId_FixedAboveByte_ReturnsNone()
    {
        Assert.Equal(CustomItemIds.None, CustomItemIds.PickBaseId(256, 0, Range(0)));
    }

    [Fact]
    public void PickBaseId_FixedMaxByte_ReturnsFixed()
    {
        Assert.Equal(255, CustomItemIds.PickBaseId(255, 0, Range(0)));
    }

    [Fact]
    public void PickBaseId_FixedZeroOnEmpty_ReturnsZero()
    {
        Assert.Equal(0, CustomItemIds.PickBaseId(0, 0, Range(0)));
    }

    [Fact]
    public void PickBaseId_Allocate_ReturnsCount()
    {
        Assert.Equal(13, CustomItemIds.PickBaseId(CustomUniqueSpec.AllocateBaseId, 13, Range(13)));
    }

    [Fact]
    public void PickBaseId_AllocateCountTaken_ReturnsNextFree()
    {
        var used = new HashSet<int> { 0, 1, 3, 4 };

        Assert.Equal(5, CustomItemIds.PickBaseId(CustomUniqueSpec.AllocateBaseId, 3, used));
    }

    [Fact]
    public void PickBaseId_OtherNegative_ReturnsNone()
    {
        Assert.Equal(CustomItemIds.None, CustomItemIds.PickBaseId(-2, 3, Range(3)));
    }

    [Fact]
    public void PickBaseId_AllocateLastFree_Returns255()
    {
        Assert.Equal(
            255,
            CustomItemIds.PickBaseId(CustomUniqueSpec.AllocateBaseId, 255, Range(255))
        );
    }

    [Fact]
    public void PickBaseId_AllocateAboveByte_ReturnsNone()
    {
        Assert.Equal(
            CustomItemIds.None,
            CustomItemIds.PickBaseId(CustomUniqueSpec.AllocateBaseId, 256, Range(256))
        );
    }

    private static HashSet<int> Range(int count)
    {
        return Enumerable.Range(0, count).ToHashSet();
    }
}
