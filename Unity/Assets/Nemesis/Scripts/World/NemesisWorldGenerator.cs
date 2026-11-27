using System;
using System.Collections.Generic;
using Nemesis.Content;
using Nemesis.Interaction;
using UnityEngine;

namespace Nemesis.World
{
    public sealed class NemesisWorldLayout
    {
        public Vector3 PlayerSpawn;
        public Vector3 MerchantSpawn;
        public Vector3 BossSpawn;
        public Vector3[] EnemySpawns;
    }

    /// <summary>
    /// Deterministic first-pass generator for the Ashen Reach hub.
    /// It uses imported, script-free catalog prefabs when available and safe primitive fallbacks otherwise.
    /// </summary>
    public static class NemesisWorldGenerator
    {
        private const float WorldHalfSize = 42f;
        private static readonly Color GroundColor = new Color(.19f, .20f, .18f);
        private static readonly Color RoadColor = new Color(.31f, .29f, .27f);
        private static readonly Color StoneColor = new Color(.28f, .27f, .29f);
        private static readonly Color DarkStoneColor = new Color(.16f, .16f, .19f);
        private static readonly Color WoodColor = new Color(.27f, .16f, .10f);
        private static readonly Color MagicColor = new Color(.34f, .12f, .62f);

        private sealed class Builder
        {
            private readonly Transform root;
            private readonly NemesisAssetCatalog catalog;
            private readonly System.Random random;
            private readonly Dictionary<int, Material> materials = new Dictionary<int, Material>();

            public Builder(Transform parent, NemesisAssetCatalog source, int seed)
            {
                catalog = source;
                random = new System.Random(seed);
                GameObject world = new GameObject("World_AshenReach");
                root = world.transform;
                root.SetParent(parent, false);
            }

            public NemesisWorldLayout Build()
            {
                BuildGroundAndRoads();
                BuildCentralPlaza();
                BuildInteractables();
                BuildDistrictBuildings();
                BuildLandmarks();
                BuildOutskirts();
                return new NemesisWorldLayout
                {
                    PlayerSpawn = new Vector3(0f, 0f, -6f),
                    MerchantSpawn = new Vector3(-7.5f, 0f, -7.5f),
                    BossSpawn = new Vector3(0f, 0f, 35f),
                    EnemySpawns = new[]
                    {
                        new Vector3(-18f, 0f, 17f),
                        new Vector3(18f, 0f, 17f),
                        new Vector3(-18f, 0f, -19f),
                        new Vector3(18f, 0f, -19f)
                    }
                };
            }

            private void BuildGroundAndRoads()
            {
                Box("AshenReach_Ground", new Vector3(0f, -.45f, 0f),
                    new Vector3(WorldHalfSize * 2f, .9f, WorldHalfSize * 2f), GroundColor);

                VisualBox("NorthSouth_MainRoad", new Vector3(0f, .025f, 0f),
                    new Vector3(5.2f, .12f, WorldHalfSize * 2f - 2f), RoadColor);
                VisualBox("EastWest_MainRoad", new Vector3(0f, .03f, 0f),
                    new Vector3(WorldHalfSize * 2f - 2f, .13f, 5.2f), RoadColor);

                for (int i = -1; i <= 1; i += 2)
                {
                    float offset = i * 15f;
                    VisualBox("DistrictNorthSouthRoad_" + i, new Vector3(offset, .045f, 0f),
                        new Vector3(2.3f, .12f, WorldHalfSize * 2f - 6f), RoadColor);
                    VisualBox("DistrictEastWestRoad_" + i, new Vector3(0f, .045f, offset),
                        new Vector3(WorldHalfSize * 2f - 6f, .12f, 2.3f), RoadColor);
                }

                VisualBox("CentralSquare_Stone", new Vector3(0f, .11f, 0f),
                    new Vector3(21f, .18f, 21f), new Color(.37f, .34f, .32f));

                for (int i = 0; i < 4; i++)
                {
                    float angle = i * 90f;
                    Vector3 position = Quaternion.Euler(0f, angle, 0f) * new Vector3(8.8f, .24f, 0f);
                    VisualBox("Square_Rune_Inlay_" + i, position,
                        new Vector3(1.25f, .055f, .34f), new Color(.42f, .29f, .52f),
                        Quaternion.Euler(0f, angle, 0f));
                }

                BuildPerimeter();
            }

            private void BuildPerimeter()
            {
                const float wallHeight = 3.8f;
                const float thickness = .8f;
                float edge = WorldHalfSize - .6f;

                Box("Perimeter_West", new Vector3(-edge, wallHeight * .5f, 0f),
                    new Vector3(thickness, wallHeight, WorldHalfSize * 2f), DarkStoneColor);
                Box("Perimeter_East", new Vector3(edge, wallHeight * .5f, 0f),
                    new Vector3(thickness, wallHeight, WorldHalfSize * 2f), DarkStoneColor);
                Box("Perimeter_South_Left", new Vector3(-22f, wallHeight * .5f, -edge),
                    new Vector3(40f, wallHeight, thickness), DarkStoneColor);
                Box("Perimeter_South_Right", new Vector3(22f, wallHeight * .5f, -edge),
                    new Vector3(40f, wallHeight, thickness), DarkStoneColor);
                Box("Perimeter_North_Left", new Vector3(-22f, wallHeight * .5f, edge),
                    new Vector3(40f, wallHeight, thickness), DarkStoneColor);
                Box("Perimeter_North_Right", new Vector3(22f, wallHeight * .5f, edge),
                    new Vector3(40f, wallHeight, thickness), DarkStoneColor);
            }

            private void BuildCentralPlaza()
            {
                SpawnProp("altar", "SealMonument", new Vector3(0f, .2f, 0f),
                    new Vector3(2.4f, 2.8f, 2.4f), PrimitiveType.Cube, new Color(.31f, .22f, .32f));

                SpawnProp("pillar", "PlazaPillar_NW", new Vector3(-8.2f, 0f, 8.2f),
                    new Vector3(1f, 3.5f, 1f), PrimitiveType.Cube, StoneColor);
                SpawnProp("pillar", "PlazaPillar_NE", new Vector3(8.2f, 0f, 8.2f),
                    new Vector3(1f, 3.5f, 1f), PrimitiveType.Cube, StoneColor);
                SpawnProp("pillar", "PlazaPillar_SW", new Vector3(-8.2f, 0f, -8.2f),
                    new Vector3(1f, 3.5f, 1f), PrimitiveType.Cube, StoneColor);
                SpawnProp("pillar", "PlazaPillar_SE", new Vector3(8.2f, 0f, -8.2f),
                    new Vector3(1f, 3.5f, 1f), PrimitiveType.Cube, StoneColor);

                Box("Plaza_Well_Base", new Vector3(5.6f, .42f, -5.4f),
                    new Vector3(2.2f, .8f, 2.2f), StoneColor);
                Cylinder("Plaza_Well_Rim", new Vector3(5.6f, 1.32f, -5.4f),
                    new Vector3(1.9f, .3f, 1.9f), new Color(.43f, .40f, .36f));

                Box("Market_Stall_Counter", new Vector3(-5.4f, .65f, -5.2f),
                    new Vector3(3.1f, 1.3f, 1.1f), WoodColor);
                Box("Market_Stall_Canopy", new Vector3(-5.4f, 1.75f, -5.2f),
                    new Vector3(3.5f, .22f, 1.55f), new Color(.42f, .16f, .15f));

                Torch("PlazaTorch_West", new Vector3(-9.7f, 0f, 0f));
                Torch("PlazaTorch_East", new Vector3(9.7f, 0f, 0f));
            }

            private void BuildInteractables()
            {
                Vector3 chestPosition = new Vector3(-10f, 0f, 10f);
                SpawnProp("chest", "AshenSupplyChest_Visual", chestPosition,
                    new Vector3(1.25f, .9f, 1.1f), PrimitiveType.Cube, new Color(.39f, .25f, .13f));

                GameObject chest = new GameObject("Interactable_AshenCache");
                chest.transform.SetParent(root, false);
                chest.transform.localPosition = chestPosition;
                chest.AddComponent<NemesisWorldInteractable>().Configure(
                    "Пепельный тайник",
                    "Открыть",
                    "Под слоем золы лежат кристаллы Эссенции.",
                    25,
                    true);
            }

            private void BuildDistrictBuildings()
            {
                Vector2[] anchors =
                {
                    new Vector2(-36.5f, -31f), new Vector2(-36.5f, -21f), new Vector2(-23f, -36f),
                    new Vector2(36.5f, -31f), new Vector2(36.5f, -21f), new Vector2(23f, -36f),
                    new Vector2(-36.5f, 31f), new Vector2(-36.5f, 21f), new Vector2(-23f, 36f),
                    new Vector2(36.5f, 31f), new Vector2(36.5f, 21f), new Vector2(23f, 36f)
                };

                string[] districtNames =
                {
                    "AshMarket", "AshMarket", "AshMarket",
                    "OldQuarter", "OldQuarter", "OldQuarter",
                    "Sanctuary", "Sanctuary", "Sanctuary",
                    "OuterWatch", "OuterWatch", "OuterWatch"
                };

                for (int i = 0; i < anchors.Length; i++)
                {
                    float width = Range(6.2f, 8.8f);
                    float depth = Range(6f, 8.2f);
                    float height = Range(4.2f, 6.4f);
                    Vector3 position = new Vector3(anchors[i].x, 0f, anchors[i].y);
                    Color wallColor = DistrictColor(i / 3);
                    Color roofColor = Color.Lerp(DarkStoneColor, wallColor, .35f);
                    CreateBuilding(districtNames[i] + "_House_" + (i % 3 + 1),
                        position, width, depth, height, wallColor, roofColor);
                }
            }

            private void CreateBuilding(string name, Vector3 ground, float width, float depth,
                float height, Color wallColor, Color roofColor)
            {
                Box(name + "_Body", ground + Vector3.up * (height * .5f),
                    new Vector3(width, height, depth), wallColor);

                Box(name + "_Roof", ground + Vector3.up * (height + .3f),
                    new Vector3(width + .8f, .65f, depth + .8f), roofColor);

                Box(name + "_Door", ground + new Vector3(0f, 1.12f, -depth * .5f - .09f),
                    new Vector3(1.25f, 2.25f, .14f), WoodColor);
                Box(name + "_DoorFrame", ground + new Vector3(0f, 2.3f, -depth * .5f - .12f),
                    new Vector3(1.65f, .22f, .20f), new Color(.46f, .34f, .20f));

                for (int side = -1; side <= 1; side += 2)
                {
                    Box(name + "_Window_" + side,
                        ground + new Vector3(side * width * .31f, height * .55f, -depth * .5f - .085f),
                        new Vector3(.85f, .9f, .12f), new Color(.23f, .32f, .39f));
                    Box(name + "_WindowFrame_" + side,
                        ground + new Vector3(side * width * .31f, height * .55f, -depth * .5f - .16f),
                        new Vector3(1.02f, 1.06f, .08f), new Color(.48f, .36f, .22f));
                }

                Box(name + "_Step", ground + new Vector3(0f, .14f, -depth * .5f - .55f),
                    new Vector3(2.05f, .28f, .85f), StoneColor);
            }

            private void BuildLandmarks()
            {
                BuildChapel(new Vector3(-27f, 0f, 27f));
                BuildForge(new Vector3(27f, 0f, 27f));
                BuildWatchtower(new Vector3(-27f, 0f, -27f));
                BuildWishingWell(new Vector3(27f, 0f, -27f));
                BuildSoulGate(new Vector3(0f, 0f, 35.5f));
            }

            private void BuildChapel(Vector3 p)
            {
                VisualBox("BrokenChapel_Floor", p + new Vector3(0f, .12f, 0f),
                    new Vector3(10f, .24f, 8f), StoneColor);
                Box("BrokenChapel_LeftWall", p + new Vector3(-4.2f, 2.1f, 0f),
                    new Vector3(.65f, 4.2f, 7.8f), DarkStoneColor);
                Box("BrokenChapel_RightWall", p + new Vector3(4.2f, 1.5f, 0f),
                    new Vector3(.65f, 3f, 7.8f), DarkStoneColor);
                Box("BrokenChapel_Arch", p + new Vector3(0f, 4.3f, -3.5f),
                    new Vector3(8.5f, .55f, .65f), StoneColor);
                SpawnProp("altar", "BrokenChapel_Altar", p + new Vector3(0f, .35f, 1.5f),
                    new Vector3(2f, 1.5f, 1.2f), PrimitiveType.Cube, new Color(.26f, .21f, .29f));
            }

            private void BuildForge(Vector3 p)
            {
                Box("Forge_Shop", p + new Vector3(0f, 1.8f, 0f),
                    new Vector3(8f, 3.6f, 7f), new Color(.30f, .22f, .17f));
                Box("Forge_Anvil", p + new Vector3(-1.8f, .55f, -4.1f),
                    new Vector3(1.4f, 1.1f, .9f), new Color(.37f, .38f, .40f));
                Box("Forge_Furnace", p + new Vector3(2f, 1.4f, -3.8f),
                    new Vector3(2.1f, 2.8f, 1.5f), DarkStoneColor);
                Sphere("Forge_Fire", p + new Vector3(2f, 2.05f, -3.4f),
                    new Vector3(.75f, .85f, .5f), new Color(1f, .32f, .07f));
                AddPointLight(p + new Vector3(2f, 2.25f, -2.8f), new Color(1f, .34f, .10f), 3.5f, 8f);
            }

            private void BuildWatchtower(Vector3 p)
            {
                Box("Watchtower_Base", p + new Vector3(0f, 2.6f, 0f),
                    new Vector3(4.2f, 5.2f, 4.2f), DarkStoneColor);
                Box("Watchtower_Deck", p + new Vector3(0f, 5.3f, 0f),
                    new Vector3(6f, .45f, 6f), StoneColor);
                for (int x = -1; x <= 1; x += 2)
                    for (int z = -1; z <= 1; z += 2)
                        Box("Watchtower_Post_" + x + "_" + z,
                            p + new Vector3(x * 2.6f, 7.2f, z * 2.6f),
                            new Vector3(.35f, 4f, .35f), WoodColor);
                Box("Watchtower_Roof", p + new Vector3(0f, 9.3f, 0f),
                    new Vector3(6.4f, .5f, 6.4f), new Color(.19f, .16f, .17f));
            }

            private void BuildWishingWell(Vector3 p)
            {
                Cylinder("WishingWell_Stone", p + new Vector3(0f, .65f, 0f),
                    new Vector3(2.7f, 1.3f, 2.7f), StoneColor);
                Cylinder("WishingWell_Water", p + new Vector3(0f, 1.32f, 0f),
                    new Vector3(1.8f, .08f, 1.8f), new Color(.10f, .23f, .32f));
                Box("WishingWell_Beam", p + new Vector3(0f, 2.65f, 0f),
                    new Vector3(.22f, 3f, .22f), WoodColor);
                Box("WishingWell_Roof", p + new Vector3(0f, 4.1f, 0f),
                    new Vector3(3.1f, .35f, 2.5f), new Color(.20f, .16f, .16f));
            }

            private void BuildSoulGate(Vector3 p)
            {
                SpawnProp("pillar", "SoulGate_LeftPillar", p + new Vector3(-3.1f, 0f, 0f),
                    new Vector3(1.6f, 8f, 1.6f), PrimitiveType.Cube, StoneColor);
                SpawnProp("pillar", "SoulGate_RightPillar", p + new Vector3(3.1f, 0f, 0f),
                    new Vector3(1.6f, 8f, 1.6f), PrimitiveType.Cube, StoneColor);
                Box("SoulGate_Lintel", p + new Vector3(0f, 8f, 0f),
                    new Vector3(7.7f, .9f, 1.6f), DarkStoneColor);
                VisualBox("SoulGate_RuneFrame", p + new Vector3(0f, 3.9f, -.45f),
                    new Vector3(3.5f, 6.3f, .16f), MagicColor);
                AddPointLight(p + new Vector3(0f, 4f, -2.2f), new Color(.48f, .16f, .9f), 5f, 12f);
                VisualBox("SoulGate_Approach", p + new Vector3(0f, .1f, -4.2f),
                    new Vector3(10f, .2f, 8f), RoadColor);
            }

            private void BuildOutskirts()
            {
                Vector2[] treeAnchors =
                {
                    new Vector2(-10f, -32f), new Vector2(10f, -32f),
                    new Vector2(-10f, 32f), new Vector2(10f, 32f),
                    new Vector2(-32f, -10f), new Vector2(32f, -10f),
                    new Vector2(-32f, 10f), new Vector2(32f, 10f)
                };

                for (int i = 0; i < treeAnchors.Length; i++)
                {
                    Vector3 p = new Vector3(
                        treeAnchors[i].x + Range(-.7f, .7f),
                        0f,
                        treeAnchors[i].y + Range(-.7f, .7f));
                    SpawnProp("tree", "AshenTree_" + i, p,
                        new Vector3(2.6f, 4.3f, 2.6f), PrimitiveType.Sphere,
                        new Color(.16f, .22f, .16f));
                }

                Vector2[] ruinAnchors =
                {
                    new Vector2(-11f, -26f), new Vector2(11f, -26f),
                    new Vector2(-11f, 26f), new Vector2(11f, 26f),
                    new Vector2(-27f, -10f), new Vector2(27f, -10f),
                    new Vector2(-27f, 10f), new Vector2(27f, 10f)
                };

                for (int i = 0; i < ruinAnchors.Length; i++)
                {
                    float x = ruinAnchors[i].x + Range(-1.3f, 1.3f);
                    float z = ruinAnchors[i].y + Range(-1.3f, 1.3f);
                    SpawnProp("pillar", "OuterRuin_" + i, new Vector3(x, 0f, z),
                        new Vector3(.9f, Range(1.3f, 2.8f), .9f), PrimitiveType.Cube, DarkStoneColor);
                }
            }

            private void Torch(string name, Vector3 p)
            {
                SpawnProp("torch", name, p + Vector3.up * .9f,
                    new Vector3(.32f, 1.8f, .32f), PrimitiveType.Cylinder, new Color(.27f, .22f, .18f));
                Sphere(name + "_Flame", p + Vector3.up * 2f,
                    new Vector3(.26f, .42f, .26f), new Color(.25f, .55f, .9f));
                AddPointLight(p + Vector3.up * 2.1f, new Color(.22f, .48f, .95f), 2.1f, 6f);
            }

            private void SpawnProp(string category, string name, Vector3 ground, Vector3 fallbackScale,
                PrimitiveType fallbackType, Color fallbackColor)
            {
                GameObject prefab = catalog != null ? catalog.FindPrefab(category) : null;
                if (prefab != null && (prefab.GetComponentInChildren<Renderer>(true) == null ||
                    prefab.GetComponentInChildren<MonoBehaviour>(true) != null))
                    prefab = null;

                GameObject instance;
                if (prefab != null)
                {
                    instance = UnityEngine.Object.Instantiate(prefab, root, false);
                    instance.name = name;
                    instance.transform.position = root.TransformPoint(ground);
                    FitToBounds(instance, fallbackScale, ground);
                    DisableColliders(instance);
                }
                else if (string.Equals(category, "tree", StringComparison.OrdinalIgnoreCase))
                {
                    CreatePrimitive(name + "_Trunk", PrimitiveType.Cylinder,
                        ground + Vector3.up * 1.25f, new Vector3(.55f, 1.25f, .55f),
                        new Color(.22f, .15f, .11f));
                    instance = CreatePrimitive(name + "_Canopy", PrimitiveType.Sphere,
                        ground + Vector3.up * 3.1f, new Vector3(2.6f, 2.1f, 2.6f), fallbackColor);
                }
                else
                {
                    Vector3 primitiveScale = fallbackScale;
                    if (fallbackType == PrimitiveType.Cylinder)
                        primitiveScale.y *= .5f;
                    instance = CreatePrimitive(name, fallbackType, ground + Vector3.up * (fallbackScale.y * .5f),
                        primitiveScale, fallbackColor);
                }

                instance.transform.SetParent(root, true);
            }

            private void FitToBounds(GameObject instance, Vector3 targetSize, Vector3 ground)
            {
                Renderer[] renderers = instance.GetComponentsInChildren<Renderer>(true);
                if (renderers.Length == 0) return;
                Bounds bounds = renderers[0].bounds;
                for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
                float largest = Mathf.Max(bounds.size.x, Mathf.Max(bounds.size.y, bounds.size.z));
                float desired = Mathf.Max(targetSize.x, Mathf.Max(targetSize.y, targetSize.z));
                instance.transform.localScale *= desired / Mathf.Max(.001f, largest);

                bounds = renderers[0].bounds;
                for (int i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
                Vector3 desiredCenter = root.TransformPoint(ground + Vector3.up * bounds.extents.y);
                instance.transform.position += desiredCenter - bounds.center;
            }

            private void DisableColliders(GameObject instance)
            {
                foreach (Collider collider in instance.GetComponentsInChildren<Collider>(true))
                    collider.enabled = false;
            }

            private void Box(string name, Vector3 position, Vector3 scale, Color color, Quaternion rotation = default)
            {
                GameObject obj = CreatePrimitive(name, PrimitiveType.Cube, position, scale, color);
                obj.transform.localRotation = rotation;
            }

            private void VisualBox(string name, Vector3 position, Vector3 size, Color color)
            {
                GameObject obj = CreatePrimitive(name, PrimitiveType.Cube, position, size, color);
                Collider collider = obj.GetComponent<Collider>();
                if (collider != null) UnityEngine.Object.Destroy(collider);
            }

            private void Cylinder(string name, Vector3 position, Vector3 size, Color color)
            {
                // Unity's built-in cylinder is two units tall, while box dimensions are one unit per scale.
                size.y *= .5f;
                CreatePrimitive(name, PrimitiveType.Cylinder, position, size, color);
            }

            private void Sphere(string name, Vector3 position, Vector3 scale, Color color)
            {
                GameObject obj = CreatePrimitive(name, PrimitiveType.Sphere, position, scale, color);
                Collider collider = obj.GetComponent<Collider>();
                if (collider != null) UnityEngine.Object.Destroy(collider);
            }

            private GameObject CreatePrimitive(string name, PrimitiveType type, Vector3 position,
                Vector3 scale, Color color)
            {
                GameObject obj = GameObject.CreatePrimitive(type);
                obj.name = name;
                obj.transform.SetParent(root, false);
                obj.transform.localPosition = position;
                obj.transform.localScale = scale;
                Renderer renderer = obj.GetComponent<Renderer>();
                if (renderer != null) renderer.sharedMaterial = GetMaterial(color);
                return obj;
            }

            private Material GetMaterial(Color color)
            {
                int key = color.GetHashCode();
                if (materials.TryGetValue(key, out Material existing) && existing != null)
                    return existing;

                Shader shader = Shader.Find("Universal Render Pipeline/Lit");
                if (shader == null) shader = Shader.Find("Standard");
                if (shader == null) shader = Shader.Find("Unlit/Color");
                Material material = new Material(shader) { color = color };
                materials[key] = material;
                return material;
            }

            private void AddPointLight(Vector3 position, Color color, float intensity, float range)
            {
                GameObject lightObject = new GameObject("PointLight_" + root.childCount);
                lightObject.transform.SetParent(root, false);
                lightObject.transform.localPosition = position;
                Light light = lightObject.AddComponent<Light>();
                light.type = LightType.Point;
                light.color = color;
                light.intensity = intensity;
                light.range = range;
            }

            private float Range(float min, float max)
            {
                return (float)(min + random.NextDouble() * (max - min));
            }

            private Color DistrictColor(int district)
            {
                switch (district)
                {
                    case 0: return new Color(.39f, .27f, .20f);
                    case 1: return new Color(.31f, .28f, .25f);
                    case 2: return new Color(.27f, .28f, .30f);
                    default: return new Color(.25f, .22f, .21f);
                }
            }
        }

        public static NemesisWorldLayout Generate(Transform parent, NemesisAssetCatalog catalog, int seed)
        {
            return new Builder(parent, catalog, seed).Build();
        }
    }
}
