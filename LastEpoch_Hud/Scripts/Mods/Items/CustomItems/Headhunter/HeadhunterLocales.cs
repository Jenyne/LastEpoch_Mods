using Il2Cpp;
using MelonLoader;

namespace LastEpoch_Hud.Scripts.Mods.Items.CustomItems.Headhunter;

internal static class HeadhunterLocales
{
    internal static string DescriptionWhenSaveReady()
    {
        if (Save_Manager.instance.IsNullOrDestroyed() || !Save_Manager.instance.initialized)
        {
            return null;
        }

        return CurrentDescription();
    }

    private static string CurrentDescription()
    {
        string result = "";
        switch (Locales.current)
        {
            case Locales.Selected.English:
            {
                result = HeadhunterDescription.En;
                break;
            }
            case Locales.Selected.French:
            {
                result = HeadhunterDescription.Fr;
                break;
            }
            case Locales.Selected.Korean:
            case Locales.Selected.German:
            case Locales.Selected.Russian:
            case Locales.Selected.Polish:
            case Locales.Selected.Portuguese:
            case Locales.Selected.Chinese:
            case Locales.Selected.Spanish:
            {
                result = HeadhunterDescription.En;
                break;
            }
        }

        return result;
    }
}
