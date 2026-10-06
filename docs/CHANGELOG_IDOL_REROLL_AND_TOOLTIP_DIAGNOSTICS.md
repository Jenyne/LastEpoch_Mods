# Idol reroll options and ground tooltip investigation

Status: prepared for in-game testing. Full build and gameplay validation pending.

## Player changes

- Added two independent, default-off options under Items → Crafting: **No Memory Amber Cost** and **Unlimited Idol Altar Uses**.
- Free rerolling covers class-specific and Weaver idols. Initial enchantment keeps its normal Amber cost.
- Unlimited uses preserves the idol altar’s existing use count. Turning it off restores the normal remaining-use limit; it does not reset the altar.
- Existing item/rank/altar/echo conditions are still handled by the native crafting code.
- The idol crafting screen refreshes when either option changes, displays zero reroll cost when enabled and “Unlimited” for altar uses.
- New labels included in English, French, Korean and Chinese locale files.

## Ground item tooltips

This branch does not yet claim a combat tooltip fix. Source review found no mod patch explicitly rejecting ground-item hover during combat. The supplied interop assemblies expose APIs, not native function bodies, so they cannot establish which native condition is failing.

Optional read-only diagnostics record hover entry, combat state, tooltip open attempts/results and closes. Output is limited to ten hover windows with at most 24 messages in each window.

To enable: close the game, open `Mods/LastEpoch_Hud/SaveModUI.json`, and set `GroundItemTooltips` to `true` inside the existing `Debug` group. Launch this build, hover a dropped item out of combat first, then during combat while minions fight. Include a test while standing still without holding a skill/mouse button. Send the resulting `[GroundTooltip]` lines from the MelonLoader log. Turn the option off afterward; no gameplay behavior changes when it is enabled.

## Test checklist

- Both options off: native cost and altar limits unchanged.
- Free cost only: reroll both supported idol types at zero Amber; normal altar uses still decrease.
- Unlimited uses only: normal Amber cost is charged; repeat beyond the altar’s normal limit.
- Both options on: both idol reroll types work repeatedly at zero Amber.
- Turn unlimited uses off: original remaining-use count returns, including when already exhausted.
- Check initial enchantment still costs Amber with only free rerolls enabled; other Weaver spending and other crafting altars remain unchanged.
- Check item result and toggle persistence after save/reload.
- Check native UI labels/button state, unmet rank/echo requirements, and tooltip traces in/out of combat.

## Developer changes

- Dedicated local idol cost/affordability patches, with a scoped Weaver favor-debit backstop around native crafting and eligibility calls.
- Unlimited idol uses suppress only the idol altar’s use consumption and temporarily relax its use-limit flag while native eligibility is evaluated. Flags and thread-local scopes restore in finalizers.
- Optional ground hover traces cover legacy and current tooltip systems without overriding combat or tooltip results.
