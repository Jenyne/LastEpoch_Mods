using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Buffs;

namespace LastEpoch_Hud.Tests.Core.CustomItems.Headhunter.Buffs;

public sealed class HeadhunterSystemRandomTests
{
    [Fact]
    public void Next_StaysInRange()
    {
        var random = new HeadhunterSystemRandom();

        for (int i = 0; i < 200; i++)
        {
            int value = random.Next(3);
            Assert.InRange(value, 0, 2);
        }
    }

    [Fact]
    public void Next_MaxOne_ReturnsZero()
    {
        var random = new HeadhunterSystemRandom();

        Assert.Equal(0, random.Next(1));
    }
}
