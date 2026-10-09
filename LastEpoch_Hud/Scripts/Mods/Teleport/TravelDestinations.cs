using System;
using System.Collections.Generic;
using Il2Cpp;
using LastEpoch_Hud.Scripts.Core.QualityOfLife;

namespace LastEpoch_Hud.Scripts.Mods.Teleport;

internal static class TravelDestinations
{
    public sealed class Destination
    {
        public string Scene;
        public string Name;
    }

    static readonly List<Destination> destinations = new();
    public static IReadOnlyList<Destination> All => destinations;

    public static void Refresh()
    {
        var list = Refs_Manager.scene_list;
        if (list.IsNullOrDestroyed())
            list = SceneList.instance;
        if (list.IsNullOrDestroyed() || list.scenes == null)
            return;
        var updated = new List<Destination>();
        var names = new HashSet<string>(StringComparer.Ordinal);
        for (int i = 0; i < list.scenes.Count; i++)
        {
            var details = list.scenes[i];
            if (
                details.IsNullOrDestroyed()
                || !TravelSceneRules.IsDestination(details.Name)
                || !names.Add(details.Name)
            )
                continue;
            string localized = details.LocalizedName;
            updated.Add(
                new Destination
                {
                    Scene = details.Name,
                    Name =
                        string.IsNullOrWhiteSpace(localized) || localized == details.Name
                            ? FavouriteTeleports.Caption(details.Name)
                            : localized,
                }
            );
        }
        updated.Sort(
            (a, b) =>
            {
                int name = StringComparer.CurrentCultureIgnoreCase.Compare(a.Name, b.Name);
                return name != 0 ? name : StringComparer.Ordinal.Compare(a.Scene, b.Scene);
            }
        );
        destinations.Clear();
        destinations.AddRange(updated);
    }

    public static Destination Find(string scene)
    {
        foreach (var destination in destinations)
            if (destination.Scene == scene)
                return destination;
        return null;
    }
}
