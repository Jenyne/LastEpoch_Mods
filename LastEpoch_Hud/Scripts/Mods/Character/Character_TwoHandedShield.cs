using HarmonyLib;
using MelonLoader;
using UnityEngine;

namespace LastEpoch_Hud.Scripts.Mods.Character
{
    [RegisterTypeInIl2Cpp]
    public class Character_TwoHandedShield : MonoBehaviour
    {
        public Character_TwoHandedShield(System.IntPtr ptr)
            : base(ptr) { }

        public static Character_TwoHandedShield instance { get; private set; }

        private const int SHIELD_BASE_TYPE = (int)Il2Cpp.EquipmentType.SHIELD;
        private static int lastMainHandType = -1;
        private static int lastOffHandType = -1;
        private static bool applied;
        private static Il2CppSystem.Collections.Generic.List<int> allowedTypes;

        void Awake()
        {
            instance = this;
        }

        public static bool Enabled()
        {
            if (Save_Manager.instance.IsNullOrDestroyed() || Save_Manager.instance.data.IsNullOrDestroyed())
            {
                return false;
            }

            return Save_Manager.instance.data.Character.Cheats.Enable_TwoHandedWithShield;
        }

        static Il2CppSystem.Collections.Generic.List<int> AllowedTypes()
        {
            if (allowedTypes == null)
            {
                allowedTypes = new Il2CppSystem.Collections.Generic.List<int>();
                allowedTypes.Add((int)Il2Cpp.EquipmentType.TWO_HANDED_AXE);
                allowedTypes.Add((int)Il2Cpp.EquipmentType.TWO_HANDED_MACE);
                allowedTypes.Add((int)Il2Cpp.EquipmentType.TWO_HANDED_SPEAR);
                allowedTypes.Add((int)Il2Cpp.EquipmentType.TWO_HANDED_STAFF);
                allowedTypes.Add((int)Il2Cpp.EquipmentType.TWO_HANDED_SWORD);
                allowedTypes.Add((int)Il2Cpp.EquipmentType.BOW);
                allowedTypes.Add((int)Il2Cpp.EquipmentType.CROSSBOW);
            }

            return allowedTypes;
        }

        void Update()
        {
            if (Enabled())
            {
                Il2Cpp.CharacterMutator.global2hWeaponWithShieldBaseTypes = AllowedTypes();
                applied = true;
            }
            else if (applied)
            {
                Il2Cpp.CharacterMutator.global2hWeaponWithShieldBaseTypes = new Il2CppSystem.Collections.Generic.List<int>();
                applied = false;
            }
        }

        static bool IsNonMeleeTwoHander(int itemType)
        {
            return itemType == (int)Il2Cpp.EquipmentType.TWO_HANDED_STAFF
                || itemType == (int)Il2Cpp.EquipmentType.BOW
                || itemType == (int)Il2Cpp.EquipmentType.CROSSBOW;
        }

        [HarmonyPatch(typeof(Il2Cpp.OffhandItemContainer), nameof(Il2Cpp.OffhandItemContainer.IncompatibleDueTo2hWeapon))]
        public class OffhandItemContainer_IncompatibleDueTo2hWeapon
        {
            [HarmonyPrefix]
            static bool Prefix(ref bool __result)
            {
                if (!Enabled())
                {
                    return true;
                }

                __result = false;
                return false;
            }
        }

        [HarmonyPatch(typeof(Il2Cpp.ItemContainersManager), nameof(Il2Cpp.ItemContainersManager.CannotPutItemInMainHandBecauseOfOffhand))]
        public class ItemContainersManager_CannotPutMainHand
        {
            [HarmonyPrefix]
            static bool Prefix(ref bool __result)
            {
                if (!Enabled())
                {
                    return true;
                }

                __result = false;
                return false;
            }
        }

        [HarmonyPatch(typeof(Il2Cpp.ItemContainersManager), nameof(Il2Cpp.ItemContainersManager.CannotPutItemInOffhandBecauseOfMainhand))]
        public class ItemContainersManager_CannotPutOffhand
        {
            [HarmonyPrefix]
            static bool Prefix(ref bool __result)
            {
                if (!Enabled())
                {
                    return true;
                }

                __result = false;
                return false;
            }
        }

        [HarmonyPatch(typeof(Il2Cpp.ItemContainersManager), nameof(Il2Cpp.ItemContainersManager.UpdateOffHandWeaponIfInvalid))]
        public class ItemContainersManager_UpdateOffHandWeaponIfInvalid
        {
            [HarmonyPrefix]
            static bool Prefix()
            {
                return !Enabled();
            }
        }

        // Equipping a shield calls this and drops the two-hander. Equipping the two-hander
        // afterwards does not drop the shield, so only the offhand path is skipped.
        [HarmonyPatch(typeof(Il2Cpp.OffhandItemContainer), "EnforceWeaponLimitations")]
        public class OffhandItemContainer_EnforceWeaponLimitations
        {
            [HarmonyPrefix]
            static bool Prefix()
            {
                return !Enabled();
            }
        }

        [HarmonyPatch(typeof(Il2Cpp.ItemContainersManager), nameof(Il2Cpp.ItemContainersManager.EnforceEquipLimitsIfWeaponSlot))]
        public class ItemContainersManager_EnforceEquipLimitsIfWeaponSlot
        {
            [HarmonyPrefix]
            static bool Prefix(Il2Cpp.ContainerID id)
            {
                if (!Enabled())
                {
                    return true;
                }

                return id != Il2Cpp.ContainerID.EQ_OFFHAND;
            }
        }

        [HarmonyPatch(typeof(Il2Cpp.EquipmentVisualsManager), nameof(Il2Cpp.EquipmentVisualsManager.EquipWeapon))]
        public class EquipmentVisualsManager_EquipWeapon
        {
            [HarmonyPrefix]
            static bool Prefix(int itemType, Il2Cpp.IMSlotType slotType)
            {
                if (!Enabled())
                {
                    return true;
                }

                if (slotType == Il2Cpp.IMSlotType.OffHand && itemType == SHIELD_BASE_TYPE && IsNonMeleeTwoHander(lastMainHandType))
                {
                    lastOffHandType = itemType;
                    return false;
                }

                return true;
            }

            [HarmonyPostfix]
            static void Postfix(Il2Cpp.EquipmentVisualsManager __instance, int itemType, Il2Cpp.IMSlotType slotType)
            {
                if (slotType == Il2Cpp.IMSlotType.MainHand)
                {
                    lastMainHandType = itemType;
                }
                else if (slotType == Il2Cpp.IMSlotType.OffHand)
                {
                    lastOffHandType = itemType;
                }

                if (!Enabled())
                {
                    return;
                }

                if (slotType == Il2Cpp.IMSlotType.MainHand && IsNonMeleeTwoHander(itemType) && lastOffHandType == SHIELD_BASE_TYPE)
                {
                    __instance.RemoveWeapon(true, false);
                }
            }
        }

        [HarmonyPatch(typeof(Il2Cpp.EquipmentVisualsManager), nameof(Il2Cpp.EquipmentVisualsManager.RemoveWeapon))]
        public class EquipmentVisualsManager_RemoveWeapon
        {
            [HarmonyPostfix]
            static void Postfix(bool offHand, bool clearData)
            {
                if (!clearData)
                {
                    return;
                }

                if (offHand)
                {
                    lastOffHandType = -1;
                }
                else
                {
                    lastMainHandType = -1;
                }
            }
        }
    }
}
