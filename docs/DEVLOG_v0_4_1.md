# v0.4.1-dev.1 — First playable combat & dungeon pass

## Included
- Moved six hub enemy spawn points into the central districts so encounters are visible and reachable shortly after spawn.
- Added the first dungeon, **Затонувший склеп**, with a glowing entrance gate, server-side travel, a return portal, stone pillars, runes, an altar, and five enemy encounters registered with the existing AI.
- Added two visible hub NPCs: the Gatekeeper and Quartermaster, each with an E-to-talk ProximityPrompt and dialogue toast.
- Added a clickable skill bar alongside keyboard controls, plus nearest-enemy fallback for basic attacks.
- Added dark-fantasy lighting, atmosphere, color grading, cursed lanterns, hit flashes, and floating damage numbers.
- Added the ShowNotice RemoteEvent. Project version: 0.4.1-dev.1.

## Controls
- Left mouse / F: basic attack
- Z: Seal Strike; X: Shadow Dash; C: Earth Rend; V: Abyss Release; B: Barrier
- E: interact with dungeon gates and NPCs
- Shift: sprint

## Test notes
The exported place XML parses successfully and source changes were reviewed statically. A Roblox Studio Play test is still required for runtime verification.
