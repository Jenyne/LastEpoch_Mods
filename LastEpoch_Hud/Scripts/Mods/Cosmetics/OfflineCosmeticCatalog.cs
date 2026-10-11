using System;
using Il2CppCysharp.Threading.Tasks;
using Il2CppLE.AssetManagement;
using Il2CppLE.Services.Cosmetics;
using LastEpoch_Hud.Scripts.Core.Cosmetics;

namespace LastEpoch_Hud.Scripts.Mods.Cosmetics;

/// <summary>
/// Offline cosmetics catalog per registry instance. Cache policy lives in <see cref="CosmeticCatalogCache" />;
/// this class reads the registry and holds the Il2Cpp copies of the owned ids.
/// </summary>
internal static class OfflineCosmeticCatalog
{
    private const string ErrorContext = "Offline cosmetics catalog";

    private static readonly CosmeticCatalogCache _cache = new(BuildCatalog);

    private static CosmeticResources _registry;
    private static Il2CppSystem.Collections.Generic.List<string> _ownedIds;
    private static UniTask<Il2CppSystem.Collections.Generic.List<string>> _ownedTask;

    /// <summary>False when the catalog is not ready; the game's own answer then stands.</summary>
    public static bool TryIsOwned(string id, out bool owned)
    {
        owned = false;
        if (!TryGetCatalog(out CosmeticCatalog catalog))
        {
            return false;
        }
        owned = catalog.Contains(id);
        return true;
    }

    /// <summary>Shared list of owned ids, or null when not ready. Callers must not store or mutate it.</summary>
    public static Il2CppSystem.Collections.Generic.List<string> OwnedIds()
    {
        return TryGetCatalog(out CosmeticCatalog _) ? _ownedIds : null;
    }

    /// <summary>Shared task over the owned ids, or null when not ready.</summary>
    public static UniTask<Il2CppSystem.Collections.Generic.List<string>> OwnedIdsTask()
    {
        return TryGetCatalog(out CosmeticCatalog _) ? _ownedTask : null;
    }

    /// <summary>Allows one retry after a failure and drops the cache when the registry changed or is gone.</summary>
    public static void OnSceneLoaded()
    {
        bool available = GlobalAssets.CosmeticsAvailable;
        long key = available ? KeyOf(GlobalAssets.Cosmetics) : 0;
        _cache.OnSceneLoaded(key, available);
        if (_cache.HasCatalog)
        {
            return;
        }
        _registry = null;
    }

    private static bool TryGetCatalog(out CosmeticCatalog catalog)
    {
        catalog = null;
        if (!_registry.IsNullOrDestroyed())
        {
            return _cache.TryGet(KeyOf(_registry), true, out catalog);
        }
        if (_cache.Failed || !GlobalAssets.CosmeticsAvailable)
        {
            return false;
        }
        return _cache.TryGet(KeyOf(GlobalAssets.Cosmetics), false, out catalog);
    }

    private static long KeyOf(CosmeticResources registry)
    {
        return registry.IsNullOrDestroyed() ? 0 : registry.Pointer.ToInt64();
    }

    private static CosmeticCatalog BuildCatalog()
    {
        try
        {
            return ReadRegistry(GlobalAssets.Cosmetics);
        }
        catch (Exception ex)
        {
            ErrorLog.Report(ex, ErrorContext);
            return new CosmeticCatalog();
        }
    }

    private static CosmeticCatalog ReadRegistry(CosmeticResources registry)
    {
        if (registry.IsNullOrDestroyed())
        {
            return new CosmeticCatalog();
        }
        CosmeticCatalog catalog = CosmeticRegistryReader.Read(registry);
        if (catalog.KeptCount == 0)
        {
            ErrorLog.Report("Registry has no cosmetics", ErrorContext);
            return catalog;
        }
        var ids = new Il2CppSystem.Collections.Generic.List<string>();
        foreach (string id in catalog.KeptIds)
        {
            ids.Add(id);
        }
        _registry = registry;
        _ownedIds = ids;
        _ownedTask = new UniTask<Il2CppSystem.Collections.Generic.List<string>>(ids);
        Main.logger_instance?.Msg(catalog.SummaryLine());
        return catalog;
    }
}
