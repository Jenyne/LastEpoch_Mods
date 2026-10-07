using LastEpoch_Hud.Scripts.Core.ForceDrop;

namespace LastEpoch_Hud.Tests.Core.ForceDrop;

public sealed class ForceDropPackingSnapshotTests
{
    static readonly PackedForceDropAffix[] SealedAndCorrupted =
    {
        new(419, 4, 101, ForceDropSeal.Regular, 0),
        new(416, 6, 255, ForceDropSeal.None, 0),
        new(1020, 6, 87, ForceDropSeal.Corruption, 6),
    };

    [Fact]
    public void NativeReordering_IsAllowedWithoutLosingSealOwnership()
    {
        Assert.Equal(
            "",
            Snapshot().Difference(Snapshot(affixes: SealedAndCorrupted.Reverse().ToArray()))
        );
    }

    [Fact]
    public void OrdinaryItems_AlsoRequireARoundTrip()
    {
        var affixes = new[] { new PackedForceDropAffix(13, 6, 255, ForceDropSeal.None, 0) };
        var saved = new[] { new PackedForceDropAffix(13, 5, 255, ForceDropSeal.None, 0) };
        Assert.NotEqual(
            "",
            Snapshot(affixes: affixes, regular: false, corruption: false, corrupted: false)
                .Difference(
                    Snapshot(affixes: saved, regular: false, corruption: false, corrupted: false)
                )
        );
    }

    [Fact]
    public void WrongAffixReceivingCorruptionSeal_IsRejected()
    {
        // Regression from the user's packing log: an ordinary affix received
        // FromCorruption while requested 1020 became unsealed.
        var saved = new[]
        {
            new PackedForceDropAffix(419, 4, 101, ForceDropSeal.Regular, 0),
            new PackedForceDropAffix(416, 6, 255, ForceDropSeal.Corruption, 0),
            new PackedForceDropAffix(1020, 6, 87, ForceDropSeal.None, 6),
        };
        Assert.Contains("seal ownership", Snapshot().Difference(Snapshot(affixes: saved)));
    }

    [Fact]
    public void ResamplingTheCorruptionRoll_IsDetected()
    {
        var saved = SealedAndCorrupted.ToArray();
        saved[2] = new PackedForceDropAffix(1020, 6, 88, ForceDropSeal.Corruption, 6);
        Assert.NotEqual("", Snapshot().Difference(Snapshot(affixes: saved)));
    }

    [Fact]
    public void ChangedPlacementMetadata_IsDetected()
    {
        var saved = SealedAndCorrupted.ToArray();
        saved[2] = new PackedForceDropAffix(1020, 6, 87, ForceDropSeal.Corruption, 6, 1);
        Assert.NotEqual("", Snapshot().Difference(Snapshot(affixes: saved)));
    }

    [Theory]
    [InlineData("base")]
    [InlineData("subtype")]
    [InlineData("unique")]
    [InlineData("rarity")]
    [InlineData("lp")]
    [InlineData("ww")]
    [InlineData("fp")]
    [InlineData("implicit")]
    [InlineData("uniqueRoll")]
    [InlineData("corrupted")]
    [InlineData("regular")]
    [InlineData("corruption")]
    [InlineData("sockets")]
    public void CompleteItemChanges_AreRejected(string changed)
    {
        var actual = Snapshot(
            itemType: changed == "base" ? 4 : 0,
            subType: changed == "subtype" ? 2 : 1,
            uniqueId: changed == "unique" ? 600 : 500,
            rarity: changed == "rarity" ? 7 : 9,
            lp: changed == "lp" ? 3 : 4,
            ww: changed == "ww" ? 1 : 0,
            fp: changed == "fp" ? 1 : 0,
            implicits: changed == "implicit" ? new[] { 1, 128, 0 } : null,
            uniqueRolls: changed == "uniqueRoll" ? new[] { 1, 255 } : null,
            corrupted: changed != "corrupted",
            regular: changed != "regular",
            corruption: changed != "corruption",
            sockets: changed == "sockets" ? 4 : null
        );
        Assert.NotEqual("", Snapshot().Difference(actual));
    }

    [Fact]
    public void DuplicateIds_AreRejectedEvenWithMatchingSnapshots()
    {
        var snapshot = Snapshot(
            affixes: new[]
            {
                new PackedForceDropAffix(1, 0, 0, ForceDropSeal.None, 0),
                new PackedForceDropAffix(1, 0, 0, ForceDropSeal.None, 0),
            },
            regular: false,
            corruption: false
        );
        Assert.Contains("Duplicate", snapshot.Difference(snapshot));
    }

    [Fact]
    public void PrimordialCannotSilentlyReplaceRegularSeal()
    {
        var affixes = SealedAndCorrupted.ToArray();
        affixes[0] = new PackedForceDropAffix(419, 7, 101, ForceDropSeal.Primordial, 0);
        Assert.NotEqual(
            "",
            Snapshot().Difference(Snapshot(affixes: affixes, regular: false, primordial: true))
        );
    }

    static ForceDropPackingSnapshot Snapshot(
        PackedForceDropAffix[] affixes = null,
        int itemType = 0,
        int subType = 1,
        int uniqueId = 500,
        int rarity = 9,
        int fp = 0,
        int lp = 4,
        int ww = 0,
        bool corrupted = true,
        bool regular = true,
        bool primordial = false,
        bool corruption = true,
        int? sockets = null,
        int[] implicits = null,
        int[] uniqueRolls = null
    )
    {
        affixes ??= SealedAndCorrupted;
        return new ForceDropPackingSnapshot(
            itemType,
            subType,
            uniqueId,
            rarity,
            fp,
            lp,
            ww,
            corrupted,
            sockets ?? affixes.Length,
            regular,
            primordial,
            corruption,
            implicits ?? new[] { 255, 128, 0 },
            uniqueRolls ?? new[] { 255, 255 },
            affixes
        );
    }
}
