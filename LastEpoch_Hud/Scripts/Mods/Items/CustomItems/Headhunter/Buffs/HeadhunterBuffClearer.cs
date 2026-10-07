using Il2Cpp;
using LastEpoch_Hud.Scripts.Core.CustomItems;
using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Buffs;
using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Config;
using LastEpoch_Hud.Scripts.Mods.Items.CustomItems.Headhunter.Bar;

namespace LastEpoch_Hud.Scripts.Mods.Items.CustomItems.Headhunter.Buffs;

/// <summary>Removes every Headhunter buff of one resolved config from the player.</summary>
internal static class HeadhunterBuffClearer
{
    private static readonly HeadhunterClearRule _clear = new();

    public static void ClearAll(HeadhunterResolvedConfig config)
    {
        HeadhunterConfigLoader.Stacks?.Reset();
        StatBuffs buffs = HeadhunterBuffSink.PlayerBuffs();
        if (config == null || buffs == null)
        {
            return;
        }

        HeadhunterBuffSink.Apply(buffs, _clear.RemoveAll(config));
        HeadhunterBuffBar.MarkDirty();
    }
}
