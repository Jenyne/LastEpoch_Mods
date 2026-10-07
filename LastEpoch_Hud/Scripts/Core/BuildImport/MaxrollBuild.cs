using System;
using System.Collections.Generic;
using System.Text.Json;

namespace LastEpoch_Hud.Scripts.Core.BuildImport;

// These snapshots preserve planner data. They are not game items or legality verdicts.
public sealed class MaxrollBuild
{
    public string Name { get; internal set; }
    public MaxrollLink Link { get; internal set; }
    public Uri SourceEndpoint { get; internal set; }
    public string ResponseJson { get; internal set; }
    public JsonElement Data { get; internal set; }
    public IReadOnlyList<MaxrollVariant> Variants { get; internal set; }
    public int? SelectedVariantIndex { get; internal set; }
    public string SelectionIssue { get; internal set; }
    public MaxrollVariant SelectedVariant =>
        SelectedVariantIndex.HasValue ? Variants[SelectedVariantIndex.Value] : null;

    public void SelectVariant(int zeroBasedIndex)
    {
        if (zeroBasedIndex < 0 || zeroBasedIndex >= Variants.Count)
            throw new ArgumentOutOfRangeException(nameof(zeroBasedIndex), "Unknown gear variant.");
        SelectedVariantIndex = zeroBasedIndex;
        SelectionIssue = null;
    }
}

public sealed class MaxrollVariant
{
    public string Name { get; internal set; }
    public string Kind { get; internal set; }
    public int SourceIndex { get; internal set; }
    public int? EmbedId { get; internal set; }
    public JsonElement Data { get; internal set; }
    public IReadOnlyList<MaxrollPlacement> Placements { get; internal set; }
    public IReadOnlyList<string> Issues { get; internal set; }
    public MaxrollTreePreview Trees { get; internal set; }
}

public sealed class MaxrollPlacement
{
    public string Section { get; internal set; }
    public string Slot { get; internal set; }
    public int? GridIndex { get; internal set; }
    public string Reference { get; internal set; }
    public bool IsEmpty { get; internal set; }
    public JsonElement SourceValue { get; internal set; }

    // Resolved inline definition; includes every original field, including unknown fields.
    public JsonElement? Item { get; internal set; }
}
