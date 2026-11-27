using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.IO;

public static class NemesisDungeonSceneBuilder
{
    const string Root = "Assets/Nemesis";
    const string DungeonAssets = "Assets/ThirdParty/NemesisFreeCC0/dungeon/assets/";
    const string GothicAssets = "Assets/ThirdParty/NemesisFreeCC0/gothic/assets/";

    static Sprite LoadSprite(string path)
    {
        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
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
        if (changed) importer.SaveAndReimport();

        Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (sprite == null)
        {
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(path);
            foreach (Object asset in assets)
            {
                sprite = asset as Sprite;
                if (sprite != null) break;
            }
        }
        if (sprite == null) Debug.LogError("NEMESIS SPRITE IMPORT FAILED: " + path + " (set Sprite Mode to Single)");
        return sprite;
    }

    [MenuItem("Nemesis/Generate Procedural Dungeon Scene")]
    public static void Build()
    {
        if (!Directory.Exists(Root + "/Scenes")) Directory.CreateDirectory(Root + "/Scenes");
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        Camera camera = new GameObject("Main Camera").AddComponent<Camera>();
        camera.tag = "MainCamera";
        camera.orthographic = true;
        camera.orthographicSize = 8.5f;
        camera.backgroundColor = new Color(.015f, .02f, .028f);
        camera.transform.position = new Vector3(0, 0, -10);

        GameObject generatorObject = new GameObject("Dungeon Generator");
        NemesisDungeonGenerator generator = generatorObject.AddComponent<NemesisDungeonGenerator>();
        generator.seed = 0;
        generator.roomCount = 10;
        generator.mapSize = new Vector2Int(64, 64);
        generator.corridorWidth = 3;
        generator.visionRadius = 8;

        generator.floorVariants = new[]
        {
            LoadSprite(DungeonAssets + "tilesets/dungeon-stone-floor-32.png"),
            LoadSprite(DungeonAssets + "tilesets/dungeon-cracked-floor-32.png"),
            LoadSprite(DungeonAssets + "tilesets/dungeon-cobble-floor-32.png")
        };
        generator.wallSprite = LoadSprite(DungeonAssets + "buildings/dungeon-stone-wall-32.png");
        generator.archSprite = LoadSprite(DungeonAssets + "buildings/dungeon-arch-passage.png");
        generator.staircaseSprite = LoadSprite(DungeonAssets + "buildings/dungeon-staircase.png");
        generator.torchSprite = LoadSprite(DungeonAssets + "items/dungeon-wall-torch.png");
        generator.barrelSprite = LoadSprite(DungeonAssets + "items/dungeon-barrel.png");
        generator.webSprite = LoadSprite(DungeonAssets + "environment/dungeon-spider-web.png");
        generator.mossSprite = LoadSprite(DungeonAssets + "environment/dungeon-moss-patch.png");
        generator.crystalSprite = LoadSprite(DungeonAssets + "items/dungeon-magic-crystal.png");
        generator.trapSprite = LoadSprite(DungeonAssets + "items/dungeon-spike-trap.png");
        generator.chestSprite = LoadSprite(DungeonAssets + "items/dungeon-treasure-chest.png");
        generator.pillarSprite = LoadSprite(DungeonAssets + "buildings/dungeon-pillar.png");
        generator.cageSprite = LoadSprite(DungeonAssets + "items/dungeon-iron-cage-lg.png");
        generator.mushroomSprite = LoadSprite(DungeonAssets + "environment/dungeon-glow-mushroom.png");
        generator.altarSprite = LoadSprite(GothicAssets + "buildings/gothic-stone-altar.png");
        generator.enemyVariants = new[]
        {
            LoadSprite(GothicAssets + "enemies/gothic-shadow-specter.png"),
            LoadSprite(GothicAssets + "enemies/gothic-bone-revenant.png"),
            LoadSprite(GothicAssets + "enemies/gothic-demon-bat.png"),
            LoadSprite(GothicAssets + "enemies/gothic-gargoyle-awakened.png")
        };
        generator.bossSprite = LoadSprite(GothicAssets + "enemies/gothic-fallen-angel.png");
        generator.playerSprite = LoadSprite(GothicAssets + "characters/gothic-hooded-monk.png");

        GameObject player = new GameObject("Player");
        SpriteRenderer playerRenderer = player.AddComponent<SpriteRenderer>();
        playerRenderer.sprite = generator.playerSprite;
        if (playerRenderer.sprite == null)
        {
            Texture2D texture = new Texture2D(16, 24, TextureFormat.RGBA32, false);
            for (int y = 0; y < 24; y++)
                for (int x = 0; x < 16; x++)
                    texture.SetPixel(x, y, (x > 3 && x < 12 && y > 2) ? new Color(.68f, .69f, .67f) : Color.clear);
            texture.Apply();
            playerRenderer.sprite = Sprite.Create(texture, new Rect(0, 0, 16, 24), new Vector2(.5f, .1f), 32);
        }
        playerRenderer.sortingOrder = 20;
        Rigidbody2D body = player.AddComponent<Rigidbody2D>();
        body.gravityScale = 0;
        body.freezeRotation = true;
        body.constraints = RigidbodyConstraints2D.FreezeRotation;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        player.AddComponent<CapsuleCollider2D>().size = new Vector2(.7f, .55f);
        player.AddComponent<NemesisPlayer>();
        NemesisCamera dungeonFollow = camera.gameObject.GetComponent<NemesisCamera>();
        if (dungeonFollow == null) dungeonFollow = camera.gameObject.AddComponent<NemesisCamera>();
        dungeonFollow.target = player.transform;
        dungeonFollow.SnapToTarget();


        new GameObject("Game Systems").AddComponent<NemesisHUD>();

        EditorSceneManager.SaveScene(scene, Root + "/Scenes/AshenDungeon.unity");
        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(Root + "/Scenes/AshenOutskirts.unity", true),
            new EditorBuildSettingsScene(Root + "/Scenes/AshenDungeon.unity", true)
        };
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("NEMESIS: top-down procedural dungeon scene generated; imported sprites use Single mode and fog of war is configured for runtime.");
    }
}


