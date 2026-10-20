using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
namespace VRBat {
public partial class BatGame {
 public ManualTuning tuning=new ManualTuning();
 public readonly List<ManualPitch> manualPitches=new List<ManualPitch>();
 public List<PlannedPitch> pitchPlan=new List<PlannedPitch>();
 public double stunnedUntil;public bool BatAvailable=>now>=stunnedUntil;
 public bool desktopInput=>!UnityEngine.XR.XRSettings.isDeviceActive;
 public Vector3 LocalGrip(Vector3 p)=>new Vector3(p.x*Mirror,p.y-dy,p.z);
 public Vector3 LocalAxis(Vector3 p)=>new Vector3(p.x*Mirror,p.y,p.z);
 public bool ManualNear {get {foreach(var p in manualPitches)if(!p.done&&p.previous.z<=AFlight.SlowZ)return true;return false;}}
 Renderer[] batRenderers;GameObject aimMarker;float charge,chargeBegan=-1,swingElapsed=10,swingDuration=.3f,swingPower;Vector3 mouseAim=new Vector3(0,1.3f,.70f),swingAim;
 void ResetManual(){foreach(var p in manualPitches)if(p.actor)Destroy(p.actor);manualPitches.Clear();stunnedUntil=0;chargeBegan=-1;swingElapsed=10;charge=0;}
 public void PrepareManualRound(){ResetManual();tuning=ManualTuning.Load();tuning.seed=System.Environment.TickCount&int.MaxValue;roundDuration=tuning.roundSeconds;pitchPlan=ManualPitchPlanner.Build(tuning);Event("tuning_loaded",$"duration={roundDuration};interval={tuning.intervalStart}/{tuning.intervalEnd};bomb={tuning.bombPercentStart}/{tuning.bombPercentEnd};seed={tuning.seed};pitches={pitchPlan.Count}");}
 public void SpawnManualPitch(PlannedPitch p){manualPitches.Add(new ManualPitch(this,p));presented[p.bomb?5:0]++;Event("manual_release",$"id={p.id};at={p.launch:F3};arrival={p.Arrival:F3};end_x={p.end.x:F3};end_y={p.end.y:F3}",p.bomb?"bomb":"ball");}
 void TickManual(float dt){for(int i=0;i<manualPitches.Count;i++)manualPitches[i].Step(dt);manualPitches.RemoveAll(p=>p.done);if(phase=="play"&&Remaining<=0&&manualPitches.Count==0)Results();}
 public void BombContact(Vector3 point){score-=Mathf.RoundToInt(tuning.bombPenalty);stunnedUntil=now+tuning.stunSeconds;chargeBegan=-1;charge=0;swingElapsed=10;
  fx.Points(point,-Mathf.RoundToInt(tuning.bombPenalty));fx.Hit("bomb",point,false);fx.Burst(point+Vector3.forward*.12f,"smoke_01",12,.08f,1.1f,.45f,new Color(.6f,.38f,.25f));
  Event("bomb_penalty",$"points={tuning.bombPenalty};stun={tuning.stunSeconds};until={stunnedUntil:F3}","bomb");}
 void ReadDesktop(float dt){
  view.eye.transform.SetPositionAndRotation(new Vector3(-.50f*Mirror,height,-.25f),Quaternion.Euler(6,10*Mirror,0));
  var mouse=Mouse.current;if(mouse==null)return;
  var ray=view.eye.ScreenPointToRay(mouse.position.ReadValue());var plane=new Plane(Vector3.forward,new Vector3(0,0,.70f));
  if(plane.Raycast(ray,out float distance)){var p=LocalGrip(ray.GetPoint(distance));mouseAim=new Vector3(Mathf.Clamp(p.x,-tuning.zoneWidth/2-.2f,tuning.zoneWidth/2+.2f),Mathf.Clamp(p.y,tuning.zoneCenterY-tuning.zoneHeight/2-.15f,tuning.zoneCenterY+tuning.zoneHeight/2+.15f),.70f);}
  if(phase=="play"&&!paused&&BatAvailable){
   if(mouse.leftButton.wasPressedThisFrame&&swingElapsed>=swingDuration)chargeBegan=(float)now;
   if(chargeBegan>=0)charge=Mathf.Clamp01(((float)now-chargeBegan)/tuning.chargeSeconds);
   if(mouse.leftButton.wasReleasedThisFrame&&chargeBegan>=0){swingPower=charge;swingDuration=Mathf.Lerp(.46f,.14f,charge);swingElapsed=0;swingAim=mouseAim;chargeBegan=-1;charge=0;Event("mouse_swing",$"power={swingPower:F3}");}
  }else{chargeBegan=-1;charge=0;}
  swingElapsed+=paused?0:dt;
  Vector3 localAxis=new Vector3(.98f,.08f,.12f).normalized;var bandPoint=mouseAim;
  if(swingElapsed<swingDuration){float u=swingElapsed/swingDuration;float angle=Mathf.Lerp(-.55f,.30f,u);localAxis=new Vector3(Mathf.Cos(angle),.08f,Mathf.Sin(angle)).normalized;bandPoint=swingAim+new Vector3(Mathf.Lerp(-.45f,.45f,u),0,.12f*Mathf.Sin(Mathf.PI*u));}
  xrGrip=W(bandPoint-localAxis*.555f);xrAxis=D(localAxis);
 }
 void UpdateManualVisuals(){
  if(batRenderers==null)batRenderers=view.bat.GetComponentsInChildren<Renderer>();bool visible=BatAvailable||((int)(now*8)%2==0);
  foreach(var r in batRenderers)if(r)r.enabled=visible;
  if(desktopInput&&phase=="play"){
   if(!aimMarker)aimMarker=BatGeometry.Ring("Mouse batting aim",W(mouseAim),.025f,.002f,view.gold,transform);
   aimMarker.transform.position=W(mouseAim);aimMarker.SetActive(BatAvailable);
  }else if(aimMarker)aimMarker.SetActive(false);
 }
 void ManualControlsGUI(){
  if(!desktopInput||phase!="play")return;
  float scale=Mathf.Clamp(Screen.height/720f,.7f,1.5f);var old=GUI.matrix;GUI.matrix=Matrix4x4.Scale(Vector3.one*scale);
  float w=Screen.width/scale,h=Screen.height/scale;var style=new GUIStyle(GUI.skin.label){font=view.font,fontSize=18,alignment=TextAnchor.MiddleCenter};
  string text=!BatAvailable?$"バット復帰まで {Mathf.Max(0,(float)(stunnedUntil-now)):F1} 秒":"マウスで打点を移動  ·  左ボタンをためて離すとスイング";
  GUI.Box(new Rect(w/2-270,h-68,540,54),"");GUI.Label(new Rect(w/2-260,h-64,520,30),text,style);
  if(BatAvailable){GUI.color=new Color(.35f,.78f,.86f);GUI.DrawTexture(new Rect(w/2-250,h-30,500*charge,5),Texture2D.whiteTexture);GUI.color=Color.white;}
  GUI.matrix=old;
 }
}
}
