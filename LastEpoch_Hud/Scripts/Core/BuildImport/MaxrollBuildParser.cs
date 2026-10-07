using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace LastEpoch_Hud.Scripts.Core.BuildImport;

public static class MaxrollBuildParser
{
    public const int MaximumBytes = 4 * 1024 * 1024;
    static readonly string[] arraySections = { "idols", "blessings", "weaverItems" };
    static readonly string[] affixArrays = { "affixes", "corruptedAffixes", "priorityAffixes" };
    static readonly string[] singleAffixes = { "sealedAffix", "primordialAffix", "corruptedAffix" };

    public static MaxrollBuild Parse(string json, MaxrollLink link = null)
    {
        using var document = ReadJson(json);
        var outer = document.RootElement;
        RequireObject(outer, "Maxroll response");
        if (outer.TryGetProperty("error", out _))
            throw new FormatException("Maxroll returned an error instead of a build.");
        if (outer.TryGetProperty("profile", out var profile))
        {
            RequireObject(profile, "Maxroll profile");
            outer = profile;
        }
        if (
            outer.TryGetProperty("game", out var game)
            && (game.ValueKind != JsonValueKind.String || game.GetString() != "le")
        )
            throw new FormatException("The response is not a Last Epoch build.");
        if (
            link != null
            && outer.TryGetProperty("id", out var responseId)
            && (
                responseId.ValueKind != JsonValueKind.String
                || responseId.GetString() != link.BuildId
            )
        )
            throw new FormatException("Maxroll returned a different build ID than requested.");
        string name = Text(outer, "name", "Maxroll Build");
        JsonElement data;
        if (outer.TryGetProperty("data", out var encoded))
        {
            if (encoded.ValueKind == JsonValueKind.String)
            {
                using var inner = ReadJson(encoded.GetString());
                data = inner.RootElement.Clone();
            }
            else
                data = encoded.Clone();
        }
        else
            data = outer.Clone();
        RequireObject(data, "Build data");
        var shared = new Dictionary<string, JsonElement>(StringComparer.Ordinal);
        bool fullBuild =
            data.TryGetProperty("profiles", out var profiles)
            || data.TryGetProperty("embeds", out _);
        if (fullBuild && data.TryGetProperty("items", out var items))
        {
            RequireObject(items, "Shared items");
            foreach (var pair in items.EnumerateObject())
                shared.Add(pair.Name, pair.Value);
        }
        var variants = new List<MaxrollVariant>();
        if (data.TryGetProperty("profiles", out profiles))
        {
            RequireArray(profiles, "Profiles");
            int index = 0;
            foreach (var value in profiles.EnumerateArray())
                variants.Add(Variant(value, "profile", index++, null, shared));
        }
        if (data.TryGetProperty("embeds", out var embeds))
        {
            RequireArray(embeds, "Embeds");
            int index = 0;
            foreach (var value in embeds.EnumerateArray())
            {
                if (Text(value, "type", "") == "equipment")
                    variants.Add(
                        Variant(value, "equipment-embed", index, Integer(value, "id"), shared)
                    );
                index++;
            }
        }
        if (!fullBuild)
        {
            if (
                !new[] { "items", "idols", "blessings", "passives", "skillTrees", "weaver" }.Any(
                    k => data.TryGetProperty(k, out _)
                )
            )
                throw new FormatException("No planner build or clipboard export was found.");
            variants.Add(Variant(data, "clipboard", 0, null, shared));
        }
        if (variants.Count == 0)
            throw new FormatException("This planner contains no profiles or equipment embeds.");
        var result = new MaxrollBuild
        {
            Name = name,
            Link = link,
            ResponseJson = json,
            Data = data,
            Variants = variants.AsReadOnly(),
        };
        Select(result, embeds);
        return result;
    }

    static void Select(MaxrollBuild build, JsonElement embeds)
    {
        string fragment = build.Link?.Fragment ?? "";
        bool hasEmbeds = embeds.ValueKind == JsonValueKind.Array && embeds.GetArrayLength() > 0;
        int? target = null;
        if (fragment.Length > 0)
        {
            bool numeric = int.TryParse(
                fragment,
                NumberStyles.None,
                CultureInfo.InvariantCulture,
                out int number
            );
            for (int i = 0; i < build.Variants.Count; i++)
            {
                var v = build.Variants[i];
                if (
                    hasEmbeds
                        ? numeric && v.Kind == "equipment-embed" && v.EmbedId == number
                        : v.Kind == "profile"
                            && (numeric ? v.SourceIndex == number - 1 : v.Name == fragment)
                )
                {
                    target = i;
                    break;
                }
            }
            if (!target.HasValue)
                build.SelectionIssue =
                    "The link selects an unavailable or non-equipment variant. Choose a listed gear variant explicitly.";
        }
        else
        {
            int? activeEmbed = Integer(build.Data, "activeEmbed");
            string kind = activeEmbed >= 0 ? "equipment-embed" : "profile";
            int source =
                activeEmbed >= 0 ? activeEmbed.Value : Integer(build.Data, "activeProfile") ?? 0;
            for (int i = 0; i < build.Variants.Count; i++)
                if (build.Variants[i].Kind == kind && build.Variants[i].SourceIndex == source)
                    target = i;
            if (
                !target.HasValue
                && build.Variants.Count == 1
                && build.Variants[0].Kind == "clipboard"
            )
                target = 0;
            if (!target.HasValue)
                build.SelectionIssue =
                    "The saved active variant is unavailable or is not equipment. Choose a listed gear variant explicitly.";
        }
        build.SelectedVariantIndex = target;
    }

    static MaxrollVariant Variant(
        JsonElement value,
        string kind,
        int index,
        int? id,
        Dictionary<string, JsonElement> shared
    )
    {
        RequireObject(value, "Gear variant");
        var issues = new List<string>();
        var placements = new List<MaxrollPlacement>();
        if (value.TryGetProperty("items", out var items))
        {
            RequireObject(items, "Equipment slots");
            foreach (var pair in items.EnumerateObject())
                placements.Add(Placement(pair.Value, "equipment", pair.Name, null, shared, issues));
        }
        foreach (string section in arraySections)
            if (value.TryGetProperty(section, out var array))
            {
                RequireArray(array, section);
                int slot = 0;
                foreach (var item in array.EnumerateArray())
                {
                    placements.Add(
                        Placement(
                            item,
                            section,
                            slot.ToString(CultureInfo.InvariantCulture),
                            slot,
                            shared,
                            issues
                        )
                    );
                    slot++;
                }
            }
        return new MaxrollVariant
        {
            Name = Text(
                value,
                "name",
                kind == "clipboard" ? "Clipboard" : "Variant " + (index + 1)
            ),
            Kind = kind,
            SourceIndex = index,
            EmbedId = id,
            Data = value.Clone(),
            Placements = placements.AsReadOnly(),
            Issues = issues.AsReadOnly(),
        };
    }

    static MaxrollPlacement Placement(
        JsonElement value,
        string section,
        string slot,
        int? grid,
        Dictionary<string, JsonElement> shared,
        List<string> issues
    )
    {
        var p = new MaxrollPlacement
        {
            Section = section,
            Slot = slot,
            GridIndex = grid,
            SourceValue = value.Clone(),
        };
        string path = section + "." + slot;
        if (value.ValueKind == JsonValueKind.Null)
        {
            p.IsEmpty = true;
            return p;
        }
        if (value.ValueKind == JsonValueKind.Number || value.ValueKind == JsonValueKind.String)
        {
            string key =
                value.ValueKind == JsonValueKind.String ? value.GetString() : value.GetRawText();
            p.Reference = key;
            if (!shared.TryGetValue(key, out value) || value.ValueKind == JsonValueKind.Null)
            {
                issues.Add(path + ": unresolved item reference " + key);
                return p;
            }
        }
        if (value.ValueKind != JsonValueKind.Object)
        {
            issues.Add(path + ": expected an item definition");
            return p;
        }
        p.Item = value.Clone();
        if (
            value.TryGetProperty("template", out var template)
            && template.ValueKind == JsonValueKind.True
        )
            issues.Add(path + ": item template needs explicit game-aware roll selection");
        CheckInteger(value, "itemType", path, issues, required: true);
        CheckInteger(value, "subType", path, issues, required: true);
        CheckInteger(value, "uniqueID", path, issues);
        foreach (string key in new[] { "implicits", "uniqueRolls" })
            if (value.TryGetProperty(key, out var rolls))
                CheckRolls(rolls, path + "." + key, issues);
        foreach (string key in affixArrays)
            if (value.TryGetProperty(key, out var array))
            {
                if (array.ValueKind != JsonValueKind.Array)
                    issues.Add(path + "." + key + ": expected an array");
                else
                    foreach (var affix in array.EnumerateArray())
                        CheckAffix(affix, path + "." + key, issues);
            }
        foreach (string key in singleAffixes)
            if (value.TryGetProperty(key, out var affix) && affix.ValueKind != JsonValueKind.Null)
                CheckAffix(affix, path + "." + key, issues);
        return p;
    }

    static void CheckAffix(JsonElement affix, string path, List<string> issues)
    {
        if (affix.ValueKind != JsonValueKind.Object)
        {
            issues.Add(path + ": expected an affix definition");
            return;
        }
        CheckInteger(affix, "id", path, issues, required: true);
        if (affix.TryGetProperty("tier", out _))
        {
            CheckInteger(affix, "tier", path, issues, required: true, minimum: 1);
            if (affix.TryGetProperty("roll", out var roll))
                CheckRoll(roll, path + ".roll", issues);
            else
                issues.Add(
                    path + ": roll absent; maximum is a planner default, not a captured value"
                );
        }
        else if (
            affix.TryGetProperty("values", out var values)
            && values.ValueKind == JsonValueKind.Array
        )
            issues.Add(path + ": explicit stat values require game-aware tier/roll conversion");
        else
            issues.Add(path + ": no tier or explicit stat values");
    }

    static void CheckRolls(JsonElement array, string path, List<string> issues)
    {
        if (array.ValueKind != JsonValueKind.Array)
        {
            issues.Add(path + ": expected a roll array");
            return;
        }
        foreach (var roll in array.EnumerateArray())
            CheckRoll(roll, path, issues);
    }

    static void CheckRoll(JsonElement value, string path, List<string> issues)
    {
        if (
            value.ValueKind != JsonValueKind.Number
            || !value.TryGetDouble(out double n)
            || !double.IsFinite(n)
            || n < 0
            || n > 1
        )
            issues.Add(path + ": normalized roll must be between 0 and 1");
    }

    static void CheckInteger(
        JsonElement value,
        string key,
        string path,
        List<string> issues,
        bool required = false,
        int minimum = 0
    )
    {
        if (!value.TryGetProperty(key, out var number))
        {
            if (required)
                issues.Add(path + ": missing " + key);
        }
        else if (
            number.ValueKind != JsonValueKind.Number
            || !number.TryGetInt32(out int n)
            || n < minimum
        )
            issues.Add(path + "." + key + ": expected an integer >= " + minimum);
    }

    static JsonDocument ReadJson(string json)
    {
        if (string.IsNullOrWhiteSpace(json) || Encoding.UTF8.GetByteCount(json) > MaximumBytes)
            throw new FormatException("The build response is empty or exceeds the 4 MiB limit.");
        var doc = JsonDocument.Parse(json, new JsonDocumentOptions { MaxDepth = 64 });
        try
        {
            CheckDuplicateKeys(doc.RootElement);
        }
        catch
        {
            doc.Dispose();
            throw;
        }
        return doc;
    }

    static void CheckDuplicateKeys(JsonElement value)
    {
        if (value.ValueKind == JsonValueKind.Object)
        {
            var keys = new HashSet<string>(StringComparer.Ordinal);
            foreach (var pair in value.EnumerateObject())
            {
                if (!keys.Add(pair.Name))
                    throw new FormatException("Duplicate JSON property: " + pair.Name);
                CheckDuplicateKeys(pair.Value);
            }
        }
        else if (value.ValueKind == JsonValueKind.Array)
            foreach (var child in value.EnumerateArray())
                CheckDuplicateKeys(child);
    }

    static int? Integer(JsonElement value, string key) =>
        value.ValueKind == JsonValueKind.Object
        && value.TryGetProperty(key, out var n)
        && n.ValueKind == JsonValueKind.Number
        && n.TryGetInt32(out int number)
            ? number
            : null;

    static string Text(JsonElement value, string key, string fallback) =>
        value.ValueKind == JsonValueKind.Object
        && value.TryGetProperty(key, out var n)
        && n.ValueKind == JsonValueKind.String
            ? n.GetString()
            : fallback;

    static void RequireObject(JsonElement value, string path)
    {
        if (value.ValueKind != JsonValueKind.Object)
            throw new FormatException(path + " must be a JSON object.");
    }

    static void RequireArray(JsonElement value, string path)
    {
        if (value.ValueKind != JsonValueKind.Array)
            throw new FormatException(path + " must be a JSON array.");
    }
}
