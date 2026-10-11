using System.Reflection;
using LastEpoch_Hud.Scripts.Core.ModUI.Sliders;

namespace LastEpoch_Hud.Tests.Core.ModUI.Sliders;

internal static class ItemSliderLimitsReflection
{
    public static SliderRange[] AllRanges() =>
        typeof(ItemSliderLimits)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(field => field.FieldType == typeof(SliderRange))
            .Select(field => (SliderRange)field.GetValue(null))
            .ToArray();

    public static SliderRange? Find(string fieldName)
    {
        FieldInfo field = typeof(ItemSliderLimits).GetField(
            fieldName,
            BindingFlags.Public | BindingFlags.Static
        );
        return field == null ? null : (SliderRange)field.GetValue(null);
    }
}
