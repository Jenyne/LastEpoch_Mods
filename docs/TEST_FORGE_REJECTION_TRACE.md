# Native forge rejection follow-up

Branch: `feat/advanced-forge-t7`. The previous in-game test still stopped at T5.

The capability hook now recognizes the max-tier localization key's actual output during that native check, instead of comparing against the literal `affix_maxed` placeholder. The existing localization registry has no active `RegisterAll` caller on this branch. This fix does not globally replace native text. Unknown keys, unrelated titles, empty labels and labels from an older check do not authorize an upgrade.

`[ForgeTrace]` records the original native result/title/flags, cached item, selected affix/tier, FP and localization keys before the override. It is limited to 12 distinct records per cached item. If the game's key changed or its title is cached outside the check, the override stays closed and the trace provides evidence for the next change. Nested checks restore their prior observation even when native code throws.

Awaiting native compilation and in-game confirmation:

1. Enable Craft Affixes to T7 and Infinite Forging Potential. Put normal T5 equipment in the forge, select its affix and retain `[ForgeTrace]` lines when the button is blocked. Swap items and repeat after toggling the option while an item is already slotted.
2. Repeat in EN and another locale. Record the exact forge title and observed localization keys. A `cachedItem=0` or `internalTier=-1` identifies a tracking/selection gap.
3. Check T5 → T6 → T7 and stopping at T7, option-off behavior, no-shard rejection, actual shard/glyph counts and FP. Use expendable equipment.

The custom high-tier Forge transaction still bypasses native Forge. Shard/glyph consumption, Chaos/Envy/unguaranteed Despair semantics and definition-backed tier limits remain unresolved. This follow-up is a targeted gate repair and diagnostic build, not a completed crafting fix.

Validation: the test sources and shared Core code compiled directly with Roslyn against the .NET 8 reference pack and the project's xUnit/Cecil dependencies. 51 tests passed, including six native-label classification cases; three native patch checks skipped because game assemblies are absent. CSharpier and whitespace checks passed on the edited implementation. This does not compile the game-dependent mod or prove the craft transaction in game.
