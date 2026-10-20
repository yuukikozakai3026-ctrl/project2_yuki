using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
public static class PackageInstaller {
 static AddAndRemoveRequest request;
 public static void Install() {
 request=Client.AddAndRemove(new[]{"com.unity.xr.openxr","com.unity.xr.management","com.unity.inputsystem","com.unity.cloud.gltfast"});
 EditorApplication.update+=Poll;
 }
 static void Poll(){ if(!request.IsCompleted)return; EditorApplication.update-=Poll; Debug.Log(request.Status==StatusCode.Success?"PACKAGES_READY":request.Error.message); EditorApplication.Exit(request.Status==StatusCode.Success?0:1); }
}
