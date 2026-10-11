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

    [Fact]
    public void ScoreEither_MatchesLocalizedKoreanText()
    {
        int score = HudSearchText.ScoreEither(
            "신 모드",
            "God Mode",
            "Cheats",
            "Character",
            "Utilities",
            "신 모드",
            "치트",
            "캐릭터",
            "유틸리티"
        );

        Assert.Equal(0, score);
    }

    [Fact]
    public void ScoreEither_KeepsEnglishAsFallbackInLocalizedUi()
    {
        int score = HudSearchText.ScoreEither(
            "god",
            "God Mode",
            "Cheats",
            "Character",
            "Utilities",
            "신 모드",
            "치트",
            "캐릭터",
            "유틸리티"
        );

        Assert.Equal(10, score);
    }

    [Fact]
    public void Normalize_IsAccentInsensitive()
    {
        Assert.Equal(HudSearchText.Normalize("Dégâts"), HudSearchText.Normalize("degats"));
    }
}
