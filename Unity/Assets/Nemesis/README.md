# Печать бездны

## Current playable slice

Unity editor: 6000.0.85f2

- `Assets/Nemesis/Scenes/AshenOutskirts.unity` - starting area with the Ash Keeper, relic chest, enemies, and descent portal.
- `Assets/Nemesis/Scenes/AshenDungeon.unity` - runtime procedural dungeon generated from imported sprites.
- Windows build: `Builds/Nemesis-Windows/Nemesis.exe`

## Controls

- WASD: move
- Left mouse button or J: melee attack
- Space: dodge
- E: interact with the Ash Keeper, relic chest, or dungeon descent
- F5: generate a fresh dungeon layout while inside the dungeon

## Fog of war

The player reveals nearby cells in a circular field of view. Unexplored terrain is hidden by a near-black overlay; previously explored terrain stays dim, and stone walls block visibility. The same top-down fog-of-war system runs in Ashen Outskirts and in the generated dungeon. The exploration radius is adjustable on the fog component / dungeon generator.

## Procedural dungeon generation

`NemesisDungeonGenerator` builds a connected room-and-corridor layout at runtime. It fills walkable cells with randomly selected stone floor sprites, creates wall boundaries and collisions, decorates rooms with props, traps, torches, moss, crystals and relic chests, spawns enemies in intermediate rooms, and places the boss in the final room.

- `seed = 0`: choose a new random layout on each dungeon entry.
- `seed = any non-zero integer`: reproduce the same layout.
- `roomCount`: target number of rooms.
- `mapSize`: grid dimensions.

The Ashen Outskirts descent portal loads `AshenDungeon`. Both scenes are included in the Windows build. See `PROCEDURAL_DUNGEONS.md` for the generation pipeline and current limitations.

## Current systems

- 2D top-down/isometric-style movement and camera follow
- Fog of war in the starting area and procedural dungeons: unseen cells are covered, explored cells stay dimmed, and walls block line of sight (default vision radius: 8 cells)
- Player health, melee damage, dodge invulnerability
- Enemies with awareness, chase and melee attack behavior
- Boss encounter in the final generated room
- Interactable relic chests, NPC dialogue, relic and XP counters
- Runtime dungeon layout generation with fresh random seeds or deterministic seed replay
- Flood-fill connectivity validation, loop corridors, and A* enemy navigation
- Room roles: combat, treasure, one-use healing shrine, and boss gate
- Damage traps, multiple enemy variants, and boss-locked return gate
- CC0 dungeon environment sprites and gothic character/enemy sprites imported under `Assets/ThirdParty/NemesisFreeCC0`

## Rebuild

Open the project in Unity 6000.0.85f2. Use `Nemesis > Build Ashen Outskirts Scene` to regenerate the starting area and `Nemesis > Generate Procedural Dungeon Scene` to recreate the dungeon scene. Build Windows x86_64 using `NemesisBuild.BuildWindows` or the Unity Build Profiles window. The project is configured to support both the legacy Input API and the Input System package.

This remains an early playable prototype, not a finished game. The imported sprite packs currently provide static art, not animation sheets. Room themes, proper pathfinding, trap damage, locked doors and keys, minimap, save/progression systems, and full combat/visual polish are still future work.



