using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class NemesisWorldArtPass
{
    private const string Catacombs = "Assets/ThirdParty/NemesisCatacombs/";
    private const string Cursed = "Assets/ThirdParty/CursedLand/PNG/Objects_separetely/";

    private static Sprite Load(string path, float ppu = 32f, Vector2? pivot = null)
    {
        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null)
        {
            Debug.LogWarning("NEMESIS ART: missing " + path);
            return null;
        }

        bool changed = false;
        if (importer.textureType != TextureImporterType.Sprite) { importer.textureType = TextureImporterType.Sprite; changed = true; }
        if (importer.spriteImportMode != SpriteImportMode.Single) { importer.spriteImportMode = SpriteImportMode.Single; changed = true; }
        if (importer.filterMode != FilterMode.Point) { importer.filterMode = FilterMode.Point; changed = true; }
        if (importer.spritePixelsPerUnit != ppu) { importer.spritePixelsPerUnit = ppu; changed = true; }
        if (importer.textureCompression != TextureImporterCompression.Uncompressed) { importer.textureCompression = TextureImporterCompression.Uncompressed; changed = true; }
        if (importer.mipmapEnabled) { importer.mipmapEnabled = false; changed = true; }
        if (!importer.alphaIsTransparency) { importer.alphaIsTransparency = true; changed = true; }
        if (pivot.HasValue)
        {
            TextureImporterSettings settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            if (settings.spriteAlignment != (int)SpriteAlignment.Custom || settings.spritePivot != pivot.Value)
            {
                settings.spriteAlignment = (int)SpriteAlignment.Custom;
                settings.spritePivot = pivot.Value;
                importer.SetTextureSettings(settings);
                changed = true;
            }
        }
        if (changed) importer.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    private static Sprite[] LoadSeries(string folder, string stem, int first, int count, string extension)
    {
        List<Sprite> result = new List<Sprite>();
        for (int i = first; i < first + count; i++)
        {
            string name = extension == "png"
                ? stem + i.ToString("00") + ".png"
                : stem + i + ".png";
            Sprite sprite = Load(folder + name, 32f, new Vector2(0.5f, 0.05f));
            if (sprite != null) result.Add(sprite);
        }
        return result.ToArray();
    }

    private static Sprite[] LoadCursedSeries(string stem, int count)
    {
        List<Sprite> result = new List<Sprite>();
        for (int i = 1; i <= count; i++)
        {
            Sprite sprite = Load(Cursed + stem + i + ".png", 32f, new Vector2(0.5f, 0.08f));
            if (sprite != null) result.Add(sprite);
        }
        return result.ToArray();
    }    private static Sprite LoadCursedFrame(string stem)
    {
        return Load(Cursed + stem + "1.png", 32f, new Vector2(0.5f, 0.08f));
    }


    private static GameObject Add(Transform parent, string name, Sprite sprite, Vector3 position, Vector3 scale, int orderOffset = 0, bool blocking = false)
    {
        if (sprite == null) return null;
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        go.transform.position = position;
        go.transform.localScale = scale;
        SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.spriteSortPoint = SpriteSortPoint.Pivot;
        renderer.sortingOrder = 100 - Mathf.RoundToInt(position.y * 100f) + orderOffset;
        NemesisDepthSort depth = go.AddComponent<NemesisDepthSort>();
        depth.orderOffset = orderOffset;

        if (blocking)
        {
            BoxCollider2D collider = go.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(0.55f, 0.32f);
            collider.offset = new Vector2(0f, 0.16f);
        }
        return go;
    }

    private static GameObject AddAnimated(Transform parent, string name, Sprite[] frames, Vector3 position, Vector3 scale, float fps, int orderOffset = 2, bool blocking = false)
    {
        if (frames == null || frames.Length == 0) return null;
        GameObject go = Add(parent, name, frames[0], position, scale, orderOffset, blocking);
        if (go == null) return null;
        NemesisSpriteAnimation animation = go.AddComponent<NemesisSpriteAnimation>();
        animation.frames = frames;
        animation.framesPerSecond = fps;
        animation.loop = true;
        animation.randomStart = true;
        return go;
    }

    private static Vector3 Iso(int x, int y)
    {
        return new Vector3((x - y) * 0.72f, (x + y) * 0.36f, 0f);
    }

    public static void Apply()
    {
        Transform root = GameObject.Find("Ashen Outskirts - World")?.transform;
        if (root == null)
        {
            Debug.LogWarning("NEMESIS ART: world root was not found; set dressing skipped.");
            return;
        }

        // Only a few selected props animate. Most scenery stays still so the
        // player, enemies and combat remain the visual focus.
        Sprite[] torch =
        {
            Load(Catacombs + "torch_1.png", 16f, new Vector2(0.5f, 0.08f)),
            Load(Catacombs + "torch_2.png", 16f, new Vector2(0.5f, 0.08f)),
            Load(Catacombs + "torch_3.png", 16f, new Vector2(0.5f, 0.08f)),
            Load(Catacombs + "torch_4.png", 16f, new Vector2(0.5f, 0.08f))
        };
        Sprite[] candleA =
        {
            Load(Catacombs + "candleA_01.png", 16f, new Vector2(0.5f, 0.05f)),
            Load(Catacombs + "candleA_02.png", 16f, new Vector2(0.5f, 0.05f)),
            Load(Catacombs + "candleA_03.png", 16f, new Vector2(0.5f, 0.05f)),
            Load(Catacombs + "candleA_04.png", 16f, new Vector2(0.5f, 0.05f))
        };
        Sprite[] spikes =
        {
            Load(Catacombs + "spike_0.png", 16f, new Vector2(0.5f, 0.05f)),
            Load(Catacombs + "spike_1.png", 16f, new Vector2(0.5f, 0.05f)),
            Load(Catacombs + "spike_2.png", 16f, new Vector2(0.5f, 0.05f)),
            Load(Catacombs + "spike_3.png", 16f, new Vector2(0.5f, 0.05f)),
            Load(Catacombs + "spike_4.png", 16f, new Vector2(0.5f, 0.05f))
        };

        // Two fire accents and one candle cluster provide subtle ambient motion.
                AddAnimated(root, "Animated Brazier West", torch, Iso(-10, -2), new Vector3(3.8f, 3.8f, 1f), 4f, 4);
        AddAnimated(root, "Animated Brazier East", torch, Iso(13, 1), new Vector3(3.8f, 3.8f, 1f), 4f, 4);
        AddAnimated(root, "Candles Near Shrine", candleA, Iso(-6, 2), new Vector3(3.6f, 3.6f, 1f), 3f, 7);

        // One readable environmental hazard keeps animation gameplay-relevant.
        GameObject trap = AddAnimated(root, "Retractable Abyss Spikes", spikes, Iso(4, -6), new Vector3(3.0f, 3.0f, 1f), 3.2f, 8, true);
        if (trap != null)
        {
            CircleCollider2D damage = trap.AddComponent<CircleCollider2D>();
            damage.radius = 0.42f;
            damage.isTrigger = true;
            trap.AddComponent<NemesisSpikeTrap>();
        }

        // Cursed vegetation is mostly static. Only one growth subtly animates.
        if (Directory.Exists(Cursed))
        {
            Add(root, "Many Eyes - West", LoadCursedFrame("Many_eyes_plant_shadow1_"), Iso(-13, 2), new Vector3(1.15f, 1.15f, 1f), -1);
            Add(root, "Many Eyes - East", LoadCursedFrame("Many_eyes_plant_shadow2_"), Iso(14, -4), new Vector3(1.15f, 1.15f, 1f), -1);
            Add(root, "Meat Flower - South", LoadCursedFrame("Meat_flower_shadow1_"), Iso(-12, -9), new Vector3(0.58f, 0.58f, 1f), 0, true);
            Add(root, "Meat Flower - North", LoadCursedFrame("Meat_flower_shadow2_"), Iso(12, 7), new Vector3(0.58f, 0.58f, 1f), 0, true);
            Add(root, "Tentacle Growth", LoadCursedFrame("Tentacle_plant_shadow1_"), Iso(-8, 10), new Vector3(0.58f, 0.58f, 1f), -1);
            AddAnimated(root, "Jaws of the Abyss", LoadCursedSeries("Jaws_plant_shadow1_", 3), Iso(7, -8), new Vector3(0.58f, 0.58f, 1f), 2.2f, 0, true);
            Add(root, "Watcher Eye", LoadCursedFrame("Eye_plant_shadow1_"), Iso(15, 9), new Vector3(1.0f, 1.0f, 1f), 0);

            Add(root, "Bone Debris West", Load(Cursed + "Bones_shadow1_1.png", 32f, new Vector2(0.5f, 0.08f)), Iso(-14, -5), new Vector3(0.5f, 0.5f, 1f), -2);
            Add(root, "Broken Ruins East", Load(Cursed + "Ruins_shadow2_1.png", 32f, new Vector2(0.5f, 0.08f)), Iso(15, 2), new Vector3(0.65f, 0.65f, 1f), 1, true);
        }
        Debug.Log("NEMESIS ART: restrained motion pass applied (2 braziers, 1 candle cluster, 1 spike trap, 1 animated plant; remaining scenery static).");
    }
}








