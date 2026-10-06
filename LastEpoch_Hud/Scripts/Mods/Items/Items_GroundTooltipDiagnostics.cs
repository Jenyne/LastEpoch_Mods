using System;
using HarmonyLib;
using Il2Cpp;
using Il2CppLE.UI.Tooltips;
using LastEpoch_Hud.Scripts.ModUI;
using UnityEngine;
using UnityEngine.EventSystems;
using ModSaveManager = LastEpoch_Hud.Scripts.ModUI.SaveManager;

namespace LastEpoch_Hud.Scripts.Mods.Items
{
    // Read-only: captures up to ten hover windows, with bounded output in each.
    internal static class Items_GroundTooltipDiagnostics
    {
        static int windows, budget, lastFrame = -1;
        static float until;
        [ThreadStatic] static int hoverDepth;
        static bool Enabled => Scenes.IsGameScene() && !ModSaveManager.instance.IsNullOrDestroyed()
            && ModSaveManager.instance.initialized && ModSettings.Debug.GroundItemTooltips.Value;
        static void Log(string text)
        {
            if (!Enabled || budget <= 0 || Time.realtimeSinceStartup > until) return;
            budget--;
            Main.logger_instance?.Msg("[GroundTooltip] " + text);
        }
        static void Begin(GameObject go)
        {
            if (!Enabled || windows >= 10 || go.IsNullOrDestroyed()) return;
            if (lastFrame == Time.frameCount) return;
            lastFrame = Time.frameCount; windows++; budget = 24; until = Time.realtimeSinceStartup + 3f;
            var label = go.GetComponentInParent<GroundItemLabel>();
            var item = label.IsNullOrDestroyed() ? null : label.getItemData();
            var button = go.GetComponent<UnityEngine.UI.Button>();
            var ability = Refs_Manager.player_actor.IsNullOrDestroyed() ? null
                : Refs_Manager.player_actor.gameObject.GetComponent<UsingAbilityPlayer>();
            Log("hover " + windows + "/10 object=" + go.name
                + " item=" + (item.IsNullOrDestroyed() ? "null" : $"{item.itemType}:{item.subType}, rarity={item.rarity}")
                + " button=" + (!button.IsNullOrDestroyed() ? button.interactable.ToString() : "none")
                + " outOfCombat=" + (!ability.IsNullOrDestroyed() ? ability.isOutOfCombat().ToString() : "unknown")
                + " mouseHeld=" + (Input.GetMouseButton(0) || Input.GetMouseButton(1))
                + " focus=" + Application.isFocused + " pause=" + Hud_Manager.IsPauseOpen());
        }
        [HarmonyPatch(typeof(GroundItemLabelButton), "OnPointerEnter")]
        static class LabelHover
        {
            [HarmonyPrefix]
            static void Prefix(GroundItemLabelButton __instance)
            {
                try { Begin(__instance.gameObject); Log("label pointer-enter"); } catch (Exception ex) { Log("trace error: " + ex.Message); }
            }
            [HarmonyPostfix]
            static void Postfix(GroundItemLabelButton __instance) { Log("label IsHovered=" + __instance.IsHovered); }
        }
        [HarmonyPatch(typeof(TooltipItem), "OnPointerEnter")]
        static class TooltipHover
        {
            [HarmonyPrefix]
            static void Prefix(TooltipItem __instance, out bool __state)
            {
                __state = false;
                if (!Enabled || __instance.slotType != TooltipItemManager.SlotType.GROUND) return;
                try { Begin(__instance.gameObject); Log("tooltip pointer-enter"); } catch (Exception ex) { Log("trace error: " + ex.Message); }
                __state = true; hoverDepth++;
            }
            [HarmonyFinalizer]
            static void Finalizer(bool __state, Exception __exception)
            {
                if (!__state) return;
                hoverDepth--;
                Log("tooltip pointer-enter returned" + (__exception == null ? "" : ": " + __exception.GetType().Name + " " + __exception.Message));
            }
        }
        [HarmonyPatch(typeof(TooltipItem), "OnPointerExit")]
        static class TooltipExit
        {
            [HarmonyPrefix]
            static void Prefix(TooltipItem __instance)
            {
                if (__instance.slotType == TooltipItemManager.SlotType.GROUND) Log("tooltip pointer-exit");
            }
        }
        [HarmonyPatch(typeof(UsingAbilityPlayer), "isOutOfCombat")]
        static class CombatGate
        {
            [HarmonyPostfix]
            static void Postfix(bool __result) { if (hoverDepth > 0) Log("hover queried out-of-combat: " + __result); }
        }
        [HarmonyPatch(typeof(TooltipSystem), "OpenItemTooltip")]
        static class Open
        {
            [HarmonyPrefix]
            static void Prefix(TooltipItemManager.SlotType __2) { if (__2 == TooltipItemManager.SlotType.GROUND) Log("tooltip system open requested"); }
            [HarmonyFinalizer]
            static void Finalizer(TooltipItemManager.SlotType __2, bool __result, Exception __exception)
            {
                if (__2 != TooltipItemManager.SlotType.GROUND) return;
                Log("tooltip system open result=" + __result + (__exception == null ? "" : " error=" + __exception.Message));
            }
        }
        [HarmonyPatch(typeof(TooltipSystem), "CloseItemTooltip")]
        static class Close
        {
            [HarmonyPrefix] static void Prefix() { Log("tooltip system close-item"); }
        }
        [HarmonyPatch(typeof(TooltipSystem), "CloseAllTooltips")]
        static class CloseAll
        {
            [HarmonyPrefix] static void Prefix() { Log("tooltip system close-all"); }
        }
        [HarmonyPatch(typeof(TooltipItemManager), "OpenTooltip", new Type[] {
            typeof(ItemDataUnpacked), typeof(TooltipItemManager.SlotType), typeof(Vector2), typeof(Vector3), typeof(GameObject), typeof(Vector2) })]
        static class LegacyOpen
        {
            [HarmonyPrefix]
            static void Prefix(TooltipItemManager.SlotType __1) { if (__1 == TooltipItemManager.SlotType.GROUND) Log("legacy tooltip open requested"); }
            [HarmonyFinalizer]
            static void Finalizer(TooltipItemManager __instance, TooltipItemManager.SlotType __1, Exception __exception)
            {
                if (__1 != TooltipItemManager.SlotType.GROUND) return;
                Log("legacy tooltip active=" + __instance.active + (__exception == null ? "" : " error=" + __exception.Message));
            }
        }
        [HarmonyPatch(typeof(TooltipItemManager), "CloseTooltip")]
        static class LegacyClose
        {
            [HarmonyPrefix] static void Prefix() { Log("legacy tooltip close"); }
        }
    }
}
