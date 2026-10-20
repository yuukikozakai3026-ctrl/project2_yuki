using System;
using System.IO;
using UnityEditor;
using UnityEngine;
public static class BuildProject {
 public static void Build(){ManualGameplayChecks.Planner();ManualGameplayChecks.BuildMac();if(Application.isBatchMode)EditorApplication.Exit(0);}
 public static void BuildWindows(){
  ManualGameplayChecks.Planner();XRSetup.Configure();Directory.CreateDirectory("Builds/Windows");
  var result=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"Assets/Scenes/HomeScene.unity","Assets/Scenes/GameScene.unity"},locationPathName="Builds/Windows/BatCarnival.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.None});
  Debug.Log("WINDOWS_BUILD_RESULT "+result.summary.result);
  if(result.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new Exception("Windows build failed");
  if(Application.isBatchMode)EditorApplication.Exit(0);
 }
 public static void BuildAndroid(){
  if(!EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android,BuildTarget.Android))
   throw new Exception("Android build target is not available");
  ManualGameplayChecks.Planner();
  XRSetup.Configure();
  PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android,"com.vrbat.carnival");
  PlayerSettings.bundleVersion="0.1.0";
  PlayerSettings.Android.bundleVersionCode=1;
  PlayerSettings.Android.minSdkVersion=AndroidSdkVersions.AndroidApiLevel26;
  PlayerSettings.Android.targetArchitectures=AndroidArchitecture.ARM64;
  PlayerSettings.Android.androidIsGame=true;
  PlayerSettings.Android.applicationEntry=AndroidApplicationEntry.GameActivity;
  PlayerSettings.defaultInterfaceOrientation=UIOrientation.LandscapeLeft;
  EditorUserBuildSettings.exportAsGoogleAndroidProject=false;
  Directory.CreateDirectory("Builds/Android");
  var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{
   scenes=new[]{"Assets/Scenes/HomeScene.unity","Assets/Scenes/GameScene.unity"},
   locationPathName="Builds/Android/BatCarnival-Quest.apk",
   target=BuildTarget.Android,
   options=BuildOptions.None
  });
  Debug.Log("ANDROID_BUILD_RESULT "+report.summary.result);
  if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new Exception("Android build failed");
  if(Application.isBatchMode)EditorApplication.Exit(0);
 }
}
