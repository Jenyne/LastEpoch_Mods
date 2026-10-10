using System;
using System.Collections.Generic;
using LastEpoch_Hud.Scripts.ModUI.Pages;
using UnityEngine;

namespace LastEpoch_Hud.Scripts.ModUI.Shell;

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
            // ForceDropBuilder supplies the view; there is no search root, so the page stays out of search.
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
