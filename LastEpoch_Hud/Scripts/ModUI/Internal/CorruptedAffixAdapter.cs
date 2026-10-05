using System;
using System.Collections.Generic;
using System.Reflection;
using Il2Cpp;

namespace LastEpoch_Hud.Scripts.ModUI
{
    // Capability checks against the running game wrappers. Never infer corruption from an id.
    public static class CorruptedAffixAdapter
    {
        static MemberInfo storage;
        static bool checkedStorage;
        static bool flagStorage;
        static readonly List<AffixList.Affix> catalog = new List<AffixList.Affix>();
        static bool catalogLoaded;

        public static bool IsSupported
        {
            get { FindStorage(); LoadCatalog(); return storage != null && catalog.Count > 0; }
        }
        static void FindStorage()
        {
            if (checkedStorage) return;
            checkedStorage = true;
            foreach (string name in new[] { "corruptedAffix", "CorruptedAffix" })
            {
                var property = typeof(ItemDataUnpacked).GetProperty(name);
                if (property != null && property.CanRead && property.CanWrite && property.PropertyType == typeof(ItemAffix)) { storage = property; return; }
                var field = typeof(ItemDataUnpacked).GetField(name);
                if (field != null && !field.IsInitOnly && field.FieldType == typeof(ItemAffix)) { storage = field; return; }
            }
            // Packed items carry a separate corrupted-affix presence flag and place that
            // affix after a sealed affix, before ordinary affixes. Require the live wrapper
            // to expose the matching writable flag before using this representation.
            foreach (string name in new[] { "hasCorruptedAffix", "HasCorruptedAffix" })
            {
                var property = typeof(ItemDataUnpacked).GetProperty(name);
                if (property != null && property.CanRead && property.CanWrite && property.PropertyType == typeof(bool)) { storage = property; flagStorage = true; return; }
                var field = typeof(ItemDataUnpacked).GetField(name);
                if (field != null && !field.IsInitOnly && field.FieldType == typeof(bool)) { storage = field; flagStorage = true; return; }
            }
        }
        public static bool IsCorruption(AffixList.Affix affix)
        {
            string type = affix.type.ToString();
            if (type.Equals("Corrupted", StringComparison.OrdinalIgnoreCase) || type.Equals("Corruption", StringComparison.OrdinalIgnoreCase)) return true;
            foreach (string name in new[] { "isCorruptedAffix", "isCorruptionAffix" })
            {
                var property = affix.GetType().GetProperty(name);
                if (property != null && property.PropertyType == typeof(bool) && property.CanRead && (bool)property.GetValue(affix)) return true;
                var field = affix.GetType().GetField(name);
                if (field != null && field.FieldType == typeof(bool) && (bool)field.GetValue(affix)) return true;
            }
            return false;
        }
        static void LoadCatalog()
        {
            if (catalogLoaded) return;
            var list = AffixList.get();
            if (list.IsNullOrDestroyed()) return;
            foreach (var a in list.singleAffixes) if (!a.IsNullOrDestroyed() && IsCorruption(a)) catalog.Add(a);
            foreach (var a in list.multiAffixes) if (!a.IsNullOrDestroyed() && IsCorruption(a)) catalog.Add(a);
            // A catalog can still be loading when the menu is first opened; retry until populated.
            catalogLoaded = catalog.Count > 0;
        }
        public static IEnumerable<AffixList.Affix> Catalog() { LoadCatalog(); return catalog; }
        public static void Apply(ItemDataUnpacked item, int id, int tier, int roll)
        {
            if (!IsSupported) throw new InvalidOperationException("Chosen corrupted affixes are unsupported by this game's item wrappers");
            AffixList.Affix definition = null;
            foreach (var a in catalog) if (a.affixId == id) { definition = a; break; }
            if (definition.IsNullOrDestroyed() || !definition.CanRollOn(item.itemType, item.subType, ItemList.ClassRequirement.Any))
                throw new InvalidOperationException("The corrupted affix cannot roll on this item");
            var affix = new ItemAffix
            {
                affixId = (ushort)id, affixName = definition.affixName, affixTitle = definition.affixTitle,
                affixType = definition.type, affixTier = (byte)Math.Max(0, Math.Min(6, tier)), affixRoll = (byte)Math.Max(0, Math.Min(255, roll))
            };
            if (flagStorage)
            {
                item.affixes.Insert(item.hasSealedRegularAffix ? 1 : 0, affix);
                item.sockets = (byte)item.affixes.Count;
                if (storage is PropertyInfo flag) flag.SetValue(item, true);
                else ((FieldInfo)storage).SetValue(item, true);
            }
            else if (storage is PropertyInfo property) property.SetValue(item, affix);
            else ((FieldInfo)storage).SetValue(item, affix);
            item.RefreshIDAndValues();
            var saved = flagStorage ? item.affixes[item.hasSealedRegularAffix ? 1 : 0]
                : storage is PropertyInfo p ? (ItemAffix)p.GetValue(item) : (ItemAffix)((FieldInfo)storage).GetValue(item);
            if (saved.IsNullOrDestroyed() || saved.affixId != id)
                throw new InvalidOperationException("The game did not retain the selected corrupted affix");
        }
    }
}
