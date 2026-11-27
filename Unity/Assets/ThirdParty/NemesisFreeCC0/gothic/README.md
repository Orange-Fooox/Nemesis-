# Free Gothic Pixel Art: Cathedral Tiles, Props and Figures

This pixel set centers on stone cathedral architecture, ceremonial details, and small figures. It contains five members in each of six categories, with 32×32 textures and mixed-size objects. The selection avoids combining these modest pixel sprites with the separate large gothic illustration medium found elsewhere in the catalog.

This pack contains 30 assets: 5 tilesets, 5 buildings, 5 environment, 5 items, 5 characters, 5 enemies. There are 17 distinct native canvas sizes. The complete size histogram appears below and in manifest.json. All downloads in the archive are PNG; the WebP preview is a labeled visual index, not an asset atlas.

Stone floor, checkered marble, nave carpet, crypt stone, and a stained-glass pattern are the texture choices. Cathedral facade, bell tower, flying buttress, crypt entrance, and stone altar provide the architectural pieces. The pillar, rose window, gargoyle statue, candelabra, and tomb are environmental details. Chalice, sacred book, rosary, key, and votive candles serve as small props. Bishop, choir boy, templar, monk, and keeper are character sprites; awakened gargoyle, specter, fallen angel, bone revenant, and demon bat are enemy sprites.

Use stone or marble beneath the architecture, then place the carpet as a contrasting patch for an aisle or focal area. Keep the altar and candelabra visible against the darker crypt texture. The rose window and stained-glass pattern are distinct images: one is a circular object, the other a square texture, so they need separate placement treatment. The tower and buttress can extend a facade visually, but no assembly rules guarantee that every boundary meets. Build a small composition first and adjust the overlap of stonework manually.

The facade is 56×64, the bell tower 24×56, and the pillar 12×48. Characters use 24×24 bounds, while most enemies are 24×28 and the bat is 20×16. These sizes let you distinguish large architecture from small figures, but they do not define a universal actor-to-building scale. Choose an object baseline and draw order that fits your camera. The red carpet and colored window provide accents within otherwise muted stone; they are not a single shared palette applied to every image.

No member has exported animation. The awakened gargoyle is a separate static design, not a verified transformation frame for the statue. The revenant and specter do not contain attack, idle, or movement cycles. The collection also lacks complete interiors, directional actor views, collision data, and autotile rules. The old demo remains accessible as a composition reference. Check edge repetition and readability on both light and dark backgrounds before committing these pieces to a large cathedral map.

Each member record declares CC0. Under CC0, attribution is not required and commercial use and modification are allowed; retaining the included notice helps document where these files came from. This summarizes the catalog’s declaration, not an independent rights audit or a warranty about trademarks, publicity rights, or third-party material. README.md and LICENSE.txt accompany the image files.

Godot 4: import PNGs as textures and assign them to Sprite2D nodes. Choose Nearest filtering on CanvasItem for pixel art. Configure origins and placement for each file.

Unity: import each PNG as Sprite (2D and UI), with Sprite Mode Single. Use Point filtering for pixel art. Choose pixels per unit for your scene; the preview is not a sheet to slice.

GameMaker: import each unrelated image into its own sprite and set origins consistently. Disable texture interpolation for crisp pixel art. Multiple images in one sprite become subimages; this collection supplies no animation sequence.

Revision: 2026-10-12

Inventory and checksums: manifest.json. PNGs are native size; preview.webp is a labeled index, not a tilesheet.

1. gothic-stone-floor: 32x32, static
2. gothic-checkered-marble: 32x32, static
3. gothic-nave-carpet: 32x32, static
4. gothic-crypt-stone: 32x32, static
5. gothic-stained-glass-tile: 32x32, static
6. gothic-cathedral-facade: 56x64, static
7. gothic-bell-tower: 24x56, static
8. gothic-flying-buttress: 32x40, static
9. gothic-crypt-entrance: 36x36, static
10. gothic-stone-altar: 32x28, static
11. gothic-pillar: 12x48, static
12. gothic-rose-window: 28x28, static
13. gothic-gargoyle: 20x24, static
14. gothic-iron-candelabra: 14x28, static
15. gothic-stone-tomb: 24x16, static
16. gothic-holy-chalice: 10x14, static
17. gothic-sacred-book: 14x10, static
18. gothic-rosary: 10x14, static
19. gothic-silver-key: 8x14, static
20. gothic-votive-candles: 14x10, static
21. gothic-bishop: 24x24, static
22. gothic-choir-boy: 24x24, static
23. gothic-knight-templar: 24x24, static
24. gothic-hooded-monk: 24x24, static
25. gothic-cathedral-keeper: 24x24, static
26. gothic-gargoyle-awakened: 24x28, static
27. gothic-shadow-specter: 24x28, static
28. gothic-fallen-angel: 24x28, static
29. gothic-bone-revenant: 24x28, static
30. gothic-demon-bat: 20x16, static
