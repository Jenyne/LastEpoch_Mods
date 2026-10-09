using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Bar;

namespace LastEpoch_Hud.Tests.Core.CustomItems.Headhunter.Bar;

public sealed class StatNameWordsTests
{
    [Theory]
    [InlineData("FooBar", "Foo Bar")]
    [InlineData("ABCFoo", "ABC Foo")]
    [InlineData("Foo", "Foo")]
    public void Split_Words(string name, string expected)
    {
        Assert.Equal(expected, StatNameWords.Split(name));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Split_NullOrEmpty_Empty(string name)
    {
        Assert.Equal("", StatNameWords.Split(name));
    }
}
