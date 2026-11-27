#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Nemesis.Bootstrap;

namespace Nemesis.EditorTools
{
    public static class NemesisSceneSetup
    {
        [MenuItem("Tools/Nemesis/Create Prototype Scene")]
        public static void CreatePrototypeScene()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            GameObject bootstrap = new GameObject("NemesisBootstrap");
            bootstrap.AddComponent<NemesisBootstrap>();

            if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
                AssetDatabase.CreateFolder("Assets", "Scenes");

            EditorSceneManager.SaveScene(scene, "Assets/Scenes/NemesisPrototype.unity");
            EditorUtility.DisplayDialog("Nemesis", "Prototype scene created. Press Play to run the greybox.", "OK");
        }
    }
}
#endif
