using System;
using LastEpoch_Hud.Scripts.Mods.Teleport;
using UnityEngine;
using UnityEngine.UI;

namespace LastEpoch_Hud.Scripts.ModUI;

internal static class TravelAnywhereControls
{
    static GameObject section;
    static Dropdown picker;
    static Text enabled;
    static Text status;
    static Button travel;
    static bool visible;
    static float nextRefresh;
    static float nextStatus;
    static object locale;
    static readonly System.Collections.Generic.List<string> ids = new();

    public static void Bind(GameObject content, GameObject misc, Text sample)
    {
        if (
            misc.IsNullOrDestroyed()
            || sample.IsNullOrDestroyed()
            || !Prefab.Child(misc, "TravelAnywhere").IsNullOrDestroyed()
        )
            return;
        var dungeons = Prefab.ViewportContent(content, "Center", "Scenes_Dungeons_Content");
        var source = dungeons.IsNullOrDestroyed()
            ? null
            : Prefab.Child(dungeons, "Teleport_Dropdown");
        if (source.IsNullOrDestroyed() || source.GetComponent<Dropdown>().IsNullOrDestroyed())
        {
            Main.logger_instance?.Warning(
                "Travel Anywhere: native scene picker template is unavailable."
            );
            return;
        }
        section = QualityOfLifeControls.Section(misc, "TravelAnywhere", 184);
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
                    RefreshStatus();
                }
            )
            .GetComponentInChildren<Text>(true);
        var selector = UnityEngine.Object.Instantiate(source, section.transform, false);
        selector.name = "Destination";
        QualityOfLifeControls.Place(selector, .03f, .97f, 54, 26);
        var layout = selector.GetComponent<LayoutElement>();
        if (!layout.IsNullOrDestroyed())
            layout.ignoreLayout = true;
        selector.SetActive(true);
        picker = selector.GetComponent<Dropdown>();
        picker.onValueChanged.RemoveAllListeners();
        Prefab.BindDropdown(picker, new Action<int>(_ => RefreshStatus()));
        travel = QualityOfLifeControls.Button(
            section,
            "Travel",
            sample,
            "Travel to selected area",
            .03f,
            .64f,
            86,
            () =>
            {
                if (picker.value > 0 && picker.value < ids.Count)
                    TravelAnywhere.StartTravel(ids[picker.value]);
                RefreshStatus();
            }
        );
        QualityOfLifeControls.Button(
            section,
            "Refresh",
            sample,
            "Refresh",
            .66f,
            .97f,
            86,
            RefreshDestinations
        );
        status = QualityOfLifeControls.Label(section, "Status", sample, "", 116, 60);
        RefreshDestinations();
    }

    public static void Tick()
    {
        if (section.IsNullOrDestroyed())
            return;
        bool active = section.activeInHierarchy;
        bool languageChanged = !ReferenceEquals(locale, Locales.current_dictionary);
        if (
            active
            && (
                (!visible || languageChanged)
                || (ids.Count <= 1 && Time.unscaledTime >= nextRefresh)
            )
        )
        {
            nextRefresh = Time.unscaledTime + 1;
            RefreshDestinations();
        }
        visible = active;
        if (active && Time.unscaledTime >= nextStatus)
        {
            nextStatus = Time.unscaledTime + .25f;
            RefreshStatus();
        }
    }

    static void RefreshDestinations()
    {
        if (picker.IsNullOrDestroyed())
            return;
        try
        {
            string selected = picker.value < ids.Count ? ids[picker.value] : "";
            TravelDestinations.Refresh();
            ids.Clear();
            ids.Add("");
            picker.options.Clear();
            picker.options.Add(new Dropdown.OptionData(LocaleRegistry.Translate("Select an area")));
            foreach (var destination in TravelDestinations.All)
            {
                ids.Add(destination.Scene);
                picker.options.Add(new Dropdown.OptionData(destination.Name));
            }
            picker.value = Math.Max(0, ids.IndexOf(selected));
            picker.RefreshShownValue();
            locale = Locales.current_dictionary;
            RefreshStatus();
        }
        catch (Exception ex)
        {
            ErrorLog.Report(ex, "Travel Anywhere picker");
        }
    }

    static void RefreshStatus()
    {
        if (status.IsNullOrDestroyed())
            return;
        bool on = ModSettings.TravelAnywhere.Enabled.Value;
        LocaleRegistry.Apply(enabled, on ? "Disable Travel Anywhere" : "Enable Travel Anywhere");
        travel.interactable =
            on && !TravelAnywhere.Busy && picker.value > 0 && picker.value < ids.Count;
        LocaleRegistry.Apply(status, TravelAnywhere.Status);
    }
}
