using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Il2Cpp;
using LastEpoch_Hud.Scripts.ModUI;
using UnityEngine;
using ModSaveManager = LastEpoch_Hud.Scripts.ModUI.SaveManager;

namespace LastEpoch_Hud.Scripts.Mods.Items
{
    internal static class Items_DropRateMultipliers
    {
        [ThreadStatic] static int spawnDepth;
        static bool reported;

        static float Factor(FloatSetting setting)
        {
            if (!setting.Enabled) return 1f;
            float percent = setting.Value;
            if (float.IsNaN(percent) || float.IsInfinity(percent)) return 1f;
            return Mathf.Clamp(Mathf.Round(percent), 0f, 1000f) / 100f;
        }
        static float Scale(float original, float factor)
        {
            if (factor == 1f || float.IsNaN(original) || float.IsInfinity(original) || original < 0f) return original;
            double result = (double)original * factor;
            return (float)Math.Min(result, float.MaxValue);
        }
        static bool CanRun(GenerateItems generator)
        {
            if (spawnDepth != 0 || !Scenes.IsGameScene() || Hud_Manager.IsPauseOpen()
                || ModSaveManager.instance.IsNullOrDestroyed() || !ModSaveManager.instance.initialized
                || generator.IsNullOrDestroyed() || generator.actor.IsNullOrDestroyed()
                || Refs_Manager.player_actor.IsNullOrDestroyed()
                || generator.actor.Pointer != Refs_Manager.player_actor.Pointer) return false;
            // Explicit force-rarity modes take priority over natural drop rates.
            if (!Save_Manager.instance.IsNullOrDestroyed() && !Save_Manager.instance.data.IsNullOrDestroyed())
            {
                var drop = Save_Manager.instance.data.Items.Drop;
                if (drop.Enable_ForceUnique || drop.Enable_ForceSet || drop.Enable_ForceLegendary) return false;
            }
            return true;
        }
        internal sealed class Snapshot
        {
            internal GenerateItems Generator;
            internal float Unique, LowUnique, Set, RareConversion, Exalted, T7;
            internal bool RarityChanged, AffixesChanged;
            internal void Restore()
            {
                if (RarityChanged)
                {
                    GenerateItems.uniqueDropRate = Unique;
                    GenerateItems.lowLevelUniqueDropRate = LowUnique;
                    GenerateItems.setDropRate = Set;
                }
                if (AffixesChanged && !Generator.IsNullOrDestroyed())
                {
                    Generator._ChanceForRareItemsToBecomeExalted0To1_k__BackingField = RareConversion;
                    Generator._ChanceForExaltedAffixesMultiplier_k__BackingField = Exalted;
                    Generator._ChanceForT7Affixes_k__BackingField = T7;
                }
            }
        }
        [HarmonyPatch]
        static class NaturalSpawn
        {
            static IEnumerable<MethodBase> TargetMethods()
            {
                // The outer entry also covers an inlined spawnRandomItem call.
                yield return AccessTools.Method(typeof(GenerateItems), "SpawnItemAtPoint");
                yield return AccessTools.Method(typeof(GenerateItems), "spawnRandomItem");
            }
            [HarmonyPrefix]
            static void Prefix(GenerateItems __instance, out Snapshot __state)
            {
                __state = null;
                if (!CanRun(__instance)) return;
                float unique = Factor(ModSettings.DropRates.Unique);
                float set = Factor(ModSettings.DropRates.Set);
                float exalted = Factor(ModSettings.DropRates.Exalted);
                float t7 = Factor(ModSettings.DropRates.T7);
                if (unique == 1f && set == 1f && exalted == 1f && t7 == 1f) return;
                var state = new Snapshot
                {
                    Generator = __instance,
                    Unique = GenerateItems.uniqueDropRate,
                    LowUnique = GenerateItems.lowLevelUniqueDropRate,
                    Set = GenerateItems.setDropRate,
                    RareConversion = __instance._ChanceForRareItemsToBecomeExalted0To1_k__BackingField,
                    Exalted = __instance._ChanceForExaltedAffixesMultiplier_k__BackingField,
                    T7 = __instance._ChanceForT7Affixes_k__BackingField,
                    RarityChanged = unique != 1f || set != 1f,
                    AffixesChanged = exalted != 1f || t7 != 1f
                };
                __state = state; spawnDepth++;
                if (state.RarityChanged)
                {
                    GenerateItems.uniqueDropRate = Scale(state.Unique, unique);
                    GenerateItems.lowLevelUniqueDropRate = Scale(state.LowUnique, unique);
                    GenerateItems.setDropRate = Scale(state.Set, set);
                }
                if (state.AffixesChanged)
                {
                    if (exalted != 1f)
                    {
                        __instance._ChanceForRareItemsToBecomeExalted0To1_k__BackingField = Mathf.Clamp01(Scale(state.RareConversion, exalted));
                        __instance._ChanceForExaltedAffixesMultiplier_k__BackingField = Scale(state.Exalted, exalted);
                    }
                    if (t7 != 1f)
                        __instance._ChanceForT7Affixes_k__BackingField = Mathf.Clamp01(Scale(state.T7, t7));
                }
                if (!reported)
                {
                    reported = true;
                    Main.logger_instance?.Msg($"[DropRates] Normal spawn overrides active: unique={unique}x, set={set}x, exalted={exalted}x, T7={t7}x; native baselines unique={state.Unique}, lowUnique={state.LowUnique}, set={state.Set}, rareConversion={state.RareConversion}, exalted={state.Exalted}, T7={state.T7}");
                }
            }
            [HarmonyFinalizer]
            static void Finalizer(Snapshot __state)
            {
                if (__state == null) return;
                try { __state.Restore(); }
                finally { spawnDepth--; }
            }
        }
    }
}
