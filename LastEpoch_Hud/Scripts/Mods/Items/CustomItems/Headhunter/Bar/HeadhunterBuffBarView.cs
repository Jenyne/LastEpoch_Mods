using System.Collections.Generic;
using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Bar;
using LastEpoch_Hud.Scripts.Core.CustomItems.Headhunter.Config;
using UnityEngine;
using UnityEngine.UI;

namespace LastEpoch_Hud.Scripts.Mods.Items.CustomItems.Headhunter.Bar;

/// <summary>Owns the bar's own canvas: shows, places and hides the icon slots.</summary>
internal static class HeadhunterBuffBarView
{
    private const float PausedAlpha = 0.45f;
    private static readonly List<HeadhunterBarSlot> _slots = new();
    private static GameObject _root;
    private static RectTransform _panel;
    private static CanvasGroup _panelGroup;
    private static Canvas _canvas;
    private static Canvas _matchedSource;
    private static UnityEngine.Camera _matchedCamera;
    private static HeadhunterBarPlacement _placement;
    private static bool _visible;
    private static HeadhunterBarGrid _grid;
    private static float _canvasScale = 1f;
    private static HeadhunterBarTooltip _tooltip;

    public static bool IsVisible => _visible;

    public static int LayoutVersion { get; private set; }

    public static int IndexAt(float x, float y)
    {
        if (!_visible)
        {
            return -1;
        }

        return HeadhunterBarGeometry.IndexAt(_placement, _canvasScale, _grid, x, y);
    }

    public static int RowAt(int index)
    {
        if (index < 0 || index >= _grid.Count)
        {
            return -1;
        }

        return _slots[index].Row;
    }

    public static int StacksAt(int index)
    {
        if (index < 0 || index >= _grid.Count)
        {
            return 0;
        }

        return _slots[index].Stacks;
    }

    public static void ShowTooltip(int index, string text)
    {
        if (index < 0 || index >= _grid.Count || !EnsureTooltip())
        {
            return;
        }

        (float sx, float sy) = HeadhunterBarGeometry.TooltipAnchor(
            _placement,
            _canvasScale,
            _grid,
            index
        );
        (float x, float y) = HeadhunterBarLayout.ToLocal(
            sx,
            sy,
            _placement.ScreenW,
            _placement.ScreenH,
            _canvasScale
        );
        _tooltip.Show(text, x, y, _placement.Scale);
    }

    public static void HideTooltip()
    {
        if (_tooltip == null || _root.IsNullOrDestroyed())
        {
            return;
        }

        _tooltip.Hide();
    }

    public static void Show(
        IReadOnlyList<HeadhunterBarEntry> entries,
        HeadhunterBarSettings settings,
        bool paused
    )
    {
        if (entries.Count == 0 || !EnsureCreated())
        {
            Hide();
            return;
        }

        _panelGroup.alpha = paused ? PausedAlpha : 1f;
        for (int i = 0; i < entries.Count; i++)
        {
            SlotAt(i).Show(entries[i]);
        }

        for (int i = entries.Count; i < _slots.Count; i++)
        {
            _slots[i].Hide();
        }

        _root.SetActive(true);
        var grid = new HeadhunterBarGrid(entries.Count, settings.PerRow);
        if (grid != _grid)
        {
            ApplyLayout(grid);
        }

        Place(settings);
        _visible = true;
    }

    public static void Hide()
    {
        if (!_visible)
        {
            return;
        }

        _visible = false;
        _grid = default;
        HideTooltip();
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
        _grid = default;
        _tooltip = null;
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
        _panel.sizeDelta = new Vector2(20f, HeadhunterBarLayout.EntrySize + 4f);
        panelObject.GetComponent<GridLayoutGroup>().enabled = false;
        _panelGroup = EnsureGroup(panelObject);
    }

    private static CanvasGroup EnsureGroup(GameObject panelObject)
    {
        CanvasGroup group = panelObject.GetComponent<CanvasGroup>();
        if (!group.IsNullOrDestroyed())
        {
            return group;
        }

        return panelObject.AddComponent<CanvasGroup>();
    }

    private static void ApplyLayout(HeadhunterBarGrid grid)
    {
        _grid = grid;
        _panel.sizeDelta = new Vector2(grid.Width, grid.Height);
        for (int i = 0; i < grid.Count; i++)
        {
            _slots[i].PlaceAt(grid.CellX(i), grid.CellBottom(i));
        }

        LayoutVersion++;
    }

    private static bool EnsureTooltip()
    {
        if (_root.IsNullOrDestroyed())
        {
            return false;
        }

        if (_tooltip != null)
        {
            return true;
        }

        if (_slots.Count == 0)
        {
            return false;
        }

        _tooltip = new HeadhunterBarTooltip(_root.transform, _slots[0].TextFont);
        return true;
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

    private static void Place(HeadhunterBarSettings settings)
    {
        SkillBarBounds bounds = HeadhunterSkillBarLocator.Read(out Canvas source);
        MatchCanvas(source);
        if (
            !HeadhunterBarLayout.TryPlace(
                bounds,
                settings,
                HeadhunterBarLayout.EntrySize,
                _canvas.scaleFactor,
                out HeadhunterBarPlacement next
            )
        )
        {
            return;
        }

        next = next with { ScreenW = Screen.width, ScreenH = Screen.height };
        if (!HeadhunterBarLayout.ShouldMove(_placement, next, HeadhunterBarLayout.EntrySize))
        {
            return;
        }

        Move(next);
    }

    private static void Move(HeadhunterBarPlacement placement)
    {
        _placement = placement;
        _canvasScale = _canvas.scaleFactor;
        LayoutVersion++;
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
