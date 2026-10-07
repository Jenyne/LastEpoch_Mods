using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using LastEpoch_Hud.Scripts.Core.BuildImport;
using UnityEngine.UI;

namespace LastEpoch_Hud.Scripts.ModUI;

internal static partial class MaxrollPreviewControls
{
    static readonly List<MaxrollTreeView> TreeRows = new();
    static MaxrollTreeSnapshot selectedTree;
    static MaxrollTreePreview treeProfile;
    static int treeSection = -1,
        selectedTreeIndex = -1;
    static bool IsTreeSection => section >= 4;
    static MaxrollTreeView SelectedView =>
        selectedTreeIndex >= 0 && selectedTreeIndex < TreeRows.Count
            ? TreeRows[selectedTreeIndex]
            : null;

    static void ResetTreeView()
    {
        selectedTree = null;
        selectedTreeIndex = -1;
        ResetGraph();
    }

    static void BuildTreeRows(MaxrollTreePreview preview)
    {
        if (!ReferenceEquals(treeProfile, preview) || treeSection != section)
        {
            TreeRows.Clear();
            treeProfile = preview;
            treeSection = section;
            if (IsTreeSection && preview != null)
            {
                if (section == 5)
                    TreeRows.Add(null); // named skill bar overview
                var catalog = Session.Build?.TreeCatalog;
                if (catalog != null)
                    TreeRows.AddRange(catalog.Views(preview, section == 4));
            }
            ResetTreeView();
        }
        if (IsTreeSection && selectedTreeIndex < 0 && TreeRows.Count > 0)
        {
            selectedTreeIndex = section == 5 && TreeRows.Count > 1 ? 1 : 0;
            if (section == 4 && preview.MasteryId.HasValue)
            {
                int mastery = TreeRows.FindIndex(view => view?.Mastery == preview.MasteryId);
                if (mastery >= 0)
                    selectedTreeIndex = mastery;
            }
        }
        selectedTree = SelectedView?.Snapshot;
    }

    static void SelectTreeRow(int index)
    {
        selectedTreeIndex = index;
        selectedTree = SelectedView?.Snapshot;
        ResetGraph();
    }

    static string TreeCaption(MaxrollTreeView view) =>
        view == null
            ? L("Skill Slots")
            : (string.IsNullOrWhiteSpace(view.Name) ? L("Unknown skill") : view.Name)
                + "\n"
                + (
                    view.Snapshot.IsDecoded
                        ? L("Points")
                            + ": "
                            + view.TotalPoints.ToString(CultureInfo.InvariantCulture)
                        : L("Tree history could not be decoded.")
                );

    static void SkillSlots(IReadOnlyList<JsonElement> slots, string label)
    {
        Fields.Add((L(label), false));
        foreach (var value in slots)
        {
            string name =
                value.ValueKind == JsonValueKind.Null ? L("Empty slot")
                : value.ValueKind == JsonValueKind.String
                    ? Session.Build?.TreeCatalog?.AbilityName(value.GetString())
                        ?? L("Unknown skill")
                : L("Unknown skill");
            Fields.Add((name, false));
        }
    }

    static void AddTreeIssues(MaxrollTreePreview preview)
    {
        if (preview == null)
            return;
        if (Session.Build.TreeCatalogIssue != null)
            Fields.Add((Session.Build.TreeCatalogIssue, false));
        foreach (string issue in preview.Issues)
            Fields.Add((issue, false));
        if (preview.Passives != null)
            foreach (string issue in preview.Passives.Issues)
                Fields.Add((L("Passives") + ": " + issue, false));
        foreach (var tree in preview.Skills)
        {
            foreach (string issue in tree.Issues)
                Fields.Add((tree.PlannerId + ": " + issue, false));
            if (
                Session.Build.TreeCatalog != null
                && !Session.Build.TreeCatalog.Trees.ContainsKey(tree.PlannerId)
            )
                Fields.Add((L("Tree data unavailable.") + " " + tree.PlannerId, false));
        }
    }
}
