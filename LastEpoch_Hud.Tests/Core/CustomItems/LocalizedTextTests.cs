using LastEpoch_Hud.Scripts.Core.CustomItems;

namespace LastEpoch_Hud.Tests.Core.CustomItems;

public sealed class LocalizedTextTests
{
    private static readonly LocalizedText _text = new(
        "english",
        ("fr", "francais"),
        ("de", "deutsch")
    );

    [Fact]
    public void For_English_ReturnsEnglish()
    {
        Assert.Equal("english", _text.For("en"));
    }

    [Fact]
    public void For_TranslatedLanguage_ReturnsTranslation()
    {
        Assert.Equal("francais", _text.For("fr"));
    }

    [Fact]
    public void For_UntranslatedLanguage_FallsBackToEnglish()
    {
        Assert.Equal("english", _text.For("ko"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void For_NullOrEmptyLanguage_ReturnsEmpty(string language)
    {
        Assert.Equal("", _text.For(language));
    }
}
