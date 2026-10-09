using LastEpoch_Hud.Scripts.Core.CustomItems;

namespace LastEpoch_Hud.Tests.Core.CustomItems;

public sealed class LocaleTextTests
{
    private static readonly Dictionary<string, string> _texts = new()
    {
        ["key"] = "value",
        ["empty"] = "",
    };

    [Fact]
    public void Get_KeyPresent_ReturnsValue()
    {
        Assert.Equal("value", LocaleText.Get(_texts, "key"));
    }

    [Fact]
    public void Get_NullTexts_ReturnsNull()
    {
        Assert.Null(LocaleText.Get(null, "key"));
    }

    [Fact]
    public void Get_MissingKey_ReturnsNull()
    {
        Assert.Null(LocaleText.Get(_texts, "other"));
    }

    [Fact]
    public void Get_EmptyValue_ReturnsNull()
    {
        Assert.Null(LocaleText.Get(_texts, "empty"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Get_NullOrEmptyKey_ReturnsNull(string key)
    {
        Assert.Null(LocaleText.Get(_texts, key));
    }
}
