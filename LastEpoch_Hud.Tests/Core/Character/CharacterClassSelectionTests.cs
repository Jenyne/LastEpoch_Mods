using LastEpoch_Hud.Scripts.Core.Character;

namespace LastEpoch_Hud.Tests.Core.Character;

public sealed class CharacterClassSelectionTests
{
    [Fact]
    public void OptionLabels_OnePerClassKind()
    {
        Assert.Equal(
            Enum.GetValues<CharacterClassKind>().Length,
            CharacterClassSelection.OptionLabels.Count
        );
        Assert.Equal(
            CharacterClassSelection.OptionLabels.Count,
            CharacterClassSelection.OptionLabels.Distinct().Count()
        );
        Assert.All(CharacterClassSelection.OptionLabels, label => Assert.NotEmpty(label));
    }

    [Fact]
    public void OptionLabels_FollowClassIdOrder_AndNameTheKind()
    {
        List<int> ids = new();
        for (int i = 0; i < CharacterClassSelection.OptionLabels.Count; i++)
        {
            Assert.True(CharacterClassSelection.TryClassIdAt(i, out int id));
            Assert.Equal(
                ((CharacterClassKind)id).ToString(),
                CharacterClassSelection.OptionLabels[i]
            );
            ids.Add(id);
        }

        Assert.Equal(ids.OrderBy(id => id).ToList(), ids);
        Assert.Equal(
            Enum.GetValues<CharacterClassKind>().Select(kind => (int)kind).OrderBy(id => id),
            ids
        );
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(int.MaxValue)]
    public void TryClassIdAt_OutOfRange_ReturnsFalse(int optionIndex)
    {
        Assert.False(CharacterClassSelection.TryClassIdAt(optionIndex, out int id));
        Assert.Equal(0, id);
    }

    [Fact]
    public void TryClassIdAt_Count_ReturnsFalse()
    {
        Assert.False(
            CharacterClassSelection.TryClassIdAt(
                CharacterClassSelection.OptionLabels.Count,
                out int id
            )
        );
        Assert.Equal(0, id);
    }

    [Fact]
    public void OptionIndexOf_RoundTripsEveryOption()
    {
        for (int i = 0; i < CharacterClassSelection.OptionLabels.Count; i++)
        {
            CharacterClassSelection.TryClassIdAt(i, out int id);
            Assert.Equal(i, CharacterClassSelection.OptionIndexOf(id));
        }
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(99)]
    public void OptionIndexOf_UnknownId_ReturnsZero(int classId)
    {
        Assert.Equal(0, CharacterClassSelection.OptionIndexOf(classId));
    }

    [Fact]
    public void LabelAt_PrefersLocalizedName()
    {
        Assert.Equal("loc", CharacterClassSelection.LabelAt(0, "loc", "cls"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void LabelAt_BlankLocalized_UsesClassName(string localized)
    {
        Assert.Equal("cls", CharacterClassSelection.LabelAt(0, localized, "cls"));
    }

    [Fact]
    public void LabelAt_NoGameNames_UsesOptionLabel()
    {
        for (int i = 0; i < CharacterClassSelection.OptionLabels.Count; i++)
        {
            Assert.Equal(
                CharacterClassSelection.OptionLabels[i],
                CharacterClassSelection.LabelAt(i, null, " ")
            );
        }
    }

    [Theory]
    [InlineData(-1, null, null)]
    [InlineData(-1, "loc", "cls")]
    [InlineData(int.MaxValue, "loc", "cls")]
    public void LabelAt_OutOfRange_ReturnsEmpty(int optionIndex, string localized, string className)
    {
        Assert.Equal(
            string.Empty,
            CharacterClassSelection.LabelAt(optionIndex, localized, className)
        );
    }

    [Fact]
    public void LabelAt_Count_ReturnsEmpty()
    {
        Assert.Equal(
            string.Empty,
            CharacterClassSelection.LabelAt(
                CharacterClassSelection.OptionLabels.Count,
                "loc",
                "cls"
            )
        );
    }

    [Theory]
    [InlineData(false, false, false, false)]
    [InlineData(false, false, true, true)]
    [InlineData(false, true, false, false)]
    [InlineData(false, true, true, false)]
    [InlineData(true, false, false, true)]
    [InlineData(true, false, true, true)]
    [InlineData(true, true, false, true)]
    [InlineData(true, true, true, true)]
    public void ShouldRebuildOptions_Table(
        bool localeChanged,
        bool builtFromGame,
        bool gameListLoaded,
        bool expected
    )
    {
        Assert.Equal(
            expected,
            CharacterClassSelection.ShouldRebuildOptions(
                localeChanged,
                builtFromGame,
                gameListLoaded
            )
        );
    }
}
