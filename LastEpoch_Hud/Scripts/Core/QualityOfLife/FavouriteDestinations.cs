using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;

namespace LastEpoch_Hud.Scripts.Core.QualityOfLife;

public sealed class FavouriteDestinations
{
    public const int Capacity = 8;
    readonly List<string> scenes = new();
    public IReadOnlyList<string> Scenes => scenes.AsReadOnly();

    public bool Add(string scene)
    {
        if (!Valid(scene) || scenes.Count >= Capacity || scenes.Contains(scene))
            return false;
        scenes.Add(scene);
        return true;
    }

    public bool Remove(string scene) => scenes.Remove(scene);

    static bool Valid(string scene)
    {
        if (string.IsNullOrWhiteSpace(scene) || scene.Length > 160 || scene != scene.Trim())
            return false;
        foreach (char c in scene)
            if (!char.IsLetterOrDigit(c) && c != '_' && c != '-' && c != ' ')
                return false;
        return true;
    }

    public string ToJson() => new JObject { ["Scenes"] = new JArray(scenes) }.ToString();

    public static FavouriteDestinations FromJson(string json)
    {
        var root = JObject.Parse(json);
        if (root["Scenes"] is not JArray array)
            throw new FormatException("Favourite destinations must contain a Scenes array.");
        var result = new FavouriteDestinations();
        foreach (var value in array)
            if (value.Type == JTokenType.String)
                result.Add(value.Value<string>());
        return result;
    }

    // Write beside the destination and replace it only after the full JSON is flushed.
    public void Save(string path)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path)));
        string temporary = path + ".tmp";
        try
        {
            using (var stream = new FileStream(temporary, FileMode.Create, FileAccess.Write))
            using (var writer = new StreamWriter(stream))
            {
                writer.Write(ToJson());
                writer.Flush();
                stream.Flush(true);
            }
            File.Move(temporary, path, true);
        }
        finally
        {
            if (File.Exists(temporary))
                File.Delete(temporary);
        }
    }
}
