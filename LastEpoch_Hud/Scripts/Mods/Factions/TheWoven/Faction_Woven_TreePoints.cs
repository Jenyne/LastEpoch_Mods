using HarmonyLib;
using Il2CppLE.Factions;

namespace LastEpoch_Hud.Scripts.Mods.Factions.TheWoven
{
    public class Faction_Woven_TreePoints
    {
        public const int SliderMax = 200;

        public static int ClampPoints(int points)
        {
            if (points < 0) { return 0; }
            if (points > ushort.MaxValue) { return ushort.MaxValue; }
            return points;
        }

        public static void ApplyToPlayer()
        {
            if (Refs_Manager.player_treedata.IsNullOrDestroyed() || Refs_Manager.player_treedata.weaverTree == null) { return; }

            try
            {
                if (CanRun())
                {
                    Refs_Manager.player_treedata.weaverTree.EarnedWeaverPoints =
                        (ushort)ClampPoints(Save_Manager.instance.data.Factions.TheWoven.TreePoints);
                }

                foreach (FactionRankUIWeaver ui in UnityEngine.Object.FindObjectsOfType<FactionRankUIWeaver>())
                {
                    if (!ui.IsNullOrDestroyed()) { ui.UpdateUnspentPointsRoot(); }
                }
            }
            catch { }
        }

        public static bool CanRun()
        {
            if ((Scenes.IsGameScene()) && (!Save_Manager.instance.IsNullOrDestroyed()))
            {
                return !Save_Manager.instance.data.IsNullOrDestroyed() && Save_Manager.instance.data.Factions.TheWoven.Enable_TreePoints;
            }
            else { return false; }
        }

        [HarmonyPatch(typeof(FactionRankUIWeaver), "OnPanelOpen", new System.Type[] { })]
        public class FactionRankUIWeaver_OnPanelOpen
        {
            [HarmonyPostfix]
            static void Postfix(FactionRankUIWeaver __instance)
            {
                if (!CanRun() || Refs_Manager.player_treedata.IsNullOrDestroyed() || Refs_Manager.player_treedata.weaverTree == null)
                {
                    return;
                }

                try
                {
                    Refs_Manager.player_treedata.weaverTree.EarnedWeaverPoints =
                        (ushort)ClampPoints(Save_Manager.instance.data.Factions.TheWoven.TreePoints);
                    __instance.UpdateUnspentPointsRoot();
                }
                catch { }
            }
        }
    }
}
