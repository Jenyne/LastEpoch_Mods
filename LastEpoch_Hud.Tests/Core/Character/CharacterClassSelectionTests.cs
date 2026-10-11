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
}
