# Free Forest Tileset: Terrain, Trees and Campsite Props

This woodland selection joins five 32×32 ground textures with trees, bushes, a cabin, a bridge, and small wildlife. The large oak and pine establish the scale of the canopy while mushrooms, flowers, and rocks fill smaller gaps. It is a collection of individual images for assembling a forest scene, with no finished map bundled.

This pack contains 30 assets: 5 tilesets, 15 environment, 4 buildings, 2 items, 4 characters. There are 21 distinct native canvas sizes. The complete size histogram appears below and in manifest.json. All downloads in the archive are PNG; the WebP preview is a labeled visual index, not an asset atlas.

Grass, dirt path, stream, mud, and fallen leaves provide the ground choices. The oak, pine, dead tree, stump, fern, fallen log, and two bushes give you distinct vegetation silhouettes. A cabin, campsite tent, hunter’s blind, and wooden bridge supply four built structures. The campfire and wood pile can anchor a small clearing. Berry foliage, red mushrooms, a mushroom cluster, two boulders, and red and yellow flowers are additional details. The orange and blue butterflies, rabbit, and owl are stored under the characters category even though they are wildlife rather than playable humanoids.

Lay down grass or dirt first, then build a clearing around the cabin or tent. Put tall trees behind smaller bushes and rocks so the trunks remain understandable. A bridge can provide a visual crossing over stream patches, but the water texture and bridge are separate files, with no automatic junction system. Fallen leaves can distinguish a second area without changing the entire vegetation set. Flowers and mushrooms have small footprints and work better as accents than as replacements for the ground texture.

The five ground images share a 32×32 canvas, but the object images use different bounds. The oak is 48×64 and the pine is 32×56; the cabin is 48×40 and the bridge is 36×20. Preserve these native sizes initially and establish a common baseline for objects placed on the same ground. The perspective is a mixture of ground texture patches and front-facing object silhouettes, so this is not a verified isometric tileset. A canopy may overlap an actor even when its collision shape is much smaller.

Every member is static, including the butterflies and campfire. You will need additional poses or frame drawings for moving wildlife and flickering fire. There are no terrain corner sets, seasonal swaps, collision polygons, or gameplay scripts. Orange and blue butterflies and the two flower colors are intentional small color choices; large and small bushes and rocks provide useful scale differences. Repeating textures across a broad area may reveal joins, so check tiling in the intended map instead of assuming edge continuity.

Each member record declares CC0. Under CC0, attribution is not required and commercial use and modification are allowed; retaining the included notice helps document where these files came from. This summarizes the catalog’s declaration, not an independent rights audit or a warranty about trademarks, publicity rights, or third-party material. README.md and LICENSE.txt accompany the image files.

Godot 4: import PNGs as textures and assign them to Sprite2D nodes. Choose Nearest filtering on CanvasItem for pixel art. Configure origins and placement for each file.

Unity: import each PNG as Sprite (2D and UI), with Sprite Mode Single. Use Point filtering for pixel art. Choose pixels per unit for your scene; the preview is not a sheet to slice.

GameMaker: import each unrelated image into its own sprite and set origins consistently. Disable texture interpolation for crisp pixel art. Multiple images in one sprite become subimages; this collection supplies no animation sequence.

Revision: 2026-10-12

Inventory and checksums: manifest.json. PNGs are native size; preview.webp is a labeled index, not a tilesheet.

1. forest-grass-32: 32x32, static
2. forest-dirt-path-32: 32x32, static
3. forest-stream-32: 32x32, static
4. forest-mud-32: 32x32, static
5. forest-fallen-leaves-32: 32x32, static
6. forest-oak-tree: 48x64, static
7. forest-pine-tree: 32x56, static
8. forest-dead-tree: 28x48, static
9. forest-tree-stump: 24x20, static
10. forest-large-bush: 28x20, static
11. forest-small-bush: 18x14, static
12. forest-fern: 16x14, static
13. forest-fallen-log: 32x16, static
14. forest-log-cabin: 48x40, static
15. forest-wooden-bridge: 36x20, static
16. forest-campsite-tent: 28x24, static
17. forest-hunters-blind: 32x40, static
18. forest-campfire: 20x16, static
19. forest-mushroom-red: 12x14, static
20. forest-mushroom-cluster: 20x14, static
21. forest-boulder-lg: 24x18, static
22. forest-boulder-sm: 16x12, static
23. forest-flower-red: 12x14, static
24. forest-flower-yellow: 12x14, static
25. forest-berry-bush: 20x16, static
26. forest-wood-pile: 20x16, static
27. forest-butterfly-orange: 10x8, static
28. forest-butterfly-blue: 10x8, static
29. forest-rabbit: 14x12, static
30. forest-owl: 14x16, static
