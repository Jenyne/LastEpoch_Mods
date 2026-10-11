using LastEpoch_Hud.Scripts.Core.Items.AutoLoot;

namespace LastEpoch_Hud.Tests.Core.Items.AutoLoot;

internal sealed class FakeShatterRandom(params int[] rolls) : IShatterRandom
{
    public int Calls { get; private set; }

    public int RollPercent()
    {
        if (Calls >= rolls.Length)
        {
            throw new InvalidOperationException("No roll expected.");
        }

        return rolls[Calls++];
    }
}
