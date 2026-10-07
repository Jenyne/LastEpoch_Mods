using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter;

namespace LastEpoch_Hud.Tests.Core.CustomItems.Headhunter;

public sealed class HeadhunterBarLayoutTests
{
    public static readonly TheoryData<SkillBarBounds, float, float> InvalidInputs = new()
    {
        { SkillBarBounds.Empty, 60f, 1f },
        { new SkillBarBounds(100, 10, 300, 10), 60f, 1f },
        { new SkillBarBounds(100, 60, 300, 10), 60f, 1f },
        { new SkillBarBounds(100, 10, 300, 60), 0f, 1f },
        { new SkillBarBounds(100, 10, 300, 60), -60f, 1f },
        { new SkillBarBounds(100, 10, 300, 60), 60f, 0f },
        { new SkillBarBounds(100, 10, 300, 60), 60f, -1f },
    };

    private static readonly SkillBarBounds _bar = new(100, 10, 300, 60);

    [Fact]
    public void TryPlace_CentersAboveBar()
    {
        bool ok = HeadhunterBarLayout.TryPlace(_bar, 60, 1, out HeadhunterBarPlacement placement);

        Assert.True(ok);
        Assert.Equal(200f, placement.X, 0.001f);
        Assert.Equal(60f + (50f * HeadhunterBarLayout.GapRatio), placement.Y, 0.001f);
        Assert.Equal((50f * HeadhunterBarLayout.SizeRatio) / 60f, placement.Scale, 0.001f);
    }

    [Fact]
    public void TryPlace_CanvasScale_DividesScaleOnly()
    {
        HeadhunterBarLayout.TryPlace(_bar, 60, 1, out HeadhunterBarPlacement one);
        HeadhunterBarLayout.TryPlace(_bar, 60, 2, out HeadhunterBarPlacement two);

        Assert.Equal(one.Scale / 2f, two.Scale, 0.001f);
        Assert.Equal(one.X, two.X, 0.001f);
        Assert.Equal(one.Y, two.Y, 0.001f);
    }

    [Theory]
    [MemberData(nameof(InvalidInputs))]
    public void TryPlace_Invalid_False(SkillBarBounds bounds, float entryHeight, float canvasScale)
    {
        Assert.False(HeadhunterBarLayout.TryPlace(bounds, entryHeight, canvasScale, out _));
    }

    [Fact]
    public void ShouldMove_FromDefault_True()
    {
        HeadhunterBarPlacement next = new(200, 70, 0.6f);

        Assert.True(HeadhunterBarLayout.ShouldMove(default, next, 60));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ShouldMove_SmallShift_False(bool moveX)
    {
        HeadhunterBarPlacement current = new(200, 70, 0.6f);
        float shift = 0.5f * HeadhunterBarLayout.MoveTolerance * current.Scale * 60;

        Assert.False(HeadhunterBarLayout.ShouldMove(current, Shift(current, moveX, shift), 60));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ShouldMove_LargeShiftOnOneAxis_True(bool moveX)
    {
        HeadhunterBarPlacement current = new(200, 70, 0.6f);
        float shift = 2f * HeadhunterBarLayout.MoveTolerance * current.Scale * 60;

        Assert.True(HeadhunterBarLayout.ShouldMove(current, Shift(current, moveX, shift), 60));
    }

    [Fact]
    public void ShouldMove_SmallScale_False()
    {
        HeadhunterBarPlacement current = new(200, 70, 0.6f);
        HeadhunterBarPlacement scaled = current with
        {
            Scale = current.Scale * (1 + (0.5f * HeadhunterBarLayout.MoveTolerance)),
        };

        Assert.False(HeadhunterBarLayout.ShouldMove(current, scaled, 60));
    }

    [Fact]
    public void ShouldMove_LargeScale_True()
    {
        HeadhunterBarPlacement current = new(200, 70, 0.6f);
        HeadhunterBarPlacement next = current with
        {
            Scale = current.Scale * (1 + (2f * HeadhunterBarLayout.MoveTolerance)),
        };

        Assert.True(HeadhunterBarLayout.ShouldMove(current, next, 60));
    }

    [Theory]
    [InlineData(2560f, 1080f)]
    [InlineData(1920f, 1440f)]
    public void ShouldMove_ScreenSizeChanged_True(float screenW, float screenH)
    {
        HeadhunterBarPlacement current = new(200, 70, 0.6f, 1920, 1080);
        HeadhunterBarPlacement next = current with { ScreenW = screenW, ScreenH = screenH };

        Assert.True(HeadhunterBarLayout.ShouldMove(current, next, 60));
    }

    [Fact]
    public void ShouldMove_SameEverything_False()
    {
        HeadhunterBarPlacement current = new(200, 70, 0.6f, 1920, 1080);
        HeadhunterBarPlacement next = current with { };

        Assert.False(HeadhunterBarLayout.ShouldMove(current, next, 60));
    }

    [Fact]
    public void ToLocal_CenterOfScreen_IsZero()
    {
        HeadhunterBarPlacement placement = new(960, 540, 1f);

        (float x, float y) = HeadhunterBarLayout.ToLocal(placement, 1920, 1080, 1f);

        Assert.Equal(0f, x, 0.001f);
        Assert.Equal(0f, y, 0.001f);
    }

    [Fact]
    public void ToLocal_DividesByCanvasScale()
    {
        HeadhunterBarPlacement placement = new(1060, 640, 1f);

        (float x, float y) = HeadhunterBarLayout.ToLocal(placement, 1920, 1080, 2f);

        Assert.Equal(50f, x, 0.001f);
        Assert.Equal(50f, y, 0.001f);
    }

    [Fact]
    public void ToLocal_UsesHeightForY()
    {
        HeadhunterBarPlacement placement = new(960, 0, 1f);

        (_, float y) = HeadhunterBarLayout.ToLocal(placement, 1920, 1080, 1f);

        Assert.Equal(-540f, y, 0.001f);
    }

    private static HeadhunterBarPlacement Shift(
        HeadhunterBarPlacement placement,
        bool moveX,
        float shift
    )
    {
        return moveX
            ? placement with
            {
                X = placement.X + shift,
            }
            : placement with
            {
                Y = placement.Y + shift,
            };
    }
}
