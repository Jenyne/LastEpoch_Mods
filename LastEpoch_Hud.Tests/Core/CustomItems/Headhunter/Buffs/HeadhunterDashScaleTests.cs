using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Buffs;

namespace LastEpoch_Hud.Tests.Core.CustomItems.Headhunter.Buffs;

public sealed class HeadhunterDashScaleTests
{
    public static readonly DashMovement[] FixedTypes =
    [
        DashMovement.FixedDistanceDash,
        DashMovement.FixedDistanceDashThenWait,
        DashMovement.FixedDistanceVariableSpeedDash,
        DashMovement.FixedDistanceDashThenWaitThenDash,
        DashMovement.FixedDistanceLeap,
    ];

    private static readonly IntPtr _player = (IntPtr)42;
    private static readonly IntPtr _other = (IntPtr)7;

    public static TheoryData<DashMovement> Fixed() => new(FixedTypes);

    public static TheoryData<DashMovement> NotFixed() =>
        new(Enum.GetValues<DashMovement>().Except(FixedTypes));

    [Theory]
    [MemberData(nameof(Fixed))]
    public void Scales_FixedDistanceTypes_True(DashMovement movement)
    {
        Assert.True(HeadhunterDashScale.Scales(movement));
    }

    [Theory]
    [MemberData(nameof(NotFixed))]
    public void Scales_OtherTypes_False(DashMovement movement)
    {
        Assert.False(HeadhunterDashScale.Scales(movement));
    }

    [Fact]
    public void DistanceFor_PlayerFixedLeap_Scaled()
    {
        float result = Grown().DistanceFor(DashMovement.FixedDistanceLeap, _player, _player, 5f);

        Assert.Equal(6f, result, 5);
    }

    [Fact]
    public void DistanceFor_PlayerFixedDash_Scaled()
    {
        float result = Grown().DistanceFor(DashMovement.FixedDistanceDash, _player, _player, 5f);

        Assert.Equal(6f, result, 5);
    }

    [Fact]
    public void DistanceFor_Leap_Unchanged()
    {
        float result = Grown().DistanceFor(DashMovement.Leap, _player, _player, 5f);

        Assert.Equal(5f, result);
    }

    [Fact]
    public void DistanceFor_OtherActor_Unchanged()
    {
        float result = Grown().DistanceFor(DashMovement.FixedDistanceDash, _other, _player, 5f);

        Assert.Equal(5f, result);
    }

    [Fact]
    public void DistanceFor_NoPlayer_Unchanged()
    {
        float result = Grown()
            .DistanceFor(DashMovement.FixedDistanceDash, IntPtr.Zero, IntPtr.Zero, 5f);

        Assert.Equal(5f, result);
    }

    [Fact]
    public void DistanceFor_Fresh_Unchanged()
    {
        var scale = new HeadhunterDashScale();

        float result = scale.DistanceFor(DashMovement.FixedDistanceDash, _player, _player, 3.7f);

        Assert.Equal(3.7f, result);
    }

    [Fact]
    public void DistanceFor_AfterReset_Unchanged()
    {
        HeadhunterDashScale scale = Grown();

        scale.Reset();

        Assert.Equal(
            3.7f,
            scale.DistanceFor(DashMovement.FixedDistanceDash, _player, _player, 3.7f)
        );
    }

    [Theory]
    [InlineData(0f)]
    [InlineData(-1f)]
    [InlineData(float.MaxValue)]
    [InlineData(float.PositiveInfinity)]
    public void DistanceFor_Sentinel_Unchanged(float distance)
    {
        float result = Grown()
            .DistanceFor(DashMovement.FixedDistanceDash, _player, _player, distance);

        Assert.Equal(distance, result);
    }

    [Fact]
    public void DistanceFor_NaN_StaysNaN()
    {
        float result = Grown()
            .DistanceFor(DashMovement.FixedDistanceDash, _player, _player, float.NaN);

        Assert.True(float.IsNaN(result));
    }

    private static HeadhunterDashScale Grown()
    {
        var scale = new HeadhunterDashScale();
        scale.Set(1.2f);
        return scale;
    }
}
