using System;
using Il2CppInterop.Runtime;
using LastEpoch_Hud.Scripts.Core.CustomItems;
using UnityEngine;
using UnityEngine.UI;

namespace LastEpoch_Hud.Scripts.Mods.Items;

/// <summary>Icons of the custom uniques, loaded once from the HUD bundle.</summary>
public static class CustomItemIcons
{
    private static readonly Sprite[] _sprites = new Sprite[CustomUniqueSpecs.All.Count];
    private static bool _attempted;

    /// <summary>Loads every icon in one pass over the bundle, once the bundle exists.</summary>
    public static void LoadOnce()
    {
        if (_attempted || Hud_Manager.asset_bundle.IsNullOrDestroyed())
        {
            return;
        }

        _attempted = true;
        LoadAll();
    }

    /// <summary>Puts the item's icon on the image; does nothing when either is missing.</summary>
    public static void Apply(Image image, int index)
    {
        if (image.IsNullOrDestroyed() || _sprites[index].IsNullOrDestroyed())
        {
            return;
        }

        // A native override sprite can mask Image.sprite entirely.
        image.overrideSprite = null;
        image.sprite = _sprites[index];
    }

    private static void LoadAll()
    {
        foreach (string name in AssetNames())
        {
            LoadIcon(name);
        }

        for (int i = 0; i < _sprites.Length; i++)
        {
            LogUnavailable(i);
        }
    }

    private static string[] AssetNames()
    {
        try
        {
            return Hud_Manager.asset_bundle.GetAllAssetNames();
        }
        catch (Exception ex)
        {
            ErrorLog.Report(ex, "Custom item icons");
            return Array.Empty<string>();
        }
    }

    private static void LoadIcon(string name)
    {
        int index = CustomUniqueLookup.IconIndexOf(name);
        if (index < 0 || !_sprites[index].IsNullOrDestroyed())
        {
            return;
        }

        try
        {
            _sprites[index] = LoadSprite(name);
            LogLoaded(index, name);
        }
        catch (Exception ex)
        {
            ErrorLog.Report(ex, $"{CustomUniqueSpecs.All[index].Name} icon");
        }
    }

    private static Sprite LoadSprite(string name)
    {
        // Load the texture explicitly and create a runtime sprite;
        // this avoids ambiguous PNG subassets and imported atlas bindings.
        Texture2D texture = Hud_Manager
            .asset_bundle.LoadAsset(name, Il2CppType.Of<Texture2D>())
            ?.TryCast<Texture2D>();
        if (!texture.IsNullOrDestroyed())
        {
            return Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f)
            );
        }

        return Hud_Manager.asset_bundle.LoadAsset(name, Il2CppType.Of<Sprite>())?.TryCast<Sprite>();
    }

    private static void LogLoaded(int index, string name)
    {
        Sprite sprite = _sprites[index];
        if (sprite.IsNullOrDestroyed())
        {
            return;
        }

        string path = name.Replace('\\', '/').ToLowerInvariant();
        Main.logger_instance?.Msg(
            $"{CustomUniqueSpecs.All[index].Name} icon loaded: {path} ({sprite.rect.width}x{sprite.rect.height})"
        );
    }

    private static void LogUnavailable(int index)
    {
        if (!_sprites[index].IsNullOrDestroyed())
        {
            return;
        }

        Main.logger_instance?.Warning($"{CustomUniqueSpecs.All[index].Name} icon unavailable");
    }
}
