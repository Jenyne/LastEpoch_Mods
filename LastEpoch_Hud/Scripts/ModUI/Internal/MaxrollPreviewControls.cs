using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using HarmonyLib;
using Il2CppTMPro;
using LastEpoch_Hud.Scripts.Core.BuildImport;
using UnityEngine;
using UnityEngine.UI;

namespace LastEpoch_Hud.Scripts.ModUI;

// A read-only overlay. It never constructs ItemData, changes inventories, or selects Force Drop data.
internal static class MaxrollPreviewControls
{
    const int ItemRows = 10,
        DetailRows = 8;
    static readonly Color Gold = new(.96f, .81f, .48f);
    static readonly Color Dark = new(.10f, .12f, .15f);
    static readonly Color Purple = new(.78f, .55f, .98f);
    static readonly MaxrollPreviewSession Session = new();
    static readonly Dictionary<int, Action> Clicks = new();
    static readonly List<Button> Items = new();
    static readonly List<Text> Details = new();
    static readonly List<MaxrollPlacement> Placements = new();
    static readonly List<(string Text, bool Corruption)> Fields = new();
    static readonly string[] Sections = { "equipment", "idols", "blessings", "weaverItems" };
    static readonly string[] SectionLabels = { "Equipment", "Idols", "Blessings", "Weaver Items" };
    static readonly List<Button> SectionButtons = new();
    static GameObject root;
    static Font font;
    static TMP_InputField input;
    static Text status,
        variant,
        itemTitle,
        itemPageLabel,
        detailPageLabel;
    static Button load,
        cancel,
        previousVariant,
        nextVariant,
        copy,
        issuesButton;
    static Button previousItems,
        nextItems,
        previousDetails,
        nextDetails;
    static Dictionary<string, string> dictionary;
    static string nativeLocale,
        message = "Paste a public Maxroll planner link.",
        error;
    static int section,
        itemPage,
        detailPage;
    static bool showIssues;
    static MaxrollPlacement selected;

    public static void Open(Font menuFont, TMP_InputField template)
    {
        if (root.IsNullOrDestroyed())
        {
            font = menuFont;
            try
            {
                Build(template);
            }
            catch
            {
                if (!root.IsNullOrDestroyed())
                    UnityEngine.Object.Destroy(root);
                root = null;
                Clicks.Clear();
                throw;
            }
        }
        root.SetActive(true);
        root.transform.SetAsLastSibling();
        Render();
    }

    public static void Tick(bool hudVisible)
    {
        if (root.IsNullOrDestroyed() || !hudVisible)
        {
            Close();
            return;
        }
        if (!root.activeSelf)
            return;
        if (Session.Poll())
        {
            selected = null;
            itemPage = detailPage = 0;
            showIssues = false;
            error = Session.Error;
            message = Session.Build == null ? "Could not load this build." : "Build loaded.";
            Render();
        }
        if (
            !ReferenceEquals(dictionary, Locales.current_dictionary)
            || nativeLocale != NativeItemNames.Locale
        )
            Render();
    }

    static void Close()
    {
        Session.Cancel();
        if (!root.IsNullOrDestroyed())
            root.SetActive(false);
        if (message == "Loading build...")
            message = "Loading cancelled.";
    }

    static void Load()
    {
        selected = null;
        showIssues = false;
        itemPage = detailPage = 0;
        error = null;
        try
        {
            Session.Load(input.text);
            message = "Loading build...";
        }
        catch (Exception ex)
        {
            error = ex.Message;
            message = "Could not load this build.";
        }
        Render();
    }

    static void Build(TMP_InputField template)
    {
        Clicks.Clear();
        Items.Clear();
        Details.Clear();
        SectionButtons.Clear();
        root = Panel(Hud_Manager.Content.content_obj, "MaxrollBuildPreview", 0, 0, 1, 1);
        var layout = root.AddComponent<LayoutElement>();
        layout.ignoreLayout = true;
        var title = Label(root, .02f, .944f, .81f, .99f, 22);
        LocaleRegistry.Apply(title, "Maxroll Build Preview");
        var separator = Panel(root, "TitleSeparator", .02f, .936f, .98f, .939f);
        separator.GetComponent<Image>().color = Gold;
        separator.GetComponent<Image>().raycastTarget = false;
        Button(root, "Close", .85f, .947f, .98f, .99f, Close);
        input = UnityEngine
            .Object.Instantiate(template.gameObject, root.transform)
            .GetComponent<TMP_InputField>();
        input.gameObject.name = "MaxrollPlannerLink";
        Rect(input.gameObject, .02f, .87f, .70f, .925f);
        var inputLayout = input.GetComponent<LayoutElement>();
        if (!inputLayout.IsNullOrDestroyed())
            inputLayout.ignoreLayout = true;
        input.onValueChanged.RemoveAllListeners();
        input.onEndEdit.RemoveAllListeners();
        input.contentType = TMP_InputField.ContentType.Standard;
        input.lineType = TMP_InputField.LineType.SingleLine;
        input.characterLimit = 2048;
        input.enabled = input.interactable = true;
        input.readOnly = false;
        input.SetTextWithoutNotify("");
        input.targetGraphic.raycastTarget = true;
        input.textComponent.color = Gold;
        input.textComponent.richText = false;
        input.textComponent.enableAutoSizing = true;
        input.textComponent.fontSizeMin = 10;
        input.textComponent.fontSizeMax = 16;
        input.textComponent.margin = Vector4.zero;
        if (!input.textViewport.IsNullOrDestroyed())
            Rect(input.textViewport.gameObject, .02f, .05f, .98f, .95f);
        Rect(input.textComponent.gameObject, 0, 0, 1, 1);
        if (!input.placeholder.IsNullOrDestroyed())
            input.placeholder.gameObject.SetActive(false);
        var inputGroup = input.GetComponent<CanvasGroup>();
        if (!inputGroup.IsNullOrDestroyed())
        {
            inputGroup.alpha = 1;
            inputGroup.interactable = inputGroup.blocksRaycasts = true;
        }
        input.gameObject.SetActive(true);
        load = Button(root, "Load Build", .71f, .87f, .84f, .925f, Load);
        cancel = Button(
            root,
            "Cancel",
            .85f,
            .87f,
            .98f,
            .925f,
            () =>
            {
                Session.Cancel();
                message = "Loading cancelled.";
                error = null;
                Render();
            }
        );
        status = Label(root, .02f, .803f, .98f, .86f, 14);
        variant = Label(root, .02f, .746f, .69f, .794f, 16);
        previousVariant = Button(
            root,
            "Previous",
            .71f,
            .749f,
            .84f,
            .793f,
            () => ChangeVariant(-1)
        );
        nextVariant = Button(root, "Next", .85f, .749f, .98f, .793f, () => ChangeVariant(1));
        var left = Panel(root, "BuildItems", .02f, .062f, .40f, .731f);
        var right = Panel(root, "ItemDetails", .41f, .062f, .98f, .731f);
        for (int i = 0; i < Sections.Length; i++)
        {
            int choice = i;
            float x = .02f + i * .245f;
            SectionButtons.Add(
                Button(
                    left,
                    SectionLabels[i],
                    x,
                    .919f,
                    x + .235f,
                    .983f,
                    () =>
                    {
                        section = choice;
                        selected = null;
                        itemPage = detailPage = 0;
                        showIssues = false;
                        Render();
                    }
                )
            );
        }
        for (int i = 0; i < ItemRows; i++)
        {
            int row = i;
            float top = .896f - i * .077f;
            Items.Add(
                Button(
                    left,
                    "",
                    .02f,
                    top - .07f,
                    .98f,
                    top,
                    () =>
                    {
                        int index = itemPage * ItemRows + row;
                        if (index < Placements.Count)
                        {
                            selected = Placements[index];
                            detailPage = 0;
                            showIssues = false;
                            Render();
                        }
                    }
                )
            );
        }
        previousItems = Button(
            left,
            "Previous",
            .02f,
            .019f,
            .32f,
            .095f,
            () =>
            {
                itemPage = Math.Max(0, itemPage - 1);
                Render();
            }
        );
        nextItems = Button(
            left,
            "Next",
            .68f,
            .019f,
            .98f,
            .095f,
            () =>
            {
                itemPage++;
                Render();
            }
        );
        itemPageLabel = Label(left, .33f, .02f, .67f, .095f, 13);
        itemPageLabel.alignment = TextAnchor.MiddleCenter;
        itemTitle = Label(right, .025f, .884f, .975f, .98f, 17);
        copy = Button(
            right,
            "Copy Item JSON",
            .025f,
            .799f,
            .48f,
            .87f,
            () =>
            {
                if (selected?.Item != null)
                    GUIUtility.systemCopyBuffer = selected.Item.Value.GetRawText();
            }
        );
        issuesButton = Button(
            right,
            "Build Issues",
            .51f,
            .799f,
            .975f,
            .87f,
            () =>
            {
                showIssues = !showIssues;
                detailPage = 0;
                Render();
            }
        );
        for (int i = 0; i < DetailRows; i++)
        {
            float top = .776f - i * .081f;
            Details.Add(Label(right, .025f, top - .075f, .975f, top, 13));
        }
        previousDetails = Button(
            right,
            "Previous",
            .025f,
            .019f,
            .32f,
            .095f,
            () =>
            {
                detailPage = Math.Max(0, detailPage - 1);
                Render();
            }
        );
        nextDetails = Button(
            right,
            "Next",
            .68f,
            .019f,
            .975f,
            .095f,
            () =>
            {
                detailPage++;
                Render();
            }
        );
        detailPageLabel = Label(right, .33f, .02f, .67f, .095f, 13);
        detailPageLabel.alignment = TextAnchor.MiddleCenter;
        var note = Label(root, .02f, .008f, .98f, .053f, 13);
        LocaleRegistry.Apply(
            note,
            "Preview only. Item creation will be added after game validation."
        );
    }

    static void ChangeVariant(int direction)
    {
        var build = Session.Build;
        if (build == null)
            return;
        int index = build.SelectedVariantIndex.HasValue
            ? build.SelectedVariantIndex.Value + direction
            : 0;
        build.SelectVariant(Math.Max(0, Math.Min(build.Variants.Count - 1, index)));
        selected = null;
        itemPage = detailPage = 0;
        showIssues = false;
        Render();
    }

    static void Render()
    {
        dictionary = Locales.current_dictionary;
        nativeLocale = NativeItemNames.Locale;
        var build = Session.Build;
        var gear = build?.SelectedVariant;
        load.interactable = !Session.IsLoading;
        cancel.interactable = Session.IsLoading;
        previousVariant.interactable =
            build != null
            && (!build.SelectedVariantIndex.HasValue || build.SelectedVariantIndex.Value > 0);
        nextVariant.interactable =
            build != null
            && (
                !build.SelectedVariantIndex.HasValue
                || build.SelectedVariantIndex.Value < build.Variants.Count - 1
            );
        status.text = L(message) + (string.IsNullOrEmpty(error) ? "" : " " + Short(error, 240));
        if (build != null)
        {
            status.text = Short(build.Name, 90) + " — " + L(message);
            if (!string.IsNullOrEmpty(build.SelectionIssue))
                status.text += " " + L("Choose a gear variant.");
        }
        variant.text =
            gear == null
                ? L("Choose a gear variant.")
                : F(
                    "Gear variant {0}",
                    (build.SelectedVariantIndex.Value + 1) + "/" + build.Variants.Count
                )
                    + ": "
                    + Short(gear.Name, 110);
        Placements.Clear();
        if (gear != null)
            foreach (var placement in gear.Placements)
                if (placement.Section == Sections[section])
                    Placements.Add(placement);
        itemPage = Math.Min(itemPage, Math.Max(0, (Placements.Count - 1) / ItemRows));
        for (int i = 0; i < Sections.Length; i++)
            SectionButtons[i].GetComponent<Image>().color =
                i == section ? new Color(.28f, .23f, .13f) : Dark;
        for (int i = 0; i < Items.Count; i++)
        {
            int index = itemPage * ItemRows + i;
            var button = Items[i];
            button.gameObject.SetActive(index < Placements.Count);
            if (index >= Placements.Count)
                continue;
            var placement = Placements[index];
            button.GetComponentInChildren<Text>(true).text =
                Position(placement) + "\n" + ItemName(placement);
            button.GetComponent<Image>().color = ReferenceEquals(selected, placement)
                ? new Color(.28f, .23f, .13f)
                : Dark;
        }
        previousItems.interactable = itemPage > 0;
        nextItems.interactable = (itemPage + 1) * ItemRows < Placements.Count;
        itemPageLabel.text =
            (itemPage + 1) + "/" + Math.Max(1, (Placements.Count + ItemRows - 1) / ItemRows);
        copy.interactable = selected?.Item != null;
        issuesButton.interactable = build != null;
        LocaleRegistry.Apply(
            issuesButton.GetComponentInChildren<Text>(true),
            showIssues ? "Item Details" : "Build Issues"
        );
        itemTitle.text =
            showIssues ? L("Build Issues")
            : selected == null ? L("Select an item to inspect.")
            : Position(selected) + " — " + ItemName(selected);
        Fields.Clear();
        if (showIssues)
        {
            if (!string.IsNullOrEmpty(build?.SelectionIssue))
                Fields.Add((build.SelectionIssue, false));
            if (gear != null)
                foreach (string issue in gear.Issues)
                    Fields.Add((issue, false));
            if (Fields.Count == 0)
                Fields.Add((L("No reader issues. Game legality has not been checked."), false));
        }
        else if (selected?.Item != null)
            Describe(selected.Item.Value);
        else if (selected != null)
            Fields.Add(
                (
                    selected.IsEmpty
                        ? L("Empty slot")
                        : L("Unresolved item reference") + ": " + selected.Reference,
                    false
                )
            );
        detailPage = Math.Min(detailPage, Math.Max(0, (Fields.Count - 1) / DetailRows));
        for (int i = 0; i < Details.Count; i++)
        {
            int index = detailPage * DetailRows + i;
            Details[i].text = index < Fields.Count ? Short(Fields[index].Text, 600) : "";
            Details[i].color = index < Fields.Count && Fields[index].Corruption ? Purple : Gold;
        }
        previousDetails.interactable = detailPage > 0;
        nextDetails.interactable = (detailPage + 1) * DetailRows < Fields.Count;
        detailPageLabel.text =
            (detailPage + 1) + "/" + Math.Max(1, (Fields.Count + DetailRows - 1) / DetailRows);
    }

    static void Describe(JsonElement item)
    {
        foreach (var property in item.EnumerateObject())
        {
            bool corruption =
                property.Name == "corruptedAffix" || property.Name == "corruptedAffixes";
            string caption = property.Name switch
            {
                "affixes" => "Normal affix",
                "sealedAffix" => "Sealed affix",
                "primordialAffix" => "Primordial affix",
                "corruptedAffix" or "corruptedAffixes" => "Corrupted affix",
                "priorityAffixes" => "Priority affix",
                "itemType" => "Base type ID",
                "subType" => "Subtype ID",
                "uniqueID" => "Unique ID",
                "implicits" => "Implicit rolls",
                "uniqueRolls" => "Unique rolls",
                _ => property.Name,
            };
            if (
                property.Value.ValueKind == JsonValueKind.Array
                && (
                    property.Name == "affixes"
                    || property.Name == "corruptedAffixes"
                    || property.Name == "priorityAffixes"
                )
            )
            {
                int i = 0;
                foreach (var affix in property.Value.EnumerateArray())
                    Fields.Add((L(caption) + " " + (++i) + ": " + AffixValue(affix), corruption));
                if (i == 0)
                    Fields.Add((L(caption) + ": []", corruption));
            }
            else
                Fields.Add(
                    (
                        L(caption)
                            + ": "
                            + (
                                caption.EndsWith("affix", StringComparison.Ordinal)
                                    ? AffixValue(property.Value)
                                    : property.Value.GetRawText()
                            ),
                        corruption
                    )
                );
        }
    }

    static string AffixValue(JsonElement value)
    {
        string raw = value.GetRawText();
        int? id = Integer(value, "id");
        if (!id.HasValue)
            return raw;
        string fallback = F("Affix ID {0}", id.Value);
        string name = fallback;
        try
        {
            var catalog = Il2Cpp.AffixList.get();
            if (!catalog.IsNullOrDestroyed())
            {
                foreach (var affix in catalog.singleAffixes)
                    if (affix.affixId == id.Value)
                    {
                        name = NativeItemNames.AffixName(affix);
                        break;
                    }
                if (name == fallback)
                    foreach (var affix in catalog.multiAffixes)
                        if (affix.affixId == id.Value)
                        {
                            name = NativeItemNames.AffixName(affix);
                            break;
                        }
            }
        }
        catch { }
        return name + "\n" + raw;
    }

    static string ItemName(MaxrollPlacement placement)
    {
        if (placement.IsEmpty)
            return L("Empty slot");
        if (!placement.Item.HasValue)
            return L("Unresolved item reference") + " " + placement.Reference;
        var item = placement.Item.Value;
        int? unique = Integer(item, "uniqueID");
        if (unique.HasValue)
        {
            string fallback = F("Unique ID {0}", unique.Value);
            try
            {
                var catalog = Il2Cpp.UniqueList.instance;
                if (!catalog.IsNullOrDestroyed())
                    foreach (var entry in catalog.uniques)
                        if (entry.uniqueID == unique.Value)
                        {
                            string name = Il2Cpp.Localization.Items.GetUniqueName(
                                entry.uniqueID,
                                true,
                                false
                            );
                            return string.IsNullOrWhiteSpace(name)
                                ? entry.displayName ?? fallback
                                : name;
                        }
            }
            catch { }
            return fallback;
        }
        int? type = Integer(item, "itemType"),
            subtype = Integer(item, "subType");
        if (type.HasValue && subtype.HasValue)
        {
            string fallback = F("Base item {0}", type.Value + "/" + subtype.Value);
            try
            {
                var catalog = Il2Cpp.ItemList.get();
                if (!catalog.IsNullOrDestroyed())
                {
                    foreach (var category in catalog.EquippableItems)
                        if (category.baseTypeID == type.Value)
                            foreach (var entry in category.subItems)
                                if (entry.subTypeID == subtype.Value)
                                    return BaseName(
                                        type.Value,
                                        subtype.Value,
                                        entry.displayName ?? fallback
                                    );
                    foreach (var category in catalog.nonEquippableItems)
                        if (category.baseTypeID == type.Value)
                            foreach (var entry in category.subItems)
                                if (entry.subTypeID == subtype.Value)
                                    return BaseName(
                                        type.Value,
                                        subtype.Value,
                                        entry.displayName ?? fallback
                                    );
                }
            }
            catch { }
            return fallback;
        }
        return L("Unknown item") + " " + placement.Reference;
    }

    static string BaseName(int type, int subtype, string fallback)
    {
        string name = Il2Cpp.Localization.Items.GetSubTypeName(type, subtype);
        return string.IsNullOrWhiteSpace(name) ? fallback : name;
    }

    static int? Integer(JsonElement value, string key) =>
        value.ValueKind == JsonValueKind.Object
        && value.TryGetProperty(key, out var field)
        && field.ValueKind == JsonValueKind.Number
        && field.TryGetInt32(out int number)
            ? number
            : null;

    static string L(string key) => LocaleRegistry.Translate(key);

    static string F(string key, object value) =>
        string.Format(CultureInfo.InvariantCulture, L(key), value);

    static string Short(string text, int limit) =>
        text == null ? ""
        : text.Length > limit ? text.Substring(0, limit) + "…"
        : text;

    static string Position(MaxrollPlacement placement) =>
        placement.GridIndex.HasValue
            ? L(SectionLabels[section]) + " " + (placement.GridIndex.Value + 1)
            : L(
                placement.Slot switch
                {
                    "head" => "Helmet",
                    "body" => "Body Armour",
                    "weapon" => "Weapon",
                    "offhand" => "Off-hand",
                    "hands" => "Gloves",
                    "waist" => "Belt",
                    "feet" => "Boots",
                    "finger1" => "Ring 1",
                    "finger2" => "Ring 2",
                    "neck" => "Amulet",
                    "relic" => "Relic",
                    "altar" => "Idol Altar",
                    _ => placement.Slot,
                }
            );

    static GameObject Panel(GameObject parent, string name, float x0, float y0, float x1, float y1)
    {
        var node = new GameObject(name);
        node.layer = parent.layer;
        node.AddComponent<RectTransform>().SetParent(parent.transform, false);
        Rect(node, x0, y0, x1, y1);
        node.AddComponent<Image>().color = Dark;
        var outline = node.AddComponent<Outline>();
        outline.effectColor = new Color(Gold.r, Gold.g, Gold.b, .65f);
        outline.effectDistance = new Vector2(1, -1);
        return node;
    }

    static void Rect(GameObject node, float x0, float y0, float x1, float y1)
    {
        var rect = node.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(x0, y0);
        rect.anchorMax = new Vector2(x1, y1);
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        rect.localScale = Vector3.one;
    }

    static Text Label(GameObject parent, float x0, float y0, float x1, float y1, int size)
    {
        var node = new GameObject("Label");
        node.layer = parent.layer;
        node.AddComponent<RectTransform>().SetParent(parent.transform, false);
        Rect(node, x0, y0, x1, y1);
        var text = node.AddComponent<Text>();
        text.font = font;
        text.fontSize = size;
        text.color = Gold;
        text.supportRichText = false;
        text.raycastTarget = false;
        text.alignment = TextAnchor.UpperLeft;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Truncate;
        return text;
    }

    static Button Button(
        GameObject parent,
        string caption,
        float x0,
        float y0,
        float x1,
        float y1,
        Action action
    )
    {
        var node = Panel(parent, "PreviewButton", x0, y0, x1, y1);
        var button = node.AddComponent<Button>();
        button.targetGraphic = node.GetComponent<Image>();
        var label = Label(node, .025f, .04f, .975f, .96f, 14);
        label.alignment = TextAnchor.MiddleLeft;
        label.resizeTextForBestFit = true;
        label.resizeTextMinSize = 10;
        label.resizeTextMaxSize = 14;
        LocaleRegistry.Apply(label, caption);
        Clicks.Add(button.GetInstanceID(), action);
        return button;
    }

    [HarmonyPatch(typeof(Button), "Press")]
    public class ButtonPress
    {
        [HarmonyPostfix]
        static void Postfix(Button __instance)
        {
            if (
                !__instance.interactable
                || !__instance.gameObject.activeInHierarchy
                || !Clicks.TryGetValue(__instance.GetInstanceID(), out var action)
            )
                return;
            try
            {
                action();
            }
            catch (Exception ex)
            {
                Main.logger_instance.Error("Maxroll preview: " + ex.Message);
                Session.Cancel();
            }
        }
    }
}
