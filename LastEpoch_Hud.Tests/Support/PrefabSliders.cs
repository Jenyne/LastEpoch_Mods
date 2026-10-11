using System.Globalization;
using LastEpoch_Hud.Scripts.Core.ModUI.Sliders;

namespace LastEpoch_Hud.Tests.Support;

/// <summary>Reads slider limits by GameObject name from a Unity YAML prefab.</summary>
internal static class PrefabSliders
{
    private const string DocumentStart = "--- !u!";
    private const string GameObjectHeader = "--- !u!1 &";

    public static IReadOnlyDictionary<string, SliderRange> Load(string prefabPath)
    {
        var names = new Dictionary<string, string>();
        var sliders = new List<(string ObjectId, SliderRange Range)>();
        foreach (List<string> document in SplitDocuments(File.ReadAllLines(prefabPath)))
        {
            ReadDocument(document, names, sliders);
        }

        return sliders
            .Where(slider => names.ContainsKey(slider.ObjectId))
            .GroupBy(slider => names[slider.ObjectId])
            .Where(group => group.Count() == 1)
            .ToDictionary(group => group.Key, group => group.Single().Range);
    }

    private static IEnumerable<List<string>> SplitDocuments(string[] lines)
    {
        var current = new List<string>();
        foreach (string line in lines)
        {
            if (line.StartsWith(DocumentStart, StringComparison.Ordinal) && current.Count > 0)
            {
                yield return current;
                current = new List<string>();
            }

            current.Add(line);
        }

        yield return current;
    }

    private static void ReadDocument(
        List<string> document,
        Dictionary<string, string> names,
        List<(string ObjectId, SliderRange Range)> sliders
    )
    {
        string header = document[0];
        if (header.StartsWith(GameObjectHeader, StringComparison.Ordinal))
        {
            names[header[GameObjectHeader.Length..].Trim()] = Field(document, "m_Name") ?? "";
            return;
        }

        string min = Field(document, "m_MinValue");
        string max = Field(document, "m_MaxValue");
        string whole = Field(document, "m_WholeNumbers");
        string owner = Field(document, "m_GameObject");
        if (min == null || max == null || whole == null || owner == null)
        {
            return;
        }

        string ownerId = owner.Replace("{fileID:", "").Replace("}", "").Trim();
        sliders.Add((ownerId, new SliderRange(Parse(min), Parse(max), whole == "1")));
    }

    private static string Field(List<string> document, string key)
    {
        string prefix = "  " + key + ":";
        string line = document.FirstOrDefault(l =>
            l.TrimEnd().StartsWith(prefix, StringComparison.Ordinal)
        );
        return line?[prefix.Length..].Trim();
    }

    private static float Parse(string value) => float.Parse(value, CultureInfo.InvariantCulture);
}
