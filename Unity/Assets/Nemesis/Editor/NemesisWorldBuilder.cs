using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using System.IO;

public static class NemesisWorldBuilder
{
    private const string Root = "Assets/Nemesis";
    private const string Forest = "Assets/ThirdParty/NemesisFreeCC0/forest/assets/";
    private const string Gothic = "Assets/ThirdParty/NemesisFreeCC0/gothic/assets/";
    private const string Dungeon = "Assets/ThirdParty/NemesisFreeCC0/dungeon/assets/";
    private const float CellWidth = 0.72f;
    private const float CellHeight = 0.36f;

    private static readonly System.Random Rng = new System.Random(27041);
    private static readonly HashSet<Vector2Int> PathCells = new HashSet<Vector2Int>();
    private static readonly HashSet<Vector2Int> Occupied = new HashSet<Vector2Int>();
    private static Transform worldRoot;

    private sealed class TileBatch
    {
        public readonly List<Vector3> vertices = new List<Vector3>();
        public readonly List<Vector2> uvs = new List<Vector2>();
        public readonly List<int> triangles = new List<int>();
    }

    private static Vector3 Iso(int x, int y)
    {
        return new Vector3((x - y) * CellWidth, (x + y) * CellHeight, 0f);
    }

    private static string F(string folder, string file)
    {
        return folder + file;
    }

    private static Sprite LoadSprite(string path, bool bottomPivot = false)
    {
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null)
        {
            Debug.LogError("NEMESIS ASSET MISSING: " + path);
            return null;
        }

        bool changed = false;
        if (importer.textureType != TextureImporterType.Sprite) { importer.textureType = TextureImporterType.Sprite; changed = true; }
        if (importer.spriteImportMode != SpriteImportMode.Single) { importer.spriteImportMode = SpriteImportMode.Single; changed = true; }
        if (importer.filterMode != FilterMode.Point) { importer.filterMode = FilterMode.Point; changed = true; }
        if (importer.spritePixelsPerUnit != 32) { importer.spritePixelsPerUnit = 32; changed = true; }
        if (importer.textureCompression != TextureImporterCompression.Uncompressed) { importer.textureCompression = TextureImporterCompression.Uncompressed; changed = true; }
        if (importer.mipmapEnabled) { importer.mipmapEnabled = false; changed = true; }
        if (importer.alphaIsTransparency == false) { importer.alphaIsTransparency = true; changed = true; }
        TextureImporterSettings settings = new TextureImporterSettings();
        importer.ReadTextureSettings(settings);
        int alignment = bottomPivot ? (int)SpriteAlignment.Custom : (int)SpriteAlignment.Center;
        Vector2 pivot = bottomPivot ? new Vector2(0.5f, 0.08f) : new Vector2(0.5f, 0.5f);
        if (settings.spriteAlignment != alignment) { settings.spriteAlignment = alignment; changed = true; }
        if (bottomPivot && settings.spritePivot != pivot) { settings.spritePivot = pivot; changed = true; }
        if (changed) importer.SetTextureSettings(settings);
        if (changed) importer.SaveAndReimport();
        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (sprite == null) Debug.LogError("NEMESIS SPRITE IMPORT FAILED: " + path);
        return sprite;
    }

    private static GameObject AddSprite(string name, Sprite sprite, Vector3 position, Vector3 scale, int orderBias = 0, bool depthSort = true)
    {
        if (sprite == null) return null;
        GameObject go = new GameObject(name);
        go.transform.SetParent(worldRoot, false);
        go.transform.localPosition = position;
        go.transform.localScale = scale;

        SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.spriteSortPoint = SpriteSortPoint.Pivot;

        renderer.sortingOrder = 100 - Mathf.RoundToInt(position.y * 100f) + orderBias;

        if (depthSort)
        {
            NemesisDepthSort sort = go.AddComponent<NemesisDepthSort>();
            sort.orderOffset = orderBias;
        }
        return go;
    }

    private static GameObject AddProp(string name, Sprite sprite, Vector2Int cell, Vector3 scale, bool blocking, int bias = 0)
    {
        Vector3 p = Iso(cell.x, cell.y);
        GameObject go = AddSprite(name, sprite, p, scale, bias, true);
        if (go == null) return null;

        if (blocking)
        {
            BoxCollider2D collider = go.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(0.42f, 0.26f);
            collider.offset = new Vector2(0f, 0.13f);
        }
        Occupied.Add(cell);
        return go;
    }

    [MenuItem("Nemesis/Build Ashen Outskirts Scene")]
    public static void Build()
    {
        if (!Directory.Exists(Root + "/Scenes")) Directory.CreateDirectory(Root + "/Scenes");
        if (!Directory.Exists("Assets/Resources/NemesisUI")) Directory.CreateDirectory("Assets/Resources/NemesisUI");

        NemesisUIAssetInstaller.Install();
        NemesisInfiniteWorldInstaller.Install();
        NemesisCharacterAssetInstaller.Install();

        PathCells.Clear();
        Occupied.Clear();
        Rng.Next();

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        Camera camera = new GameObject("Main Camera").AddComponent<Camera>();
        camera.tag = "MainCamera";
        camera.orthographic = true;
        camera.orthographicSize = 8.4f;
        camera.backgroundColor = new Color(0.018f, 0.025f, 0.029f);
        camera.transform.position = new Vector3(0f, 0f, -10f);
        camera.clearFlags = CameraClearFlags.SolidColor;

        worldRoot = new GameObject("Ashen Outskirts - World").transform;
        worldRoot.position = Vector3.zero;

        Sprite grass = LoadSprite(F(Dungeon, "tilesets/dungeon-cobble-floor-32.png"));
        Sprite leaves = LoadSprite(F(Dungeon, "tilesets/dungeon-cracked-floor-32.png"));
        Sprite mud = LoadSprite(F(Dungeon, "tilesets/dungeon-stone-floor-32.png"));
        Sprite path = LoadSprite(F(Dungeon, "tilesets/dungeon-cobble-floor-32.png"));
        Sprite stream = LoadSprite(F(Dungeon, "tilesets/dungeon-lava-tile-32.png"));

        BuildPathLayout();
        BuildGround(grass, leaves, mud, path, stream);

        Sprite oak = LoadSprite(F(Forest, "environment/forest-oak-tree.png"), true);
        Sprite pine = LoadSprite(F(Forest, "environment/forest-pine-tree.png"), true);
        Sprite deadTree = LoadSprite(F(Forest, "environment/forest-dead-tree.png"), true);
        Sprite largeBush = LoadSprite(F(Forest, "environment/forest-large-bush.png"), true);
        Sprite smallBush = LoadSprite(F(Forest, "environment/forest-small-bush.png"), true);
        Sprite fern = LoadSprite(F(Forest, "environment/forest-fern.png"), true);
        Sprite boulderLarge = LoadSprite(F(Forest, "environment/forest-boulder-lg.png"), true);
        Sprite boulderSmall = LoadSprite(F(Forest, "environment/forest-boulder-sm.png"), true);
        Sprite fallenLog = LoadSprite(F(Forest, "environment/forest-fallen-log.png"), true);
        Sprite stump = LoadSprite(F(Forest, "environment/forest-tree-stump.png"), true);
        Sprite berries = LoadSprite(F(Forest, "environment/forest-berry-bush.png"), true);
        Sprite mushrooms = LoadSprite(F(Forest, "environment/forest-mushroom-cluster.png"), true);
        Sprite redMushroom = LoadSprite(F(Forest, "environment/forest-mushroom-red.png"), true);
        Sprite flowerRed = LoadSprite(F(Forest, "environment/forest-flower-red.png"), true);
        Sprite flowerYellow = LoadSprite(F(Forest, "environment/forest-flower-yellow.png"), true);

        PlaceForestProps(oak, pine, deadTree, largeBush, smallBush, fern, boulderLarge, boulderSmall, fallenLog, stump, berries, mushrooms, redMushroom, flowerRed, flowerYellow);

        Sprite cabin = LoadSprite(F(Forest, "buildings/forest-log-cabin.png"), true);
        Sprite lookout = LoadSprite(F(Forest, "buildings/forest-hunters-blind.png"), true);
        Sprite tent = LoadSprite(F(Forest, "buildings/forest-campsite-tent.png"), true);
        Sprite bridge = LoadSprite(F(Forest, "buildings/forest-wooden-bridge.png"), true);
        AddProp("Abandoned Hunter Cabin", cabin, new Vector2Int(-12, 7), new Vector3(1.25f, 1.25f, 1f), true, 1);
        AddProp("Hunter's Lookout", lookout, new Vector2Int(12, -7), Vector3.one, true, 1);
        AddProp("Forgotten Camp", tent, new Vector2Int(-8, -8), new Vector3(1.1f, 1.1f, 1f), true, 1);
        AddProp("Broken Wooden Crossing", bridge, new Vector2Int(7, 2), new Vector3(1.4f, 1.1f, 1f), false, 1);

        Sprite cathedral = LoadSprite(F(Gothic, "buildings/gothic-cathedral-facade.png"), true);
        Sprite crypt = LoadSprite(F(Gothic, "buildings/gothic-crypt-entrance.png"), true);
        Sprite altar = LoadSprite(F(Gothic, "buildings/gothic-stone-altar.png"), true);
        Sprite candelabra = LoadSprite(F(Gothic, "environment/gothic-iron-candelabra.png"), true);
        Sprite tomb = LoadSprite(F(Gothic, "environment/gothic-stone-tomb.png"), true);
        Sprite campfire = LoadSprite(F(Forest, "items/forest-campfire.png"), true);
        Sprite woodPile = LoadSprite(F(Forest, "items/forest-wood-pile.png"), true);
        Sprite dungeonPillar = LoadSprite(F(Dungeon, "buildings/dungeon-pillar.png"), true);
        Sprite dungeonArch = LoadSprite(F(Dungeon, "buildings/dungeon-arch-passage.png"), true);
        Sprite dungeonWall = LoadSprite(F(Dungeon, "buildings/dungeon-stone-wall-32.png"), true);
        Sprite dungeonChain = LoadSprite(F(Dungeon, "items/dungeon-hanging-chain.png"), true);
        Sprite dungeonBarrel = LoadSprite(F(Dungeon, "items/dungeon-barrel.png"), true);
        Sprite dungeonSkulls = LoadSprite(F(Dungeon, "items/dungeon-skull-pile.png"), true);
        Sprite dungeonDoor = LoadSprite(F(Dungeon, "buildings/dungeon-iron-door.png"), true);

        AddProp("The Silent Cathedral", cathedral, new Vector2Int(12, 9), new Vector3(1.8f, 1.8f, 1f), true, 2);
        AddProp("North Cathedral Column", dungeonPillar, new Vector2Int(9, 10), new Vector3(1.15f, 1.15f, 1f), true, 2);
        AddProp("South Cathedral Column", dungeonPillar, new Vector2Int(15, 8), new Vector3(1.15f, 1.15f, 1f), true, 2);
        AddProp("Processional Arch", dungeonArch, new Vector2Int(4, 10), new Vector3(1.7f, 1.7f, 1f), true, 1);
        AddProp("Broken Stone Wall North", dungeonWall, new Vector2Int(-3, 11), new Vector3(1.45f, 1.45f, 1f), true, 0);
        AddProp("Broken Stone Wall West", dungeonWall, new Vector2Int(-15, -1), new Vector3(1.45f, 1.45f, 1f), true, 0);
        AddProp("Sealed Iron Door", dungeonDoor, new Vector2Int(8, 9), new Vector3(1.35f, 1.35f, 1f), true, 2);
        AddProp("Chain Over The Red Chasm", dungeonChain, new Vector2Int(-13, -7), new Vector3(1.7f, 1.7f, 1f), false, 1);
        AddProp("Abandoned Merchant Barrel", dungeonBarrel, new Vector2Int(-7, -7), Vector3.one, true, 0);
        AddProp("Bone Heap Near The Abyss", dungeonSkulls, new Vector2Int(-10, -11), new Vector3(0.95f, 0.95f, 1f), false, 0);
        AddProp("Ancient Tomb", tomb, new Vector2Int(6, 8), new Vector3(1.2f, 1.2f, 1f), true, 1);
        AddProp("Ashen Candelabra", candelabra, new Vector2Int(10, 7), new Vector3(1.2f, 1.2f, 1f), false, 2);
        AddProp("Forgotten Wood Pile", woodPile, new Vector2Int(-5, -7), new Vector3(1.1f, 1.1f, 1f), false, 1);
        GameObject fire = AddProp("Wayfarer's Campfire", campfire, new Vector2Int(-4, -3), new Vector3(1.5f, 1.5f, 1f), false, 5);

        Sprite heroSprite = Resources.Load<Sprite>("NemesisCharacters/Player/Idle/Down/00");
        if (heroSprite == null) heroSprite = LoadSprite(F(Gothic, "characters/gothic-hooded-monk.png"), true);
        Sprite keeperSprite = LoadSprite(F(Gothic, "characters/gothic-cathedral-keeper.png"), true);
        Sprite specter = LoadSprite(F(Gothic, "enemies/gothic-shadow-specter.png"), true);
        Sprite revenant = LoadSprite(F(Gothic, "enemies/gothic-bone-revenant.png"), true);
        Sprite bat = LoadSprite(F(Gothic, "enemies/gothic-demon-bat.png"), true);
        Sprite gargoyle = LoadSprite(F(Gothic, "enemies/gothic-gargoyle-awakened.png"), true);
        Sprite fallenAngel = LoadSprite(F(Gothic, "enemies/gothic-fallen-angel.png"), true);
        Sprite chestSprite = LoadSprite(F(Dungeon, "items/dungeon-treasure-chest.png"), true);

        Vector3 playerPosition = Iso(-2, -3);
        GameObject player = AddSprite("Player", heroSprite, playerPosition, new Vector3(0.95f, 0.95f, 1f), 6, false);
        player.tag = "Player";
        Rigidbody2D body = player.AddComponent<Rigidbody2D>();
        body.gravityScale = 0f;
        body.freezeRotation = true;
        body.constraints = RigidbodyConstraints2D.FreezeRotation;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        body.linearDamping = 5f;
        CapsuleCollider2D playerCollider = player.AddComponent<CapsuleCollider2D>();
        playerCollider.size = new Vector2(0.48f, 0.46f);
        player.AddComponent<NemesisHeroAnimator>();
        player.AddComponent<NemesisPlayer>();

        AddEnemy("Ash Wraith", specter, new Vector2Int(2, 1), 65, 12, 1.55f, 3.7f, new Vector3(1.55f, 1.55f, 1f));
        AddEnemy("Bone Revenant", revenant, new Vector2Int(6, 3), 90, 17, 1.1f, 4f, new Vector3(1.5f, 1.5f, 1f));
        AddEnemy("Demon Bat", bat, new Vector2Int(-9, 5), 40, 9, 2.3f, 4.5f, new Vector3(1.8f, 1.8f, 1f));
        AddEnemy("Awakened Gargoyle", gargoyle, new Vector2Int(10, 8), 145, 20, 1.2f, 4.3f, new Vector3(1.8f, 1.8f, 1f));
        GameObject boss = AddEnemy("The Hollow Knight", fallenAngel, new Vector2Int(13, 10), 420, 22, 1.05f, 4.2f, new Vector3(2.4f, 2.4f, 1f));
        boss.name = "The Hollow Knight";

        GameObject keeper = AddSprite("The Ash Keeper", keeperSprite, Iso(-6, -4), new Vector3(1.55f, 1.55f, 1f), 5, true);
        CircleCollider2D keeperCollider = keeper.AddComponent<CircleCollider2D>();
        keeperCollider.radius = 0.46f;
        keeperCollider.isTrigger = true;
        NemesisNPC keeperNpc = keeper.AddComponent<NemesisNPC>();

        GameObject chest = AddSprite("Sealed Relic Chest", chestSprite, Iso(1, -6), new Vector3(1.5f, 1.5f, 1f), 3, true);
        BoxCollider2D chestCollider = chest.AddComponent<BoxCollider2D>();
        chestCollider.size = new Vector2(0.75f, 0.65f);
        chestCollider.isTrigger = true;
        chest.AddComponent<NemesisChest>();

        GameObject shrine = AddSprite("Ashen Shrine", altar, Iso(-8, 2), new Vector3(1.6f, 1.6f, 1f), 5, true);
        CircleCollider2D shrineCollider = shrine.AddComponent<CircleCollider2D>();
        shrineCollider.radius = 0.62f;
        shrineCollider.isTrigger = true;
        shrine.AddComponent<NemesisShrine>();

        GameObject portal = AddSprite("Descent to the Hollow March", crypt, Iso(8, 7), new Vector3(1.7f, 1.7f, 1f), 6, true);
        CircleCollider2D portalCollider = portal.AddComponent<CircleCollider2D>();
        portalCollider.radius = 0.78f;
        portalCollider.isTrigger = true;
        portal.AddComponent<NemesisDungeonPortal>();

        NemesisWorldArtPass.Apply();

        NemesisCamera follow = camera.GetComponent<NemesisCamera>();
        if (follow == null) follow = camera.gameObject.AddComponent<NemesisCamera>();
        follow.target = player.transform;
        follow.SnapToTarget();

        NemesisInfiniteWorld streaming = new GameObject("Infinite World Streamer").AddComponent<NemesisInfiniteWorld>();
        streaming.target = player.transform;

        new GameObject("Game Systems").AddComponent<NemesisHUD>();
        new GameObject("Outskirts Exploration Fog").AddComponent<NemesisOutskirtsFogSetup>();

        EditorSceneManager.SaveScene(scene, Root + "/Scenes/AshenOutskirts.unity");
        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(Root + "/Scenes/AshenOutskirts.unity", true),
            new EditorBuildSettingsScene(Root + "/Scenes/AshenDungeon.unity", true)
        };
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("NEMESIS: Ashen Outskirts rebuilt with forest, gothic, dungeon sprites, real landmarks, colliders and player/enemy sprites.");
    }

    [MenuItem("Nemesis/Build All Scenes")]
    public static void BuildAllScenes()
    {
        NemesisUIAssetInstaller.Install();
        NemesisInfiniteWorldInstaller.Install();
        NemesisCharacterAssetInstaller.Install();
        Build();
        NemesisDungeonSceneBuilder.Build();
        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(Root + "/Scenes/AshenOutskirts.unity", true),
            new EditorBuildSettingsScene(Root + "/Scenes/AshenDungeon.unity", true)
        };
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("NEMESIS: all playable scenes rebuilt.");
    }

    private static void BuildPathLayout()
    {
        PathCells.Clear();
        Vector2Int[] route =
        {
            new Vector2Int(-1, -11),
            new Vector2Int(-1, -7),
            new Vector2Int(1, -4),
            new Vector2Int(3, -1),
            new Vector2Int(5, 3),
            new Vector2Int(8, 7),
            new Vector2Int(11, 9)
        };

        for (int i = 1; i < route.Length; i++)
            DrawPathSegment(route[i - 1], route[i], 1);

        for (int x = -2; x <= 1; x++)
        for (int y = -4; y <= -1; y++)
            PathCells.Add(new Vector2Int(x, y));
    }

    private static void DrawPathSegment(Vector2Int a, Vector2Int b, int radius)
    {
        int dx = Mathf.Abs(b.x - a.x);
        int dy = Mathf.Abs(b.y - a.y);
        int sx = a.x < b.x ? 1 : -1;
        int sy = a.y < b.y ? 1 : -1;
        int error = dx - dy;
        Vector2Int p = a;

        while (true)
        {
            for (int ox = -radius; ox <= radius; ox++)
            for (int oy = -radius; oy <= radius; oy++)
            {
                Vector2Int cell = new Vector2Int(p.x + ox, p.y + oy);
                if (cell.x >= -16 && cell.x <= 16 && cell.y >= -12 && cell.y <= 12)
                    PathCells.Add(cell);
            }
            if (p == b) break;
            int twice = error * 2;
            if (twice > -dy) { error -= dy; p.x += sx; }
            if (twice < dx) { error += dx; p.y += sy; }
        }
    }

    private static void BuildGround(Sprite grass, Sprite leaves, Sprite mud, Sprite path, Sprite stream)
    {
        Dictionary<Sprite, TileBatch> batches = new Dictionary<Sprite, TileBatch>();
        Dictionary<Sprite, int> sortingOrders = new Dictionary<Sprite, int>();

        for (int y = -12; y <= 12; y++)
        for (int x = -16; x <= 16; x++)
        {
            Vector2Int cell = new Vector2Int(x, y);
            Sprite choice;
            int order = 0;
            bool riverCell = stream != null && x <= -12 && y <= -7 && x + y <= -18;
            if (riverCell) { choice = stream; order = 2; }
            else if (PathCells.Contains(cell) || (x >= -12 && x <= -8 && y >= -11 && y <= -8)) { choice = path != null ? path : mud; order = 1; }
            else { int roll = Rng.Next(100); choice = roll < 58 ? grass : roll < 84 ? leaves : mud; }

            if (choice == null) continue;
            if (!batches.TryGetValue(choice, out TileBatch batch))
            {
                batch = new TileBatch();
                batches.Add(choice, batch);
                sortingOrders.Add(choice, order);
            }
            AddDiamond(batch, choice, Iso(x, y));
        }

        string outputFolder = "Assets/Nemesis/Generated";
        string materialFolder = outputFolder + "/Materials";
        if (!Directory.Exists(outputFolder)) Directory.CreateDirectory(outputFolder);
        if (!Directory.Exists(materialFolder)) Directory.CreateDirectory(materialFolder);

        foreach (string oldGuid in AssetDatabase.FindAssets("Ground_", new[] { outputFolder }))
            AssetDatabase.DeleteAsset(AssetDatabase.GUIDToAssetPath(oldGuid));
        foreach (KeyValuePair<Sprite, TileBatch> entry in batches)
        {
            Sprite sprite = entry.Key;
            TileBatch batch = entry.Value;
            string safeName = sprite.name.Replace(" ", "_").Replace("-", "_");
            string meshPath = outputFolder + "/Ground_" + safeName + ".asset";
            string materialPath = materialFolder + "/Ground_" + safeName + ".mat";

            if (AssetDatabase.LoadAssetAtPath<Mesh>(meshPath) != null) AssetDatabase.DeleteAsset(meshPath);
            Mesh mesh = new Mesh();
            mesh.name = "Isometric Ground - " + sprite.name;
            mesh.indexFormat = IndexFormat.UInt32;
            mesh.SetVertices(batch.vertices);
            mesh.SetUVs(0, batch.uvs);
            mesh.SetTriangles(batch.triangles, 0);
            mesh.RecalculateBounds();
            AssetDatabase.CreateAsset(mesh, meshPath);

            Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (material == null)
            {
                Shader shader = Shader.Find("Sprites/Default");
                if (shader == null) shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
                material = new Material(shader);
                material.name = "Ground - " + sprite.name;
                AssetDatabase.CreateAsset(material, materialPath);
            }
            material.mainTexture = sprite.texture;
            EditorUtility.SetDirty(material);

            GameObject layer = new GameObject("Ground Layer - " + sprite.name);
            layer.transform.SetParent(worldRoot, false);
            MeshFilter filter = layer.AddComponent<MeshFilter>();
            filter.sharedMesh = mesh;
            MeshRenderer renderer = layer.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            renderer.sortingLayerName = "Default";
            renderer.sortingOrder = sortingOrders[sprite];
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
        }
        AssetDatabase.SaveAssets();
    }

    private static void AddDiamond(TileBatch batch, Sprite sprite, Vector3 center)
    {
        Rect rect = sprite.textureRect;
        Texture texture = sprite.texture;
        float u0 = rect.xMin / texture.width;
        float u1 = rect.xMax / texture.width;
        float v0 = rect.yMin / texture.height;
        float v1 = rect.yMax / texture.height;
        float x = center.x;
        float y = center.y;
        int index = batch.vertices.Count;

        batch.vertices.Add(new Vector3(x, y + CellHeight, 0f));
        batch.vertices.Add(new Vector3(x + CellWidth, y, 0f));
        batch.vertices.Add(new Vector3(x, y - CellHeight, 0f));
        batch.vertices.Add(new Vector3(x - CellWidth, y, 0f));
        batch.uvs.Add(new Vector2((u0 + u1) * 0.5f, v1));
        batch.uvs.Add(new Vector2(u1, (v0 + v1) * 0.5f));
        batch.uvs.Add(new Vector2((u0 + u1) * 0.5f, v0));
        batch.uvs.Add(new Vector2(u0, (v0 + v1) * 0.5f));
        batch.triangles.Add(index);
        batch.triangles.Add(index + 1);
        batch.triangles.Add(index + 2);
        batch.triangles.Add(index);
        batch.triangles.Add(index + 2);
        batch.triangles.Add(index + 3);
    }
    private static void PlaceForestProps(
        Sprite oak, Sprite pine, Sprite deadTree, Sprite largeBush, Sprite smallBush, Sprite fern,
        Sprite boulderLarge, Sprite boulderSmall, Sprite fallenLog, Sprite stump, Sprite berries,
        Sprite mushrooms, Sprite redMushroom, Sprite flowerRed, Sprite flowerYellow)
    {
        Vector2Int[] treeCells =
        {
            new Vector2Int(-15,-10), new Vector2Int(-13,-5), new Vector2Int(-15,1), new Vector2Int(-13,8),
            new Vector2Int(-10,11), new Vector2Int(-7,10), new Vector2Int(-4,11), new Vector2Int(0,11),
            new Vector2Int(4,11), new Vector2Int(8,11), new Vector2Int(13,10), new Vector2Int(15,6),
            new Vector2Int(14,0), new Vector2Int(15,-7), new Vector2Int(11,-10), new Vector2Int(6,-11),
            new Vector2Int(1,-11), new Vector2Int(-5,-10), new Vector2Int(-10,-9), new Vector2Int(13,-3)
        };

        for (int i = 0; i < treeCells.Length; i++)
        {
            Sprite tree = i % 5 == 0 ? deadTree : i % 2 == 0 ? oak : pine;
            AddProp(i % 5 == 0 ? "Dead Forest Tree" : "Old Forest Tree", tree, treeCells[i],
                i % 2 == 0 ? new Vector3(1.18f, 1.18f, 1f) : new Vector3(1.05f, 1.05f, 1f), true, 0);
        }

        int placed = 0;
        int attempts = 0;
        while (placed < 30 && attempts++ < 400)
        {
            int x = Rng.Next(-15, 16);
            int y = Rng.Next(-11, 12);
            Vector2Int cell = new Vector2Int(x, y);
            if (Occupied.Contains(cell) || PathCells.Contains(cell)) continue;
            if (Vector2Int.Distance(cell, new Vector2Int(-2, -3)) < 4f) continue;
            if (Mathf.Abs(x) < 5 && y < 1 && y > -8) continue;

            int kind = Rng.Next(7);
            if (kind == 0) AddProp("Overgrown Bush", largeBush, cell, Vector3.one, false);
            else if (kind == 1) AddProp("Forest Fern", fern, cell, new Vector3(1.1f, 1.1f, 1f), false);
            else if (kind == 2) AddProp("Stone Boulder", boulderLarge, cell, Vector3.one, true);
            else if (kind == 3) AddProp("Small Boulder", boulderSmall, cell, Vector3.one, true);
            else if (kind == 4) AddProp("Berry Thicket", berries, cell, Vector3.one, false);
            else if (kind == 5) AddProp("Mushroom Cluster", mushrooms, cell, Vector3.one, false);
            else AddProp("Small Forest Bush", smallBush, cell, Vector3.one, false);
            placed++;
        }

        for (int i = 0; i < 12; i++)
        {
            int x = Rng.Next(-14, 15);
            int y = Rng.Next(-10, 11);
            Vector2Int cell = new Vector2Int(x, y);
            if (Occupied.Contains(cell) || PathCells.Contains(cell)) continue;
            Sprite detail = i % 4 == 0 ? flowerRed : i % 4 == 1 ? flowerYellow : i % 4 == 2 ? redMushroom : stump;
            AddProp("Forest Floor Detail", detail, cell, Vector3.one, false, -2);
        }

        AddProp("Fallen Ancient Log", fallenLog, new Vector2Int(-11, 1), new Vector3(1.35f, 1.35f, 1f), true);
    }

    private static GameObject AddEnemy(string name, Sprite sprite, Vector2Int cell, int health, int damage, float speed, float aggro, Vector3 scale)
    {
        GameObject enemy = AddSprite(name, sprite, Iso(cell.x, cell.y), scale, 7, false);
        if (enemy == null) return null;
        Rigidbody2D rb = enemy.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.linearDamping = 3f;
        CircleCollider2D collider = enemy.AddComponent<CircleCollider2D>();
        collider.radius = 0.34f;
        NemesisEnemy ai = enemy.AddComponent<NemesisEnemy>();
        ai.health = health;
        ai.damage = damage;
        ai.speed = speed;
        ai.aggroRadius = aggro;
        return enemy;
    }
}













