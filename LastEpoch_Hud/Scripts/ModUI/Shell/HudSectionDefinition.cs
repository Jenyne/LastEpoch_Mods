namespace LastEpoch_Hud.Scripts.ModUI.Shell;

internal sealed class HudSectionDefinition
{
    public readonly string Id;
    public readonly string Label;
    public readonly bool Accordion;
    public readonly HudPageDefinition[] Pages;

    public HudSectionDefinition(
        string id,
        string label,
        bool accordion,
        params HudPageDefinition[] pages
    )
    {
        Id = id;
        Label = label;
        Accordion = accordion;
        Pages = pages;
    }
}
