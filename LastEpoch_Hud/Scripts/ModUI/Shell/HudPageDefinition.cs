using System;
using UnityEngine;

namespace LastEpoch_Hud.Scripts.ModUI.Shell;

// A page's navigation metadata and runtime lifecycle live together here. Adding a
// page should require one entry, not another branch in HudLayout and a separate
// search-routing table.
internal sealed class HudPageDefinition
{
    private readonly Action<GameObject, GameObject, Font> build;
    private readonly Action show;
    private readonly Action hide;
    private readonly Action refresh;

    public readonly string Id;
    public readonly string Label;
    public readonly string SearchRootName;

    public HudPageDefinition(
        string id,
        string label,
        string searchRootName,
        Action<GameObject, GameObject, Font> build,
        Action show,
        Action hide,
        Action refresh = null
    )
    {
        Id = id;
        Label = label;
        SearchRootName = searchRootName;
        this.build = build;
        this.show = show;
        this.hide = hide;
        this.refresh = refresh;
    }

    public void Build(GameObject parent, GameObject hud, Font font) =>
        build?.Invoke(parent, hud, font);

    public void Show() => show?.Invoke();

    public void Hide() => hide?.Invoke();

    public void Refresh() => refresh?.Invoke();
}
