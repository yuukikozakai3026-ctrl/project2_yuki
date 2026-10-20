using System;
using System.Collections.Generic;
using UnityEngine;
namespace VRBat {
public sealed class Challenge {
 public readonly BatGame g;public readonly string kind;public readonly bool intro;public readonly int lane,sourceLane,magicCount,realIndex,variant;public readonly float hitY,approach,slowDuration;
 public double start,routeAt,slowAt,arrival,deadline,resolvedAt;public bool done,ok,recoveryHint;public Vector3 position;public int hitCount;
 public readonly List<GameObject> actors=new List<GameObject>();public readonly bool[] processed;public readonly Vector3[] offsets;public readonly PitchPath path;
 public readonly Vector3 initial;readonly Vector3[] previous;readonly Quaternion[] priorRotation;readonly bool[] emitted;readonly BatStrikeGate[] strikeGates;
 readonly List<TrailRenderer> trails=new List<TrailRenderer>();readonly List<int> contacts=new List<int>(8);readonly List<float> axisContacts=new List<float>(8);readonly bool[] cues=new bool[5];readonly PitchApproach entry;bool slowCue,trickCue;readonly double visualStart;float lastTelemetry=-1;
 public bool FakeWasHit {get;private set;} public double FakeHitAt {get;private set;}
 void Log(string what,string detail="")=>g.Event(what,detail,kind);
 public Challenge(BatGame game,string type,int? selectedLane=null,Vector3? releasePoint=null){
  g=game;visualStart=g.effectTime;kind=type;intro=g.phase=="intro";start=g.now-(releasePoint.HasValue?.65:0);int step=intro?g.introIndex:g.index;
  lane=selectedLane.HasValue?Mathf.Clamp(selectedLane.Value,-1,1):intro?0:new[]{0,0,-1,1,0,0,1,-1,0,0,0,0}[step%12];sourceLane=lane;hitY=intro?1.22f:kind=="magic"?1.34f:kind=="chest"?1.30f:new[]{1.22f,1.15f,1.28f}[step%3];
  variant=intro?0:g.NextPitchVariant();path=new PitchPath(kind,lane,hitY,variant);initial=releasePoint??new Vector3(lane*1.65f,1.45f,lane==0?AFlight.CenterLaunchZ:AFlight.SideLaunchZ);
  entry=new PitchApproach(initial,path,AFlight.EarlyTrick(kind));approach=entry.Duration;routeAt=start+.65+approach;slowAt=routeAt+path.SlowEntryTime;slowDuration=path.Duration-path.SlowEntryTime;
  magicCount=kind=="magic"?new[]{3,5,8}[g.magicStep++%3]:1;realIndex=kind=="magic"?(magicCount==3?1:magicCount==5?3:5):0;
  int count=kind=="bullets"?3:magicCount;offsets=new Vector3[count];processed=new bool[count];previous=new Vector3[count];priorRotation=new Quaternion[count];emitted=new bool[count];strikeGates=new BatStrikeGate[count];
  for(int i=0;i<count;i++){
   strikeGates[i]=new BatStrikeGate();
   offsets[i]=kind=="magic"?MagicOffset(i,count):kind=="bullets"?new Vector3((i-1)*.16f,(i-1)*.015f,0):Vector3.zero;
   var actor=g.view.Target(kind,false,g.W(initial+offsets[i]),g.targets);if(kind=="magic")PitchVisuals.SetMagic(actor,i==realIndex,g.view);
   actors.Add(actor);priorRotation[i]=Quaternion.identity;previous[i]=initial+offsets[i];actor.SetActive(false);
   var trail=PitchVisuals.Trail(actor,kind,g.view);trails.Add(trail);
  }
  arrival=slowAt+slowDuration+(kind=="bullets"?.26:0);deadline=arrival;position=initial;
  Log("pitch_config",$"kind={kind};count={count};real={realIndex};fast={AFlight.FastSpeed:F3};slow={AFlight.NearSpeed(kind):F3};slow_z={path.SlowEntry.z:F2};trick_z={path.Start.z:F2};slow_seconds={slowDuration:F3};variant={variant};end_x={path.End.x:F3};end_y={path.End.y:F3}");
 }
 public static Vector3 MagicOffset(int i,int n){if(n==3)return new Vector3((i-1)*.30f,0,0);if(n==5)return i==0?Vector3.zero:new Vector3(Mathf.Cos((i-1)*Mathf.PI/2)*.36f,Mathf.Sin((i-1)*Mathf.PI/2)*.20f,0);return new Vector3((i%4-1.5f)*.28f,(i/4-.5f)*.27f,0);}
 public Vector3 ItemPosition(int i,double now){double since=now-start-.65-(kind=="bullets"?i*.13:0);if(since<0)return initial+offsets[i];if(since<approach)return entry.At((float)since)+offsets[i];return path.At((float)(since-approach))+offsets[i];}
 public float PathElapsed(int i)=>(float)(g.now-routeAt-(kind=="bullets"?i*.13:0));
 public void Step(float dt){
  if(done)return;double now=g.now;float local=(float)(now-routeAt);position=ItemPosition(realIndex,now);
  g.view.Muzzle(g.left?-sourceLane:sourceLane,now<slowAt?1:0,now<start+.65?.55f:0);
  if(!trickCue&&local>=0&&AFlight.EarlyTrick(kind)){trickCue=true;Log("trick_start",$"z={path.Start.z:F2}");}
  if(!slowCue&&now>=slowAt){slowCue=true;Log("slow_start",$"z={path.SlowEntry.z:F2}");g.fx.SlowCue(g.W(path.SlowEntry));g.fx.Sound("minimize_001",-23,.8f);}
  FlightCues(local);
  contacts.Clear();axisContacts.Clear();
  for(int i=0;i<actors.Count;i++){
   if(processed[i])continue;double launch=start+.65+(kind=="bullets"?i*.13:0);
   if(now<launch)continue;
   if(!emitted[i]){emitted[i]=true;actors[i].SetActive(true);trails[i].Clear();Log("launch",$"item={i};x={initial.x:F2};z={initial.z:F2}");g.fx.LaunchCue(g.W(initial+offsets[i]));if(kind=="bomb")g.fx.BombFuse(actors[i],this);}
   var p=ItemPosition(i,now);var rotation=PitchVisuals.Rotation(kind,(float)(g.effectTime-visualStart),path.Duration);
   if(kind=="cake"){
    float at=Mathf.Clamp(PathElapsed(i),.015f,path.Duration-.015f);
    Vector3 tangent=path.At(at+.015f)-path.At(at-.015f);
    float heading=Mathf.Atan2(tangent.x,-tangent.z)*Mathf.Rad2Deg;
    rotation=Quaternion.Euler(8,heading,-heading*.35f);
   }
   actors[i].transform.SetPositionAndRotation(g.W(p),g.left?new Quaternion(rotation.x,-rotation.y,-rotation.z,rotation.w):rotation);
   if(kind=="chest")PitchVisuals.Lid(actors[i],ChestLid(local));
   // No speed threshold. Only the painted band at first contact can return an object.
   if(now>=slowAt+(kind=="bullets"?i*.13:0)&&now<=deadline&&Contact(i,previous[i],p,priorRotation[i],rotation,out float s)){contacts.Add(i);axisContacts.Add(s);}
   previous[i]=p;priorRotation[i]=rotation;
  }
  bool containsReal=kind=="magic"&&contacts.Contains(realIndex);
  if(containsReal){int at=contacts.IndexOf(realIndex);Log("magic_real_contact",$"simultaneous={contacts.Count};fake_first={FakeWasHit}");Hit(realIndex,axisContacts[at]);for(int i=0;i<actors.Count;i++)if(!processed[i]){processed[i]=true;g.fx.Dissolve(actors[i],.65f);}Resolve(true,"magic_real");return;}
  for(int k=0;k<contacts.Count;k++){
   int i=contacts[k];if(kind=="magic"){processed[i]=true;FakeWasHit=true;FakeHitAt=g.now;Log("magic_fake_contact",$"item={i};score_unchanged={g.score}");g.fx.Dissolve(actors[i],.24f);g.fx.Sound("magic_dissolve",-24,1.15f);}
   else Hit(i,axisContacts[k]);
  }
  if(kind!="magic"&&hitCount==actors.Count){if(kind=="bullets")g.fx.BulletVolley(g.W(position));Resolve(true,"hit");return;}
  if(local-lastTelemetry>=.05f){lastTelemetry=local;var pose=actors[realIndex].transform.rotation;Log("flight_sample",$"kind={kind};x={position.x:F5};y={position.y:F5};z={position.z:F5};slow_t={now-slowAt:F3};route_t={local:F3};qx={pose.x:F5};qy={pose.y:F5};qz={pose.z:F5};qw={pose.w:F5};lid={ChestLid(local):F3}");}
  if(now>=arrival){for(int i=0;i<actors.Count;i++)if(!processed[i]){processed[i]=true;Log("miss",$"item={i};real={kind!="magic"||i==realIndex}");g.fx.SoftMiss(actors[i],g.D((path.End-path.At(path.Duration-.02f))/.02f));}Resolve(false,"miss");}
 }
 void Hit(int i,float s){processed[i]=true;hitCount++;bool strong=AFlight.IsStrong(s);var p=g.W(ItemPosition(i,g.now));Log("contact",$"item={i};axis={s:F5};strong={strong}");g.Award(strong?20:10,p,strong);g.fx.Hit(kind,p,strong);var swing=((g.grip+g.axis*s)-(g.oldGrip+g.oldAxis*s))*240f;var incoming=g.D(path.At(Mathf.Min(path.Duration,PathElapsed(i)+.05f))-path.At(Mathf.Max(0,PathElapsed(i)-.05f)));g.fx.HitFar(actors[i],p,kind,offsets[i],strong,swing,incoming,variant,i);}
 void Resolve(bool success,string reason){done=true;ok=success;resolvedAt=g.now;g.view.Muzzle(g.left?-sourceLane:sourceLane,0);if(!intro&&success)g.success[g.KindIndex(kind)]++;if(!intro)g.RecordAResult(kind,success);Log("resolved",reason);}
 public void Destroy(){foreach(var o in actors)if(o)UnityEngine.Object.Destroy(o);}
 Vector3 Grip=>new Vector3(g.grip.x*g.Mirror,g.grip.y-g.dy,g.grip.z);
 Vector3 OldGrip=>new Vector3(g.oldGrip.x*g.Mirror,g.oldGrip.y-g.dy,g.oldGrip.z);
 Vector3 Axis=>new Vector3(g.axis.x*g.Mirror,g.axis.y,g.axis.z);
 Vector3 OldAxis=>new Vector3(g.oldAxis.x*g.Mirror,g.oldAxis.y,g.oldAxis.z);
 bool Contact(int i,Vector3 before,Vector3 after,Quaternion from,Quaternion to,out float s){
  bool hit=strikeGates[i].Sweep(kind,before,after,from,to,OldGrip,OldAxis,Grip,Axis,out s,out bool rejected);
  if(rejected){Log("invalid_bat_contact",$"item={i};reason=tip_or_unmarked_surface;object={after.x:F3}/{after.y:F3}/{after.z:F3};grip={Grip.x:F3}/{Grip.y:F3}/{Grip.z:F3};axis={Axis.x:F3}/{Axis.y:F3}/{Axis.z:F3}");g.bandHintUntil=g.now+.24;}
  return hit;
 }
 void FlightCues(float local){
  if(local<0)return;
  if(kind=="boomerang"&&!cues[4]&&local>=path.TimeAt(.32f)){cues[4]=true;g.fx.Sound("boomerang_whoosh",-12,.85f);Log("uturn_cue");}
  float[] fractions=kind=="poison"?new[]{.23f,.85f}:new float[0];
  for(int i=0;i<fractions.Length;i++){float at=path.TimeAt(fractions[i]);if(!cues[i]&&local>=at-.30f){cues[i]=true;Vector3 before=(path.At(at)-path.At(at-.02f))/.02f,after=(path.At(at+.02f)-path.At(at))/.02f;Vector3 impulse=after-before;float stop=i+1<fractions.Length?path.TimeAt(fractions[i+1])-.30f:path.Duration;g.fx.RecoilCue(actors[0],g.D(-impulse.normalized),kind,this,at,stop);Log("recoil_cue",$"part={i};lead={at-local:F3}");}}
 }
 float ChestLid(float local){if(local<0)return .5f;float phase=6.5f*Mathf.PI*path.ProgressAt(local);return .5f-.5f*Mathf.Sin(phase);}

}
}
