#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Nemesis.EditorTools
{
    /// <summary>
    /// Reproducible Windows x64 build entry point for the Nemesis Unity project.
    /// Run from the Editor menu or with -executeMethod in batch mode.
    /// </summary>
    public static class NemesisBuild
    {
        private const string ScenePath = "Assets/Scenes/NemesisPrototype.unity";
        private const string OutputDirectory = "Build";
        private const string OutputPath = OutputDirectory + "/Nemesis.exe";

        [MenuItem("Tools/Nemesis/Build Windows x64")]
        public static void BuildWindows()
        {
            PerformBuild();
            EditorUtility.DisplayDialog("Nemesis Build", "Windows build finished. Check the Build folder.", "OK");
        }

        public static void PerformBuild()
        {
            EnsurePrototypeScene();

            EditorUserBuildSettings.SwitchActiveBuildTarget(
                BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64);

            PlayerSettings.productName = "Nemesis";
            PlayerSettings.companyName = "Nemesis Team";
            PlayerSettings.defaultScreenWidth = 1600;
            PlayerSettings.defaultScreenHeight = 900;
            PlayerSettings.runInBackground = true;

            Directory.CreateDirectory(OutputDirectory);
            var options = new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = OutputPath,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
            {
                throw new BuildFailedException(
                    "Nemesis Windows build failed: " + report.summary.result +
                    ". Errors: " + report.summary.totalErrors);
            }

            Debug.Log("Nemesis Windows build created: " + Path.GetFullPath(OutputPath) +
                      " (" + report.summary.totalSize + " bytes)");
        }

        private static void EnsurePrototypeScene()
        {
            if (File.Exists(ScenePath))
                return;

            string directory = Path.GetDirectoryName(ScenePath);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            Scene scene = EditorSceneManager.NewScene(
                NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var bootstrap = new GameObject("NemesisBootstrap");
            bootstrap.AddComponent<Nemesis.Bootstrap.NemesisBootstrap>();
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.Refresh();
        }
    }
}
#endif
