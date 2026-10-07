using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Bar;

namespace LastEpoch_Hud.Tests.Core.CustomItems.Headhunter.Bar;

public sealed class HeadhunterBarGeometryTests
{
    private const float CanvasScale = 2f;
    private static readonly HeadhunterBarPlacement _placement = new(500, 100, 0.5f);

    private static float Icon => HeadhunterBarLayout.EntrySize;

    private static float Gap => HeadhunterBarLayout.Spacing;

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void IndexAt_IconCenters_ReturnIndex(int index)
    {
        float x = CellLeft(3, index) + (Icon / 2f);
        float y = _placement.Y + (Icon / 2f);

        Assert.Equal(index, HeadhunterBarGeometry.IndexAt(_placement, CanvasScale, 3, x, y));
    }

    [Fact]
    public void IndexAt_Gap_MinusOne()
    {
        float x = CellLeft(3, 0) + Icon + (Gap / 2f);
        float y = _placement.Y + (Icon / 2f);

        Assert.Equal(-1, HeadhunterBarGeometry.IndexAt(_placement, CanvasScale, 3, x, y));
    }

    [Theory]
    [InlineData(-1f)]
    [InlineData(1f)]
    public void IndexAt_AboveOrBelow_MinusOne(float offset)
    {
        float x = CellLeft(3, 1) + (Icon / 2f);
        float edge = offset > 0 ? _placement.Y + Icon : _placement.Y;

        Assert.Equal(
            -1,
            HeadhunterBarGeometry.IndexAt(_placement, CanvasScale, 3, x, edge + offset)
        );
    }

    [Theory]
    [InlineData(-1f)]
    [InlineData(1f)]
    public void IndexAt_OutsideRow_MinusOne(float side)
    {
        float edge = side < 0 ? Left(3) : Left(3) + HeadhunterBarLayout.PanelWidth(3);
        float y = _placement.Y + (Icon / 2f);

        Assert.Equal(
            -1,
            HeadhunterBarGeometry.IndexAt(_placement, CanvasScale, 3, edge + (side * 0.5f), y)
        );
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void IndexAt_NoIcons_MinusOne(int count)
    {
        float y = _placement.Y + (Icon / 2f);

        Assert.Equal(-1, HeadhunterBarGeometry.IndexAt(_placement, CanvasScale, count, 500, y));
    }

    [Theory]
    [InlineData(0f, 2f)]
    [InlineData(-0.5f, 2f)]
    [InlineData(0.5f, 0f)]
    [InlineData(0.5f, -2f)]
    public void IndexAt_NonPositiveScale_MinusOne(float scale, float canvasScale)
    {
        HeadhunterBarPlacement placement = _placement with { Scale = scale };
        float y = placement.Y + (Icon / 2f);

        Assert.Equal(-1, HeadhunterBarGeometry.IndexAt(placement, canvasScale, 1, 500, y));
    }

    [Fact]
    public void IndexAt_ScaleChangesIconSize()
    {
        HeadhunterBarPlacement small = _placement with { Scale = 0.25f };
        float left = small.X - (Icon * 0.25f * CanvasScale / 2f);
        float x = left + (0.75f * Icon);
        float y = small.Y + 1f;

        Assert.Equal(-1, HeadhunterBarGeometry.IndexAt(small, CanvasScale, 1, x, y));
    }

    [Fact]
    public void TopCenter_SingleIcon_IsBarTopCenter()
    {
        (float x, float y) = HeadhunterBarGeometry.TopCenter(_placement, CanvasScale, 1, 0);

        Assert.Equal(_placement.X, x, 0.001f);
        Assert.Equal(_placement.Y + Icon, y, 0.001f);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void TopCenter_MatchesIndexAt(int index)
    {
        (float x, float y) = HeadhunterBarGeometry.TopCenter(_placement, CanvasScale, 3, index);

        Assert.Equal(index, HeadhunterBarGeometry.IndexAt(_placement, CanvasScale, 3, x, y - 1f));
    }

    [Fact]
    public void IndexAt_Edges_LeftTopInclusive()
    {
        Assert.Equal(
            0,
            HeadhunterBarGeometry.IndexAt(_placement, CanvasScale, 3, CellLeft(3, 0), _placement.Y)
        );
    }

    [Fact]
    public void IndexAt_Edges_RightOfCellExclusive()
    {
        float y = _placement.Y + (Icon / 2f);

        Assert.Equal(
            -1,
            HeadhunterBarGeometry.IndexAt(_placement, CanvasScale, 3, CellLeft(3, 0) + Icon, y)
        );
    }

    [Fact]
    public void IndexAt_Edges_BottomExclusive()
    {
        float x = CellLeft(3, 0) + (Icon / 2f);

        Assert.Equal(
            -1,
            HeadhunterBarGeometry.IndexAt(_placement, CanvasScale, 3, x, _placement.Y + Icon)
        );
    }

    [Fact]
    public void IndexAt_WhereFourthIconWouldBe_MinusOne()
    {
        float x = CellLeft(3, 3) + (Icon / 2f);
        float y = _placement.Y + (Icon / 2f);

        Assert.Equal(-1, HeadhunterBarGeometry.IndexAt(_placement, CanvasScale, 3, x, y));
    }

    [Fact]
    public void IndexAt_ScaledRow_HitsIndex()
    {
        HeadhunterBarPlacement placement = new(500, 100, 1f);
        float x = ScaledLeft(2) + ((Icon + Gap) * 1.5f) + (Icon * 1.5f / 2f);
        float y = placement.Y + (Icon * 1.5f / 2f);

        Assert.Equal(1, HeadhunterBarGeometry.IndexAt(placement, 1.5f, 2, x, y));
    }

    [Fact]
    public void TopCenter_ScaledRow_UsesScaledIconSize()
    {
        HeadhunterBarPlacement placement = new(500, 100, 1f);

        (float x, float y) = HeadhunterBarGeometry.TopCenter(placement, 1.5f, 2, 1);

        Assert.Equal(ScaledLeft(2) + ((Icon + Gap) * 1.5f) + (Icon * 1.5f / 2f), x, 0.001f);
        Assert.Equal(100 + (Icon * 1.5f), y, 0.001f);
    }

    private static float ScaledLeft(int count)
    {
        return 500 - (HeadhunterBarLayout.PanelWidth(count) * 1.5f / 2f);
    }

    private static float Left(int count)
    {
        return _placement.X - (HeadhunterBarLayout.PanelWidth(count) / 2f);
    }

    private static float CellLeft(int count, int index)
    {
        return Left(count) + (index * (Icon + Gap));
    }
}
