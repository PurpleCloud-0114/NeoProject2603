using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

class MyEditorScript
{
    static void PerformBuild()
    {
        Debug.Log("[Jenkins Build] 빌드 전 환경 설정 시작...");

        // 1. Android SDK/JDK/NDK는 Unity 내장 사용
        EditorPrefs.SetBool("JdkUseEmbedded", true);
        EditorPrefs.SetBool("SdkUseEmbedded", true);
        EditorPrefs.SetBool("NdkUseEmbedded", true);

        // 2. 저장 경로 설정
        string buildDir = "Builds/Android";
        Directory.CreateDirectory(buildDir);

        // 3. 파일명 설정 (예: NeoProject_20260424_1150.apk)
        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmm");
        string fileName = $"NeoProject_{timestamp}.apk";
        string buildPath = Path.Combine(buildDir, fileName);

        // 4. 빌드 씬 확인 (없으면 즉시 실패)
        string[] scenes = GetEnabledScenes();
        if (scenes.Length == 0)
        {
            Debug.LogError("[Jenkins Build] Build Settings에 활성화된 씬이 없습니다!");
            EditorApplication.Exit(1);
            return;
        }
        Debug.Log($"[Jenkins Build] 빌드 씬 {scenes.Length}개:");
        foreach (var s in scenes) Debug.Log($"  - {s}");

        // 5. 빌드 옵션 구성
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = buildPath,
            target = BuildTarget.Android,
            options = BuildOptions.None
        };

        // 6. 빌드 실행
        Debug.Log($"[Jenkins Build] 시작: {fileName}");
        var report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        var summary = report.summary;

        if (summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            Debug.Log($"[Jenkins Build] 성공! 크기: {summary.totalSize / (1024 * 1024)} MB, 시간: {summary.totalTime}");
            // 성공한 후에 구버전 정리
            CleanUpOldBuilds(buildDir, 10);
            EditorApplication.Exit(0);
        }
        else
        {
            Debug.LogError($"[Jenkins Build] 실패! Result={summary.result}, " +
                           $"TotalErrors={summary.totalErrors}, TotalWarnings={summary.totalWarnings}");

            // 단계별 에러/경고 풀어서 출력
            foreach (var step in report.steps)
            {
                foreach (var msg in step.messages)
                {
                    if (msg.type == LogType.Error || msg.type == LogType.Exception)
                        Debug.LogError($"[{step.name}] {msg.content}");
                    else if (msg.type == LogType.Warning)
                        Debug.LogWarning($"[{step.name}] {msg.content}");
                }
            }
            EditorApplication.Exit(1);
        }
    }

    private static void CleanUpOldBuilds(string path, int maxCount)
    {
        DirectoryInfo info = new DirectoryInfo(path);
        if (!info.Exists) return;

        var files = info.GetFiles("*.apk")
                        .OrderBy(f => f.CreationTime)
                        .ToList();

        if (files.Count > maxCount)
        {
            int deleteCount = files.Count - maxCount;
            for (int i = 0; i < deleteCount; i++)
            {
                Debug.Log($"[Cleanup] 삭제된 구버전 빌드: {files[i].Name}");
                files[i].Delete();
            }
        }
    }

    private static string[] GetEnabledScenes()
    {
        return EditorBuildSettings.scenes
            .Where(s => s.enabled)
            .Select(s => s.path)
            .ToArray();
    }
}