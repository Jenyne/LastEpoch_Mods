namespace LastEpoch_Hud.Scripts.ModUI.Shell;

internal sealed class HudSearchMatch
{
    public IHudSearchPage Page { get; init; }
    public string PageId { get; init; }
    public string Section { get; init; }
    public string Tab { get; init; }
    public string Card { get; init; }
    public string Label { get; init; }
    public int Score { get; init; }

    public string Breadcrumb =>
        string.IsNullOrWhiteSpace(Card)
            ? Section + "  ›  " + Tab
            : Section + "  ›  " + Tab + "  ›  " + Card;
}
