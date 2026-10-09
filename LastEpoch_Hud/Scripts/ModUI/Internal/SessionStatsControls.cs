using LastEpoch_Hud.Scripts.Mods.UI;
using UnityEngine;
using UnityEngine.UI;

namespace LastEpoch_Hud.Scripts.ModUI;

internal static class SessionStatsControls
{
    static Text totals;
    static Text show;
    static Text pause;

    public static void Bind(GameObject content, Text sample)
    {
        if (
            content.IsNullOrDestroyed()
            || sample.IsNullOrDestroyed()
            || !Prefab.Child(content, "SessionStats").IsNullOrDestroyed()
        )
            return;
        var section = QualityOfLifeControls.Section(content, "SessionStats", 200);
        QualityOfLifeControls.Label(section, "Title", sample, "Session Gains", 0, 24);
        totals = QualityOfLifeControls.Label(section, "Totals", sample, "", 24, 76);
        totals.fontSize = 12;
        show = QualityOfLifeControls
            .Button(
                section,
                "ShowOverlay",
                sample,
                "",
                .03f,
                .44f,
                106,
                () =>
                {
                    ModSettings.SessionStats.ShowOverlay.Set(
                        !ModSettings.SessionStats.ShowOverlay.Value
                    );
                    Refresh();
                }
            )
            .GetComponentInChildren<Text>(true);
        pause = QualityOfLifeControls
            .Button(
                section,
                "Pause",
                sample,
                "",
                .46f,
                .72f,
                106,
                () =>
                {
                    SessionGainCounters.SetPaused(!SessionGainCounters.Session.Paused);
                    Refresh();
                }
            )
            .GetComponentInChildren<Text>(true);
        QualityOfLifeControls.Button(
            section,
            "Reset",
            sample,
            "Reset",
            .74f,
            .97f,
            106,
            () =>
            {
                SessionGainCounters.Reset();
                Refresh();
            }
        );
        QualityOfLifeControls.Label(
            section,
            "MoveHint",
            sample,
            "Hold Alt and drag the counter to move it.",
            134,
            28
        );
        QualityOfLifeControls.Button(
            section,
            "ResetPosition",
            sample,
            "Reset counter position",
            .03f,
            .97f,
            166,
            SessionGainCounters.ResetPosition
        );
        Refresh();
    }

    public static void Refresh()
    {
        if (totals.IsNullOrDestroyed())
            return;
        totals.text = SessionGainCounters.Format();
        LocaleRegistry.Apply(
            show,
            ModSettings.SessionStats.ShowOverlay.Value ? "Hide counter HUD" : "Show counter HUD"
        );
        LocaleRegistry.Apply(pause, SessionGainCounters.Session.Paused ? "Resume" : "Pause");
    }
}
