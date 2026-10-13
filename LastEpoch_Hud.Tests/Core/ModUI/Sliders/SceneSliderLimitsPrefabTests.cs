using LastEpoch_Hud.Scripts.Core.ModUI.Sliders;
using LastEpoch_Hud.Tests.Support;

namespace LastEpoch_Hud.Tests.Core.ModUI.Sliders;

public sealed class SceneSliderLimitsPrefabTests
{
    [Theory]
    [InlineData(nameof(SceneSliderLimits.CameraZoomMinimum), "Slider_Scenes_Camera_ZoomMinimum")]
    [InlineData(
        nameof(SceneSliderLimits.CameraZoomPerScroll),
        "Slider_Scenes_Camera_ZoomPerScroll"
    )]
    [InlineData(nameof(SceneSliderLimits.CameraZoomSpeed), "Slider_Scenes_Camera_ZoomSpeed")]
    [InlineData(
        nameof(SceneSliderLimits.CameraDefaultRotation),
        "Slider_Scenes_Camera_DefaultRotation"
    )]
    [InlineData(
        nameof(SceneSliderLimits.CameraOffsetMinimum),
        "Slider_Scenes_Camera_OffsetMinimum"
    )]
    [InlineData(
        nameof(SceneSliderLimits.CameraOffsetMaximum),
        "Slider_Scenes_Camera_OffsetMaximum"
    )]
    [InlineData(nameof(SceneSliderLimits.CameraAngleMinimum), "Slider_Scenes_Camera_AngleMinimum")]
    [InlineData(nameof(SceneSliderLimits.CameraAngleMaximum), "Slider_Scenes_Camera_AngleMaximum")]
    public void CameraRange_MatchesPrefab(string field, string slider)
    {
        IReadOnlyDictionary<string, SliderRange> prefab = PrefabSliders.Load(
            PrefabSliders.HudPrefabPath
        );

        Assert.True(prefab.ContainsKey(slider), $"Prefab slider missing: {slider}");
        SliderRange? actual = SliderLimitsReflection.Find(typeof(SceneSliderLimits), field);
        Assert.True(actual.HasValue, $"Limits field missing: {field}");
        Assert.Equal(prefab[slider], actual.Value);
    }

    [Theory]
    [InlineData(
        nameof(SceneSliderLimits.MonolithMaxStability),
        "Slider_Scenes_Monoliths_MaxStability"
    )]
    [InlineData(
        nameof(SceneSliderLimits.MonolithMobsDensity),
        "Slider_Scenes_Monoliths_MobsDensity"
    )]
    [InlineData(
        nameof(SceneSliderLimits.MonolithMobsDefeat),
        "Slider_Scenes_Monoliths_MobsDefeatOnStart"
    )]
    [InlineData(
        nameof(SceneSliderLimits.MonolithBlessingSlots),
        "Slider_Scenes_Monoliths_BlessingSlots"
    )]
    public void MonolithRange_MatchesPrefab(string field, string slider)
    {
        IReadOnlyDictionary<string, SliderRange> prefab = PrefabSliders.Load(
            PrefabSliders.HudPrefabPath
        );

        Assert.True(prefab.ContainsKey(slider), $"Prefab slider missing: {slider}");
        SliderRange? actual = SliderLimitsReflection.Find(typeof(SceneSliderLimits), field);
        Assert.True(actual.HasValue, $"Limits field missing: {field}");
        // WorldMonolithsPage forces whole numbers although the prefab says false.
        Assert.Equal(new SliderRange(prefab[slider].Min, prefab[slider].Max, true), actual.Value);
    }
}
