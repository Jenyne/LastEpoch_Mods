using Il2Cpp;
using LastEpoch_Hud.Scripts.Core.CustomItems;
using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter;
using MelonLoader;

namespace LastEpoch_Hud.Scripts.Mods.Items.CustomItems.Headhunter;

/// <summary>Removes the Headhunter buffs as soon as Headhunter leaves the player.</summary>
internal static class HeadhunterUnequipHandler
{
    private static readonly HeadhunterClearRule _clear = new();

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

        ClearAll();
    }

    public static void OnEquipmentChanged()
    {
        if (HeadhunterKillSource.IsHeadhunterWorn())
        {
            return;
        }

        ClearAll();
    }

    private static void ClearAll()
    {
        HeadhunterResolvedConfig config = HeadhunterConfigLoader.Resolved;
        StatBuffs buffs = HeadhunterBuffSink.PlayerBuffs();
        if (config == null || buffs == null)
        {
            return;
        }

        HeadhunterBuffSink.Apply(buffs, _clear.RemoveAll(config));
    }
}
