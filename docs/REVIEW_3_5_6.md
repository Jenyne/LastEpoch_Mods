# Follow-up for queue items 3, 5 and 6

Updated 2026-10-09. The active queue is **3, 5 and 6**. Item 7 is user-confirmed and done for now; deeper work is deferred.

Published source: #3 [764ea599](https://github.com/Jenyne/LastEpoch_Mods/commit/764ea5995caba8fce375d84ae931a910a71a2c26), #5 [b59281d6](https://github.com/Jenyne/LastEpoch_Mods/commit/b59281d66dcccace92a50e67723965ebcc1ff1b4), #6 [75e23254](https://github.com/Jenyne/LastEpoch_Mods/commit/75e23254a0b0f54b6107f023384592926a58b9cf).

| Queue | Source change | Remaining evidence |
|---|---|---|
| 3 | Removed all native global-cap writes and obsolete cap-ownership helper/tests. Added once-per-run read-only `[MasteryApi]` method inventory. Existing checkbox/visual hooks remain. Hover implementation unchanged. | Two 7a91 runs end at the 22→45 cap-write boundary without completion; fatal cause is still unproven. User confirms clicking the checkbox no longer crashes on 764ea599; allocation is still not functional. Broader off/on, reopening and restart checks remain pending. A targeted allocation-check patch is still needed. Node requirements work/persist. Hover is on hold. |
| 5 | Replaces the literal max-tier marker gate with the known native localization key's output from the same check; logs original result/title, flags, item, tier, FP and keys. | Native key/title and item-tracking confirmation. Custom crafting still needs material consumption, complete glyph semantics and tier-definition checks. |
| 6 | Uses Hud_Manager's live Drop resolver, waits for visible layout, preserves legacy rows, inherits UI layers and handles native toggle/slider/input paths. | Four visible controls, synchronized inputs, persistence and separate natural-rate comparisons. Backend rate calculations are unchanged. |

## Concrete forge finding

The older review focused on the untranslated `affix_maxed` placeholder. The localization override registry could have supplied that marker, but this branch has **no active caller of `LocalizationOverride.RegisterAll`**. The follow-up observes the exact known native key without globally replacing its text. Unknown keys and unrelated, empty or stale labels keep the capability gate closed. If the game's key changed or the label is cached outside the check, `[ForgeTrace]` supplies evidence instead of broadly overriding rejection.

## Runtime check order

1. Selection 6: open Items > Drop, scroll to Natural Drop Rates, verify four rows and legacy controls. Test toggle/drag/type 0, 50, 100 and 1000, close/reopen and restart. Keep `[DropRates]` lines.
2. Selection 5: enable T7 crafting and Infinite FP on expendable normal equipment. Keep `[ForgeTrace]` while selecting a T5 affix. Check no-shard rejection and actual material consumption alongside T5 → T6 → T7.
3. Selection 3: leave Remove Node Requirements off. Toggle mastery unlock closed/open, off/on, page changes and restart. Keep `[MasteryTrace]` visual-state/click/spend and `[MasteryApi]` signatures. Attempt beyond-chain allocation but expect it may still be blocked: this candidate isolates the crash boundary without replacing the native gate. Preserve mastery/innate/prerequisites and keep matching logs if it crashes. Hover is on hold.

Each selection is a separate topic build. Close the game before switching. The runner compiles and tests against the installed SDK before replacing the DLL. No new change here is in-game confirmed.

## Validation

The environment's .NET CLI/MSBuild fails during process-information initialization. The same test sources and shared Core code were compiled directly with Roslyn against .NET 8 and the project dependencies, then run with xUnit's in-process runner.

| Branch | Passed | Skipped | Failed |
|---|---:|---:|---:|
| Mastery / tooltips | 999 | 6 | 0 |
| Advanced Forge | 51 | 3 | 0 |
| Drop rates | 45 | 3 | 0 |

Skipped checks require game assemblies. These runs cover game-independent logic and repository contracts; they do not compile or verify the new game-dependent hooks. CSharpier checked the new/updated hook implementations; whitespace checks passed throughout. The refreshed runner has 10 entries: Force Drop is selection 1, duplicate selection 2 is retired, and IDs 3–11 remain unchanged. Only Force Drop consolidation and the new user-confirmed #3 checkbox status changed; installer control flow is byte-for-byte unchanged.

The eight obsolete cap-ownership test cases were removed with the global-write strategy; the lower count does not indicate failing tests.
