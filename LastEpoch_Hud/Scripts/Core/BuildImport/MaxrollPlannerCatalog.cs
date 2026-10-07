using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace LastEpoch_Hud.Scripts.Core.BuildImport;

// Display metadata from Maxroll's public planner. These are not native game IDs.
public sealed class MaxrollPlannerCatalog
{
    public IReadOnlyDictionary<string, MaxrollPlannerTree> Trees { get; private set; }
    public IReadOnlyDictionary<int, MaxrollPlannerClass> Classes { get; private set; }
    public IReadOnlyDictionary<string, string> AbilityNames { get; private set; }

    public string AbilityName(string id) =>
        id != null && AbilityNames.TryGetValue(id, out var name) ? name : null;

    public IReadOnlyList<MaxrollTreeView> Views(MaxrollTreePreview preview, bool passives)
    {
        var views = new List<MaxrollTreeView>();
        if (preview == null)
            return views.AsReadOnly();
        if (passives)
        {
            if (
                preview.Passives != null
                && preview.ClassId.HasValue
                && Classes.TryGetValue(preview.ClassId.Value, out var character)
                && Trees.TryGetValue(character.TreeId, out var definition)
            )
                for (int mastery = 0; mastery < character.MasteryNames.Count; mastery++)
                    views.Add(
                        new MaxrollTreeView(
                            preview.Passives,
                            definition,
                            character.MasteryNames[mastery],
                            mastery
                        )
                    );
        }
        else
        {
            var ordered = new List<MaxrollTreeSnapshot>();
            foreach (var slot in preview.SpecializedSkills)
                if (slot.ValueKind == JsonValueKind.String)
                    foreach (var snapshot in preview.Skills)
                        if (
                            Trees.TryGetValue(snapshot.PlannerId, out var tree)
                            && tree.AbilityId == slot.GetString()
                            && !ordered.Contains(snapshot)
                        )
                            ordered.Add(snapshot);
            ordered.AddRange(preview.Skills.Where(tree => !ordered.Contains(tree)));
            foreach (var snapshot in ordered)
                if (Trees.TryGetValue(snapshot.PlannerId, out var definition))
                    views.Add(new MaxrollTreeView(snapshot, definition, definition.Name, null));
        }
        return views.AsReadOnly();
    }

    public static MaxrollPlannerCatalog Parse(string json)
    {
        using var document = JsonDocument.Parse(json);
        var root = document.RootElement;
        var names = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var entry in root.GetProperty("abilities").EnumerateObject())
        {
            string name = Text(entry.Value, "abilityName");
            if (!string.IsNullOrWhiteSpace(name))
                names.Add(entry.Name, name);
        }
        var classes = new Dictionary<int, MaxrollPlannerClass>();
        foreach (var entry in root.GetProperty("classes").EnumerateArray())
        {
            int id = Integer(entry, "classID");
            var masteries = entry
                .GetProperty("masteries")
                .EnumerateArray()
                .Select(m => Text(m, "name"))
                .ToArray();
            if (masteries.Length == 0 || masteries.Any(string.IsNullOrWhiteSpace))
                throw new FormatException("The planner's class names are incomplete.");
            classes.Add(
                id,
                new MaxrollPlannerClass
                {
                    TreeId = Text(entry, "treeID"),
                    MasteryNames = Array.AsReadOnly(masteries),
                }
            );
        }
        var trees = new Dictionary<string, MaxrollPlannerTree>(StringComparer.Ordinal);
        foreach (var entry in root.GetProperty("skillTrees").EnumerateObject())
        {
            string ability = Text(entry.Value, "ability");
            string name =
                ability != null && names.TryGetValue(ability, out var title) ? title : null;
            var nodes = new Dictionary<int, MaxrollPlannerNode>();
            foreach (var pair in entry.Value.GetProperty("nodes").EnumerateObject())
            {
                int id = int.Parse(pair.Name, NumberStyles.None, CultureInfo.InvariantCulture);
                if (id < 0)
                    throw new FormatException("Invalid planner node ID.");
                var node = pair.Value;
                var transform = node.GetProperty("transform");
                int max = Integer(node, "maxPoints");
                string nodeName = Text(node, "nodeName");
                if (max < 0 || string.IsNullOrWhiteSpace(nodeName))
                    throw new FormatException("The planner's node data is incomplete.");
                var requirements = node.GetProperty("requirements")
                    .EnumerateArray()
                    .Select(r => new MaxrollNodeRequirement
                    {
                        NodeId = Integer(r, "node"),
                        Points = Integer(r, "requirement"),
                    })
                    .ToArray();
                var stats = new List<string>();
                if (node.TryGetProperty("stats", out var values))
                    foreach (var stat in values.EnumerateArray())
                    {
                        string statName = Text(stat, "statName");
                        string value = Text(stat, "value");
                        if (!string.IsNullOrEmpty(statName))
                            stats.Add(value + " " + statName);
                    }
                nodes.Add(
                    id,
                    new MaxrollPlannerNode
                    {
                        Id = id,
                        Name = nodeName,
                        MaximumPoints = max,
                        Mastery = Integer(node, "mastery"),
                        MasteryRequirement = Integer(node, "masteryRequirement"),
                        X = Coordinate(transform, "x"),
                        Y = Coordinate(transform, "y"),
                        Description = Text(node, "description"),
                        ExtraText = Text(node, "altText"),
                        PointBonusDescription = Text(node, "pointBonusDescription"),
                        Stats = stats.AsReadOnly(),
                        Requirements = Array.AsReadOnly(requirements),
                    }
                );
            }
            trees.Add(
                entry.Name,
                new MaxrollPlannerTree
                {
                    Id = entry.Name,
                    Name = name,
                    AbilityId = ability,
                    Nodes = new ReadOnlyDictionary<int, MaxrollPlannerNode>(nodes),
                }
            );
        }
        if (trees.Count == 0 || classes.Count == 0)
            throw new FormatException("The planner catalog is empty.");
        return new MaxrollPlannerCatalog
        {
            Trees = new ReadOnlyDictionary<string, MaxrollPlannerTree>(trees),
            Classes = new ReadOnlyDictionary<int, MaxrollPlannerClass>(classes),
            AbilityNames = new ReadOnlyDictionary<string, string>(names),
        };
    }

    static int Integer(JsonElement value, string key) =>
        value.TryGetProperty(key, out var field) ? field.GetInt32() : 0;

    static float Coordinate(JsonElement value, string key)
    {
        float result = value.TryGetProperty(key, out var field) ? field.GetSingle() : 0;
        if (!float.IsFinite(result))
            throw new FormatException("A planner node has a non-finite position.");
        return result;
    }

    static string Text(JsonElement value, string key) =>
        value.TryGetProperty(key, out var field) && field.ValueKind == JsonValueKind.String
            ? PlainText(field.GetString())
            : null;

    static string PlainText(string text) =>
        WebUtility.HtmlDecode(
            Regex.Replace(
                Regex.Replace(text ?? "", @"<br\s*/?>", "\n", RegexOptions.IgnoreCase),
                @"<[^>]*>",
                ""
            )
        );
}

public sealed class MaxrollPlannerClass
{
    public string TreeId { get; internal set; }
    public IReadOnlyList<string> MasteryNames { get; internal set; }
}

public sealed class MaxrollPlannerTree
{
    public string Id { get; internal set; }
    public string Name { get; internal set; }
    public string AbilityId { get; internal set; }
    public IReadOnlyDictionary<int, MaxrollPlannerNode> Nodes { get; internal set; }
}

public sealed class MaxrollPlannerNode
{
    public int Id { get; internal set; }
    public string Name { get; internal set; }
    public int MaximumPoints { get; internal set; }
    public int Mastery { get; internal set; }
    public int MasteryRequirement { get; internal set; }
    public float X { get; internal set; }
    public float Y { get; internal set; }
    public string Description { get; internal set; }
    public string ExtraText { get; internal set; }
    public string PointBonusDescription { get; internal set; }
    public IReadOnlyList<string> Stats { get; internal set; }
    public IReadOnlyList<MaxrollNodeRequirement> Requirements { get; internal set; }
}

public sealed class MaxrollNodeRequirement
{
    public int NodeId { get; internal set; }
    public int Points { get; internal set; }
}

public sealed class MaxrollTreeView
{
    public MaxrollTreeSnapshot Snapshot { get; }
    public MaxrollPlannerTree Definition { get; }
    public string Name { get; }
    public int? Mastery { get; }
    public IReadOnlyList<MaxrollPlannerNode> Nodes { get; }
    public long TotalPoints => Nodes.Sum(n => (long)Rank(n.Id));
    public bool HasUnknownRanks =>
        Snapshot.Ranks.Any(r => r.Value > 0 && !Definition.Nodes.ContainsKey(r.Key));

    public MaxrollTreeView(
        MaxrollTreeSnapshot snapshot,
        MaxrollPlannerTree definition,
        string name,
        int? mastery
    )
    {
        Snapshot = snapshot;
        Definition = definition;
        Name = name;
        Mastery = mastery;
        Nodes = definition
            .Nodes.Values.Where(n => !mastery.HasValue || n.Mastery == mastery.Value)
            .ToList()
            .AsReadOnly();
    }

    public int Rank(int id) => Snapshot.Ranks.TryGetValue(id, out int value) ? value : 0;
}
