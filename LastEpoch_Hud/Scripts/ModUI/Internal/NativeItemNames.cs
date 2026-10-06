using System;
using System.Collections.Generic;
using Il2Cpp;
using UnityEngine.UI;
using FD = LastEpoch_Hud.Scripts.Hud_Manager.Content.OdlForceDrop;

namespace LastEpoch_Hud.Scripts.ModUI;

// Native translations are presentation only. Selection uses catalog indices and ids.
internal static class NativeItemNames
{
    internal sealed class Category
    {
        public int baseType;
        public string raw,
            name;
    }

    internal sealed class ItemChoice
    {
        public int subType;
        public ushort uniqueId;
        public UniqueList.LegendaryType legendaryType;
        public string raw,
            name,
            aliases;
    }

    public static string Locale
    {
        get
        {
            try
            {
                return Il2Cpp.Localization.Locale ?? "";
            }
            catch
            {
                return "";
            }
        }
    }

    static string Name(Func<string> translated, string fallback)
    {
        try
        {
            string value = translated();
            if (!string.IsNullOrWhiteSpace(value))
                return value;
        }
        catch { }
        return fallback ?? "";
    }

    static string Raw(string display, string internalName)
    {
        return !string.IsNullOrWhiteSpace(display) ? display : internalName ?? "";
    }

    public static string RarityName(string raw)
    {
        string fallback = LocaleRegistry.Translate(raw);
        if (raw == "Unique")
            return Name(() => Il2Cpp.Localization.Items.GetRarityName(7, false), fallback);
        if (raw == "Set")
            return Name(() => Il2Cpp.Localization.Items.GetRarityName(8, false), fallback);
        return fallback;
    }

    public static string AffixName(AffixList.Affix definition)
    {
        if (definition.IsNullOrDestroyed())
            return "None";
        string fallback = Raw(definition.affixDisplayName, definition.affixName);
        return Name(
            () => Il2Cpp.Localization.Items.GetAffixDisplayName(definition.affixId, fallback),
            fallback
        );
    }

    public static string AffixName(int id, string fallback)
    {
        return Name(() => Il2Cpp.Localization.Items.GetAffixDisplayName(id, fallback), fallback);
    }

    public static string AffixAliases(AffixList.Affix definition)
    {
        return definition.IsNullOrDestroyed()
            ? ""
            : definition.affixName + "\n" + definition.affixDisplayName;
    }

    public static Dictionary<int, Category> Categories(Dropdown dropdown)
    {
        var result = new Dictionary<int, Category>();
        var list = ItemList.get();
        if (list.IsNullOrDestroyed() || dropdown.IsNullOrDestroyed())
            return result;
        var entries = new List<Category>();
        foreach (var type in list.EquippableItems)
            entries.Add(new Category { baseType = type.baseTypeID, raw = type.BaseTypeName });
        foreach (var type in list.nonEquippableItems)
            entries.Add(new Category { baseType = type.baseTypeID, raw = type.BaseTypeName });
        // Refuse an index map if the legacy catalog has changed order or contents.
        if (dropdown.options.Count != entries.Count + 1)
            return result;
        for (int i = 0; i < entries.Count; i++)
        {
            var entry = entries[i];
            if (!string.Equals(dropdown.options[i + 1].text, entry.raw, StringComparison.Ordinal))
                return new Dictionary<int, Category>();
            entry.name = Name(
                () => Il2Cpp.Localization.Items.GetBaseTypeName(entry.baseType),
                entry.raw
            );
            result.Add(i + 1, entry);
        }
        return result;
    }

    public static Dictionary<int, ItemChoice> Items(Dropdown dropdown, int baseType, int rarity)
    {
        var result = new Dictionary<int, ItemChoice>();
        var list = ItemList.get();
        if (list.IsNullOrDestroyed() || dropdown.IsNullOrDestroyed())
            return result;
        var entries = new List<ItemChoice>();
        if (rarity == 0)
        {
            bool equipment = false;
            foreach (var type in list.EquippableItems)
                if (type.baseTypeID == baseType)
                {
                    equipment = true;
                    foreach (var item in type.subItems)
                        entries.Add(
                            BaseItem(baseType, item.subTypeID, item.displayName, item.name)
                        );
                }
            if (!equipment)
                foreach (var type in list.nonEquippableItems)
                    if (type.baseTypeID == baseType)
                        foreach (var item in type.subItems)
                            entries.Add(
                                BaseItem(baseType, item.subTypeID, item.displayName, item.name)
                            );
        }
        else if ((rarity == 7 || rarity == 8) && !UniqueList.instance.IsNullOrDestroyed())
            foreach (var unique in UniqueList.instance.uniques)
                if (unique.baseType == baseType && unique.isSetItem == (rarity == 8))
                {
                    if (unique.subTypes.IsNullOrDestroyed() || unique.subTypes.Count == 0)
                        return result;
                    string raw = Raw(unique.displayName, unique.name);
                    ushort id = unique.uniqueID;
                    entries.Add(
                        new ItemChoice
                        {
                            subType = unique.subTypes[0],
                            uniqueId = id,
                            legendaryType = unique.legendaryType,
                            raw = raw,
                            name = Name(
                                () => Il2Cpp.Localization.Items.GetUniqueName(id, true, false),
                                raw
                            ),
                            aliases =
                                raw + "\n" + unique.name + "\n" + unique.alternativeSearchName,
                        }
                    );
                }
        if (dropdown.options.Count != entries.Count + 1)
            return result;
        for (int i = 0; i < entries.Count; i++)
        {
            if (
                !string.Equals(
                    dropdown.options[i + 1].text,
                    entries[i].raw,
                    StringComparison.Ordinal
                )
            )
                return new Dictionary<int, ItemChoice>();
            result.Add(i + 1, entries[i]);
        }
        return result;
    }

    static ItemChoice BaseItem(int baseType, int subType, string display, string internalName)
    {
        string raw = Raw(display, internalName);
        return new ItemChoice
        {
            subType = subType,
            raw = raw,
            name = Name(() => Il2Cpp.Localization.Items.GetSubTypeName(baseType, subType), raw),
            aliases = raw + "\n" + internalName,
        };
    }

    public static void SelectCategory(Category category)
    {
        FD.item_type = category.baseType;
        FD.UpdateRarity();
        FD.UpdateItems();
        FD.shard_initialized = false;
        FD.UpdateUI();
    }

    public static void SelectItem(ItemChoice item)
    {
        FD.item_subtype = item.subType;
        FD.item_unique_id = item.uniqueId;
        FD.item_legendary_type = item.legendaryType;
        FD.shard_initialized = false;
        FD.UpdateUI();
    }

    public static bool Matches(string query, string translated, string aliases)
    {
        return string.IsNullOrEmpty(query)
            || (translated ?? "").IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0
            || (aliases ?? "").IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0;
    }
}
