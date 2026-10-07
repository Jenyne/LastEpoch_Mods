using LastEpoch_Hud.Scripts.Core.ForceDrop;

namespace LastEpoch_Hud.Tests.Core.ForceDrop;

public sealed class ForceDropPackingOrderTests
{
    [Fact]
    public void RingVariant_PrecedesCorruptionInPacking()
    {
        // Runtime failure: corruption 1016 preceded fixed modifier 1131, and
        // decoding assigned FromCorruption to 1131. Put the variant first.
        Assert.Equal(
            new[] { 1, 0 },
            ForceDropPackingOrder.UniquePrefixIndices(new[] { 1016, 1131 }, new[] { 1131 })
        );
    }

    [Fact]
    public void PrefixingVariants_PreservesNativeSealAndOrdinaryOrder()
    {
        int[] ids = { 419, 1016, 72, 1131, 100 };
        var order = ForceDropPackingOrder.UniquePrefixIndices(ids, new[] { 1131 });
        Assert.Equal(new[] { 1131, 419, 1016, 72, 100 }, order.Select(i => ids[i]));
    }

    [Fact]
    public void GloveVariants_BothPrecedeOtherAffixesWithoutReconstruction()
    {
        var source = new[]
        {
            new PackedForceDropAffix(13, 6, 217, ForceDropSeal.None, 0),
            new PackedForceDropAffix(1143, 0, 255, ForceDropSeal.None, 7),
            new PackedForceDropAffix(1016, 3, 85, ForceDropSeal.Corruption, 6),
            new PackedForceDropAffix(1140, 0, 255, ForceDropSeal.None, 7),
        };
        var order = ForceDropPackingOrder.UniquePrefixIndices(
            source.Select(a => a.Id).ToArray(),
            new[] { 1140, 1143 }
        );
        var ordered = order.Select(i => source[i]).ToArray();
        Assert.Equal(new[] { 1143, 1140, 13, 1016 }, ordered.Select(a => a.Id));
        Assert.Same(source[0], ordered[2]);
        Assert.Same(source[2], ordered[3]);
        Assert.Equal(217, ordered[2].Roll);
        Assert.Equal(ForceDropSeal.Corruption, ordered[3].Seal);
    }

    [Fact]
    public void PreparingAgain_DoesNotChangeCanonicalOrder()
    {
        int[] ids = { 1131, 419, 1016, 72, 100 };
        Assert.Equal(
            Enumerable.Range(0, ids.Length),
            ForceDropPackingOrder.UniquePrefixIndices(ids, new[] { 1131 })
        );
        Assert.Equal(
            Enumerable.Range(0, ids.Length),
            ForceDropPackingOrder.UniquePrefixIndices(ids, Array.Empty<int>())
        );
    }

    [Fact]
    public void ASelectedModifierCannotBeInventedWhenAbsent()
    {
        Assert.Throws<ArgumentException>(() =>
            ForceDropPackingOrder.UniquePrefixIndices(new[] { 13, 1016 }, new[] { 1131 })
        );
    }

    [Fact]
    public void DuplicateItemOrVariantIdsRemainRejected()
    {
        Assert.Throws<ArgumentException>(() =>
            ForceDropPackingOrder.UniquePrefixIndices(new[] { 1131, 1131 }, new[] { 1131 })
        );
        Assert.Throws<ArgumentException>(() =>
            ForceDropPackingOrder.UniquePrefixIndices(new[] { 1131, 1016 }, new[] { 1131, 1131 })
        );
    }
}
