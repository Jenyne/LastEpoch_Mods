using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Bar;

namespace LastEpoch_Hud.Tests.Core.CustomItems.Headhunter.Bar;

public sealed class HeadhunterBarGridTests
{
    private static readonly HeadhunterBarGrid _sixteen = new(16, 10);

    private static float Icon => HeadhunterBarLayout.EntrySize;

    private static float Step => HeadhunterBarLayout.EntrySize + HeadhunterBarLayout.Spacing;

    [Fact]
    public void Rows_SixteenByTen_Two()
    {
        Assert.Equal(2, _sixteen.Rows);
    }

    [Fact]
    public void Rows_TenByTen_One()
    {
        Assert.Equal(1, new HeadhunterBarGrid(10, 10).Rows);
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(5, 0)]
    public void Rows_NoIconsOrZeroPerRow_Zero(int count, int perRow)
    {
        var grid = new HeadhunterBarGrid(count, perRow);

        Assert.Equal(0, grid.Rows);
        Assert.Equal(0f, grid.Width);
        Assert.Equal(0f, grid.Height);
        Assert.Equal(-1, grid.IndexAt(0f, 0f));
    }

    [Theory]
    [InlineData(5, 0)]
    [InlineData(0, 0)]
    public void CellXAndCellBottom_ZeroPerRow_ReturnZero(int count, int perRow)
    {
        var grid = new HeadhunterBarGrid(count, perRow);

        Assert.Equal(0f, grid.CellX(0));
        Assert.Equal(0f, grid.CellBottom(0));
    }

    [Fact]
    public void CountInRow_SixteenByTen_TenThenSix()
    {
        Assert.Equal(10, _sixteen.CountInRow(0));
        Assert.Equal(6, _sixteen.CountInRow(1));
        Assert.Equal(0, _sixteen.CountInRow(2));
    }

    [Fact]
    public void CellX_BottomRow_CenteredOnBar()
    {
        Assert.Equal(-4.5f * Step, _sixteen.CellX(0), 0.001f);
        Assert.Equal(4.5f * Step, _sixteen.CellX(9), 0.001f);
    }

    [Fact]
    public void CellX_ShortTopRow_Centered()
    {
        Assert.Equal(-2.5f * Step, _sixteen.CellX(10), 0.001f);
        Assert.Equal(2.5f * Step, _sixteen.CellX(15), 0.001f);
    }

    [Fact]
    public void CellBottom_SecondRow_OneRowStep()
    {
        Assert.Equal(0f, _sixteen.CellBottom(0), 0.001f);
        Assert.Equal(Step, _sixteen.CellBottom(10), 0.001f);
    }

    [Fact]
    public void Width_UsesWidestRow()
    {
        Assert.Equal(HeadhunterBarLayout.PanelWidth(10), _sixteen.Width, 0.001f);
    }

    [Fact]
    public void Height_TwoRows_TwoIconsPlusGap()
    {
        Assert.Equal((2f * Icon) + HeadhunterBarLayout.Spacing, _sixteen.Height, 0.001f);
    }

    [Fact]
    public void IndexAt_EveryCellCenter_ReturnsIndex()
    {
        for (int i = 0; i < 16; i++)
        {
            float y = _sixteen.CellBottom(i) + (Icon / 2f);

            Assert.Equal(i, _sixteen.IndexAt(_sixteen.CellX(i), y));
        }
    }

    [Fact]
    public void IndexAt_GapBetweenRows_MinusOne()
    {
        float y = Icon + (HeadhunterBarLayout.Spacing / 2f);

        Assert.Equal(-1, _sixteen.IndexAt(_sixteen.CellX(0), y));
    }

    [Fact]
    public void IndexAt_TopRowOutsideShortSpan_MinusOne()
    {
        float y = _sixteen.CellBottom(10) + (Icon / 2f);

        Assert.Equal(-1, _sixteen.IndexAt(_sixteen.CellX(0), y));
    }

    [Fact]
    public void IndexAt_TopRowRightOfShortSpan_MinusOne()
    {
        float y = _sixteen.CellBottom(10) + (Icon / 2f);

        Assert.Equal(-1, _sixteen.IndexAt(_sixteen.CellX(9), y));
        Assert.Equal(-1, _sixteen.IndexAt(_sixteen.CellX(15) + Step, y));
    }

    [Fact]
    public void IndexAt_AboveTopRow_MinusOne()
    {
        float x = _sixteen.CellX(12);
        float controlY = _sixteen.CellBottom(12) + (Icon / 2f);
        float thirdRowY = _sixteen.CellBottom(12) + Step + (Icon / 2f);

        Assert.Equal(12, _sixteen.IndexAt(x, controlY));
        Assert.Equal(-1, _sixteen.IndexAt(x, thirdRowY));
    }

    [Fact]
    public void IndexAt_BelowBottom_MinusOne()
    {
        Assert.Equal(-1, _sixteen.IndexAt(_sixteen.CellX(0), -1f));
    }

    [Fact]
    public void IndexAt_Edges_LeftBottomInclusiveRightTopExclusive()
    {
        float left = _sixteen.CellX(0) - (Icon / 2f);
        float middle = Icon / 2f;

        Assert.Equal(0, _sixteen.IndexAt(left, middle));
        Assert.Equal(0, _sixteen.IndexAt(_sixteen.CellX(0), 0f));
        Assert.Equal(-1, _sixteen.IndexAt(left + Icon, middle));
        Assert.Equal(-1, _sixteen.IndexAt(_sixteen.CellX(0), Icon));
    }
}
