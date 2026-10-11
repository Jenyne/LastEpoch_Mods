using HarmonyLib;
using Il2Cpp;
using UnityEngine.UI;

namespace LastEpoch_Hud.Scripts;

public partial class Hud_Manager
{
    public class Hooks
    {
        //All Hooks have to be replace by Unity Actions
        [HarmonyPatch(typeof(Toggle), "OnPointerClick")]
        public class Toggle_OnPointerClick
        {
            [HarmonyPostfix]
            static void Postfix(
                ref Toggle __instance,
                UnityEngine.EventSystems.PointerEventData __0
            )
            {
                if (
                    hud_object.IsNullOrDestroyed()
                    || Save_Manager.instance.IsNullOrDestroyed()
                    || Refs_Manager.player_data.IsNullOrDestroyed()
                    || !hud_object.active
                )
                {
                    return;
                }
                switch (__instance.name)
                {
                    case "Toggle_UnlockAllSkills":
                    {
                        Save_Manager.instance.data.Skills.Enable_AllSkills = __instance.isOn;
                        break;
                    }
                    case "Toggle_Weaver_PointMultiplier":
                    {
                        Save_Manager.instance.data.Factions.TheWoven.Enable_PointMultiplier =
                            __instance.isOn;
                        Mods.Factions.TheWoven.Faction_Woven_TreePoints.ApplyToPlayer();
                        break;
                    }
                    case "Toggle_NoTarget":
                    {
                        Save_Manager.instance.data.Skills.MovementSkills.Enable_NoTarget =
                            __instance.isOn;
                        break;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(Slider), "Set", new System.Type[] { typeof(float), typeof(bool) })]
        public class Slider_Set
        {
            [HarmonyPostfix]
            static void Postfix(Slider __instance, bool sendCallback)
            {
                if (!sendCallback || Content.Character.Data.suppressWeaverSlider)
                {
                    return;
                }
                if (__instance.IsNullOrDestroyed() || __instance.name != "Slider_Weaver_TreePoints")
                {
                    return;
                }
                if (
                    hud_object.IsNullOrDestroyed()
                    || !hud_object.active
                    || Save_Manager.instance.IsNullOrDestroyed()
                )
                {
                    return;
                }
                Content.Character.Data.SetWeaverTreePoints(__instance.value);
            }
        }

        [HarmonyPatch(typeof(Slider), "set_value")]
        public class Slider_set_value
        {
            [HarmonyPostfix]
            static void Postfix(ref Slider __instance, float __0)
            {
                if (Content.Character.Data.suppressWeaverSlider)
                {
                    return;
                }
                if (
                    hud_object.IsNullOrDestroyed()
                    || Save_Manager.instance.IsNullOrDestroyed()
                    || !hud_object.active
                    || Refs_Manager.player_data.IsNullOrDestroyed()
                )
                {
                    return;
                }
                if (__instance.name == "Slider_Weaver_TreePoints")
                {
                    Content.Character.Data.SetWeaverTreePoints(__instance.value);
                    return;
                }
                if (__instance.name == "Slider_Weaver_PointMultiplier")
                {
                    int multiplier = SettingRow.Clamp(__0);
                    Save_Manager.instance.data.Factions.TheWoven.PointMultiplier = multiplier;
                    Mods.Factions.TheWoven.Faction_Woven_TreePoints.ApplyToPlayer();
                }
            }
        }
    }
}
