using System.Collections.Generic;

namespace LastEpoch_Hud.Scripts.Core.CustomItems;

/// <summary>Headhunter description text from its config.</summary>
public static class HeadhunterDescription
{
    public static string Text(IReadOnlyDictionary<string, string> texts, float durationSeconds)
    {
        string template = LocaleText.Get(texts, CustomItemLocaleKeys.HeadhunterDescription);
        return TextTemplate.Fill(template, durationSeconds);
    }
}
