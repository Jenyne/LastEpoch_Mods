using System.Collections.Generic;

namespace LastEpoch_Hud.Scripts.ModUI.Shell;

internal interface IHudSearchPage
{
    string PageId { get; }
    IReadOnlyList<HudSearchEntry> SearchEntries { get; }
    void ApplySearch(string query);
    void ClearSearch();
}
