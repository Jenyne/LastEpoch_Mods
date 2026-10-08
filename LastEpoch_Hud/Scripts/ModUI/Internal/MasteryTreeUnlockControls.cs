using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace LastEpoch_Hud.Scripts.ModUI;

internal static class MasteryTreeUnlockControls
{
    public static void Bind(GameObject viewport)
    {
        if (
            viewport.IsNullOrDestroyed()
            || Prefab.Child(viewport, "UnlockOtherMasteryTrees") != null
        )
            return;
        var original = Prefab.Child(viewport, "RemoveNodeRequirements");
        if (original.IsNullOrDestroyed())
            return;
        var row = Object.Instantiate(original, original.transform.parent);
        row.name = "UnlockOtherMasteryTrees";
        row.transform.SetSiblingIndex(original.transform.GetSiblingIndex() + 1);
        var toggle = row.GetComponentInChildren<Toggle>(true);
        if (toggle.IsNullOrDestroyed())
        {
            Object.Destroy(row);
            return;
        }
        toggle.gameObject.name = "Toggle_UnlockOtherMasteryTrees";
        toggle.group = null;
        toggle.interactable = true;
        toggle.onValueChanged.RemoveAllListeners();
        var labelObject = Prefab.Child(toggle.gameObject, "Label");
        if (!labelObject.IsNullOrDestroyed())
            Prefab.ApplyLabel(labelObject.GetComponent<Text>(), "Unlock Other Mastery Trees");
        toggle.SetIsOnWithoutNotify(ModSettings.MasteryTreeUnlock.Enabled.Value);
        ModSettings.MasteryTreeUnlock.Enabled.Changed += enabled =>
        {
            if (!toggle.IsNullOrDestroyed())
                toggle.SetIsOnWithoutNotify(enabled);
            Mods.Skills.Passives_MasteryLock.Sync();
        };
        LayoutRebuilder.MarkLayoutForRebuild(viewport.GetComponent<RectTransform>());
    }

    [HarmonyPatch(typeof(Toggle), "OnPointerClick")]
    private static class Click
    {
        [HarmonyPostfix]
        private static void Postfix(Toggle __instance)
        {
            if (
                !__instance.IsNullOrDestroyed()
                && __instance.interactable
                && __instance.gameObject.name == "Toggle_UnlockOtherMasteryTrees"
            )
                ModSettings.MasteryTreeUnlock.Enabled.Set(__instance.isOn);
        }
    }
}
