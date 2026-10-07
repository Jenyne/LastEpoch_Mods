using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter;

namespace LastEpoch_Hud.Tests.Core.CustomItems.Headhunter;

public sealed class SecondsTextCacheTests
{
    [Fact]
    public void Get_GrowsAndReturnsDigits()
    {
        var cache = new SecondsTextCache();

        Assert.Equal("42", cache.Get(42));
        Assert.Equal("5", cache.Get(5));
        Assert.Equal("0", cache.Get(0));
    }

    [Fact]
    public void Get_SameValue_ReturnsCachedInstance()
    {
        var cache = new SecondsTextCache();

        Assert.Same(cache.Get(7), cache.Get(7));
    }

    [Fact]
    public void Get_AfterGrowth_KeepsEarlierInstances()
    {
        var cache = new SecondsTextCache();
        string before = cache.Get(33);
        cache.Get(50);

        Assert.Same(before, cache.Get(33));
    }
}
