# Travel Anywhere restoration

Combined branch for queue selection **11**: `feat/travel-anywhere`. Selection 10 is retired; the full QoL branch history is merged here. Original parent: `feat/gold-favourites-session-stats` at `87e5d8c4`. This test build includes the gold button, favourites and session counters from that parent. It also includes the key teleport list, movable overlay and live favour reconciliation from QoL follow-up `638504c0`. Key teleport presets use ordinary unlocked waypoints; custom saved non-waypoint favourites retain the Travel Anywhere route.

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

It also confirms that `LoadWaypointScene`, `isActive` and the scene/gate members belong to **UIWaypoint**, the base class. The current fork already reads `noWaypointInScene` in its waypoint helper. This is source/historical API evidence; it does not establish compatibility with the installed 1.5 SDK.

## Where and how to use it

Scenes > Misc > Travel Anywhere. The option defaults off and persists in `SaveModUI.json` under `TravelAnywhere.Enabled`.

1. Enable Travel Anywhere.
2. On the world map, left-click a node to open its normal area menu and use **Travel**. The full scene picker has been removed at the user’s request. Right-click remains a convenience path for nodes with a usable waypoint component; non-waypoint right-click is a known limitation accepted for now.
3. While enabled, **Favourite current area** can save a non-waypoint area. Its favourite uses the same direct loader. With the option off, favourites return to their unlocked-waypoint rules.

The map override temporarily sets eligible nodes to `noWaypointInScene = false` and `isActive = true`. It runs before and after the existing standard-node hover handler, and refreshes visible waypoint widgets while the map is open, so the normal menu can offer waypoint travel for a non-waypoint area. Left-click is left to the game; the earlier immediate left-click fallback has been removed. Right-click reads the frontmost UI hit and requires a `UIWaypoint` parent. A popup over the map blocks direct clicks. The menu/native waypoint action is intercepted while enabled so both paths use the same loader without dispatching the normal waypoint service as well.

Original map flags are restored when the option is disabled, the map closes or the component is destroyed. Real saved unlocks and the separate Unlock All Waypoints option are respected. Snapshots are checked against scene identity before restoring a reused widget. An already-open temporary Travel action is blocked after disabling; reopening the menu should rebuild its normal availability. No waypoint is spawned into the physical game area, and no scene is added to the saved unlock list.

Map widgets that expose no waypoint component require a later adapter; this build does not assert that every game map widget is covered. The current native menu may have additional availability or visual checks, so the Travel button and waypoint appearance still need runtime confirmation.

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

User tested `2466cc0a` on 2026-10-09 and reports map Travel Anywhere works well. The log confirms this build loaded, resolving the earlier CS1061 build blocker. It also shows the full picker attempted `EoT -> WE502`, reported “Scene loading did not start,” and stayed busy, blocking key waypoint teleports. The picker is removed; rejected loads now immediately release the guard only after verifying no target loaded and the original source/player stayed intact. Async operation checks use reference null tests; real pending loads/cleanup retain the guard. This follow-up still needs runtime confirmation.

The same log resolves the key destinations as `EoT`, `Dun1Q10`, `Dun2Q10`, `Dun3Q10`, `Bazaar`, `Observatory` (in displayed order). Destination resolution is confirmed for this session; successful key teleport travel was blocked and remains to be tested.

Use test-menu selection **11** after refreshing `chore/test-queue-runner`. The runner builds against the installed SDK and resolves Harmony patch targets before installing the DLL.

1. **Option off:** check ordinary waypoint travel, unlocked-waypoint favourites, Safe Teleport and the main-quest action retain their existing behaviour. No new map-click travel should occur.
2. **Controls:** verify only the Travel Anywhere toggle/status remain, alongside the key teleport list. There must be no full scene dropdown or Travel-to-selected-area button. Close/reopen and switch locales without duplicating controls.
3. **Non-waypoint area:** use the map menu for a known campaign location without a waypoint. Confirm correct destination and usable spawn, camera, movement/pathfinding, mobs, damage, XP and loot. Walk through an exit/NPC interaction afterward, then travel back.
4. **Map/menu:** test a non-waypoint node, a locked waypoint and an unlocked waypoint. Left-click must keep opening the normal area menu without the mod starting travel; confirm its waypoint/Travel controls become usable while enabled, then travel from that menu. Right-click supported nodes and verify one `[TravelAnywhere] Load` and one completion. Test a popup over a node, the same-area node, each era and controller menu navigation. Disable with a menu already open; its temporary action must be blocked. Reopen it and check ordinary availability. Repeat with Unlock All Waypoints on/off, close/reopen the map and change eras to check flag restoration/reused widgets.
5. **Spawn gates:** test ordinary gate-zero scenes and nodes with nonzero gates. Check placement actually reaches the selected location rather than another entrance. Missing/unusable spawns should retain the source and report a recovery outcome.
6. **Busy/recovery:** double-click, click another node while loading, turn the option off during loading and try Safe Teleport. Verify no second load. For a failed/timed-out destination, check the original area remains playable, position/camera/navigation recover and partial destination cleanup finishes before another request is accepted.
7. **Favourites:** save a non-waypoint area, leave, return using its favourite, remove/re-add and restart. With Travel Anywhere disabled, the non-waypoint favourite must be blocked. Verify no new entries were added to `UnlockedWaypointScenes`.
8. **Long session:** repeat several trips, including a town, combat area and End of Time; also test leaving an echo/arena for a static area. Watch for duplicate players/managers, missing enemies, broken portals, frame-time/memory growth and quest state problems. Return to character selection and load another character.
9. **Persistence/locales:** restart with the option enabled and disabled; check EN > FR > KO > ZH > EN controls, native location names, status labels and selected area identity.
10. **Parent features:** retest fixed gold, all key teleport buttons (including dungeon entrances), Alt-dragged/saved/clamped counter position, natural favour gains, manual grant exclusion, session-counter zone continuity/reset, both kinds of favourites and Safe Teleport. Key teleports must retain their unlocked-waypoint route with Travel Anywhere on/off. Other pending Force Drop/crafting/drop-rate branches are separate builds.

Keep the complete log and exact commit. Expected bounded lines are `[TravelAnywhere] Load`, `Complete` or a reason for recovery. On a build failure, retain `build.log`; on native target failure, retain `tests.log`. The runner stops before installation in either case.

## Recorded verification

- The Windows build of `5744c5b7` failed before installation with CS1061: the generated `IList<SceneDetails>` wrapper does not expose inherited `Count`. The fix reads `Count` through its native `ICollection<SceneDetails>` interface and retains the list indexer. It uses the actual collection size, not a fixed scene count. The subsequent `2466cc0a` log confirms the fixed native build loaded.
- Direct Roslyn/.NET 8 execution: **1,074 passed, six SDK-dependent checks skipped**, zero failures. Includes 40 new cases for historical destination exclusions, generated-area sources and transition ordering/failure/recovery.
- Changed C# source checked with CSharpier; whitespace and locale JSON checks passed. The menu/right-click follow-up changes four C# files; unrelated locale entries are preserved while the map guidance is updated in all five locales.
- The compiled historical API and source were compared. No old DLL was installed or executed.
- Native compilation and loading of `2466cc0a`, plus map-menu travel, are user-confirmed. This follow-up’s native build, key teleport travel and detailed scene-recovery regressions remain **pending**. Pure tests confirm the protocol rules, not current-game success. Temporary native flags, normal left-click menus, right-click dispatch and stale-menu restoration are not covered by those pure tests; use the runtime checks above.

Unity references used during the review: [additive scene management](https://docs.unity.com/en-us/engine/6000.7/script-reference/unityengine/scenemanagement/scenemanager), [SetActiveScene result](https://docs.unity3d.com/ru/2021.1/ScriptReference/SceneManagement.SceneManager.SetActiveScene.html), [root-only scene transfer](https://docs.unity3d.com/ja/current/ScriptReference/SceneManagement.SceneManager.MoveGameObjectToScene.html), and [async scene unload](https://docs.unity.com/en-us/engine/6000.3/script-reference/unityengine/scenemanagement/scenemanager/unloadsceneasync), and [async operation controls](https://docs.unity3d.com/2021.3/Documentation/ScriptReference/AsyncOperation.html). The [Unity pointer-button reference](https://docs.unity.cn/Packages/com.unity.ugui%402.0/api/UnityEngine.EventSystems.PointerEventData.InputButton.html) was also checked for right-click dispatch. These describe Unity operations, rather than Last Epoch's service invariants.

## Dungeon preset hang — 2026-10-09 follow-up

The user reports being stuck after dungeon preset travel and needing Alt+F4. Latest log confirms build `f53998db`: Observatory travel at 09:17:45.484 followed by `Dun2Q10` at 09:17:54.815. Player log shows scene load and `ClientDungeonService.HandleTransition`, not a rejected/missing scene. The previous run similarly reaches `Dun1Q10`. Later AbilityObjectIndicator.OnDestroy exceptions occur during shutdown and do not identify the initial hang cause. The exact reason dungeon handling stalls is unproven.

The fix avoids that path: dungeon presets target the campaign areas outside the entrances, not Dun* lobby scenes. Temporal Sanctum uses Ruined Coast; Lightless Arbor prefers Shrouded Ridge when it has a waypoint, otherwise Surface; Soulfire Bastion uses Felled Wood. Identifiers are resolved from live map data, not guessed. Campaign names are corroborated by the developer-hosted [game guide](https://forum.lastepoch.com/t/community-game-guide/26057/11). The English-name resolver can remain unavailable in other locales; it must reject missing/ambiguous matches rather than guessing a scene.

Generic teleport validation and direct Travel Anywhere exclude Dun+digit scenes. Already-saved Dun1Q10/Dun2Q10/Dun3Q10 favourites redirect to their campaign preset; other dungeon scenes are blocked with a status message. Existing favourites are not deleted. Normal native dungeon map actions retain their own game handling.

### Retest selection 11

- Close the game and rebuild/reinstall selection 11. Open the world map, then Refresh key teleports. Keep the new `[KeyTeleports]` mapping.
- Test all three dungeon buttons from End of Time. They must target campaign scenes, never Dun1Q10/Dun2Q10/Dun3Q10, and finish loading with movement/camera working. A missing or locked waypoint must report unavailable without starting travel.
- Repeat from Observatory/Bazaar; double-click a button and check no overlapping transitions. Check End of Time, Bazaar and Observatory still travel normally.
- Use an old saved dungeon-lobby favourite: it should redirect safely or report unavailable. Other dungeon-room favourites must be blocked. Save/remove normal favourites and restart to check persistence.
- Walk into each dungeon through its normal entrance and check its ordinary key/tier flow. With Travel Anywhere enabled, dungeon scenes must not be force-loaded additively by this mod.

This is a source/core-tested correction; native compilation and in-game success remain pending. It avoids the reproduced failing route and does not claim to repair arbitrary stalled native dungeon transitions.

Preset and generic-waypoint validation uses each map pin’s original waypoint flag, excluding temporary Travel Anywhere adapters. Verify this with Travel Anywhere on/off: a synthetic non-waypoint button must not become a generic waypoint target. Validation: 1,103 core tests passed, six native-SDK checks skipped; CSharpier passed.
