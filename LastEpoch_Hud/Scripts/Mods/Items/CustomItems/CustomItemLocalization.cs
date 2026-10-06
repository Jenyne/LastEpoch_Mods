using LastEpoch_Hud.Scripts.Core.CustomItems;

namespace LastEpoch_Hud.Scripts.Mods.Items.CustomItems;

/// <summary>Shared text table and current game language for custom items.</summary>
public static class CustomItemLocalization
{
    public static CustomItemTextTable Table { get; } = new();

    public static string Language()
    {
        return Locales.current == Locales.Selected.Unknow ? "" : Locales.dictionnary_filename;
    }

    public static string Text(LocalizedText text)
    {
        return text.For(Language());
    }

    public static string Resolve(string key)
    {
        return Table.Resolve(key, Language());
    }
}
