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

        if (__1.rarity == 5 || __1.rarity == 6)
        {
            __1.rarity = 4;
            __1.RefreshIDAndValues();
        }

        return true;
    }
}
