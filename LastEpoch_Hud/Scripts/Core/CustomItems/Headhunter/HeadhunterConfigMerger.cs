using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter;

/// <summary>Adds default rows and fields newer than the file's defaults version, keeping everything else.</summary>
public static class HeadhunterConfigMerger
{
    public static HeadhunterMergeResult Merge(string json, HeadhunterMergeDefaults defaults)
    {
        JObject root = TryParseRoot(json);
        if (root == null)
        {
            return new HeadhunterMergeResult(json, false, 0);
        }
        if (HasUnmergeableStats(root))
        {
            return new HeadhunterMergeResult(json, false, 0);
        }
        int fileVersion = ReadStamp(root);
        if (fileVersion >= defaults.Version)
        {
            return new HeadhunterMergeResult(json, false, 0);
        }

        int added =
            AddStats(root, defaults.Stats, fileVersion)
            + AddFields(root, defaults.Fields, fileVersion);
        root[HeadhunterConfigKeys.DefaultsVersion] = defaults.Version;
        return new HeadhunterMergeResult(root.ToString(Formatting.Indented), true, added);
    }

    private static JObject TryParseRoot(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }
        try
        {
            return JToken.Parse(json) as JObject;
        }
        catch (JsonReaderException)
        {
            return null;
        }
    }

    private static int ReadStamp(JObject root)
    {
        return HeadhunterConfigParser.TryGetInt(
            root[HeadhunterConfigKeys.DefaultsVersion],
            out int version
        )
            ? version
            : HeadhunterConfigDefaults.UnstampedDefaultsVersion;
    }

    private static bool HasUnmergeableStats(JObject root)
    {
        JToken stats = root[HeadhunterConfigKeys.Stats];
        return stats != null && stats is not JArray;
    }

    private static int AddStats(
        JObject root,
        IReadOnlyList<HeadhunterVersionedStat> stats,
        int fileVersion
    )
    {
        if (root[HeadhunterConfigKeys.Stats] is not JArray rows)
        {
            return 0;
        }
        int added = 0;
        foreach (HeadhunterVersionedStat row in stats)
        {
            if (row.Since <= fileVersion || ContainsRow(rows, row.Entry))
            {
                continue;
            }
            rows.Add(HeadhunterConfigWriter.BuildStat(row.Entry));
            added++;
        }
        return added;
    }

    private static bool ContainsRow(JArray rows, HeadhunterStatEntry entry)
    {
        foreach (JToken row in rows)
        {
            if (row is not JObject obj)
            {
                continue;
            }
            if (obj[HeadhunterConfigKeys.Stat] is not JValue { Value: string name })
            {
                continue;
            }
            if (
                string.Equals(name, entry.Stat, StringComparison.Ordinal)
                && string.Equals(ReadTag(obj), entry.Tag, StringComparison.Ordinal)
            )
            {
                return true;
            }
        }
        return false;
    }

    private static string ReadTag(JObject row)
    {
        return row[HeadhunterConfigKeys.Tag] is JValue { Value: string tag } ? tag : null;
    }

    private static int AddFields(
        JObject root,
        IReadOnlyList<HeadhunterVersionedField> fields,
        int fileVersion
    )
    {
        int added = 0;
        foreach (HeadhunterVersionedField field in fields)
        {
            if (field.Since <= fileVersion)
            {
                continue;
            }
            JObject parent = ResolveParent(root, field.Parent);
            if (parent == null || parent.ContainsKey(field.Key))
            {
                continue;
            }
            parent[field.Key] = field.Value.DeepClone();
            added++;
        }
        return added;
    }

    private static JObject ResolveParent(JObject root, string parent)
    {
        return parent.Length == 0 ? root : root[parent] as JObject;
    }
}
