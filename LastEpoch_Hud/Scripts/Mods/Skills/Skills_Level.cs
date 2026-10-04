using System.Collections.Generic;
using HarmonyLib;
using Il2Cpp;
using UnityEngine;

namespace LastEpoch_Hud.Scripts.Mods.Skills
{
    public class Skills_Level
    {
        public static bool CanRun()
        {
            return Ready() && (LevelOn() || MultiplierOn());
        }

        static bool Ready()
        {
            if ((Scenes.IsGameScene()) && (!Save_Manager.instance.IsNullOrDestroyed()) && (!Refs_Manager.player_treedata.IsNullOrDestroyed()))
            {
                if ((!Save_Manager.instance.data.IsNullOrDestroyed()) && (!Refs_Manager.player_treedata.specialisedSkillTrees.IsNullOrDestroyed()))
                {
                    return true;
                }
            }
            return false;
        }

        static bool LevelOn()
        {
            return Save_Manager.instance.data.Skills.Enable_SkillLevel;
        }

        static bool MultiplierOn()
        {
            return Save_Manager.instance.data.Skills.Enable_SkillLevelMultiplier;
        }

        public static byte ChosenLevel()
        {
            int level = (int)Save_Manager.instance.data.Skills.SkillLevel;
            if (level < 0) { level = 0; }
            if (level > byte.MaxValue) { level = byte.MaxValue; }
            return (byte)level;
        }

        static int Multiplier()
        {
            return SettingRow.Clamp(Save_Manager.instance.data.Skills.SkillLevelMultiplier);
        }

        static bool writing;
        static bool nativeAdditionalRefresh;
        static readonly Dictionary<string, byte> real_additional = new Dictionary<string, byte>();
        static readonly Dictionary<string, byte> pending_real_additional = new Dictionary<string, byte>();

        public static void Sync()
        {
            Reapply();
            RefreshOpenTree();
        }

        public static void ApplyAll()
        {
            if (!LevelOn() && !MultiplierOn()) { return; }
            Sync();
        }

        static void Reapply()
        {
            if (!Ready()) { return; }
            try
            {
                writing = true;
                if (LevelOn()) { ApplyLevels(); }
                else { RestoreLevels(); }
                if (MultiplierOn()) { ApplyPointBonus(); }
                else { RestorePoints(); }
                writing = false;
            }
            catch { writing = false; }
        }

        static void ApplyLevels()
        {
            byte level = ChosenLevel();
            foreach (LocalTreeData.SkillTreeData data in Refs_Manager.player_treedata.specialisedSkillTrees)
            {
                if (data == null) { continue; }
                data.level = level;
            }
        }

        static void RestoreLevels()
        {
            foreach (LocalTreeData.SkillTreeData data in Refs_Manager.player_treedata.specialisedSkillTrees)
            {
                if (data == null) { continue; }
                data.level = SpecialisedAbilityManager.getAbilityLevel(data.abilityXp);
            }
        }

        static int PointLevel(LocalTreeData.SkillTreeData data)
        {
            if (LevelOn()) { return ChosenLevel(); }
            int level = SpecialisedAbilityManager.getAbilityLevel(data.abilityXp);
            if (level < 0) { level = 0; }
            if (level > byte.MaxValue) { level = byte.MaxValue; }
            return level;
        }

        static string Key(LocalTreeData.SkillTreeData data)
        {
            if (data == null || data.ability.IsNullOrDestroyed()) { return null; }
            return data.slot + ":" + data.ability.abilityName;
        }

        static byte BonusAdditional(int level, int real)
        {
            long additional = (((long)level + real) * Multiplier()) - level;
            if (additional < 0) { additional = 0; }
            if (additional > byte.MaxValue) { additional = byte.MaxValue; }
            return (byte)additional;
        }

        static void ApplyPointBonus()
        {
            foreach (LocalTreeData.SkillTreeData data in Refs_Manager.player_treedata.specialisedSkillTrees)
            {
                string key = Key(data);
                if (key == null) { continue; }
                if (!real_additional.ContainsKey(key)) { real_additional[key] = data.additionalMaxPointsFromStats; }
                data.additionalMaxPointsFromStats = BonusAdditional(PointLevel(data), real_additional[key]);
            }
        }

        static void RestorePoints()
        {
            foreach (LocalTreeData.SkillTreeData data in Refs_Manager.player_treedata.specialisedSkillTrees)
            {
                string key = Key(data);
                if (key == null || !real_additional.ContainsKey(key)) { continue; }
                data.additionalMaxPointsFromStats = real_additional[key];
            }
            real_additional.Clear();
        }

        static void OnAdditionalPointsUpdating()
        {
            if (writing || !Ready()) { return; }
            try
            {
                nativeAdditionalRefresh = true;
                pending_real_additional.Clear();
                writing = true;

                foreach (LocalTreeData.SkillTreeData data in Refs_Manager.player_treedata.specialisedSkillTrees)
                {
                    if (data == null) { continue; }

                    // Native skill-stat refresh must see the real XP-derived level.
                    if (LevelOn())
                    {
                        data.level = SpecialisedAbilityManager.getAbilityLevel(data.abilityXp);
                    }

                    // Likewise, never feed our multiplied point allowance back into the
                    // game's own over-investment/respec validation.
                    if (MultiplierOn())
                    {
                        string key = Key(data);
                        if (key != null && real_additional.ContainsKey(key))
                        {
                            data.additionalMaxPointsFromStats = real_additional[key];
                        }
                    }
                }

                writing = false;
            }
            catch
            {
                writing = false;
                nativeAdditionalRefresh = false;
            }
        }

        static void OnAdditionalPointsUpdated()
        {
            if (writing || !Ready())
            {
                nativeAdditionalRefresh = false;
                pending_real_additional.Clear();
                return;
            }

            try
            {
                writing = true;

                if (MultiplierOn())
                {
                    // The native refresh has now finished and each tree contains the
                    // NEW real +skills contribution. Rebuild the cache every time,
                    // including increases where LE never calls the over-investment
                    // respec routine.
                    real_additional.Clear();
                    foreach (LocalTreeData.SkillTreeData data in Refs_Manager.player_treedata.specialisedSkillTrees)
                    {
                        string key = Key(data);
                        if (key == null) { continue; }

                        if (pending_real_additional.ContainsKey(key))
                        {
                            // A decreasing cap went through our respec wrapper; its
                            // postfix restored the raw real value recorded there.
                            real_additional[key] = pending_real_additional[key];
                        }
                        else
                        {
                            // Increasing/unchanged caps do not necessarily invoke the
                            // respec routine, so take the freshly calculated native value.
                            real_additional[key] = data.additionalMaxPointsFromStats;
                        }
                    }
                }
                else
                {
                    real_additional.Clear();
                }

                if (LevelOn()) { ApplyLevels(); }
                if (MultiplierOn()) { ApplyPointBonus(); }
                writing = false;
            }
            catch { writing = false; }
            finally
            {
                nativeAdditionalRefresh = false;
                pending_real_additional.Clear();
            }
        }

        static void RefreshOpenTree()
        {
            try
            {
                foreach (SkillsPanelManager panel in Object.FindObjectsOfType<SkillsPanelManager>())
                {
                    if (!panel.IsNullOrDestroyed()) { panel.updateVisuals(false); }
                }
            }
            catch { }
        }

        [HarmonyPatch(typeof(SkillsPanelManager), "OnOpenSkillTree")]
        public class SkillsPanelManager_OnOpenSkillTree
        {
            [HarmonyPostfix]
            static void Postfix(SkillsPanelManager __instance, SkillTree __0)
            {
                try
                {
                    if (__0.IsNullOrDestroyed()) { return; }
                    Sync();
                    if (!__instance.IsNullOrDestroyed()) { __instance.updateVisuals(false); }
                }
                catch { Main.logger_instance?.Msg("SkillsPanelManager.OnOpenSkillTree() ERROR"); }
            }
        }

        [HarmonyPatch(typeof(LocalTreeData), "getAbilityLevel")]
        public class LocalTreeData_getAbilityLevel
        {
            [HarmonyPostfix]
            static void Postfix(LocalTreeData __instance, Ability __0, ref byte __result)
            {
                if (writing || !CanRun() || __0.IsNullOrDestroyed() || __instance.specialisedSkillTrees.IsNullOrDestroyed()) { return; }
                foreach (LocalTreeData.SkillTreeData data in __instance.specialisedSkillTrees)
                {
                    if (data == null || data.ability.IsNullOrDestroyed()) { continue; }
                    if (data.ability.abilityName != __0.abilityName) { continue; }
                    if (LevelOn())
                    {
                        byte level = ChosenLevel();
                        writing = true;
                        data.level = level;
                        writing = false;
                        __result = level;
                    }
                    else
                    {
                        byte real = SpecialisedAbilityManager.getAbilityLevel(data.abilityXp);
                        if (data.level != real)
                        {
                            writing = true;
                            data.level = real;
                            writing = false;
                        }
                        __result = real;
                        if (MultiplierOn() && !nativeAdditionalRefresh)
                        {
                            writing = true;
                            ApplyPointBonus();
                            writing = false;
                        }
                    }
                    return;
                }
            }
        }

        [HarmonyPatch(typeof(LocalTreeData), nameof(LocalTreeData.levelUpAbility))]
        public class LocalTreeData_levelUpAbility
        {
            [HarmonyPostfix]
            static void Postfix()
            {
                if (!CanRun()) { return; }
                Reapply();
            }
        }

        [HarmonyPatch(typeof(LocalTreeData), nameof(LocalTreeData.ApplyAbilityXp))]
        public class LocalTreeData_ApplyAbilityXp
        {
            [HarmonyPostfix]
            static void Postfix()
            {
                if (!CanRun()) { return; }
                Reapply();
            }
        }

        [HarmonyPatch(typeof(LocalTreeData), nameof(LocalTreeData.LevelSkillsToMinLevel))]
        public class LocalTreeData_LevelSkillsToMinLevel
        {
            [HarmonyPostfix]
            static void Postfix()
            {
                if (!CanRun()) { return; }
                Reapply();
            }
        }

        [HarmonyPatch(typeof(LocalTreeData), "respecNodesFromSkillIfOverInvested")]
        public class LocalTreeData_respecNodesFromSkillIfOverInvested
        {
            [HarmonyPrefix]
            static void Prefix(LocalTreeData.SkillTreeData __0, ref byte __1)
            {
                if (!Ready() || !MultiplierOn() || __0 == null) { return; }

                // The caller can enter this routine with the vanilla lost-point delta
                // already accumulated in overAllocatedAmount (__1). Once we replace
                // the cap with the multiplied cap, keeping that carry-in double-counts
                // the gear loss (for example +2 at x2 becoming 2 + 4 = 6 removed).
                // Recalculate the overage from zero against the multiplied cap.
                __1 = 0;

                // At this point the native stat refresh has already written the NEW
                // real +skills value into additionalMaxPointsFromStats. Preserve it,
                // then present the multiplied cap to LE's normal over-investment
                // respec routine. Example: losing +2 skills at x2 lowers the effective
                // cap by 4, so the native respec removes exactly four allocated points.
                string key = Key(__0);
                if (key == null) { return; }

                byte real = __0.additionalMaxPointsFromStats;
                pending_real_additional[key] = real;
                __0.additionalMaxPointsFromStats = BonusAdditional(PointLevel(__0), real);
            }

            [HarmonyPostfix]
            static void Postfix(LocalTreeData.SkillTreeData __0)
            {
                if (!nativeAdditionalRefresh || __0 == null) { return; }

                // Keep the synthetic multiplied value scoped to the respec calculation.
                // The rest of LE's native stat refresh should continue seeing the real
                // +skills contribution; our outer postfix reapplies the multiplier.
                string key = Key(__0);
                if (key != null && pending_real_additional.ContainsKey(key))
                {
                    __0.additionalMaxPointsFromStats = pending_real_additional[key];
                }
            }
        }

        [HarmonyPatch(typeof(LocalTreeData), nameof(LocalTreeData.setAdditionalMaxPointsFromStatsOnServerOrInSingleplayer))]
        public class LocalTreeData_setAdditionalMaxPointsFromStats
        {
            [HarmonyPrefix]
            static void Prefix()
            {
                OnAdditionalPointsUpdating();
            }

            [HarmonyPostfix]
            static void Postfix()
            {
                OnAdditionalPointsUpdated();
                RefreshOpenTree();
            }
        }

        [HarmonyPatch(typeof(LocalTreeData), nameof(LocalTreeData.receiveAdditionalMaxPointsFromStatsOnClient))]
        public class LocalTreeData_receiveAdditionalMaxPointsFromStatsOnClient
        {
            [HarmonyPostfix]
            static void Postfix()
            {
                OnAdditionalPointsUpdated();
                RefreshOpenTree();
            }
        }
    }
}
