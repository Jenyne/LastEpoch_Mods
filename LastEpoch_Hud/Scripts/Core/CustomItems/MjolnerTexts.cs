namespace LastEpoch_Hud.Scripts.Core.CustomItems;

/// <summary>Mjolner name and lore. The description is built from save settings.</summary>
public static class MjolnerTexts
{
    public static readonly LocalizedText UniqueName = new("Mjölner", ("ru", "Мьёльнир"));

    public static readonly LocalizedText Lore = new(
        "Look the storm in the eye and you will have its respect.",
        ("fr", "Entrez dans l'œil de la tempête et vous gagnerez son respect."),
        ("de", "Blickt dem Sturm ins Auge,\r\nund sein Respekt ist Euch gewiss."),
        ("pt", "Encare o olho da tempestade, e ela te respeitará.")
    );
}
