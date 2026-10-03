using HarmonyLib;
using Il2Cpp;

namespace LastEpoch_Hud.Scripts.Mods.Items
{
    public class Items_AutoPickup_AncienBone
    {
        public static bool CanRun()
        {
            if ((Scenes.IsGameScene()) && (!Save_Manager.instance.IsNullOrDestroyed()) &&
                (!Refs_Manager.player_actor.IsNullOrDestroyed()))
            {
                return Save_Manager.instance.data.Items.Pickup.Enable_AutoPickup_Materials;
            }
            else { return false; }
        }

        [HarmonyPatch(typeof(GroundItemManager), "dropAncientBoneForPlayer")]
        public class GroundItemManager_dropAncientBoneForPlayer
        {
            [HarmonyPrefix]
            static bool Prefix(Actor __0, int __1)
            {
                if ((!CanRun()) || (__1 <= 0) || (__0.IsNullOrDestroyed())) { return true; }

                AncientBonesTracker tracker = __0.ancientBonesTracker;
                if (tracker.IsNullOrDestroyed()) { tracker = PlayerFinder.getAncientBonesTracker(); }
                if (tracker.IsNullOrDestroyed()) { return true; }

                return !tracker.modifyAncientBones(__1);
            }
        }
    }
}
