namespace LastEpoch_Hud.Scripts.Core.CustomItems;

/// <summary>Headhunter names and lore.</summary>
public static class HeadhunterTexts
{
    public static readonly LocalizedText SubtypeName = new(
        "HH Leather belt",
        ("fr", "HH Ceinture en cuir"),
        ("de", "HH Ledergürtel"),
        ("ru", "HH Ремень"),
        ("pt", "Cinto de Couro")
    );

    public static readonly LocalizedText UniqueName = new(
        "Headhunter",
        ("fr", "Chasseur de têtes"),
        ("de", "Kopfjäger"),
        ("ru", "Охотник за головами"),
        ("pt", "Caçador de Cabeças")
    );

    public static readonly LocalizedText Lore = new(
        "A man's soul rules from a cavern of bone, learns and\r\njudges through flesh-born windows. The heart is meat.\r\nThe head is where the Man is.\"\r\n- Lavianga, Advisor to Kaom",
        (
            "fr",
            "L'âme d'un homme règne depuis une caverne d'os,\r\napprend et juge à travers des fenêtres plantées dans la chair.\r\nLe cœur est un morceau de viande. La tête est le siège de l'homme.\r\n- Lavianga, conseiller de Kaom"
        ),
        (
            "de",
            "Die Seele eines Mannes regiert\r\naus einer Höhle aus Knochen,\r\nlernt und urteilt aus Fenstern,\r\ngeboren aus Fleisch. Das Herz ist Fleisch.\r\nDer Kopf ist dort, wo der Mann ist.\r\n– Lavianga, Berater von Kaom"
        )
    );
}
