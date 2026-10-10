using Il2Cpp;

namespace LastEpoch_Hud.Scripts.Mods.Items.AutoLoot;

/// <summary>Shatters a dropped item into affix shards; exceptions reach the caller.</summary>
public static class AutoShatter
{
    public static bool TryShatter(ItemDataUnpacked item)
    {
        Save_Manager.Data.Pickup pickup = Save_Manager.instance.data.Items.Pickup;
        int chance = pickup.AutoShatter_Chance;
        if (chance <= 0)
        {
            return false;
        }
        if ((chance < 100) && (UnityEngine.Random.Range(0, 100) >= chance))
        {
            return false;
        }

        ItemContainersManager manager = ItemContainersManager.Instance;
        if (manager.IsNullOrDestroyed())
        {
            return false;
        }
        if (pickup.Enable_AutoShatter_UseRune && !HasRune(manager))
        {
            return false;
        }
        if (
            !GrantShards(
                manager,
                item,
                pickup.AutoShatter_AffixChance,
                pickup.AutoShatter_QuantityChance
            )
        )
        {
            return false;
        }
        if (pickup.Enable_AutoShatter_UseRune)
        {
            ConsumeRune(manager);
        }
        return true;
    }

    private static bool HasRune(ItemContainersManager manager)
    {
        if (
            manager.materials.IsNullOrDestroyed()
            || manager.materials.shattering.IsNullOrDestroyed()
        )
        {
            return false;
        }
        return manager.materials.shattering.GetQuantity() > 0;
    }

    private static void ConsumeRune(ItemContainersManager manager)
    {
        SingleSubTypeContainer container = manager.materials.shattering;
        if (
            (container.IsNullOrDestroyed())
            || (!container.TryGetContent(out ItemContainerEntry entry))
            || (entry.IsNullOrDestroyed())
        )
        {
            return;
        }
        container.TryRemoveItem(entry, 1, Context.SILENT);
    }

    private static bool GrantShards(
        ItemContainersManager manager,
        ItemDataUnpacked item,
        int affixChance,
        int quantityChance
    )
    {
        if (manager.shardStorage == null)
        {
            return false;
        }
        if (affixChance <= 0)
        {
            return true;
        }
        Il2CppSystem.Collections.Generic.List<ItemAffix> affixes = item.affixes;
        if (affixes.IsNullOrDestroyed())
        {
            return true;
        }
        for (int i = 0; i < affixes.Count; i++)
        {
            ItemAffix affix = affixes[i];
            if ((affix.IsNullOrDestroyed()) || (affix.affixId == 0))
            {
                continue;
            }
            if ((affixChance < 100) && (UnityEngine.Random.Range(0, 100) >= affixChance))
            {
                continue;
            }
            int quantity = 1;
            if (
                (quantityChance > 0)
                && ((quantityChance >= 100) || (UnityEngine.Random.Range(0, 100) < quantityChance))
            )
            {
                quantity = 2;
            }
            manager.shardStorage.AddShard(affix.affixId, quantity, true);
        }
        return true;
    }
}
