using System.Text.RegularExpressions;
using LastEpoch_Hud.Scripts.Core.Cosmetics;

namespace LastEpoch_Hud.Tests.Core.Cosmetics;

public sealed class CosmeticCatalogTests
{
    [Fact]
    public void Add_DuplicateId_CountsOnce()
    {
        var catalog = new CosmeticCatalog();
        catalog.Add("MTX_A", "A", true, false);
        catalog.Add("MTX_A", "A", true, false);

        Assert.Equal(1, catalog.KeptCount);
        Assert.Equal(new[] { "MTX_A" }, catalog.KeptIds);
    }

    [Fact]
    public void Add_NoStoreIcon_NotKept_Counted()
    {
        var catalog = new CosmeticCatalog();

        CosmeticVerdict verdict = catalog.Add("MTX_A", "A", false, false);

        Assert.Equal(CosmeticVerdict.NoStoreIcon, verdict);
        Assert.False(catalog.Contains("MTX_A"));
        Assert.Equal(1, catalog.RemovedBy(CosmeticVerdict.NoStoreIcon));
        Assert.Equal(1, catalog.RemovedCount);
        Assert.Equal(0, catalog.KeptCount);
    }

    [Fact]
    public void Add_NoStoreIcon_Title_Kept()
    {
        var catalog = new CosmeticCatalog();

        CosmeticVerdict verdict = catalog.Add("MTX_A", "A", false, true);

        Assert.Equal(CosmeticVerdict.Keep, verdict);
        Assert.True(catalog.Contains("MTX_A"));
    }

    [Fact]
    public void Add_FirstSightingDecides()
    {
        var catalog = new CosmeticCatalog();
        catalog.Add("MTX_A", "Test A", true, false);

        CosmeticVerdict second = catalog.Add("MTX_A", "A", true, false);

        Assert.Equal(CosmeticVerdict.TestEntry, second);
        Assert.False(catalog.Contains("MTX_A"));
        Assert.Equal(1, catalog.RemovedBy(CosmeticVerdict.TestEntry));
    }

    [Fact]
    public void Add_KeptFirst_StaysKept()
    {
        var catalog = new CosmeticCatalog();

        CosmeticVerdict first = catalog.Add("MTX_A", "A", true, false);
        CosmeticVerdict second = catalog.Add("MTX_A", "Test A", true, false);

        Assert.Equal(CosmeticVerdict.Keep, first);
        Assert.Equal(CosmeticVerdict.Keep, second);
        Assert.True(catalog.Contains("MTX_A"));
        Assert.Equal(0, catalog.RemovedBy(CosmeticVerdict.TestEntry));
        Assert.Equal(1, catalog.KeptCount);
    }

    [Fact]
    public void KeptIds_KeepFirstSeenOrder()
    {
        var catalog = new CosmeticCatalog();
        catalog.Add("MTX_B", "B", true, false);
        catalog.Add("MTX_A", "A", true, false);
        catalog.Add("MTX_B", "B", true, false);

        Assert.Equal(new[] { "MTX_B", "MTX_A" }, catalog.KeptIds);
    }

    [Fact]
    public void RemovedBy_CountsDistinctIdsPerReason()
    {
        var catalog = new CosmeticCatalog();
        catalog.Add(null, "Foo", true, false);
        catalog.Add("", "Foo", true, false);
        catalog.Add("TestHelmet", "Foo", true, false);
        catalog.Add("Foo", "Foo", true, false);
        catalog.Add("MTX_X_Test", "Foo", true, false);

        Assert.Equal(1, catalog.RemovedBy(CosmeticVerdict.EmptyId));
        Assert.Equal(2, catalog.RemovedBy(CosmeticVerdict.UnknownPrefix));
        Assert.Equal(1, catalog.RemovedBy(CosmeticVerdict.TestEntry));
        Assert.Equal(0, catalog.RemovedBy(CosmeticVerdict.Blocklisted));
        Assert.Equal(4, catalog.RemovedCount);
        Assert.Equal(0, catalog.RemovedBy(CosmeticVerdict.Keep));
        Assert.Equal(0, catalog.KeptCount);
    }

    [Fact]
    public void Contains_NullEmptyOrUnknown_False()
    {
        var catalog = new CosmeticCatalog();
        catalog.Add("MTX_K", "K", true, false);
        catalog.Add("Foo", "Foo", true, false);

        Assert.False(catalog.Contains(null));
        Assert.False(catalog.Contains(""));
        Assert.False(catalog.Contains("MTX_Z"));
        Assert.False(catalog.Contains("Foo"));
        Assert.True(catalog.Contains("MTX_K"));
    }

    [Fact]
    public void SummaryLine_NumbersInOrder()
    {
        var catalog = new CosmeticCatalog();
        foreach (char letter in "ABCDEFG")
        {
            catalog.Add("MTX_" + letter, "Foo", true, false);
        }
        catalog.Add("", "Foo", true, false);
        catalog.Add("Foo", "Foo", true, false);
        catalog.Add("Bar", "Foo", true, false);
        catalog.Add("MTX_X_Test", "Foo", true, false);
        catalog.Add("MTX_Y_Test", "Foo", true, false);
        catalog.Add("MTX_Z_Test", "Foo", true, false);
        catalog.Add("MTX_N", "Foo", false, false);

        int[] numbers = Regex
            .Matches(catalog.SummaryLine(), @"\d+")
            .Select(m => int.Parse(m.Value))
            .ToArray();

        Assert.Equal(new[] { 7, 7, 1, 2, 0, 3, 1 }, numbers);
    }

    [Fact]
    public void TryGetVerdict_SeenId_ReturnsStored()
    {
        var catalog = new CosmeticCatalog();
        catalog.Add("MTX_A", "Test A", true, false);
        catalog.Add("MTX_B", "B", false, false);
        catalog.Add("MTX_C", "C", true, false);

        Assert.True(catalog.TryGetVerdict("MTX_A", out CosmeticVerdict a));
        Assert.Equal(CosmeticVerdict.TestEntry, a);
        Assert.True(catalog.TryGetVerdict("MTX_B", out CosmeticVerdict b));
        Assert.Equal(CosmeticVerdict.NoStoreIcon, b);
        Assert.True(catalog.TryGetVerdict("MTX_C", out CosmeticVerdict c));
        Assert.Equal(CosmeticVerdict.Keep, c);
    }

    [Fact]
    public void TryGetVerdict_UnseenId_FalseAndStoresNothing()
    {
        var catalog = new CosmeticCatalog();

        Assert.False(catalog.TryGetVerdict("MTX_D", out _));
        Assert.False(catalog.TryGetVerdict(null, out _));
        Assert.Equal(0, catalog.KeptCount);
        Assert.Equal(0, catalog.RemovedCount);
    }
}
