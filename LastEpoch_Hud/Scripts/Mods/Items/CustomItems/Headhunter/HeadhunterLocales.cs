using System.Collections.Generic;
using Il2Cpp;
using LastEpoch_Hud.Scripts.Core.CustomItems;
using MelonLoader;

namespace LastEpoch_Hud.Scripts.Mods.Items.CustomItems.Headhunter;

internal static class HeadhunterLocales
{
    internal static string Description(IReadOnlyDictionary<string, string> texts)
    {
        if (Save_Manager.instance.IsNullOrDestroyed() || !Save_Manager.instance.initialized)
        {
            return null;
        }

        Save_Manager.Data.Headhunter headhunter = Save_Manager.instance.data.Items.Headhunter;
        return HeadhunterDescription.Text(
            texts,
            headhunter.MinGenerated,
            headhunter.MaxGenerated,
            headhunter.BuffDuration
        );
    }
}
