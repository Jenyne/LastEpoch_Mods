using LastEpoch_Hud.Scripts.Core.Factions;

namespace LastEpoch_Hud.Tests.Core.Factions;

/// <summary>Factions dropdown option to faction mapping.</summary>
public sealed class FactionSelectionTests
{
    [Theory]
    [InlineData(1, FactionKind.CircleOfFortune)]
    [InlineData(2, FactionKind.MerchantsGuild)]
    [InlineData(3, FactionKind.ForgottenKnights)]
    [InlineData(4, FactionKind.TheWeaver)]
    public void TryKindAt_FactionOption_ReturnsKind(int option, FactionKind expected)
    {
        bool found = FactionSelection.TryKindAt(option, out FactionKind kind);

        Assert.True(found);
        Assert.Equal(expected, kind);
    }

    [Fact]
    public void TryKindAt_SelectPlaceholder_ReturnsFalse()
    {
        bool found = FactionSelection.TryKindAt(0, out FactionKind _);

        Assert.False(found);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(5)]
    [InlineData(int.MaxValue)]
    public void TryKindAt_OutOfRange_ReturnsFalse(int option)
    {
        bool found = FactionSelection.TryKindAt(option, out FactionKind _);

        Assert.False(found);
    }

    [Fact]
    public void OptionLabels_PlaceholderPlusEveryKind()
    {
        int count = FactionSelection.OptionLabels.Count;
        var kinds = new List<FactionKind>();
        for (int i = 1; i < count; i++)
        {
            Assert.True(FactionSelection.TryKindAt(i, out FactionKind kind));
            kinds.Add(kind);
        }

        Assert.Equal(Enum.GetValues<FactionKind>().Length + 1, count);
        Assert.Equal(kinds.Count, kinds.Distinct().Count());
        Assert.False(FactionSelection.TryKindAt(count, out FactionKind _));
    }

    [Fact]
    public void OptionLabels_MatchLabelOfKindAtIndex()
    {
        for (int i = 1; i < FactionSelection.OptionLabels.Count; i++)
        {
            Assert.True(FactionSelection.TryKindAt(i, out FactionKind kind));
            Assert.Equal(FactionSelection.LabelOf(kind), FactionSelection.OptionLabels[i]);
        }
    }

    [Fact]
    public void LabelOf_EveryKind_NonEmptyAndDistinct()
    {
        FactionKind[] kinds = Enum.GetValues<FactionKind>();
        string[] labels = kinds.Select(FactionSelection.LabelOf).ToArray();

        Assert.All(labels, label => Assert.False(string.IsNullOrEmpty(label)));
        Assert.Equal(kinds.Length, labels.Distinct().Count());
    }

    [Fact]
    public void LabelOf_UndefinedKind_ReturnsEmpty()
    {
        Assert.Equal(string.Empty, FactionSelection.LabelOf((FactionKind)999));
    }
}
