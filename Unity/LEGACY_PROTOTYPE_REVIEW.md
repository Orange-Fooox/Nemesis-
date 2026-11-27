# Roblox mechanics audit for the Unity port

Updated: 2026-11-27

This document records a static source review of Roblox main and the current Unity unity-port branch. Roblox main is the reference project and was not modified by this audit.

## World generation

### Implemented in Roblox source
- HubGenerator.Generate(seed) builds a 420×420-stud hub.
- The same seed drives building offsets, rubble, and enemy spawn marker offsets.
- The hub contains a collidable ground plane, protected spawn plaza, perimeter walls, cross streets, central square, altar, 12 building slots, tree/lantern decoration, 90 rubble attempts, and six enemy spawn markers.
- Bootstrap.server.lua disables automatic character loading while the world is generated, validates the ground and spawn, creates a fallback floor/spawn when needed, initializes population/dungeon services, and only then enables character loading.
- PopulationService sorts enemy names and spawn markers before seeded selection and creates up to the configured six R6 enemies.

### Gaps and risks
- Only the hub and a fixed first dungeon layout exist. The documented modular room-graph dungeon generator, loot/event generators, region population, patrol/respawn logic, and separate subsystem seeds are not implemented.
- Hub generation is deterministic for the same seed and code version, but it does not yet use independent derived seeds for hub/population/dungeon/loot/events.
- Rubble generation makes 90 placement attempts, not a guarantee of 90 placed pieces; collisions with protected areas and enemy markers are filtered, but building overlap is not comprehensively tested.
- The first dungeon is a fixed greybox layout, not a procedural room graph and not yet built from the 542 PolygonDungeon MeshParts.
- Roblox Studio, Luau compilation, and multiplayer runtime were not available in this review, so runtime behavior remains unverified.

## Nemesis system

### Implemented in Roblox source
- NemesisService records melee/ranged counters, dodges, blocks, encounter count, and a server-global regional threat value.
- CombatService records confirmed basic melee attacks and Barrier use; EnemyAIService records the start of an encounter.
- Regional threat can be increased/reduced and mapped to Calm, Uneasy, ReinforcedPatrols, EliteThreats, Invasion, and RegionalEvent.

### Critical source defect
The checked-in Roblox/Source/Server/NemesisService.lua and the embedded copy in Roblox/Releases/NEMEZIDA_Studio_v0_4_dev6.rbxlx both omit the function-closing end after the if block in NemesisService.GetThreatState(). The next Players.PlayerRemoving connection is therefore parsed inside the function body and the module's final structure is incomplete. This is a Luau syntax blocker that must be corrected before treating the module as loadable. Roblox main was deliberately left untouched.

### Missing gameplay integration
- The recorded profile is memory-only and is deleted when the player leaves; there is no DataStore persistence or decay.
- The profile is not actually used to adapt enemy behavior.
- Ranged actions and dodges are not currently recorded by the reviewed CombatService/MovementService call sites; Barrier is counted as Block.
- No procedural Nemesis Hunter spawn, unique hunter identity, encounter history, hunter reward flow, or anti-repeat encounter persistence is wired into the playable source.
- Regional threat is one server-wide integer, not tracked per region and not persisted.
- The design document's 650 HP / 55 ATK / 20 DEF hunter and 10% event chance are design targets, not implemented runtime values.

## Unity port status and parity gaps

- Unity currently has a simple seeded-independent greybox arena, one standard raider, one Ashen Warlord boss, and a merchant.
- Unity's standard enemy becomes hostile after damage and has a short alert timeout; it does not yet reproduce Roblox pathfinding, line-of-sight checks, home return, or server-authoritative provocation.
- The Unity boss is a separate 450 HP telegraphed AoE prototype. It is not the Nemesis Hunter system and does not adapt to player habits.
- The current Unity asset library generator creates original free primitive-based prefabs/materials. These are reusable placeholders, not imported third-party Asset Store packages or final art.
- No Unity Editor compile/playtest has been run in this environment.

## Recommended port order

1. Fix and validate the Roblox NemesisService syntax in a future Roblox maintenance change (not in this Unity-only branch).
2. Add a seeded Unity hub generator with isolated seeds for world layout and population; guarantee spawn-safe space and stable landmark positions.
3. Port encounter telemetry with bounded/decaying counters and save schema versioning.
4. Add readable, capped enemy adaptation (maximum +20% resistance) and telegraphed counter-patterns.
5. Implement a real Nemesis Hunter director: eligible encounter trigger, 10% base chance, unique ID/profile, spawn limits, reward deduplication, and chronicle.
6. Add persistence only after the data model and encounter lifecycle are tested.
