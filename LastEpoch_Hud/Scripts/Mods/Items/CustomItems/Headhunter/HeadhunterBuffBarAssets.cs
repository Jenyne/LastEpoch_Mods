using System;
using Il2CppInterop.Runtime;
using UnityEngine;

namespace LastEpoch_Hud.Scripts.Mods.Items.CustomItems.Headhunter;

/// <summary>Loads the two buff bar prefabs from the HUD bundle once.</summary>
internal static class HeadhunterBuffBarAssets
{
    private const string BarSuffix = "headhunter/prefab/buffs.prefab";
    private const string EntrySuffix = "headhunter/prefab/buff.prefab";
    private static bool _attempted;

    public static GameObject BarPrefab { get; private set; }
    public static GameObject EntryPrefab { get; private set; }

    public static bool TryLoad()
    {
        if (!BarPrefab.IsNullOrDestroyed() && !EntryPrefab.IsNullOrDestroyed())
        {
            return true;
        }

        if (_attempted || Hud_Manager.asset_bundle.IsNullOrDestroyed())
        {
            return false;
        }

        _attempted = true;
        LoadAll();
        if (!BarPrefab.IsNullOrDestroyed() && !EntryPrefab.IsNullOrDestroyed())
        {
            return true;
        }

        Main.logger_instance?.Warning("Headhunter buff bar prefabs missing in the bundle");
        return false;
    }

    private static void LoadAll()
    {
        foreach (string name in Hud_Manager.asset_bundle.GetAllAssetNames())
        {
            string lower = name.Replace('\\', '/').ToLowerInvariant();
            if (lower.EndsWith(BarSuffix, StringComparison.Ordinal))
            {
                BarPrefab = Load(name);
            }

            if (lower.EndsWith(EntrySuffix, StringComparison.Ordinal))
            {
                EntryPrefab = Load(name);
            }
        }
    }

    private static GameObject Load(string name)
    {
        GameObject prefab = Hud_Manager
            .asset_bundle.LoadAsset(name, Il2CppType.Of<GameObject>())
            ?.TryCast<GameObject>();
        if (!prefab.IsNullOrDestroyed())
        {
            prefab.hideFlags |= HideFlags.DontUnloadUnusedAsset;
        }

        return prefab;
    }
}
