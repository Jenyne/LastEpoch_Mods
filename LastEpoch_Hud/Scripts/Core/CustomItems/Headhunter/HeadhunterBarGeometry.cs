namespace LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter;

/// <summary>Where each bar icon sits on screen.</summary>
public static class HeadhunterBarGeometry
{
    public static int IndexAt(
        HeadhunterBarPlacement placement,
        float canvasScale,
        int count,
        float x,
        float y
    )
    {
        if (count <= 0 || placement.Scale <= 0f || canvasScale <= 0f)
        {
            return -1;
        }

        float icon = IconPixels(placement, canvasScale);
        if (y < placement.Y || y >= placement.Y + icon)
        {
            return -1;
        }

        float step = icon + GapPixels(placement, canvasScale);
        float offset = x - Left(placement, canvasScale, count);
        if (offset < 0f)
        {
            return -1;
        }

        int index = (int)(offset / step);
        if (index >= count || offset - (index * step) >= icon)
        {
            return -1;
        }

        return index;
    }

    public static (float X, float Y) TopCenter(
        HeadhunterBarPlacement placement,
        float canvasScale,
        int count,
        int index
    )
    {
        float icon = IconPixels(placement, canvasScale);
        float step = icon + GapPixels(placement, canvasScale);
        float x = Left(placement, canvasScale, count) + (index * step) + (icon / 2f);
        return (x, placement.Y + icon);
    }

    private static float IconPixels(HeadhunterBarPlacement placement, float canvasScale)
    {
        return HeadhunterBarLayout.EntrySize * placement.Scale * canvasScale;
    }

    private static float GapPixels(HeadhunterBarPlacement placement, float canvasScale)
    {
        return HeadhunterBarLayout.Spacing * placement.Scale * canvasScale;
    }

    private static float Left(HeadhunterBarPlacement placement, float canvasScale, int count)
    {
        float width = HeadhunterBarLayout.PanelWidth(count) * placement.Scale * canvasScale;
        return placement.X - (width / 2f);
    }
}
