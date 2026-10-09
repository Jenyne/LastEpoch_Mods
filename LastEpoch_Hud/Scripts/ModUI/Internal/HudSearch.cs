using System;
using System.Collections.Generic;
using System.Linq;
using LastEpoch_Hud.Scripts.Core.ModUI;

namespace LastEpoch_Hud.Scripts.ModUI;

internal sealed class HudSearchEntry
{
    public string Card { get; init; }
    public string Label { get; init; }
}

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

internal interface IHudSearchPage
{
    string PageId { get; }
    IReadOnlyList<HudSearchEntry> SearchEntries { get; }
    void ApplySearch(string query);
    void ClearSearch();
}

internal static class HudSearch
{
    private static readonly Dictionary<string, IHudSearchPage> Pages = new(StringComparer.Ordinal);

    public static void Reset() => Pages.Clear();

    public static void Register(IHudSearchPage page)
    {
        if (page == null || string.IsNullOrWhiteSpace(page.PageId))
            return;
        // ForceDrop intentionally has no searchable page mapping, but keep this
        // guard so a future renderer cannot accidentally add it to global search.
        if (string.Equals(page.PageId, "items.force-drop", StringComparison.Ordinal))
            return;
        Pages[page.PageId] = page;
    }

    public static IReadOnlyList<HudSearchMatch> Find(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return Array.Empty<HudSearchMatch>();

        var matches = new List<HudSearchMatch>();
        foreach (var pair in Pages)
        {
            if (!HudNavigation.TryGetPage(pair.Key, out var section, out var page))
                continue;
            foreach (var entry in pair.Value.SearchEntries)
            {
                int score = HudSearchText.Score(
                    query,
                    entry.Label,
                    entry.Card,
                    page.Label,
                    section.Label
                );
                if (score < 0)
                    continue;
                matches.Add(
                    new HudSearchMatch
                    {
                        Page = pair.Value,
                        PageId = page.Id,
                        Section = section.Label,
                        Tab = page.Label,
                        Card = entry.Card,
                        Label = entry.Label,
                        Score = score,
                    }
                );
            }
        }

        return matches
            .OrderBy(match => match.Score)
            .ThenBy(match => match.Section, StringComparer.OrdinalIgnoreCase)
            .ThenBy(match => match.Tab, StringComparer.OrdinalIgnoreCase)
            .ThenBy(match => match.Card, StringComparer.OrdinalIgnoreCase)
            .ThenBy(match => match.Label, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    public static void ClearAll()
    {
        foreach (var page in Pages.Values)
            page.ClearSearch();
    }
}
