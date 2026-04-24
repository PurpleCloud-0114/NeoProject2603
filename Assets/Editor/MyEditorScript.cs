using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

class MyEditorScript
{
    static void PerformBuild()
    {
        // Unity 설치 경로의 내장 JDK/SDK/NDK를 직접 지정
        string unityRoot = @"C:\Program Files\Unity\Hub\Editor\6000.2.8f1\Editor\Data\PlaybackEngines\AndroidPlayer";
        string jdkPath = Path.Combine(unityRoot, "OpenJDK");
        string sdkPath = Path.Combine(unityRoot, "SDK");
        string ndkPath = Path.Combine(unityRoot, "NDK");
        string gradlePath = Path.Combine(unityRoot, "Tools", "gradle");

        EditorPrefs.SetString("JdkPath", jdkPath);
        EditorPrefs.SetString("AndroidSdkRoot", sdkPath);
        EditorPrefs.SetString("AndroidNdkRoot", ndkPath);
        EditorPrefs.SetString("AndroidNdkRootR23b", ndkPath);
        EditorPrefs.SetString("GradlePath", gradlePath);

        // embedded 플래그도 같이
        EditorPrefs.SetBool("JdkUseEmbedded", true);
        EditorPrefs.SetBool("SdkUseEmbedded", true);
        EditorPrefs.SetBool("NdkUseEmbedded", true);
        EditorPrefs.SetBool("GradleUseEmbedded", true);

        Debug.Log($"[Build] JDK: {jdkPath} (exists={Directory.Exists(jdkPath)})");
        Debug.Log($"[Build] SDK: {sdkPath} (exists={Directory.Exists(sdkPath)})");
        Debug.Log($"[Build] NDK: {ndkPath} (exists={Directory.Exists(ndkPath)})");

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