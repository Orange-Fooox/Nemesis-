using System.IO;
using UnityEditor;
using UnityEngine;

public static class NemesisInfiniteWorldInstaller
{
    private const string ResourceRoot = "Assets/Resources/NemesisWorld";

    [MenuItem("Nemesis/Install Infinite World Runtime Assets")]
    public static void Install()
    {
        EnsureFolder("Assets/Resources");
        EnsureFolder(ResourceRoot);
        EnsureFolder(ResourceRoot + "/Terrain");
        EnsureFolder(ResourceRoot + "/Props");
        EnsureFolder(ResourceRoot + "/Enemies");

        Copy("Assets/ThirdParty/NemesisFreeCC0/dungeon/assets/tilesets/dungeon-cobble-floor-32.png", ResourceRoot + "/Terrain/Cobble.png");
        Copy("Assets/ThirdParty/NemesisFreeCC0/dungeon/assets/tilesets/dungeon-cracked-floor-32.png", ResourceRoot + "/Terrain/Cracked.png");
        Copy("Assets/ThirdParty/NemesisFreeCC0/dungeon/assets/tilesets/dungeon-stone-floor-32.png", ResourceRoot + "/Terrain/Stone.png");
        Copy("Assets/ThirdParty/NemesisFreeCC0/dungeon/assets/tilesets/dungeon-lava-tile-32.png", ResourceRoot + "/Terrain/Lava.png");

        Copy("Assets/ThirdParty/NemesisFreeCC0/forest/assets/environment/forest-oak-tree.png", ResourceRoot + "/Props/OakTree.png");
        Copy("Assets/ThirdParty/NemesisFreeCC0/forest/assets/environment/forest-pine-tree.png", ResourceRoot + "/Props/PineTree.png");
        Copy("Assets/ThirdParty/NemesisFreeCC0/forest/assets/environment/forest-dead-tree.png", ResourceRoot + "/Props/DeadTree.png");
        Copy("Assets/ThirdParty/NemesisFreeCC0/forest/assets/environment/forest-large-bush.png", ResourceRoot + "/Props/Bush.png");
        Copy("Assets/ThirdParty/NemesisFreeCC0/forest/assets/environment/forest-boulder-lg.png", ResourceRoot + "/Props/Boulder.png");
        Copy("Assets/ThirdParty/NemesisFreeCC0/gothic/assets/environment/gothic-stone-tomb.png", ResourceRoot + "/Props/Tomb.png");

        Copy("Assets/ThirdParty/NemesisFreeCC0/gothic/assets/enemies/gothic-shadow-specter.png", ResourceRoot + "/Enemies/Specter.png");
        Copy("Assets/ThirdParty/NemesisFreeCC0/gothic/assets/enemies/gothic-bone-revenant.png", ResourceRoot + "/Enemies/Revenant.png");
        Copy("Assets/ThirdParty/NemesisFreeCC0/gothic/assets/enemies/gothic-demon-bat.png", ResourceRoot + "/Enemies/Bat.png");
        Copy("Assets/ThirdParty/NemesisFreeCC0/gothic/assets/enemies/gothic-gargoyle-awakened.png", ResourceRoot + "/Enemies/Gargoyle.png");

        AssetDatabase.Refresh();
        ConfigureSprites();
        AssetDatabase.SaveAssets();
        Debug.Log("NEMESIS WORLD: copied and configured runtime Resources for infinite chunks (4 ground tiles, 6 props, 4 enemy sprites).");
    }

    private static void Copy(string source, string destination)
    {
        if (!File.Exists(source))
        {
            Debug.LogError("NEMESIS INFINITE WORLD: source asset missing: " + source);
            return;
        }
        if (File.Exists(destination)) File.Delete(destination);
        File.Copy(source, destination, true);
    }

    private static void ConfigureSprites()
    {
        string[] files = Directory.GetFiles(ResourceRoot, "*.png", SearchOption.AllDirectories);
        foreach (string file in files)
        {
            string path = file.Replace('\\', '/');
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) continue;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.filterMode = FilterMode.Point;
            importer.spritePixelsPerUnit = 32f;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.SaveAndReimport();
        }
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        string parent = Path.GetDirectoryName(path).Replace('\\', '/');
        string folder = Path.GetFileName(path);
        if (!AssetDatabase.IsValidFolder(parent))
            EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, folder);
    }
}