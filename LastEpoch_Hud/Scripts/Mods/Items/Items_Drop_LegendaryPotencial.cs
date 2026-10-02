using HarmonyLib;
using UnityEngine;
using Il2Cpp;

namespace LastEpoch_Hud.Scripts.Mods.Items
{
    public class Items_Drop_LegendaryPotencial
    {
        public static bool CanRun()
        {
            if ((Hud_Manager.IsPauseOpen()) && (Hud_Manager.Content.OdlForceDrop.enable)) { return false; }
            else if ((Scenes.IsGameScene()) && (!Save_Manager.instance.IsNullOrDestroyed()))
            {
                if (!Save_Manager.instance.data.IsNullOrDestroyed())
                {
                    return Save_Manager.instance.data.Items.Drop.Enable_LegendaryPotencial;
                }
                else { return false; }
            }
            else { return false; }
        }

        [HarmonyPatch(typeof(ItemData), "RollLegendaryPotential", new System.Type[] { typeof(UniqueList.Entry), typeof(int), typeof(int), typeof(float), typeof(float), typeof(bool), typeof(float) }, new ArgumentType[] { ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Ref, ArgumentType.Normal })]
        public class rollLegendaryPotential
        {
            // Preserve the game's side effects and improvedByCoF out parameter.
            [HarmonyPostfix]
            static void Postfix(ref int __result)
            {
                if (!CanRun()) { return; }
                int min = Mathf.Clamp((int)Save_Manager.instance.data.Items.Drop.LegendaryPotencial_Min, 0, 4);
                int max = Mathf.Clamp((int)Save_Manager.instance.data.Items.Drop.LegendaryPotencial_Max, min, 4);
                __result = Random.Range(min, max + 1);
            }
        }
    }
}