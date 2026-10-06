# Keyless dungeon entry and all Herald idols — diagnostic build

This branch adds bounded logs only. It does not restore Herald items or change dungeon/key behavior.

## How to collect the needed data
1. Build/install this branch.
2. Enable Scenes > Dungeons > Enter Without Key.
3. Open a dungeon entry panel, attempt entry without a key, choose an available tier and enter.
4. Repeat with a key if entry fails; note its count before and after.
5. Send Latest.log and describe which dungeon was tested. Relevant messages start with [KeylessHerald].
6. Disable Enter Without Key to disable these probes. Each event is limited to 12 logs per game session.

## What is recorded
- Entry UI, lobby attempt and client service calls.
- Early key-validity and key-affordability results, required subtype and amount.
- Item removal calls during a dungeon lobby; item base/subtype only.
- Actual loaded bundle assets containing Herald.
- All currently registered unique items containing Herald.
- Whether old Ice unique ID 504 and base 25/subtype 3 already exist.
- Availability of the old Ice source abilities, Avalanche and Maggot Explosion, and their prefab references.

Native signatures were checked against supplied Il2CppLE.dll and C# syntax parsed. Full build/game tests are pending. These probes may miss inlined or alternative native paths; absence of a log is a useful clue rather than proof that a method is unused.

## Source audit: all custom Herald idols
| Herald | Linked revision | Current master |
| --- | --- | --- |
| Ice | Item, player/minion kill hooks and explosion prototype | Icon assets; implementation missing |
| Ash | No implementation found | Icon assets; no implementation found |
| Thunder | No implementation found | Icon assets; no implementation found |
| Agony | No implementation found | Icon assets; no implementation found |
| Purity | No implementation found | Icon assets; no implementation found |

Compared RCInet revision 88689d683eeea0e9f7940574736b89e462ed83a5, RCInet master 6054a5f218594b7c8d632167ec6ac47fff9d7f46, RCInet 3.2.3 a4e46374b6fdeab61fa0d8064eeb7cb023787fc8, Jenyne master 165948b1dfa1881f3dee77286bbab63aa23aec32 and Syncingoutt master f553080c3acc48768dd1a4db2d71e487256d5488. This is an audit of those revisions, not a claim that no other historical implementation exists.

The Ice prototype needs a current soft-reference prefab loader, current casting signature, explicit damage conversion, guarded item registration and actor-safe kill handling. Its original code used an abilityPrefab property absent from the supplied current wrappers and a four-argument CastAfterDelay call; the current native signature has seven arguments. The other Herald behaviors require an actual design or another implementation source; icons alone do not define stats/effects.

The current keyless patch only changes IsOccupiedWithValidDungeonKey. The newer DungeonLobby/ClientDungeonService flow and key-spending path require live traces before extending it.
