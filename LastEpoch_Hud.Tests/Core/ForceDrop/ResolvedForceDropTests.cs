using LastEpoch_Hud.Scripts.Core.ForceDrop;

namespace LastEpoch_Hud.Tests.Core.ForceDrop;

public sealed class ResolvedForceDropTests
{
    [Theory]
    [InlineData(1, 1)]
    [InlineData(2, 1)]
    [InlineData(3, 1)]
    [InlineData(4, 1)]
    [InlineData(4, 2)]
    [InlineData(4, 3)]
    [InlineData(4, 4)]
    public void UniqueWithTransferredAffixes_ConsumesAllLegendaryPotential(int lp, int count)
    {
        var affixes = Enumerable
            .Range(1, count)
            .Select(id => new ResolvedForceDropAffix(id, 6, 255, ForceDropSeal.None));
        var request = new ResolvedForceDrop(
            21,
            10,
            477,
            7,
            0,
            lp,
            0,
            false,
            new[] { 255, 255, 255 },
            new int[8],
            affixes,
            new[] { 1131 },
            null
        );
        Assert.Equal(0, request.LegendaryPotential);
        Assert.Equal(count, request.Affixes.Count);
        Assert.Equal(1131, request.VariantIds.Single());
    }

    [Theory]
    [InlineData(1, 7, 1)]
    [InlineData(8, 7, 7)]
    [InlineData(8, 8, 8)]
    [InlineData(0, 7, 0)]
    [InlineData(7, 8, 7)]
    [InlineData(20, 8, 8)]
    public void TierRange_UsesDefinitionAndRoute(int count, int route, int expected)
    {
        Assert.Equal(expected, ForceDropTierRules.MaximumDisplayTier(count, route));
        Assert.False(ForceDropTierRules.Supports(expected, count, route));
        if (expected > 0)
            Assert.True(ForceDropTierRules.Supports(expected - 1, count, route));
    }

    [Fact]
    public void CorruptedRequest_ConsumesPotentialButKeepsEverySelectedModifier()
    {
        var affix = new ResolvedForceDropAffix(13, 6, 255, ForceDropSeal.None);
        var corruption = new ResolvedForceDropAffix(1074, 6, 255, ForceDropSeal.Corruption);
        var request = new ResolvedForceDrop(
            21,
            10,
            477,
            7,
            100,
            4,
            28,
            true,
            new[] { 255, 255, 255 },
            Enumerable.Repeat(255, 8),
            new[] { affix },
            new[] { 1131 },
            corruption
        );
        Assert.Equal(0, request.ForgingPotential);
        Assert.Equal(0, request.LegendaryPotential);
        Assert.Equal(0, request.WeaversWill);
        Assert.Same(affix, request.Affixes.Single());
        Assert.Equal(1131, request.VariantIds.Single());
        Assert.Same(corruption, request.Corruption);
    }

    [Theory]
    [InlineData(100, 4, 0)]
    [InlineData(0, 0, 28)]
    public void UncorruptedRequest_PreservesPotential(int fp, int lp, int ww)
    {
        var request = new ResolvedForceDrop(
            21,
            10,
            477,
            7,
            fp,
            lp,
            ww,
            false,
            new[] { 255, 255, 255 },
            Enumerable.Repeat(255, 8),
            Array.Empty<ResolvedForceDropAffix>(),
            new[] { 1131 },
            null
        );
        Assert.Equal(fp, request.ForgingPotential);
        Assert.Equal(lp, request.LegendaryPotential);
        Assert.Equal(ww, request.WeaversWill);
    }

    [Fact]
    public void OrdinaryIdol_CannotUseEquipmentTiers()
    {
        // Live catalog: Standard/Idols definitions have one native tier.
        Assert.True(ForceDropTierRules.Supports(0, 1, 7));
        Assert.False(ForceDropTierRules.Supports(6, 1, 7));
        Assert.False(ForceDropTierRules.Supports(7, 1, 8));
    }

    [Fact]
    public void ResolvedRequest_OwnsItsValuesAfterThePickerChanges()
    {
        int[] rolls = { 255, 128, 0 };
        int[] variants = { 900 };
        var affixes = new List<ResolvedForceDropAffix> { new(13, 6, 125, ForceDropSeal.None) };
        var request = Request(rolls: rolls, variants: variants, affixes: affixes);
        rolls[0] = 0;
        variants[0] = 901;
        affixes.Clear();
        Assert.Equal(255, request.ImplicitRolls[0]);
        Assert.Equal(900, request.VariantIds[0]);
        Assert.Equal(125, request.Affixes.Single().Roll);
        Assert.Throws<NotSupportedException>(() => ((IList<int>)request.ImplicitRolls)[0] = 10);
    }

    [Fact]
    public void SevenAffixesIncludeBothSealsAndIndependentCorruption()
    {
        var affixes = new[]
        {
            new ResolvedForceDropAffix(11, 6, 200, ForceDropSeal.None),
            new ResolvedForceDropAffix(12, 6, 201, ForceDropSeal.None),
            new ResolvedForceDropAffix(13, 6, 202, ForceDropSeal.None),
            new ResolvedForceDropAffix(14, 6, 203, ForceDropSeal.None),
            new ResolvedForceDropAffix(15, 5, 204, ForceDropSeal.Regular),
            new ResolvedForceDropAffix(16, 7, 205, ForceDropSeal.Primordial),
        };
        var corruption = new ResolvedForceDropAffix(17, 6, 206, ForceDropSeal.Corruption);
        var request = Request(affixes: affixes, corruption: corruption);
        Assert.Equal(6, request.Affixes.Count);
        Assert.Equal(7, request.Affixes.Count + 1);
        Assert.Equal(1, request.Affixes.Count(a => a.Seal == ForceDropSeal.Regular));
        Assert.Equal(1, request.Affixes.Count(a => a.Seal == ForceDropSeal.Primordial));
        Assert.Equal(ForceDropSeal.Corruption, request.Corruption.Seal);
    }

    [Fact]
    public void RegularSealAndCorruption_AreIndependentSlots()
    {
        var request = Request(
            affixes: new[] { new ResolvedForceDropAffix(419, 4, 101, ForceDropSeal.Regular) },
            corruption: new ResolvedForceDropAffix(1020, 6, 87, ForceDropSeal.Corruption)
        );
        Assert.Equal(ForceDropSeal.Regular, request.Affixes.Single().Seal);
        Assert.Equal(87, request.Corruption.Roll);
    }

    [Theory]
    [InlineData(-1, 0, 0)]
    [InlineData(65536, 0, 0)]
    [InlineData(1, 8, 0)]
    [InlineData(1, -1, 0)]
    [InlineData(1, 0, 256)]
    [InlineData(1, 0, -1)]
    public void PackedBounds_AreNeverRelaxed(int id, int tier, int roll)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new ResolvedForceDropAffix(id, tier, roll, ForceDropSeal.None)
        );
    }

    [Fact]
    public void CorruptionCannotReuseOrdinaryAffixId()
    {
        Assert.Throws<ArgumentException>(() =>
            Request(
                affixes: new[] { new ResolvedForceDropAffix(1, 0, 255, ForceDropSeal.None) },
                corruption: new ResolvedForceDropAffix(1, 0, 255, ForceDropSeal.Corruption)
            )
        );
    }

    [Fact]
    public void VariantCannotReuseOrdinaryAffixId()
    {
        Assert.Throws<ArgumentException>(() =>
            Request(
                variants: new[] { 1 },
                affixes: new[] { new ResolvedForceDropAffix(1, 0, 255, ForceDropSeal.None) }
            )
        );
    }

    [Fact]
    public void TwoRegularSeals_AreNotOneSlot()
    {
        Assert.Throws<ArgumentException>(() =>
            Request(
                affixes: new[]
                {
                    new ResolvedForceDropAffix(1, 0, 255, ForceDropSeal.Regular),
                    new ResolvedForceDropAffix(2, 0, 255, ForceDropSeal.Regular),
                }
            )
        );
    }

    static ResolvedForceDrop Request(
        int[] rolls = null,
        int[] variants = null,
        IEnumerable<ResolvedForceDropAffix> affixes = null,
        ResolvedForceDropAffix corruption = null
    )
    {
        return new ResolvedForceDrop(
            0,
            1,
            0,
            0,
            0,
            0,
            0,
            corruption != null,
            rolls ?? new[] { 255, 255, 255 },
            new int[8],
            affixes ?? Array.Empty<ResolvedForceDropAffix>(),
            variants ?? Array.Empty<int>(),
            corruption
        );
    }
}
