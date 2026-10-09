# Mastery allocation and hover evidence

Branch: `fix/mastery-lock-ground-tooltips`. This follow-up adds observation only; neither reported failure is marked fixed.

## Mastery

Enable Unlock Other Mastery Trees with Remove Node Requirements off. Open another mastery and try a beyond-chain node with adequate points and prerequisites. Keep `[MasteryTrace]` lines. A click with no native-spend line means rejection occurs before `tryToSpendPassivePoint`; a native rejection identifies the allocation path. The trace records before/after points and both cap values and is limited to 12 unique records per tree instance.

## Ground items

Hover the same dropped item and press F9 once while idle, then while fighting or summons attack. Repeat with the damage meter hidden and visible; note its recording state. Keep `[HoverTrace]` snapshots and annotate which one failed to show the tooltip. The first raycast hit and listener hierarchy identify possible UI interception. No hit alone does not prove that native ground-item hover was allowed. F9 does not force tooltips or alter UI/world-action state; snapshots are rate-limited.

Native compilation and actual in-game tracing still need the installed game's SDK.

Validation: shared Core and test sources compiled directly with Roslyn; 1,007 tests passed and six game-dependent checks skipped because native assemblies are absent. All three edited implementation files passed CSharpier and whitespace checks. The new game-dependent trace hooks themselves still require native compilation and runtime verification.
