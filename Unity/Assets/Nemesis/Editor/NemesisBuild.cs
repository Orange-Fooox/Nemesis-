using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using System.IO;

public static class NemesisBuild
{
    [MenuItem("Nemesis/Build Complete Windows Game")]
    public static void BuildCompleteWindowsGame()
    {
        NemesisWorldBuilder.BuildAllScenes();
        BuildWindows();
    }

    [MenuItem("Nemesis/Build Windows")]
    public static void BuildWindows()
    {
        string root = Directory.GetCurrentDirectory();
        string output = Path.Combine(root, "Builds", "Pechat-Bezdny-Windows");
        Directory.CreateDirectory(output);
        var options = new BuildPlayerOptions
        {
            scenes = new[] { "Assets/Nemesis/Scenes/AshenOutskirts.unity", "Assets/Nemesis/Scenes/AshenDungeon.unity" },
            locationPathName = Path.Combine(output, "Pechat_Bezdny.exe"),
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None
        };
        BuildReport report = BuildPipeline.BuildPlayer(options);
        Debug.Log("NEMESIS BUILD RESULT: " + report.summary.result + " | " + report.summary.totalSize + " bytes | " + report.summary.totalTime);
        if (report.summary.result != BuildResult.Succeeded)
            throw new System.Exception("Windows build failed: " + report.summary.result);
    }
}
