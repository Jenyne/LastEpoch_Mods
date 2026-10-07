using Il2Cpp;
using LastEpoch_Hud.Scripts.Core.CustomItems;
using MelonLoader;

namespace LastEpoch_Hud.Scripts.Mods.Items.CustomItems.Headhunter;

/// <summary>Removes the Headhunter buffs as soon as Headhunter leaves the player.</summary>
internal static class HeadhunterUnequipHandler
{
    public static void OnItemRemoved(ItemContainerEntryHandler removed)
    {
        if (removed.IsNullOrDestroyed() || removed.entry.IsNullOrDestroyed())
        {
            return;
        }

        ItemData data = removed.entry.data;
        if (data.IsNullOrDestroyed() || data.uniqueID != CustomUniqueSpecs.Headhunter.UniqueId)
        {
            return;
        }

        HeadhunterBuffClearer.ClearAll(HeadhunterConfigLoader.Resolved);
    }

    public static void OnEquipmentChanged()
    {
        if (HeadhunterKillSource.IsHeadhunterWorn())
        {
            return;
        }

        HeadhunterBuffClearer.ClearAll(HeadhunterConfigLoader.Resolved);
    }
}
