# Mastery unlock and ground-item hover test

Branch: `fix/mastery-lock-ground-tooltips`, based on upstream `master` at `92fb33de`.
This branch is independent of the pending Illegal Mode build.

## Latest report and follow-up

Two distinct MelonLoader runs on `7a91a7c1` stop after `[MasteryTrace] Native cap requested: 22->45`, before the completed-write trace. `Latest(6).log` duplicates the 00:24 archive. This narrows the failure to the global-cap write boundary but does not provide a fatal native stack proving its exact cause.

This candidate removes all writes to `GlobalTreeData.maximumUnchosenMasteryLevel`, including restoration on disable, and removes the obsolete cap-ownership helper/tests. Toggle synchronization only updates chain visuals; the native global cap stays unchanged. The cloned checkbox keeps its own event and the forced native tree rebuild remains removed. A bounded, once-per-process `[MasteryApi]` inventory records managed wrapper method signatures from LocalTreeData, GlobalTreeData and SkillTreeNode without invoking them. It provides evidence for a later targeted allocation-check patch rather than guessing signatures or temporarily changing the character's chosen mastery.

**This is a crash-isolation candidate, not a completed allocation bypass.** Without replacing the cap gate, spending beyond the chain may still be rejected. Remove Node Requirements is user-confirmed working and persistent and is unchanged. Existing chosen mastery, native prerequisites, point costs and rank limits are unchanged.

**Hover is on hold at the user's request.** No hover implementation changes are included in this follow-up. Existing F9 diagnostics remain available, but these new logs contain no HoverTrace snapshots.

Formatting/diff checks pass; 999 core/source tests pass with six SDK-dependent skips. The count is lower because eight obsolete cap-ownership test cases were removed. These checks do not compile or exercise the native mastery UI. Current native compilation and gameplay confirmation require the guarded Windows build.

Close the game, fetch and select the branch, then run:

```powershell
.\scripts\Test-MasteryLockAndTooltips.ps1 -GamePath "D:\SteamLibrary\steamapps\common\Last Epoch"
```

The script updates the branch, builds Release, tests that exact DLL against your
game assemblies, backs up the installed DLL, and installs only after success.

## Non-main mastery trees

1. Use a character with a chosen mastery and enough passive points. Leave
   **Remove Node Requirements** disabled so this test exercises normal prerequisites.
2. In **Skills**, enable **Unlock Other Mastery Trees**, below Remove Node Requirements.
   First test with the passive-tree panel closed, then with it open. Keep the
   `[MasteryTrace] Toggle requested`, `Visual unlock active` and `[MasteryApi]` lines in the
   MelonLoader log. Close/reopen or change the tree page for its native display refresh.
3. Open each of the two other mastery trees. The mastery chains should disappear.
   Try a node beyond the former chain with adequate points and prerequisites.
   Allocation may still fail in this crash-isolation candidate; keep click/spend traces.
4. Check normal requirements: unconnected nodes and nodes without enough earlier
   mastery points should remain unavailable; point costs and node rank limits still apply.
5. Confirm the character's selected mastery and its innate bonus have not changed.
6. Change tree pages, close/reopen the tree, and switch zones. Check chains do not return.
7. Restart the game with the option enabled. Check the checkbox, allocated points
   and stats persist. Test another character, including one that has not chosen a mastery.
8. Turn the option off. Native mastery restrictions and chain visuals should return.
   This option does not automatically refund points allocated beyond the normal cap;
   check the game's respec behavior separately.
9. Switch between English, French, Korean and Chinese; the new checkbox should update.

## Ground-item tooltips during combat (on hold)

1. With the mod menu closed, hover dropped items during combat, including while
   summons attack. Check both the name label and actual item tooltip.
2. Repeat with the damage meter hidden, visible, recording, and stopped (F7 by default).
3. Test items near the screen center and outside each visible damage meter panel.
   Open and close the meter's settings and detail panels, then repeat.
4. Check meter buttons, dropdowns, settings, scrolling and controller casting still work.
5. Repeat after a scene change and restart. Compare with the same scenario without
   the mod if hover still fails. Report meter visibility/state with the log.
6. Press F9 over the same item idle/in combat, with the menu closed and the meter
   hidden/visible. Keep raycaster types, candidate listeners and listener totals.
   If the checkbox still crashes, the corresponding MelonLoader log or native
   crash stack is needed; the supplied Player preview errors do not locate it.

The hover change removes the meter canvas's full-screen mouse listener and hides
its sprite template Images. Mouse listeners remain on the three actual panels.
Combat behavior needs in-game confirmation; native API checks cannot reproduce it.
