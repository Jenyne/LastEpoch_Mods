using HarmonyLib;
using Il2Cpp;

namespace LastEpoch_Hud.Scripts.Mods.Items.CustomItems.Headhunter;

[HarmonyPatch(typeof(ItemContainersManager), "OnEquipmentOrIdolChange")]
public class HeadhunterEquipmentChangePatch
{
    [HarmonyPostfix]
    private static void Postfix()
    {
        HeadhunterUnequipHandler.OnEquipmentChanged();
    }
}
