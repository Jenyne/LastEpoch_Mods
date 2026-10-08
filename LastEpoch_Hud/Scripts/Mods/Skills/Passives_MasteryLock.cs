using System.Collections.Generic;
using HarmonyLib;
using Il2Cpp;
using LastEpoch_Hud.Scripts.Core;
using LastEpoch_Hud.Scripts.ModUI;
using UnityEngine;

namespace LastEpoch_Hud.Scripts.Mods.Skills;

internal static class Passives_MasteryLock
{
    private static readonly MasteryLimitOverride limit = new();
    private static readonly Dictionary<int, (GameObject Line, bool Active)> hiddenLines = new();
    private static bool refreshing;
    private static bool reportedError;

    internal static bool Enabled =>
        ModSettings.MasteryTreeUnlock.Enabled.Value
        && Scenes.IsGameScene()
        && !Refs_Manager.player_treedata.IsNullOrDestroyed()
        && Refs_Manager.player_treedata.passiveTree != null
        && Refs_Manager.player_treedata.chosenMastery > 0;

    public static void Sync()
    {
        try
        {
            bool wasActive = limit.Active;
            byte current = GlobalTreeData.maximumUnchosenMasteryLevel;
            byte target = limit.Sync(
                current,
                GlobalTreeData.masteryClassMaximumMasteryLevel,
                Enabled
            );
            if (current != target)
                GlobalTreeData.maximumUnchosenMasteryLevel = target;
            if (wasActive != limit.Active || current != target)
                Refresh();
        }
        catch (System.Exception exception)
        {
            if (!reportedError)
            {
                reportedError = true;
                Main.logger_instance?.Error("Mastery tree unlock: " + exception.Message);
            }
        }
    }

    private static void RestoreLines()
    {
        foreach (var entry in hiddenLines.Values)
            if (!entry.Line.IsNullOrDestroyed())
                entry.Line.SetActive(entry.Active);
        hiddenLines.Clear();
    }

    private static void Refresh()
    {
        if (refreshing)
            return;
        refreshing = true;
        try
        {
            RestoreLines();
            if (
                Scenes.IsGameScene()
                && !Refs_Manager.player_treedata.IsNullOrDestroyed()
                && Refs_Manager.player_treedata.passiveTree != null
                && !TreeUIManager.characterTree.IsNullOrDestroyed()
            )
            {
                TreeUIManager.updatePassiveTreeUIWithData();
                UpdateLines();
            }
        }
        finally
        {
            refreshing = false;
        }
    }

    private static void UpdateLines()
    {
        if (!Enabled || TreeUIManager.characterTree.IsNullOrDestroyed())
            return;
        byte chosen = Refs_Manager.player_treedata.chosenMastery;
        foreach (
            var panel in TreeUIManager.characterTree.GetComponentsInChildren<CharacterTreeMasteryPanelRefs>(
                true
            )
        )
        {
            if (
                panel.masteryIndex <= 0
                || panel.masteryIndex == chosen
                || panel.masteryLockLines == null
            )
                continue;
            foreach (var line in panel.masteryLockLines)
            {
                if (line.IsNullOrDestroyed())
                    continue;
                int id = line.GetInstanceID();
                if (!hiddenLines.ContainsKey(id))
                    hiddenLines.Add(id, (line, line.activeSelf));
                line.SetActive(false);
            }
        }
    }

    // Only the visual method receives a different mastery argument. The saved
    // chosen mastery and native prerequisites, points and node ranks stay intact.
    [HarmonyPatch(typeof(TreeUIManager), nameof(TreeUIManager.UpdateNodeLockVisual))]
    private static class NodeLockVisual
    {
        [HarmonyPrefix]
        private static void Prefix(SkillTreeNode __0, ref byte __1)
        {
            if (Enabled && !__0.IsNullOrDestroyed() && __0.inPassiveTree() && __0.mastery > 0)
                __1 = (byte)__0.mastery;
        }
    }

    [HarmonyPatch(typeof(MasteryBarDisplay), nameof(MasteryBarDisplay.GetMaxMasteryPoints))]
    private static class BarLimit
    {
        [HarmonyPrefix]
        private static void Prefix(int __0, ref bool __2)
        {
            if (Enabled && __0 > 0)
                __2 = true;
        }
    }

    [HarmonyPatch(typeof(TreeUIManager), nameof(TreeUIManager.updatePassiveTreeDisplayForMastery))]
    private static class TreeDisplay
    {
        [HarmonyPostfix]
        private static void Postfix() => UpdateLines();
    }

    [HarmonyPatch(
        typeof(Il2CppLE.UI.PanelSystem.FullScreenPanelPassiveTree),
        "UpdateMasteryDisplay"
    )]
    private static class PanelDisplay
    {
        [HarmonyPrefix]
        private static void Prefix() => Sync();

        [HarmonyPostfix]
        private static void Postfix() => UpdateLines();
    }
}
