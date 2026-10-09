using LastEpoch_Hud.Scripts.Core.ForceDrop;

namespace LastEpoch_Hud.Tests.Core.ForceDrop;

public sealed class ForceDropItemSearchTests
{
    [Theory]
    [InlineData("seed", "Seed of Example", "Helmet\nUnique", true)]
    [InlineData("  SEED helmet ", "Seed of Example", "Helmet\nUnique", true)]
    [InlineData("seed ring", "Seed of Example", "Helmet\nUnique", false)]
    [InlineData("seed", "Graine d'exemple", "Seed of Example\nseedInternal", true)]
    [InlineData("투구", "예시의 씨앗", "투구\n고유", true)]
    [InlineData(" ", "Anything", null, true)]
    public void SearchCombinesTranslatedNamesAliasesAndCategoryTokens(
        string query,
        string name,
        string aliases,
        bool expected
    ) => Assert.Equal(expected, ForceDropItemSearch.Matches(query, name, aliases));

    [Fact]
    public void SelectionUsesExactIdentityAcrossCategoriesAndRarities()
    {
        var selected = new ForceDropItemIdentity(5, 2, 7, 101);
        var options = new[]
        {
            (1, new ForceDropItemIdentity(6, 2, 7, 101)),
            (2, new ForceDropItemIdentity(5, 2, 8, 101)),
            (3, new ForceDropItemIdentity(5, 2, 7, 102)),
            (4, selected),
        };
        Assert.Equal(4, ForceDropItemSearch.FindOption(selected, options));
        Assert.Equal(1, ForceDropItemSearch.FindOption(selected, new[] { (1, selected) }));
    }

    [Fact]
    public void StaleAmbiguousAndPlaceholderMatchesCannotSelectAnItem()
    {
        var identity = new ForceDropItemIdentity(5, 2, 7, 101);
        Assert.Equal(-1, ForceDropItemSearch.FindOption(identity, new[] { (0, identity) }));
        Assert.Equal(
            -1,
            ForceDropItemSearch.FindOption(identity, new[] { (1, identity), (2, identity) })
        );
        Assert.Equal(
            -1,
            ForceDropItemSearch.FindOption(
                identity,
                new[] { (1, new ForceDropItemIdentity(5, 2, 7, 102)) }
            )
        );
    }
}
