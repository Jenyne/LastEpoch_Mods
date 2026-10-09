using LastEpoch_Hud.Scripts.Core.QualityOfLife;
using LastEpoch_Hud.Scripts.Mods.Teleport;
using UnityEngine;
using UnityEngine.UI;

namespace LastEpoch_Hud.Scripts.ModUI;

internal static class FavouriteTeleportControls
{
    static GameObject section;
    static Text status;
    static readonly GameObject[] rows = new GameObject[FavouriteDestinations.Capacity];
    static readonly Text[] captions = new Text[FavouriteDestinations.Capacity];

    public static void Bind(GameObject content, Text sample)
    {
        if (
            content.IsNullOrDestroyed()
            || sample.IsNullOrDestroyed()
            || !Prefab.Child(content, "FavouriteTeleports").IsNullOrDestroyed()
        )
            return;
        section = QualityOfLifeControls.Section(content, "FavouriteTeleports", 100);
        QualityOfLifeControls.Label(section, "Title", sample, "Favourite Teleports", 0, 24);
        KeyTeleportDestinations.Refresh();
        QualityOfLifeControls.Button(
            section,
            "RefreshKeys",
            sample,
            "Refresh key teleports",
            .03f,
            .97f,
            26,
            () =>
            {
                KeyTeleportDestinations.Refresh();
                FavouriteTeleports.SetStatus(
                    "Choose a key teleport, or save up to 8 extra favourites."
                );
                Refresh();
            }
        );
        for (int i = 0; i < KeyTeleportDestinations.Labels.Length; i++)
        {
            int index = i;
            QualityOfLifeControls.Button(
                section,
                "KeyTeleport" + i,
                sample,
                KeyTeleportDestinations.Labels[i],
                .03f,
                .97f,
                54 + i * 28,
                () =>
                {
                    KeyTeleportDestinations.Travel(index);
                    Refresh();
                }
            );
        }
        QualityOfLifeControls.Button(
            section,
            "SaveCurrent",
            sample,
            "Favourite current waypoint",
            .03f,
            .97f,
            54 + KeyTeleportDestinations.Labels.Length * 28,
            () =>
            {
                FavouriteTeleports.SaveCurrent();
                Refresh();
            }
        );
        for (int i = 0; i < rows.Length; i++)
        {
            int index = i;
            var row = QualityOfLifeControls.Node(section, "Favourite" + i);
            rows[i] = row;
            QualityOfLifeControls.Place(
                row,
                .03f,
                .97f,
                82 + KeyTeleportDestinations.Labels.Length * 28 + i * 28,
                24
            );
            var travel = QualityOfLifeControls.Button(
                row,
                "Travel",
                sample,
                "",
                0,
                .78f,
                0,
                () =>
                {
                    if (index < FavouriteTeleports.Scenes.Count)
                        FavouriteTeleports.Travel(FavouriteTeleports.Scenes[index]);
                    Refresh();
                }
            );
            captions[i] = travel.GetComponentInChildren<Text>(true);
            QualityOfLifeControls.Button(
                row,
                "Remove",
                sample,
                "Remove",
                .80f,
                1,
                0,
                () =>
                {
                    if (index < FavouriteTeleports.Scenes.Count)
                        FavouriteTeleports.Remove(FavouriteTeleports.Scenes[index]);
                    Refresh();
                }
            );
        }
        status = QualityOfLifeControls.Label(section, "Status", sample, "", 54, 40);
        Refresh();
    }

    static void Refresh()
    {
        if (section.IsNullOrDestroyed())
            return;
        var scenes = FavouriteTeleports.Scenes;
        for (int i = 0; i < rows.Length; i++)
        {
            rows[i].SetActive(i < scenes.Count);
            if (i < scenes.Count)
                LocaleRegistry.Apply(captions[i], FavouriteTeleports.Caption(scenes[i]));
        }
        float top = 82 + KeyTeleportDestinations.Labels.Length * 28 + scenes.Count * 28;
        QualityOfLifeControls.Place(status.gameObject, .03f, .97f, top, 40);
        LocaleRegistry.Apply(status, FavouriteTeleports.Status);
        QualityOfLifeControls.Height(section, top + 44);
        LayoutRebuilder.ForceRebuildLayoutImmediate(
            section.transform.parent.GetComponent<RectTransform>()
        );
    }
}
