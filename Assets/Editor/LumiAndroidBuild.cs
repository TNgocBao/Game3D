using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

[InitializeOnLoad]
public static class LumiAndroidBuild
{
    const string ApkPath="Builds/Android/NarutoAdventure.apk";
    const string Request="Temp/AndroidPrepare.request";
    static LumiAndroidBuild(){EditorApplication.update+=Poll;}
    static void Poll(){if(EditorApplication.isCompiling||EditorApplication.isUpdating||EditorApplication.isPlaying)return;if(!File.Exists(Request))return;File.Delete(Request);Prepare();Directory.CreateDirectory("Logs/AndroidBuild");File.WriteAllText("Logs/AndroidBuild/Preparation.txt","PASS\nAndroid settings prepared. Android Build Support module installed: "+Directory.Exists(Path.Combine(EditorApplication.applicationContentsPath,"PlaybackEngines/AndroidPlayer")));}
    [MenuItem("Naruto/Android/1 - Prepare APK settings")]
    public static void Prepare()
    {
        PlayerSettings.companyName="NGOC BAO";
        PlayerSettings.productName="Naruto Adventure";
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android,"com.ngocbao.narutoadventure");
        PlayerSettings.bundleVersion="0.1.0";
        PlayerSettings.Android.bundleVersionCode=Mathf.Max(1,PlayerSettings.Android.bundleVersionCode);
        PlayerSettings.Android.minSdkVersion=AndroidSdkVersions.AndroidApiLevel22;
        PlayerSettings.Android.targetSdkVersion=AndroidSdkVersions.AndroidApiLevelAuto;
        PlayerSettings.defaultInterfaceOrientation=UIOrientation.AutoRotation;
        PlayerSettings.allowedAutorotateToPortrait=false;
        PlayerSettings.allowedAutorotateToPortraitUpsideDown=false;
        PlayerSettings.allowedAutorotateToLandscapeLeft=true;
        PlayerSettings.allowedAutorotateToLandscapeRight=true;
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android,ScriptingImplementation.IL2CPP);
        PlayerSettings.Android.targetArchitectures=AndroidArchitecture.ARMv7|AndroidArchitecture.ARM64;
        EditorUserBuildSettings.buildAppBundle=false;
        EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene("Assets/3D.unity",true)};
        AssetDatabase.SaveAssets();
        Debug.Log("Android APK settings ready: landscape, IL2CPP, ARMv7 + ARM64, scene Assets/3D.unity.");
    }

    [MenuItem("Naruto/Android/2 - Build APK")]
    public static void BuildApk()
    {
        string module=Path.Combine(EditorApplication.applicationContentsPath,"PlaybackEngines/AndroidPlayer");
        if(!Directory.Exists(module))throw new InvalidOperationException("Android Build Support is not installed for this Unity Editor. Add Android Build Support, Android SDK & NDK Tools, and OpenJDK from Unity Hub.");
        Prepare();Directory.CreateDirectory(Path.GetDirectoryName(ApkPath));
        BuildReport report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"Assets/3D.unity"},locationPathName=ApkPath,target=BuildTarget.Android,options=BuildOptions.None});
        if(report.summary.result!=BuildResult.Succeeded)throw new BuildFailedException("APK build failed: "+report.summary.result);
        Debug.Log("APK built: "+Path.GetFullPath(ApkPath));
        EditorUtility.RevealInFinder(Path.GetFullPath(ApkPath));
    }
}
