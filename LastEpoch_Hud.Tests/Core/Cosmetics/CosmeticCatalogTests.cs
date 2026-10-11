using System.Text.RegularExpressions;
using LastEpoch_Hud.Scripts.Core.Cosmetics;

namespace LastEpoch_Hud.Tests.Core.Cosmetics;

public sealed class CosmeticCatalogTests
{
    [Fact]
    public void Add_DuplicateId_CountsOnce()
    {
        var catalog = new CosmeticCatalog();
        catalog.Add("MTX_A", "A");
        catalog.Add("MTX_A", "A");

        Assert.Equal(1, catalog.KeptCount);
        Assert.Equal(new[] { "MTX_A" }, catalog.KeptIds);
    }

    [Fact]
    public void Add_FirstSightingDecides()
    {
        var catalog = new CosmeticCatalog();
        catalog.Add("MTX_A", "Test A");

        CosmeticVerdict second = catalog.Add("MTX_A", "A");

        Assert.Equal(CosmeticVerdict.TestEntry, second);
        Assert.False(catalog.Contains("MTX_A"));
        Assert.Equal(1, catalog.RemovedBy(CosmeticVerdict.TestEntry));
    }

    [Fact]
    public void Add_KeptFirst_StaysKept()
    {
        var catalog = new CosmeticCatalog();

        CosmeticVerdict first = catalog.Add("MTX_A", "A");
        CosmeticVerdict second = catalog.Add("MTX_A", "Test A");

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
        catalog.Add("MTX_B", "B");
        catalog.Add("MTX_A", "A");
        catalog.Add("MTX_B", "B");

        Assert.Equal(new[] { "MTX_B", "MTX_A" }, catalog.KeptIds);
    }

    [Fact]
    public void RemovedBy_CountsDistinctIdsPerReason()
    {
        var catalog = new CosmeticCatalog();
        catalog.Add(null, "Foo");
        catalog.Add("", "Foo");
        catalog.Add("TestHelmet", "Foo");
        catalog.Add("Foo", "Foo");
        catalog.Add("MTX_X_Test", "Foo");

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
        catalog.Add("MTX_K", "K");
        catalog.Add("Foo", "Foo");

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
            catalog.Add("MTX_" + letter, "Foo");
        }
        catalog.Add("", "Foo");
        catalog.Add("Foo", "Foo");
        catalog.Add("Bar", "Foo");
        catalog.Add("MTX_X_Test", "Foo");
        catalog.Add("MTX_Y_Test", "Foo");
        catalog.Add("MTX_Z_Test", "Foo");

        int[] numbers = Regex
            .Matches(catalog.SummaryLine(), @"\d+")
            .Select(m => int.Parse(m.Value))
            .ToArray();

        Assert.Equal(new[] { 7, 6, 1, 2, 0, 3 }, numbers);
    }

    [Fact]
    public void IsJunk_SeenId_UsesStoredVerdict()
    {
        var catalog = new CosmeticCatalog();
        catalog.Add("MTX_A", "Test A");
        catalog.Add("MTX_B", "B");

        Assert.True(catalog.IsJunk("MTX_A", "A"));
        Assert.False(catalog.IsJunk("MTX_B", "Test B"));
    }

    [Fact]
    public void IsJunk_UnseenId_StoresNothing()
    {
        var catalog = new CosmeticCatalog();

        Assert.True(catalog.IsJunk("MTX_C", "Test C"));
        Assert.Equal(CosmeticVerdict.Keep, catalog.Add("MTX_C", "C"));
        Assert.Equal(1, catalog.KeptCount);
        Assert.Equal(0, catalog.RemovedCount);
    }

    [Fact]
    public void IsJunk_UnseenId_FallsBackToRules()
    {
        var catalog = new CosmeticCatalog();

        Assert.True(catalog.IsJunk("TestHelmet", "x"));
        Assert.False(catalog.IsJunk("MTX_Real", "Real"));
    }
}
