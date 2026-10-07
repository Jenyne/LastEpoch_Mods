using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text.Json;

namespace LastEpoch_Hud.Scripts.Core.BuildImport;

// Planner IDs and ranks only. Decoding does not validate game prerequisites or spend points.
public sealed class MaxrollTreePreview
{
    public int? ClassId { get; internal set; }
    public int? MasteryId { get; internal set; }
    public int? Level { get; internal set; }
    public MaxrollTreeSnapshot Passives { get; internal set; }
    public IReadOnlyList<MaxrollTreeSnapshot> Skills { get; internal set; }
    public IReadOnlyList<JsonElement> ActiveSkills { get; internal set; }
    public IReadOnlyList<JsonElement> SpecializedSkills { get; internal set; }
    public IReadOnlyList<string> Issues { get; internal set; }
}

public sealed class MaxrollTreeSnapshot
{
    public string PlannerId { get; internal set; }
    public JsonElement Source { get; internal set; }
    public int? Position { get; internal set; }
    public int HistoryLength { get; internal set; }
    public bool IsDecoded { get; internal set; }
    public long TotalPoints { get; internal set; }
    public IReadOnlyDictionary<int, int> Ranks { get; internal set; }
    public IReadOnlyList<MaxrollTreeStep> Steps { get; internal set; }
    public IReadOnlyList<string> Issues { get; internal set; }
}

public sealed class MaxrollTreeStep
{
    public int HistoryIndex { get; internal set; }
    public bool IsBulk { get; internal set; }
    public IReadOnlyDictionary<int, int> RanksAfter { get; internal set; }
}

public static class MaxrollTreeParser
{
    public static MaxrollTreePreview Parse(JsonElement profile)
    {
        var issues = new List<string>();
        var skills = new List<MaxrollTreeSnapshot>();
        MaxrollTreeSnapshot passives = null;
        if (profile.ValueKind != JsonValueKind.Object)
            issues.Add("Tree preview requires a profile object.");
        else
        {
            if (profile.TryGetProperty("passives", out var value))
                passives = Decode(value, "passives");
            if (profile.TryGetProperty("skillTrees", out value))
            {
                if (value.ValueKind != JsonValueKind.Object)
                    issues.Add("skillTrees: expected an object.");
                else
                    foreach (var tree in value.EnumerateObject())
                        skills.Add(Decode(tree.Value, tree.Name));
            }
        }
        return new MaxrollTreePreview
        {
            ClassId = Integer(profile, "class"),
            MasteryId = Integer(profile, "mastery"),
            Level = Integer(profile, "level"),
            Passives = passives,
            Skills = skills.AsReadOnly(),
            ActiveSkills = Slots(profile, "activeSkills", issues),
            SpecializedSkills = Slots(profile, "specializedSkills", issues),
            Issues = issues.AsReadOnly(),
        };
    }

    public static MaxrollTreeSnapshot Decode(JsonElement source, string plannerId)
    {
        var issues = new List<string>();
        var ranks = new Dictionary<int, int>();
        var steps = new List<MaxrollTreeStep>();
        int? position = null;
        int length = 0;
        bool decoded = false;
        if (source.ValueKind != JsonValueKind.Object)
            issues.Add("Expected a tree history object.");
        else if (
            !source.TryGetProperty("history", out var history)
            || history.ValueKind != JsonValueKind.Array
        )
            issues.Add("history: expected an array.");
        else
        {
            length = history.GetArrayLength();
            position = Integer(source, "position");
            if (!position.HasValue || position < 0 || position > length)
                issues.Add("position: expected an integer cursor between zero and history length.");
            else
            {
                decoded = true;
                // Maxroll captures state immediately before history[position]. Object
                // steps overwrite only the listed ranks (Object.assign), not the whole
                // tree and not an increment. Steps after the cursor remain in Source.
                for (int index = 0; index < position.Value; index++)
                {
                    var entry = history[index];
                    var changes = new Dictionary<int, int>();
                    if (
                        entry.ValueKind == JsonValueKind.Number
                        && entry.TryGetInt32(out int node)
                        && node >= 0
                    )
                    {
                        ranks.TryGetValue(node, out int previous);
                        if (previous == int.MaxValue)
                        {
                            issues.Add("history[" + index + "]: node rank overflows an integer.");
                            decoded = false;
                            break;
                        }
                        changes[node] = previous + 1;
                    }
                    else if (entry.ValueKind == JsonValueKind.Object)
                    {
                        foreach (var pair in entry.EnumerateObject())
                        {
                            if (
                                !int.TryParse(
                                    pair.Name,
                                    NumberStyles.None,
                                    CultureInfo.InvariantCulture,
                                    out node
                                )
                                || node < 0
                                || pair.Name != node.ToString(CultureInfo.InvariantCulture)
                                || pair.Value.ValueKind != JsonValueKind.Number
                                || !pair.Value.TryGetInt32(out int rank)
                                || rank < 0
                                || changes.ContainsKey(node)
                            )
                            {
                                issues.Add(
                                    "history["
                                        + index
                                        + "]: expected unique numeric node IDs and nonnegative integer ranks."
                                );
                                decoded = false;
                                break;
                            }
                            changes.Add(node, rank);
                        }
                    }
                    else
                    {
                        issues.Add("history[" + index + "]: unsupported allocation step.");
                        decoded = false;
                    }
                    if (!decoded)
                        break;
                    foreach (var pair in changes)
                        ranks[pair.Key] = pair.Value;
                    steps.Add(
                        new MaxrollTreeStep
                        {
                            HistoryIndex = index,
                            IsBulk = entry.ValueKind == JsonValueKind.Object,
                            RanksAfter = new ReadOnlyDictionary<int, int>(changes),
                        }
                    );
                }
            }
        }
        // Never expose a partially decoded tree as a usable allocation snapshot.
        if (!decoded)
        {
            ranks.Clear();
            steps.Clear();
        }
        long total = 0;
        foreach (int rank in ranks.Values)
            total += rank;
        return new MaxrollTreeSnapshot
        {
            PlannerId = plannerId,
            Source = source.Clone(),
            Position = position,
            HistoryLength = length,
            IsDecoded = decoded,
            TotalPoints = total,
            Ranks = new ReadOnlyDictionary<int, int>(ranks),
            Steps = steps.AsReadOnly(),
            Issues = issues.AsReadOnly(),
        };
    }

    static int? Integer(JsonElement value, string key) =>
        value.ValueKind == JsonValueKind.Object
        && value.TryGetProperty(key, out var field)
        && field.ValueKind == JsonValueKind.Number
        && field.TryGetInt32(out int number)
            ? number
            : null;

    static IReadOnlyList<JsonElement> Slots(JsonElement profile, string key, List<string> issues)
    {
        var values = new List<JsonElement>();
        if (profile.ValueKind == JsonValueKind.Object && profile.TryGetProperty(key, out var value))
        {
            if (value.ValueKind != JsonValueKind.Array)
                issues.Add(key + ": expected an array.");
            else
                foreach (var slot in value.EnumerateArray())
                {
                    values.Add(slot.Clone());
                    if (
                        slot.ValueKind != JsonValueKind.String
                        && slot.ValueKind != JsonValueKind.Null
                    )
                        issues.Add(
                            key
                                + "["
                                + (values.Count - 1)
                                + "]: expected an ability ID or empty slot."
                        );
                }
        }
        return values.AsReadOnly();
    }
}
