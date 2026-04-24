using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

class MyEditorScript
{
    static void PerformBuild()
    {
        // 1. 저장 경로 설정
        string buildDir = "Builds/Android";
        if (!Directory.Exists(buildDir)) Directory.CreateDirectory(buildDir);

        // 2. 구버전 빌드 파일 정리 (10개 유지)
        CleanUpOldBuilds(buildDir, 10);

        // 3. 파일명 설정 (예: NeoProject_20260424_1150.apk)
        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmm");
        string fileName = $"NeoProject_{timestamp}.apk";
        string buildPath = Path.Combine(buildDir, fileName);

        // 4. 빌드 옵션 구성
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = GetEnabledScenes();
        buildPlayerOptions.locationPathName = buildPath;
        buildPlayerOptions.target = BuildTarget.Android;
        buildPlayerOptions.options = BuildOptions.None;

        // 5. 빌드 실행 및 결과 확인
        Debug.Log($"[Jenkins Build] 시작: {fileName}");
        var report = BuildPipeline.BuildPlayer(buildPlayerOptions);

        if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            Debug.Log("[Jenkins Build] 성공!");
        }
        else
        {
            Debug.LogError("[Jenkins Build] 실패!");
            EditorApplication.Exit(1); // 젠킨스에 에러 신호 전달
        }
    }

    private static void CleanUpOldBuilds(string path, int maxCount)
    {
        DirectoryInfo info = new DirectoryInfo(path);
        if (!info.Exists) return;

        var files = info.GetFiles("*.apk")
                        .OrderBy(f => f.CreationTime)
                        .ToList();

        if (files.Count >= maxCount)
        {
            int deleteCount = files.Count - maxCount + 1;
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