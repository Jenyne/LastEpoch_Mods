using System.Collections.Generic;
using Il2Cpp;
using MelonLoader;
using Newtonsoft.Json;
using UnityEngine;

namespace LastEpoch_Hud.Scripts.Mods.Items.CustomItems.Headhunter;

public static class HeadhunterConfig
{
    public static TextAsset Json;
    internal static List<HeadhunterBuffEntry> BuffConfig = new();
    internal static List<HeadhunterBuffEntry> BuffConfigBackup = new();
    private static readonly string _filename = "hh_buffs.json";

    public static bool LoadConfig()
    {
        if (Save_Manager.instance.IsNullOrDestroyed())
        {
            return false;
        }
        BuffConfig = new List<HeadhunterBuffEntry>();
        BuffConfigBackup = new List<HeadhunterBuffEntry>();
        if (!System.IO.File.Exists(Save_Manager.instance.path + _filename))
        {
            DefaultConfig();
            return false;
        }
        try
        {
            BuffConfig = JsonConvert.DeserializeObject<List<HeadhunterBuffEntry>>(
                System.IO.File.ReadAllText(Save_Manager.instance.path + _filename)
            );
            BuffConfigBackup = BuffConfig;
            HeadhunterBuffs.GenerateBuffsList();
        }
        catch
        {
            DefaultConfig();
        }

        return true;
    }

    public static void SaveConfig()
    {
        BuffConfigBackup = BuffConfig; //Use to check if buffs changed
        string jsonString = JsonConvert.SerializeObject(BuffConfig, Formatting.Indented);
        if (!System.IO.Directory.Exists(Save_Manager.instance.path))
        {
            System.IO.Directory.CreateDirectory(Save_Manager.instance.path);
        }
        if (System.IO.File.Exists(Save_Manager.instance.path + _filename))
        {
            System.IO.File.Delete(Save_Manager.instance.path + _filename);
        }
        System.IO.File.WriteAllText(Save_Manager.instance.path + _filename, jsonString);
        HeadhunterBuffs.GenerateBuffsList();
    }

    private static void DefaultConfig()
    {
        BuffConfig = JsonConvert.DeserializeObject<List<HeadhunterBuffEntry>>(Json.text);
        BuffConfigBackup = BuffConfig;
        SaveConfig();
    }
}
