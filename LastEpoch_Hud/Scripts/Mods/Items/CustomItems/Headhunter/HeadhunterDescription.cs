namespace LastEpoch_Hud.Scripts.Mods.Items.CustomItems.Headhunter;

internal static class HeadhunterDescription
{
    public static string En =
        "When you or your minions Kill a monster, you gain "
        + Save_Manager.instance.data.Items.Headhunter.MinGenerated
        + " to "
        + Save_Manager.instance.data.Items.Headhunter.MaxGenerated
        + " random Modifiers for "
        + Save_Manager.instance.data.Items.Headhunter.BuffDuration
        + " seconds";
    public static string Fr =
        "Lorsque vous ou vos serviteurs tuez un monstre, vous gagnez "
        + Save_Manager.instance.data.Items.Headhunter.MinGenerated
        + " à "
        + Save_Manager.instance.data.Items.Headhunter.MaxGenerated
        + " modificateurs aléatoires pendant "
        + Save_Manager.instance.data.Items.Headhunter.BuffDuration
        + " secondes.";

    // Add all languages here
}
