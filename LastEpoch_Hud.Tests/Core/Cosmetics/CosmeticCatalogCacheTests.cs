using LastEpoch_Hud.Scripts.Core.Cosmetics;

namespace LastEpoch_Hud.Tests.Core.Cosmetics;

public sealed class CosmeticCatalogCacheTests
{
    private const long Key = 10;
    private const long OtherKey = 20;

    [Fact]
    public void TryGet_SameAliveRegistry_BuildsOnce()
    {
        var builder = new CountingCatalogBuilder(2);
        var cache = new CosmeticCatalogCache(builder.Build);

        bool first = cache.TryGet(Key, true, out CosmeticCatalog firstCatalog);
        bool second = cache.TryGet(Key, true, out CosmeticCatalog secondCatalog);

        Assert.True(first);
        Assert.True(second);
        Assert.Same(firstCatalog, secondCatalog);
        Assert.Equal(1, builder.Calls);
    }

    [Fact]
    public void TryGet_RegistryDead_Rebuilds()
    {
        var builder = new CountingCatalogBuilder(2);
        var cache = new CosmeticCatalogCache(builder.Build);

        cache.TryGet(Key, true, out CosmeticCatalog _);
        bool ready = cache.TryGet(Key, false, out CosmeticCatalog _);

        Assert.True(ready);
        Assert.Equal(2, builder.Calls);
    }

    [Fact]
    public void TryGet_BuilderReturnsZeroKept_NotReadyAndNoRetry()
    {
        var builder = new CountingCatalogBuilder(0);
        var cache = new CosmeticCatalogCache(builder.Build);

        bool first = cache.TryGet(Key, true, out CosmeticCatalog _);
        bool second = cache.TryGet(Key, true, out CosmeticCatalog _);

        Assert.False(first);
        Assert.False(second);
        Assert.Equal(1, builder.Calls);
    }

    [Fact]
    public void OnSceneLoaded_AfterFailure_AllowsOneRetry()
    {
        var builder = new CountingCatalogBuilder(0);
        var cache = new CosmeticCatalogCache(builder.Build);
        cache.TryGet(Key, true, out CosmeticCatalog _);

        cache.OnSceneLoaded(Key, true);
        cache.TryGet(Key, true, out CosmeticCatalog _);
        cache.TryGet(Key, true, out CosmeticCatalog _);

        Assert.Equal(2, builder.Calls);
    }

    [Fact]
    public void OnSceneLoaded_SameKey_KeepsCatalog()
    {
        var builder = new CountingCatalogBuilder(2);
        var cache = new CosmeticCatalogCache(builder.Build);
        cache.TryGet(Key, true, out CosmeticCatalog _);

        cache.OnSceneLoaded(Key, true);
        cache.TryGet(Key, true, out CosmeticCatalog _);

        Assert.Equal(1, builder.Calls);
    }

    [Fact]
    public void OnSceneLoaded_KeyChanged_Resets()
    {
        var builder = new CountingCatalogBuilder(2);
        var cache = new CosmeticCatalogCache(builder.Build);
        cache.TryGet(Key, true, out CosmeticCatalog _);

        cache.OnSceneLoaded(OtherKey, true);
        cache.TryGet(OtherKey, true, out CosmeticCatalog _);

        Assert.Equal(2, builder.Calls);
    }

    [Fact]
    public void OnSceneLoaded_RegistryGone_Resets()
    {
        var builder = new CountingCatalogBuilder(2);
        var cache = new CosmeticCatalogCache(builder.Build);
        cache.TryGet(Key, true, out CosmeticCatalog _);

        cache.OnSceneLoaded(0, false);
        cache.TryGet(Key, true, out CosmeticCatalog _);

        Assert.Equal(2, builder.Calls);
    }

    [Fact]
    public void MarkBroken_WithCachedCatalog_NotReadyAndNoBuild()
    {
        var builder = new CountingCatalogBuilder(2);
        var cache = new CosmeticCatalogCache(builder.Build);
        cache.TryGet(Key, true, out CosmeticCatalog _);

        cache.MarkBroken();
        bool ready = cache.TryGet(Key, true, out CosmeticCatalog catalog);

        Assert.False(ready);
        Assert.Null(catalog);
        Assert.Equal(1, builder.Calls);
        Assert.False(cache.HasCatalog);
    }

    [Fact]
    public void MarkBroken_BeforeBuild_NeverBuilds()
    {
        var builder = new CountingCatalogBuilder(2);
        var cache = new CosmeticCatalogCache(builder.Build);

        cache.MarkBroken();
        bool ready = cache.TryGet(Key, false, out CosmeticCatalog _);

        Assert.False(ready);
        Assert.Equal(0, builder.Calls);
        Assert.True(cache.Failed);
        Assert.True(cache.Broken);
    }

    [Fact]
    public void OnSceneLoaded_AfterMarkBroken_StaysBroken()
    {
        var builder = new CountingCatalogBuilder(2);
        var cache = new CosmeticCatalogCache(builder.Build);
        cache.MarkBroken();

        cache.OnSceneLoaded(Key, true);
        bool ready = cache.TryGet(Key, true, out CosmeticCatalog _);

        Assert.False(ready);
        Assert.Equal(0, builder.Calls);
        Assert.True(cache.Broken);
        Assert.True(cache.Failed);
    }

    [Fact]
    public void TryGet_BuildThrows_PropagatesAndStaysUnfailed()
    {
        var cache = new CosmeticCatalogCache(() => throw new InvalidOperationException());

        Assert.Throws<InvalidOperationException>(() =>
            cache.TryGet(Key, true, out CosmeticCatalog _)
        );

        Assert.False(cache.Failed);
        Assert.False(cache.HasCatalog);
    }

    [Fact]
    public void OnSceneLoaded_NothingCached_DoesNotBuild()
    {
        var builder = new CountingCatalogBuilder(2);
        var cache = new CosmeticCatalogCache(builder.Build);

        cache.OnSceneLoaded(Key, true);

        Assert.Equal(0, builder.Calls);
    }
}
