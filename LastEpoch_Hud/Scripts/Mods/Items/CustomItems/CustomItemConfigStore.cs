using System;
using System.IO;

namespace LastEpoch_Hud.Scripts.Mods.Items.CustomItems;

/// <summary>One config file under the mod folder's CustomItems directory. Never throws on IO errors.</summary>
internal sealed class CustomItemConfigStore
{
    private static readonly string _folder = Path.Combine(
        Directory.GetCurrentDirectory(),
        "Mods",
        Main.mod_name,
        "CustomItems"
    );

    public CustomItemConfigStore(string fileName)
    {
        FilePath = Path.Combine(_folder, fileName);
    }

    public string FilePath { get; }

    public bool Exists()
    {
        return File.Exists(FilePath);
    }

    public void Write(string text)
    {
        try
        {
            Directory.CreateDirectory(_folder);
            File.WriteAllText(FilePath, text);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            Main.logger_instance?.Warning("Could not write " + FilePath + ": " + ex.Message);
        }
    }

    public string Read()
    {
        try
        {
            return File.ReadAllText(FilePath);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            Main.logger_instance?.Warning("Could not read " + FilePath + ": " + ex.Message);
            return null;
        }
    }
}
