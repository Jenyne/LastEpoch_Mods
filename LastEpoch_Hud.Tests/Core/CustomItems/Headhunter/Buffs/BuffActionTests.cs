using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Buffs;

namespace LastEpoch_Hud.Tests.Core.CustomItems.Headhunter.Buffs;

public sealed class BuffActionTests
{
    [Fact]
    public void More_DefaultsToZero()
    {
        var action = new BuffAction(BuffActionKind.Add, "k", 1, 0.5f, 0.1f, 10f, 1);

        Assert.Equal(0f, action.More);
        Assert.Equal(
            new BuffAction(BuffActionKind.Add, "k", 1, 0.5f, 0.1f, 10f, 1, More: 0f),
            action
        );
    }
}
