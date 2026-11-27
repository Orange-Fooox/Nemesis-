# Free Dungeon Tileset and Props

Build a small stone dungeon from five 32×32 floor textures, doorway pieces, storage props, and cave details. This selection keeps the darker stone and iron artwork together instead of mixing it with the larger illustrated dungeon assets. The rat and bat are small static decorations or enemy images; they contain no animation sequence.

This pack contains 30 assets: 5 tilesets, 6 buildings, 13 items, 4 environment, 2 enemies. There are 22 distinct native canvas sizes. The complete size histogram appears below and in manifest.json. All downloads in the archive are PNG; the WebP preview is a labeled visual index, not an asset atlas.

The floor choices are stone, cracked stone, cobblestone, lava, and water. Stone walls, an iron entrance, an iron door, an arch passage, stairs, and a pillar supply the architectural vocabulary. The two cages have visibly different sizes; the barrel, treasure chest, skull pile, and spike trap can occupy floor space without requiring a separate furniture collection. A potion shelf, weapon rack, cauldron, and bookshelf let you suggest storage or a work area. The hanging chain, crystal, mushroom, moss, vine, and web add accents around otherwise repetitive masonry.

Start with a floor texture and place the wall pieces above it in your scene. Use the arch or iron door as a focal entrance, then reserve open floor around the chest or trap so the silhouette remains readable. The bookshelf and rack are taller than a floor cell, so place them as separate objects rather than slicing them into tiles. The lava and water images are square texture patches; no connected shoreline or corner rules are supplied. Inspect repeated edges in your own map before treating any texture as seamless.

The pack deliberately combines 32px floor cells with narrower and taller props. Match object baselines manually: the pillar is 16×48, the arch is 40×48, and the hanging chain is 8×32. Their canvases describe image bounds, not collision footprints. Keep walls and freestanding props on different draw layers where necessary. There is no exported navigation mesh, collision setup, autotile configuration, lighting shader, or engine scene.

All members in this revision are static. The torch image depicts a flame but contains no flicker frames; the rat and bat have no walk or flight cycles. The older demo remains available as a visual example, but its contents do not define this ZIP. Use the labeled preview to identify files and the exact member grid to follow individual source pages. The moss and vines offer green contrast against the dark floor; add them sparingly if you need traps or pickups to remain prominent.

Each member record declares CC0. Under CC0, attribution is not required and commercial use and modification are allowed; retaining the included notice helps document where these files came from. This summarizes the catalog’s declaration, not an independent rights audit or a warranty about trademarks, publicity rights, or third-party material. README.md and LICENSE.txt accompany the image files.

Godot 4: import PNGs as textures and assign them to Sprite2D nodes. Choose Nearest filtering on CanvasItem for pixel art. Configure origins and placement for each file.

Unity: import each PNG as Sprite (2D and UI), with Sprite Mode Single. Use Point filtering for pixel art. Choose pixels per unit for your scene; the preview is not a sheet to slice.

GameMaker: import each unrelated image into its own sprite and set origins consistently. Disable texture interpolation for crisp pixel art. Multiple images in one sprite become subimages; this collection supplies no animation sequence.

Revision: 2026-10-12

Inventory and checksums: manifest.json. PNGs are native size; preview.webp is a labeled index, not a tilesheet.

1. dungeon-stone-floor-32: 32x32, static
2. dungeon-cracked-floor-32: 32x32, static
3. dungeon-cobble-floor-32: 32x32, static
4. dungeon-lava-tile-32: 32x32, static
5. dungeon-water-pool-32: 32x32, static
6. dungeon-stone-wall-32: 32x32, static
7. dungeon-entrance: 32x32, static
8. dungeon-iron-door: 24x40, static
9. dungeon-arch-passage: 40x48, static
10. dungeon-staircase: 32x32, static
11. dungeon-pillar: 16x48, static
12. dungeon-iron-cage-lg: 28x36, static
13. dungeon-iron-cage-sm: 20x28, static
14. dungeon-wall-torch: 16x28, static
15. dungeon-barrel: 16x20, static
16. dungeon-treasure-chest: 24x20, static
17. dungeon-skull-pile: 24x16, static
18. dungeon-spike-trap: 28x16, static
19. dungeon-hanging-chain: 8x32, static
20. dungeon-magic-crystal: 14x20, static
21. dungeon-potion-shelf: 28x32, static
22. dungeon-weapon-rack: 24x32, static
23. dungeon-cauldron: 20x20, static
24. dungeon-bookshelf: 24x36, static
25. dungeon-glow-mushroom: 16x16, static
26. dungeon-moss-patch: 20x10, static
27. dungeon-hanging-vine: 10x28, static
28. dungeon-spider-web: 20x20, static
29. dungeon-rat: 16x12, static
30. dungeon-bat: 20x12, static
