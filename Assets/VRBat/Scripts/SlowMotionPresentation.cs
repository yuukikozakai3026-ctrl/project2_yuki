using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace VRBat {
public partial class BatGame {
 public double effectTime; public float effectScale=1,effectDelta;
 public float SoundTimeScale=>Mathf.Lerp(.55f,1,effectScale);
 bool slowPresentation; double lastEffectFrame,lastClockSample;float slowOpacity;CanvasGroup slowGroup;
 readonly List<LineRenderer> slowCorners=new List<LineRenderer>();
 Material slowCornerMaterial;public SlowViewTint slowViewTint;
 void SetupSlowMotion(){
  slowViewTint=SlowViewTint.Create(view.eye);
  var panel=view.Panel("Slow motion status",new Vector3(-2.85f,2.28f,4.5f),new Vector2(1.60f,.23f),view.near);
  foreach(var image in panel.GetComponentsInChildren<Image>())if(image.name=="Gold top line")image.color=BatGeometry.ColorOf("#79E9FF");
  slowGroup=panel.gameObject.AddComponent<CanvasGroup>();slowGroup.alpha=0;slowGroup.interactable=false;slowGroup.blocksRaycasts=false;
  var label=view.Label(panel,"◀  S L O W  ▶",Vector2.zero,new Vector2(1.50f,.17f),.085f);label.color=BatGeometry.ColorOf("#79E9FF");
  slowCornerMaterial=new Material(Resources.Load<Shader>("FestivalParticles"));slowCornerMaterial.SetTexture("_MainTex",Texture2D.whiteTexture);slowCornerMaterial.SetColor("_TintColor",new Color(.25f,.75f,.85f,1));
  // Peripheral HUD marks at a comfortable stereo depth, outside the batting area.
  for(int i=0;i<4;i++){
   float x=i%2==0?-1:1,y=i<2?1:-1;
   var line=BatGeometry.Line("Slow focus corner",new[]{new Vector3(x*1.95f,y*.69f,2),new Vector3(x*1.95f,y*.82f,2),new Vector3(x*1.72f,y*.82f,2)},.006f,slowCornerMaterial,view.eye.transform);
   line.useWorldSpace=false;line.enabled=false;slowCorners.Add(line);
  }
 }
 void ResetPresentation(){effectScale=1;slowPresentation=false;slowOpacity=0;if(slowViewTint)slowViewTint.SetAmount(0);lastEffectFrame=effectTime;lastClockSample=now;}
 void AdvancePresentationClock(float dt){
  bool active=phase=="play"&&ManualNear;
  if(active!=slowPresentation){slowPresentation=active;Event(active?"world_slow_enter":"world_slow_exit",$"fx_time={effectTime:F5}");}
  effectScale=Mathf.MoveTowards(effectScale,active?.20f:1,dt*(active?8:10));
  effectTime+=dt*effectScale;
  if((captureSequence||quitAfter)&&now-lastClockSample>=.25){lastClockSample=now;Event("effect_clock",$"fx_time={effectTime:F5};scale={effectScale:F3};slow={active}");}
 }
 void SyncPresentationFrame(){
  effectDelta=(float)(effectTime-lastEffectFrame);lastEffectFrame=effectTime;
  if(!paused)slowOpacity=Mathf.MoveTowards(slowOpacity,slowPresentation?1:0,Time.unscaledDeltaTime*8);
  if(slowGroup)slowGroup.alpha=slowOpacity;
  if(slowViewTint)slowViewTint.SetAmount(slowOpacity);
  foreach(var corner in slowCorners){corner.enabled=slowOpacity>.01f;corner.startColor=corner.endColor=new Color(1,1,1,slowOpacity*.42f);}
  if(view.music){view.music.pitch=SoundTimeScale;if(paused&&view.music.isPlaying)view.music.Pause();else if(!paused&&!view.music.isPlaying)view.music.UnPause();}
 }
}
public partial class Effects {
 readonly Dictionary<AudioSource,float> voicePitches=new Dictionary<AudioSource,float>();bool voicesPaused;
 IEnumerator EffectWait(float duration){double end=game.effectTime+duration;while(game.effectTime<end)yield return null;}
 void UpdateVoices(){
  foreach(var pair in voicePitches){if(!pair.Key)continue;pair.Key.pitch=pair.Value*game.SoundTimeScale;if(game.paused&&!voicesPaused)pair.Key.Pause();else if(!game.paused&&voicesPaused)pair.Key.UnPause();}
  voicesPaused=game.paused;
 }
}
}
