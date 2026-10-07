using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Kills;

namespace LastEpoch_Hud.Tests.Core.CustomItems.Headhunter.Kills;

public sealed class KillDeduperTests
{
    [Fact]
    public void TryClaim_SameActorSameFrame_SecondReturnsFalse()
    {
        var deduper = new KillDeduper();

        Assert.True(deduper.TryClaim(5, 42));
        Assert.False(deduper.TryClaim(5, 42));
    }

    [Fact]
    public void TryClaim_OtherActorSameFrame_ReturnsTrue()
    {
        var deduper = new KillDeduper();

        Assert.True(deduper.TryClaim(5, 42));
        Assert.True(deduper.TryClaim(5, 7));
    }

    [Fact]
    public void TryClaim_SameActorLaterFrame_ReturnsTrue()
    {
        var deduper = new KillDeduper();

        Assert.True(deduper.TryClaim(5, 42));
        Assert.True(deduper.TryClaim(6, 42));
    }

    [Fact]
    public void TryClaim_InterleavedSameFrame_RepeatReturnsFalse()
    {
        var deduper = new KillDeduper();

        Assert.True(deduper.TryClaim(5, 1));
        Assert.True(deduper.TryClaim(5, 2));
        Assert.False(deduper.TryClaim(5, 1));
    }
}
