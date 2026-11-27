#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

public static class NemesisUIAssetInstaller
{
    private const string ResourceFolder = "Assets/Resources/NemesisUI";

    private static readonly string[,] Files =
    {
        { "Assets/ThirdParty/NemesisFreeCC0/gothic/assets/characters/gothic-hooded-monk.png", "Portrait_HoodedMonk.png" },
        { "Assets/ThirdParty/NemesisFreeCC0/gothic/assets/buildings/gothic-cathedral-facade.png", "Backdrop_Cathedral.png" },
        { "Assets/ThirdParty/NemesisFreeCC0/gothic/assets/items/gothic-holy-chalice.png", "Icon_HolyChalice.png" },
        { "Assets/ThirdParty/NemesisFreeCC0/gothic/assets/items/gothic-silver-key.png", "Icon_SilverKey.png" },
        { "Assets/ThirdParty/NemesisFreeCC0/gothic/assets/items/gothic-sacred-book.png", "Icon_SacredBook.png" },
        { "Assets/ThirdParty/NemesisFreeCC0/gothic/assets/items/gothic-rosary.png", "Icon_Rosary.png" },
        { "Assets/ThirdParty/NemesisFreeCC0/gothic/assets/items/gothic-votive-candles.png", "Icon_VotiveCandles.png" },
        { "Assets/ThirdParty/NemesisFreeCC0/gothic/assets/enemies/gothic-shadow-specter.png", "Icon_ShadowSpecter.png" },
        { "Assets/ThirdParty/NemesisFreeCC0/gothic/assets/enemies/gothic-bone-revenant.png", "Icon_BoneRevenant.png" },
        { "Assets/ThirdParty/NemesisFreeCC0/gothic/assets/enemies/gothic-fallen-angel.png", "Icon_FallenAngel.png" },
        { "Assets/ThirdParty/NemesisFreeCC0/dungeon/assets/items/dungeon-magic-crystal.png", "Icon_MagicCrystal.png" }
    };

    [MenuItem("Nemesis/Install UI Art Resources")]
    public static void Install()
    {
        Directory.CreateDirectory(ResourceFolder);
        for (int i = 0; i < Files.GetLength(0); i++)
        {
            string source = Files[i, 0];
            string destination = ResourceFolder + "/" + Files[i, 1];
            if (!File.Exists(source))
            {
                Debug.LogWarning("NEMESIS UI: missing art source " + source);
                continue;
            }

            if (!File.Exists(destination) || File.GetLastWriteTimeUtc(source) > File.GetLastWriteTimeUtc(destination))
                File.Copy(source, destination, true);
        }

        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

        for (int i = 0; i < Files.GetLength(0); i++)
        {
            string path = ResourceFolder + "/" + Files[i, 1];
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) continue;

            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.filterMode = FilterMode.Point;
            importer.spritePixelsPerUnit = 32;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.SaveAndReimport();
        }

        AssetDatabase.SaveAssets();
        Debug.Log("NEMESIS UI: imported " + Files.GetLength(0) + " pixel-art interface sprites.");
    }
}
#endif
