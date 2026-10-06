# Prophecy reward multiplier — pending game testing

- Character > Cheats: enable Multiply Prophecy Rewards and enter a whole-number multiplier from 1 to 10.
- Default disabled, multiplier 1; persists in SaveModUI.json.
- Scoped to the local player's native SpawnRewardForPlayer call. Reward-count hooks multiply positive quantities once, including nested calls.
- Does not repeat prophecy fulfillment, alter saved reward definitions or change charge-consumption arguments. Native reward previews remain unchanged.
- English, French, Korean and Simplified Chinese labels supplied.

Validation: native signatures checked against supplied Il2CppLE.dll; syntax checked. Full compilation and gameplay pending.

Test 1x/2x/10x counts on equipment, idols, materials and boss-specific rewards; compare charge consumption with the disabled setting. Test lenses and doubled rewards, zone changes and saved settings after restarting. Monitor the one-time warning if no quantity hook is reached: native inlining or a different count path may require another patch.
