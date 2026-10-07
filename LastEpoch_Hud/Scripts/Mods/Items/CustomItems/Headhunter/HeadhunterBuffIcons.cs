using System.Collections.Generic;
using Il2Cpp;
using Il2CppLE.AssetBundles;
using Il2CppLE.AssetManagement;
using LastEpoch_Hud.Scripts.Core.CustomItems;
using UnityEngine;
using UnityEngine.UI;

namespace LastEpoch_Hud.Scripts.Mods.Items.CustomItems.Headhunter;

/// <summary>Icon per buffed stat: bundle icon, then game stat icon, then the item icon.</summary>
internal static class HeadhunterBuffIcons
{
    private static readonly Dictionary<int, Sprite> _sprites = new();
    private static readonly Dictionary<int, LoadRef<Sprite>> _loadRefs = new();

    public static Sprite For(int statId)
    {
        if (_sprites.TryGetValue(statId, out Sprite cached) && !cached.IsNullOrDestroyed())
        {
            return cached;
        }

        Sprite sprite = Resolve(statId);
        _sprites[statId] = sprite;
        return sprite;
    }

    private static Sprite Resolve(int statId)
    {
        Sprite sprite = FromBundle(statId);
        if (!sprite.IsNullOrDestroyed())
        {
            return sprite;
        }

        sprite = FromGameStatIcons(statId);
        if (!sprite.IsNullOrDestroyed())
        {
            return sprite;
        }

        int index = CustomUniqueLookup.IndexOf(CustomUniqueSpecs.Headhunter.UniqueId);
        return CustomItemIcons.Get(index);
    }

    private static Sprite FromBundle(int statId)
    {
        GameObject prefab = HeadhunterBuffBarAssets.BarPrefab;
        GameObject child = Functions.GetChild(prefab, ((SP)statId).ToString(), false);
        if (child.IsNullOrDestroyed())
        {
            return null;
        }

        Image image = child.GetComponent<Image>();
        if (image.IsNullOrDestroyed() || image.sprite.IsNullOrDestroyed())
        {
            return null;
        }

        return Protect(image.sprite);
    }

    private static Sprite FromGameStatIcons(int statId)
    {
        if (!GlobalAssets.NodeTooltipIconListAvailable)
        {
            return null;
        }

        if (!NodeTooltipIconList.hasSpriteForPropertyAndTags((ushort)statId, AT.None))
        {
            return null;
        }

        SoftRef<Sprite> softRef = NodeTooltipIconList.getSprite((ushort)statId, AT.None);
        if (softRef == null || !softRef)
        {
            return null;
        }

        LoadRef<Sprite> loadRef = SoftRefExtensions.CreateLoadRef(softRef, "LastEpoch_Hud", 0);
        if (loadRef == null)
        {
            return null;
        }

        loadRef.BlockForLoad();
        _loadRefs[statId] = loadRef;
        return loadRef.AssetOrNull;
    }

    private static Sprite Protect(Sprite sprite)
    {
        sprite.hideFlags |= HideFlags.DontUnloadUnusedAsset;
        if (!sprite.texture.IsNullOrDestroyed())
        {
            sprite.texture.hideFlags |= HideFlags.DontUnloadUnusedAsset;
        }

        return sprite;
    }
}
