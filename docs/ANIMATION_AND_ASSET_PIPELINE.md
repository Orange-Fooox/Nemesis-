# Unity visual, model and animation pipeline

Updated: 2026-11-27  
Target: Unity 6 (6000.0.85f2)

## Visual direction

The current executable prototype is a 3D isometric greybox. The intended production look remains dark fantasy: charcoal stone, ash, weathered iron, warm windows and lanterns, restrained violet magic accents, high readability from the isometric camera.

Imported low-poly models are used to replace generated primitive placeholders while combat, level layout and camera behavior are being verified. The first pack is a structural/environment kit, not a final character art set.

## Bundled model pack: Kenney Mini Dungeon 2.0

- Creator: Kenney.
- Original asset page: https://kenney.nl/assets/mini-dungeon
- License: Creative Commons Zero 1.0 Universal (CC0).
- Original `License.txt` is preserved in the imported pack folder.
- 30 GLB models, including animated human and orc characters, chest, gate, columns, walls, floors, stairs, barrel, spear, sword, shields and props.
- One shared colormap texture.
- Optional credit: "Mini Dungeon by Kenney (kenney.nl) — CC0".

The official Kenney CDN returned HTTP 403 to GitHub Actions. The pack is therefore fetched from a public mirror at a pinned commit, while preserving the author's original files and license notice. The CI workflow checks all 30 GLB files and packages them into the downloadable project ZIP.

Unity folder:

`Assets/NemesisContent/Imported/KenneyMiniDungeon/`

## GLB import

The project includes `com.unity.cloud.gltfast` version 6.21.0 in `Packages/manifest.json`. This enables Unity Editor to import GLB/GLTF models as project assets.

After first project import:

1. Wait for Package Manager to resolve glTFast and for all GLB assets to finish importing.
2. Run **Tools → Nemesis → Scan Imported Asset Packs**.
3. Inspect `Assets/Resources/NemesisAssetCatalog.asset`.
4. Review `Assets/NemesisContent/ImportedAssetReport.md`.

The scanner skips objects without a Renderer or objects containing embedded custom MonoBehaviour scripts. This is a conservative visual-only policy. It scans GLB/GLTF files explicitly under the Imported folder in addition to ordinary Prefab/Model assets.

## Runtime integration status

The asset catalog currently supports models for equipment and environment props by keyword. The market area has barrel and crate spawn points, and world props can use catalog entries where names match. Generated primitive fallbacks remain active when a suitable imported model is missing or unsafe.

The imported assets and catalog pipeline have been successfully packaged by CI. Actual Unity import, scale correction, material binding, rig/animation validation and Play Mode integration still require a run in the target Unity Editor.

## Animation plan

The Mini Dungeon pack includes animated human/orc characters, an animated chest, gate and trap. These animation clips are not yet wired into production Animator Controllers. The remaining integration work is:

- inspect the imported humanoid rig and decide the correct Avatar configuration;
- verify clip names, root motion and import settings;
- create Animator Controllers for the player and enemy prototypes;
- map idle, locomotion, attack, hurt, dodge and death states;
- build the boss set separately, because a standard humanoid rig is not assumed to fit the boss;
- test animation events and hit frames against actual combat windows.

Do not mark animations as integrated until they have been tested in Play Mode.

## Generated placeholders

The Editor asset generator creates primitive-based characters, props and equipment so the scene remains usable before final art is available. These assets are prototype fallbacks and not production-quality hand-painted models.

## Historical Roblox notes

The original Roblox Creator Store shortlist and R15/R6 animation notes are superseded by this Unity pipeline. Roblox was the initial prototyping platform only; all active implementation is Unity/C#.
