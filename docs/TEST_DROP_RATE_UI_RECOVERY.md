# Natural Drop Rates UI recovery

Branch: `feat/independent-drop-rates`. The previous test showed no sliders.

Controls now bind from the legacy Items > Drop resolver instead of the strict SettingsGroup hierarchy. Construction waits for the menu to be visible before measuring its scroll content; existing row geometry is preserved. New controls inherit the UI layer, and pointer/slider/input hooks are scoped to this section. A single `[DropRates] Four Natural Drop Rates controls bound` line records success; missing fonts/content produce a warning.

2026-10-09 follow-up: `Latest(7).log` confirms build `75e23254` reached that binding message, but the screenshot shows no rate controls. The shipped Drop prefab has `VerticalLayoutGroup.childControlHeight=false`, so the section's preferred height was ignored and its actual height remained zero. The section now has an explicit 288-unit RectTransform height; inactive layout groups use the manual placement fallback. After a further layout frame, the binding message includes section/content dimensions and warns instead of reporting success when the section has no area.

Awaiting native build and in-game confirmation:

1. Open Items > Drop and scroll below Weaver Will to Natural Drop Rates. Confirm four rows and normal legacy controls. Retain the dimension-bearing `[DropRates]` line; section height should be 288 and width should be positive.
2. Enable each row, drag its slider and type 0, 50, 100 and 1000; click outside the input. Slider, input and saved setting must agree. Close/reopen and restart.
3. Check EN/FR/KO captions and menu resizing.
4. Keep Force Unique/Set/Legendary off. Compare natural drops with each modifier independently, then disabled/100%. Forced rewards are not a valid rate comparison.
5. Retain the bind confirmation, first runtime `[DropRates]` baseline line and any errors. No drop-rate effectiveness or persistence is confirmed by this UI change.

Validation: shared Core and test sources compiled directly with Roslyn; 45 tests passed and three native patch checks skipped because game assemblies are absent. The new control implementation passed CSharpier and all changes passed whitespace checks. These results do not compile or exercise the game-dependent UI.
