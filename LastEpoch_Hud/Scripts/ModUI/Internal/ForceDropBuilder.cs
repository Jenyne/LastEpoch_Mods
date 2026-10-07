using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using HarmonyLib;
using Il2Cpp;
using Il2CppTMPro;
using LastEpoch_Hud.Scripts.Core.ForceDrop;
using UnityEngine;
using UnityEngine.UI;
using FD = LastEpoch_Hud.Scripts.Hud_Manager.Content.OdlForceDrop;

namespace LastEpoch_Hud.Scripts.ModUI;

// A new view over the existing catalog and item creation code. No asset bundle rebuild.
public static class ForceDropBuilder
{
    static readonly Color gold = new Color(0.96f, 0.81f, 0.48f);
    static readonly Color dark = new Color(0.10f, 0.12f, 0.15f);
    static readonly Dictionary<int, Action> clicks = new Dictionary<int, Action>();
    static GameObject root,
        basePage,
        affixPage,
        uniquePage,
        ragePage,
        picker;
    static TMP_InputField template,
        search,
        pickerSearch;
    static bool categoryPicker,
        rarityPicker;
    static readonly List<Choice> visiblePicks = new List<Choice>();
    static readonly List<Text> pickerHeaders = new List<Text>();
    static readonly string[] groups = { "Weapons", "Armour", "Accessories", "Idols", "Other" };
    static Font font;
    static Text preview,
        status,
        pickerTitle;
    static Button typeButton,
        rarityButton,
        dropButton,
        corruptButton,
        corruptionSelect,
        pickerPrevious,
        pickerNext;
    static int corruptionId = -1;
    static readonly int[] variantIds = { -1, -1 };
    static readonly string[] variantNames = { "Choose modifier 1", "Choose modifier 2" };
    static readonly Button[] variantSelect = new Button[2];
    static Text variantHeader,
        variantDescription;
    static ushort rageUniqueId;
    static int rageMetadataId = -1;

    static string corruptionName = "None";
    static Number corruptionTier,
        corruptionRoll;
    static readonly List<Button> itemButtons = new List<Button>();
    static readonly List<Button> pickButtons = new List<Button>();
    static readonly List<Choice> choices = new List<Choice>();
    static readonly List<Choice> filtered = new List<Choice>();
    static readonly List<int> itemIndexes = new List<int>();
    static readonly List<Number> numbers = new List<Number>();
    static readonly AffixRow[] rows = new AffixRow[5];
    static Number forging,
        quantity,
        lp,
        ww;
    static readonly Number[] implicits = new Number[3],
        uniqueRolls = new Number[8];
    static int itemPage,
        pickerPage;
    static string lastSearch = "",
        lastPickerSearch = "",
        lastItems = "";
    static string lastNativeLocale;
    static object lastModLocale;
    static int lastCategoryCount;
    static Dictionary<int, NativeItemNames.Category> nativeCategories =
        new Dictionary<int, NativeItemNames.Category>();
    static Dictionary<int, NativeItemNames.ItemChoice> nativeItems =
        new Dictionary<int, NativeItemNames.ItemChoice>();
    static bool corrupted,
        failed,
        metadataLogged;
    static float nextCorruptionCheck;
    static string result = "";
    public static bool IsReady => !root.IsNullOrDestroyed();

    sealed class Choice
    {
        public int id;
        public int group;
        public string name;
        public string aliases;
        public Action select;
    }

    sealed class Number
    {
        public TMP_InputField input;
        public GameObject group;
        public int min,
            max,
            value;
        public bool random;
        public Button mode;

        public void Read()
        {
            if (
                int.TryParse(
                    input.text,
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    out int n
                )
            )
                value = Math.Max(min, Math.Min(max, n));
            if (!input.isFocused)
                input.SetTextWithoutNotify(value.ToString(CultureInfo.InvariantCulture));
        }
    }

    sealed class AffixRow
    {
        public int id = -1;
        public string name = "None";
        public Button select;
        public Number tier,
            roll;
    }

    public static bool Tick()
    {
        if (!IsReady && !failed)
        {
            try
            {
                Build();
            }
            catch (Exception ex)
            {
                failed = true;
                if (!root.IsNullOrDestroyed())
                    UnityEngine.Object.Destroy(root);
                root = null;
                if (!FD.content_obj.IsNullOrDestroyed())
                    foreach (var child in Functions.GetAllChild(FD.content_obj))
                        child.SetActive(true);
                Main.logger_instance.Error("Force Drop builder setup: " + ex.Message);
            }
        }
        if (!IsReady)
            return false;
        RefreshNativeLocale();
        RefreshTierLimits();
        foreach (var n in numbers)
            n.Read();
        string signature =
            FD.item_type + ":" + FD.item_rarity + ":" + FD.items_dropdown.options.Count;
        if (lastSearch != search.text || lastItems != signature)
        {
            lastSearch = search.text;
            lastItems = signature;
            itemPage = 0;
            RefreshItems();
        }
        if (picker.activeSelf && lastPickerSearch != pickerSearch.text)
        {
            lastPickerSearch = pickerSearch.text;
            pickerPage = 0;
            RefreshPicker();
        }
        Caption(typeButton, SelectedCategoryName("Choose category"));
        Caption(
            rarityButton,
            NativeItemNames.RarityName(Selected(FD.rarity_dropdown, "Choose rarity"))
        );
        lp.group.SetActive(
            FD.item_rarity > 6
                && FD.item_legendary_type == UniqueList.LegendaryType.LegendaryPotential
        );
        ww.group.SetActive(
            FD.item_rarity > 6
                && FD.item_legendary_type != UniqueList.LegendaryType.LegendaryPotential
        );
        uniquePage.SetActive(FD.item_rarity > 6);
        var rageEntry = SelectedRageEntry();
        int variantCount = UniqueVariantAdapter.VariantCount(rageEntry);
        bool showRage = variantCount > 0;
        if (rageUniqueId != FD.item_unique_id)
        {
            ResetRage();
            rageUniqueId = (ushort)FD.item_unique_id;
        }
        ragePage.SetActive(showRage);
        if (showRage && rageMetadataId != FD.item_unique_id)
        {
            rageMetadataId = FD.item_unique_id;
            Main.logger_instance.Msg(
                "Unique variant native pool: unique="
                    + rageEntry.uniqueID
                    + ", specific="
                    + rageEntry.dropsSpecificLegendaryAffixes
                    + ", count="
                    + rageEntry.droppableLegendaryAffixCount
                    + ", excludesSlotLimits="
                    + rageEntry.excludeSpecificAffixesFromPrefixSuffixLimits
                    + ", pool="
                    + (
                        rageEntry.droppableLegendaryAffixes.IsNullOrDestroyed()
                            ? "null"
                            : rageEntry.droppableLegendaryAffixes.Count.ToString()
                    )
                    + ", resolved="
                    + UniqueVariantAdapter.Catalog(rageEntry).Count
            );
        }
        // Keep the exclusive modifier next to the item preview and outside the affix grid.
        bool twoVariants = variantCount == 2;
        Rect(ragePage, .04f, .28f, .96f, twoVariants ? .54f : .45f);
        LocaleRegistry.Apply(
            variantHeader,
            twoVariants ? "Exclusive glove modifiers" : "Unsated Rage modifier"
        );
        LocaleRegistry.Apply(
            variantDescription,
            twoVariants
                ? "Two exclusive modifiers · separate from LP"
                : "Exclusive ring modifier · separate from LP"
        );
        Rect(variantHeader.gameObject, .03f, twoVariants ? .78f : .72f, .97f, .97f);
        Rect(variantDescription.gameObject, .03f, .02f, .97f, twoVariants ? .22f : .25f);
        for (int slot = 0; slot < variantSelect.Length; slot++)
        {
            variantSelect[slot].gameObject.SetActive(slot < variantCount);
            variantSelect[slot].interactable = UniqueVariantAdapter.HasVariants(rageEntry);
            Caption(
                variantSelect[slot],
                variantSelect[slot].interactable
                    ? variantNames[slot]
                    : "Unique modifier pool unavailable"
            );
        }
        Rect(
            variantSelect[0].gameObject,
            .03f,
            twoVariants ? .51f : .29f,
            .97f,
            twoVariants ? .75f : .69f
        );
        Rect(preview.gameObject, .04f, showRage ? (twoVariants ? .56f : .47f) : .28f, .96f, .90f);
        if (Time.unscaledTime >= nextCorruptionCheck)
        {
            nextCorruptionCheck = Time.unscaledTime + 2f;
            corruptionSelect.interactable = CorruptedAffixAdapter.IsSupported;
            if (corruptionSelect.interactable && corruptionId < 0)
                Caption(corruptionSelect, "Corrupted affix: None");
        }
        forging.input.interactable = !corrupted && !forging.random;
        if (forging.mode != null)
            forging.mode.interactable = !corrupted;
        lp.input.interactable = !corrupted && !lp.random;
        ww.input.interactable = !corrupted && !ww.random;
        if (lp.mode != null)
            lp.mode.interactable = !corrupted;
        if (ww.mode != null)
            ww.mode.interactable = !corrupted;
        RefreshPreview();
        return true;
    }

    static void Build()
    {
        if (!FD.Type_Initialized || FD.content_obj.IsNullOrDestroyed())
            return;
        foreach (
            var candidate in Hud_Manager.hud_object.GetComponentsInChildren<TMP_InputField>(true)
        )
            if (
                candidate.name == "InputField"
                && candidate.transform.parent != null
                && candidate.transform.parent.name == "Name"
            )
            {
                template = candidate;
                break;
            }
        if (template.IsNullOrDestroyed())
            throw new InvalidOperationException("Input template is unavailable");
        var texts = FD.content_obj.GetComponentsInChildren<Text>(true);
        foreach (var t in texts)
            if (!t.font.IsNullOrDestroyed())
            {
                font = t.font;
                break;
            }
        if (font.IsNullOrDestroyed())
            throw new InvalidOperationException("Menu font is unavailable");
        clicks.Clear();
        numbers.Clear();
        itemButtons.Clear();
        pickButtons.Clear();
        pickerHeaders.Clear();
        root = Panel(FD.content_obj, "ForceDropBuilder", 0, 0, 1, 1);
        Label(root, "Force Drop", 0.02f, 0.955f, 0.98f, 0.995f, 22);
        var left = Panel(root, "Choose item", 0.01f, 0.02f, 0.29f, 0.945f);
        var middle = Panel(root, "Customize", 0.30f, 0.02f, 0.73f, 0.945f);
        var right = Panel(root, "Preview", 0.74f, 0.02f, 0.99f, 0.945f);
        Label(left, "Choose item", .03f, .95f, .97f, .99f, 18);
        Label(left, "Search items", .03f, .90f, .97f, .94f);
        search = Input(left, "Item search", .03f, .84f, .97f, .89f, "", false);
        typeButton = Button(
            left,
            "Choose category",
            .03f,
            .77f,
            .97f,
            .825f,
            () => CatalogPicker(FD.type_dropdown, FD.SelectType, true)
        );
        rarityButton = Button(
            left,
            "Choose rarity",
            .03f,
            .705f,
            .97f,
            .76f,
            () => CatalogPicker(FD.rarity_dropdown, FD.SelectRarity, true)
        );
        for (int i = 0; i < 12; i++)
        {
            int slot = i;
            itemButtons.Add(
                Button(
                    left,
                    "",
                    .03f,
                    .65f - i * .045f,
                    .97f,
                    .69f - i * .045f,
                    () => ChooseItem(slot)
                )
            );
        }
        Button(
            left,
            "Previous",
            .03f,
            .035f,
            .48f,
            .09f,
            () =>
            {
                itemPage = Math.Max(0, itemPage - 1);
                RefreshItems();
            }
        );
        Button(
            left,
            "Next",
            .52f,
            .035f,
            .97f,
            .09f,
            () =>
            {
                if ((itemPage + 1) * 12 < itemIndexes.Count)
                    itemPage++;
                RefreshItems();
            }
        );
        Label(middle, "Customize", .03f, .955f, .97f, .99f, 18);
        Button(middle, "Random", .03f, .905f, .32f, .948f, () => Preset(true));
        Button(middle, "Maximum", .35f, .905f, .64f, .948f, () => Preset(false));
        Button(
            middle,
            "Custom",
            .67f,
            .905f,
            .97f,
            .948f,
            () =>
            {
                foreach (var n in numbers)
                    n.random = false;
                RefreshModes();
            }
        );
        basePage = Panel(middle, "Base properties", .02f, .735f, .98f, .895f);
        forging = NumericGrid(basePage, "Forging potential", 0, 1, 0, 255, 100, false);
        for (int i = 0; i < 3; i++)
            implicits[i] = NumericGrid(
                basePage,
                "Implicit " + (i + 1),
                (i + 1) % 2,
                i < 1 ? 1 : 0,
                0,
                100,
                100,
                true
            );
        affixPage = Panel(middle, "Affixes", .02f, .345f, .98f, .725f);
        Label(affixPage, "Affix", .03f, .93f, .55f, .99f, 13);
        Label(affixPage, "Tier", .59f, .93f, .72f, .99f, 13);
        Label(affixPage, "Roll %", .75f, .93f, .88f, .99f, 13);
        for (int i = 0; i < 5; i++)
        {
            int index = i;
            float y = .79f - i * .17f;
            var row = new AffixRow();
            rows[i] = row;
            string slot =
                i == 4 ? "Sealed"
                : i < 2 ? "Prefix " + (i + 1)
                : "Suffix " + (i - 1);
            Label(affixPage, slot, .03f, y, .17f, y + .12f, 12);
            row.select = Button(
                affixPage,
                "None",
                .18f,
                y,
                .57f,
                y + .13f,
                () => AffixPicker(index)
            );
            row.tier = NumericCompact(affixPage, .59f, y, .72f, y + .13f, 1, 7, 7);
            row.roll = NumericCompact(affixPage, .75f, y, .86f, y + .13f, 0, 100, 100);
            row.roll.mode = Button(
                affixPage,
                "Fixed",
                .88f,
                y,
                .98f,
                y + .13f,
                () =>
                {
                    row.roll.random = !row.roll.random;
                    RefreshModes();
                }
            );
        }
        var corruptionPanel = Panel(middle, "Corruption", .02f, .235f, .98f, .335f);
        corruptButton = Button(
            corruptionPanel,
            "Corrupted: No",
            .02f,
            .25f,
            .24f,
            .80f,
            () =>
            {
                corrupted = !corrupted;
                Caption(corruptButton, corrupted ? "Corrupted: Yes" : "Corrupted: No");
            }
        );
        corruptionSelect = Button(
            corruptionPanel,
            "Corrupted affix: None",
            .26f,
            .25f,
            .61f,
            .80f,
            CorruptionPicker
        );
        corruptionTier = NumericCompact(corruptionPanel, .64f, .25f, .77f, .80f, 1, 7, 7);
        corruptionRoll = NumericCompact(corruptionPanel, .80f, .25f, .96f, .80f, 0, 100, 100);
        Label(corruptionPanel, "Tier", .64f, .81f, .77f, .99f, 11);
        Label(corruptionPanel, "Roll %", .80f, .81f, .96f, .99f, 11);
        corruptionSelect.interactable = CorruptedAffixAdapter.IsSupported;
        if (!CorruptedAffixAdapter.IsSupported)
            Caption(corruptionSelect, "Corruption data unavailable");
        uniquePage = Panel(middle, "Unique properties", .02f, .025f, .98f, .225f);
        lp = NumericGrid(uniquePage, "Legendary Potential", 0, 2, 0, 4, 0, false, 3);
        ww = NumericGrid(uniquePage, "Weaver's Will", 1, 2, 0, 28, 0, false, 3);
        for (int i = 0; i < 8; i++)
            uniqueRolls[i] = NumericGrid(
                uniquePage,
                "Roll " + (i + 1),
                i % 4,
                i < 4 ? 1 : 0,
                0,
                100,
                100,
                true,
                3,
                4
            );
        ragePage = Panel(right, "Unique variants", .04f, .28f, .96f, .45f);
        variantHeader = Label(ragePage, "Unsated Rage modifier", .03f, .72f, .97f, .96f, 15);
        variantSelect[0] = Button(
            ragePage,
            "Choose Rage",
            .03f,
            .29f,
            .97f,
            .69f,
            () => RagePicker(0)
        );
        variantSelect[1] = Button(
            ragePage,
            "Choose modifier 2",
            .03f,
            .25f,
            .97f,
            .49f,
            () => RagePicker(1)
        );
        variantSelect[1].gameObject.SetActive(false);
        variantDescription = Label(
            ragePage,
            "Exclusive ring modifier · separate from LP",
            .03f,
            .04f,
            .97f,
            .25f,
            11
        );
        ragePage.SetActive(false);
        Label(right, "Item preview", .04f, .92f, .96f, .99f, 20);
        preview = Label(right, "Choose an item", .04f, .28f, .96f, .90f, 15);
        quantity = Numeric(right, "Quantity", .20f, 1, 99, 1, false, false);
        dropButton = Button(right, "Drop Item", .04f, .105f, .96f, .18f, Drop);
        Button(right, "Reset", .04f, .03f, .96f, .09f, Reset);
        status = Label(root, "", .30f, .00f, .99f, .025f, 12);
        BuildPicker();
        // Hide only after the entire replacement view has been built successfully.
        foreach (var child in Functions.GetAllChild(FD.content_obj))
            if (child != root)
                child.SetActive(false);
        RefreshItems();
        LogCorruptionMetadata();
    }

    static void Preset(bool random)
    {
        foreach (var n in numbers)
        {
            if (n == quantity || n == corruptionTier || n == corruptionRoll)
                continue;
            n.random = random && n.mode != null;
            if (!random)
            {
                n.value = n.max;
                n.input.SetTextWithoutNotify(n.value.ToString());
            }
        }
        RefreshModes();
    }

    static void RefreshModes()
    {
        foreach (var n in numbers)
            if (n.mode != null)
            {
                Caption(n.mode, n.random ? "Random" : "Fixed");
                n.input.interactable = !n.random;
            }
    }

    static void Reset()
    {
        foreach (var r in rows)
        {
            r.id = -1;
            r.name = "None";
            Caption(r.select, "None");
        }
        corrupted = false;
        corruptionId = -1;
        corruptionName = "None";
        Caption(corruptButton, "Corrupted: No");
        quantity.value = 1;
        quantity.input.SetTextWithoutNotify("1");
        lp.value = ww.value = 0;
        lp.input.SetTextWithoutNotify("0");
        ww.input.SetTextWithoutNotify("0");
        ResetRage();
        Preset(true);
        result = "";
    }

    static void RefreshItems()
    {
        nativeItems = NativeItemNames.Items(FD.items_dropdown, FD.item_type, FD.item_rarity);
        itemIndexes.Clear();
        for (int i = 1; i < FD.items_dropdown.options.Count; i++)
        {
            string raw = FD.items_dropdown.options[i].text;
            nativeItems.TryGetValue(i, out var item);
            if (
                NativeItemNames.Matches(
                    search.text,
                    item == null ? raw : item.name,
                    item == null ? raw : item.aliases
                )
            )
                itemIndexes.Add(i);
        }
        for (int slot = 0; slot < itemButtons.Count; slot++)
        {
            int index = itemPage * 12 + slot;
            itemButtons[slot].gameObject.SetActive(index < itemIndexes.Count);
            if (index < itemIndexes.Count)
            {
                bool selected = itemIndexes[index] == FD.items_dropdown.value;
                Caption(
                    itemButtons[slot],
                    (selected ? "Selected: " : "") + ItemName(itemIndexes[index])
                );
                itemButtons[slot].GetComponent<Image>().color = selected
                    ? new Color(.29f, .24f, .13f)
                    : dark;
                itemButtons[slot].GetComponent<Outline>().effectColor = selected
                    ? gold
                    : new Color(gold.r, gold.g, gold.b, .65f);
            }
        }
    }

    static void ChooseItem(int slot)
    {
        int i = itemPage * 12 + slot;
        if (i >= itemIndexes.Count)
            return;
        int option = itemIndexes[i];
        FD.items_dropdown.SetValueWithoutNotify(option);
        if (nativeItems.TryGetValue(option, out var item))
            NativeItemNames.SelectItem(item);
        else
            FD.SelectItem();
        ResetRage();
        RefreshItems();
        foreach (var row in rows)
        {
            row.id = -1;
            row.name = "None";
            Caption(row.select, "None");
        }
        corruptionId = -1;
        corruptionName = "None";
        Caption(
            corruptionSelect,
            CorruptedAffixAdapter.IsSupported
                ? "Corrupted affix: None"
                : "Corrupted affix API unavailable"
        );
        result = "";
    }

    static UniqueList.Entry SelectedRageEntry()
    {
        if (
            FD.item_rarity < 7
            || FD.items_dropdown.value <= 0
            || UniqueList.instance.IsNullOrDestroyed()
        )
            return null;
        return UniqueList.getUnique((ushort)FD.item_unique_id);
    }

    static void ResetRage()
    {
        bool ring = UniqueVariantAdapter.IsUnsated(SelectedRageEntry());
        for (int slot = 0; slot < variantIds.Length; slot++)
        {
            variantIds[slot] = -1;
            variantNames[slot] =
                ring && slot == 0 ? "Choose Rage"
                : slot == 0 ? "Choose modifier 1"
                : "Choose modifier 2";
            if (!variantSelect[slot].IsNullOrDestroyed())
                Caption(variantSelect[slot], variantNames[slot]);
        }
    }

    static void RagePicker(int slot)
    {
        choices.Clear();
        foreach (var definition in UniqueVariantAdapter.Catalog(SelectedRageEntry()))
        {
            int id = definition.affixId;
            if (variantIds[1 - slot] == id)
                continue;
            string name = NativeItemNames.AffixName(definition);
            choices.Add(
                new Choice
                {
                    id = id,
                    name = name,
                    aliases = NativeItemNames.AffixAliases(definition),
                    select = () =>
                    {
                        variantIds[slot] = id;
                        variantNames[slot] = name;
                        Caption(variantSelect[slot], name);
                    },
                }
            );
        }
        choices.Sort((a, b) => string.Compare(a.name, b.name, StringComparison.OrdinalIgnoreCase));
        OpenPicker(
            UniqueVariantAdapter.IsUnsated(SelectedRageEntry())
                ? "Unsated Rage — exclusive ring modifier"
            : slot == 0 ? "Withstand the Elements — modifier 1"
            : "Withstand the Elements — modifier 2"
        );
    }

    static void CorruptionPicker()
    {
        choices.Clear();
        choices.Add(
            new Choice
            {
                id = -1,
                name = "None",
                select = () =>
                {
                    corruptionId = -1;
                    corruptionName = "None";
                    Caption(corruptionSelect, "Corrupted affix: None");
                },
            }
        );
        foreach (var a in CorruptedAffixAdapter.Catalog())
        {
            if (!CorruptedAffixAdapter.FitsItem(a, FD.item_type, FD.item_subtype))
                continue;
            int id = a.affixId;
            bool used = false;
            foreach (var row in rows)
                if (row.id == id)
                {
                    used = true;
                    break;
                }
            if (used || choices.Exists(x => x.id == id))
                continue;
            string name = NativeItemNames.AffixName(a);
            choices.Add(
                new Choice
                {
                    id = id,
                    name = name,
                    aliases = NativeItemNames.AffixAliases(a),
                    select = () =>
                    {
                        corruptionId = id;
                        corruptionName = name;
                        corrupted = true;
                        Caption(corruptButton, "Corrupted: Yes");
                        Caption(corruptionSelect, name);
                    },
                }
            );
        }
        OpenPicker("Corrupted affix");
    }

    public static void DropSelection()
    {
        if (IsReady)
            Drop();
    }

    static int[] SelectedVariantIds()
    {
        return UniqueVariantAdapter.VariantCount(SelectedRageEntry()) == 2
            ? new[] { variantIds[0], variantIds[1] }
            : new[] { variantIds[0] };
    }

    static void CatalogPicker(Dropdown catalog, Action changed, bool skipPlaceholder)
    {
        choices.Clear();
        for (int i = skipPlaceholder ? 1 : 0; i < catalog.options.Count; i++)
        {
            string label = catalog.options[i].text;
            if (string.IsNullOrWhiteSpace(label))
                continue;
            if (
                catalog == FD.type_dropdown
                && label.IndexOf("blessing", StringComparison.OrdinalIgnoreCase) >= 0
            )
                continue;
            nativeCategories.TryGetValue(i, out var category);
            string display =
                catalog == FD.type_dropdown
                    ? CategoryName(i, label)
                    : NativeItemNames.RarityName(label);
            int index = i;
            choices.Add(
                new Choice
                {
                    group = CategoryGroup(label),
                    name = display,
                    aliases = label + "\n" + CategoryLabel(label),
                    select = () =>
                    {
                        catalog.SetValueWithoutNotify(index);
                        if (catalog == FD.type_dropdown && category != null)
                            NativeItemNames.SelectCategory(category);
                        else
                            changed();
                        ResetRage();
                        foreach (var r in rows)
                        {
                            r.id = -1;
                            r.name = "None";
                            Caption(r.select, "None");
                        }
                        corruptionId = -1;
                        corruptionName = "None";
                        lastItems = "";
                    },
                }
            );
        }
        OpenPicker(catalog == FD.type_dropdown ? "Category" : "Rarity");
        categoryPicker = catalog == FD.type_dropdown;
        rarityPicker = !categoryPicker;
        RefreshPicker();
    }

    static void AffixPicker(int slot)
    {
        choices.Clear();
        choices.Add(
            new Choice
            {
                id = -1,
                name = "None",
                select = () =>
                {
                    rows[slot].id = -1;
                    rows[slot].name = "None";
                    Caption(rows[slot].select, "None");
                },
            }
        );
        var list = AffixList.get();
        if (list.IsNullOrDestroyed())
            return;
        foreach (var a in ForceDropCatalog.Definitions())
            AddAffixChoice(a, slot);
        choices.Sort((a, b) => string.Compare(a.name, b.name, StringComparison.OrdinalIgnoreCase));
        OpenPicker(
            slot == 4 ? "Sealed affix"
            : slot < 2 ? "Prefix"
            : "Suffix"
        );
    }

    static void AddAffixChoice(AffixList.Affix a, int slot)
    {
        if (
            a.IsNullOrDestroyed()
            || CorruptedAffixAdapter.IsCorruption(a)
            || UniqueVariantAdapter.IsVariant(a)
            || ForceDropCatalog.MaximumTier(a) == 0
            || FD.item_type < 0
            || FD.item_subtype < 0
        )
            return;
        if (a.type != AffixList.AffixType.PREFIX && a.type != AffixList.AffixType.SUFFIX)
            return;
        if (
            slot < 4
            && a.type != (slot < 2 ? AffixList.AffixType.PREFIX : AffixList.AffixType.SUFFIX)
        )
            return;
        if (!CorruptedAffixAdapter.FitsItem(a, FD.item_type, FD.item_subtype))
            return;
        int id = a.affixId;
        for (int other = 0; other < rows.Length; other++)
            if (other != slot && rows[other].id == id)
                return;
        if (corrupted && corruptionId == id)
            return;
        if (choices.Exists(x => x.id == id))
            return;
        string name = NativeItemNames.AffixName(a);
        choices.Add(
            new Choice
            {
                id = id,
                name = name,
                aliases = NativeItemNames.AffixAliases(a),
                select = () =>
                {
                    rows[slot].id = id;
                    rows[slot].name = name;
                    Caption(rows[slot].select, name);
                },
            }
        );
    }

    static AffixList.Affix FindAffix(int id) => ForceDropCatalog.Find(id);

    static void RefreshTierLimits()
    {
        foreach (var row in rows)
            SetTierLimit(
                row.tier,
                row.id < 0 ? 7 : ForceDropCatalog.MaximumTier(FindAffix(row.id))
            );
        SetTierLimit(
            corruptionTier,
            corruptionId < 0 ? 7 : ForceDropCatalog.MaximumTier(FindAffix(corruptionId))
        );
    }

    static void SetTierLimit(Number number, int maximum)
    {
        number.max = Math.Max(1, maximum);
        if (number.value > number.max)
        {
            number.value = number.max;
            number.input.SetTextWithoutNotify(number.value.ToString(CultureInfo.InvariantCulture));
        }
    }

    static bool ValidSelectedItem()
    {
        var list = ItemList.get();
        if (list.IsNullOrDestroyed() || FD.item_type < 0 || FD.item_subtype < 0)
            return false;
        bool baseExists = false;
        foreach (var type in list.EquippableItems)
            if (type.baseTypeID == FD.item_type)
                foreach (var item in type.subItems)
                    if (item.subTypeID == FD.item_subtype)
                    {
                        baseExists = true;
                        break;
                    }
        foreach (var type in list.nonEquippableItems)
            if (type.baseTypeID == FD.item_type)
                foreach (var item in type.subItems)
                    if (item.subTypeID == FD.item_subtype)
                    {
                        baseExists = true;
                        break;
                    }
        if (!baseExists)
            return false;
        if (FD.item_rarity == 0)
            return true;
        if (UniqueList.instance.IsNullOrDestroyed())
            return false;
        foreach (var entry in UniqueList.instance.uniques)
            if (
                entry.uniqueID == FD.item_unique_id
                && entry.baseType == FD.item_type
                && entry.isSetItem == (FD.item_rarity == 8)
            )
                foreach (var subType in entry.subTypes)
                    if (subType == FD.item_subtype)
                        return true;
        return false;
    }

    static string Validate()
    {
        if (
            FD.type_dropdown.value <= 0
            || FD.rarity_dropdown.value <= 0
            || FD.items_dropdown.value <= 0
            || FD.item_subtype < 0
        )
            return "Choose a category, rarity and item.";
        if (
            Refs_Manager.player_actor.IsNullOrDestroyed()
            || Refs_Manager.ground_item_manager.IsNullOrDestroyed()
        )
            return "Enter the game before dropping items.";
        var ids = new HashSet<int>();
        foreach (var r in rows)
            if (r.id >= 0 && !ids.Add(r.id))
                return "Each affix must be different, including the sealed affix.";
        if (!ValidSelectedItem())
            return "The selected item does not match its category. Choose it again.";
        var rageEntry = SelectedRageEntry();
        int variantCount = UniqueVariantAdapter.VariantCount(rageEntry);
        if (variantCount > 0)
        {
            if (!UniqueVariantAdapter.HasVariants(rageEntry))
                return "Unique modifier pool unavailable";
            var variantCatalog = UniqueVariantAdapter.Catalog(rageEntry);
            for (int slot = 0; slot < variantCount; slot++)
            {
                if (!variantCatalog.Exists(a => a.affixId == variantIds[slot]))
                    return variantCount == 1
                        ? "Choose the ring's exclusive Rage modifier."
                        : "Choose both exclusive glove modifiers.";
                if (!ids.Add(variantIds[slot]))
                    return "Exclusive unique modifiers must be different and separate from ordinary affixes.";
            }
        }
        for (int slot = 0; slot < rows.Length; slot++)
        {
            var row = rows[slot];
            if (row.id < 0)
                continue;
            var definition = FindAffix(row.id);
            if (
                definition.IsNullOrDestroyed()
                || CorruptedAffixAdapter.IsCorruption(definition)
                || UniqueVariantAdapter.IsVariant(definition)
            )
                return "Choose a regular affix for "
                    + (
                        slot == 4 ? "Sealed"
                        : slot < 2 ? "Prefix"
                        : "Suffix"
                    )
                    + ".";
            if (
                definition.type != AffixList.AffixType.PREFIX
                && definition.type != AffixList.AffixType.SUFFIX
            )
                return "Special modifiers belong in the Corruption row.";
            if (
                slot < 4
                && definition.type
                    != (slot < 2 ? AffixList.AffixType.PREFIX : AffixList.AffixType.SUFFIX)
            )
                return "Affix type does not match its slot.";
            if (!CorruptedAffixAdapter.FitsItem(definition, FD.item_type, FD.item_subtype))
                return "An affix cannot roll on the selected item. Choose it again.";
            if (row.tier.value > ForceDropCatalog.MaximumTier(definition))
                return "The selected tier does not exist for this affix.";
        }
        if (corrupted && corruptionId >= 0)
        {
            AffixList.Affix definition = null;
            foreach (var entry in CorruptedAffixAdapter.Catalog())
                if (entry.affixId == corruptionId)
                {
                    definition = entry;
                    break;
                }
            if (
                !CorruptedAffixAdapter.IsSupported
                || definition.IsNullOrDestroyed()
                || !CorruptedAffixAdapter.IsCorruption(definition)
                || !CorruptedAffixAdapter.FitsItem(definition, FD.item_type, FD.item_subtype)
            )
                return "The corrupted affix is not valid for this item.";
            if (corruptionTier.value > ForceDropCatalog.MaximumTier(definition))
                return "The selected tier does not exist for this affix.";
            if (!ids.Add(corruptionId))
                return "Corruption cannot duplicate another affix.";
        }
        foreach (var n in numbers)
            if (!int.TryParse(n.input.text, out int value) || value < n.min || value > n.max)
                return "Use whole numbers within each field's range.";
        if (FD.item_type >= 100 && corrupted)
            return "Corruption is available for equipment only.";
        return "";
    }

    static void RefreshPreview()
    {
        var s = new StringBuilder(
            FD.items_dropdown.value > 0 ? ItemName(FD.items_dropdown.value) : L("Choose an item")
        );
        s.Append("\n\n")
            .Append(SelectedCategoryName(""))
            .Append("\n")
            .Append(NativeItemNames.RarityName(Selected(FD.rarity_dropdown, "")));
        if (FD.item_type < 100)
        {
            if (FD.item_rarity < 7)
                s.Append("\n")
                    .Append(L("Forging potential"))
                    .Append(": ")
                    .Append(
                        corrupted ? "0"
                        : forging.random ? L("Random")
                        : forging.value.ToString()
                    );
            foreach (var r in rows)
                if (r.id >= 0)
                    s.Append("\n\n")
                        .Append(r == rows[4] ? L("Sealed") + ": " : "")
                        .Append(r.name)
                        .Append("\nT")
                        .Append(r.tier.value)
                        .Append(" · ")
                        .Append((r.roll.random ? L("Random") : r.roll.value.ToString()))
                        .Append(" %");
            if (FD.item_rarity > 6)
                s.Append("\n\n")
                    .Append(
                        FD.item_legendary_type == UniqueList.LegendaryType.LegendaryPotential
                            ? L("LP")
                                + ": "
                                + (
                                    corrupted ? "0"
                                    : lp.random ? L("Random")
                                    : lp.value.ToString()
                                )
                            : L("Weaver's Will")
                                + ": "
                                + (
                                    corrupted ? "0"
                                    : ww.random ? L("Random")
                                    : ww.value.ToString()
                                )
                    );
        }
        int variantCount = UniqueVariantAdapter.VariantCount(SelectedRageEntry());
        for (int slot = 0; slot < variantCount; slot++)
            s.Append("\n\n")
                .Append(
                    L(
                        variantCount == 1 ? "Ring variant"
                        : slot == 0 ? "Glove modifier 1"
                        : "Glove modifier 2"
                    )
                )
                .Append(": ")
                .Append(L(variantNames[slot]));
        s.Append("\n\n").Append(L("Corrupted")).Append(": ").Append(corrupted ? L("Yes") : L("No"));
        if (corrupted && corruptionId >= 0)
            s.Append("\n")
                .Append(corruptionName)
                .Append("\nT")
                .Append(corruptionTier.value)
                .Append(" · ")
                .Append(corruptionRoll.value)
                .Append(" %");
        preview.text = s.ToString();
        string problem = Validate();
        status.text = L(
            problem.Length > 0 ? problem
            : result.Length > 0 ? result
            : "Drops at your character."
        );
        dropButton.interactable = problem.Length == 0;
    }

    static int Sample(Number n) => n.random ? UnityEngine.Random.Range(n.min, n.max + 1) : n.value;

    static int Roll(Number n) =>
        n.random ? UnityEngine.Random.Range(0, 256) : Mathf.RoundToInt(n.value / 100f * 255f);

    static ResolvedForceDrop ResolveItem()
    {
        bool equipment = FD.item_type < 100;
        bool unique = FD.item_rarity >= 7;
        var selected = new List<ResolvedForceDropAffix>();
        if (equipment)
            for (int slot = 0; slot < rows.Length; slot++)
            {
                var row = rows[slot];
                if (row.id >= 0)
                    selected.Add(
                        new ResolvedForceDropAffix(
                            row.id,
                            row.tier.value - 1,
                            Roll(row.roll),
                            slot == 4 ? ForceDropSeal.Regular : ForceDropSeal.None
                        )
                    );
            }
        var implicitValues = new int[implicits.Length];
        for (int i = 0; i < implicitValues.Length; i++)
            implicitValues[i] = equipment ? Roll(implicits[i]) : 0;
        var uniqueValues = new int[uniqueRolls.Length];
        if (unique)
            for (int i = 0; i < uniqueValues.Length; i++)
                uniqueValues[i] = Roll(uniqueRolls[i]);
        bool usesLP =
            unique && FD.item_legendary_type == UniqueList.LegendaryType.LegendaryPotential;
        return new ResolvedForceDrop(
            FD.item_type,
            FD.item_subtype,
            unique ? FD.item_unique_id : 0,
            FD.item_rarity,
            equipment && !unique && !corrupted ? Sample(forging) : 0,
            usesLP && !corrupted ? Sample(lp) : 0,
            unique && !usesLP && !corrupted ? Sample(ww) : 0,
            corrupted,
            implicitValues,
            uniqueValues,
            selected,
            UniqueVariantAdapter.VariantCount(SelectedRageEntry()) > 0
                ? SelectedVariantIds()
                : Array.Empty<int>(),
            corrupted && corruptionId >= 0
                ? new ResolvedForceDropAffix(
                    corruptionId,
                    corruptionTier.value - 1,
                    Roll(corruptionRoll),
                    ForceDropSeal.Corruption
                )
                : null
        );
    }

    static void Drop()
    {
        RefreshTierLimits();
        foreach (var n in numbers)
            n.Read();
        string problem = Validate();
        if (problem.Length != 0)
        {
            result = problem;
            return;
        }
        int requested = quantity.value;
        int dropped = 0;
        try
        {
            for (int copy = 0; copy < requested; copy++)
            {
                // Each copy gets one immutable request and independently resolved
                // rolls. Construction and verification never sample them again.
                ForceDropItemCreator.Drop(ResolveItem());
                dropped++;
            }
            result = "Dropped " + dropped + " item(s).";
        }
        catch (Exception ex)
        {
            result =
                "Dropped " + dropped + " of " + requested + " item(s). Drop failed: " + ex.Message;
            Main.logger_instance.Error(result);
        }
    }

    static void BuildPicker()
    {
        picker = Panel(root, "Search picker", .15f, .08f, .85f, .89f);
        pickerTitle = Label(picker, "Select", .03f, .90f, .83f, .98f, 20);
        Button(picker, "Close", .84f, .90f, .97f, .98f, () => picker.SetActive(false));
        pickerSearch = Input(picker, "Search choices", .03f, .81f, .97f, .88f, "", false);
        for (int i = 0; i < 75; i++)
        {
            int slot = i;
            pickButtons.Add(
                Button(
                    picker,
                    "",
                    .03f,
                    .735f - i * .063f,
                    .97f,
                    .79f - i * .063f,
                    () =>
                    {
                        int index = slot;
                        if (index >= visiblePicks.Count)
                            return;
                        visiblePicks[index].select();
                        picker.SetActive(false);
                    }
                )
            );
        }
        pickerPrevious = Button(
            picker,
            "Previous",
            .03f,
            .03f,
            .48f,
            .085f,
            () =>
            {
                pickerPage = Math.Max(0, pickerPage - 1);
                RefreshPicker();
            }
        );
        pickerNext = Button(
            picker,
            "Next",
            .52f,
            .03f,
            .97f,
            .085f,
            () =>
            {
                if (HasNextPickerPage())
                    pickerPage++;
                RefreshPicker();
            }
        );
        for (int i = 0; i < groups.Length; i++)
            pickerHeaders.Add(
                Label(picker, groups[i], .03f + i * .19f, .74f, .21f + i * .19f, .795f, 15)
            );
        picker.SetActive(false);
    }

    static void OpenPicker(string title)
    {
        categoryPicker = rarityPicker = false;
        LocaleRegistry.Apply(pickerTitle, title);
        pickerSearch.SetTextWithoutNotify("");
        lastPickerSearch = "";
        pickerPage = 0;
        picker.SetActive(true);
        picker.transform.SetAsLastSibling();
        RefreshPicker();
    }

    static void RefreshNativeLocale()
    {
        string locale = NativeItemNames.Locale;
        object modLocale = Locales.current_dictionary;
        int categoryCount = FD.type_dropdown.options.Count;
        if (
            lastNativeLocale == locale
            && ReferenceEquals(lastModLocale, modLocale)
            && lastCategoryCount == categoryCount
        )
            return;
        lastNativeLocale = locale;
        lastModLocale = modLocale;
        lastCategoryCount = categoryCount;
        nativeCategories = NativeItemNames.Categories(FD.type_dropdown);
        lastItems = "";
        lastSearch = "";
        itemPage = 0;
        // Picker actions keep stable ids; reopen to rebuild its translated labels.
        if (!picker.IsNullOrDestroyed())
            picker.SetActive(false);
        foreach (var row in rows)
        {
            row.name = row.id < 0 ? "None" : NativeItemNames.AffixName(row.id, row.name);
            Caption(row.select, row.name);
        }
        corruptionName =
            corruptionId < 0 ? "None" : NativeItemNames.AffixName(corruptionId, corruptionName);
        Caption(corruptionSelect, corruptionId < 0 ? "Corrupted affix: None" : corruptionName);
        for (int slot = 0; slot < variantIds.Length; slot++)
        {
            if (variantIds[slot] < 0)
                continue;
            variantNames[slot] = NativeItemNames.AffixName(variantIds[slot], variantNames[slot]);
            Caption(variantSelect[slot], variantNames[slot]);
        }
    }

    static string ItemName(int index)
    {
        if (nativeItems.TryGetValue(index, out var item))
            return item.name;
        return index > 0 && index < FD.items_dropdown.options.Count
            ? FD.items_dropdown.options[index].text
            : L("Choose an item");
    }

    static string CategoryName(int index, string fallback)
    {
        if (!nativeCategories.TryGetValue(index, out var category))
            return L(CategoryLabel(fallback));
        string label = CategoryLabel(category.raw);
        if (label == "Runes" || label == "Glyphs")
            return L(label);
        return category.name;
    }

    static string SelectedCategoryName(string fallback)
    {
        return FD.type_dropdown.value > 0
            ? CategoryName(FD.type_dropdown.value, Selected(FD.type_dropdown, fallback))
            : L(fallback);
    }

    static string CategoryLabel(string name)
    {
        if (name.IndexOf("crafting modifier", StringComparison.OrdinalIgnoreCase) >= 0)
            return "Runes";
        if (name.IndexOf("crafting support", StringComparison.OrdinalIgnoreCase) >= 0)
            return "Glyphs";
        // Keep the lens family visible together; retain the subtype so each button is distinct.
        if (name.EndsWith(" Lens", StringComparison.OrdinalIgnoreCase))
            return "Lens: " + name.Substring(0, name.Length - 5);
        return name;
    }

    static int CategoryGroup(string name)
    {
        string n = name.ToLowerInvariant();
        foreach (
            string token in new[]
            {
                "axe",
                "bow",
                "dagger",
                "mace",
                "scepter",
                "sceptre",
                "staff",
                "staves",
                "sword",
                "wand",
                "spear",
                "quiver",
                "fist",
                "polearm",
            }
        )
            if (n.Contains(token))
                return 0;
        foreach (
            string token in new[]
            {
                "helmet",
                "body armor",
                "body armour",
                "belt",
                "boot",
                "glove",
                "shield",
            }
        )
            if (n.Contains(token))
                return 1;
        foreach (string token in new[] { "ring", "amulet", "relic" })
            if (n.Contains(token))
                return 2;
        if (n.Contains("idol"))
            return 3;
        return 4;
    }

    static bool HasNextPickerPage() =>
        !categoryPicker && !rarityPicker && (pickerPage + 1) * 20 < filtered.Count;

    static void RefreshPicker()
    {
        filtered.Clear();
        visiblePicks.Clear();
        foreach (var choice in choices)
            if (NativeItemNames.Matches(pickerSearch.text, choice.name, choice.aliases))
                filtered.Add(choice);
        pickerPrevious.gameObject.SetActive(!categoryPicker && !rarityPicker);
        pickerNext.gameObject.SetActive(!categoryPicker && !rarityPicker);
        foreach (var header in pickerHeaders)
            header.gameObject.SetActive(categoryPicker);
        if (categoryPicker)
            while (pickButtons.Count < filtered.Count)
            {
                int slot = pickButtons.Count;
                pickButtons.Add(
                    Button(
                        picker,
                        "",
                        0,
                        0,
                        1,
                        1,
                        () =>
                        {
                            if (slot >= visiblePicks.Count)
                                return;
                            visiblePicks[slot].select();
                            picker.SetActive(false);
                        }
                    )
                );
            }
        foreach (var button in pickButtons)
            button.gameObject.SetActive(false);
        if (categoryPicker)
        {
            int rowCount = 15;
            for (int group = 0; group < groups.Length; group++)
                rowCount = Math.Max(rowCount, filtered.FindAll(x => x.group == group).Count);
            float step = .62f / rowCount;
            for (int group = 0; group < groups.Length; group++)
            {
                var column = filtered.FindAll(x => x.group == group);
                column.Sort(
                    (x, y) => string.Compare(x.name, y.name, StringComparison.OrdinalIgnoreCase)
                );
                for (int row = 0; row < column.Count; row++)
                {
                    int index = row;
                    if (index >= column.Count)
                        break;
                    var button = pickButtons[visiblePicks.Count];
                    Rect(
                        button.gameObject,
                        .03f + group * .19f,
                        .73f - (row + 1) * step,
                        .21f + group * .19f,
                        .73f - row * step - .004f
                    );
                    Caption(button, column[index].name);
                    button.gameObject.SetActive(true);
                    visiblePicks.Add(column[index]);
                }
            }
        }
        else
        {
            int count = rarityPicker ? Math.Min(4, filtered.Count) : 20;
            for (int slot = 0; slot < count; slot++)
            {
                int index = rarityPicker ? slot : pickerPage * 20 + slot;
                if (index >= filtered.Count)
                    break;
                int col = rarityPicker ? slot : slot % 2,
                    row = rarityPicker ? 0 : slot / 2;
                float width = rarityPicker ? .235f : .475f;
                var button = pickButtons[visiblePicks.Count];
                Rect(
                    button.gameObject,
                    .03f + col * width,
                    .69f - row * .058f,
                    .03f + col * width + width - .015f,
                    .739f - row * .058f
                );
                Caption(button, filtered[index].name);
                button.gameObject.SetActive(true);
                visiblePicks.Add(filtered[index]);
            }
        }
    }

    static Number NumericGrid(
        GameObject parent,
        string name,
        int col,
        int row,
        int min,
        int max,
        int value,
        bool percent,
        int rowCount = 2,
        int columns = 2
    )
    {
        float width = .96f / columns,
            height = .92f / rowCount;
        var cell = Panel(
            parent,
            name,
            .02f + col * width,
            .04f + row * height,
            .02f + (col + 1) * width - .01f,
            .04f + (row + 1) * height - .01f
        );
        Label(cell, name + (percent ? " %" : ""), .025f, .60f, .97f, .97f, 12);
        var n = NumericCompact(cell, .03f, .08f, .55f, .57f, min, max, value);
        n.group = cell;
        n.mode = Button(
            cell,
            "Fixed",
            .60f,
            .08f,
            .97f,
            .57f,
            () =>
            {
                n.random = !n.random;
                RefreshModes();
            }
        );
        return n;
    }

    static GameObject Panel(GameObject parent, string name, float x0, float y0, float x1, float y1)
    {
        var go = new GameObject(name);
        go.AddComponent<RectTransform>();
        go.transform.SetParent(parent.transform, false);
        Rect(go, x0, y0, x1, y1);
        go.AddComponent<Image>().color = dark;
        var outline = go.AddComponent<Outline>();
        outline.effectColor = new Color(gold.r, gold.g, gold.b, .65f);
        outline.effectDistance = new Vector2(1f, -1f);
        return go;
    }

    static void Rect(GameObject go, float x0, float y0, float x1, float y1)
    {
        var rect = go.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(x0, y0);
        rect.anchorMax = new Vector2(x1, y1);
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        rect.localScale = Vector3.one;
    }

    static Text Label(
        GameObject parent,
        string text,
        float x0,
        float y0,
        float x1,
        float y1,
        int size = 15
    )
    {
        var go = new GameObject("Label");
        go.AddComponent<RectTransform>();
        go.transform.SetParent(parent.transform, false);
        Rect(go, x0, y0, x1, y1);
        var label = go.AddComponent<Text>();
        label.font = font;
        label.fontSize = size;
        label.color = gold;
        LocaleRegistry.Apply(label, text);
        label.raycastTarget = false;
        label.alignment = TextAnchor.UpperLeft;
        label.horizontalOverflow = HorizontalWrapMode.Wrap;
        label.verticalOverflow = VerticalWrapMode.Truncate;
        return label;
    }

    static Button Button(
        GameObject parent,
        string text,
        float x0,
        float y0,
        float x1,
        float y1,
        Action action
    )
    {
        var go = Panel(parent, "Button", x0, y0, x1, y1);
        var button = go.AddComponent<Button>();
        button.targetGraphic = go.GetComponent<Image>();
        var label = Label(go, text, .025f, .04f, .975f, .96f);
        label.alignment = TextAnchor.MiddleLeft;
        clicks[button.GetInstanceID()] = action;
        return button;
    }

    static void Caption(Button button, string text)
    {
        LocaleRegistry.Apply(button.GetComponentInChildren<Text>(true), text);
    }

    static string L(string text) => LocaleRegistry.Translate(text);

    static string Selected(Dropdown catalog, string fallback) =>
        catalog.value > 0 && catalog.value < catalog.options.Count
            ? catalog.options[catalog.value].text
            : fallback;

    static TMP_InputField Input(
        GameObject parent,
        string name,
        float x0,
        float y0,
        float x1,
        float y1,
        string value,
        bool numeric
    )
    {
        var go = UnityEngine.Object.Instantiate(template.gameObject, parent.transform);
        go.name = name;
        Rect(go, x0, y0, x1, y1);
        var layout = go.GetComponent<LayoutElement>();
        if (!layout.IsNullOrDestroyed())
            layout.ignoreLayout = true;
        var input = go.GetComponent<TMP_InputField>();
        input.onValueChanged.RemoveAllListeners();
        input.onEndEdit.RemoveAllListeners();
        input.enabled = true;
        input.readOnly = false;
        input.interactable = true;
        input.characterLimit = numeric ? 8 : 100;
        input.contentType = numeric
            ? TMP_InputField.ContentType.IntegerNumber
            : TMP_InputField.ContentType.Standard;
        input.SetTextWithoutNotify(value);
        input.targetGraphic.raycastTarget = true;
        var background = go.GetComponent<Image>();
        if (!background.IsNullOrDestroyed())
        {
            background.sprite = null;
            background.color = new Color(.16f, .18f, .21f);
        }
        if (!input.placeholder.IsNullOrDestroyed())
            input.placeholder.gameObject.SetActive(false);
        input.textComponent.color = gold;
        input.textComponent.fontSize = 15;
        input.textComponent.enableAutoSizing = true;
        input.textComponent.fontSizeMin = 10;
        input.textComponent.fontSizeMax = 15;
        input.textComponent.raycastTarget = false;
        input.textComponent.margin = Vector4.zero;
        if (!input.textViewport.IsNullOrDestroyed())
            Rect(input.textViewport.gameObject, .04f, .05f, .96f, .95f);
        Rect(input.textComponent.gameObject, 0, 0, 1, 1);
        var group = go.GetComponent<CanvasGroup>();
        if (!group.IsNullOrDestroyed())
        {
            group.alpha = 1f;
            group.interactable = true;
            group.blocksRaycasts = true;
        }
        go.SetActive(true);
        return input;
    }

    static Number NumericCompact(
        GameObject parent,
        float x0,
        float y0,
        float x1,
        float y1,
        int min,
        int max,
        int value
    )
    {
        var n = new Number
        {
            min = min,
            max = max,
            value = value,
            input = Input(parent, "Number", x0, y0, x1, y1, value.ToString(), true),
        };
        numbers.Add(n);
        return n;
    }

    static Number Numeric(
        GameObject parent,
        string name,
        float y,
        int min,
        int max,
        int value,
        bool percent,
        bool mode
    )
    {
        Label(parent, name + (percent ? " (%)" : ""), .03f, y - .015f, .56f, y + .055f, 14);
        var n = NumericCompact(
            parent,
            .57f,
            y - .025f,
            mode ? .77f : .96f,
            y + .055f,
            min,
            max,
            value
        );
        if (mode)
            n.mode = Button(
                parent,
                "Fixed",
                .79f,
                y - .025f,
                .97f,
                y + .055f,
                () =>
                {
                    n.random = !n.random;
                    RefreshModes();
                }
            );
        return n;
    }

    static void LogCorruptionMetadata()
    {
        if (metadataLogged)
            return;
        metadataLogged = true;
        foreach (var nested in typeof(AffixList).GetNestedTypes())
            if (
                nested.IsEnum
                && nested.Name.IndexOf("special", StringComparison.OrdinalIgnoreCase) >= 0
            )
                Main.logger_instance.Msg(
                    "Force Drop " + nested.Name + ": " + string.Join(", ", Enum.GetNames(nested))
                );
        Main.logger_instance.Msg(
            "Force Drop affix types: "
                + string.Join(", ", Enum.GetNames(typeof(AffixList.AffixType)))
        );
        foreach (var member in typeof(AffixList).GetMembers())
            if (
                member.MemberType == System.Reflection.MemberTypes.Field
                || member.MemberType == System.Reflection.MemberTypes.Property
            )
                Main.logger_instance.Msg("Force Drop catalog API: " + member);
        foreach (
            var type in new[]
            {
                typeof(ItemData),
                typeof(ItemDataUnpacked),
                typeof(ItemAffix),
                typeof(AffixList),
                typeof(AffixList.Affix),
                typeof(AffixList.SingleAffix),
                typeof(AffixList.MultiAffix),
            }
        )
        foreach (var member in type.GetMembers())
            if (
                member.Name.IndexOf("corrupt", StringComparison.OrdinalIgnoreCase) >= 0
                || member.Name.IndexOf("special", StringComparison.OrdinalIgnoreCase) >= 0
            )
                Main.logger_instance.Msg("Force Drop corruption API: " + type.Name + "." + member);
    }

    [HarmonyPatch(typeof(Button), "Press")]
    public class ButtonPress
    {
        [HarmonyPostfix]
        static void Postfix(Button __instance)
        {
            if (!__instance.interactable || !__instance.gameObject.activeInHierarchy)
                return;
            if (clicks.TryGetValue(__instance.GetInstanceID(), out var action))
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    Main.logger_instance.Error("Force Drop action: " + ex.Message);
                }
        }
    }
}
