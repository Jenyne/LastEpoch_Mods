using System.Collections.Generic;
using Il2Cpp;
using Il2CppLE.AssetBundles;
using Il2CppLE.AssetManagement;
using LastEpoch_Hud.Scripts.Core.CustomItems;
using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter;
using UnityEngine;
using UnityEngine.UI;

namespace LastEpoch_Hud.Scripts.Mods.Items.CustomItems.Headhunter;

/// <summary>Icon per buffed stat: bundle icon, then game stat icon, then the item icon.</summary>
internal static class HeadhunterBuffIcons
{
    private static readonly Dictionary<HeadhunterStatKey, Sprite> _sprites = new();
    private static readonly Dictionary<HeadhunterStatKey, LoadRef<Sprite>> _loadRefs = new();

    public static Sprite For(int statId, int tags)
    {
        var key = new HeadhunterStatKey(statId, tags);
        if (_sprites.TryGetValue(key, out Sprite cached) && !cached.IsNullOrDestroyed())
        {
            return cached;
        }

        Sprite sprite = Resolve(statId, tags);
        _sprites[key] = sprite;
        return sprite;
    }

    private static Sprite Resolve(int statId, int tags)
    {
        Sprite sprite = tags == 0 ? null : FromGameStatIcons(statId, tags);
        if (!sprite.IsNullOrDestroyed())
        {
            return sprite;
        }

        sprite = FromBundle(statId);
        if (!sprite.IsNullOrDestroyed())
        {
            return sprite;
        }

        sprite = FromGameStatIcons(statId, 0);
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

    private static Sprite FromGameStatIcons(int statId, int tags)
    {
        if (!GlobalAssets.NodeTooltipIconListAvailable)
        {
            return null;
        }

        if (!NodeTooltipIconList.hasSpriteForPropertyAndTags((ushort)statId, (AT)tags))
        {
            return null;
        }

        SoftRef<Sprite> softRef = NodeTooltipIconList.getSprite((ushort)statId, (AT)tags);
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
        _loadRefs[new HeadhunterStatKey(statId, tags)] = loadRef;
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
