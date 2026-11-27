# Nemesis - Procedural Dungeon Generation

The `AshenDungeon` scene creates a new connected dungeon at runtime from the imported CC0 art packs.

## Generation pipeline

1. Place non-overlapping rectangular rooms inside a bounded grid.
2. Connect each room to the previous one with multi-cell corridors, then add occasional loop connections to create alternate routes.
3. Validate walkable connectivity with a flood-fill pass and repair any disconnected room.
4. Fill walkable cells with randomly selected stone, cracked-stone, and cobblestone sprites. Build stone-wall boundaries with collision shapes.
5. Assign room roles: entrance, combat, treasure, shrine, and boss. Decorate each with a mixture of pillars, barrels, cages, webs, mushrooms, moss, torches, crystals, and traps.
6. Spawn randomized gothic enemies in intermediate rooms. Treasure rooms hold several relic chests. Shrine rooms contain a one-use healing shrine. The final room contains the Hollow Knight and a gate that unlocks after the boss is defeated.

## Randomness and replay

- `seed = 0`: a fresh random seed is chosen on every generation, including F5 regeneration.
- Any non-zero `seed`: the same layout is reproduced every time.
- The actual seed and room count are written to the Unity player log and shown in the HUD.
- `roomCount`, `mapSize`, and `corridorWidth` control layout scale and corridor width.

## Navigation and interaction

Fog of war covers unexplored cells in near-black. The player reveals a radius around themselves; explored rooms remain dimly visible, while walls block sight. The same system is used in the starting outskirts and generated dungeon. Dungeon enemies use grid-based A* pathfinding to route around stone walls and follow corridors. The player can use E to open chests, activate a shrine, or use a portal. Pressure traps damage the player while they remain on the trigger. F5 generates a fresh layout during play.

## Scenes

- `AshenOutskirts`: starting area, Ash Keeper, relic chest, enemies, and descent portal to the dungeon.
- `AshenDungeon`: runtime-generated dungeon with a return portal in the entrance room and a boss-locked gate in the final room.

Both scenes must remain enabled in `ProjectSettings/EditorBuildSettings.asset` and included by `NemesisBuild.BuildWindows`.

## Current prototype limits

The imported sprite packs provide static images, not animation sheets. A* currently uses four-neighbour movement on the isometric grid. More elaborate room templates, multiple floor themes, animated hazards, locked doors and keys, minimap, save/progression systems, and final combat/visual polish remain future work. Collision and visual readability should still be reviewed in the running game before production use.

