using Il2Cpp;

namespace LastEpoch_Hud.Scripts.ModUI.ForceDrop;

// An illegal unique's native regular-seal presence flag owns the first affix
// after its fixed unique modifiers. Restore that ownership after native refresh
// or decode; use the saved flag, so recognition survives mode changes/reload.
internal static class IllegalUniqueSealAdapter
{
    public static void Restore(ItemDataUnpacked item)
    {
        if (
            item.IsNullOrDestroyed()
            || item.rarity != 9
            || !item.hasSealedRegularAffix
            || item.affixes.IsNullOrDestroyed()
            || UniqueList.instance.IsNullOrDestroyed()
        )
            return;
        var entry = UniqueList.getUnique(item.uniqueID);
        if (entry.IsNullOrDestroyed())
            return;
        int index = UniqueVariantAdapter.VariantCount(entry);
        if (index >= item.affixes.Count)
            return;
        // Reject an ambiguous fixed prefix rather than sealing an unrelated slot.
        for (int i = 0; i < index; i++)
            if (
                item.affixes[i].IsNullOrDestroyed()
                || entry.droppableLegendaryAffixes.IsNullOrDestroyed()
                || !entry.droppableLegendaryAffixes.Contains(item.affixes[i].affixId)
            )
                return;
        var regular = item.affixes[index];
        if (regular.IsNullOrDestroyed() || regular.IsSealedCorrupted)
            return;
        foreach (var affix in item.affixes)
            if (
                affix.IsNullOrDestroyed()
                || (affix.IsSealedRegular && affix.affixId != regular.affixId)
            )
                return;
        regular.sealedAffixType = SealedAffixType.Regular;
        bool primordial = false;
        foreach (var affix in item.affixes)
            primordial |= affix.IsSealedPrimordial;
        item.hasSealedPrimordialAffix = primordial;
    }
}
