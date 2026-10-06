using LastEpoch_Hud.Scripts.Core.CustomItems;

namespace LastEpoch_Hud.Tests.Core.CustomItems;

/// <summary>Per-item texts match the language switches they replace.</summary>
public sealed class CustomItemTextsTests
{
    private const string HeadhunterLoreEn =
        "A man's soul rules from a cavern of bone, learns and\r\njudges through flesh-born windows. The heart is meat.\r\nThe head is where the Man is.\"\r\n- Lavianga, Advisor to Kaom";
    private const string HeadhunterLoreFr =
        "L'âme d'un homme règne depuis une caverne d'os,\r\napprend et juge à travers des fenêtres plantées dans la chair.\r\nLe cœur est un morceau de viande. La tête est le siège de l'homme.\r\n- Lavianga, conseiller de Kaom";
    private const string HeadhunterLoreDe =
        "Die Seele eines Mannes regiert\r\naus einer Höhle aus Knochen,\r\nlernt und urteilt aus Fenstern,\r\ngeboren aus Fleisch. Das Herz ist Fleisch.\r\nDer Kopf ist dort, wo der Mann ist.\r\n– Lavianga, Berater von Kaom";
    private const string MjolnerLoreEn = "Look the storm in the eye and you will have its respect.";

    [Theory]
    [InlineData("en", "HH Leather belt")]
    [InlineData("fr", "HH Ceinture en cuir")]
    [InlineData("de", "HH Ledergürtel")]
    [InlineData("ru", "HH Ремень")]
    [InlineData("pt", "Cinto de Couro")]
    [InlineData("ko", "HH Leather belt")]
    [InlineData("pl", "HH Leather belt")]
    [InlineData("zh", "HH Leather belt")]
    [InlineData("es", "HH Leather belt")]
    public void Headhunter_SubtypeName(string language, string expected)
    {
        Assert.Equal(expected, HeadhunterTexts.SubtypeName.For(language));
    }

    [Theory]
    [InlineData("en", "Headhunter")]
    [InlineData("fr", "Chasseur de têtes")]
    [InlineData("de", "Kopfjäger")]
    [InlineData("ru", "Охотник за головами")]
    [InlineData("pt", "Caçador de Cabeças")]
    [InlineData("ko", "Headhunter")]
    [InlineData("pl", "Headhunter")]
    [InlineData("zh", "Headhunter")]
    [InlineData("es", "Headhunter")]
    public void Headhunter_UniqueName(string language, string expected)
    {
        Assert.Equal(expected, HeadhunterTexts.UniqueName.For(language));
    }

    [Theory]
    [InlineData("en", HeadhunterLoreEn)]
    [InlineData("fr", HeadhunterLoreFr)]
    [InlineData("de", HeadhunterLoreDe)]
    [InlineData("ru", HeadhunterLoreEn)]
    [InlineData("pt", HeadhunterLoreEn)]
    [InlineData("ko", HeadhunterLoreEn)]
    [InlineData("pl", HeadhunterLoreEn)]
    [InlineData("zh", HeadhunterLoreEn)]
    [InlineData("es", HeadhunterLoreEn)]
    public void Headhunter_Lore(string language, string expected)
    {
        Assert.Equal(expected, HeadhunterTexts.Lore.For(language));
    }

    [Theory]
    [InlineData("en", "Mjölner")]
    [InlineData("fr", "Mjölner")]
    [InlineData("de", "Mjölner")]
    [InlineData("ru", "Мьёльнир")]
    [InlineData("pt", "Mjölner")]
    [InlineData("ko", "Mjölner")]
    [InlineData("pl", "Mjölner")]
    [InlineData("zh", "Mjölner")]
    [InlineData("es", "Mjölner")]
    public void Mjolner_UniqueName(string language, string expected)
    {
        Assert.Equal(expected, MjolnerTexts.UniqueName.For(language));
    }

    [Theory]
    [InlineData("en", MjolnerLoreEn)]
    [InlineData("fr", "Entrez dans l'œil de la tempête et vous gagnerez son respect.")]
    [InlineData("de", "Blickt dem Sturm ins Auge,\r\nund sein Respekt ist Euch gewiss.")]
    [InlineData("ru", MjolnerLoreEn)]
    [InlineData("pt", "Encare o olho da tempestade, e ela te respeitará.")]
    [InlineData("ko", MjolnerLoreEn)]
    [InlineData("pl", MjolnerLoreEn)]
    [InlineData("zh", MjolnerLoreEn)]
    [InlineData("es", MjolnerLoreEn)]
    public void Mjolner_Lore(string language, string expected)
    {
        Assert.Equal(expected, MjolnerTexts.Lore.For(language));
    }

    [Theory]
    [InlineData("en")]
    [InlineData("fr")]
    [InlineData("de")]
    [InlineData("ru")]
    [InlineData("pt")]
    [InlineData("ko")]
    [InlineData("pl")]
    [InlineData("zh")]
    [InlineData("es")]
    public void EssentiaSanguis_AllTextsEnglish(string language)
    {
        Assert.Equal("Furtive Wraps", EssentiaSanguisTexts.SubtypeName.For(language));
        Assert.Equal("Essentia Sanguis", EssentiaSanguisTexts.UniqueName.For(language));
        Assert.Equal(
            "Life Leech is Converted to Ward Leech",
            EssentiaSanguisTexts.Description.For(language)
        );
        Assert.Equal(
            "The darkest clouds clashed and coupled,\r\ngiving birth to four lightning children of hate.",
            EssentiaSanguisTexts.Lore.For(language)
        );
    }

    [Theory]
    [InlineData("en")]
    [InlineData("fr")]
    [InlineData("de")]
    [InlineData("ru")]
    [InlineData("pt")]
    [InlineData("ko")]
    [InlineData("pl")]
    [InlineData("zh")]
    [InlineData("es")]
    public void SandsOfSilk_AllTextsEnglish(string language)
    {
        Assert.Equal("Shrouded Vest", SandsOfSilkTexts.SubtypeName.For(language));
        Assert.Equal("Sands of Silk", SandsOfSilkTexts.UniqueName.For(language));
        Assert.Equal("The desert is ever flowing.", SandsOfSilkTexts.Lore.For(language));
    }
}
