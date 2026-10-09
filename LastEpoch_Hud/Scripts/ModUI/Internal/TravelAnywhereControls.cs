using LastEpoch_Hud.Scripts.Mods.Teleport;
using UnityEngine;
using UnityEngine.UI;

namespace LastEpoch_Hud.Scripts.ModUI;

internal static class TravelAnywhereControls
{
    static GameObject section;
    static Text enabled;
    static Text status;
    static float nextStatus;

    public static void Bind(GameObject misc, Text sample)
    {
        if (
            misc.IsNullOrDestroyed()
            || sample.IsNullOrDestroyed()
            || !Prefab.Child(misc, "TravelAnywhere").IsNullOrDestroyed()
        )
            return;
        section = QualityOfLifeControls.Section(misc, "TravelAnywhere", 120);
        QualityOfLifeControls.Label(section, "Title", sample, "Travel Anywhere", 0, 24);
        enabled = QualityOfLifeControls
            .Button(
                section,
                "Enabled",
                sample,
                "",
                .03f,
                .97f,
                26,
                () =>
                {
                    ModSettings.TravelAnywhere.Enabled.Set(
                        !ModSettings.TravelAnywhere.Enabled.Value
                    );
                    TravelMapWaypoints.Tick();
                    RefreshStatus();
                }
            )
            .GetComponentInChildren<Text>(true);
        status = QualityOfLifeControls.Label(section, "Status", sample, "", 54, 60);
        RefreshStatus();
    }

    public static void Tick()
    {
        if (section.IsNullOrDestroyed() || !section.activeInHierarchy)
            return;
        if (Time.unscaledTime >= nextStatus)
        {
            nextStatus = Time.unscaledTime + .25f;
            RefreshStatus();
        }
    }

    static void RefreshStatus()
    {
        if (status.IsNullOrDestroyed())
            return;
        LocaleRegistry.Apply(
            enabled,
            ModSettings.TravelAnywhere.Enabled.Value
                ? "Disable Travel Anywhere"
                : "Enable Travel Anywhere"
        );
        LocaleRegistry.Apply(status, TravelAnywhere.Status);
    }
}
