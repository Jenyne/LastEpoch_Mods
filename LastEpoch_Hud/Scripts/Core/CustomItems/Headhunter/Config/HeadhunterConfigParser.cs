using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Config;

/// <summary>Reads config JSON field by field. Bad values fall back to defaults and are reported, never thrown.</summary>
public static class HeadhunterConfigParser
{
    public static HeadhunterConfigParseResult Parse(string json, IReadOnlySet<string> knownStats)
    {
        var problems = new List<HeadhunterConfigProblem>();
        JObject root = ParseRoot(json, problems);
        if (root == null)
        {
            return Result(HeadhunterConfigDefaults.Config, problems, false);
        }

        var config = new HeadhunterConfig
        {
            Version = ReadVersion(root, problems),
            DurationSeconds = ReadDuration(root, problems),
            MaxStacks = ReadMaxStacks(root, problems),
            Triggers = ReadTriggers(root, problems),
            Stats = ReadStats(root, knownStats, problems),
            AffixMap = HeadhunterAffixMapParser.Read(root, problems),
        };
        return Result(config, problems, true);
    }

    internal static bool TryGetInt(JToken token, out int value)
    {
        value = 0;
        if (token is not JValue { Value: long number })
        {
            return false;
        }
        if (number < int.MinValue || number > int.MaxValue)
        {
            return false;
        }
        value = (int)number;
        return true;
    }

    internal static void Report(List<HeadhunterConfigProblem> problems, string path, string message)
    {
        problems.Add(new HeadhunterConfigProblem(path, message));
    }

    private static HeadhunterConfigParseResult Result(
        HeadhunterConfig config,
        List<HeadhunterConfigProblem> problems,
        bool readable
    )
    {
        return new HeadhunterConfigParseResult
        {
            Config = config,
            Problems = problems,
            IsReadable = readable,
        };
    }

    private static JObject ParseRoot(string json, List<HeadhunterConfigProblem> problems)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            Report(problems, "", "File is empty.");
            return null;
        }
        try
        {
            var token = JToken.Parse(json);
            if (token is JObject root)
            {
                return root;
            }
            Report(problems, "", "Root must be a JSON object.");
            return null;
        }
        catch (JsonReaderException ex)
        {
            Report(problems, "", "Invalid JSON: " + ex.Message);
            return null;
        }
    }

    private static int ReadVersion(JObject root, List<HeadhunterConfigProblem> problems)
    {
        JToken token = root[HeadhunterConfigKeys.Version];
        if (token == null)
        {
            return HeadhunterConfigDefaults.CurrentVersion;
        }
        if (!TryGetInt(token, out int version))
        {
            Report(problems, HeadhunterConfigKeys.Version, "Must be a whole number.");
            return HeadhunterConfigDefaults.CurrentVersion;
        }
        if (version != HeadhunterConfigDefaults.CurrentVersion)
        {
            Report(problems, HeadhunterConfigKeys.Version, "Unsupported version " + version + ".");
        }
        return version;
    }

    private static float ReadDuration(JObject root, List<HeadhunterConfigProblem> problems)
    {
        JToken token = root[HeadhunterConfigKeys.DurationSeconds];
        if (token == null)
        {
            return HeadhunterConfigDefaults.DurationSeconds;
        }
        if (TryReadNumber(token, out float seconds) && seconds > 0f)
        {
            return seconds;
        }
        Report(problems, HeadhunterConfigKeys.DurationSeconds, "Must be a number above 0.");
        return HeadhunterConfigDefaults.DurationSeconds;
    }

    private static int ReadMaxStacks(JObject root, List<HeadhunterConfigProblem> problems)
    {
        JToken token = root[HeadhunterConfigKeys.MaxStacks];
        if (token == null)
        {
            return HeadhunterConfigDefaults.MaxStacks;
        }
        if (TryGetInt(token, out int stacks) && stacks >= 1)
        {
            return stacks;
        }
        Report(problems, HeadhunterConfigKeys.MaxStacks, "Must be a whole number of at least 1.");
        return HeadhunterConfigDefaults.MaxStacks;
    }

    private static HeadhunterTriggers ReadTriggers(
        JObject root,
        List<HeadhunterConfigProblem> problems
    )
    {
        JToken token = root[HeadhunterConfigKeys.Triggers];
        if (token == null)
        {
            return HeadhunterConfigDefaults.Triggers;
        }
        if (token is not JObject triggers)
        {
            Report(problems, HeadhunterConfigKeys.Triggers, "Must be an object.");
            return HeadhunterConfigDefaults.Triggers;
        }
        return new HeadhunterTriggers(
            ReadTrigger(
                triggers,
                HeadhunterConfigKeys.Rare,
                HeadhunterConfigDefaults.Triggers.Rare,
                problems
            ),
            ReadTrigger(
                triggers,
                HeadhunterConfigKeys.Boss,
                HeadhunterConfigDefaults.Triggers.Boss,
                problems
            ),
            ReadTrigger(
                triggers,
                HeadhunterConfigKeys.Miniboss,
                HeadhunterConfigDefaults.Triggers.Miniboss,
                problems
            ),
            ReadTrigger(
                triggers,
                HeadhunterConfigKeys.MinionKills,
                HeadhunterConfigDefaults.Triggers.MinionKills,
                problems
            ),
            ReadTrigger(
                triggers,
                HeadhunterConfigKeys.Magic,
                HeadhunterConfigDefaults.Triggers.Magic,
                problems
            )
        );
    }

    private static bool ReadTrigger(
        JObject triggers,
        string name,
        bool fallback,
        List<HeadhunterConfigProblem> problems
    )
    {
        JToken token = triggers[name];
        if (token == null)
        {
            return fallback;
        }
        if (TryReadBool(token, out bool value))
        {
            return value;
        }
        Report(problems, HeadhunterConfigKeys.Triggers + "." + name, "Must be true or false.");
        return fallback;
    }

    private static IReadOnlyList<HeadhunterStatEntry> ReadStats(
        JObject root,
        IReadOnlySet<string> knownStats,
        List<HeadhunterConfigProblem> problems
    )
    {
        JToken token = root[HeadhunterConfigKeys.Stats];
        if (token == null)
        {
            return HeadhunterConfigDefaults.Stats;
        }
        if (token is not JArray array)
        {
            Report(problems, HeadhunterConfigKeys.Stats, "Must be a list.");
            return HeadhunterConfigDefaults.Stats;
        }

        var stats = new List<HeadhunterStatEntry>();
        var seen = new HashSet<(string Stat, string Tag)>();
        for (int index = 0; index < array.Count; index++)
        {
            string path = HeadhunterConfigKeys.Stats + "[" + index + "]";
            if (
                !TryReadEntry(
                    array[index],
                    path,
                    knownStats,
                    problems,
                    out HeadhunterStatEntry entry
                )
            )
            {
                continue;
            }
            if (!seen.Add((entry.Stat, entry.Tag)))
            {
                Report(problems, path + "." + HeadhunterConfigKeys.Stat, "Duplicate stat.");
                continue;
            }
            stats.Add(entry);
        }
        return stats;
    }

    private static bool TryReadEntry(
        JToken token,
        string path,
        IReadOnlySet<string> knownStats,
        List<HeadhunterConfigProblem> problems,
        out HeadhunterStatEntry entry
    )
    {
        entry = default;
        if (token is not JObject obj)
        {
            Report(problems, path, "Must be an object.");
            return false;
        }
        JToken statToken = obj[HeadhunterConfigKeys.Stat];
        if (statToken == null)
        {
            Report(problems, path, "Missing stat name.");
            return false;
        }
        if (statToken is not JValue { Value: string stat } || !knownStats.Contains(stat))
        {
            Report(problems, path + "." + HeadhunterConfigKeys.Stat, "Unknown stat.");
            return false;
        }
        if (
            !TryReadField(
                obj,
                HeadhunterConfigKeys.Added,
                HeadhunterConfigDefaults.EntryAdded,
                path,
                problems,
                out float added
            )
        )
        {
            return false;
        }
        if (
            !TryReadField(
                obj,
                HeadhunterConfigKeys.Increased,
                HeadhunterConfigDefaults.EntryIncreased,
                path,
                problems,
                out float increased
            )
        )
        {
            return false;
        }
        if (!TryReadEnabled(obj, path, problems, out bool enabled))
        {
            return false;
        }
        if (!TryReadTag(obj, path, problems, out string tag))
        {
            return false;
        }
        entry = new HeadhunterStatEntry(stat, added, increased, enabled, tag);
        return true;
    }

    private static bool TryReadTag(
        JObject obj,
        string path,
        List<HeadhunterConfigProblem> problems,
        out string tag
    )
    {
        tag = null;
        JToken token = obj[HeadhunterConfigKeys.Tag];
        if (token == null || token.Type == JTokenType.Null)
        {
            return true;
        }
        if (token is JValue { Value: string text } && text.Length > 0)
        {
            tag = text;
            return true;
        }
        Report(problems, path + "." + HeadhunterConfigKeys.Tag, "Must be a non-empty text.");
        return false;
    }

    private static bool TryReadField(
        JObject obj,
        string name,
        float fallback,
        string path,
        List<HeadhunterConfigProblem> problems,
        out float value
    )
    {
        value = fallback;
        JToken token = obj[name];
        if (token == null)
        {
            return true;
        }
        if (TryReadNumber(token, out value))
        {
            return true;
        }
        Report(problems, path + "." + name, "Must be a finite number.");
        return false;
    }

    private static bool TryReadEnabled(
        JObject obj,
        string path,
        List<HeadhunterConfigProblem> problems,
        out bool value
    )
    {
        value = HeadhunterConfigDefaults.EntryEnabled;
        JToken token = obj[HeadhunterConfigKeys.Enabled];
        if (token == null)
        {
            return true;
        }
        if (TryReadBool(token, out value))
        {
            return true;
        }
        Report(problems, path + "." + HeadhunterConfigKeys.Enabled, "Must be true or false.");
        return false;
    }

    private static bool TryReadNumber(JToken token, out float value)
    {
        value = 0f;
        if (token is not JValue { Value: long or double } number)
        {
            return false;
        }
        value = Convert.ToSingle(number.Value, CultureInfo.InvariantCulture);
        return float.IsFinite(value);
    }

    private static bool TryReadBool(JToken token, out bool value)
    {
        value = false;
        if (token is not JValue { Value: bool flag })
        {
            return false;
        }
        value = flag;
        return true;
    }
}
