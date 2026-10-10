using System;
using HarmonyLib;
using Il2Cpp;

namespace LastEpoch_Hud.Scripts.Mods.Items;

/// <summary>Clamps loaded rarity 5/6 drops to 4 so items with more than four affixes load.</summary>
[HarmonyPatch(typeof(GroundItemManager), "dropItemForPlayer")]
public class DropRarityClampPatch
{
    [HarmonyPrefix]
    [HarmonyPriority(Priority.First)]
    private static bool Prefix(ItemData __1)
    {
        if (!Scenes.IsGameScene())
        {
            return true;
        }

        try
        {
            Clamp(__1);
        }
        catch (Exception ex)
        {
            ErrorLog.Report(ex, "Drop rarity clamp");
        }

        return true;
    }

    private static void Clamp(ItemData item)
    {
        if (item.IsNullOrDestroyed())
        {
            return;
        }

        if (item.rarity != 5 && item.rarity != 6)
        {
            return;
        }

        item.rarity = 4;
        item.RefreshIDAndValues();
    }
}
