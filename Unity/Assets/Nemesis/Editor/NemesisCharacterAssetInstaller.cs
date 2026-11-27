using System.IO;
using UnityEditor;
using UnityEngine;

public static class NemesisCharacterAssetInstaller
{
    private const string SpriteRoot = "Assets/Resources/NemesisCharacters/Player";

    [MenuItem("Nemesis/Import Hero Animation Frames")]
    public static void Install()
    {
        if (!Directory.Exists(SpriteRoot))
        {
            Debug.LogWarning("NEMESIS HERO: 2DPIXX directional animation frames are missing; the fallback pilgrim sprite will be used.");
            return;
        }

        AssetDatabase.Refresh();
        string[] files = Directory.GetFiles(SpriteRoot, "*.png", SearchOption.AllDirectories);
        int imported = 0;
        foreach (string file in files)
        {
            string assetPath = file.Replace('\\', '/');
            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null) continue;

            bool changed = false;
            if (importer.textureType != TextureImporterType.Sprite) { importer.textureType = TextureImporterType.Sprite; changed = true; }
            if (importer.spriteImportMode != SpriteImportMode.Single) { importer.spriteImportMode = SpriteImportMode.Single; changed = true; }
            if (importer.filterMode != FilterMode.Point) { importer.filterMode = FilterMode.Point; changed = true; }
            if (importer.spritePixelsPerUnit != 96f) { importer.spritePixelsPerUnit = 96f; changed = true; }
            if (importer.textureCompression != TextureImporterCompression.Uncompressed) { importer.textureCompression = TextureImporterCompression.Uncompressed; changed = true; }
            if (importer.mipmapEnabled) { importer.mipmapEnabled = false; changed = true; }
            if (!importer.alphaIsTransparency) { importer.alphaIsTransparency = true; changed = true; }
            TextureImporterSettings settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            if (settings.spriteAlignment != (int)SpriteAlignment.Custom) { settings.spriteAlignment = (int)SpriteAlignment.Custom; changed = true; }
            if (settings.spritePivot != new Vector2(0.5f, 0.08f)) { settings.spritePivot = new Vector2(0.5f, 0.08f); changed = true; }
            if (changed) importer.SetTextureSettings(settings);


            if (changed)
            {
                importer.SaveAndReimport();
                imported++;
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("NEMESIS HERO (2DPIXX CC-BY-4.0): prepared " + files.Length + " sprite frames; reimported " + imported + " textures. States: Idle, Walk, Attack, Hurt, Death; directions: Down, Left, Right, Up.");
    }
}



