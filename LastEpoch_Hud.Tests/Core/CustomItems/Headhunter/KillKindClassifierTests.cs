using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter;

namespace LastEpoch_Hud.Tests.Core.CustomItems.Headhunter;

public sealed class KillKindClassifierTests
{
    [Theory]
    [InlineData(true, true, true, true, KillKind.Boss)]
    [InlineData(true, false, false, true, KillKind.Boss)]
    [InlineData(false, true, true, true, KillKind.Miniboss)]
    [InlineData(false, true, false, false, KillKind.Miniboss)]
    [InlineData(false, false, true, true, KillKind.Rare)]
    [InlineData(false, false, true, false, KillKind.Rare)]
    [InlineData(false, false, false, true, KillKind.Magic)]
    [InlineData(false, false, false, false, KillKind.Normal)]
    public void Classify_ReturnsKind_FromFlags(
        bool isBoss,
        bool isMiniboss,
        bool isRare,
        bool isMagic,
        KillKind expected
    )
    {
        KillKind kind = KillKindClassifier.Classify(isBoss, isMiniboss, isRare, isMagic);

        Assert.Equal(expected, kind);
    }
}
