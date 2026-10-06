using System.Collections.Generic;

namespace LastEpoch_Hud.Scripts.Core.CustomItems;

/// <summary>Headhunter description text from its settings.</summary>
public static class HeadhunterDescription
{
    public static string Text(
        IReadOnlyDictionary<string, string> texts,
        int minGenerated,
        int maxGenerated,
        float buffDuration
    )
    {
        string template = LocaleText.Get(texts, CustomItemLocaleKeys.HeadhunterDescription);
        return TextTemplate.Fill(template, minGenerated, maxGenerated, buffDuration);
    }
}
