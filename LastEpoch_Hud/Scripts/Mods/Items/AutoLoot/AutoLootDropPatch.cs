using System;
using HarmonyLib;
using Il2Cpp;
using Il2CppItemFiltering;
using LastEpoch_Hud.Scripts.Core.Items.AutoLoot;

namespace LastEpoch_Hud.Scripts.Mods.Items.AutoLoot;

/// <summary>Auto-pickup, auto-sell and auto-shatter of dropped items, driven by the game's loot filter.</summary>
[HarmonyPatch(typeof(GroundItemManager), "dropItemForPlayer")]
public class AutoLootDropPatch
{
    [HarmonyPrefix]
    private static bool Prefix(Actor __0, ItemData __1)
    {
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
            return Run(__0, __1, settings);
        }
        catch (Exception ex)
        {
            ErrorLog.Report(ex, "AutoLoot drop");
            return true;
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

    private static bool Run(Actor actor, ItemData item, AutoLootSettings settings)
    {
        if (actor.IsNullOrDestroyed() || item.IsNullOrDestroyed())
        {
            return true;
        }

        if (!Scenes.IsGameScene() || item.isTutorialItem())
        {
            return true;
        }

        LootRoute route = AutoLootRules.Route(LootKindReader.Read(item), settings);
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
