using System.Collections.Generic;
using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter;
using UnityEngine;
using UnityEngine.UI;

namespace LastEpoch_Hud.Scripts.Mods.Items.CustomItems.Headhunter;

/// <summary>Owns the bar's own canvas: shows, places and hides the icon slots.</summary>
internal static class HeadhunterBuffBarView
{
    private const float EntrySize = 60f;
    private static readonly List<HeadhunterBarSlot> _slots = new();
    private static GameObject _root;
    private static RectTransform _panel;
    private static Canvas _canvas;
    private static Canvas _matchedSource;
    private static UnityEngine.Camera _matchedCamera;
    private static HeadhunterBarPlacement _placement;
    private static bool _visible;

    public static void Show(IReadOnlyList<HeadhunterBarEntry> entries)
    {
        if (entries.Count == 0 || !EnsureCreated())
        {
            Hide();
            return;
        }

        for (int i = 0; i < entries.Count; i++)
        {
            SlotAt(i).Show(entries[i]);
        }

        for (int i = entries.Count; i < _slots.Count; i++)
        {
            _slots[i].Hide();
        }

        _root.SetActive(true);
        Place();
        _visible = true;
    }

    public static void Hide()
    {
        if (!_visible)
        {
            return;
        }

        _visible = false;
        if (!_root.IsNullOrDestroyed())
        {
            _root.SetActive(false);
        }
    }

    private static bool EnsureCreated()
    {
        if (!_root.IsNullOrDestroyed())
        {
            return true;
        }

        if (!HeadhunterBuffBarAssets.TryLoad())
        {
            return false;
        }

        _slots.Clear();
        _placement = default;
        _matchedSource = null;
        _root = Object.Instantiate(HeadhunterBuffBarAssets.BarPrefab);
        _root.name = "HeadhunterBuffBar";
        Object.DontDestroyOnLoad(_root);
        ConfigureCanvas();
        ConfigurePanel();
        return true;
    }

    private static void ConfigureCanvas()
    {
        _root.transform.localScale = Vector3.one;
        _canvas = _root.GetComponent<Canvas>();
        GraphicRaycaster raycaster = _root.GetComponent<GraphicRaycaster>();
        if (!raycaster.IsNullOrDestroyed())
        {
            raycaster.enabled = false;
        }
    }

    private static void ConfigurePanel()
    {
        GameObject panelObject = Functions.GetChild(_root, "Panel");
        _panel = panelObject.GetComponent<RectTransform>();
        _panel.anchorMin = new Vector2(0.5f, 0.5f);
        _panel.anchorMax = new Vector2(0.5f, 0.5f);
        _panel.pivot = new Vector2(0.5f, 0f);
        _panel.sizeDelta = new Vector2(20f, 64f);
        GridLayoutGroup grid = panelObject.GetComponent<GridLayoutGroup>();
        grid.constraint = GridLayoutGroup.Constraint.FixedRowCount;
        grid.constraintCount = 1;
        grid.childAlignment = TextAnchor.LowerCenter;
        grid.cellSize = new Vector2(EntrySize, EntrySize);
        grid.spacing = new Vector2(4f, 0f);
    }

    private static HeadhunterBarSlot SlotAt(int index)
    {
        while (_slots.Count <= index)
        {
            GameObject entry = Object.Instantiate(
                HeadhunterBuffBarAssets.EntryPrefab,
                _panel.transform
            );
            _slots.Add(new HeadhunterBarSlot(entry));
        }

        return _slots[index];
    }

    private static void Place()
    {
        SkillBarBounds bounds = HeadhunterSkillBarLocator.Read(out Canvas source);
        MatchCanvas(source);
        if (
            !HeadhunterBarLayout.TryPlace(
                bounds,
                EntrySize,
                _canvas.scaleFactor,
                out HeadhunterBarPlacement next
            )
        )
        {
            return;
        }

        next = next with { ScreenW = Screen.width, ScreenH = Screen.height };
        if (!HeadhunterBarLayout.ShouldMove(_placement, next, EntrySize))
        {
            return;
        }

        Move(next);
    }

    private static void Move(HeadhunterBarPlacement placement)
    {
        _placement = placement;
        (float x, float y) = HeadhunterBarLayout.ToLocal(
            placement,
            placement.ScreenW,
            placement.ScreenH,
            _canvas.scaleFactor
        );
        _panel.anchoredPosition = new Vector2(x, y);
        _panel.localScale = new Vector3(placement.Scale, placement.Scale, 1f);
    }

    private static void MatchCanvas(Canvas source)
    {
        if (source.IsNullOrDestroyed())
        {
            ApplyOverlayDefaults();
            return;
        }

        if (source == _matchedSource && source.worldCamera == _matchedCamera)
        {
            return;
        }

        _matchedSource = source;
        _matchedCamera = source.worldCamera;
        _canvas.renderMode = source.renderMode;
        _canvas.worldCamera = source.worldCamera;
        _canvas.planeDistance = source.planeDistance;
        _canvas.sortingLayerID = source.sortingLayerID;
        _canvas.sortingOrder = source.sortingOrder - 1;
    }

    private static void ApplyOverlayDefaults()
    {
        if (_matchedSource == null && _canvas.sortingOrder == -1)
        {
            return;
        }

        _matchedSource = null;
        _matchedCamera = null;
        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = -1;
    }
}
