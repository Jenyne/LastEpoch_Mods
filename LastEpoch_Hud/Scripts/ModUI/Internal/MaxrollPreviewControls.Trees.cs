using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using LastEpoch_Hud.Scripts.Core.BuildImport;
using UnityEngine.UI;

namespace LastEpoch_Hud.Scripts.ModUI;

internal static partial class MaxrollPreviewControls
{
    static readonly List<MaxrollTreeSnapshot> TreeRows = new();
    static MaxrollTreeSnapshot selectedTree;
    static Button historyButton;
    static bool showHistory;
    static bool IsTreeSection => section >= 4;

    static void BuildTreeRows(MaxrollTreePreview preview)
    {
        TreeRows.Clear();
        if (!IsTreeSection || preview == null)
            return;
        if (section == 4)
        {
            if (preview.Passives != null)
            {
                TreeRows.Add(preview.Passives);
                selectedTree ??= preview.Passives;
            }
        }
        else
        {
            // The first Skills row previews active and specialized slot order.
            TreeRows.Add(null);
            TreeRows.AddRange(preview.Skills);
        }
    }

    static string TreeName(MaxrollTreeSnapshot tree) =>
        section == 4 ? L("Passives") : L("Planner tree ID") + ": " + Short(tree.PlannerId, 70);

    static string TreeCaption(MaxrollTreeSnapshot tree) =>
        tree == null
            ? L("Skill Slots") + "\n" + L("Active Skills") + " / " + L("Specialized Skills")
            : TreeName(tree)
                + "\n"
                + (
                    tree.IsDecoded
                        ? L("Points")
                            + ": "
                            + tree.TotalPoints.ToString(CultureInfo.InvariantCulture)
                        : L("Tree history could not be decoded.")
                );

    static void DescribeTree(MaxrollTreePreview preview)
    {
        if (preview == null)
            return;
        if (selectedTree == null && section == 4)
        {
            Fields.Add((L("No tree data in this variant."), false));
            return;
        }
        Fields.Add((L("Planner IDs require game validation before point allocation."), false));
        Fields.Add(
            (
                L("Class ID")
                    + ": "
                    + Number(preview.ClassId)
                    + "; "
                    + L("Mastery ID")
                    + ": "
                    + Number(preview.MasteryId),
                false
            )
        );
        Fields.Add((L("Level") + ": " + Number(preview.Level), false));
        if (selectedTree == null)
        {
            SkillSlots(preview.ActiveSkills, "Active Skills");
            SkillSlots(preview.SpecializedSkills, "Specialized Skills");
            if (preview.Skills.Count == 0)
                Fields.Add((L("No tree data in this variant."), false));
            return;
        }
        var tree = selectedTree;
        Fields.Add(
            (L("History cursor") + ": " + Number(tree.Position) + "/" + tree.HistoryLength, false)
        );
        if (!tree.IsDecoded)
        {
            Fields.Add((L("Tree history could not be decoded."), false));
            foreach (string issue in tree.Issues)
                Fields.Add((issue, false));
            return;
        }
        Fields.Add(
            (L("Points") + ": " + tree.TotalPoints.ToString(CultureInfo.InvariantCulture), false)
        );
        if (showHistory)
        {
            foreach (var step in tree.Steps)
            {
                string prefix = L("Step") + " " + (step.HistoryIndex + 1) + ": ";
                if (step.IsBulk)
                    prefix += L("Bulk assignment") + "; ";
                if (step.RanksAfter.Count == 0)
                    Fields.Add((prefix + "{}", false));
                foreach (var pair in step.RanksAfter)
                    Fields.Add(
                        (
                            prefix
                                + L("Planner node ID")
                                + " "
                                + pair.Key
                                + ", "
                                + L("Rank")
                                + " "
                                + pair.Value,
                            false
                        )
                    );
            }
        }
        else
            foreach (var pair in tree.Ranks.OrderBy(pair => pair.Key))
                Fields.Add(
                    (
                        L("Planner node ID") + " " + pair.Key + ": " + L("Rank") + " " + pair.Value,
                        false
                    )
                );
    }

    static string Number(int? value) => value?.ToString(CultureInfo.InvariantCulture) ?? "—";

    static void SkillSlots(IReadOnlyList<JsonElement> slots, string label)
    {
        for (int i = 0; i < slots.Count; i++)
        {
            var value = slots[i];
            string text =
                value.ValueKind == JsonValueKind.Null ? L("Empty slot")
                : value.ValueKind == JsonValueKind.String ? value.GetString()
                : value.GetRawText();
            Fields.Add((L(label) + " " + (i + 1) + ": " + text, false));
        }
    }

    static void AddTreeIssues(MaxrollTreePreview preview)
    {
        if (preview == null)
            return;
        foreach (string issue in preview.Issues)
            Fields.Add((issue, false));
        if (preview.Passives != null)
            foreach (string issue in preview.Passives.Issues)
                Fields.Add((L("Passives") + ": " + issue, false));
        foreach (var tree in preview.Skills)
        foreach (string issue in tree.Issues)
            Fields.Add((tree.PlannerId + ": " + issue, false));
    }
}
