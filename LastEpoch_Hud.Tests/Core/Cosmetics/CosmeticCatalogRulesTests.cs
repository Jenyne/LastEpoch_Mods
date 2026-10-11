using LastEpoch_Hud.Scripts.Core.Cosmetics;

namespace LastEpoch_Hud.Tests.Core.Cosmetics;

public sealed class CosmeticCatalogRulesTests
{
    [Theory]
    [InlineData("MTX_Foo")]
    [InlineData("mtx_foo")]
    [InlineData("SP_Foo")]
    [InlineData("KS_Foo")]
    [InlineData("Event_Foo")]
    [InlineData("TD_Foo")]
    [InlineData("MTX_Bundle_Foo")]
    [InlineData("MTX_Skills_Gauntlet_of_Fury_JudgementV2")]
    public void Judge_RealPrefix_Keeps(string id)
    {
        Assert.Equal(CosmeticVerdict.Keep, CosmeticCatalogRules.Judge(id, "Foo"));
    }

    [Fact]
    public void Judge_NullDisplayName_Keeps()
    {
        Assert.Equal(CosmeticVerdict.Keep, CosmeticCatalogRules.Judge("MTX_Foo", null));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Judge_NullOrEmptyId_EmptyId(string id)
    {
        Assert.Equal(CosmeticVerdict.EmptyId, CosmeticCatalogRules.Judge(id, "Foo"));
    }

    [Theory]
    [InlineData("Mtx_Foo")]
    [InlineData("MTXFoo")]
    [InlineData("sp_Foo")]
    [InlineData("TestHelmet")]
    [InlineData("AshTribeHelmet")]
    public void Judge_UnknownPrefix_Removes(string id)
    {
        Assert.Equal(CosmeticVerdict.UnknownPrefix, CosmeticCatalogRules.Judge(id, "Foo"));
    }

    [Fact]
    public void Judge_Blocklisted_Removes()
    {
        Assert.Equal(
            CosmeticVerdict.Blocklisted,
            CosmeticCatalogRules.Judge("MTX_Chest_TemporalGuardianFix", "Foo")
        );
    }

    [Theory]
    [InlineData("MTX_Foo_Test")]
    [InlineData("MTX_Foo_Test1")]
    [InlineData("MTX_TestFoo_Bar")]
    [InlineData("MTX_Foo-test")]
    [InlineData("MTX__Test")]
    [InlineData("MTX_Foo_Test_")]
    [InlineData("MTX_Foo-_Test")]
    public void Judge_TestSegmentInId_Removes(string id)
    {
        Assert.Equal(CosmeticVerdict.TestEntry, CosmeticCatalogRules.Judge(id, "Foo"));
    }

    [Theory]
    [InlineData("Test Foo")]
    [InlineData("TestFoo1")]
    [InlineData("Foo-test")]
    [InlineData(" Test")]
    [InlineData("Foo  Test")]
    [InlineData("Test-")]
    public void Judge_TestWordInDisplayName_Removes(string displayName)
    {
        Assert.Equal(CosmeticVerdict.TestEntry, CosmeticCatalogRules.Judge("MTX_Foo", displayName));
    }

    [Theory]
    [InlineData("MTX_Testament", "Foo")]
    [InlineData("MTX_Contest", "Foo")]
    [InlineData("MTX_Latest", "Foo")]
    [InlineData("MTX_Tests", "Foo")]
    [InlineData("MTX_Foo", "Testament")]
    [InlineData("MTX_Foo", "Latest Foo")]
    [InlineData("MTX_Foo", "FooTest Bar")]
    [InlineData("MTX_Foo_", "Foo")]
    [InlineData("MTX_Foo_Tes", "Foo")]
    [InlineData("MTX_Foo", "Foo_")]
    [InlineData("MTX_Foo", "Te")]
    [InlineData("MTX_Foo", "testy")]
    [InlineData("MTX_Foo", "-")]
    public void Judge_TestInsideWord_Keeps(string id, string displayName)
    {
        Assert.Equal(CosmeticVerdict.Keep, CosmeticCatalogRules.Judge(id, displayName));
    }

    [Theory]
    [InlineData("", "Test", CosmeticVerdict.EmptyId)]
    [InlineData("TestFoo", "Test", CosmeticVerdict.UnknownPrefix)]
    [InlineData("MTX_Chest_TemporalGuardianFix", "Test", CosmeticVerdict.Blocklisted)]
    public void Judge_Precedence(string id, string displayName, CosmeticVerdict expected)
    {
        Assert.Equal(expected, CosmeticCatalogRules.Judge(id, displayName));
    }

    [Theory]
    [InlineData("MTX_Foo", "Foo", false)]
    [InlineData("MTX_Testament", "Foo", false)]
    [InlineData("MTX_Chest_TemporalGuardianFix", "x", true)]
    [InlineData("TestHelmet", null, true)]
    [InlineData("MTX_Foo_Test1", "Foo", true)]
    [InlineData("", "Foo", true)]
    [InlineData(null, null, true)]
    [InlineData("MTX_Foo", "Test Foo", true)]
    public void IsJunk_ReturnsExpected(string id, string displayName, bool expected)
    {
        Assert.Equal(expected, CosmeticCatalogRules.IsJunk(id, displayName));
    }

    [Fact]
    public void IsJunk_DoesNotAllocate()
    {
        CosmeticCatalogRules.IsJunk("MTX_Foo_Bar-Baz", "Foo Bar Baz");

        long before = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < 100; i++)
        {
            CosmeticCatalogRules.IsJunk("MTX_Foo_Bar-Baz", "Foo Bar Baz");
        }
        long after = GC.GetAllocatedBytesForCurrentThread();

        Assert.Equal(0, after - before);
    }
}
