# Visual/animation implementation notes — v0.4.1

## Asset strategy
Use curated Creator Store models through Studio Toolbox. Creator Store includes models, meshes, PBR materials, visual effects, audio, and UI assets. Inspect each asset's publisher, ratings, preview, technical details, and embedded scripts before insertion. Disable scripts that are not required.

### Shortlist
- Forest Pack (asset 6432306802): environment foliage and ground dressing. Creator Store: https://create.roblox.com/store/asset/6432306802/Forest-Pack
- Synty Dungeon Pack: Cave & Castle Interiors (asset 6934021345): modular dungeon architecture. Creator Store: https://create.roblox.com/store/asset/6934021345/Synty-Dungeon-Pack-Cave-Castle-Interiors
- 70+ R15 Fantasy RPG Combat Animation Pack (asset 118285911149600): optional paid pack, $14.99 at time checked; 72 clips across sword, mage, archer, and dual-blade styles. Not acquired or inserted. Creator Store: https://create.roblox.com/store/asset/118285911149600/70-R15-Fantasy-RPG-Combat-Animation-Pack

## Implemented in source
- Client-side procedural melee swing for R15/R6 shoulder joints, with a quick wind-up, strike, recovery, and short-lived blade trail.
- Heavy swing variant used for skills.
- HUD panel palette warmed to parchment/bronze rather than near-black.

## Animation pipeline
Roblox animation assets need to be published and permissioned for the experience. The official Animation Editor workflow requires a rig, authored keyframes, and publishing to obtain an animation asset ID. Animation packs have stricter CurveAnimation requirements and are not automatically interchangeable with arbitrary animation clips. Do not paste unverified IDs into production scripts.

## QA limitations
This repository update is source-only. It has not been executed in Roblox Studio in this environment. In-game screenshots and performance checks must be captured from an actual Studio Play session; concept images are not represented as gameplay screenshots.
