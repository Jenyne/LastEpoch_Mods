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

    // Deliberately do not override LocalTreeData.tryToSpendSkillPoint.
    // Raising additionalMaxPointsFromStats to 255 during allocation bypasses
    // native skill-tree limits and can destabilize already assigned nodes.

}
