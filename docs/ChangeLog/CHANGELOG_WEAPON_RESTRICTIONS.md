# Ignore Weapon Restrictions

## Player changes
- Expands the existing two-handed-with-shield toggle to **Ignore Weapon Restrictions** in Character → Cheats.
- Allows weapon combinations including two-handed + shield/catalyst/quiver, one-handed + two-handed, and dual two-handed weapons.
- Extends off-hand type acceptance to all weapon types and native off-hand item types.
- Retains level, class, faction, and other non-weapon requirements through the native equipment-container checks.
- Keeps the existing saved setting and toggle identity; existing users retain their enable state.
- Supplies English, French, Korean, and Chinese labels.
- Unsupported off-hand two-handed models, and off-hand models alongside a staff/bow/crossbow, are hidden without clearing the equipped item. Main-hand visuals remain native.

## Developer changes
- Extends native additional off-hand type providers and hand-slot compatibility checks.
- Suppresses automatic weapon-combination enforcement on both main-hand and off-hand paths while enabled.
- Extends dual-wield recognition to an equipped off-hand weapon, with both-hand checks for the dual-wield state. Native stat calculations remain responsible for damage and affixes.
- Restores the previous global two-hander/shield compatibility list when disabled instead of replacing it with an empty list.
- Keeps legacy component/JSON identifiers for compatibility.
- Does not bypass weapon requirements of individual skills or invent attack animations for unsupported combinations.

## Validation
C# syntax, locale JSON, and native wrapper API checks completed. Full build and gameplay verification remain pending.

Test: each combination in both equip orders and via drag/quick-equip; compare off-hand affix and damage contributions; verify class/level restrictions still apply; change zones and save/reload; verify default behavior with the toggle disabled. Dual two-handers are a testable implementation, not yet a confirmed gameplay result.
