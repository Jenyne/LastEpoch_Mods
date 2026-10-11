using LastEpoch_Hud.Scripts.Core.ForceDrop;

namespace LastEpoch_Hud.Tests.Core.ForceDrop;

public sealed class ForceDropClassSearchTests
{
    [Theory]
    [InlineData("Acolyte", 0)]
    [InlineData("Lich", 0)]
    [InlineData("Necromancer", 0)]
    [InlineData("Warlock", 0)]
    [InlineData("Mage", 1)]
    [InlineData("Sorcerer", 1)]
    [InlineData("Spellblade", 1)]
    [InlineData("Rune Master", 1)]
    [InlineData("Primalist", 2)]
    [InlineData("Druid", 2)]
    [InlineData("Shaman", 2)]
    [InlineData("Beastmaster", 2)]
    [InlineData("Rogue", 3)]
    [InlineData("Falconer", 3)]
    [InlineData("Marksman", 3)]
    [InlineData("Blade Dancer", 3)]
    [InlineData("Sentinel", 4)]
    [InlineData("Forge Guard", 4)]
    [InlineData("Void Knight", 4)]
    [InlineData("Paladin", 4)]
    public void MasteriesResolveToNativeBaseClassMask(string name, int expected)
    {
        Assert.True(ForceDropClassSearch.TryBaseClass(name, out int index));
        Assert.Equal(expected, index);
    }

    [Theory]
    [InlineData(null, 31)]
    [InlineData("", 31)]
    [InlineData("None", 31)]
    [InlineData("NonSpecific", 31)]
    [InlineData("NonSpecificClass", 31)]
    [InlineData("Acolyte", 1)]
    [InlineData("Mage", 2)]
    [InlineData("Primalist", 4)]
    [InlineData("Rogue", 8)]
    [InlineData("Sentinel", 16)]
    [InlineData("Acolyte, Primalist", 5)]
    [InlineData("Mage | Sentinel", 18)]
    [InlineData("UnknownNativeEnumValue", 31)]
    public void SearchMaskUsesClassMetadataWithoutItemCompatibility(
        string specificity,
        int expectedMask
    ) => Assert.Equal(expectedMask, ForceDropClassSearch.MaskFromSpecificity(specificity));

    [Theory]
    [InlineData("lich", 1, true)]
    [InlineData("lich", 2, false)]
    [InlineData("aco", 1, true)]
    [InlineData("aco", 4, false)]
    [InlineData("class:aco", 1, true)]
    [InlineData("-aco", 1, false)]
    [InlineData("-aco", 31, true)]
    [InlineData("shaman", 4, true)]
    [InlineData("shaman", 1, false)]
    [InlineData("lich", 0, false)]
    [InlineData("class:necromancer", 1, true)]
    [InlineData("class:paladin", 16, true)]
    [InlineData("class:forge guard", 16, true)]
    [InlineData("class:blade dancer", 8, true)]
    [InlineData("-lich", 1, false)]
    [InlineData("-lich", 2, true)]
    [InlineData("lich", 31, true)]
    [InlineData("-lich", 31, true)]
    public void ClassFilteringUsesBaseClassMasks(string query, int mask, bool expected) =>
        Assert.Equal(expected, ForceDropClassSearch.Matches(query, mask, _ => true));

    [Fact]
    public void SkeletonAffixIsAvailableForAcolyteRegardlessOfItemSubtype()
    {
        // This is a synthetic definition mask; runtime IL2CPP metadata is
        // separately verified by testing the actual Force Drop picker.
        int skeletonMask = ForceDropClassSearch.MaskFromSpecificity("Acolyte");
        Assert.True(
            ForceDropClassSearch.Matches(
                "lich",
                skeletonMask,
                query => ForceDropItemSearch.Matches(query, "+1 Skeletons", "Skeleton")
            )
        );
        Assert.True(
            ForceDropClassSearch.Matches(
                "aco skeleton",
                skeletonMask,
                query => ForceDropItemSearch.Matches(query, "+1 Skeletons", "Skeleton")
            )
        );
        Assert.False(ForceDropClassSearch.Matches("shaman", skeletonMask, _ => true));
        Assert.True(ForceDropClassSearch.Matches("lich", 31, _ => true));
    }

    [Fact]
    public void SearchCombinesMasteryTokensWithNativeLanguageNames()
    {
        Assert.True(
            ForceDropClassSearch.Matches(
                "lich 毒",
                1,
                query => ForceDropItemSearch.Matches(query, "毒 daño", "Poison")
            )
        );
        Assert.False(
            ForceDropClassSearch.Matches(
                "paladin 毒",
                1,
                query => ForceDropItemSearch.Matches(query, "毒 daño", "Poison")
            )
        );
        Assert.True(ForceDropClassSearch.Matches("class:all", 1, _ => true));
    }

    [Fact]
    public void NonAffixPickersKeepPlainTextSearch()
    {
        Assert.True(ForceDropClassSearch.Matches("lich", -1, text => text == "lich"));
    }
}
