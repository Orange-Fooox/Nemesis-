# Nemesis prototype and imported asset library

## Included imported pack

The source-archive workflow downloads **Kenney Mini Dungeon 2.0** from the creator's official download URL and extracts it into:

`Assets/NemesisContent/Imported/KenneyMiniDungeon/`

- Creator: Kenney
- License: **Creative Commons CC0 1.0 Universal**
- Official asset page: https://kenney.nl/assets/mini-dungeon
- Official download URL: https://kenney.nl/media/pages/assets/mini-dungeon/6cd72dc849-1785314274/kenney_mini-dungeon.zip
- Scope: 30 low-poly 3D objects for dungeon and roguelike games, including environment pieces, props, weapons and shields.
- Optional credit: "Mini Dungeon by Kenney". Credit is not required by CC0.

The workflow checks that 3D model files exist before it creates the Unity ZIP. The extracted source files and an `ASSET_SOURCE.md` provenance note travel with the archive. The pack is downloaded during packaging rather than stored as a large binary commit in the Git history.

## Importing the assets into the runtime catalog

After opening the project and allowing Unity to import the models:

1. Run **Tools → Nemesis → Scan Imported Asset Packs**.
2. The scanner indexes Prefab and Model assets, including content under `Assets/NemesisContent/Imported/`.
3. It writes the runtime catalog to `Assets/Resources/NemesisAssetCatalog.asset`.
4. It writes a report to `Assets/NemesisContent/ImportedAssetReport.md`.
5. The world generator uses safe matching assets for eligible props (for example, columns, chests, barrels and crates), with generated fallback visuals retained.

The scanner skips visuals with embedded custom MonoBehaviour scripts or without a Renderer. It only indexes content that is already present; it does not download packages or bypass their licenses.

## Original generated prototype assets

The Editor generator creates local prototype prefabs and materials under:
- `Assets/NemesisContent/Prefabs/`
- `Assets/NemesisContent/Materials/`

They include an armored player, ash raider, Ashen Warlord, merchant, weapons, armor and environment pieces. These primitive-based greybox assets remain safe fallbacks when no imported model matches.

## Future content

Additional packs should be added only when the creator's own source page documents a compatible license. Keep their original source and license notices adjacent to their extracted files under `Assets/NemesisContent/Imported/PackageName/`. A $0 Asset Store price alone does not grant redistribution rights.
