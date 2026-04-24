using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

class MyEditorScript
{
    static void PerformBuild()
    {
        // Android SDK/JDK/NDK는 Unity 내장 사용
        EditorPrefs.SetBool("JdkUseEmbedded", true);
        EditorPrefs.SetBool("SdkUseEmbedded", true);
        EditorPrefs.SetBool("NdkUseEmbedded", true);

        string buildDir = "Builds/Android";
        Directory.CreateDirectory(buildDir);

        string fileName = $"NeoProject_{DateTime.Now:yyyyMMdd_HHmm}.apk";
        string buildPath = Path.Combine(buildDir, fileName);

        var report = BuildPipeline.BuildPlayer(
            FindEnabledEditorScenes(),
            buildPath,
            BuildTarget.Android,
            BuildOptions.None
        );

        if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            Debug.Log($"[Build] 성공: {fileName}");
            EditorApplication.Exit(0);
        }
        else
        {
            Debug.LogError($"[Build] 실패: {report.summary.result}");
            foreach (var step in report.steps)
                foreach (var msg in step.messages)
                    if (msg.type == LogType.Error || msg.type == LogType.Exception)
                        Debug.LogError($"[{step.name}] {msg.content}");
            EditorApplication.Exit(1);
        }
    }

    private static string[] FindEnabledEditorScenes()
    {
        List<string> scenes = new List<string>();
        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
            if (scene.enabled) scenes.Add(scene.path);
        return scenes.ToArray();
    }
}