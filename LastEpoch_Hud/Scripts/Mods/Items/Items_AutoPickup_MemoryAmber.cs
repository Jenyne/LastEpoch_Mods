using HarmonyLib;
using Il2CppLE.Factions;
using UnityEngine;

namespace LastEpoch_Hud.Scripts.Mods.Items;

public class Items_AutoPickup_MemoryAmber
{
    public static bool CanRun()
    {
        return Scenes.IsGameScene()
            && !Save_Manager.instance.IsNullOrDestroyed()
            && !Save_Manager.instance.data.IsNullOrDestroyed()
            && !Refs_Manager.player_actor.IsNullOrDestroyed()
            && Save_Manager.instance.data.Items.Pickup.Enable_AutoPickup_MemoryAmber;
    }

    [HarmonyPatch(
        typeof(PickupableObjectsManager),
        "CreatePickupableObjectForPlayer",
        new System.Type[]
        {
            typeof(PickupableObjectType),
            typeof(PickupableObjectSet),
            typeof(Vector3),
            typeof(uint),
        }
    )]
    public class PickupableObjectsManager_CreatePickupableObjectForPlayer
    {
        [HarmonyPrefix]
        static void Prefix(PickupableObjectSet __1, out uint __state)
        {
            __state = __1.IsNullOrDestroyed() ? 0 : __1.NextID;
        }

        [HarmonyPostfix]
        static void Postfix(
            PickupableObjectsManager __instance,
            PickupableObjectType __0,
            PickupableObjectSet __1,
            uint __state
        )
        {
            if (
                !CanRun()
                || __0 != PickupableObjectType.MemoryAmber
                || __1.IsNullOrDestroyed()
                || __1.PlayerActor != Refs_Manager.player_actor
                || __1.NextID == __state
            )
            {
                return;
            }
            if (__1.pickupables.IsNullOrDestroyed() || !__1.pickupables.ContainsKey(__0))
            {
                return;
            }
            var objects = __1.pickupables[__0];
            // Indexing avoids invalidating an enumerator when PickupObject removes the item.
            for (int i = 0; i < objects.Count; i++)
            {
                var pickup = objects[i];
                if (pickup.Id != __state)
                {
                    continue;
                }
                __instance.PickupObject(__1, pickup, i);
                break;
            }
        }
    }
}
