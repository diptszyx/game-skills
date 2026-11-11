using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class SmokeBuild
{
    public static void BuildAndroid()
    {
        if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.Android, BuildTarget.Android))
            throw new InvalidOperationException("Android build support is unavailable for this Editor installation.");
        Directory.CreateDirectory("Assets/Scenes");
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        new GameObject("Camera", typeof(Camera));
        EditorSceneManager.SaveScene(scene, "Assets/Scenes/Smoke.unity");
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.indiegame.shell.smoke");
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;        EditorUserBuildSettings.buildAppBundle = false;
        Directory.CreateDirectory("Builds");
        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
            scenes = new[] { "Assets/Scenes/Smoke.unity" }, locationPathName = "Builds/smoke.apk",
            target = BuildTarget.Android, options = BuildOptions.Development
        });
        if (report.summary.result != BuildResult.Succeeded)
            throw new Exception("APK build failed: " + report.summary.result + ", errors=" + report.summary.totalErrors);
        Debug.Log("SMOKE_APK_BYTES=" + new FileInfo("Builds/smoke.apk").Length);
    }
}
