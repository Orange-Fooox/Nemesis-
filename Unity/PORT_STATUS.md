# Unity development status

Updated: 2026-11-27  
Target: Unity 6 (6000.0.85f2)

## Milestone 0 — Unity foundation
- [x] Unity project structure and C# runtime scripts added.
- [x] Isometric camera and camera-relative movement.
- [x] Sprint, stamina, dodge, HP and Essence.
- [x] Basic melee and five abilities.
- [x] Neutral-until-provoked Ash Raider prototype.
- [x] Ashen Warlord boss prototype with telegraphed area attack.
- [x] Procedural low-poly character, equipment and environment placeholder kit.
- [x] Editor menu for asset library and prototype scene generation.
- [x] Editor catalog scanner for imported Prefab/Model assets.
- [x] Local player combat telemetry and first bounded enemy adaptation response.
- [ ] Open in Unity Editor, compile and playtest.
- [ ] Confirm that the locally installed armory package is discovered and correctly fitted to character anchors.
- [ ] Confirm Windows x64 build.

## Milestone 1 — playable vertical slice
- [ ] Verify all scripts and generated prefabs in Unity.
- [ ] Add robust hit detection, damage feedback and combat state handling.
- [ ] Add proper Animator controllers, final models, VFX and sound.
- [ ] Add EditMode and PlayMode tests.
- [ ] Build a small hub encounter loop with one NPC, one dungeon room and one boss.

## Milestone 2 — world and progression
- [ ] Seeded hub generation and stable spawn-safe layout.
- [ ] Modular dungeon room graph, connectors and validation.
- [ ] Enemy archetypes and encounter director.
- [ ] Inventory, equipment, loot, vendors and progression.
- [ ] Local save/load with schema versioning.

## Milestone 3 — Nemesis system
- [ ] Player combat telemetry with bounded and decaying counters.
- [ ] Readable, capped enemy adaptation.
- [ ] Nemesis Hunter director, unique identities and encounter chronicle.
- [ ] Persistence and reward deduplication.

## Historical migration notes
The initial prototype was developed on Roblox. Its design, balance notes and mechanic reviews informed this Unity/C# implementation. Roblox is no longer an active development target. See `docs/UNITY_PORT_PLAN.md` for the migration rationale and retained design knowledge.

## Verification boundary
All checked items indicate source or editor tooling is present in the repository. They do not mean Unity runtime behavior has been verified. Do not mark compile, Play Mode or build tasks complete until they are executed in the target Unity Editor.
