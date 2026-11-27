# NEMEZIDA visual pass: Creator Store asset selection

The current greybox map is intentionally kept as a safe fallback. The visual direction is now **bright, stylized dark fantasy**: readable daylight in the hub, warm stone and timber, green woodland edges, and a darker dungeon interior.

## Selected Roblox Creator Store packs

1. **Synty Dungeon Pack: Cave & Castle Interiors** — Roblox-published, 97% positive with 700+ votes. Modular castle/cave interiors, rocks, runes, bridges, and tunnels. Asset ID: `6934021345`.
   - https://create.roblox.com/store/asset/6934021345/Synty-Dungeon-Pack-Cave-Castle-Interiors
2. **Forest Pack** — Roblox-published, 97% positive with 2K+ votes. Trees, bushes, flowers, boulders, logs, ground details, and animated wildlife. Asset ID: `6432306802`.
   - https://create.roblox.com/store/asset/6432306802/Forest-Pack

These two are the core style-matched packs: the Synty dungeon set provides the modular stone architecture, while Forest Pack gives the overworld natural detail. They are selected from the Roblox Creator Store, not arbitrary external downloads.

## Import in Roblox Studio

1. Open **View → Toolbox → Creator Store**.
2. Search each exact pack name or open the links above while signed into the account that owns the experience.
3. Click **Get Model**, then insert the model from Toolbox into Workspace.
4. Move the Forest Pack models into `Workspace/NemezidaAssets/Forest` and the dungeon pack models into `Workspace/NemezidaAssets/Dungeon`.
5. Inspect the Forest Pack's scripts before publishing. The Creator Store listing says it contains scripts; disable/remove any scripts not needed by the game. Keep the asset geometry and textures.
6. Reposition/duplicate the modular pieces to replace the temporary block buildings and dungeon walls. Do not delete the generated spawn, enemy markers, or portal interaction parts.

**Important:** public third-party models cannot safely be fetched automatically by a game script unless the experience enables third-party asset loading. The project deliberately does not download arbitrary models at runtime. Insert the chosen packs through Studio's Toolbox so their assets become part of the place and their dependencies/permissions are handled by Studio.

## Changes in this pass

- Hub lighting changed from midnight to clear afternoon.
- Reduced atmospheric haze and removed the heavy purple-grey color filter.
- Added subtle bloom for magic lights only.
- Recolored the ground to grass, roads to warm cobblestone, and buildings to lighter stone/timber.
- Added lightweight native Roblox tree canopies around the outskirts as a temporary fallback.
- Kept the dungeon's geometry and portals intact; it remains moodier than the overworld.

## References

- Creator Store overview and Toolbox import workflow: https://create.roblox.com/docs/production/creator-store
- AssetService third-party asset permissions: https://create.roblox.com/docs/reference/engine/classes/AssetService
