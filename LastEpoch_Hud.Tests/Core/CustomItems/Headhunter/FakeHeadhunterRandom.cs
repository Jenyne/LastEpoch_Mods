using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter;

namespace LastEpoch_Hud.Tests.Core.CustomItems.Headhunter;

/// <summary>Random source that always returns one fixed value and records its calls.</summary>
internal sealed class FakeHeadhunterRandom(int value) : IHeadhunterRandom
{
    public int Calls { get; private set; }

    public int LastMax { get; private set; }

    public int Next(int maxExclusive)
    {
        Calls++;
        LastMax = maxExclusive;
        return value;
    }
}
