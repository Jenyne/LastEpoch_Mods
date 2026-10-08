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
    public void UniqueVariantAndCorruption_CanHaveAnIndependentSocketCount()
    {
        var affixes = new[]
        {
            new PackedForceDropAffix(1131, 0, 255, ForceDropSeal.None, 7),
            new PackedForceDropAffix(1016, 6, 255, ForceDropSeal.Corruption, 6),
        };
        var live = Snapshot(affixes: affixes, regular: false, sockets: 1);
        Assert.Equal("", live.Difference(Snapshot(affixes: affixes, regular: false, sockets: 0)));
        Assert.Equal("", live.Difference(Snapshot(affixes: affixes, regular: false, sockets: 1)));
        // A different nonzero stored value is still a round-trip mismatch.
        Assert.NotEqual(
            "",
            live.Difference(Snapshot(affixes: affixes, regular: false, sockets: 2))
        );
    }

    [Fact]
    public void RingModifierCannotReceiveTheCorruptionSealInstead()
    {
        var expected = new[]
        {
            new PackedForceDropAffix(1131, 0, 255, ForceDropSeal.None, 7),
            new PackedForceDropAffix(1016, 6, 255, ForceDropSeal.Corruption, 6),
        };
        var swapped = new[]
        {
            new PackedForceDropAffix(1016, 6, 255, ForceDropSeal.None, 6),
            new PackedForceDropAffix(1131, 0, 255, ForceDropSeal.Corruption, 7),
        };
        Assert.Contains(
            "seal ownership",
            Snapshot(affixes: expected, regular: false, sockets: 1)
                .Difference(Snapshot(affixes: swapped, regular: false, sockets: 1))
        );
    }

    [Fact]
    public void UnsatedRuntimeRegression_ZeroDecodedSocketsPreservesTheModifier()
    {
        Assert.Equal("", Unsated(sockets: 1).Difference(Unsated(sockets: 0)));
    }

    [Theory]
    [InlineData("missing")]
    [InlineData("id")]
    [InlineData("tier")]
    [InlineData("roll")]
    [InlineData("special")]
    [InlineData("placement")]
    public void UniqueZeroSockets_DoesNotHideChangedOrMissingAffixes(string changed)
    {
        var affixes =
            changed == "missing"
                ? Array.Empty<PackedForceDropAffix>()
                : new[]
                {
                    new PackedForceDropAffix(
                        changed == "id" ? 1137 : 1138,
                        changed == "tier" ? 1 : 0,
                        changed == "roll" ? 254 : 255,
                        ForceDropSeal.None,
                        changed == "special" ? 0 : 7,
                        changed == "placement" ? 1 : 0
                    ),
                };
        Assert.NotEqual("", Unsated(1).Difference(Unsated(0, affixes)));
    }

    [Theory]
    [InlineData(7)]
    [InlineData(8)]
    [InlineData(9)]
    public void UniqueFamily_ZeroSocketsDoesNotEraseIndependentSeals(int rarity)
    {
        Assert.Equal("", Snapshot(rarity: rarity).Difference(Snapshot(rarity: rarity, sockets: 0)));
        var changed = SealedAndCorrupted.ToArray();
        changed[0] = new PackedForceDropAffix(419, 4, 101, ForceDropSeal.None, 0);
        Assert.NotEqual(
            "",
            Snapshot(rarity: rarity)
                .Difference(Snapshot(rarity: rarity, sockets: 0, affixes: changed, regular: false))
        );
    }

    [Fact]
    public void BaseItem_ZeroDecodedSocketsPreservesTheAffixButZeroLiveSocketsRemainInvalid()
    {
        var affixes = new[] { new PackedForceDropAffix(13, 6, 255, ForceDropSeal.None, 0) };
        var expected = Snapshot(
            affixes: affixes,
            rarity: 1,
            uniqueId: 0,
            regular: false,
            corruption: false,
            corrupted: false,
            lp: 0
        );
        var actual = Snapshot(
            affixes: affixes,
            rarity: 1,
            uniqueId: 0,
            regular: false,
            corruption: false,
            corrupted: false,
            lp: 0,
            sockets: 0
        );
        Assert.Equal("", expected.Difference(actual));
        Assert.Contains("Socket count", actual.IntegrityError());
        Assert.Contains("Before packing: Socket count", actual.Difference(expected));
    }

    static ForceDropPackingSnapshot Unsated(int sockets, PackedForceDropAffix[] affixes = null)
    {
        // Exact values from the user's 77512efd runtime rejection, 11:23:50.
        return new ForceDropPackingSnapshot(
            21,
            10,
            477,
            7,
            0,
            0,
            0,
            false,
            sockets,
            false,
            false,
            false,
            new[] { 255, 255, 255 },
            Enumerable.Repeat(255, 8),
            affixes ?? new[] { new PackedForceDropAffix(1138, 0, 255, ForceDropSeal.None, 7, 0) }
        );
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void CorruptedBaseRuntimeRegression_PreservesEveryAffixWithZeroDecodedSockets(bool idol)
    {
        var live = CorruptedBase(idol, idol ? 4 : 5);
        var decoded = CorruptedBase(idol, 0);
        Assert.Equal("", live.IntegrityError());
        Assert.Equal("", decoded.IntegrityError(decoded: true));
        Assert.Equal("", live.Difference(decoded));
        Assert.Equal("", live.Difference(CorruptedBase(idol, live.Sockets)));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void CorruptedBase_DoesNotAcceptAnArbitrarySocketCount(bool idol)
    {
        var live = CorruptedBase(idol, idol ? 4 : 5);
        Assert.Contains("Socket count", live.Difference(CorruptedBase(idol, 2)));
        Assert.Contains(
            "socket count changed",
            live.Difference(CorruptedBase(idol, live.Affixes.Count))
        );
    }

    [Theory]
    [InlineData(true, "missing")]
    [InlineData(true, "id")]
    [InlineData(true, "tier")]
    [InlineData(true, "roll")]
    [InlineData(true, "special")]
    [InlineData(true, "placement")]
    [InlineData(true, "seal")]
    [InlineData(false, "missing")]
    [InlineData(false, "id")]
    [InlineData(false, "tier")]
    [InlineData(false, "roll")]
    [InlineData(false, "special")]
    [InlineData(false, "placement")]
    [InlineData(false, "seal")]
    public void CorruptedBase_ZeroSocketsDoesNotHideMissingOrChangedAffixes(
        bool idol,
        string changed
    )
    {
        var live = CorruptedBase(idol, idol ? 4 : 5);
        var saved = live.Affixes.ToArray();
        int index = Array.FindIndex(saved, a => a.Seal == ForceDropSeal.Corruption);
        var affix = saved[index];
        if (changed == "missing")
            saved = saved.Where((_, i) => i != index).ToArray();
        else
            saved[index] = new PackedForceDropAffix(
                changed == "id" ? affix.Id + 1 : affix.Id,
                changed == "tier" ? affix.Tier + 1 : affix.Tier,
                changed == "roll" ? affix.Roll - 1 : affix.Roll,
                changed == "seal" ? ForceDropSeal.None : affix.Seal,
                changed == "special" ? 0 : affix.SpecialType,
                changed == "placement" ? 1 : affix.AffixType
            );
        Assert.NotEqual("", live.Difference(CorruptedBase(idol, 0, saved)));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void CorruptedBase_ZeroDecodedSocketsDoesNotHideALostCorruptionSeal(bool idol)
    {
        var live = CorruptedBase(idol, idol ? 4 : 5);
        var saved = live.Affixes.Where(a => a.Seal != ForceDropSeal.Corruption).ToArray();
        Assert.Contains(
            "corruption or seal flags",
            live.Difference(CorruptedBase(idol, 0, saved, corruption: false))
        );
    }

    [Fact]
    public void CorruptedBase_ZeroSocketsDoesNotHideALostRegularSeal()
    {
        var live = CorruptedBase(false, 5);
        var saved = live.Affixes.ToArray();
        var seal = saved[0];
        saved[0] = new PackedForceDropAffix(
            seal.Id,
            seal.Tier,
            seal.Roll,
            ForceDropSeal.None,
            seal.SpecialType,
            seal.AffixType
        );
        Assert.Contains(
            "corruption or seal flags",
            live.Difference(CorruptedBase(false, 0, saved, regular: false))
        );
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, false)]
    public void SetBaseRuntimeRegression_ZeroDecodedSocketsPreservesTheCompleteItem(
        bool regular,
        bool corrupted
    )
    {
        var live = SetRing(regular, corrupted, regular ? 5 : 4);
        Assert.Equal("", live.IntegrityError());
        Assert.Equal("", live.Difference(SetRing(regular, corrupted, 0)));
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(true, false)]
    [InlineData(false, false)]
    public void SetBase_AnUnrelatedNonzeroDecodedCountRemainsRejected(bool regular, bool corrupted)
    {
        Assert.Contains(
            "Socket count",
            SetRing(regular, corrupted, regular ? 5 : 4).Difference(SetRing(regular, corrupted, 2))
        );
    }

    [Theory]
    [InlineData(true, true, "missing")]
    [InlineData(true, true, "tier")]
    [InlineData(true, true, "roll")]
    [InlineData(true, true, "set")]
    [InlineData(true, true, "seal")]
    [InlineData(true, false, "missing")]
    [InlineData(true, false, "tier")]
    [InlineData(true, false, "roll")]
    [InlineData(true, false, "set")]
    [InlineData(true, false, "seal")]
    [InlineData(false, false, "missing")]
    [InlineData(false, false, "tier")]
    [InlineData(false, false, "roll")]
    [InlineData(false, false, "set")]
    [InlineData(false, false, "seal")]
    public void SetBase_ZeroDecodedSocketsDoesNotHideChangedAffixes(
        bool regular,
        bool corrupted,
        string changed
    )
    {
        var live = SetRing(regular, corrupted, regular ? 5 : 4);
        var saved = live.Affixes.ToArray();
        int index = Array.FindIndex(saved, a => a.SpecialType == 3);
        var affix = saved[index];
        if (changed == "missing")
            saved = saved.Where((_, i) => i != index).ToArray();
        else
            saved[index] = new PackedForceDropAffix(
                affix.Id,
                changed == "tier" ? 5 : affix.Tier,
                changed == "roll" ? 254 : affix.Roll,
                changed == "seal" ? ForceDropSeal.Regular : affix.Seal,
                changed == "set" ? 0 : affix.SpecialType,
                affix.AffixType
            );
        Assert.NotEqual("", live.Difference(SetRing(regular, corrupted, 0, saved)));
    }

    static ForceDropPackingSnapshot SetRing(
        bool regular,
        bool corrupted,
        int sockets,
        PackedForceDropAffix[] affixes = null
    )
    {
        // Exact rejections from build 33fedcf1, 13:20:38/50 and 13:21:05.
        affixes ??= regular
            ? new[]
            {
                new PackedForceDropAffix(9, 6, 255, ForceDropSeal.Regular, 0, 0),
                new PackedForceDropAffix(959, 6, 255, ForceDropSeal.None, 3, 0),
                new PackedForceDropAffix(502, 6, 255, ForceDropSeal.None, 0, 0),
                new PackedForceDropAffix(824, 6, 255, ForceDropSeal.None, 0, 1),
                new PackedForceDropAffix(25, 6, 255, ForceDropSeal.None, 0, 1),
            }
            : new[]
            {
                new PackedForceDropAffix(960, 6, 255, ForceDropSeal.None, 3, 0),
                new PackedForceDropAffix(70, 6, 255, ForceDropSeal.None, 0, 0),
                new PackedForceDropAffix(24, 6, 255, ForceDropSeal.None, 0, 1),
                new PackedForceDropAffix(13, 6, 255, ForceDropSeal.None, 0, 1),
            };
        return new ForceDropPackingSnapshot(
            21,
            11,
            0,
            4,
            corrupted ? 0 : 63,
            0,
            0,
            corrupted,
            sockets,
            regular,
            false,
            false,
            new[] { 255, 255, 255 },
            Array.Empty<int>(),
            affixes
        );
    }

    static ForceDropPackingSnapshot CorruptedBase(
        bool idol,
        int sockets,
        PackedForceDropAffix[] affixes = null,
        bool? regular = null,
        bool corruption = true
    )
    {
        // Exact live/decoded data from build 0805d229, 13:07:53 and 13:09:17.
        affixes ??= idol
            ? new[]
            {
                new PackedForceDropAffix(1070, 0, 255, ForceDropSeal.Corruption, 6, 0),
                new PackedForceDropAffix(757, 0, 255, ForceDropSeal.None, 0, 0),
                new PackedForceDropAffix(938, 6, 255, ForceDropSeal.None, 4, 1),
                new PackedForceDropAffix(296, 0, 255, ForceDropSeal.None, 0, 1),
                new PackedForceDropAffix(897, 6, 255, ForceDropSeal.None, 4, 1),
            }
            : new[]
            {
                new PackedForceDropAffix(502, 6, 255, ForceDropSeal.Regular, 0, 0),
                new PackedForceDropAffix(995, 0, 255, ForceDropSeal.Corruption, 6, 0),
                new PackedForceDropAffix(72, 0, 255, ForceDropSeal.None, 0, 0),
                new PackedForceDropAffix(960, 6, 255, ForceDropSeal.None, 3, 0),
                new PackedForceDropAffix(825, 0, 255, ForceDropSeal.None, 0, 1),
                new PackedForceDropAffix(45, 6, 255, ForceDropSeal.None, 0, 1),
            };
        return new ForceDropPackingSnapshot(
            idol ? 33 : 21,
            idol ? 10 : 11,
            0,
            4,
            0,
            0,
            0,
            true,
            sockets,
            regular ?? !idol,
            false,
            corruption,
            new[] { 255, 255, 255 },
            Array.Empty<int>(),
            affixes
        );
    }

    [Fact]
    public void SocketRejection_RecordsBothCountsAndDecodedAffixes()
    {
        string error = Snapshot().Difference(Snapshot(sockets: 2));
        Assert.Contains("socket count changed", error);
        Assert.Contains("sockets=2, count=3", error);
        Assert.Contains("expected={", error);
        Assert.Contains("decoded={", error);
        Assert.Contains("1020:6:87:Corruption:6:0", error);
    }

    [Fact]
    public void AffixRejection_RecordsTheExactChangedRoll()
    {
        var saved = SealedAndCorrupted.ToArray();
        saved[2] = new PackedForceDropAffix(1020, 6, 88, ForceDropSeal.Corruption, 6);
        string error = Snapshot().Difference(Snapshot(affixes: saved));
        Assert.Contains("1020:6:87:Corruption:6:0", error);
        Assert.Contains("1020:6:88:Corruption:6:0", error);
    }

    [Fact]
    public void CorruptionAffixSurvivingWithoutItsSeal_RemainsRejected()
    {
        var saved = SealedAndCorrupted.ToArray();
        saved[2] = new PackedForceDropAffix(1020, 6, 87, ForceDropSeal.None, 6);
        // The user's 57fef5ef log kept its chosen corruption ID while losing
        // both the FromCorruption seal and presence flag during refresh.
        string error = Snapshot()
            .Difference(Snapshot(affixes: saved, sockets: 0, corruption: false));
        Assert.Contains("corruption or seal flags", error);
    }

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

    [Fact]
    public void IndependentPrimordialSealMustSurvivePackingAsT8()
    {
        var affixes = SealedAndCorrupted
            .Append(new PackedForceDropAffix(8, 7, 125, ForceDropSeal.Primordial, 0))
            .ToArray();
        var expected = Snapshot(affixes: affixes, primordial: true);
        Assert.Equal(
            "",
            expected.Difference(Snapshot(affixes: affixes.Reverse().ToArray(), primordial: true))
        );
        Assert.NotEqual("", expected.Difference(Snapshot(affixes: affixes, primordial: false)));
        affixes[3] = new PackedForceDropAffix(8, 6, 125, ForceDropSeal.Primordial, 0);
        Assert.NotEqual("", expected.Difference(Snapshot(affixes: affixes, primordial: true)));
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
