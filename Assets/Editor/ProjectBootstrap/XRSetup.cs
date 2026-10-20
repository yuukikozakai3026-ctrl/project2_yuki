using System.IO;
using UnityEditor;
using UnityEditor.XR.Management;
using UnityEditor.XR.Management.Metadata;
using UnityEngine;
using UnityEngine.XR.Management;
using UnityEngine.XR.OpenXR;
using UnityEngine.XR.OpenXR.Features.Interactions;
using UnityEngine.XR.OpenXR.Features.MetaQuestSupport;
public static class XRSetup {
 public static void Configure(){
 const string path="Assets/VRBat/XRSettings.asset";
 var settings=AssetDatabase.LoadAssetAtPath<XRGeneralSettingsPerBuildTarget>(path);
 if(!settings){settings=ScriptableObject.CreateInstance<XRGeneralSettingsPerBuildTarget>();AssetDatabase.CreateAsset(settings,path);}
 EditorBuildSettings.AddConfigObject(XRGeneralSettings.settingsKey,settings,true);
 foreach(var group in new[]{BuildTargetGroup.Standalone,BuildTargetGroup.Android}){
 if(!settings.HasManagerSettingsForBuildTarget(group))settings.CreateDefaultManagerSettingsForBuildTarget(group);
 var manager=settings.ManagerSettingsForBuildTarget(group);XRPackageMetadataStore.AssignLoader(manager,"UnityEngine.XR.OpenXR.OpenXRLoader",group);
 settings.SettingsForBuildTarget(group).InitManagerOnStart=group==BuildTargetGroup.Android;
 var xr=OpenXRSettings.GetSettingsForBuildTargetGroup(group);
 if(!xr)throw new System.InvalidOperationException("OpenXR settings are missing for "+group);
 foreach(var feature in xr.GetFeatures<UnityEngine.XR.OpenXR.Features.OpenXRFeature>()){
 if(feature is OculusTouchControllerProfile||feature is KHRSimpleControllerProfile)feature.enabled=true;
 if(feature is ValveIndexControllerProfile)feature.enabled=group==BuildTargetGroup.Standalone;
 if(group==BuildTargetGroup.Android&&feature is MetaQuestTouchPlusControllerProfile)feature.enabled=true;
 EditorUtility.SetDirty(feature);
 }
 if(group==BuildTargetGroup.Android){
 var quest=xr.GetFeature<MetaQuestFeature>();
 if(!quest)throw new System.InvalidOperationException("Meta Quest Support feature is missing");
 quest.enabled=true;
 EditorUtility.SetDirty(quest);
 Debug.Log("QUEST_IMMERSIVE_FEATURE_ENABLED "+quest.enabled);
 }
 EditorUtility.SetDirty(xr);
 }
 EditorUtility.SetDirty(settings);AssetDatabase.SaveAssets();Debug.Log("XR_CONFIGURED");
 }
}
