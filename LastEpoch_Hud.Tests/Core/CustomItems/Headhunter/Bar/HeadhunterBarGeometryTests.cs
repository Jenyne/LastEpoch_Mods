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

        Assert.Equal(index, HeadhunterBarGeometry.IndexAt(_placement, CanvasScale, Grid(3), x, y));
    }

    [Fact]
    public void IndexAt_Gap_MinusOne()
    {
        float x = CellLeft(3, 0) + Icon + (Gap / 2f);
        float y = _placement.Y + (Icon / 2f);

        Assert.Equal(-1, HeadhunterBarGeometry.IndexAt(_placement, CanvasScale, Grid(3), x, y));
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
            HeadhunterBarGeometry.IndexAt(_placement, CanvasScale, Grid(3), x, edge + offset)
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
            HeadhunterBarGeometry.IndexAt(_placement, CanvasScale, Grid(3), edge + (side * 0.5f), y)
        );
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void IndexAt_NoIcons_MinusOne(int count)
    {
        float y = _placement.Y + (Icon / 2f);

        Assert.Equal(
            -1,
            HeadhunterBarGeometry.IndexAt(_placement, CanvasScale, Grid(count), 500, y)
        );
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

        Assert.Equal(-1, HeadhunterBarGeometry.IndexAt(placement, canvasScale, Grid(1), 500, y));
    }

    [Fact]
    public void IndexAt_ScaleChangesIconSize()
    {
        HeadhunterBarPlacement small = _placement with { Scale = 0.25f };
        float left = small.X - (Icon * 0.25f * CanvasScale / 2f);
        float x = left + (0.75f * Icon);
        float y = small.Y + 1f;

        Assert.Equal(-1, HeadhunterBarGeometry.IndexAt(small, CanvasScale, Grid(1), x, y));
    }

    [Fact]
    public void TooltipAnchor_SingleIcon_IsBarTopCenter()
    {
        (float x, float y) = HeadhunterBarGeometry.TooltipAnchor(
            _placement,
            CanvasScale,
            Grid(1),
            0
        );

        Assert.Equal(_placement.X, x, 0.001f);
        Assert.Equal(_placement.Y + Icon, y, 0.001f);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void TooltipAnchor_MatchesIndexAt(int index)
    {
        (float x, float y) = HeadhunterBarGeometry.TooltipAnchor(
            _placement,
            CanvasScale,
            Grid(3),
            index
        );

        Assert.Equal(
            index,
            HeadhunterBarGeometry.IndexAt(_placement, CanvasScale, Grid(3), x, y - 1f)
        );
    }

    [Fact]
    public void IndexAt_Edges_LeftTopInclusive()
    {
        Assert.Equal(
            0,
            HeadhunterBarGeometry.IndexAt(
                _placement,
                CanvasScale,
                Grid(3),
                CellLeft(3, 0),
                _placement.Y
            )
        );
    }

    [Fact]
    public void IndexAt_Edges_RightOfCellExclusive()
    {
        float y = _placement.Y + (Icon / 2f);

        Assert.Equal(
            -1,
            HeadhunterBarGeometry.IndexAt(
                _placement,
                CanvasScale,
                Grid(3),
                CellLeft(3, 0) + Icon,
                y
            )
        );
    }

    [Fact]
    public void IndexAt_Edges_BottomExclusive()
    {
        float x = CellLeft(3, 0) + (Icon / 2f);

        Assert.Equal(
            -1,
            HeadhunterBarGeometry.IndexAt(_placement, CanvasScale, Grid(3), x, _placement.Y + Icon)
        );
    }

    [Fact]
    public void IndexAt_WhereFourthIconWouldBe_MinusOne()
    {
        float x = CellLeft(3, 3) + (Icon / 2f);
        float y = _placement.Y + (Icon / 2f);

        Assert.Equal(-1, HeadhunterBarGeometry.IndexAt(_placement, CanvasScale, Grid(3), x, y));
    }

    [Fact]
    public void IndexAt_ScaledRow_HitsIndex()
    {
        HeadhunterBarPlacement placement = new(500, 100, 1f);
        float x = ScaledLeft(2) + ((Icon + Gap) * 1.5f) + (Icon * 1.5f / 2f);
        float y = placement.Y + (Icon * 1.5f / 2f);

        Assert.Equal(1, HeadhunterBarGeometry.IndexAt(placement, 1.5f, Grid(2), x, y));
    }

    [Fact]
    public void TooltipAnchor_ScaledRow_UsesScaledIconSize()
    {
        HeadhunterBarPlacement placement = new(500, 100, 1f);

        (float x, float y) = HeadhunterBarGeometry.TooltipAnchor(placement, 1.5f, Grid(2), 1);

        Assert.Equal(ScaledLeft(2) + ((Icon + Gap) * 1.5f) + (Icon * 1.5f / 2f), x, 0.001f);
        Assert.Equal(100 + (Icon * 1.5f), y, 0.001f);
    }

    [Fact]
    public void IndexAt_TwoRowsScaled_HitsTopRowIcon()
    {
        HeadhunterBarPlacement placement = new(500, 100, 1f);
        HeadhunterBarGrid grid = Grid(16);
        const float scale = 1.5f;
        float x = placement.X + (grid.CellX(12) * scale);
        float y = placement.Y + ((grid.CellBottom(12) + (Icon / 2f)) * scale);

        Assert.Equal(12, HeadhunterBarGeometry.IndexAt(placement, scale, grid, x, y));
    }

    [Fact]
    public void IndexAt_TwoRows_GapBetweenRows_MinusOne()
    {
        HeadhunterBarGrid grid = Grid(16);
        float scale = _placement.Scale * CanvasScale;
        float x = _placement.X + (grid.CellX(0) * scale);
        float iconY = _placement.Y + (Icon / 2f * scale);
        float gapY = _placement.Y + ((Icon + (Gap / 2f)) * scale);

        Assert.Equal(0, HeadhunterBarGeometry.IndexAt(_placement, CanvasScale, grid, x, iconY));
        Assert.Equal(-1, HeadhunterBarGeometry.IndexAt(_placement, CanvasScale, grid, x, gapY));
    }

    [Fact]
    public void TooltipAnchor_TwoRows_IsIconXAndBarTop()
    {
        HeadhunterBarGrid grid = Grid(16);
        float scale = _placement.Scale * CanvasScale;

        (float x, float y) = HeadhunterBarGeometry.TooltipAnchor(_placement, CanvasScale, grid, 3);

        Assert.Equal(_placement.X + (grid.CellX(3) * scale), x, 0.001f);
        Assert.Equal(_placement.Y + (grid.Height * scale), y, 0.001f);
    }

    private static HeadhunterBarGrid Grid(int count)
    {
        return new HeadhunterBarGrid(count, 10);
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
