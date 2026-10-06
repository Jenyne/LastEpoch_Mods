using LastEpoch_Hud.Scripts.Core.CustomItems;

namespace LastEpoch_Hud.Tests.Core.CustomItems;

public sealed class CustomItemTextTableTests
{
    private static readonly Dictionary<string, string> _english = new() { ["loc"] = "text-a" };
    private static readonly Dictionary<string, string> _french = new() { ["loc"] = "text-b" };

    [Fact]
    public void Resolve_UnknownKey_ReturnsNull()
    {
        Assert.Null(new CustomItemTextTable().Resolve("missing", _english));
    }

    [Fact]
    public void Resolve_NullKey_ReturnsNull()
    {
        Assert.Null(new CustomItemTextTable().Resolve(null, _english));
    }

    [Fact]
    public void Resolve_LocaleKey_ReturnsDictionaryValue()
    {
        var table = new CustomItemTextTable();
        table.RegisterLocaleKey("key", "loc");

        Assert.Equal("text-a", table.Resolve("key", _english));
    }

    [Fact]
    public void Resolve_OtherDictionary_ReturnsOtherText()
    {
        var table = new CustomItemTextTable();
        table.RegisterLocaleKey("key", "loc");

        table.Resolve("key", _english);

        Assert.Equal("text-b", table.Resolve("key", _french));
    }

    [Fact]
    public void Resolve_NullTexts_ReturnsNull()
    {
        var table = new CustomItemTextTable();
        table.Register("key", _ => "text");

        Assert.Null(table.Resolve("key", null));
    }

    [Fact]
    public void Resolve_LocaleKeyMissing_ReturnsNull()
    {
        var table = new CustomItemTextTable();
        table.RegisterLocaleKey("key", "other");

        Assert.Null(table.Resolve("key", _english));
    }

    [Fact]
    public void Resolve_PassesTextsToResolver()
    {
        var table = new CustomItemTextTable();
        IReadOnlyDictionary<string, string> received = null;
        table.Register(
            "key",
            texts =>
            {
                received = texts;
                return "text";
            }
        );

        table.Resolve("key", _french);

        Assert.Same(_french, received);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Resolve_ResolverReturnsNullOrEmpty_ReturnsNull(string result)
    {
        var table = new CustomItemTextTable();
        table.Register("key", _ => result);

        Assert.Null(table.Resolve("key", _english));
    }

    [Fact]
    public void Register_SameKeyTwice_LastWins()
    {
        var table = new CustomItemTextTable();
        table.Register("key", _ => "first");
        table.Register("key", _ => "second");

        Assert.Equal("second", table.Resolve("key", _english));
    }

    [Fact]
    public void RegisterLocaleKey_AfterRegister_LastWins()
    {
        var table = new CustomItemTextTable();
        table.Register("key", _ => "first");
        table.RegisterLocaleKey("key", "loc");

        Assert.Equal("text-a", table.Resolve("key", _english));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Register_NullOrEmptyKey_Ignored(string key)
    {
        var table = new CustomItemTextTable();

        table.Register(key, _ => "text");

        Assert.Null(table.Resolve("", _english));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void RegisterLocaleKey_NullOrEmptyGameKey_Ignored(string key)
    {
        var table = new CustomItemTextTable();

        table.RegisterLocaleKey(key, "loc");

        Assert.Null(table.Resolve("", _english));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void RegisterLocaleKey_NullOrEmptyLocaleKey_Ignored(string localeKey)
    {
        var table = new CustomItemTextTable();

        table.RegisterLocaleKey("key", localeKey);

        Assert.Null(table.Resolve("key", _english));
    }

    [Fact]
    public void Register_NullResolver_Ignored()
    {
        var table = new CustomItemTextTable();

        table.Register("key", null);

        Assert.Null(table.Resolve("key", _english));
    }
}
