#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using Nemesis.Presentation;

namespace Nemesis.EditorTools
{
    public static class NemesisAssetLibraryBuilder
    {
        private const string Root = "Assets/NemesisContent";
        private const string Prefabs = Root + "/Prefabs";
        private const string Materials = Root + "/Materials";

        [MenuItem("Tools/Nemesis/Generate Asset Library")]
        public static void Generate()
        {
            EnsureFolder("Assets", "NemesisContent");
            EnsureFolder(Root, "Prefabs");
            EnsureFolder(Root, "Materials");

            CreateCharacter("Player_Armored", NemesisCharacterStyle.Player);
            CreateCharacter("Ash_Raider", NemesisCharacterStyle.Raider);
            CreateCharacter("Ashen_Warlord_Boss", NemesisCharacterStyle.Boss);
            CreateCharacter("Ashen_Merchant_NPC", NemesisCharacterStyle.Merchant);

            // Equipment
            CreateProp("Iron_Helm", PrimitiveType.Sphere, new Vector3(.62f,.38f,.58f), new Color(.38f,.40f,.45f));
            CreateProp("Ashen_Chestplate", PrimitiveType.Cube, new Vector3(.72f,.78f,.38f), new Color(.20f,.18f,.24f));
            CreateProp("Warden_Greatsword", PrimitiveType.Cube, new Vector3(.14f,1.55f,.10f), new Color(.62f,.67f,.72f));
            CreateProp("Ashen_Shield", PrimitiveType.Cylinder, new Vector3(.72f,.16f,.95f), new Color(.25f,.31f,.38f));
            CreateProp("Iron_Greaves", PrimitiveType.Capsule, new Vector3(.28f,.72f,.30f), new Color(.34f,.36f,.40f));
            CreateProp("Bone_Dagger", PrimitiveType.Cube, new Vector3(.08f,.72f,.07f), new Color(.78f,.76f,.65f));
            CreateProp("Ashen_Bow", PrimitiveType.Cylinder, new Vector3(.10f,1.1f,.10f), new Color(.42f,.25f,.16f));
            CreateProp("Warden_Helmet_Crest", PrimitiveType.Cylinder, new Vector3(.3f,.55f,.3f), new Color(.55f,.12f,.14f));

            // Environment and dungeon kit
            CreateProp("Ruined_Pillar", PrimitiveType.Cube, new Vector3(1.1f,3.2f,1.1f), new Color(.22f,.21f,.24f));
            CreateProp("Ancient_Obelisk", PrimitiveType.Cube, new Vector3(.8f,2.8f,.8f), new Color(.29f,.22f,.17f));
            CreateProp("Soul_Flame", PrimitiveType.Sphere, new Vector3(.55f,.55f,.55f), new Color(.12f,.72f,.95f));
            CreateProp("Crypt_Sarcophagus", PrimitiveType.Cube, new Vector3(1.5f,.65f,2.4f), new Color(.25f,.23f,.28f));
            CreateProp("Broken_Arch_Left", PrimitiveType.Cube, new Vector3(.55f,3.2f,.7f), new Color(.24f,.22f,.25f));
            CreateProp("Dungeon_Stone_Block", PrimitiveType.Cube, new Vector3(2f,1f,2f), new Color(.31f,.29f,.32f));
            CreateProp("Rune_Stone", PrimitiveType.Cylinder, new Vector3(1.1f,.18f,1.1f), new Color(.35f,.13f,.48f));
            CreateProp("Gravestone", PrimitiveType.Cube, new Vector3(.75f,1.25f,.22f), new Color(.30f,.29f,.32f));
            CreateProp("Ashen_Barrel", PrimitiveType.Cylinder, new Vector3(.65f,.9f,.65f), new Color(.31f,.19f,.12f));
            CreateProp("Supply_Crate", PrimitiveType.Cube, new Vector3(.85f,.85f,.85f), new Color(.42f,.29f,.17f));
            CreateProp("Wooden_Barricade", PrimitiveType.Cube, new Vector3(2.2f,.8f,.22f), new Color(.28f,.17f,.11f));
            CreateProp("Lantern_Stand", PrimitiveType.Cylinder, new Vector3(.22f,1.8f,.22f), new Color(.23f,.20f,.19f));
            CreateProp("Corrupted_Heart", PrimitiveType.Sphere, new Vector3(.9f,.9f,.9f), new Color(.38f,.035f,.12f));
            CreateProp("Ashen_Tree_Trunk", PrimitiveType.Cylinder, new Vector3(.55f,2.8f,.55f), new Color(.25f,.16f,.12f));
            CreateProp("Ashen_Tree_Canopy", PrimitiveType.Sphere, new Vector3(2.2f,2f,2.2f), new Color(.18f,.29f,.19f));
            CreateProp("Dungeon_Floor_Tile", PrimitiveType.Cube, new Vector3(2f,.18f,2f), new Color(.27f,.25f,.29f));

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("Nemesis Asset Library",
                "Generated free, original prototype prefabs and materials in Assets/NemesisContent. See Assets/NemesisContent/README.md for scope and licensing notes.", "OK");
        }

        private static void CreateCharacter(string name, NemesisCharacterStyle style)
        {
            string path = Prefabs + "/" + name + ".prefab";
            if (File.Exists(path)) AssetDatabase.DeleteAsset(path);
            GameObject instance = NemesisCharacterVisual.Create(name, style);
            MaterializeMaterials(instance, name);
            PrefabUtility.SaveAsPrefabAsset(instance, path);
            Object.DestroyImmediate(instance);
        }

        private static void MaterializeMaterials(GameObject root, string prefix)
        {
            int index = 0;
            foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(true))
            {
                Color color = renderer.sharedMaterial != null ? renderer.sharedMaterial.color : Color.white;
                var material = new Material(FindLitShader());
                material.color = color;
                string materialPath = Materials + "/" + prefix + "_" + (index++) + ".mat";
                if (File.Exists(materialPath)) AssetDatabase.DeleteAsset(materialPath);
                AssetDatabase.CreateAsset(material, materialPath);
                renderer.sharedMaterial = material;
            }
        }

        private static void CreateProp(string name, PrimitiveType type, Vector3 scale, Color color)
        {
            string path = Prefabs + "/" + name + ".prefab";
            if (File.Exists(path)) AssetDatabase.DeleteAsset(path);
            GameObject go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.localScale = scale;
            var material = new Material(FindLitShader());
            material.color = color;
            string materialPath = Materials + "/" + name + ".mat";
            if (File.Exists(materialPath)) AssetDatabase.DeleteAsset(materialPath);
            AssetDatabase.CreateAsset(material, materialPath);
            go.GetComponent<Renderer>().sharedMaterial = material;
            PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
        }

        private static Shader FindLitShader()
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");
            if (shader == null) shader = Shader.Find("Unlit/Color");
            return shader;
        }

        private static void EnsureFolder(string parent, string name)
        {
            string path = parent + "/" + name;
            if (!AssetDatabase.IsValidFolder(path)) AssetDatabase.CreateFolder(parent, name);
        }
    }

}
#endif
