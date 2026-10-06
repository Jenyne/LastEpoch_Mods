using Il2Cpp;
using MelonLoader;
using UnityEngine;

namespace LastEpoch_Hud.Scripts.Mods.Items.CustomItems.Headhunter;

public static class HeadhunterAssets
{
    public static bool Loaded;
    private static bool _loading;
    private static bool _attempted;

    public static void Load()
    {
        if (Loaded || _attempted || Hud_Manager.asset_bundle.IsNullOrDestroyed() || _loading)
        {
            return;
        }
        _loading = true;
        _attempted = true;
        try
        {
            foreach (string name in Hud_Manager.asset_bundle.GetAllAssetNames())
            {
                if (IsBuffsJson(name))
                {
                    HeadhunterConfig.Json = Hud_Manager
                        .asset_bundle.LoadAsset(name)
                        .TryCast<TextAsset>();
                }
            }
            Loaded = !HeadhunterConfig.Json.IsNullOrDestroyed();
        }
        catch (System.Exception ex)
        {
            Main.logger_instance?.Error("Headhunter Asset Error: " + ex.Message);
        }
        _loading = false;
    }

    private static bool IsBuffsJson(string name)
    {
        return name.Replace("\\", "/").ToLowerInvariant().Contains("/headhunter/")
            && Functions.Check_Json(name)
            && name.Contains("hh_buffs")
            && HeadhunterConfig.Json.IsNullOrDestroyed();
    }
}
