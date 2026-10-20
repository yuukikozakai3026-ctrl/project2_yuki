using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

namespace VRBat {
[Serializable]
public sealed class ManualTuning {
 public int schemaVersion=1, seed=20261006;
 public float roundSeconds=60, intervalStart=2.2f, intervalEnd=.95f, intervalJitter=.18f;
 public float bombPercentStart=4, bombPercentEnd=10, bombPenalty=50, stunSeconds=1.5f;
 public float minimumPoints=5, maximumPoints=100, maximumScoreSpeed=8, bandSpeedBonus=.5f, chargeSeconds=1;
 public float zoneWidth=1.1f, zoneHeight=.65f, zoneCenterY=1.3f, flightSeconds=2.4f, arcHeight=.65f, launchDistance=7.75f;
 public float ballGap=.30f, bombGap=.65f, clearance=.32f, bombClearance=.50f;
 public static string ConfigPath=>Path.Combine(Application.persistentDataPath,"Tuning","settings.json");
 public static ManualTuning Load(){
  var path=File.Exists(ConfigPath)?ConfigPath:Path.Combine(Application.streamingAssetsPath,"ManualTuning.json");
  var c=new ManualTuning();
  try { if(File.Exists(path)) JsonUtility.FromJsonOverwrite(File.ReadAllText(path),c); }
  catch(Exception e){Debug.LogWarning("調整設定を読み込めません。初期設定を使用します: "+e.Message);}
  c.Validate(); return c;
 }
 public void Validate(){
  roundSeconds=Safe(roundSeconds,10,300,60);intervalStart=Safe(intervalStart,.4f,8,2.2f);intervalEnd=Safe(intervalEnd,.4f,intervalStart,.95f);
  intervalJitter=Safe(intervalJitter,0,.5f,.18f);bombPercentStart=Safe(bombPercentStart,0,100,4);bombPercentEnd=Safe(bombPercentEnd,0,100,10);
  bombPenalty=Safe(bombPenalty,0,1000,50);stunSeconds=Safe(stunSeconds,0,10,1.5f);
  minimumPoints=Safe(minimumPoints,0,1000,5);maximumPoints=Safe(maximumPoints,minimumPoints,5000,100);
  maximumScoreSpeed=Safe(maximumScoreSpeed,.5f,30,8);bandSpeedBonus=Safe(bandSpeedBonus,0,10,.5f);chargeSeconds=Safe(chargeSeconds,.2f,3,1);
  zoneWidth=Safe(zoneWidth,.3f,1.8f,1.1f);zoneHeight=Safe(zoneHeight,.2f,1.1f,.65f);zoneCenterY=Safe(zoneCenterY,.8f,1.7f,1.3f);
  flightSeconds=Safe(flightSeconds,1,6,2.4f);arcHeight=Safe(arcHeight,.05f,2,.65f);launchDistance=Safe(launchDistance,4,16,7.75f);
  ballGap=Safe(ballGap,.15f,2,.30f);bombGap=Safe(bombGap,ballGap,3,.65f);clearance=Safe(clearance,.2f,1,.32f);bombClearance=Safe(bombClearance,clearance,1.5f,.50f);
 }
 static float Safe(float n,float lo,float hi,float fallback)=>Mathf.Clamp(float.IsFinite(n)?n:fallback,lo,hi);
 public float Progress(float t)=>Mathf.SmoothStep(0,1,t/roundSeconds);
 public float Interval(float t)=>Mathf.Lerp(intervalStart,intervalEnd,Progress(t));
 public float BombPercent(float t)=>Mathf.Lerp(bombPercentStart,bombPercentEnd,Progress(t));
 public int Points(float speed,bool band)=>Mathf.RoundToInt(Mathf.Lerp(minimumPoints,maximumPoints,Mathf.Clamp01((speed+(band?bandSpeedBonus:0))/maximumScoreSpeed)));
}

public sealed class PlannedPitch {
 public int id; public bool bomb; public float launch,seconds,arc;public Vector3 source,end;
 public float Arrival=>launch+seconds;
 public static float Progress(float time)=>Mathf.Clamp01(time)+.24f*Mathf.Sin(Mathf.PI*Mathf.Clamp01(time));
 public Vector3 At(float time){float u=Progress((time-launch)/seconds);return Vector3.Lerp(source,end,u)+Vector3.up*(4*arc*u*(1-u));}
}

public static class ManualPitchPlanner {
 sealed class Rng {uint state;public Rng(int seed){state=(uint)seed;}public float Next(){state=unchecked(state*1664525u+1013904223u);return (state>>8)/16777216f;}}
 public static List<PlannedPitch> Build(ManualTuning c){
  var result=new List<PlannedPitch>();var rng=new Rng(c.seed);float time=.8f;
  while(time<c.roundSeconds){
   bool bomb=rng.Next()*100<c.BombPercent(time);PlannedPitch candidate=null;
   for(int delay=0;delay<120&&candidate==null;delay++){
    for(int attempt=0;attempt<24;attempt++){
     var p=new PlannedPitch{id=result.Count,bomb=bomb,launch=time,seconds=c.flightSeconds,arc=c.arcHeight,
      source=new Vector3(0,1.71f,c.launchDistance),end=new Vector3((rng.Next()-.5f)*c.zoneWidth,c.zoneCenterY+(rng.Next()-.5f)*c.zoneHeight,.70f)};
     if(Safe(p,result,c)){candidate=p;break;}
    }
    if(candidate==null)time+=.05f;
   }
   if(candidate==null||time>=c.roundSeconds)break;
   result.Add(candidate);time+=Mathf.Max(.4f,c.Interval(time)*(1+(rng.Next()*2-1)*c.intervalJitter));
  }
  return result;
 }
 public static bool Safe(PlannedPitch p,List<PlannedPitch> others,ManualTuning c){
  foreach(var q in others){
   if(q.Arrival<p.launch)continue;
   bool bomb=p.bomb||q.bomb;float gap=bomb?c.bombGap:c.ballGap,clear=bomb?c.bombClearance:c.clearance;
   if(Mathf.Abs(p.Arrival-q.Arrival)<gap)return false;
   float stop=Mathf.Min(p.Arrival,q.Arrival);
   for(float t=p.launch;t<=stop+.001f;t+=.025f)if(Vector3.Distance(p.At(t),q.At(t))<clear)return false;
   if(Vector3.Distance(p.At(stop),q.At(stop))<clear)return false;
  }
  return true;
 }
}
}
