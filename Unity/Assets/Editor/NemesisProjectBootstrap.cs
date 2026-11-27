#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Nemesis.Bootstrap;

namespace Nemesis.EditorTools
{
    /// <summary>
    /// Makes a freshly opened checkout immediately playable by creating the bootstrap scene
    /// and registering it in Build Settings on the first Editor load.
    /// </summary>
    [InitializeOnLoad]
    public static class NemesisProjectBootstrap
    {
        private const string ScenePath = "Assets/Scenes/NemesisPrototype.unity";

        static NemesisProjectBootstrap()
        {
            EditorApplication.delayCall += EnsureProjectScene;
        }

        private static void EnsureProjectScene()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                EditorApplication.delayCall += EnsureProjectScene;
                return;
            }

            bool created = false;
            if (!File.Exists(ScenePath))
            {
                string directory = Path.GetDirectoryName(ScenePath);
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                Scene scene = EditorSceneManager.NewScene(
                    NewSceneSetup.EmptyScene, NewSceneMode.Single);

                GameObject bootstrap = new GameObject("NemesisBootstrap");
                bootstrap.AddComponent<NemesisBootstrap>();

                EditorSceneManager.SaveScene(scene, ScenePath);
                created = true;
            }

            EditorBuildSettingsScene[] current = EditorBuildSettings.scenes;
            bool registered = false;
            foreach (EditorBuildSettingsScene item in current)
            {
                if (item.path == ScenePath)
                {
                    registered = item.enabled;
                    break;
                }
            }

            if (!registered)
            {
                var scenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(current);
                scenes.RemoveAll(item => item.path == ScenePath);
                scenes.Insert(0, new EditorBuildSettingsScene(ScenePath, true));
                EditorBuildSettings.scenes = scenes.ToArray();
            }

            if (created)
                Debug.Log("[Nemesis] First-run scene created at " + ScenePath +
                          ". Press Play to launch the Ashen Reach prototype.");
        }
    }
}
#endif
