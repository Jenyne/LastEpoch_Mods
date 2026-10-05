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
            foreach (string name in new[] { "hasSealedAffixFromCorruption" })
            {
                var property = typeof(ItemDataUnpacked).GetProperty(name);
                if (property != null && property.CanRead && property.CanWrite && property.PropertyType == typeof(bool)) { storage = property; flagStorage = true; return; }
                var field = typeof(ItemDataUnpacked).GetField(name);
                if (field != null && !field.IsInitOnly && field.FieldType == typeof(bool)) { storage = field; flagStorage = true; return; }
            }
        }
        public static bool IsCorruption(AffixList.Affix affix)
        {
            return !affix.IsNullOrDestroyed() &&
                affix.specialAffixType == AffixList.SpecialAffixType.Corrupted;
        }
        public static bool FitsItem(AffixList.Affix definition, int baseType, int subType)
        {
            if (definition.IsNullOrDestroyed() || baseType < 0 || subType < 0) return false;
            var list = ItemList.get();
            if (list.IsNullOrDestroyed()) return false;
            try
            {
                foreach (var type in list.EquippableItems)
                    if (type.baseTypeID == baseType)
                        foreach (var item in type.subItems)
                            if (item.subTypeID == subType)
                            {
                                if (!LastEpoch_Hud.Scripts.Mods.Items.Items_Req_Class.TryGetOriginalRequirement(
                                    baseType, subType, item.classRequirement, out var requiredClass)) return false;
                                if (requiredClass == ItemList.ClassRequirement.None) requiredClass = ItemList.ClassRequirement.Any;
                                return definition.CanRollOn(baseType, subType, requiredClass);
                            }
            }
            catch { return false; }
            // No permissive fallback for an unknown category or subtype.
            return false;
        }
        static void LoadCatalog()
        {
            if (catalogLoaded) return;
            catalog.Clear();
            var list = AffixList.get();
            if (list.IsNullOrDestroyed()) return;
            foreach (var a in list.singleAffixes) if (!a.IsNullOrDestroyed() && IsCorruption(a)) catalog.Add(a);
            foreach (var a in list.multiAffixes) if (!a.IsNullOrDestroyed() && IsCorruption(a)) catalog.Add(a);
            // AllAffixes includes definitions omitted by the editor's single/multi views.
            var all = list.AllAffixes;
            if (!all.IsNullOrDestroyed()) foreach (var definition in all)
                if (!definition.IsNullOrDestroyed() && IsCorruption(definition) &&
                    !catalog.Exists(x => x.affixId == definition.affixId)) catalog.Add(definition);
            // Also support separately exposed collections.
            foreach (string name in new[] { "specialAffixes", "corruptedAffixes" })
            {
                object collection = null;
                var property = typeof(AffixList).GetProperty(name);
                if (property != null && property.CanRead) collection = property.GetValue(list);
                var field = typeof(AffixList).GetField(name);
                if (field != null) collection = field.GetValue(list);
                if (collection == null) continue;
                var countProperty = collection.GetType().GetProperty("Count") ?? collection.GetType().GetProperty("Length");
                var indexer = collection.GetType().GetProperty("Item", new[] { typeof(int) });
                if (countProperty == null || indexer == null) continue;
                int count = (int)countProperty.GetValue(collection);
                if (count < 0 || count > 10000) continue;
                for (int i = 0; i < count; i++)
                    if (indexer.GetValue(collection, new object[] { i }) is AffixList.Affix definition &&
                        !definition.IsNullOrDestroyed() && IsCorruption(definition) &&
                        !catalog.Exists(x => x.affixId == definition.affixId)) catalog.Add(definition);
            }
            // A catalog can still be loading when the menu is first opened; retry until populated.
            catalogLoaded = catalog.Count > 0;
        }
        public static IEnumerable<AffixList.Affix> Catalog() { LoadCatalog(); return catalog; }
        public static void Apply(ItemDataUnpacked item, int id, int tier, int roll)
        {
            if (!IsSupported) throw new InvalidOperationException("Chosen corrupted affixes are unsupported by this game's item wrappers");
            AffixList.Affix definition = null;
            foreach (var a in catalog) if (a.affixId == id) { definition = a; break; }
            if (definition.IsNullOrDestroyed() || !FitsItem(definition, item.itemType, item.subType))
                throw new InvalidOperationException("The corrupted affix cannot roll on this item");
            var affix = new ItemAffix
            {
                affixId = (ushort)id, affixName = definition.affixName, affixTitle = definition.affixTitle,
                affixType = definition.type, specialAffixType = definition.specialAffixType,
                titleType = definition.titleType, sealedAffixType = Il2Cpp.SealedAffixType.FromCorruption,
                affixTier = (byte)Math.Max(0, Math.Min(6, tier)), affixRoll = (byte)Math.Max(0, Math.Min(255, roll))
            };
            if (flagStorage)
            {
                item.affixes.Insert((item.hasSealedRegularAffix ? 1 : 0) + (item.hasSealedPrimordialAffix ? 1 : 0), affix);
                item.sockets = (byte)item.affixes.Count;
                if (storage is PropertyInfo flag) flag.SetValue(item, true);
                else ((FieldInfo)storage).SetValue(item, true);
            }
            else if (storage is PropertyInfo property) property.SetValue(item, affix);
            else ((FieldInfo)storage).SetValue(item, affix);
            item.RefreshIDAndValues();
            // Verify using the game's own accessor, rather than trusting our list position.
            var accessor = typeof(ItemDataUnpacked).GetMethod("TryGetSealedCorruptedAffixe",
                new[] { typeof(ItemAffix).MakeByRefType() });
            object[] arguments = { null };
            bool recognized = accessor != null && (bool)accessor.Invoke(item, arguments);
            var saved = arguments[0] as ItemAffix;
            if (!recognized || saved.IsNullOrDestroyed() || saved.affixId != id || !saved.IsSealedCorrupted)
                throw new InvalidOperationException("The game did not recognize the selected corrupted affix; no item was dropped");
        }
    }
}
