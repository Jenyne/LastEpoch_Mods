# Review of queue items 3, 5 and 7

Reviewed 2026-10-08. This is source investigation, not a new runtime-tested fix. No gameplay patches changed in this review. Current game reference assemblies are no longer in the recovered workspace, so new native patch signatures and in-game outcomes cannot be verified here.

| Item | Reviewed branch / commit | Outcome |
|---|---|---|
| 3 | `fix/mastery-lock-ground-tooltips` / `0268b997` | Both reported failures remain open. Mastery allocation coverage gap identified; tooltip cause still needs a live raycast trace. |
| 5 | `feat/advanced-forge-t7` / `59e6c24c` | Stale message gate identified; high-tier custom crafting also omits visible material-consumption and glyph semantics. |
| 7 | `test/offline-guard-diagnostics` / `99eebd98` | Existing startup/observation remains user-confirmed. Session-end logging mismatch identified; observers do not enforce mutation permissions. |

## 5: Advanced Forge

Source: `LastEpoch_Hud/Scripts/Mods/Items/Items_AdvancedForge.cs` on the reviewed branch; shared `Scripts/Mods/Craft/Craft_Locales.cs`.

### Concrete defects and gaps

- `ForgeCapability.Postfix` requires `__0 == Craft_Locales.affix_is_maxed` before overriding the game's T5 rejection. `affix_is_maxed` is initialized to literal `"affix_maxed"`. The active source contains no assignment refreshing it from native localization; the declared key dates to game 1.3.1.1. A translated/native message that differs from that placeholder will never enter this override. This is a concrete defect and a plausible explanation for the failed button, not proof of the exact runtime rejection text.
- The cached item is populated only by `OnMainItemChange` and a `OneItemContainer` cast. There is no fallback to the currently slotted item when enabling the option. Confirm this cache is populated and corresponds to the live forge item before interpreting a failed capability override.
- The custom T5/T6 `Forge.Prefix` mutates the affix and returns false, skipping native `Forge`. It has no visible shard/glyph consumption call or replacement for the native completion transaction. A separate hook could conceivably consume resources, but this patch itself does not establish that behavior.
- Chaos and Envy are treated as ordinary roll-and-tier upgrades in the new code. The historical `Craft_MaxTier` implementation had explicit Chaos affix replacement and Envy subtype logic. The new path also falls through to a normal upgrade for unguaranteed Despair rather than preserving its native seal behavior.
- Tier eligibility uses a global internal cap of 6; it does not check the selected definition's available tier count. High-tier crafting must not manufacture undefined tiers.

### Recommended code changes

1. Replace the obsolete text comparison with a current native rejection classification. If only a localized message is exposed, resolve the current native key for the active locale and log a bounded diagnostic when it cannot be resolved. Do not turn every T5 rejection into success: corruption, item rarity/type, missing materials and unsupported glyphs must remain independent checks.
2. Resolve the live forge item at the capability/button/craft boundary, and clear references on removal; add one diagnostic per changed item/rejection rather than per frame.
3. Limit upgrades to ordinary eligible equipment and actual definition-backed tiers; reject sealed, unique/set, corrupted and otherwise unsupported routes as appropriate to normal crafting.
4. Prefer retaining the native craft transaction after lifting only the tier restriction. If a custom high-tier transaction is still necessary, explicitly implement resource consumption, glyph effects, failure handling and UI refresh. Do not silently substitute a normal upgrade for Chaos, Envy or Despair.
5. Verify T5 → T6 → T7, T7 stopping, option-off behavior, no-shard rejection, glyph consumption/effects, FP limits, item replacement and restart.

The missing mod locale key already fixed on this branch is a separate issue from the stale native rejection-message comparison.

## 3: Mastery and combat ground-item tooltips

### Mastery

Source: `Scripts/Mods/Skills/Passives_MasteryLock.cs` on the reviewed branch, plus existing `Passives_SpendPoints.cs` and `Skills_Nodes_Req.cs`.

The new unlock writes `GlobalTreeData.maximumUnchosenMasteryLevel`, changes the displayed maximum and substitutes the node's mastery only for `UpdateNodeLockVisual`. It does not patch `LocalTreeData.tryToSpendPassivePoint` or an allocation eligibility predicate. Existing passive-spend overrides depend on the separate Remove Node Requirements setting; the test deliberately leaves that setting off.

This identifies a coverage gap, not the exact native condition responsible for the runtime lock. Trace a rejected spend with chosen mastery, target mastery, the live cap and native result, then change only the actual mastery restriction. Preserve node prerequisites, available points, rank caps, the character's saved mastery and mastery bonus. Do not simply return success from `tryToSpendPassivePoint`: that can report success without performing the native point allocation. Do not use a general requirement bypass as the mastery fix.

### Ground-item hover

The current patch moves `UIMouseListener` from the full-screen damage-meter canvas to three panels and hides the sprite-source Images. This is a reasonable narrowed candidate fix, but the user's combat-hover test failed, so it is not the confirmed cause or solution.

Other paths remain to investigate: active mod HUD listeners, native UI mouse-over state, actual raycast-winning graphics and combat-only label/tooltip gating. The main HUD still attaches a `UIMouseListener` to its prefab root; that alone is not proof of a bug while the HUD is inactive. Authored meter graphics have raycast targets, but child stretch anchors alone do not prove a screen-wide blocker.

Add a manually triggered or rate-limited trace of the first actual UI raycast hit while hovering loot: object hierarchy, canvas, active state, listener ownership, mod-menu state and meter state. Compare idle/combat and meter hidden/visible/recording/stopped. Remove or narrow only the blocker shown by that evidence; keep meter controls and controller world actions working. Do not force all UI mouse-over state false or enable every ground tooltip globally.

## 7: Offline diagnostics

Source: `Scripts/Core/Diagnostics/OfflineGuardObservation.cs` and `Scripts/Mods/Diagnostics/OfflineGuardDiagnostics.cs` on the reviewed branch.

### What is confirmed and what it does

The startup shortcut, hidden online switch and quiet confirmation were user-confirmed. The observer correlates offline StartPlay, character initialization, local actor/tracker, local data-store presence, gameplay mode, online-session state and offline network initialization. Unknown evidence does not become a positive signal, and explicit online evidence revokes the candidate.

This classification is not consumed as a permission check by gameplay mutation patches. The online UI blocks are also conditional on the auto-offline configuration. This branch therefore does not establish unconditional offline-only enforcement.

### Logging mismatch

`EndSession` increments `Epoch` before setting Revoked. The logger emits revocation only when `confirmedEpoch == observation.Epoch`. After a confirmed session ends, that equality fails, so the documented session-end revocation message can disappear. Direct online revocations that do not increment the epoch can still log. The core state is revoked correctly; this is a reporting gap, not evidence of an online permission bypass.

Keep the terminated observation's generation separately, or queue a single revocation event at termination before correlation is reset. Deduplicate repeated exit/transition hooks and retain one confirmation per character load. Test confirmation → temporary zone evidence loss → recovery, confirmation → session end, repeated exit hooks and loading a different character.

### Enforcement follow-up

If advancing the original offline-only protection work, create a separate topic branch with a fail-closed permission API: unknown/loading/online/revoked states cannot authorize mutation. Apply it at actual mutation boundaries, not only visible buttons, and revoke before online transitions. Preserve native initialization and saves rather than forcing offline flags. This must be tested against current game service behavior; client-side code can still be modified, so it cannot be represented as tamper-proof.

## Development order

1. Fix #5's stale capability gate together with the crafting transaction gaps.
2. Instrument #3's native allocation rejection and the combat raycast winner, then target the demonstrated restrictions.
3. Treat #7's epoch logging as a small follow-up; keep comprehensive runtime enforcement separate from the confirmed observer.

None of these source findings changes the recorded in-game status into a pass.
