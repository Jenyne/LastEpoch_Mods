using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Bar;

namespace LastEpoch_Hud.Tests.Core.CustomItems.Headhunter.Bar;

public sealed class HeadhunterBarLogTests
{
    [Fact]
    public void Format_ListsNamesInOrderWithCount()
    {
        HeadhunterBarEntry[] entries =
        [
            new(1, 5, 0.5f, 0, 1),
            new(2, 5, 0.5f, 1, 1),
            new(3, 5, 0.5f, 2, 1),
        ];

        string line = HeadhunterBarLog.Format(entries, Name, TagName);

        Assert.True(
            line.IndexOf("SB", StringComparison.Ordinal)
                < line.IndexOf("SC", StringComparison.Ordinal)
        );
        Assert.Contains("3", line);
    }

    [Fact]
    public void Format_Empty_CountZero()
    {
        string line = HeadhunterBarLog.Format([], Name, TagName);

        Assert.Contains("0", line);
    }

    [Fact]
    public void Format_TaggedEntry_AppendsTagName()
    {
        HeadhunterBarEntry[] entries = [new(1, 5, 0.5f, 0, 1), new(1, 5, 0.5f, 1, 1, 8)];

        string line = HeadhunterBarLog.Format(entries, id => "S" + id, TagName);

        Assert.Contains("[S1, S1[T8]]", line);
    }

    private static string Name(int id)
    {
        return "S" + (char)('A' + id);
    }

    private static string TagName(int id)
    {
        return "T" + id;
    }
}
