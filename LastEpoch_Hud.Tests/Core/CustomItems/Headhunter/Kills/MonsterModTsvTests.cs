using System.Collections.Generic;
using System.Globalization;
using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Kills;

namespace LastEpoch_Hud.Tests.Core.CustomItems.Headhunter.Kills;

public sealed class MonsterModTsvTests
{
    [Fact]
    public void Clean_NullIsEmpty()
    {
        Assert.Equal("", MonsterModTsv.Clean(null));
    }

    [Fact]
    public void Clean_TabsAndNewlinesBecomeSpaces()
    {
        Assert.Equal("a b  c", MonsterModTsv.Clean("a\tb\r\nc"));
    }

    [Fact]
    public void Stats_FormatsAndJoins()
    {
        var stats = new List<MonsterModStat>
        {
            new("PropA", 1f, 0.5f, new[] { 0.2f, 0.3f }, "TagX"),
            new("PropB", 0f, 0f, System.Array.Empty<float>(), "None"),
        };

        Assert.Equal("PropA:1/0.5/0.2,0.3[TagX];PropB:0/0/0[None]", MonsterModTsv.Stats(stats));
    }

    [Fact]
    public void Stats_InvariantUnderFrenchCulture()
    {
        CultureInfo previous = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = new CultureInfo("fr-FR");
        try
        {
            var stats = new List<MonsterModStat>
            {
                MonsterModTestData.Stat("PropA", 1.5f, 0.5f, 0.25f, 0.75f),
            };

            Assert.Equal("PropA:1.5/0.5/0.25,0.75[TagX]", MonsterModTsv.Stats(stats));
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
        }
    }

    [Fact]
    public void Row_RareModifier_InvariantUnderFrenchCulture()
    {
        CultureInfo previous = CultureInfo.CurrentCulture;
        CultureInfo.CurrentCulture = new CultureInfo("fr-FR");
        try
        {
            MonsterModRow row = MonsterModTestData.Row(
                MonsterModRow.SuffixList,
                new List<MonsterModStat>()
            );

            Assert.Equal("1.5", MonsterModTsv.Row(row).Split('	')[7]);
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
        }
    }

    [Fact]
    public void Stats_EmptyIsEmpty()
    {
        Assert.Equal("", MonsterModTsv.Stats(new List<MonsterModStat>()));
    }

    [Fact]
    public void Row_CellsInHeaderOrder()
    {
        MonsterModRow row = MonsterModTestData.Row(
            MonsterModRow.SuffixList,
            new List<MonsterModStat> { MonsterModTestData.Stat("PropA", 1f, 0f) }
        );
        row.Title = "Title\tone";

        string[] cells = MonsterModTsv.Row(row).Split('\t');

        Assert.Equal(MonsterModTsv.Header.Split('\t').Length, cells.Length);
        Assert.Equal(10, cells.Length);
        Assert.Equal(MonsterModRow.SuffixList, cells[0]);
        Assert.Equal("7", cells[1]);
        Assert.Equal("Title one", cells[2]);
        Assert.Equal("TypeA", cells[3]);
        Assert.Equal("RarityA", cells[4]);
        Assert.Equal("GroupA", cells[5]);
        Assert.Equal("ClassA", cells[6]);
        Assert.Equal("1.5", cells[7]);
        Assert.Equal("ScaleA", cells[8]);
        Assert.Equal("PropA:1/0/0[TagX]", cells[9]);
    }

    [Fact]
    public void Row_MissingMod_EmptyCells()
    {
        var row = new MonsterModRow
        {
            List = MonsterModRow.PrefixList,
            ModKey = 3,
            ClassName = "missing",
        };

        Assert.Equal("prefix	3					missing			", MonsterModTsv.Row(row));
    }

    [Fact]
    public void Row_NoRareModifier_EmptyRareAndScaling()
    {
        MonsterModRow row = MonsterModTestData.Row(
            MonsterModRow.PrefixList,
            new List<MonsterModStat>()
        );
        row.RareModifier = null;

        string[] cells = MonsterModTsv.Row(row).Split('\t');

        Assert.Equal(10, cells.Length);
        Assert.Equal("", cells[7]);
        Assert.Equal("", cells[8]);
    }

    [Fact]
    public void Build_HeaderThenOneLinePerRow()
    {
        var rows = new List<MonsterModRow>
        {
            MonsterModTestData.Row(MonsterModRow.PrefixList, new List<MonsterModStat>()),
            MonsterModTestData.Row(MonsterModRow.SuffixList, new List<MonsterModStat>()),
        };

        string text = MonsterModTsv.Build(rows);

        string[] lines = text.Split('\n');
        Assert.EndsWith("\n", text);
        Assert.Equal(4, lines.Length);
        Assert.Equal(MonsterModTsv.Header, lines[0]);
        Assert.Equal(MonsterModTsv.Row(rows[0]), lines[1]);
        Assert.Equal(MonsterModTsv.Row(rows[1]), lines[2]);
    }
}
