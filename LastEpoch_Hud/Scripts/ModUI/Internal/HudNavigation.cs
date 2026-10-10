using System;
using System.Collections.Generic;
using UnityEngine;

namespace LastEpoch_Hud.Scripts.ModUI;

// A page's navigation metadata and runtime lifecycle live together here. Adding a
// page should require one entry, not another branch in HudLayout and a separate
// search-routing table.
internal sealed class HudPageDefinition
{
    private readonly Action<GameObject, GameObject, Font> build;
    private readonly Action show;
    private readonly Action hide;
    private readonly Action refresh;

    public readonly string Id;
    public readonly string Label;
    public readonly string SearchRootName;

    public HudPageDefinition(
        string id,
        string label,
        string searchRootName,
        Action<GameObject, GameObject, Font> build,
        Action show,
        Action hide,
        Action refresh = null
    )
    {
        Id = id;
        Label = label;
        SearchRootName = searchRootName;
        this.build = build;
        this.show = show;
        this.hide = hide;
        this.refresh = refresh;
    }

    public void Build(GameObject parent, GameObject hud, Font font) =>
        build?.Invoke(parent, hud, font);

    public void Show() => show?.Invoke();

    public void Hide() => hide?.Invoke();

    public void Refresh() => refresh?.Invoke();
}

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

internal static class HudNavigation
{
    public static readonly HudSectionDefinition[] Sections =
    {
        new(
            "character",
            "Utilities",
            true,
            Page(
                "character.main",
                "Character",
                "Utilities_Character",
                Utilities_Character.Build,
                Utilities_Character.Show,
                Utilities_Character.Hide,
                Utilities_Character.Refresh
            ),
            Page(
                "character.multipliers",
                "Multipliers",
                "Utilities_Multipliers",
                Utilities_Multipliers.Build,
                Utilities_Multipliers.Show,
                Utilities_Multipliers.Hide
            ),
            Page(
                "character.currency",
                "Currency",
                "Utilities_Currency",
                (parent, _, font) => Utilities_Currency.Build(parent, font),
                Utilities_Currency.Show,
                Utilities_Currency.Hide
            ),
            Page(
                "character.buffs",
                "Buffs",
                "Utilities_Buffs",
                Utilities_Buffs.Build,
                Utilities_Buffs.Show,
                Utilities_Buffs.Hide
            ),
            Page(
                "character.qol",
                "QOL",
                "Utilities_QOL",
                Utilities_QOL.Build,
                Utilities_QOL.Show,
                Utilities_QOL.Hide,
                Utilities_QOL.Refresh
            )
        ),
        new(
            "items",
            "Items",
            true,
            Page(
                "items.drop",
                "Drop",
                "Items_Drop",
                Items_Drop.Build,
                Items_Drop.Show,
                Items_Drop.Hide,
                Items_Drop.Refresh
            ),
            // Force Drop deliberately stays outside search until its separate UI
            // rewrite is complete. Its legacy body is still managed by this page.
            Page(
                "items.force-drop",
                "Force Drop",
                null,
                null,
                Items_ForceDrop.Show,
                Items_ForceDrop.Hide
            ),
            Page(
                "items.crafting",
                "Crafting Slot",
                "Items_CraftingSlot",
                Items_CraftingSlot.Build,
                Items_CraftingSlot.Show,
                Items_CraftingSlot.Hide,
                Items_CraftingSlot.Refresh
            )
        ),
        new(
            "world",
            "World",
            true,
            Page(
                "world.difficulty",
                "Difficulty",
                "World_Difficulty",
                World_Difficulty.Build,
                World_Difficulty.Show,
                World_Difficulty.Hide,
                World_Difficulty.Refresh
            ),
            Page(
                "world.monoliths",
                "Monoliths",
                "World_Monoliths",
                World_Monoliths.Build,
                World_Monoliths.Show,
                World_Monoliths.Hide,
                World_Monoliths.Refresh
            ),
            Page(
                "world.misc",
                "Misc",
                "World_Misc",
                World_Misc.Build,
                World_Misc.Show,
                World_Misc.Hide,
                World_Misc.Refresh
            ),
            Page(
                "world.camera",
                "Camera",
                "World_Camera",
                World_Camera.Build,
                World_Camera.Show,
                World_Camera.Hide,
                World_Camera.Refresh
            )
        ),
        new(
            "skills",
            "Skills",
            true,
            Page(
                "skills.minions",
                "Minions",
                "Skills_Minions",
                Skills_Minions.Build,
                Skills_Minions.Show,
                Skills_Minions.Hide,
                Skills_Minions.Refresh
            ),
            Page(
                "skills.companions",
                "Companions",
                "Skills_Companions",
                Skills_Companions.Build,
                Skills_Companions.Show,
                Skills_Companions.Hide,
                Skills_Companions.Refresh
            ),
            Page(
                "skills.summon",
                "Summon",
                "Skills_Summon",
                Skills_Summon.Build,
                Skills_Summon.Show,
                Skills_Summon.Hide,
                Skills_Summon.Refresh
            ),
            Page(
                "skills.qol",
                "QOL",
                "Skills_QOL",
                Skills_QOL.Build,
                Skills_QOL.Show,
                Skills_QOL.Hide,
                Skills_QOL.Refresh
            )
        ),
    };

    public static IEnumerable<HudPageDefinition> Pages
    {
        get
        {
            foreach (var section in Sections)
            foreach (var page in section.Pages)
                yield return page;
        }
    }

    public static string SearchPageId(string rootName)
    {
        if (string.IsNullOrEmpty(rootName))
            return null;
        foreach (var page in Pages)
            if (string.Equals(page.SearchRootName, rootName, StringComparison.Ordinal))
                return page.Id;
        return null;
    }

    public static bool TryGetPage(
        string pageId,
        out HudSectionDefinition section,
        out HudPageDefinition page
    )
    {
        foreach (var candidateSection in Sections)
        foreach (var candidatePage in candidateSection.Pages)
            if (string.Equals(candidatePage.Id, pageId, StringComparison.Ordinal))
            {
                section = candidateSection;
                page = candidatePage;
                return true;
            }
        section = null;
        page = null;
        return false;
    }

    public static bool TryValidate(out string error)
    {
        var sectionIds = new HashSet<string>(StringComparer.Ordinal);
        var pageIds = new HashSet<string>(StringComparer.Ordinal);
        var searchRoots = new HashSet<string>(StringComparer.Ordinal);
        foreach (var section in Sections)
        {
            if (string.IsNullOrWhiteSpace(section.Id) || !sectionIds.Add(section.Id))
            {
                error = "Missing or duplicate HUD section id: " + section.Id;
                return false;
            }
            foreach (var page in section.Pages)
            {
                if (string.IsNullOrWhiteSpace(page.Id) || !pageIds.Add(page.Id))
                {
                    error = "Missing or duplicate HUD page id: " + page.Id;
                    return false;
                }
                if (
                    !string.IsNullOrEmpty(page.SearchRootName)
                    && !searchRoots.Add(page.SearchRootName)
                )
                {
                    error = "Duplicate HUD search root: " + page.SearchRootName;
                    return false;
                }
            }
        }
        error = null;
        return true;
    }

    private static HudPageDefinition Page(
        string id,
        string label,
        string searchRootName,
        Action<GameObject, GameObject, Font> build,
        Action show,
        Action hide,
        Action refresh = null
    ) => new(id, label, searchRootName, build, show, hide, refresh);
}
