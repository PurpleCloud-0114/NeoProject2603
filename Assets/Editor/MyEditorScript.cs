using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

class MyEditorScript
{
    static void PerformBuild()
    {
        string unityRoot = @"C:\Program Files\Unity\Hub\Editor\6000.2.8f1\Editor\Data\PlaybackEngines\AndroidPlayer";
        string jdkPath = Path.Combine(unityRoot, "OpenJDK");
        string sdkPath = Path.Combine(unityRoot, "SDK");
        string ndkPath = Path.Combine(unityRoot, "NDK");

        // 프로세스 환경변수에 직접 주입 (빌드 파이프라인이 이걸 봄)
        Environment.SetEnvironmentVariable("JAVA_HOME", jdkPath, EnvironmentVariableTarget.Process);
        Environment.SetEnvironmentVariable("ANDROID_HOME", sdkPath, EnvironmentVariableTarget.Process);
        Environment.SetEnvironmentVariable("ANDROID_SDK_ROOT", sdkPath, EnvironmentVariableTarget.Process);
        Environment.SetEnvironmentVariable("ANDROID_NDK_ROOT", ndkPath, EnvironmentVariableTarget.Process);

        // EditorPrefs도 같이 (보조)
        EditorPrefs.SetString("JdkPath", jdkPath);
        EditorPrefs.SetString("AndroidSdkRoot", sdkPath);
        EditorPrefs.SetString("AndroidNdkRoot", ndkPath);
        EditorPrefs.SetString("AndroidNdkRootR23b", ndkPath);

        Debug.Log($"[Build] JAVA_HOME = {Environment.GetEnvironmentVariable("JAVA_HOME")}");
        Debug.Log($"[Build] ANDROID_HOME = {Environment.GetEnvironmentVariable("ANDROID_HOME")}");
        Debug.Log($"[Build] ANDROID_NDK_ROOT = {Environment.GetEnvironmentVariable("ANDROID_NDK_ROOT")}");

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