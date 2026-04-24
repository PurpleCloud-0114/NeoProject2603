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
        Debug.Log("[Jenkins Build] 빌드 전 환경 설정 시작...");

        // 1. Android SDK/JDK 경로 강제 설정 (젠킨스 SYSTEM 계정 대응)
        // 유니티 6000.2.8f1 설치 경로를 기준으로 합니다.
        string unityEditorPath = Path.GetDirectoryName(EditorApplication.applicationPath);
        string androidPlayerPath = Path.Combine(unityEditorPath, "Data", "PlaybackEngines", "AndroidPlayer");

        string jdkPath = Path.Combine(androidPlayerPath, "OpenJDK");
        string sdkPath = Path.Combine(androidPlayerPath, "SDK");
        string ndkPath = Path.Combine(androidPlayerPath, "NDK");

        // 유니티 내부 환경 설정에 경로 주입
        EditorPrefs.SetString("JdkUseEmbedded", "true");
        EditorPrefs.SetString("AndroidJdkRoot", jdkPath);
        EditorPrefs.SetString("AndroidSdkRoot", sdkPath);
        EditorPrefs.SetString("AndroidNdkRoot", ndkPath);

        Debug.Log($"[Jenkins Build] JDK Path: {jdkPath}");
        Debug.Log($"[Jenkins Build] SDK Path: {sdkPath}");

        // 2. 저장 경로 설정
        string buildDir = "Builds/Android";
        if (!Directory.Exists(buildDir)) Directory.CreateDirectory(buildDir);

        // 3. 구버전 빌드 파일 정리 (10개 유지)
        CleanUpOldBuilds(buildDir, 10);

        // 4. 파일명 설정 (예: NeoProject_20260424_1150.apk)
        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmm");
        string fileName = $"NeoProject_{timestamp}.apk";
        string buildPath = Path.Combine(buildDir, fileName);

        // 5. 빌드 옵션 구성
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = GetEnabledScenes();
        buildPlayerOptions.locationPathName = buildPath;
        buildPlayerOptions.target = BuildTarget.Android;
        buildPlayerOptions.options = BuildOptions.None;

        // 6. 빌드 실행 및 결과 확인
        Debug.Log($"[Jenkins Build] 시작: {fileName}");
        var report = BuildPipeline.BuildPlayer(buildPlayerOptions);

        if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            Debug.Log("[Jenkins Build] 성공!");
        }
        else
        {
            Debug.LogError("[Jenkins Build] 실패!");
            // 상세 에러 확인을 위해 리포트 출력 유도 후 종료
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