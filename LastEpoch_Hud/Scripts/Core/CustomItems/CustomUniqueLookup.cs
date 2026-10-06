using System;

namespace LastEpoch_Hud.Scripts.Core.CustomItems;

/// <summary>Finds a custom unique from the keys the game hands to our patches.</summary>
public static class CustomUniqueLookup
{
    /// <summary>Index in <see cref="CustomUniqueSpecs.All"/> of the item with this unique id, -1 if none.</summary>
    public static int IndexOf(int uniqueId)
    {
        for (int i = 0; i < CustomUniqueSpecs.All.Count; i++)
        {
            if (CustomUniqueSpecs.All[i].UniqueId == uniqueId)
            {
                return i;
            }
        }
        return -1;
    }

    /// <summary>Index of the item whose icon asset ends the bundle asset name, -1 if none.</summary>
    public static int IconIndexOf(string assetName)
    {
        if (string.IsNullOrEmpty(assetName))
        {
            return -1;
        }
        string name = assetName.Replace('\\', '/');
        for (int i = 0; i < CustomUniqueSpecs.All.Count; i++)
        {
            if (
                name.EndsWith(
                    CustomUniqueSpecs.All[i].IconAsset,
                    StringComparison.OrdinalIgnoreCase
                )
            )
            {
                return i;
            }
        }
        return -1;
    }

    /// <summary>Visual borrowed by the item matching all three keys, null if none.</summary>
    public static CustomItemVisualSource VisualSource(int equipmentType, int subType, int uniqueId)
    {
        for (int i = 0; i < CustomUniqueSpecs.All.Count; i++)
        {
            CustomUniqueSpec spec = CustomUniqueSpecs.All[i];
            if (
                spec.BaseType == equipmentType
                && spec.BaseId == subType
                && spec.UniqueId == uniqueId
            )
            {
                return spec.VisualSource;
            }
        }
        return null;
    }
}
