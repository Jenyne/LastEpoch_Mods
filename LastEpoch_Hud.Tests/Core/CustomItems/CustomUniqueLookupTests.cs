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

    [Fact]
    public void VisualSource_RegisteredEs_ReturnsEsVisual()
    {
        CustomItemVisualSource source = CustomUniqueLookup.VisualSource(4, 40, 503, FilledTable());

        Assert.Same(CustomUniqueSpecs.EssentiaSanguis.VisualSource, source);
    }

    [Fact]
    public void VisualSource_RegisteredSos_ReturnsSosVisual()
    {
        CustomItemVisualSource source = CustomUniqueLookup.VisualSource(1, 41, 502, FilledTable());

        Assert.Same(CustomUniqueSpecs.SandsOfSilk.VisualSource, source);
    }

    [Theory]
    [InlineData(4, 40, 503)]
    [InlineData(4, -1, 503)]
    public void VisualSource_NotRegistered_ReturnsNull(int equipmentType, int subType, int uniqueId)
    {
        Assert.Null(
            CustomUniqueLookup.VisualSource(
                equipmentType,
                subType,
                uniqueId,
                new CustomUniqueSubtypes()
            )
        );
    }

    [Theory]
    [InlineData(4, 15, 503)]
    [InlineData(1, 40, 503)]
    [InlineData(4, 41, 503)]
    [InlineData(4, 40, 22)]
    [InlineData(4, 40, 0)]
    [InlineData(2, 42, 500)]
    [InlineData(7, 10, 501)]
    public void VisualSource_NoMatch_ReturnsNull(int equipmentType, int subType, int uniqueId)
    {
        Assert.Null(
            CustomUniqueLookup.VisualSource(equipmentType, subType, uniqueId, FilledTable())
        );
    }

    private static CustomUniqueSubtypes FilledTable()
    {
        var table = new CustomUniqueSubtypes();
        table.Set(503, 40);
        table.Set(502, 41);
        table.Set(500, 42);
        table.Set(501, 10);
        return table;
    }
}
