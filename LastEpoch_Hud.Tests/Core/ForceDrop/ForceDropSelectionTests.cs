using LastEpoch_Hud.Scripts.Core.ForceDrop;

namespace LastEpoch_Hud.Tests.Core.ForceDrop;

public sealed class ForceDropSelectionTests
{
    [Theory]
    [InlineData(ForceDropRarity.Base, 0)]
    [InlineData(ForceDropRarity.Unique, 7)]
    [InlineData(ForceDropRarity.Set, 8)]
    public void GameRarity_MapsRarityToGameByte(ForceDropRarity rarity, int expected)
    {
        Assert.Equal(expected, ForceDropSelection.GameRarity(rarity));
    }

    [Fact]
    public void Rarities_ListBaseUniqueSetInFixedOrder_WhenSetComesBeforeUnique()
    {
        ForceDropUniqueEntry[] uniques =
        {
            Entry(1, 5, true, new[] { 0 }, "A"),
            Entry(2, 5, false, new[] { 0 }, "B"),
        };

        List<ForceDropRarity> result = ForceDropSelection.Rarities(5, uniques);

        Assert.Equal(
            new[] { ForceDropRarity.Base, ForceDropRarity.Unique, ForceDropRarity.Set },
            result
        );
    }

    [Fact]
    public void Rarities_ReturnOnlyBase_WhenNoUniques()
    {
        List<ForceDropRarity> result = ForceDropSelection.Rarities(
            5,
            new List<ForceDropUniqueEntry>()
        );

        Assert.Equal(new[] { ForceDropRarity.Base }, result);
    }

    [Fact]
    public void Rarities_IgnoreOtherTypesAndEmptySubtypes()
    {
        ForceDropUniqueEntry[] uniques =
        {
            Entry(1, 5, false, Array.Empty<int>(), "A"),
            Entry(2, 5, true, null, "B"),
            Entry(3, 9, false, new[] { 0 }, "C"),
        };

        List<ForceDropRarity> result = ForceDropSelection.Rarities(5, uniques);

        Assert.Equal(new[] { ForceDropRarity.Base }, result);
    }

    [Fact]
    public void Rarities_ReturnEmpty_WhenTypeBelowZero()
    {
        ForceDropUniqueEntry[] uniques = { Entry(1, 5, false, new[] { 0 }, "A") };

        List<ForceDropRarity> result = ForceDropSelection.Rarities(-1, uniques);

        Assert.Empty(result);
    }

    [Fact]
    public void Items_ListBaseItemsOfTypeInOrder_WhenRarityBase()
    {
        ForceDropItemOption[] bases =
        {
            BaseItem(5, 0, "A"),
            BaseItem(6, 0, "X"),
            BaseItem(5, 1, "B"),
        };

        List<ForceDropItemOption> result = ForceDropSelection.Items(
            5,
            ForceDropRarity.Base,
            bases,
            new[] { Entry(10, 5, false, new[] { 0 }, "U") }
        );

        Assert.Equal(new[] { bases[0], bases[2] }, result);
    }

    [Fact]
    public void Items_KeepUniqueAndSetWithSameLabelApart()
    {
        ForceDropUniqueEntry[] uniques =
        {
            Entry(10, 5, false, new[] { 0 }, "A"),
            Entry(11, 5, true, new[] { 0 }, "A"),
        };
        var noBases = new List<ForceDropItemOption>();

        List<ForceDropItemOption> asUnique = ForceDropSelection.Items(
            5,
            ForceDropRarity.Unique,
            noBases,
            uniques
        );
        List<ForceDropItemOption> asSet = ForceDropSelection.Items(
            5,
            ForceDropRarity.Set,
            noBases,
            uniques
        );

        Assert.Equal(new[] { 10 }, asUnique.ConvertAll(o => o.UniqueId));
        Assert.Equal(new[] { 11 }, asSet.ConvertAll(o => o.UniqueId));
    }

    [Fact]
    public void Items_KeepDistinctIds_WhenLabelsRepeat()
    {
        ForceDropUniqueEntry[] uniques =
        {
            Entry(10, 5, false, new[] { 0 }, "A"),
            Entry(12, 5, false, new[] { 0 }, "A"),
        };

        List<ForceDropItemOption> result = ForceDropSelection.Items(
            5,
            ForceDropRarity.Unique,
            new List<ForceDropItemOption>(),
            uniques
        );

        Assert.Equal(new[] { 10, 12 }, result.ConvertAll(o => o.UniqueId));
    }

    [Fact]
    public void Items_ListOnlyUniquesOfType_WhenOtherTypesPresent()
    {
        ForceDropUniqueEntry[] uniques =
        {
            Entry(10, 5, false, new[] { 0 }, "A"),
            Entry(11, 9, false, new[] { 0 }, "B"),
        };
        ForceDropItemOption[] bases = { BaseItem(5, 0, "C") };

        List<ForceDropItemOption> result = ForceDropSelection.Items(
            5,
            ForceDropRarity.Unique,
            bases,
            uniques
        );

        Assert.Equal(new[] { 10 }, result.ConvertAll(o => o.UniqueId));
    }

    [Theory]
    [InlineData("A", "b", "A")]
    [InlineData("", "b", "b")]
    [InlineData(" ", "b", "b")]
    [InlineData(null, "b", "b")]
    [InlineData(null, null, "")]
    public void CatalogLabel_FallsBackToInternalName(
        string display,
        string internalName,
        string expected
    )
    {
        Assert.Equal(expected, ForceDropSelection.CatalogLabel(display, internalName));
    }

    [Fact]
    public void Rarities_ReturnBaseAndSet_WhenOnlySetItems()
    {
        ForceDropUniqueEntry[] uniques = { Entry(1, 5, true, new[] { 0 }, "A") };

        List<ForceDropRarity> result = ForceDropSelection.Rarities(5, uniques);

        Assert.Equal(new[] { ForceDropRarity.Base, ForceDropRarity.Set }, result);
    }

    [Fact]
    public void Rarities_ReturnBaseAndUnique_WhenOnlyUniqueItems()
    {
        ForceDropUniqueEntry[] uniques = { Entry(2, 5, false, new[] { 0 }, "B") };

        List<ForceDropRarity> result = ForceDropSelection.Rarities(5, uniques);

        Assert.Equal(new[] { ForceDropRarity.Base, ForceDropRarity.Unique }, result);
    }

    [Fact]
    public void Items_UseEntryBaseTypeAndFirstSubtype_WhenUnique()
    {
        ForceDropUniqueEntry[] uniques = { Entry(10, 5, false, new[] { 4, 9 }, "A") };

        List<ForceDropItemOption> result = ForceDropSelection.Items(
            5,
            ForceDropRarity.Unique,
            new List<ForceDropItemOption>(),
            uniques
        );

        Assert.Equal(
            new[] { new ForceDropItemOption(5, 4, ForceDropRarity.Unique, 10, "A") },
            result
        );
    }

    [Fact]
    public void Items_SkipEntriesWithoutSubtypes_AndKeepOrder()
    {
        ForceDropUniqueEntry[] uniques =
        {
            Entry(10, 5, false, new[] { 0 }, "A"),
            Entry(11, 5, false, Array.Empty<int>(), "B"),
            Entry(12, 5, false, null, "C"),
            Entry(13, 5, false, new[] { 2 }, "D"),
        };

        List<ForceDropItemOption> result = ForceDropSelection.Items(
            5,
            ForceDropRarity.Unique,
            new List<ForceDropItemOption>(),
            uniques
        );

        Assert.Equal(new[] { 10, 13 }, result.ConvertAll(o => o.UniqueId));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(5)]
    [InlineData(6)]
    public void TryOptionAt_ReturnsFalse_WhenPlaceholderOrOutOfRange(int index)
    {
        int[] options = { 7, 8, 9, 10 };

        Assert.False(ForceDropSelection.TryOptionAt(options, index, out _));
    }

    [Theory]
    [InlineData(1, 7)]
    [InlineData(4, 10)]
    public void TryOptionAt_ReturnsOptionBeforeIndex_WhenInRange(int index, int expected)
    {
        int[] options = { 7, 8, 9, 10 };

        Assert.True(ForceDropSelection.TryOptionAt(options, index, out int option));
        Assert.Equal(expected, option);
    }

    private static ForceDropUniqueEntry Entry(
        int id,
        int baseType,
        bool isSet,
        int[] subTypes,
        string label
    ) => new(id, baseType, isSet, subTypes, label);

    private static ForceDropItemOption BaseItem(int baseType, int subType, string label) =>
        new(baseType, subType, ForceDropRarity.Base, 0, label);
}
