using System.Globalization;
using LastEpoch_Hud.Scripts.Core.CustomItems;

namespace LastEpoch_Hud.Tests.Core.CustomItems;

public sealed class MjolnerDescriptionTests
{
    private const string EnglishProc =
        "If you have at least 105 Strength and 95 Intelligence, 50 to 100% chance to Trigger a Lightning Spell on Hit with an Attack";
    private const string FrenchProc =
        "Si vous avez au moins 105 de Force et 95 d'Intelligence, 50 à 100% de chance de déclencher un sort de foudre lors d'une attaque réussie";
    private const string GermanProc =
        "Wenn Sie mindestens 105 Stärke und 95 Intelligenz haben, 50 bis 100% Chance, bei Treffer mit einem Angriff einen Blitzzauber auszulösen";
    private const string PortugueseProc =
        "Se você tiver pelo menos 105 de Força e 95 de Inteligência, ganhe 50 a 100% de chance para Ativar uma Magia de Raio ao Acertar um Ataque";
    private const string EnglishSocketed =
        "If you have at least 105 Strength and 95 Intelligence, Trigger Lightning Nova, Smite and Static on Hit, with a 2 second Cooldown";
    private const string FrenchSocketed =
        "Si vous avez au moins 105 de Force et 95 d'Intelligence, déclenche Lightning Nova, Smite et Static à l'impact, avec un temps de recharge de 2 seconde";

    [Theory]
    [InlineData("en", EnglishProc)]
    [InlineData("ru", EnglishProc)]
    [InlineData("ko", EnglishProc)]
    [InlineData("pl", EnglishProc)]
    [InlineData("zh", EnglishProc)]
    [InlineData("es", EnglishProc)]
    [InlineData("xx", EnglishProc)]
    [InlineData("fr", FrenchProc)]
    [InlineData("de", GermanProc)]
    [InlineData("pt", PortugueseProc)]
    public void LightningProc_Language_MatchesLegacyText(string language, string expected)
    {
        Assert.Equal(expected, Proc(language));
    }

    [Theory]
    [InlineData("fr", FrenchSocketed)]
    [InlineData("en", EnglishSocketed)]
    [InlineData("de", EnglishSocketed)] // German is not translated, English on purpose
    [InlineData("ru", EnglishSocketed)]
    [InlineData("pt", EnglishSocketed)]
    [InlineData("ko", EnglishSocketed)]
    [InlineData("pl", EnglishSocketed)]
    [InlineData("zh", EnglishSocketed)]
    [InlineData("es", EnglishSocketed)]
    [InlineData("xx", EnglishSocketed)]
    public void SocketedSkills_Language_MatchesLegacyText(string language, string expected)
    {
        Assert.Equal(expected, Socketed(language));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void LightningProc_MissingLanguage_ReturnsEmpty(string language)
    {
        Assert.Equal("", Proc(language));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void SocketedSkills_MissingLanguage_ReturnsEmpty(string language)
    {
        Assert.Equal("", Socketed(language));
    }

    [Fact]
    public void LightningProc_Chance_ScaledFrom255AndTruncated()
    {
        Assert.Equal(
            "If you have at least 105 Strength and 95 Intelligence, 50 to 0% chance to Trigger a Lightning Spell on Hit with an Attack",
            Proc("en", 130f, 2f)
        );
    }

    [Fact]
    public void SocketedSkills_FractionalCooldown_InSeconds()
    {
        CultureInfo previous = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
        try
        {
            Assert.Equal(
                "If you have at least 105 Strength and 95 Intelligence, Trigger Lightning Nova, Smite and Static on Hit, with a 0.25 second Cooldown",
                Socketed("en", 250)
            );
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
        }
    }

    private static string Proc(string language, float min = 127.5f, float max = 255f) =>
        MjolnerDescription.LightningProc(language, 105, 95, min, max);

    private static string Socketed(string language, double cooldownMs = 2000) =>
        MjolnerDescription.SocketedSkills(
            language,
            105,
            95,
            cooldownMs,
            "Lightning Nova",
            "Smite",
            "Static"
        );
}
