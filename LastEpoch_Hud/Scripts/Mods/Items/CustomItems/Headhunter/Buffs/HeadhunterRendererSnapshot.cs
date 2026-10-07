using System.Collections.Generic;
using Il2Cpp;
using UnityEngine;

namespace LastEpoch_Hud.Scripts.Mods.Items.CustomItems.Headhunter.Buffs;

/// <summary>Captures the player renderers' look and puts it back.</summary>
internal sealed class HeadhunterRendererSnapshot
{
    private readonly List<Renderer> _renderers = new();
    private readonly List<MaterialPropertyBlock> _blocks = new();
    private readonly List<Material[]> _materials = new();

    public int Count => _renderers.Count;

    public static HeadhunterRendererSnapshot Capture(RendererManager manager)
    {
        var snapshot = new HeadhunterRendererSnapshot();
        foreach (Renderer renderer in manager.renderers)
        {
            snapshot.Add(renderer);
        }

        return snapshot;
    }

    /// <summary>First live material captured, or null when there is none.</summary>
    public Material FirstMaterial()
    {
        foreach (Material[] materials in _materials)
        {
            if (materials == null || materials.Length == 0)
            {
                continue;
            }

            if (!materials[0].IsNullOrDestroyed())
            {
                return materials[0];
            }
        }

        return null;
    }

    public void Restore()
    {
        for (int i = 0; i < _renderers.Count; i++)
        {
            Renderer renderer = _renderers[i];
            if (renderer.IsNullOrDestroyed())
            {
                continue;
            }

            renderer.SetPropertyBlock(_blocks[i]);
            renderer.sharedMaterials = _materials[i];
        }
    }

    private void Add(Renderer renderer)
    {
        if (renderer.IsNullOrDestroyed())
        {
            return;
        }

        _renderers.Add(renderer);
        _blocks.Add(CopyBlock(renderer));
        _materials.Add(renderer.sharedMaterials);
    }

    private static MaterialPropertyBlock CopyBlock(Renderer renderer)
    {
        if (!renderer.HasPropertyBlock())
        {
            return null;
        }

        var block = new MaterialPropertyBlock();
        renderer.GetPropertyBlock(block);
        return block;
    }
}
