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

        // Torch and candle animation from the supplied RF_Catacombs pack.
        Sprite[] torch = new Sprite[]
        {
            Load(Catacombs + "torch_1.png", 16f, new Vector2(0.5f, 0.08f)),
            Load(Catacombs + "torch_2.png", 16f, new Vector2(0.5f, 0.08f)),
            Load(Catacombs + "torch_3.png", 16f, new Vector2(0.5f, 0.08f)),
            Load(Catacombs + "torch_4.png", 16f, new Vector2(0.5f, 0.08f))
        };
        Sprite[] candleA = new Sprite[]
        {
            Load(Catacombs + "candleA_01.png", 16f, new Vector2(0.5f, 0.05f)),
            Load(Catacombs + "candleA_02.png", 16f, new Vector2(0.5f, 0.05f)),
            Load(Catacombs + "candleA_03.png", 16f, new Vector2(0.5f, 0.05f)),
            Load(Catacombs + "candleA_04.png", 16f, new Vector2(0.5f, 0.05f))
        };
        AddAnimated(root, "Animated Brazier West", torch, Iso(-10, -2), new Vector3(3.8f, 3.8f, 1f), 2f, 4);
        AddAnimated(root, "Shrine Candle", candleA, Iso(-6, 2), new Vector3(3.6f, 3.6f, 1f), 1.8f, 7);
        Debug.Log("NEMESIS ART: a restrained set of torch and candle animations was placed in Ashen Outskirts.");
    }
}







