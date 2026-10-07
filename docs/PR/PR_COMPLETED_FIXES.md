# Integrate tested gameplay fixes and compact HUD controls

Combines the completed, in-game-tested fixes on current `Syncingoutt/master` (`88ca851e`, including the Area of Effect slider). The combined build was tested by Jenyne; the final UI test loaded build `8bcf260f`.

## Changes for players

- Reveal dungeon objectives without the previous dungeon crashes; enter without a key while retaining optional portal charm selection.
- Ignore weapon restrictions: support 2H + 2H, 2H + shield and 2H + 1H combinations, with functional equipment stats and saved equipment.
- Bind Safe Teleport with modifier keys, persist the binding across restarts and reject conflicts with AutoCast. Controls are under Scenes → Misc.
- Add optional Infinite Forging Potential. Crafting's Deselect All clears other crafting cheats while keeping Infinite enabled.
- Add exactly 10,000 Memory Amber per click; correct doubled Soul Ember grants and replace the amount input with Add 1,000 Soul Embers.
- Group currency actions in Character → Cheats and blessing actions at the bottom of Character → Data, using compact full-width bars.
- Add two distinct exclusive modifier selectors for Withstand the Elements, separate from LP and regular affixes; retain Unsated Rage's separate modifier selector.
- Use native translated item, category and affix names in Force Drop, with searches accepting translated and English names.
- Organize Scenes into Dungeons, Misc and Minimap. Minimap contains Zoom and Fog of War; sections retain native checkbox styling and gold headers.
- Add the corresponding English, French, Korean and Chinese HUD locale labels.

## Developer details

- Dungeon reveal invokes native objective pulses after initialization and tracks each pulse request to avoid reentrant activation; cleanup removes destroyed pulse pointers.
- Free entry retains the native key/charm/tier UI flow.
- Weapon compatibility patches retain the existing component/save key. Unsupported offhand weapon visuals are suppressed while preserving equipped data.
- Infinite Forging Potential preserves the starting value around native crafting cost methods.
- Safe Teleport uses existing transition services and saved ModUI settings; modifier capture and conflict checks share the keybinding framework.
- Runtime HUD helpers reuse native controls and accommodate the existing prefab hierarchy. Existing save group/key names are preserved when controls move.
- Correct the DungeonReveal setting's path to the runtime checkbox, resolving its stale automatic-binding warning.
- Preserve current upstream AoE hooks, save fields, buff implementation and numeric input behavior.

## Validation

- Jenyne confirmed the combined gameplay features working in game. Individual tests also confirmed equipment statistics/persistence, Safe Teleport binding persistence, crafting persistence, special glove modifiers/persistence and native Force Drop translations.
- Dungeon reveal/free entry were previously confirmed over two complete runs and multiple floors, including portal charm selection.
- Final supplied log for `8bcf260f`: no ERROR entries or crash traces; settings loaded, Safe Teleport bound 2/2, blessing selections completed, and dungeon pulse requests completed.
- CSharpier: all 163 C# files passed. Whitespace and locale JSON/base-key coverage checks passed.
- The subsequent DungeonReveal settings-path correction needs one startup/toggle spot-check. Unchanged upstream IL2CPP registration warnings remain in the log.
- Local MSBuild/.NET test execution is blocked by this environment's process-information initialization; Windows builds and gameplay validation were performed by the user.

## Scope

This PR excludes the unfinished Force Drop illegal-mode/corruption-pool rebuild, prophecy multiplier, idol rerolls, combat tooltip investigation, independent drop rates and mechanic spawn work. The intermittent Mjölner icon issue remains assigned to the custom-item maintainer.
