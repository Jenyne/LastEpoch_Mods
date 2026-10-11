using LastEpoch_Hud.Scripts.Core.Character;
using LastEpoch_Hud.Tests.Support;

namespace LastEpoch_Hud.Tests.Core.Character;

/// <summary>The class enum must mirror the game's CharacterClassID.</summary>
public sealed class CharacterClassGameTests
{
    [Fact]
    public void Classes_MatchGameCharacterClassId()
    {
        GameEnvironment.SkipWithoutGame();
        Dictionary<string, int> ids = GameEnumIds.Read("CharacterClassID");

        foreach (CharacterClassKind kind in Enum.GetValues<CharacterClassKind>())
        {
            Assert.Contains(kind.ToString(), ids.Keys);
            Assert.Equal((int)kind, ids[kind.ToString()]);
        }

        Assert.Equal(Enum.GetValues<CharacterClassKind>().Length, ids.Count);
    }
}
