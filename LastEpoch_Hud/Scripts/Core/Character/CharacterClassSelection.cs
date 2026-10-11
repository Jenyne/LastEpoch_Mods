using System.Collections.Generic;
using System.Linq;

namespace LastEpoch_Hud.Scripts.Core.Character;

/// <summary>Maps the Class dropdown options to class ids.</summary>
public static class CharacterClassSelection
{
    /// <summary>Class order of the options (ascending class id).</summary>
    private static readonly CharacterClassKind[] _order =
    {
        CharacterClassKind.Primalist,
        CharacterClassKind.Mage,
        CharacterClassKind.Sentinel,
        CharacterClassKind.Acolyte,
        CharacterClassKind.Rogue,
    };

    /// <summary>Dropdown option texts (class names), built once.</summary>
    public static IReadOnlyList<string> OptionLabels { get; } =
        _order.Select(kind => kind.ToString()).ToArray();

    /// <summary>Class id of an option; false when the index is out of range.</summary>
    public static bool TryClassIdAt(int optionIndex, out int classId)
    {
        classId = 0;
        if (optionIndex < 0 || optionIndex >= _order.Length)
        {
            return false;
        }

        classId = (int)_order[optionIndex];
        return true;
    }

    /// <summary>Option showing a class id; 0 when the id is unknown.</summary>
    public static int OptionIndexOf(int classId)
    {
        int index = System.Array.IndexOf(_order, (CharacterClassKind)classId);
        return index < 0 ? 0 : index;
    }
}
