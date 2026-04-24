using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Android;
using UnityEngine;

class MyEditorScript
{
    static void PerformBuild()
    {
        string unityRoot = @"C:\Program Files\Unity\Hub\Editor\6000.2.8f1\Editor\Data\PlaybackEngines\AndroidPlayer";

        // JDK는 별도 설치한 Java 17 사용 (Unity 내장 OpenJDK가 구버전이라서)
        AndroidExternalToolsSettings.jdkRootPath = @"C:\Program Files\Eclipse Adoptium\jdk-17.0.18.8-hotspot";
        AndroidExternalToolsSettings.sdkRootPath = Path.Combine(unityRoot, "SDK");
        AndroidExternalToolsSettings.ndkRootPath = Path.Combine(unityRoot, "NDK");

        Debug.Log($"[Build] JDK = {AndroidExternalToolsSettings.jdkRootPath}");
        Debug.Log($"[Build] SDK = {AndroidExternalToolsSettings.sdkRootPath}");
        Debug.Log($"[Build] NDK = {AndroidExternalToolsSettings.ndkRootPath}");

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