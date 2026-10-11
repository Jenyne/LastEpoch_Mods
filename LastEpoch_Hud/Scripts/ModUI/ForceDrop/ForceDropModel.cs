using Il2Cpp;
using UnityEngine;
using UnityEngine.UI;

namespace LastEpoch_Hud.Scripts.ModUI.ForceDrop;

/// <summary>Force Drop catalog selection backed by the hidden prefab dropdowns.</summary>
internal static class ForceDropModel
{
    public static bool Initialized;
    public static bool Enabled;

    public static GameObject ContentObject;

    public static Dropdown TypeDropdown;
    public static int ItemType = -1;
    public static bool TypeInitialized;
    public static bool InitializingType;

    public static Dropdown RarityDropdown;
    public static int ItemRarity = -1;

    public static Dropdown ItemsDropdown;
    public static int ItemSubtype = -1;
    public static int ItemUniqueId = -1;

    public static UniqueList.LegendaryType ItemLegendaryType = UniqueList
        .LegendaryType
        .LegendaryPotential;

    public static void GetRefs()
    {
        if (Hud_Manager.Content.content_obj.IsNullOrDestroyed())
        {
            return;
        }
        ContentObject = Functions.GetChild(
            Hud_Manager.Content.content_obj,
            "Old_ForceDrop_Content"
        );
        if (ContentObject.IsNullOrDestroyed())
        {
            Main.logger_instance.Error("content_obj is null");
            return;
        }
        GameObject leftBaseContent = Functions.GetViewportContent(ContentObject, "Left", "Item");
        if (leftBaseContent.IsNullOrDestroyed())
        {
            Main.logger_instance.Error("left_obj not found");
            return;
        }
        TypeDropdown = Functions.Get_DopboxInPanel(
            leftBaseContent,
            "Type",
            "Dropdown_Items_ForceDrop_Type",
            new System.Action<int>(
                (_) =>
                {
                    SelectType();
                }
            )
        );
        RarityDropdown = Functions.Get_DopboxInPanel(
            leftBaseContent,
            "Rarity",
            "Dropdown_Items_ForceDrop_Rarity",
            new System.Action<int>(
                (_) =>
                {
                    SelectRarity();
                }
            )
        );
        ItemsDropdown = Functions.Get_DopboxInPanel(
            leftBaseContent,
            "Item",
            "Dropdown_Items_ForceDrop_Item",
            new System.Action<int>(
                (_) =>
                {
                    SelectItem();
                }
            )
        );
        if (TypeDropdown.IsNullOrDestroyed())
        {
            Main.logger_instance.Error("Error type_dropdown not found");
        }
        if (RarityDropdown.IsNullOrDestroyed())
        {
            Main.logger_instance.Error("Error rarity_dropdown not found");
        }
        if (ItemsDropdown.IsNullOrDestroyed())
        {
            Main.logger_instance.Error("Error items_dropdown not found");
        }
        Initialized =
            !TypeDropdown.IsNullOrDestroyed()
            && !RarityDropdown.IsNullOrDestroyed()
            && !ItemsDropdown.IsNullOrDestroyed();
    }

    /// <summary>Hides every prefab child of the Force Drop page once.</summary>
    public static void HideLegacyControls()
    {
        if (ContentObject.IsNullOrDestroyed())
        {
            return;
        }
        foreach (GameObject child in Functions.GetAllChild(ContentObject))
        {
            child.SetActive(false);
        }
    }

    public static void SetActive(bool show)
    {
        if (!ContentObject.IsNullOrDestroyed())
        {
            ContentObject.active = show;
            Enabled = show;
        }
    }

    public static void InitForceDrop()
    {
        if ((!TypeInitialized) && (!InitializingType))
        {
            InitializingType = true;
            try
            {
                TypeDropdown.ClearOptions();
                var options = new Il2CppSystem.Collections.Generic.List<Dropdown.OptionData>();
                options.Add(new Dropdown.OptionData { text = "Select" });
                foreach (ItemList.BaseEquipmentItem item in ItemList.get().EquippableItems)
                {
                    options.Add(new Dropdown.OptionData { text = item.BaseTypeName });
                }
                foreach (ItemList.BaseNonEquipmentItem item in ItemList.get().nonEquippableItems)
                {
                    options.Add(new Dropdown.OptionData { text = item.BaseTypeName });
                }
                TypeDropdown.options = options;
                TypeDropdown.value = 0;

                RarityDropdown.ClearOptions();
                RarityDropdown.enabled = false;

                ItemsDropdown.ClearOptions();
                ItemsDropdown.enabled = false;

                TypeInitialized = true;
            }
            catch (System.Exception ex)
            {
                Main.logger_instance.Error("Force drop list init error : " + ex.Message);
            }
            InitializingType = false;
        }
    }

    public static void SelectType()
    {
        if (TypeInitialized)
        {
            int index = TypeDropdown.value;
            if (index < TypeDropdown.options.Count)
            {
                string typeStr = TypeDropdown.options[TypeDropdown.value].text;
                ItemType = -1;
                bool found = false;
                foreach (ItemList.BaseEquipmentItem item in ItemList.get().EquippableItems)
                {
                    if (item.BaseTypeName == typeStr)
                    {
                        ItemType = item.baseTypeID;
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    foreach (
                        ItemList.BaseNonEquipmentItem item in ItemList.get().nonEquippableItems
                    )
                    {
                        if (item.BaseTypeName == typeStr)
                        {
                            ItemType = item.baseTypeID;
                            break;
                        }
                    }
                }
                UpdateRarity();
                UpdateItems();
            }
        }
    }

    public static void UpdateRarity()
    {
        if (
            (Enabled)
            && (LastEpoch_Hud.Scenes.IsGameScene())
            && (!Refs_Manager.item_list.IsNullOrDestroyed())
            && (TypeInitialized)
            && (!TypeDropdown.IsNullOrDestroyed())
            && (!RarityDropdown.IsNullOrDestroyed())
            && (!ItemsDropdown.IsNullOrDestroyed())
        )
        {
            RarityDropdown.ClearOptions();
            var options = new Il2CppSystem.Collections.Generic.List<Dropdown.OptionData>();
            options.Add(new Dropdown.OptionData { text = "Select" });
            if ((TypeDropdown.value > 0) && (ItemType > -1))
            {
                bool hasUnique = false;
                bool hasSet = false;
                if (UniqueList.instance.IsNullOrDestroyed())
                {
                    UniqueList.getUnique(0);
                }
                if (!UniqueList.instance.IsNullOrDestroyed())
                {
                    foreach (UniqueList.Entry unique in UniqueList.instance.uniques)
                    {
                        if (unique.baseType == ItemType)
                        {
                            if (unique.isSetItem)
                            {
                                hasSet = true;
                            }
                            else
                            {
                                hasUnique = true;
                            }
                        }
                    }
                }
                options.Add(new Dropdown.OptionData { text = "Base Item" });
                if (hasUnique)
                {
                    options.Add(new Dropdown.OptionData { text = "Unique" });
                }
                if (hasSet)
                {
                    options.Add(new Dropdown.OptionData { text = "Set" });
                }
                RarityDropdown.enabled = true;
            }
            else
            {
                RarityDropdown.enabled = false;
            }
            RarityDropdown.options = options;
            RarityDropdown.value = 0;
            ItemRarity = -1;
        }
    }

    public static void SelectRarity()
    {
        if (TypeInitialized)
        {
            int index = RarityDropdown.value;
            if (index < RarityDropdown.options.Count)
            {
                string rarityStr = RarityDropdown.options[index].text;
                ItemRarity = RarityFromLabel(rarityStr);
                UpdateItems();
            }
        }
    }

    public static void UpdateItems()
    {
        if (
            (Enabled)
            && (LastEpoch_Hud.Scenes.IsGameScene())
            && (!Refs_Manager.item_list.IsNullOrDestroyed())
            && (TypeInitialized)
            && (!ItemsDropdown.IsNullOrDestroyed())
        )
        {
            ItemsDropdown.ClearOptions();

            var options = new Il2CppSystem.Collections.Generic.List<Dropdown.OptionData>();
            options.Add(new Dropdown.OptionData { text = "Select" });
            if ((ItemType > -1) && (ItemRarity > -1))
            {
                if (ItemRarity == 0)
                {
                    bool typeFound = false;
                    foreach (ItemList.BaseEquipmentItem baseItem in ItemList.get().EquippableItems)
                    {
                        if (baseItem.baseTypeID == ItemType)
                        {
                            foreach (ItemList.EquipmentItem item in baseItem.subItems)
                            {
                                string name = item.displayName;
                                if (name == "")
                                {
                                    name = item.name;
                                }
                                options.Add(new Dropdown.OptionData { text = name });
                            }
                            typeFound = true;
                        }
                    }
                    if (!typeFound)
                    {
                        foreach (
                            ItemList.BaseNonEquipmentItem baseItem in ItemList
                                .get()
                                .nonEquippableItems
                        )
                        {
                            if (baseItem.baseTypeID == ItemType)
                            {
                                foreach (ItemList.NonEquipmentItem item in baseItem.subItems)
                                {
                                    string name = item.displayName;
                                    if (name == "")
                                    {
                                        name = item.name;
                                    }
                                    options.Add(new Dropdown.OptionData { text = name });
                                }
                            }
                        }
                    }
                }
                if ((ItemRarity == 7) || (ItemRarity == 8))
                {
                    if (UniqueList.instance.IsNullOrDestroyed())
                    {
                        UniqueList.getUnique(0);
                    }
                    if (!UniqueList.instance.IsNullOrDestroyed())
                    {
                        foreach (UniqueList.Entry unique in UniqueList.instance.uniques)
                        {
                            if (
                                (unique.baseType == ItemType)
                                && (
                                    ((ItemRarity == 7) && (!unique.isSetItem))
                                    || ((ItemRarity == 8) && (unique.isSetItem))
                                )
                            )
                            {
                                string name = unique.displayName;
                                if (name == "")
                                {
                                    name = unique.name;
                                }
                                options.Add(new Dropdown.OptionData { text = name });
                            }
                        }
                    }
                }
                ItemsDropdown.enabled = true;
            }
            else
            {
                ItemsDropdown.enabled = false;
            }
            ItemsDropdown.options = options;
            ItemsDropdown.value = 0;
        }
    }

    public static void SelectItem()
    {
        if (TypeInitialized)
        {
            int index = ItemsDropdown.value;
            if (index < ItemsDropdown.options.Count)
            {
                string itemStr = ItemsDropdown.options[ItemsDropdown.value].text;

                ItemSubtype = -1;
                ItemUniqueId = 0;

                bool itemFound = false;
                if (ItemRarity == 0)
                {
                    foreach (ItemList.BaseEquipmentItem baseItem in ItemList.get().EquippableItems)
                    {
                        if (baseItem.baseTypeID == ItemType)
                        {
                            foreach (ItemList.EquipmentItem item in baseItem.subItems)
                            {
                                if ((itemStr == item.displayName) || (itemStr == item.name))
                                {
                                    ItemSubtype = item.subTypeID;
                                    itemFound = true;
                                    break;
                                }
                            }
                        }
                    }
                    if (!itemFound)
                    {
                        foreach (
                            ItemList.BaseNonEquipmentItem baseItem in ItemList
                                .get()
                                .nonEquippableItems
                        )
                        {
                            if (baseItem.baseTypeID == ItemType)
                            {
                                foreach (ItemList.NonEquipmentItem item in baseItem.subItems)
                                {
                                    if ((itemStr == item.displayName) || (itemStr == item.name))
                                    {
                                        ItemSubtype = item.subTypeID;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
                if ((ItemRarity == 7) || (ItemRarity == 8))
                {
                    if (UniqueList.instance.IsNullOrDestroyed())
                    {
                        UniqueList.getUnique(0);
                    }
                    if (!UniqueList.instance.IsNullOrDestroyed())
                    {
                        foreach (UniqueList.Entry unique in UniqueList.instance.uniques)
                        {
                            if ((itemStr == unique.displayName) || (itemStr == unique.name))
                            {
                                ItemSubtype = unique.subTypes[0]; //need to be fix here
                                ItemUniqueId = unique.uniqueID;
                                ItemLegendaryType = unique.legendaryType;
                                break;
                            }
                        }
                    }
                }
            }
        }
    }

    private static int RarityFromLabel(string label)
    {
        if (label == "Base Item")
        {
            return 0;
        }
        if (label == "Unique")
        {
            return 7;
        }
        if (label == "Set")
        {
            return 8;
        }
        return -1;
    }
}
