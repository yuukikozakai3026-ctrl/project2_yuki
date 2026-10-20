using System;
using System.IO;
using System.Collections.Generic;
using System.Globalization;
using UnityEditor;
using UnityEngine;
using VRBat;

public static class ManualGameplayChecks {
 static void Check(bool ok,string message){if(!ok)throw new Exception(message);}
 public static string Planner(){
  var c=new ManualTuning();var plans=ManualPitchPlanner.Build(c);Check(plans.Count>0,"No pitches");
  for(int i=0;i<plans.Count;i++){
   var p=plans[i];Check(ManualPitchPlanner.Safe(p,plans.GetRange(0,i),c),"Unsafe planned trajectory "+i);
   Check(Mathf.Abs(p.end.x)<=c.zoneWidth/2&&Mathf.Abs(p.end.y-c.zoneCenterY)<=c.zoneHeight/2,"Outside zone");
   Check(Vector3.Distance(p.At(p.launch),p.source)<.001f&&Vector3.Distance(p.At(p.Arrival),p.end)<.001f,"Path discontinuity");
  }
  var dense=new ManualTuning{intervalStart=.4f,intervalEnd=.4f,bombPercentStart=100,bombPercentEnd=100};var bombs=ManualPitchPlanner.Build(dense);
  for(int i=0;i<bombs.Count;i++)Check(bombs[i].bomb&&ManualPitchPlanner.Safe(bombs[i],bombs.GetRange(0,i),dense),"Dense bomb overlap");
  var zero=new ManualTuning{bombPercentStart=0,bombPercentEnd=0};Check(ManualPitchPlanner.Build(zero).TrueForAll(p=>!p.bomb),"0% bombs");
  Check(c.Points(0,false)<c.Points(4,false)&&c.Points(4,false)<c.Points(8,false),"Speed points");
  Check(c.Points(4,true)>c.Points(4,false)&&c.Points(100,true)==Mathf.RoundToInt(c.maximumPoints),"Band bonus or cap");
  float axis;Check(BatGeometry.SweepSphere(new Vector3(.15f,0,.1f),new Vector3(.15f,0,0),.07f,Vector3.zero,Vector3.right,Vector3.zero,Vector3.right,out axis)&&!AFlight.IsStrong(axis),"Unmarked contact must work");
  var json=new List<string>();foreach(var p in plans)json.Add(string.Format(CultureInfo.InvariantCulture,"{{\"id\":{0},\"launch\":{1},\"x\":{2},\"y\":{3},\"bomb\":{4}}}",p.id,p.launch,p.end.x,p.end.y,p.bomb?1:0));
  File.WriteAllText("/tmp/vrbat-unity-plan.json","["+string.Join(",",json)+"]");
  return $"PLANNER_OK default={plans.Count} denseBombs={bombs.Count}";
 }
 public static string Runtime(){
  var g=UnityEngine.Object.FindFirstObjectByType<BatGame>();Check(g&&g.desktopInput,"Desktop runtime missing");
  g.phase="play";g.runAt=g.now;g.roundDuration=600;g.manualPitches.Clear();g.stunnedUntil=0;g.score=0;
  var source=new Vector3(0,1.71f,7.75f);var end=new Vector3(0,1.3f,.70f);
  var p=new PlannedPitch{id=900,bomb=false,launch=-2.39f,seconds=2.4f,arc=.65f,source=source,end=end};
  var at=p.At(0);g.grip=g.W(at-Vector3.right*.15f);g.axis=g.D(Vector3.right);g.oldGrip=g.grip;g.oldAxis=g.axis;
  var ball=new ManualPitch(g,p);ball.Step(1f/240);Check(ball.done&&g.score==g.tuning.Points(0,false),"Bunt on unmarked shaft");int low=g.score;
  p=new PlannedPitch{id=901,bomb=false,launch=-2.39f,seconds=2.4f,arc=.65f,source=source,end=end};at=p.At(0);
  g.grip=g.W(at-Vector3.right*.555f);g.oldGrip=g.grip-Vector3.forward*.025f;g.axis=g.D(Vector3.right);g.oldAxis=g.axis;
  ball=new ManualPitch(g,p);ball.Step(1f/240);Check(ball.done&&g.score-low>low,"Fast band hit score");
  int before=g.score;g.BombContact(g.W(end));Check(g.score==before-Mathf.RoundToInt(g.tuning.bombPenalty)&&!g.BatAvailable,"Bomb penalty");
  p=new PlannedPitch{id=902,bomb=false,launch=-2.39f,seconds=2.4f,arc=.65f,source=source,end=end};at=p.At(0);g.grip=g.W(at-Vector3.right*.555f);g.oldGrip=g.grip;
  ball=new ManualPitch(g,p);ball.Step(1f/240);Check(!ball.done&&g.score==before-Mathf.RoundToInt(g.tuning.bombPenalty),"Disabled bat hit");UnityEngine.Object.Destroy(ball.actor);
  g.stunnedUntil=g.now-.01;Check(g.BatAvailable,"Bat recovery");g.phase="menu";
  return "RUNTIME_OK bunt/fast-band/bomb/stun/recovery";
 }
 public static void BuildMac(){
  Directory.CreateDirectory("Builds");
  var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"Assets/Scenes/HomeScene.unity","Assets/Scenes/GameScene.unity"},locationPathName="Builds/Bat Carnival.app",target=BuildTarget.StandaloneOSX,options=BuildOptions.None});
  Debug.Log("MANUAL_BUILD_RESULT "+report.summary.result);
  if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new Exception("Build failed");
 }
}
