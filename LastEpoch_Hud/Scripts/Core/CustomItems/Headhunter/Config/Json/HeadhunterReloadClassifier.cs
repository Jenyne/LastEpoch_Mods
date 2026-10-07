using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Config.Json;

/// <summary>Decides if a config edit touched only the bar layout.</summary>
public static class HeadhunterReloadClassifier
{
    public static HeadhunterReloadKind Classify(string previousJson, string nextJson)
    {
        if (!TryParseWithoutBar(previousJson, out JObject previous))
        {
            return HeadhunterReloadKind.Full;
        }
        if (!TryParseWithoutBar(nextJson, out JObject next))
        {
            return HeadhunterReloadKind.Full;
        }
        if (!JToken.DeepEquals(previous, next))
        {
            return HeadhunterReloadKind.Full;
        }

        return HeadhunterReloadKind.LayoutOnly;
    }

    private static bool TryParseWithoutBar(string json, out JObject root)
    {
        root = null;
        if (string.IsNullOrWhiteSpace(json))
        {
            return false;
        }
        try
        {
            root = JToken.Parse(json) as JObject;
        }
        catch (JsonReaderException)
        {
            return false;
        }
        if (root == null)
        {
            return false;
        }

        root.Remove(HeadhunterConfigKeys.Bar);
        return true;
    }
}
