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
}
