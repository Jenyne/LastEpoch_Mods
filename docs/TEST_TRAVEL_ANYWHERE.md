# Travel Anywhere restoration

Branch: `feat/travel-anywhere`. Parent: `feat/gold-favourites-session-stats` at `87e5d8c4`. This test build includes the gold button, favourites and session counters from that parent. The original QoL branch remains unchanged.

## What the old version actually did

- `ed4fe49` (2026-04-01) added a scene picker backed by `SceneList.sceneDetailsCollection`, a `PlayerSpawnManager.instance` reference and direct additive travel.
- `cd9e6d3` (2026-04-02) added closing/resuming the mod HUD after travel and a name-based helper for the main-quest action.
- `ef327f9` removed the spawn-manager reference during the current-game HUD update. The old travel helper still referenced it.
- `65b8e77` (2026-10-04) explicitly hid the picker and replaced the additive loader with the native waypoint transition service.

The old route was `LoadSceneAsync(Additive)` > set active scene > `TryPlacePlayerAtSpawn` > unload the previous scene. It ignored both the active-scene result and the placement result, with no timeout or overlapping-request guard.

The compiled keyboard release from `ed4fe49` was inspected without executing it. It confirms `PlayerSpawnManager.instance`, `SceneDetails.Name`/`LocalizedName`, the placement method's **bool** return, and the actual eight-argument call:

```text
TryPlacePlayerAtSpawn(Actor, source, destination, int gate,
    out PlayerSpawn, false, true, false)
```

It also confirms that `LoadWaypointScene` is declared on **UIWaypoint**, the base class. This is historical API evidence; it does not establish compatibility with the installed 1.5 SDK.

## Where and how to use it

Scenes > Misc > Travel Anywhere. The option defaults off and persists in `SaveModUI.json` under `TravelAnywhere.Enabled`.

1. Enable Travel Anywhere.
2. Select an area and click **Travel to selected area**, or open the world map and left-click a map node.
3. **Refresh** rebuilds the destination list. Native localized area names are used; untranslated new entries remain available.
4. While enabled, **Favourite current area** can save a non-waypoint area. Its favourite uses the same direct loader. With the option off, favourites return to their unlocked-waypoint rules.

The map-click path reads the frontmost UI hit and requires a `UIWaypoint` parent. A popup over the map blocks clicks. Native waypoint clicks are intercepted while this mode is enabled so a click cannot dispatch both the additive loader and the normal waypoint loader. Map widgets that expose no waypoint component require a later adapter; this build does not assert that every game map widget is covered.

The destination list uses the game scene database and retains the old menu/utility/PCG/arena exclusions. Waypoint existence/unlock and missing localization are not eligibility requirements. This restores static-area selection; generated echo/arena instances remain outside its destination scope. Leaving a generated area for an eligible static area is allowed and still requires a runtime check.

## Transition behaviour and limits

- Waits for the load operation and a valid loaded destination before activation/placement.
- Uses the current spawn-manager singleton, verifies the original local actor, and requires successful placement plus a valid selected spawn before retiring the source.
- Moves a player root out of the source scene when necessary. An arbitrary hierarchy containing the player is not moved.
- Rejects same-area, excluded/unknown, already-loaded and overlapping requests. New direct and native waypoint travel stays blocked until in-flight cleanup finishes, including when the option is switched off mid-flight.
- A 30-second load deadline or eight-second placement deadline enters recovery while retaining the source. Actor identity changes prevent unloading a destination that owns the current player.
- Unity async scene operations expose no cancellation method. A timed-out load is observed until it finishes before removing its destination. Accepted source unloads are also observed until completion. These soft deadlines protect ordering; they cannot force the engine to finish a stalled operation.
- Updates the mod's active-scene cache and does not add destinations to the saved waypoint-unlock list.

The native game can have additional scene/actor/service state beyond these checks. Recovery position, camera, navigation, quests, mob spawning and interaction must be verified in game. A change of local actor during recovery is reported and leaves cleanup blocked instead of deleting its scene.

## Build and gameplay checks

Use test-menu selection **11** after refreshing `chore/test-queue-runner`. The runner builds against the installed SDK and resolves Harmony patch targets before installing the DLL.

1. **Option off:** check ordinary waypoint travel, unlocked-waypoint favourites, Safe Teleport and the main-quest action retain their existing behaviour. No new map-click travel should occur.
2. **Picker:** verify the section/dropdown/buttons appear, the popup is usable through the Misc scroll viewport, selection is preserved on Refresh, and close/reopen does not duplicate listeners or controls.
3. **Non-waypoint area:** select a known campaign location without a waypoint. Confirm correct destination and usable spawn, camera, movement/pathfinding, mobs, damage, XP and loot. Walk through an exit/NPC interaction afterward, then travel back.
4. **Map:** test a non-waypoint node, a locked waypoint and an unlocked waypoint. Verify one click gives one `[TravelAnywhere] Load` and one completion. Test a popup over a node, the same-area node, each era and controller map/picker navigation.
5. **Spawn gates:** test ordinary gate-zero scenes and nodes with nonzero gates. Check placement actually reaches the selected location rather than another entrance. Missing/unusable spawns should retain the source and report a recovery outcome.
6. **Busy/recovery:** double-click, click another node while loading, turn the option off during loading and try Safe Teleport. Verify no second load. For a failed/timed-out destination, check the original area remains playable, position/camera/navigation recover and partial destination cleanup finishes before another request is accepted.
7. **Favourites:** save a non-waypoint area, leave, return using its favourite, remove/re-add and restart. With Travel Anywhere disabled, the non-waypoint favourite must be blocked. Verify no new entries were added to `UnlockedWaypointScenes`.
8. **Long session:** repeat several trips, including a town, combat area and End of Time; also test leaving an echo/arena for a static area. Watch for duplicate players/managers, missing enemies, broken portals, frame-time/memory growth and quest state problems. Return to character selection and load another character.
9. **Persistence/locales:** restart with the option enabled and disabled; check EN > FR > KO > ZH > EN controls, native location names, status labels and selected area identity.
10. **Parent features:** retest fixed gold, session-counter zone continuity/reset, both kinds of favourites and Safe Teleport. Other pending Force Drop/crafting/drop-rate branches are separate builds.

Keep the complete log and exact commit. Expected bounded lines are `[TravelAnywhere] Load`, `Complete` or a reason for recovery. On a build failure, retain `build.log`; on native target failure, retain `tests.log`. The runner stops before installation in either case.

## Recorded verification

- Direct Roslyn/.NET 8 execution: **1,064 passed, six SDK-dependent checks skipped**, zero failures. Includes 40 new cases for historical destination exclusions, generated-area sources and transition ordering/failure/recovery.
- Changed C# source checked with CSharpier; whitespace and locale JSON checks passed.
- The compiled historical API and source were compared. No old DLL was installed or executed.
- Native mod compilation, current SDK signatures/Harmony targets and all gameplay/map/scene-recovery behaviour remain **pending**. Pure tests confirm the protocol rules, not current-game success.

Unity references used during the review: [additive scene management](https://docs.unity.com/en-us/engine/6000.7/script-reference/unityengine/scenemanagement/scenemanager), [SetActiveScene result](https://docs.unity3d.com/ru/2021.1/ScriptReference/SceneManagement.SceneManager.SetActiveScene.html), [root-only scene transfer](https://docs.unity3d.com/ja/current/ScriptReference/SceneManagement.SceneManager.MoveGameObjectToScene.html), and [async scene unload](https://docs.unity.com/en-us/engine/6000.3/script-reference/unityengine/scenemanagement/scenemanager/unloadsceneasync), and [async operation controls](https://docs.unity3d.com/2021.3/Documentation/ScriptReference/AsyncOperation.html). These describe Unity operations, rather than Last Epoch's service invariants.
