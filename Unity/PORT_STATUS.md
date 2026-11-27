# Unity development status

Updated: 2026-11-27  
Target: Unity 6 (6000.0.85f2)

## Milestone 0 — Unity foundation and content bootstrap

- [x] Unity project structure and C# runtime scripts added.
- [x] Unity Input System dependency and unified keyboard/mouse/gamepad adapter.
- [x] Isometric camera and camera-relative movement.
- [x] Sprint, stamina, dodge, HP and Essence.
- [x] Basic melee and five abilities.
- [x] Neutral-until-provoked Ash Raider prototype.
- [x] Ashen Warlord boss prototype with telegraphed area attack.
- [x] Procedural low-poly character, equipment and environment placeholder kit.
- [x] Editor menu for asset library and prototype scene generation.
- [x] First-run Editor bootstrap that creates the prototype scene and registers it in Build Settings.
- [x] Editor catalog scanner for imported Prefab/Model assets.
- [x] Scanner explicitly includes GLB/GLTF assets under Assets/NemesisContent/Imported.
- [x] Unity glTFast 6.21.0 dependency configured for GLB import.
- [x] Kenney Mini Dungeon 2.0 added to the packaged Unity project: 30 GLB models, shared colormap and original CC0 License.txt.
- [x] CI source-archive job successfully downloaded the pack and created a validated project ZIP.
- [x] Local player combat telemetry and first bounded enemy adaptation response.
- [x] Seeded Ashen Reach hub source: crossroads, central plaza, four districts, 12 buildings, landmarks and outer spawn points.
- [x] First interaction loop: Mira dialogue and one-use Essence cache.
- [x] Collision-aware CharacterController movement for raiders and the boss.
- [ ] Open the updated project in Unity Editor, confirm Package Manager resolves glTFast, compile and playtest.
- [ ] Run Tools/Nemesis/Scan Imported Asset Packs and verify the actual selected GLB models in the generated catalog.
- [ ] Verify GLB scale, orientation, texture mapping and rig import in the target scene.
- [ ] Verify hub navigation, NPC interaction range and chest reward in Play Mode.
- [ ] Confirm Windows x64 build after configuring the GitHub secret UNITY_LICENSE.

## Milestone 1 — playable vertical slice

- [ ] Verify all scripts and generated prefabs in Unity.
- [ ] Add robust hit detection, damage feedback and combat state handling.
- [ ] Configure Animator controllers and integrate imported character rigs/animations.
- [ ] Add final environment dressing, VFX and sound.
- [ ] Add EditMode and PlayMode tests.
- [ ] Build a small hub encounter loop with one NPC, one dungeon room and one boss.
- [ ] Add the first complete dungeon room using the imported modular wall, floor, gate and prop models.

## Milestone 2 — world and progression

- [ ] Runtime-verify seeded hub generation and stable spawn-safe layout.
- [ ] Modular dungeon room graph, connectors and validation.
- [ ] Enemy archetypes and encounter director.
- [ ] Inventory, equipment, loot, vendors and progression.
- [ ] Local save/load with schema versioning.
- [ ] Multiple dungeon themes and repeatable room modifiers.

## Milestone 3 — Nemesis system

- [ ] Player combat telemetry with bounded and decaying counters.
- [ ] Readable, capped enemy adaptation.
- [ ] Nemesis Hunter director, unique identities and encounter chronicle.
- [ ] Persistence and reward deduplication.
- [ ] Nemesis-specific cosmetics, gear drops and escalating rival encounters.

## Verification boundary

Checked items describe source, content or CI packaging steps that are present and completed. They do **not** mean Unity runtime behavior has been verified. The latest CI run successfully packaged the source archive and downloaded the asset pack. Its Windows build job stopped before Unity compilation because no UNITY_LICENSE secret was available. Do not mark compile, Play Mode or executable build tasks complete until they are executed in the target Unity Editor.

## Historical migration notes

The initial prototype was developed on Roblox. Its design, balance notes and mechanic reviews informed this Unity/C# implementation. Roblox is no longer an active development target. See `docs/UNITY_PORT_PLAN.md` for the migration rationale and retained design knowledge.
