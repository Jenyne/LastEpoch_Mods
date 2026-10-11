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
                    case "Toggle_NoTarget":
                    {
                        Save_Manager.instance.data.Skills.MovementSkills.Enable_NoTarget =
                            __instance.isOn;
                        break;
                    }
                }
            }
        }
    }
}
