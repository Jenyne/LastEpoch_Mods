using HarmonyLib;
using Il2Cpp;

namespace LastEpoch_Hud.Scripts.Mods.Skills;

public class Skills_SpendPoints
{
    public static bool CanRun()
    {
        if ((Scenes.IsGameScene()) && (!Save_Manager.instance.IsNullOrDestroyed()))
        {
            if (!Save_Manager.instance.data.IsNullOrDestroyed())
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            return false;
        }
    }

    [HarmonyPatch(typeof(SkillTreeNode), "Clicked", new System.Type[] { })]
    public class SkillTreeNode_Clicked_ShiftMax
    {
        private static bool bulkClick;

        [HarmonyPrefix]
        static bool Prefix(SkillTreeNode __instance)
        {
            if (bulkClick || __instance.IsNullOrDestroyed())
            {
                return true;
            }

            bool shift =
                UnityEngine.Input.GetKey(UnityEngine.KeyCode.LeftShift)
                || UnityEngine.Input.GetKey(UnityEngine.KeyCode.RightShift);
            if (!shift)
            {
                return true;
            }

            bulkClick = true;
            try
            {
                int safety = 0;
                while (safety++ < 255)
                {
                    int before = __instance.pointsAllocated;
                    if (before >= __instance.maxPoints)
                    {
                        break;
                    }

                    // Re-enter the game's normal click path. bulkClick makes the
                    // recursive call bypass this wrapper, so all native requirement,
                    // available-point and save/update logic still executes.
                    __instance.Clicked();

                    int after = __instance.pointsAllocated;
                    if (after <= before)
                    {
                        break;
                    }
                }
            }
            catch (System.Exception ex)
            {
                Main.logger_instance?.Warning("Shift-click max skill node failed: " + ex.Message);
            }
            finally
            {
                bulkClick = false;
            }

            // The Shift-click has already executed the native click as many times
            // as possible, so suppress the original outer click.
            return false;
        }
    }

    [HarmonyPatch(typeof(LocalTreeData), "tryToSpendSkillPoint")]
    public class TryToSpendSkillPoint
    {
        private static bool Added_MaxPoint = false;
        private static byte Backup_MaxPoint = 0;

        [HarmonyPrefix]
        static void Prefix(ref LocalTreeData __instance, bool __result, Ability __0, byte __1)
        {
            if (CanRun())
            {
                Added_MaxPoint = false;
                if ((!__result) && (Save_Manager.instance.data.Skills.Disable_NodeRequirement))
                {
                    foreach (LocalTreeData.SkillTreeData tree in __instance.specialisedSkillTrees)
                    {
                        if (tree.ability == __0)
                        {
                            Added_MaxPoint = true;
                            Backup_MaxPoint = tree.additionalMaxPointsFromStats;
                            tree.additionalMaxPointsFromStats = 255;
                            break;
                        }
                    }
                }
            }
        }

        [HarmonyPostfix]
        static void Postfix(ref LocalTreeData __instance, bool __result, Ability __0, byte __1)
        {
            if ((CanRun()) && (Added_MaxPoint))
            {
                foreach (LocalTreeData.SkillTreeData tree in __instance.specialisedSkillTrees)
                {
                    if (tree.ability == __0)
                    {
                        tree.additionalMaxPointsFromStats = Backup_MaxPoint;
                        Added_MaxPoint = false;
                        break;
                    }
                }
            }
        }
    }
}
