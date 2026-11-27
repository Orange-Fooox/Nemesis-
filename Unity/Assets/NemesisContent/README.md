# Nemesis prototype and imported asset library

## Bundled imported pack: Kenney Mini Dungeon 2.0

The source-archive workflow downloads the original Kenney 2.0 model set from a pinned public mirror and extracts it into:

`Assets/NemesisContent/Imported/KenneyMiniDungeon/`

It includes 30 GLB models: animated human and orc characters, a chest, gate, walls, floor pieces, column, stairs, barrel, shield variants, sword, spear, coins, potions and dungeon props, plus the shared colormap and original `License.txt`.

- Creator: Kenney
- Original pack page: https://kenney.nl/assets/mini-dungeon
- License: **Creative Commons Zero 1.0 Universal (CC0)**
- Creator license included in the asset folder: `License.txt`
- Optional credit: "Mini Dungeon by Kenney (kenney.nl) — CC0"

The official Kenney page marks the pack CC0. The original license file states the models may be used for personal, educational and commercial purposes, with attribution optional. The GitHub mirror is used as a stable download endpoint because the creator's direct CDN returned HTTP 403 to the CI runner; the downloaded models remain the original Kenney content and the upstream license file is retained.

## GLB import support

Unity's official glTFast package (`com.unity.cloud.gltfast`) is pinned in `Packages/manifest.json`. It imports `.glb` files as native Unity assets. The shared colormap is kept in the same relative folder structure as the models.

## Indexing imported assets

After opening the project and allowing Unity to import the models:

1. Run **Tools → Nemesis → Scan Imported Asset Packs**.
2. The scanner indexes Prefab and Model assets, and explicitly includes GLB/GLTF assets under `Assets/NemesisContent/Imported/`.
3. It writes the runtime catalog to `Assets/Resources/NemesisAssetCatalog.asset`.
4. It writes a report to `Assets/NemesisContent/ImportedAssetReport.md`.
5. Eligible world and equipment props use matched models; generated fallback visuals remain active for unsupported categories.

The scanner skips visuals with embedded custom MonoBehaviour scripts or without a Renderer. It only indexes content already imported by Unity; it does not execute third-party package scripts.

## Original generated prototype assets

The Editor generator creates local prototype prefabs and materials under:
- `Assets/NemesisContent/Prefabs/`
- `Assets/NemesisContent/Materials/`

These include an armored player, ash raider, Ashen Warlord, merchant, equipment and environment pieces. They remain fallbacks for categories not covered by the imported models.

## Future content

More packs can be added after the creator's source page and license have been checked. Keep each source's original license and provenance note beside the imported files under `Assets/NemesisContent/Imported/PackageName/`.
