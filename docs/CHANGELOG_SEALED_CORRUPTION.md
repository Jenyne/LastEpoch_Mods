# Force Drop: sealed and corrupted affixes

- Remove the rejection based solely on the native special-affix operation's regular-seal output.
- A regular sealed affix and a corruption-sealed affix can be retained independently; this does not require illegal-item mode.
- Verify that existing regular and primordial seal flags remain unchanged.
- Keep id/tier/roll/type checks and compare all original affixes after packing.
- Verify that each seal flag matches exactly one corresponding affix after the final refresh.

Validation: C# syntax/API checks. In-game validation pending: one regular sealed affix plus corruption, four ordinary affixes plus both seals, unique/legendary items, and save/reload. The native operation may have further restrictions; report the exact new drop error if it still refuses the combination.
