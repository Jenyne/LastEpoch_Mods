using HarmonyLib;
using Il2Cpp;
using LastEpoch_Hud.Scripts.ModUI;
using ModSaveManager = LastEpoch_Hud.Scripts.ModUI.SaveManager;
using UnityEngine;

namespace LastEpoch_Hud.Scripts.Mods.Items
{
    // Restores the old Craft_MaxTier behavior in a narrower form:
    // normal equipment may be upgraded through displayed T7 (internal tier 6).
    // T8 remains reserved for primordial mechanics.
    internal static class Items_AdvancedForge
    {
        static CraftingSlotManager craftingSlotManager;
        static ItemData item;

        static bool Ready() =>
            ModSaveManager.instance != null
            && ModSaveManager.instance.initialized
            && Scenes.IsGameScene();

        static bool T7Enabled() => Ready() && ModSettings.AdvancedForge.AllowT7Crafting.Value;

        static int GetTier(ItemData data, int affixId)
        {
            if (data.IsNullOrDestroyed()) return -1;
            foreach (ItemAffix affix in data.affixes)
                if (affix.affixId == affixId && !affix.IsSealed)
                    return affix.affixTier;
            return -1;
        }

        static ItemAffix GetAffix(ItemData data, int affixId)
        {
            if (data.IsNullOrDestroyed()) return null;
            foreach (ItemAffix affix in data.affixes)
                if (affix.affixId == affixId && !affix.IsSealed)
                    return affix;
            return null;
        }

        static bool IsIdol(ItemData data) =>
            !data.IsNullOrDestroyed() && data.itemType > 24 && data.itemType < 34;

        static int MaxForgeCost(int internalTier)
        {
            // Historical RCInet values for upgrades into T6/T7.
            if (internalTier >= 5) return 36;
            return 30;
        }

        static int SupportGlyphSubtype(CraftingSlotManager manager)
        {
            if (manager.IsNullOrDestroyed()) return -1;
            OneItemContainer support = manager.GetSupport();
            if (support.IsNullOrDestroyed()) return -1;
            ItemData supportItem = support.getItem();
            if (supportItem.IsNullOrDestroyed() || supportItem.itemType != 103) return -1;
            return supportItem.subType;
        }

        static bool PreserveFp() =>
            ModSettings.InfiniteForgingPotential.Enabled.Value
            || ModSettings.AdvancedForge.GuaranteedGlyphOfHope.Value;

        static void ApplyForcedRoll(ItemAffix affix)
        {
            if (affix.IsNullOrDestroyed() || !ModSettings.AdvancedForge.AffixRoll.Enabled) return;
            affix.affixRoll = (byte)Mathf.Clamp(
                Mathf.RoundToInt(ModSettings.AdvancedForge.AffixRoll.Value),
                0,
                255
            );
        }

        [HarmonyPatch(typeof(CraftingManager), "OnMainItemChange")]
        internal static class MainItemChanged
        {
            [HarmonyPostfix]
            static void Postfix(Il2CppSystem.Object __0)
            {
                item = null;
                if (__0.IsNullOrDestroyed()) return;
                OneItemContainer container = __0.TryCast<OneItemContainer>();
                if (!container.IsNullOrDestroyed() && !container.content.IsNullOrDestroyed())
                    item = container.content.data;
            }
        }

        [HarmonyPatch(typeof(CraftingManager), "OnMainItemRemoved")]
        internal static class MainItemRemoved
        {
            [HarmonyPostfix]
            static void Postfix() => item = null;
        }

        [HarmonyPatch(typeof(CraftingSlotManager), "Awake")]
        internal static class SlotManagerAwake
        {
            [HarmonyPostfix]
            static void Postfix(CraftingSlotManager __instance) => craftingSlotManager = __instance;
        }

        // Vanilla rejects upgrades once displayed T5 is reached. Re-open capability
        // only for the T5->T6 and T6->T7 transitions.
        [HarmonyPatch(typeof(CraftingManager), "CheckForgeCapability")]
        internal static class ForgeCapability
        {
            [HarmonyPostfix]
            static void Postfix(
                CraftingManager __instance,
                ref bool __result,
                ref string __0,
                ref bool __1,
                ref bool __2,
                ref string __3)
            {
                if (!T7Enabled() || item.IsNullOrDestroyed() || IsIdol(item)) return;
                if (__0 != Scripts.Mods.Craft.Craft_Locales.affix_is_maxed) return;

                int tier = GetTier(item, __instance.appliedAffixID);
                if (tier < 4 || tier >= 6) return;

                // Infinite FP already handles the real cost. Without it, retain the
                // old mod's conservative "must be able to afford worst case" gate.
                if (!PreserveFp() && item.forgingPotential <= MaxForgeCost(tier)) return;

                if (!craftingSlotManager.IsNullOrDestroyed() && !PreserveFp())
                    craftingSlotManager.maxForgingPotentialText.text =
                        "<size=13><color=#FF0000>-" + MaxForgeCost(tier);

                __0 = "Upgrade Affix";
                __1 = false;
                __2 = false;
                __3 = "";
                __result = true;
            }
        }

        [HarmonyPatch(typeof(CraftingUpgradeButton), "UpdateButton")]
        internal static class UpgradeButton
        {
            [HarmonyPrefix]
            static void Prefix(int __0, ref bool __1)
            {
                if (!T7Enabled() || item.IsNullOrDestroyed() || IsIdol(item) || __0 < 0) return;
                int tier = GetTier(item, __0);
                if (tier < 4 || tier >= 6) return;
                if (PreserveFp() || item.forgingPotential > MaxForgeCost(tier))
                    __1 = true;
            }
        }

        // The historical implementation had to own the T5+ craft because vanilla
        // never reaches its normal upgrade path above T5.
        [HarmonyPatch(typeof(CraftingSlotManager), "Forge")]
        internal static class Forge
        {
            [HarmonyPrefix, HarmonyPriority(Priority.First)]
            static bool Prefix(CraftingSlotManager __instance)
            {
                if (!T7Enabled() || item.IsNullOrDestroyed() || IsIdol(item)) return true;

                int affixId = __instance.appliedAffixID;
                ItemAffix affix = GetAffix(item, affixId);
                if (affix.IsNullOrDestroyed() || affix.affixTier < 4 || affix.affixTier >= 6)
                    return true;

                int oldTier = affix.affixTier;
                int glyph = SupportGlyphSubtype(__instance);

                // Glyph of Despair: when guarantee is enabled and the actual glyph is
                // slotted, force the seal outcome rather than silently inventing a glyph.
                if (glyph == 3 && ModSettings.AdvancedForge.GuaranteedGlyphOfDespair.Value)
                {
                    item.SealAffix(affix);
                    item.RefreshIDAndValues();
                    return false;
                }

                // Glyph of Order (subtype 2) preserves the existing roll.
                if (glyph != 2)
                    affix.affixRoll = (byte)Random.Range(0f, 255f);

                affix.affixTier++;
                ApplyForcedRoll(affix);

                bool noCost = PreserveFp();
                if (!noCost && glyph == 0)
                {
                    // Native Hope is 25%; guarantee only changes that outcome when
                    // the player actually slotted a Hope glyph.
                    noCost = ModSettings.AdvancedForge.GuaranteedGlyphOfHope.Value
                        || Random.RandomRangeInt(0, 4) == 0;
                }

                if (!noCost)
                    item.forgingPotential -= (byte)Random.RandomRangeInt(1, MaxForgeCost(oldTier));

                item.RefreshIDAndValues();
                return false;
            }
        }
    }
}
