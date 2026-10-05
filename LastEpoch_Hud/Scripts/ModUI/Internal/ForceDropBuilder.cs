using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using HarmonyLib;
using Il2Cpp;
using Il2CppTMPro;
using UnityEngine;
using UnityEngine.UI;
using FD = LastEpoch_Hud.Scripts.Hud_Manager.Content.OdlForceDrop;

namespace LastEpoch_Hud.Scripts.ModUI
{
    // A new view over the existing catalog and item creation code. No asset bundle rebuild.
    public static class ForceDropBuilder
    {
        static readonly Color gold = new Color(0.96f, 0.81f, 0.48f);
        static readonly Color dark = new Color(0.10f, 0.12f, 0.15f);
        static readonly Dictionary<int, Action> clicks = new Dictionary<int, Action>();
        static GameObject root, basePage, affixPage, uniquePage, picker;
        static TMP_InputField template, search, pickerSearch;
        static Font font;
        static Text preview, status, pickerTitle;
        static Button typeButton, rarityButton, dropButton, corruptButton, corruptionSelect;
        static int corruptionId = -1;
        static string corruptionName = "None";
        static Number corruptionTier, corruptionRoll;
        static readonly List<Button> itemButtons = new List<Button>();
        static readonly List<Button> pickButtons = new List<Button>();
        static readonly List<Choice> choices = new List<Choice>();
        static readonly List<Choice> filtered = new List<Choice>();
        static readonly List<int> itemIndexes = new List<int>();
        static readonly List<Number> numbers = new List<Number>();
        static readonly AffixRow[] rows = new AffixRow[5];
        static Number forging, quantity, lp, ww;
        static readonly Number[] implicits = new Number[3], uniqueRolls = new Number[8];
        static int itemPage, pickerPage;
        static string lastSearch = "", lastPickerSearch = "", lastItems = "";
        static bool corrupted, failed, metadataLogged;
        static string result = "";
        public static bool IsReady => !root.IsNullOrDestroyed();

        sealed class Choice { public int id; public string name; public Action select; }
        sealed class Number
        {
            public TMP_InputField input;
            public int min, max, value;
            public bool random;
            public Button mode;
            public void Read()
            {
                if (int.TryParse(input.text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int n))
                    value = Math.Max(min, Math.Min(max, n));
                if (!input.isFocused) input.SetTextWithoutNotify(value.ToString(CultureInfo.InvariantCulture));
            }
        }
        sealed class AffixRow
        {
            public int id = -1;
            public string name = "None";
            public Button select;
            public Number tier, roll;
        }

        public static bool Tick()
        {
            if (!IsReady && !failed)
            {
                try { Build(); }
                catch (Exception ex)
                {
                    failed = true;
                    if (!root.IsNullOrDestroyed()) UnityEngine.Object.Destroy(root);
                    root = null;
                    if (!FD.content_obj.IsNullOrDestroyed())
                        foreach (var child in Functions.GetAllChild(FD.content_obj)) child.SetActive(true);
                    Main.logger_instance.Error("Force Drop builder setup: " + ex.Message);
                }
            }
            if (!IsReady) return false;
            foreach (var n in numbers) n.Read();
            string signature = FD.item_type + ":" + FD.item_rarity + ":" + FD.items_dropdown.options.Count;
            if (lastSearch != search.text || lastItems != signature)
            {
                lastSearch = search.text; lastItems = signature; itemPage = 0; RefreshItems();
            }
            if (picker.activeSelf && lastPickerSearch != pickerSearch.text)
            {
                lastPickerSearch = pickerSearch.text; pickerPage = 0; RefreshPicker();
            }
            Caption(typeButton, Selected(FD.type_dropdown, "Choose category"));
            Caption(rarityButton, Selected(FD.rarity_dropdown, "Choose rarity"));
            lp.input.gameObject.SetActive(FD.item_rarity > 6 && FD.item_legendary_type == UniqueList.LegendaryType.LegendaryPotential);
            lp.mode.gameObject.SetActive(lp.input.gameObject.activeSelf);
            ww.input.gameObject.SetActive(FD.item_rarity > 6 && FD.item_legendary_type != UniqueList.LegendaryType.LegendaryPotential);
            ww.mode.gameObject.SetActive(ww.input.gameObject.activeSelf);
            RefreshPreview();
            return true;
        }

        static void Build()
        {
            if (!FD.Type_Initialized || FD.content_obj.IsNullOrDestroyed()) return;
            foreach (var candidate in Hud_Manager.hud_object.GetComponentsInChildren<TMP_InputField>(true))
                if (candidate.name == "InputField" && candidate.transform.parent != null && candidate.transform.parent.name == "Name")
                { template = candidate; break; }
            if (template.IsNullOrDestroyed()) throw new InvalidOperationException("Input template is unavailable");
            var texts = FD.content_obj.GetComponentsInChildren<Text>(true);
            foreach (var t in texts) if (!t.font.IsNullOrDestroyed()) { font = t.font; break; }
            if (font.IsNullOrDestroyed()) throw new InvalidOperationException("Menu font is unavailable");
            clicks.Clear(); numbers.Clear(); itemButtons.Clear(); pickButtons.Clear();
            root = Panel(FD.content_obj, "ForceDropBuilder", 0, 0, 1, 1);
            Label(root, "Force Drop", 0.02f, 0.92f, 0.98f, 0.99f, 24);
            var left = Panel(root, "Choose item", 0.01f, 0.02f, 0.29f, 0.91f);
            var middle = Panel(root, "Customize", 0.30f, 0.02f, 0.73f, 0.91f);
            var right = Panel(root, "Preview", 0.74f, 0.02f, 0.99f, 0.91f);
            Label(left, "Choose item", .03f, .92f, .97f, .99f, 20);
            Label(left, "Search items", .03f, .86f, .97f, .91f);
            search = Input(left, "Item search", .03f, .80f, .97f, .86f, "", false);
            typeButton = Button(left, "Choose category", .03f, .72f, .97f, .79f, () => CatalogPicker(FD.type_dropdown, FD.SelectType, true));
            rarityButton = Button(left, "Choose rarity", .03f, .64f, .97f, .71f, () => CatalogPicker(FD.rarity_dropdown, FD.SelectRarity, true));
            for (int i = 0; i < 9; i++)
            {
                int slot = i;
                itemButtons.Add(Button(left, "", .03f, .57f - i * .054f, .97f, .62f - i * .054f, () => ChooseItem(slot)));
            }
            Button(left, "Previous", .03f, .035f, .48f, .09f, () => { itemPage = Math.Max(0, itemPage - 1); RefreshItems(); });
            Button(left, "Next", .52f, .035f, .97f, .09f, () => { if ((itemPage + 1) * 9 < itemIndexes.Count) itemPage++; RefreshItems(); });
            Label(middle, "Customize", .03f, .92f, .97f, .99f, 20);
            Button(middle, "Random", .03f, .84f, .32f, .91f, () => Preset(true));
            Button(middle, "Maximum", .35f, .84f, .64f, .91f, () => Preset(false));
            Button(middle, "Custom", .67f, .84f, .97f, .91f, () => { foreach (var n in numbers) n.random = false; RefreshModes(); });
            Button(middle, "Base", .03f, .76f, .32f, .83f, () => Page(0));
            Button(middle, "Affixes", .35f, .76f, .64f, .83f, () => Page(1));
            Button(middle, "Unique", .67f, .76f, .97f, .83f, () => Page(2));
            basePage = Panel(middle, "Base page", .02f, .04f, .98f, .74f);
            affixPage = Panel(middle, "Affix page", .02f, .04f, .98f, .74f);
            uniquePage = Panel(middle, "Unique page", .02f, .04f, .98f, .74f);
            forging = Numeric(basePage, "Forging potential", .86f, 0, 255, 100, false, true);
            for (int i = 0; i < 3; i++) implicits[i] = Numeric(basePage, "Implicit " + (i + 1), .70f - i * .14f, 0, 100, 100, true, true);
            Label(basePage, "Corruption", .03f, .26f, .97f, .34f, 18);
            corruptButton = Button(basePage, "Corrupted: No", .03f, .16f, .97f, .24f, () => { corrupted = !corrupted; Caption(corruptButton, corrupted ? "Corrupted: Yes" : "Corrupted: No"); });
            corruptionSelect = Button(basePage, "Corrupted affix: None", .03f, .07f, .55f, .14f, CorruptionPicker);
            corruptionTier = NumericCompact(basePage, .58f, .07f, .74f, .14f, 1, 7, 7);
            corruptionRoll = NumericCompact(basePage, .77f, .07f, .96f, .14f, 0, 100, 100);
            Label(basePage, "Tier / Roll %", .58f, .01f, .96f, .06f, 12);
            corruptionSelect.interactable = CorruptedAffixAdapter.IsSupported;
            if (!CorruptedAffixAdapter.IsSupported) Caption(corruptionSelect, "Corrupted affix API unavailable");
            for (int i = 0; i < 5; i++)
            {
                int index = i;
                float y = .87f - i * .17f;
                var row = new AffixRow(); rows[i] = row;
                Label(affixPage, i == 4 ? "Sealed affix" : (i < 2 ? "Prefix " + (i + 1) : "Suffix " + (i - 1)), .03f, y + .02f, .45f, y + .09f, 13);
                row.select = Button(affixPage, "None", .03f, y - .06f, .57f, y + .02f, () => AffixPicker(index));
                row.tier = NumericCompact(affixPage, .59f, y - .06f, .73f, y + .02f, 1, 7, 7);
                row.roll = NumericCompact(affixPage, .75f, y - .06f, .86f, y + .02f, 0, 100, 100);
                row.roll.mode = Button(affixPage, "Fixed", .87f, y - .06f, .98f, y + .02f, () => { row.roll.random = !row.roll.random; RefreshModes(); });
                Label(affixPage, "T", .59f, y + .02f, .73f, y + .07f, 12);
                Label(affixPage, "%", .76f, y + .02f, .94f, y + .07f, 12);
            }
            Label(affixPage, "Regular tiers 1–7. Primordial creation uses the rune.", .03f, .01f, .97f, .06f, 12);
            lp = Numeric(uniquePage, "Legendary Potential", .87f, 0, 4, 0, false, true);
            ww = Numeric(uniquePage, "Weaver's Will", .76f, 0, 28, 0, false, true);
            for (int i = 0; i < 8; i++) uniqueRolls[i] = Numeric(uniquePage, "Unique roll " + (i + 1), .65f - i * .08f, 0, 100, 100, true, true);
            Label(right, "Item preview", .04f, .92f, .96f, .99f, 20);
            preview = Label(right, "Choose an item", .04f, .28f, .96f, .90f, 15);
            quantity = Numeric(right, "Quantity", .20f, 1, 99, 1, false, false);
            dropButton = Button(right, "Drop Item", .04f, .105f, .96f, .18f, Drop);
            Button(right, "Reset", .04f, .03f, .96f, .09f, Reset);
            status = Label(root, "", .30f, .00f, .99f, .025f, 12);
            BuildPicker();
            // Hide only after the entire replacement view has been built successfully.
            foreach (var child in Functions.GetAllChild(FD.content_obj)) if (child != root) child.SetActive(false);
            Page(0); RefreshItems(); LogCorruptionMetadata();
        }

        static void Page(int page) { basePage.SetActive(page == 0); affixPage.SetActive(page == 1); uniquePage.SetActive(page == 2); }
        static void Preset(bool random)
        {
            foreach (var n in numbers)
            {
                if (n == quantity || n == corruptionTier || n == corruptionRoll) continue;
                n.random = random && n.mode != null;
                if (!random) { n.value = n.max; n.input.SetTextWithoutNotify(n.value.ToString()); }
            }
            RefreshModes();
        }
        static void RefreshModes()
        {
            foreach (var n in numbers) if (n.mode != null) { Caption(n.mode, n.random ? "Random" : "Fixed"); n.input.interactable = !n.random; }
        }
        static void Reset()
        {
            foreach (var r in rows) { r.id = -1; r.name = "None"; Caption(r.select, "None"); }
            corrupted = false; corruptionId = -1; corruptionName = "None"; Caption(corruptButton, "Corrupted: No");
            quantity.value = 1; quantity.input.SetTextWithoutNotify("1");
            lp.value = ww.value = 0; lp.input.SetTextWithoutNotify("0"); ww.input.SetTextWithoutNotify("0");
            Preset(true); result = "";
        }

        static void RefreshItems()
        {
            itemIndexes.Clear();
            for (int i = 1; i < FD.items_dropdown.options.Count; i++)
                if (FD.items_dropdown.options[i].text.IndexOf(search.text, StringComparison.OrdinalIgnoreCase) >= 0) itemIndexes.Add(i);
            for (int slot = 0; slot < itemButtons.Count; slot++)
            {
                int index = itemPage * 9 + slot;
                itemButtons[slot].gameObject.SetActive(index < itemIndexes.Count);
                if (index < itemIndexes.Count) Caption(itemButtons[slot], FD.items_dropdown.options[itemIndexes[index]].text);
            }
        }
        static void ChooseItem(int slot)
        {
            int i = itemPage * 9 + slot;
            if (i >= itemIndexes.Count) return;
            FD.items_dropdown.SetValueWithoutNotify(itemIndexes[i]); FD.SelectItem();
            foreach (var row in rows) { row.id = -1; row.name = "None"; Caption(row.select, "None"); }
            corruptionId = -1; corruptionName = "None";
            Caption(corruptionSelect, CorruptedAffixAdapter.IsSupported ? "Corrupted affix: None" : "Corrupted affix API unavailable");
            result = "";
        }
        static void CorruptionPicker()
        {
            choices.Clear();
            choices.Add(new Choice { name = "None", select = () => { corruptionId = -1; corruptionName = "None"; Caption(corruptionSelect, "Corrupted affix: None"); } });
            foreach (var a in CorruptedAffixAdapter.Catalog())
            {
                if (!a.CanRollOn(FD.item_type, FD.item_subtype, ItemList.ClassRequirement.Any)) continue;
                int id = a.affixId; string name = a.getAffixDisplayName();
                if (string.IsNullOrEmpty(name)) name = a.affixName;
                choices.Add(new Choice { name = name, select = () => { corruptionId = id; corruptionName = name; corrupted = true; Caption(corruptButton, "Corrupted: Yes"); Caption(corruptionSelect, name); } });
            }
            OpenPicker("Corrupted affix");
        }
        public static void ApplySelectedCorruption(ItemDataUnpacked item)
        {
            if (!IsReady || !corrupted || corruptionId < 0) return;
            CorruptedAffixAdapter.Apply(item, corruptionId, corruptionTier.value - 1, Roll(corruptionRoll));
        }
        static void CatalogPicker(Dropdown catalog, Action changed, bool skipPlaceholder)
        {
            choices.Clear();
            for (int i = skipPlaceholder ? 1 : 0; i < catalog.options.Count; i++)
            {
                int index = i;
                choices.Add(new Choice { name = catalog.options[i].text, select = () => { catalog.SetValueWithoutNotify(index); changed(); foreach (var r in rows) { r.id = -1; r.name = "None"; Caption(r.select, "None"); } corruptionId = -1; corruptionName = "None"; lastItems = ""; } });
            }
            OpenPicker(catalog == FD.type_dropdown ? "Category" : "Rarity");
        }
        static void AffixPicker(int slot)
        {
            choices.Clear();
            choices.Add(new Choice { name = "None", select = () => { rows[slot].id = -1; rows[slot].name = "None"; Caption(rows[slot].select, "None"); } });
            var list = AffixList.get();
            if (list.IsNullOrDestroyed()) return;
            foreach (var a in list.singleAffixes) AddAffixChoice(a, slot);
            foreach (var a in list.multiAffixes) AddAffixChoice(a, slot);
            choices.Sort((a, b) => string.Compare(a.name, b.name, StringComparison.OrdinalIgnoreCase));
            OpenPicker(slot == 4 ? "Sealed affix" : slot < 2 ? "Prefix" : "Suffix");
        }
        static void AddAffixChoice(AffixList.Affix a, int slot)
        {
            if (a.IsNullOrDestroyed() || CorruptedAffixAdapter.IsCorruption(a) || FD.item_type < 0 || FD.item_subtype < 0) return;
            if (slot < 4 && a.type != (slot < 2 ? AffixList.AffixType.PREFIX : AffixList.AffixType.SUFFIX)) return;
            if (!a.CanRollOn(FD.item_type, FD.item_subtype, ItemList.ClassRequirement.Any)) return;
            int id = a.affixId;
            string name = a.getAffixDisplayName(); if (string.IsNullOrEmpty(name)) name = a.affixName;
            choices.Add(new Choice { id = id, name = name, select = () => { rows[slot].id = id; rows[slot].name = name; Caption(rows[slot].select, name); } });
        }

        static string Validate()
        {
            if (FD.type_dropdown.value <= 0 || FD.rarity_dropdown.value <= 0 || FD.items_dropdown.value <= 0 || FD.item_subtype < 0) return "Choose a category, rarity and item.";
            if (Refs_Manager.player_actor.IsNullOrDestroyed() || Refs_Manager.ground_item_manager.IsNullOrDestroyed()) return "Enter the game before dropping items.";
            var ids = new HashSet<int>();
            foreach (var r in rows) if (r.id >= 0 && !ids.Add(r.id)) return "Each affix must be different, including the sealed affix.";
            foreach (var n in numbers) if (!int.TryParse(n.input.text, out int value) || value < n.min || value > n.max) return "Use whole numbers within each field's range.";
            if (FD.item_type >= 100 && corrupted) return "Corruption is available for equipment only.";
            return "";
        }
        static void RefreshPreview()
        {
            var s = new StringBuilder(Selected(FD.items_dropdown, "Choose an item"));
            s.Append("\n\n").Append(Selected(FD.type_dropdown, "")).Append("\n").Append(Selected(FD.rarity_dropdown, ""));
            if (FD.item_type < 100)
            {
                if (FD.item_rarity < 7) s.Append("\nForging potential: ").Append(forging.random ? "Random" : forging.value.ToString());
                foreach (var r in rows) if (r.id >= 0) s.Append("\n\n").Append(r == rows[4] ? "Sealed: " : "").Append(r.name).Append("\nT").Append(r.tier.value).Append(" · ").Append((r.roll.random ? "Random" : r.roll.value.ToString())).Append(" %");
                if (FD.item_rarity > 6) s.Append("\n\n").Append(FD.item_legendary_type == UniqueList.LegendaryType.LegendaryPotential ? "LP: " + (lp.random ? "Random" : lp.value.ToString()) : "Weaver's Will: " + (ww.random ? "Random" : ww.value.ToString()));
            }
            s.Append("\n\nCorrupted: ").Append(corrupted ? "Yes" : "No");
            if (corrupted && corruptionId >= 0) s.Append("\n").Append(corruptionName).Append("\nT").Append(corruptionTier.value).Append(" · ").Append(corruptionRoll.value).Append(" %");
            preview.text = s.ToString();
            string problem = Validate(); status.text = problem.Length > 0 ? problem : result.Length > 0 ? result : "Drops at your character.";
            dropButton.interactable = problem.Length == 0;
        }

        static void Assign(Slider slider, int value)
        {
            if (slider.IsNullOrDestroyed()) throw new InvalidOperationException("Required drop control is missing");
            slider.SetValueWithoutNotify(value);
        }
        static int Sample(Number n) => n.random ? UnityEngine.Random.Range(n.min, n.max + 1) : n.value;
        static int Roll(Number n) => n.random ? UnityEngine.Random.Range(0, 256) : Mathf.RoundToInt(n.value / 100f * 255f);
        static void Drop()
        {
            foreach (var n in numbers) n.Read();
            if (Validate().Length != 0) return;
            try
            {
                for (int copy = 0; copy < quantity.value; copy++)
                {
                bool equipment = FD.item_type < 100;
                FD.implicits_roll = equipment;
                Assign(FD.implicit_0_slider, Roll(implicits[0])); Assign(FD.implicit_1_slider, Roll(implicits[1])); Assign(FD.implicit_2_slider, Roll(implicits[2]));
                FD.forgin_potencial_roll = equipment; Assign(FD.forgin_potencial_slider, Sample(forging));
                FD.affixs_roll = equipment;
                FD.affix_0_id = equipment ? rows[0].id : -1; FD.affix_1_id = equipment ? rows[1].id : -1;
                FD.affix_2_id = equipment ? rows[2].id : -1; FD.affix_3_id = equipment ? rows[3].id : -1;
                FD.affix_4_id = FD.affix_5_id = -1;
                Assign(FD.affix_0_tier_slider, rows[0].tier.value - 1); Assign(FD.affix_1_tier_slider, rows[1].tier.value - 1);
                Assign(FD.affix_2_tier_slider, rows[2].tier.value - 1); Assign(FD.affix_3_tier_slider, rows[3].tier.value - 1);
                Assign(FD.affix_0_value_slider, Roll(rows[0].roll)); Assign(FD.affix_1_value_slider, Roll(rows[1].roll));
                Assign(FD.affix_2_value_slider, Roll(rows[2].roll)); Assign(FD.affix_3_value_slider, Roll(rows[3].roll));
                foreach (var toggle in new[] { FD.affix_0_random_toggle, FD.affix_1_random_toggle, FD.affix_2_random_toggle, FD.affix_3_random_toggle })
                    if (!toggle.IsNullOrDestroyed()) toggle.SetIsOnWithoutNotify(false);
                FD.seal_id = equipment ? rows[4].id : -1; FD.seal_roll = equipment && rows[4].id >= 0;
                Assign(FD.seal_tier_slider, rows[4].tier.value - 1); Assign(FD.seal_value_slider, Roll(rows[4].roll));
                FD.unique_mods_roll = true;
                var sliders = new[] { FD.unique_mod_0_slider, FD.unique_mod_1_slider, FD.unique_mod_2_slider, FD.unique_mod_3_slider, FD.unique_mod_4_slider, FD.unique_mod_5_slider, FD.unique_mod_6_slider, FD.unique_mod_7_slider };
                for (int i = 0; i < sliders.Length; i++) Assign(sliders[i], Roll(uniqueRolls[i]));
                FD.legenday_potencial_roll = true; Assign(FD.legenday_potencial_slider, Sample(lp));
                FD.weaver_will_roll = true; Assign(FD.weaver_will_slider, Sample(ww));
                Assign(FD.forcedrop_quantity_slider, 1);
                if (FD.corrupted_toggle.IsNullOrDestroyed() && corrupted) throw new InvalidOperationException("Corruption control is unavailable");
                if (!FD.corrupted_toggle.IsNullOrDestroyed()) FD.corrupted_toggle.SetIsOnWithoutNotify(corrupted);
                FD.UpdateUI(); FD.Drop();
                }
                result = "Dropped " + quantity.value + " item(s).";
            }
            catch (Exception ex) { result = "Drop failed: " + ex.Message; Main.logger_instance.Error(result); }
        }

        static void BuildPicker()
        {
            picker = Panel(root, "Search picker", .15f, .08f, .85f, .89f);
            pickerTitle = Label(picker, "Select", .03f, .90f, .83f, .98f, 20);
            Button(picker, "Close", .84f, .90f, .97f, .98f, () => picker.SetActive(false));
            pickerSearch = Input(picker, "Search choices", .03f, .81f, .97f, .88f, "", false);
            for (int i = 0; i < 10; i++)
            {
                int slot = i;
                pickButtons.Add(Button(picker, "", .03f, .735f - i * .063f, .97f, .79f - i * .063f, () =>
                {
                    int index = pickerPage * 10 + slot;
                    if (index >= filtered.Count) return;
                    filtered[index].select(); picker.SetActive(false);
                }));
            }
            Button(picker, "Previous", .03f, .03f, .48f, .085f, () => { pickerPage = Math.Max(0, pickerPage - 1); RefreshPicker(); });
            Button(picker, "Next", .52f, .03f, .97f, .085f, () => { if ((pickerPage + 1) * 10 < filtered.Count) pickerPage++; RefreshPicker(); });
            picker.SetActive(false);
        }
        static void OpenPicker(string title)
        {
            pickerTitle.text = title; pickerSearch.SetTextWithoutNotify(""); lastPickerSearch = ""; pickerPage = 0;
            picker.SetActive(true); picker.transform.SetAsLastSibling(); RefreshPicker();
        }
        static void RefreshPicker()
        {
            filtered.Clear();
            foreach (var c in choices) if ((c.name ?? "").IndexOf(pickerSearch.text, StringComparison.OrdinalIgnoreCase) >= 0) filtered.Add(c);
            for (int slot = 0; slot < pickButtons.Count; slot++)
            {
                int index = pickerPage * 10 + slot; pickButtons[slot].gameObject.SetActive(index < filtered.Count);
                if (index < filtered.Count) Caption(pickButtons[slot], filtered[index].name);
            }
        }

        static GameObject Panel(GameObject parent, string name, float x0, float y0, float x1, float y1)
        {
            var go = new GameObject(name); go.AddComponent<RectTransform>(); go.transform.SetParent(parent.transform, false);
            Rect(go, x0, y0, x1, y1);
            go.AddComponent<Image>().color = dark;
            var outline = go.AddComponent<Outline>(); outline.effectColor = new Color(gold.r, gold.g, gold.b, .65f); outline.effectDistance = new Vector2(1f, -1f);
            return go;
        }
        static void Rect(GameObject go, float x0, float y0, float x1, float y1)
        {
            var rect = go.GetComponent<RectTransform>(); rect.anchorMin = new Vector2(x0, y0); rect.anchorMax = new Vector2(x1, y1);
            rect.offsetMin = rect.offsetMax = Vector2.zero; rect.localScale = Vector3.one;
        }
        static Text Label(GameObject parent, string text, float x0, float y0, float x1, float y1, int size = 15)
        {
            var go = new GameObject("Label"); go.AddComponent<RectTransform>(); go.transform.SetParent(parent.transform, false); Rect(go, x0, y0, x1, y1);
            var label = go.AddComponent<Text>(); label.font = font; label.fontSize = size; label.color = gold; label.text = text; label.raycastTarget = false;
            label.alignment = TextAnchor.UpperLeft; label.horizontalOverflow = HorizontalWrapMode.Wrap; label.verticalOverflow = VerticalWrapMode.Truncate;
            return label;
        }
        static Button Button(GameObject parent, string text, float x0, float y0, float x1, float y1, Action action)
        {
            var go = Panel(parent, "Button", x0, y0, x1, y1); var button = go.AddComponent<Button>(); button.targetGraphic = go.GetComponent<Image>();
            var label = Label(go, text, .025f, .04f, .975f, .96f); label.alignment = TextAnchor.MiddleLeft;
            clicks[button.GetInstanceID()] = action;
            return button;
        }
        static void Caption(Button button, string text) { button.GetComponentInChildren<Text>(true).text = text; }
        static string Selected(Dropdown catalog, string fallback) => catalog.value > 0 && catalog.value < catalog.options.Count ? catalog.options[catalog.value].text : fallback;
        static TMP_InputField Input(GameObject parent, string name, float x0, float y0, float x1, float y1, string value, bool numeric)
        {
            var go = UnityEngine.Object.Instantiate(template.gameObject, parent.transform); go.name = name; Rect(go, x0, y0, x1, y1);
            var layout = go.GetComponent<LayoutElement>(); if (!layout.IsNullOrDestroyed()) layout.ignoreLayout = true;
            var input = go.GetComponent<TMP_InputField>(); input.onValueChanged.RemoveAllListeners(); input.onEndEdit.RemoveAllListeners();
            input.enabled = true; input.readOnly = false; input.interactable = true; input.characterLimit = numeric ? 8 : 100;
            input.contentType = numeric ? TMP_InputField.ContentType.IntegerNumber : TMP_InputField.ContentType.Standard;
            input.SetTextWithoutNotify(value); input.targetGraphic.raycastTarget = true;
            var background = go.GetComponent<Image>(); if (!background.IsNullOrDestroyed()) { background.sprite = null; background.color = new Color(.16f, .18f, .21f); }
            if (!input.placeholder.IsNullOrDestroyed()) input.placeholder.gameObject.SetActive(false);
            input.textComponent.color = gold; input.textComponent.fontSize = 15; input.textComponent.enableAutoSizing = true;
            input.textComponent.fontSizeMin = 10; input.textComponent.fontSizeMax = 15; input.textComponent.raycastTarget = false;
            input.textComponent.margin = Vector4.zero;
            if (!input.textViewport.IsNullOrDestroyed()) Rect(input.textViewport.gameObject, .04f, .05f, .96f, .95f);
            Rect(input.textComponent.gameObject, 0, 0, 1, 1);
            var group = go.GetComponent<CanvasGroup>(); if (!group.IsNullOrDestroyed()) { group.alpha = 1f; group.interactable = true; group.blocksRaycasts = true; }
            go.SetActive(true); return input;
        }
        static Number NumericCompact(GameObject parent, float x0, float y0, float x1, float y1, int min, int max, int value)
        {
            var n = new Number { min = min, max = max, value = value, input = Input(parent, "Number", x0, y0, x1, y1, value.ToString(), true) };
            numbers.Add(n); return n;
        }
        static Number Numeric(GameObject parent, string name, float y, int min, int max, int value, bool percent, bool mode)
        {
            Label(parent, name + (percent ? " (%)" : ""), .03f, y - .015f, .56f, y + .055f, 14);
            var n = NumericCompact(parent, .57f, y - .025f, mode ? .77f : .96f, y + .055f, min, max, value);
            if (mode) n.mode = Button(parent, "Fixed", .79f, y - .025f, .97f, y + .055f, () => { n.random = !n.random; RefreshModes(); });
            return n;
        }
        static void LogCorruptionMetadata()
        {
            if (metadataLogged) return; metadataLogged = true;
            Main.logger_instance.Msg("Force Drop affix types: " + string.Join(", ", Enum.GetNames(typeof(AffixList.AffixType))));
            foreach (var type in new[] { typeof(ItemData), typeof(ItemDataUnpacked), typeof(ItemAffix), typeof(AffixList), typeof(AffixList.Affix) })
                foreach (var member in type.GetMembers())
                    if (member.Name.IndexOf("corrupt", StringComparison.OrdinalIgnoreCase) >= 0)
                        Main.logger_instance.Msg("Force Drop corruption API: " + type.Name + "." + member);
        }
        [HarmonyPatch(typeof(Button), "Press")]
        public class ButtonPress
        {
            [HarmonyPostfix]
            static void Postfix(Button __instance)
            {
                if (!__instance.interactable || !__instance.gameObject.activeInHierarchy) return;
                if (clicks.TryGetValue(__instance.GetInstanceID(), out var action))
                    try { action(); } catch (Exception ex) { Main.logger_instance.Error("Force Drop action: " + ex.Message); }
            }
        }
    }
}
