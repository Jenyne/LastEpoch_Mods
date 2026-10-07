using System.Collections.Generic;
using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter;

namespace LastEpoch_Hud.Scripts.Mods.Items.CustomItems.Headhunter;

internal static class HeadhunterLocales
{
    internal static string Description(IReadOnlyDictionary<string, string> texts)
    {
        return HeadhunterDescription.Text(texts, HeadhunterConfigLoader.Current.DurationSeconds);
    }
}
