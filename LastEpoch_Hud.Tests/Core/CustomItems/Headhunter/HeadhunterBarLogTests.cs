using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter;

namespace LastEpoch_Hud.Tests.Core.CustomItems.Headhunter;

public sealed class HeadhunterBarLogTests
{
    [Fact]
    public void Format_ListsNamesInOrderWithCount()
    {
        HeadhunterBarEntry[] entries = [new(1, 5, 0.5f), new(2, 5, 0.5f), new(3, 5, 0.5f)];

        string line = HeadhunterBarLog.Format(entries, Name);

        Assert.True(
            line.IndexOf("SB", StringComparison.Ordinal)
                < line.IndexOf("SC", StringComparison.Ordinal)
        );
        Assert.Contains("3", line);
    }

    [Fact]
    public void Format_Empty_CountZero()
    {
        string line = HeadhunterBarLog.Format([], Name);

        Assert.Contains("0", line);
    }

    private static string Name(int id)
    {
        return "S" + (char)('A' + id);
    }
}
