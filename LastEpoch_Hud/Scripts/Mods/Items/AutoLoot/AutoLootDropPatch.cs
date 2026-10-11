using System;
using HarmonyLib;
using Il2Cpp;
using Il2CppItemFiltering;
using LastEpoch_Hud.Scripts.Core.Items.AutoLoot;
using UnityEngine;

namespace LastEpoch_Hud.Scripts.Mods.Items.AutoLoot;

/// <summary>Auto-pickup, auto-sell and auto-shatter of dropped items, driven by the game's loot filter; materials go to storage via the game's click pickup.</summary>
[HarmonyPatch(typeof(GroundItemManager), "dropItemForPlayer")]
public class AutoLootDropPatch
{
    [HarmonyPrefix]
    private static bool Prefix(
        Actor __0,
        ItemData __1,
        ref Vector3 __2,
        ref bool __3,
        out bool __state
    )
    {
        __state = false;
        AutoLootSettings settings = AutoLootRules.ForOrigin(
            ReadSettings(),
            ModItemDrop.CurrentOrigin
        );
        if (!settings.AnyEnabled)
        {
            return true;
        }

        try
        {
            LootRoute route = ReadRoute(__0, __1, settings);
            if (route != LootRoute.Store)
            {
                return Run(__0, __1, route, settings);
            }

            __2 = __0.position();
            __3 = false;
            __state = true;
            return true;
        }
        catch (Exception ex)
        {
            ErrorLog.Report(ex, "AutoLoot drop");
            return true;
        }
    }

    [HarmonyPostfix]
    private static void Postfix(GroundItemManager __instance, Actor __0, ItemData __1, bool __state)
    {
        if (!__state)
        {
            return;
        }

        try
        {
            PickUpNewGroundItem(__instance, __0, __1);
        }
        catch (Exception ex)
        {
            ErrorLog.Report(ex, "AutoLoot store");
        }
    }

    private static AutoLootSettings ReadSettings()
    {
        if (Save_Manager.instance.IsNullOrDestroyed())
        {
            return default;
        }

        Save_Manager.Data.Pickup pickup = Save_Manager.instance.data.Items.Pickup;
        return new AutoLootSettings(
            pickup.Enable_AutoPickup_Keys,
            pickup.Enable_AutoPickup_WovenEchoes,
            pickup.Enable_AutoPickup_Materials,
            pickup.Enable_AutoPickup_FromFilter,
            pickup.Enable_AutoSell_FromFilter,
            pickup.Enable_AutoShatter_FromFilter
        );
    }

    private static LootRoute ReadRoute(Actor actor, ItemData item, AutoLootSettings settings)
    {
        if (actor.IsNullOrDestroyed() || item.IsNullOrDestroyed())
        {
            return LootRoute.Ignore;
        }

        if (!Scenes.IsGameScene() || item.isTutorialItem())
        {
            return LootRoute.Ignore;
        }

        return AutoLootRules.Route(
            LootKindReader.Read(item),
            item.MatchesStackableItemFlags(StackableItemFlags.All),
            settings
        );
    }

    private static bool Run(Actor actor, ItemData item, LootRoute route, AutoLootSettings settings)
    {
        if (route == LootRoute.Ignore)
        {
            return true;
        }

        if (route == LootRoute.PickUp)
        {
            return !TryPickUp(actor, item);
        }

        return RunFilter(actor, item, settings);
    }

    private static void PickUpNewGroundItem(GroundItemManager ground, Actor actor, ItemData item)
    {
        if (TryFindNewGroundItemId(ground, actor, item, out uint id))
        {
            ground.pickupItem(actor, id, StackableItemFlags.All);
        }
    }

    private static bool TryFindNewGroundItemId(
        GroundItemManager ground,
        Actor actor,
        ItemData item,
        out uint id
    )
    {
        id = 0;
        if (ground.IsNullOrDestroyed() || actor.IsNullOrDestroyed() || item.IsNullOrDestroyed())
        {
            return false;
        }

        if (!ground.TryGetGroundItemList(actor, out GroundItemManager.GroundItemList list))
        {
            return false;
        }

        if (list.IsNullOrDestroyed() || list.items.IsNullOrDestroyed() || list.items.Count == 0)
        {
            return false;
        }

        GroundItemManager.GroundItem last = list.items[list.items.Count - 1];
        if (last.IsNullOrDestroyed() || last.itemData.IsNullOrDestroyed())
        {
            return false;
        }

        if (last.itemData.Pointer != item.Pointer)
        {
            return false;
        }

        id = last.id;
        return true;
    }

    private static bool RunFilter(Actor actor, ItemData item, AutoLootSettings settings)
    {
        ItemFilter filter = LootFilterReader.ActiveFilter();
        ItemDataUnpacked unpacked = item.getAsUnpacked();
        LootVisibility visibility = LootFilterReader.Read(filter, unpacked);
        FilterLootPlan plan = AutoLootRules.ForFilter(visibility, settings);
        if (plan.IsEmpty)
        {
            return true;
        }

        if (plan.PickUp)
        {
            return !TryPickUp(actor, item);
        }

        if (plan.Shatter && AutoShatter.TryShatter(unpacked))
        {
            return false;
        }

        if (plan.Sell)
        {
            return !TrySell(actor, unpacked);
        }

        return true;
    }

    private static bool TryPickUp(Actor actor, ItemData item)
    {
        ItemContainersManager manager = ItemContainersManager.Instance;
        if (manager.IsNullOrDestroyed())
        {
            return false;
        }

        return manager.attemptToPickupItem(item, actor.position());
    }

    private static bool TrySell(Actor actor, ItemDataUnpacked item)
    {
        if (actor.goldTracker.IsNullOrDestroyed())
        {
            return false;
        }

        actor.goldTracker.modifyGold(item.VendorSaleValue);
        return true;
    }
}
