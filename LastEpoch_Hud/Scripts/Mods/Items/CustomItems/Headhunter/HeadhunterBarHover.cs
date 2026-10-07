using System;
using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter;
using UnityEngine;

namespace LastEpoch_Hud.Scripts.Mods.Items.CustomItems.Headhunter;

/// <summary>Per-frame hover check that drives the bar tooltip.</summary>
internal static class HeadhunterBarHover
{
    private static readonly HeadhunterHoverTracker _hover = new();
    private static int _index = -1;

    public static void Tick()
    {
        try
        {
            int index = HoveredIndex();
            int statId = HeadhunterBuffBarView.StatAt(index);
            if (!_hover.Changed(statId, HeadhunterBuffBarView.LayoutVersion))
            {
                return;
            }

            _index = index;
            Redraw(statId);
        }
        catch (Exception ex)
        {
            ErrorLog.Report(ex, "Headhunter bar tooltip");
        }
    }

    private static int HoveredIndex()
    {
        if (!HeadhunterBuffBarView.IsVisible)
        {
            return -1;
        }

        Vector3 mouse = Input.mousePosition;
        return HeadhunterBuffBarView.IndexAt(mouse.x, mouse.y);
    }

    private static void Redraw(int statId)
    {
        if (statId < 0)
        {
            HeadhunterBuffBarView.HideTooltip();
            return;
        }

        HeadhunterResolvedConfig config = HeadhunterConfigLoader.Resolved;
        if (config == null || !config.TryGetStat(statId, out HeadhunterBuffStat stat))
        {
            HeadhunterBuffBarView.HideTooltip();
            return;
        }

        HeadhunterStatNames.TryRead(statId, out string gameName, out bool addedAsPercent);
        string label = HeadhunterBuffLabel.Format(
            gameName,
            HeadhunterStatNames.EnumName(statId),
            stat.Added,
            stat.Increased,
            addedAsPercent
        );
        HeadhunterBuffBarView.ShowTooltip(_index, label);
    }
}
