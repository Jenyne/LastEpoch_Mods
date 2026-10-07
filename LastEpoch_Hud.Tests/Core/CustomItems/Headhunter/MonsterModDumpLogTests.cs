using System.Collections.Generic;
using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter;

namespace LastEpoch_Hud.Tests.Core.CustomItems.Headhunter;

public sealed class MonsterModDumpLogTests
{
    [Fact]
    public void Line_CountsListsAndStats()
    {
        var withStats = new List<MonsterModStat> { MonsterModTestData.Stat("PropA", 1f, 0f) };
        var rows = new List<MonsterModRow>
        {
            MonsterModTestData.Row(MonsterModRow.PrefixList, withStats),
            MonsterModTestData.Row(MonsterModRow.PrefixList, new List<MonsterModStat>()),
            MonsterModTestData.Row(MonsterModRow.SuffixList, new List<MonsterModStat>()),
        };

        Assert.Equal(
            "Monster mods dumped: 2 prefixes, 1 suffixes, 1 with stats -> p.tsv",
            MonsterModDumpLog.Line(rows, "p.tsv")
        );
    }
}
