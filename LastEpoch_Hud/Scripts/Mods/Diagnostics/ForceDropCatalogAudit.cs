using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Il2Cpp;
using LastEpoch_Hud.Scripts.ModUI;
using Newtonsoft.Json;
using UnityEngine;

namespace LastEpoch_Hud.Scripts.Mods.Diagnostics;

// Opt-in inspection only: no ItemData construction, packing, corruption, or drops.
internal static class ForceDropCatalogAudit
{
    static bool attempted;
    static readonly string[] members =
    {
        "affixId",
        "affixName",
        "affixDisplayName",
        "type",
        "specialAffixType",
        "rollsOn",
        "classSpecificity",
        "uniqueId",
        "levelRequirement",
        "weighting",
        "group",
        "canRollOn",
        "tiers",
        "t6Compatibility",
        "standardAffixEffectModifier",
        "maximumAffixEffectModifierForT6",
        "convertOnIncompatibleItemType",
        "affixIDToConvertTo",
        "property",
        "tags",
        "specialTag",
        "extraTag",
        "modifierType",
        "setProperty",
        "affixProperties",
        "unifiedType",
        "setRequirement",
        "hideInTooltip",
        "minRoll",
        "maxRoll",
        "extraRolls",
        "isMultiAffix",
        "baseTypeID",
        "subTypeID",
        "baseType",
        "subTypes",
        "name",
        "displayName",
        "classRequirement",
        "levelRequirement",
        "equipmentType",
        "subItems",
        "uniqueID",
        "isSetItem",
        "setID",
        "isPrimordialItem",
        "legendaryType",
        "hideFromPlayers",
        "canDropRandomly",
        "canDropAsLegendary",
        "rerollChance",
        "dropsSpecificLegendaryAffixes",
        "droppableLegendaryAffixCount",
        "droppableLegendaryAffixes",
        "excludeSpecificAffixesFromPrefixSuffixLimits",
        "convertPotentialToLegendaryAffixes",
        "additionalRandomLegendaryAffixes",
        "isPreCorrupted",
        "preCorruptPositiveChance",
        "validPreCorrupts",
        "category",
        "cannotCombineWithType",
        "positiveChance",
        "positiveOutcomes",
        "negativeOutcomes",
        "corruptionCategoryConfig",
        "corruptionEquipmentTypeConfig",
        "corruptionOutcome",
        "weight",
        "maximumChanceType",
        "maximumChance",
        "tierWeights",
        "replacesAffix",
        "minAffixesForReplace",
        "minRemovableTiers",
        "maxRemovableTiers",
        "normalizedRollValue",
        "canSetRollOnAffixes",
        "canSetRollOnUniqueMod",
        "canSetRollOnImplicits",
        "randomizeSubType",
        "randomizeAffixes",
        "rarityToSet",
        "extraCorruptions",
        "extraCorruptionOutcomes",
        "nemesisIterations",
        "setName",
        "mods",
        "itemsInSet",
        "stat",
        "value",
        "minValue",
        "maxValue",
        "baseTypes",
        "itemTypes",
        "classes",
        "specificSubTypes",
        "excludedSubTypes",
    };
    static List<string> issues;
    static int remainingNodes;

    public static void Tick()
    {
        if (
            attempted
            || !ModSettings.ForceDropAudit.DumpOnNextLaunch.Value
            || !Scenes.IsGameScene()
        )
            return;
        try
        {
            var affixes = AffixList.get();
            var items = ItemList.get();
            if (
                affixes.IsNullOrDestroyed()
                || items.IsNullOrDestroyed()
                || UniqueList.instance.IsNullOrDestroyed()
            )
                return;
            if (affixes.AllAffixes.IsNullOrDestroyed() || affixes.AllAffixes.Count == 0)
                return;
            attempted = true;
            issues = new List<string>();
            remainingNodes = 200000;
            var definitions = new Dictionary<int, Dictionary<string, object>>();
            AddAffixes(definitions, affixes.singleAffixes, "singleAffixes");
            AddAffixes(definitions, affixes.multiAffixes, "multiAffixes");
            AddAffixes(definitions, affixes.AllAffixes, "AllAffixes");
            var snapshot = new Dictionary<string, object>
            {
                ["schemaVersion"] = 1,
                ["capturedUtc"] = DateTime.UtcNow.ToString("O"),
                ["gameLocale"] = NativeItemNames.Locale,
                ["purpose"] =
                    "Read-only game definitions; not a legal-item whitelist or persistence verification.",
                ["affixes"] = definitions,
                ["equipment"] = Read(items.EquippableItems, "equipment", 0),
                ["nonEquipment"] = Read(items.nonEquippableItems, "nonEquipment", 0),
                ["uniques"] = Read(UniqueList.instance.uniques, "uniques", 0),
                ["corruptionConfig"] = ReadMember(
                    items,
                    "corruptionOutcomeConfig",
                    "corruptionConfig",
                    0
                ),
                ["capabilities"] = Capabilities(),
                ["issues"] = issues,
            };
            string folder = Path.Combine(Application.dataPath, "..", "Mods", Main.mod_name);
            Directory.CreateDirectory(folder);
            string path = Path.Combine(
                folder,
                "ForceDropCatalog_" + DateTime.UtcNow.ToString("yyyyMMdd_HHmmss") + ".json"
            );
            File.WriteAllText(path, JsonConvert.SerializeObject(snapshot, Formatting.Indented));
            ModSettings.ForceDropAudit.DumpOnNextLaunch.Set(false);
            Main.logger_instance?.Msg(
                "Force Drop catalog audit saved: "
                    + path
                    + " (affixes="
                    + definitions.Count
                    + ", issues="
                    + issues.Count
                    + ")"
            );
        }
        catch (Exception ex)
        {
            attempted = true;
            Main.logger_instance?.Error("Force Drop catalog audit failed: " + ex.Message);
        }
    }

    static void AddAffixes(
        Dictionary<int, Dictionary<string, object>> records,
        object collection,
        string source
    )
    {
        int count = Count(collection);
        if (count < 0)
        {
            issues.Add(source + ": collection unavailable");
            return;
        }
        for (int i = 0; i < Math.Min(count, 10000); i++)
        {
            try
            {
                if (At(collection, i) is not AffixList.Affix affix || affix.IsNullOrDestroyed())
                {
                    issues.Add(source + "[" + i + "]: null/unrecognized definition");
                    continue;
                }
                if (records.TryGetValue(affix.affixId, out var existing))
                {
                    ((List<string>)existing["catalogSources"]).Add(source);
                    continue;
                }
                var record = ReadObject(affix, source + "[" + i + "]", 0);
                record["catalogSources"] = new List<string> { source };
                records.Add(affix.affixId, record);
            }
            catch (Exception ex)
            {
                issues.Add(source + "[" + i + "]: " + ex.GetBaseException().Message);
            }
        }
        if (count > 10000)
            issues.Add(source + ": truncated at 10000 entries");
    }

    static object ReadMember(object instance, string name, string path, int depth)
    {
        try
        {
            var property = instance.GetType().GetProperty(name);
            if (property != null && property.CanRead && property.GetIndexParameters().Length == 0)
                return Read(property.GetValue(instance), path, depth);
            var field = instance.GetType().GetField(name);
            if (field != null)
                return Read(field.GetValue(instance), path, depth);
            issues.Add(path + ": member unavailable");
        }
        catch (Exception ex)
        {
            issues.Add(path + ": " + ex.GetBaseException().Message);
        }
        return null;
    }

    static object Read(object value, string path, int depth)
    {
        if (value == null)
            return null;
        var type = value.GetType();
        if (value is string || type.IsPrimitive || value is decimal)
            return value;
        if (type.IsEnum)
            return new Dictionary<string, object>
            {
                ["name"] = value.ToString(),
                ["value"] = Convert.ToInt64(value),
            };
        if (--remainingNodes < 0 || depth > 6)
        {
            issues.Add(path + ": capture limit reached");
            return null;
        }
        int count = Count(value);
        if (count >= 0)
        {
            var result = new List<object>();
            for (int i = 0; i < Math.Min(count, 10000) && remainingNodes > 0; i++)
            {
                try
                {
                    result.Add(Read(At(value, i), path + "[" + i + "]", depth + 1));
                }
                catch (Exception ex)
                {
                    issues.Add(path + "[" + i + "]: " + ex.GetBaseException().Message);
                    result.Add(null);
                }
            }
            if (result.Count != count)
                issues.Add(
                    path
                        + ": truncated collection (captured="
                        + result.Count
                        + ", total="
                        + count
                        + ")"
                );
            return result;
        }
        return ReadObject(value, path, depth);
    }

    static Dictionary<string, object> ReadObject(object value, string path, int depth)
    {
        var result = new Dictionary<string, object> { ["wrapperType"] = value.GetType().FullName };
        var available = new List<string>();
        foreach (var property in value.GetType().GetProperties())
            if (property.CanRead && property.GetIndexParameters().Length == 0)
                available.Add(property.Name);
        result["availableProperties"] = available;
        foreach (string name in members)
        {
            if (result.ContainsKey(name))
                continue;
            if (value.GetType().GetProperty(name) != null || value.GetType().GetField(name) != null)
                result[name] = ReadMember(value, name, path + "." + name, depth + 1);
        }
        return result;
    }

    static int Count(object collection)
    {
        if (collection == null)
            return -1;
        if (collection is Array array)
            return array.Length;
        var type = collection.GetType();
        var count = type.GetProperty("Count") ?? type.GetProperty("Length");
        return count != null && type.GetProperty("Item", new[] { typeof(int) }) != null
            ? Convert.ToInt32(count.GetValue(collection))
            : -1;
    }

    static object At(object collection, int index) =>
        collection is Array array
            ? array.GetValue(index)
            : collection
                .GetType()
                .GetProperty("Item", new[] { typeof(int) })
                .GetValue(collection, new object[] { index });

    static Dictionary<string, object> Capabilities()
    {
        var result = new Dictionary<string, object>();
        foreach (
            var type in new[]
            {
                typeof(ItemData),
                typeof(ItemDataUnpacked),
                typeof(ItemAffix),
                typeof(AffixList.Affix),
            }
        )
        {
            var signatures = new List<string>();
            foreach (
                var method in type.GetMethods(
                    BindingFlags.Public
                        | BindingFlags.Instance
                        | BindingFlags.Static
                        | BindingFlags.DeclaredOnly
                )
            )
                if (
                    method.Name.Contains("Corrupt")
                    || method.Name.Contains("RollOn")
                    || method.Name.Contains("Tier")
                    || method.Name.Contains("Seal")
                )
                    signatures.Add(method.ToString());
            result[type.FullName] = signatures;
        }
        return result;
    }
}
