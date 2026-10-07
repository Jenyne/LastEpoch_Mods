using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter;

namespace LastEpoch_Hud.Tests.Core.CustomItems.Headhunter;

public sealed class KillKindClassifierTests
{
    [Theory]
    [InlineData(true, true, true, KillKind.Boss)]
    [InlineData(true, false, false, KillKind.Boss)]
    [InlineData(false, true, true, KillKind.Miniboss)]
    [InlineData(false, true, false, KillKind.Miniboss)]
    [InlineData(false, false, true, KillKind.Rare)]
    [InlineData(false, false, false, KillKind.Normal)]
    public void Classify_ReturnsKind_FromFlags(
        bool isBoss,
        bool isMiniboss,
        bool isRare,
        KillKind expected
    )
    {
        KillKind kind = KillKindClassifier.Classify(isBoss, isMiniboss, isRare);

        Assert.Equal(expected, kind);
    }
}
