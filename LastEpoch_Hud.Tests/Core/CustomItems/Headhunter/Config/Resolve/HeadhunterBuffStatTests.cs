using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Config.Resolve;

namespace LastEpoch_Hud.Tests.Core.CustomItems.Headhunter.Config.Resolve;

public sealed class HeadhunterBuffStatTests
{
    [Fact]
    public void AddedFor_IsAddedTimesStacks()
    {
        var stat = new HeadhunterBuffStat(1, "FakeBuff", 5f, 0f);

        Assert.Equal(15f, stat.AddedFor(3));
    }

    [Fact]
    public void IncreasedFor_IsIncreasedTimesStacks()
    {
        var stat = new HeadhunterBuffStat(1, "FakeBuff", 0f, 0.1f);

        Assert.Equal(0.4f, stat.IncreasedFor(4), 5);
    }
}
