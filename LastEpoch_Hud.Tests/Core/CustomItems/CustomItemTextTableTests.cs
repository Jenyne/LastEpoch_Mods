using LastEpoch_Hud.Scripts.Core.CustomItems;

namespace LastEpoch_Hud.Tests.Core.CustomItems;

public sealed class CustomItemTextTableTests
{
    private static readonly LocalizedText _text = new("english", ("fr", "francais"));

    [Fact]
    public void Resolve_UnknownKey_ReturnsNull()
    {
        Assert.Null(new CustomItemTextTable().Resolve("missing", "en"));
    }

    [Fact]
    public void Resolve_NullKey_ReturnsNull()
    {
        Assert.Null(new CustomItemTextTable().Resolve(null, "en"));
    }

    [Fact]
    public void Resolve_LocalizedText_ReturnsTextForLanguage()
    {
        var table = new CustomItemTextTable();
        table.Register("key", _text);

        Assert.Equal("francais", table.Resolve("key", "fr"));
    }

    [Fact]
    public void Resolve_LocalizedTextUnknownLanguage_ReturnsNull()
    {
        var table = new CustomItemTextTable();
        table.Register("key", _text);

        Assert.Null(table.Resolve("key", ""));
    }

    [Fact]
    public void Resolve_PassesLanguageToResolver()
    {
        var table = new CustomItemTextTable();
        string received = null;
        table.Register(
            "key",
            (Func<string, string>)(
                language =>
                {
                    received = language;
                    return "text";
                }
            )
        );

        table.Resolve("key", "de");

        Assert.Equal("de", received);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Resolve_ResolverReturnsNullOrEmpty_ReturnsNull(string result)
    {
        var table = new CustomItemTextTable();
        table.Register("key", (Func<string, string>)(_ => result));

        Assert.Null(table.Resolve("key", "en"));
    }

    [Fact]
    public void Register_SameKeyTwice_LastWins()
    {
        var table = new CustomItemTextTable();
        table.Register("key", (Func<string, string>)(_ => "first"));
        table.Register("key", (Func<string, string>)(_ => "second"));

        Assert.Equal("second", table.Resolve("key", "en"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Register_NullOrEmptyKey_Ignored(string key)
    {
        var table = new CustomItemTextTable();

        table.Register(key, (Func<string, string>)(_ => "text"));

        Assert.Null(table.Resolve("", "en"));
    }

    [Fact]
    public void Register_NullResolver_Ignored()
    {
        var table = new CustomItemTextTable();

        table.Register("key", (Func<string, string>)null);

        Assert.Null(table.Resolve("key", "en"));
    }

    [Fact]
    public void Register_NullLocalizedText_Ignored()
    {
        var table = new CustomItemTextTable();

        table.Register("key", (LocalizedText)null);

        Assert.Null(table.Resolve("key", "en"));
    }
}
