using LastEpoch_Hud.Scripts.Core.ModUI;

namespace LastEpoch_Hud.Tests.Core.ModUI;

public sealed class HudSearchTextTests
{
    [Theory]
    [InlineData("god mode", 0)]
    [InlineData("god", 10)]
    [InlineData("mode", 20)]
    public void Score_PrioritizesDirectLabelMatches(string query, int expected)
    {
        Assert.Equal(
            expected,
            HudSearchText.Score(query, "God Mode", "Cheats", "Character", "Utilities")
        );
    }

    [Fact]
    public void Score_MatchesTermsAcrossSettingAndLocation()
    {
        int score = HudSearchText.Score(
            "character god",
            "God Mode",
            "Cheats",
            "Character",
            "Utilities"
        );

        Assert.Equal(70, score);
    }

    [Fact]
    public void Score_MatchesCardNameAndReturnsEveryCardEntry()
    {
        int score = HudSearchText.Score(
            "cheats",
            "No Cooldown",
            "Cheats",
            "Character",
            "Utilities"
        );

        Assert.Equal(50, score);
    }

    [Fact]
    public void Score_RejectsUnrelatedSetting()
    {
        Assert.Equal(-1, HudSearchText.Score("god", "Strength", "Buffs", "Buffs", "Utilities"));
    }

    [Fact]
    public void Normalize_IsCaseAndPunctuationInsensitive()
    {
        Assert.Equal("mana regeneration", HudSearchText.Normalize("  MANA--Regeneration!  "));
    }
}
