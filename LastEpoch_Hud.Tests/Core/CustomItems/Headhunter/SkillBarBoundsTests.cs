using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter;

namespace LastEpoch_Hud.Tests.Core.CustomItems.Headhunter;

public sealed class SkillBarBoundsTests
{
    [Fact]
    public void Empty_IsEmpty()
    {
        Assert.True(SkillBarBounds.Empty.IsEmpty);
    }

    [Fact]
    public void Include_GrowsMinMax()
    {
        SkillBarBounds bounds = SkillBarBounds.Empty.Include(10, 20).Include(30, 5);

        Assert.False(bounds.IsEmpty);
        Assert.Equal(10f, bounds.MinX);
        Assert.Equal(5f, bounds.MinY);
        Assert.Equal(30f, bounds.MaxX);
        Assert.Equal(20f, bounds.MaxY);
    }

    [Fact]
    public void ScreenFallback_CenteredInsideScreen()
    {
        var bounds = SkillBarBounds.ScreenFallback(1920, 1080);

        Assert.False(bounds.IsEmpty);
        Assert.Equal(960f, bounds.CenterX, 2);
        Assert.InRange(bounds.MinX, 0f, 1920f);
        Assert.InRange(bounds.MaxX, 0f, 1920f);
        Assert.InRange(bounds.MinY, 0f, 1080f);
        Assert.True(bounds.MinY < bounds.MaxY);
        Assert.True(bounds.MaxY <= 1080f);
    }

    [Theory]
    [InlineData(1.07f)]
    [InlineData(0.92f)]
    public void IncludeRect_RemovesIconScale(float scale)
    {
        float half = 50f * scale;

        SkillBarBounds bounds = SkillBarBounds.Empty.IncludeRect(
            500 - half,
            100 - half,
            500 + half,
            100 + half,
            scale
        );

        AssertBounds(bounds, 450, 50, 550, 150);
    }

    [Theory]
    [InlineData(0f)]
    [InlineData(-1f)]
    public void IncludeRect_NonPositiveScale_TreatedAsOne(float scale)
    {
        SkillBarBounds bounds = SkillBarBounds.Empty.IncludeRect(450, 50, 550, 150, scale);

        AssertBounds(bounds, 450, 50, 550, 150);
    }

    private static void AssertBounds(
        SkillBarBounds bounds,
        float minX,
        float minY,
        float maxX,
        float maxY
    )
    {
        Assert.Equal(minX, bounds.MinX, 0.001f);
        Assert.Equal(minY, bounds.MinY, 0.001f);
        Assert.Equal(maxX, bounds.MaxX, 0.001f);
        Assert.Equal(maxY, bounds.MaxY, 0.001f);
    }
}
