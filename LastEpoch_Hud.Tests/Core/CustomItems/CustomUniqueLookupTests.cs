using LastEpoch_Hud.Scripts.Core.CustomItems;

namespace LastEpoch_Hud.Tests.Core.CustomItems;

public sealed class CustomUniqueLookupTests
{
    [Theory]
    [InlineData(500)]
    [InlineData(501)]
    [InlineData(502)]
    [InlineData(503)]
    public void IndexOf_CustomId_ReturnsItsSpec(int uniqueId)
    {
        int index = CustomUniqueLookup.IndexOf(uniqueId);

        Assert.Equal((ushort)uniqueId, CustomUniqueSpecs.All[index].UniqueId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(22)]
    [InlineData(499)]
    [InlineData(504)]
    [InlineData(-1)]
    public void IndexOf_OtherId_ReturnsMinusOne(int uniqueId)
    {
        Assert.Equal(-1, CustomUniqueLookup.IndexOf(uniqueId));
    }

    [Theory]
    [InlineData("assets/headhunter/texture2d/icon.png", "Headhunter")]
    [InlineData("assets/mjolnir/texture2d/mjolner.png", "Mjolner")]
    [InlineData("assets/sandsofsilk/texture2d/icon.png", "Sands of Silk")]
    [InlineData("assets/essentiasanguis/texture2d/icon.png", "Essentia Sanguis")]
    public void IconIndexOf_BundlePath_ReturnsItsItem(string assetName, string name)
    {
        int index = CustomUniqueLookup.IconIndexOf(assetName);

        Assert.Equal(name, CustomUniqueSpecs.All[index].Name);
    }

    [Fact]
    public void IconIndexOf_BackslashesAndCase_Match()
    {
        int index = CustomUniqueLookup.IconIndexOf("Assets\\Headhunter\\Texture2D\\Icon.png");

        Assert.Equal("Headhunter", CustomUniqueSpecs.All[index].Name);
    }

    [Theory]
    [InlineData("assets/headhunter/texture2d/buffs_icons/armour.png")]
    [InlineData("assets/headhunter/prefab/screen.png")]
    [InlineData("assets/headhunter/hh_buffs.json")]
    [InlineData("assets/x/notmjolner.png")]
    [InlineData("")]
    [InlineData(null)]
    public void IconIndexOf_OtherAsset_ReturnsMinusOne(string assetName)
    {
        Assert.Equal(-1, CustomUniqueLookup.IconIndexOf(assetName));
    }

    [Theory]
    [InlineData(4, 15, 503, 0, 22)]
    [InlineData(1, 71, 502, 0, 7)]
    public void VisualSource_EsAndSos_ReturnBorrowedItem(
        int equipmentType,
        int subType,
        int uniqueId,
        int sourceSubType,
        int sourceUniqueId
    )
    {
        CustomItemVisualSource source = CustomUniqueLookup.VisualSource(
            equipmentType,
            subType,
            uniqueId
        );

        Assert.NotNull(source);
        Assert.Equal(sourceSubType, source.SubType);
        Assert.Equal((ushort)sourceUniqueId, source.UniqueId);
    }

    [Theory]
    [InlineData(1, 15, 503)]
    [InlineData(4, 0, 503)]
    [InlineData(2, 0, 500)]
    [InlineData(7, 10, 501)]
    [InlineData(4, 0, 22)]
    [InlineData(4, 15, 0)]
    [InlineData(1, 71, 0)]
    [InlineData(4, 15, 22)]
    public void VisualSource_NoMatch_ReturnsNull(int equipmentType, int subType, int uniqueId)
    {
        Assert.Null(CustomUniqueLookup.VisualSource(equipmentType, subType, uniqueId));
    }
}
