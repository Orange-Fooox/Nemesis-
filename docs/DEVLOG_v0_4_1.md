# v0.4.1-dev.1 — First playable combat & dungeon pass

## Included
- Moved the six hub enemy spawn points into the playable central districts so enemies are visible and reachable shortly after spawn.
- Added a first dungeon, **Затонувший склеп**, with a glowing entrance gate in the Ashen Frontier, server-side teleport, return portal, pillars, altar, and five cloned enemy encounters registered with the existing AI.
- Added two visible hub NPCs: the Gatekeeper and Quartermaster. Their ProximityPrompts display dialogue through a client notice event.
- Added a clickable combat skill bar alongside the existing keyboard shortcuts.
- Added dark-fantasy lighting, atmosphere, and color grading.
- Added the ShowNotice RemoteEvent and advanced the project version to 0.4.1-dev.1.

## Controls
- Left mouse / F: basic attack (nearest enemy fallback within range)
- Z: Seal Strike
- X: Shadow Dash
- C: Earth Rend
- V: Abyss Release
- B: Barrier
- E: interact with dungeon gates and NPCs
- Shift: sprint

## Test notes
The project XML was parsed after modification. Static source review was performed. The updated place still requires a Play test in Roblox Studio for runtime verification; no Studio runtime is available in this environment.
