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
                UtilitiesCharacterPage.Build,
                UtilitiesCharacterPage.Show,
                UtilitiesCharacterPage.Hide,
                UtilitiesCharacterPage.Refresh
            ),
            Page(
                "character.multipliers",
                "Multipliers",
                "Utilities_Multipliers",
                UtilitiesMultipliersPage.Build,
                UtilitiesMultipliersPage.Show,
                UtilitiesMultipliersPage.Hide
            ),
            Page(
                "character.currency",
                "Currency",
                "Utilities_Currency",
                (parent, _, font) => UtilitiesCurrencyPage.Build(parent, font),
                UtilitiesCurrencyPage.Show,
                UtilitiesCurrencyPage.Hide
            ),
            Page(
                "character.buffs",
                "Buffs",
                "Utilities_Buffs",
                UtilitiesBuffsPage.Build,
                UtilitiesBuffsPage.Show,
                UtilitiesBuffsPage.Hide
            ),
            Page(
                "character.qol",
                "QOL",
                "Utilities_QOL",
                UtilitiesQolPage.Build,
                UtilitiesQolPage.Show,
                UtilitiesQolPage.Hide,
                UtilitiesQolPage.Refresh
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
                ItemsDropPage.Build,
                ItemsDropPage.Show,
                ItemsDropPage.Hide,
                ItemsDropPage.Refresh
            ),
            // ForceDropBuilder supplies the view; there is no search root, so the page stays out of search.
            Page(
                "items.force-drop",
                "Force Drop",
                null,
                null,
                ItemsForceDropPage.Show,
                ItemsForceDropPage.Hide
            ),
            Page(
                "items.crafting",
                "Crafting Slot",
                "Items_CraftingSlot",
                ItemsCraftingSlotPage.Build,
                ItemsCraftingSlotPage.Show,
                ItemsCraftingSlotPage.Hide,
                ItemsCraftingSlotPage.Refresh
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
                WorldDifficultyPage.Build,
                WorldDifficultyPage.Show,
                WorldDifficultyPage.Hide,
                WorldDifficultyPage.Refresh
            ),
            Page(
                "world.monoliths",
                "Monoliths",
                "World_Monoliths",
                WorldMonolithsPage.Build,
                WorldMonolithsPage.Show,
                WorldMonolithsPage.Hide,
                WorldMonolithsPage.Refresh
            ),
            Page(
                "world.misc",
                "Misc",
                "World_Misc",
                WorldMiscPage.Build,
                WorldMiscPage.Show,
                WorldMiscPage.Hide,
                WorldMiscPage.Refresh
            ),
            Page(
                "world.camera",
                "Camera",
                "World_Camera",
                WorldCameraPage.Build,
                WorldCameraPage.Show,
                WorldCameraPage.Hide,
                WorldCameraPage.Refresh
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
                SkillsMinionsPage.Build,
                SkillsMinionsPage.Show,
                SkillsMinionsPage.Hide,
                SkillsMinionsPage.Refresh
            ),
            Page(
                "skills.companions",
                "Companions",
                "Skills_Companions",
                SkillsCompanionsPage.Build,
                SkillsCompanionsPage.Show,
                SkillsCompanionsPage.Hide,
                SkillsCompanionsPage.Refresh
            ),
            Page(
                "skills.summon",
                "Summon",
                "Skills_Summon",
                SkillsSummonPage.Build,
                SkillsSummonPage.Show,
                SkillsSummonPage.Hide,
                SkillsSummonPage.Refresh
            ),
            Page(
                "skills.qol",
                "QOL",
                "Skills_QOL",
                SkillsQolPage.Build,
                SkillsQolPage.Show,
                SkillsQolPage.Hide,
                SkillsQolPage.Refresh
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
