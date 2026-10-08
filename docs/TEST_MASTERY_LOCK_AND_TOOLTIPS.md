# Mastery unlock and ground-item hover test

Branch: `fix/mastery-lock-ground-tooltips`, based on upstream `master` at `92fb33de`.
This branch is independent of the pending Illegal Mode build.

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
3. Open each of the two other mastery trees. The mastery chains should disappear.
   Spend enough points to unlock nodes beyond the former chain, and allocate one.
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

## Ground-item tooltips during combat

1. With the mod menu closed, hover dropped items during combat, including while
   summons attack. Check both the name label and actual item tooltip.
2. Repeat with the damage meter hidden, visible, recording, and stopped (F7 by default).
3. Test items near the screen center and outside each visible damage meter panel.
   Open and close the meter's settings and detail panels, then repeat.
4. Check meter buttons, dropdowns, settings, scrolling and controller casting still work.
5. Repeat after a scene change and restart. Compare with the same scenario without
   the mod if hover still fails. Report meter visibility/state with the log.

The hover change removes the meter canvas's full-screen mouse listener and hides
its sprite template Images. Mouse listeners remain on the three actual panels.
Combat behavior needs in-game confirmation; native API checks cannot reproduce it.
