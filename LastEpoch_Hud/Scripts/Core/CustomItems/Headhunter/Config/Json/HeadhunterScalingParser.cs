using System.Collections.Generic;
using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Config.Defaults;
using Newtonsoft.Json.Linq;

namespace LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Config.Json;

/// <summary>Reads the scaling section. Bad values are reported and replaced by defaults.</summary>
internal static class HeadhunterScalingParser
{
    public static HeadhunterGrowthCurve ReadValueGrowth(
        JObject root,
        List<HeadhunterConfigProblem> problems
    )
    {
        HeadhunterGrowthCurve defaults = HeadhunterConfigDefaults.ValueGrowth;
        JObject scaling = ReadObject(root, HeadhunterConfigKeys.Scaling, "", problems);
        JObject values = ReadObject(
            scaling,
            HeadhunterConfigKeys.Values,
            HeadhunterConfigKeys.Scaling + ".",
            problems
        );
        if (values == null)
        {
            return defaults;
        }

        return new HeadhunterGrowthCurve(
            ReadPercent(values, HeadhunterConfigKeys.PerStack, defaults.PerStackPercent, problems),
            ReadPercent(values, HeadhunterConfigKeys.Cap, defaults.CapPercent, problems)
        );
    }

    private static JObject ReadObject(
        JObject parent,
        string key,
        string pathPrefix,
        List<HeadhunterConfigProblem> problems
    )
    {
        JToken token = parent?[key];
        if (token == null)
        {
            return null;
        }
        if (token is JObject section)
        {
            return section;
        }

        HeadhunterConfigParser.Report(
            problems,
            HeadhunterConfigProblemCode.NotObject,
            pathPrefix + key,
            "Must be an object."
        );
        return null;
    }

    private static float ReadPercent(
        JObject values,
        string key,
        float fallback,
        List<HeadhunterConfigProblem> problems
    )
    {
        JToken token = values[key];
        if (token == null)
        {
            return fallback;
        }
        if (HeadhunterConfigParser.TryReadNumber(token, out float percent) && percent >= 0f)
        {
            return percent;
        }

        HeadhunterConfigParser.Report(
            problems,
            HeadhunterConfigProblemCode.NotNonNegativeNumber,
            HeadhunterConfigKeys.Scaling + "." + HeadhunterConfigKeys.Values + "." + key,
            "Must be a number of at least 0."
        );
        return fallback;
    }
}
