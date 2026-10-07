using System;
using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Buffs;

namespace LastEpoch_Hud.Tests.Core.CustomItems.Headhunter.Buffs;

public sealed class HeadhunterRendererMatchTests
{
    private static readonly IntPtr _managerA = new(100);
    private static readonly IntPtr _managerB = new(200);
    private static readonly IntPtr _known = new(1);
    private static readonly IntPtr _unknown = new(2);

    [Fact]
    public void SameIds_Equal_True()
    {
        Assert.True(
            HeadhunterRendererMatch.SameIds(
                new[] { new IntPtr(1), new IntPtr(2) },
                new[] { new IntPtr(1), new IntPtr(2) }
            )
        );
    }

    [Fact]
    public void SameIds_NullEither_False()
    {
        IntPtr[] ids = new[] { new IntPtr(1) };

        Assert.False(HeadhunterRendererMatch.SameIds(null, ids));
        Assert.False(HeadhunterRendererMatch.SameIds(ids, null));
    }

    [Fact]
    public void SameIds_LengthDiffers_False()
    {
        Assert.False(
            HeadhunterRendererMatch.SameIds(
                new[] { new IntPtr(1), new IntPtr(2) },
                new[] { new IntPtr(1) }
            )
        );
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void SameIds_OneSlotDiffers_False(int slot)
    {
        IntPtr[] current = new[] { new IntPtr(1), new IntPtr(2), new IntPtr(3) };
        IntPtr[] expected = new[] { new IntPtr(1), new IntPtr(2), new IntPtr(3) };
        expected[slot] = new IntPtr(99);

        Assert.False(HeadhunterRendererMatch.SameIds(current, expected));
    }

    [Fact]
    public void SameIds_BothZeroSlot_True()
    {
        Assert.True(
            HeadhunterRendererMatch.SameIds(
                new[] { new IntPtr(1), IntPtr.Zero },
                new[] { new IntPtr(1), IntPtr.Zero }
            )
        );
    }

    [Fact]
    public void IsFresh_SameManagerUnknownRenderer_True()
    {
        Assert.True(
            HeadhunterRendererMatch.IsFresh(_managerA, _managerA, new[] { _known }, _unknown)
        );
    }

    [Fact]
    public void IsFresh_SameManagerKnownRenderer_False()
    {
        Assert.False(
            HeadhunterRendererMatch.IsFresh(_managerA, _managerA, new[] { _known }, _known)
        );
    }

    [Fact]
    public void IsFresh_OtherManager_False()
    {
        Assert.False(
            HeadhunterRendererMatch.IsFresh(_managerA, _managerB, new[] { _known }, _unknown)
        );
    }

    [Fact]
    public void IsFresh_NoPrevious_False()
    {
        Assert.False(
            HeadhunterRendererMatch.IsFresh(
                IntPtr.Zero,
                IntPtr.Zero,
                Array.Empty<IntPtr>(),
                _unknown
            )
        );
    }

    [Fact]
    public void ChangedInPlace_SamePointer_True()
    {
        Assert.True(HeadhunterRendererMatch.ChangedInPlace(new IntPtr(5), new IntPtr(5)));
    }

    [Fact]
    public void ChangedInPlace_CapturedZero_False()
    {
        Assert.False(HeadhunterRendererMatch.ChangedInPlace(IntPtr.Zero, IntPtr.Zero));
    }

    [Fact]
    public void ChangedInPlace_Replaced_False()
    {
        Assert.False(HeadhunterRendererMatch.ChangedInPlace(new IntPtr(5), new IntPtr(6)));
    }

    [Theory]
    [InlineData(0, 7, 7, HeadhunterRarityRestore.Clear)]
    [InlineData(5, 5, 5, HeadhunterRarityRestore.Clear)]
    [InlineData(5, 7, 7, HeadhunterRarityRestore.PutBack)]
    [InlineData(5, 7, 9, HeadhunterRarityRestore.Keep)]
    [InlineData(0, 7, 0, HeadhunterRarityRestore.Keep)]
    [InlineData(0, 0, 0, HeadhunterRarityRestore.Keep)]
    [InlineData(5, 0, 5, HeadhunterRarityRestore.Keep)]
    public void RarityRestore_PicksStep(
        long captured,
        long tinted,
        long current,
        HeadhunterRarityRestore expected
    )
    {
        Assert.Equal(
            expected,
            HeadhunterRendererMatch.RarityRestore(
                new IntPtr(captured),
                new IntPtr(tinted),
                new IntPtr(current)
            )
        );
    }
}
