using System;
using UnityEngine;
using VRBat;
public static class GeometryChecks {
 public static void Run(){
  BandChecks();
  foreach(float s in new[]{.32f,.43f,.555f,.68f}){var center=new Vector3(.039f+.07f-.00001f,s,0);if(!BatGeometry.SphereContact(center,.07f,Vector3.zero,Vector3.up,out float hit,out _))throw new Exception("Visible bat contact missed");if(Mathf.Abs(s-hit)>.00001f)throw new Exception("Band projection changed");if(AFlight.IsStrong(s)!=AFlight.IsStrong(hit))throw new Exception("Strong hit boundary changed");}
  if(BatGeometry.SphereContact(new Vector3(.12f,.555f,0),.07f,Vector3.zero,Vector3.up,out _,out _))throw new Exception("Invisible enlarged bat");
  foreach(var kind in new[]{"ball","cake","poison","boomerang","magic","bomb","bullets","chest"}){
   var path=new PitchPath(kind,0,1.22f);if(Mathf.Abs(path.SlowEntry.z-AFlight.SlowZ)>.001f||Mathf.Abs((path.SlowEntry.z-path.End.z)/(2f-.48f)-2f/3f)>.001f)throw new Exception("Slow range not two thirds: "+kind);
   // A finite-difference chord is shorter than arc travel when it crosses a sharp recoil corner.
   for(float t=.05f;t<path.Duration-.05f;t+=.013f){var p=path.At(t);float speed=Vector3.Distance(p,path.At(t+.001f))/.001f;if(speed>(t>=path.SlowEntryTime+.015f?AFlight.NearSpeed(kind)+.012f:AFlight.FarSpeed(kind)+.02f))throw new Exception("Slow speed changed: "+kind+" "+speed);if(p.z<.47f||Mathf.Abs(p.x)>1.1f||p.y<.85f||p.y>2.4f)throw new Exception("Pitch leaves front view: "+kind);}
   float measured=0;for(float t=path.SlowEntryTime+.02f;t<path.Duration;t+=.002f)measured+=Vector3.Distance(path.At(t),path.At(Mathf.Min(path.Duration,t+.002f)));if(Mathf.Abs(measured/(path.Duration-path.SlowEntryTime-.02f)-AFlight.NearSpeed(kind))>.004f)throw new Exception("Average slow pace changed: "+kind);
   var end=path.End;if(Mathf.Abs(end.x)>.001f||Mathf.Abs(end.y-1.22f)>.001f)throw new Exception("Unreachable finish "+kind);
  }
  var start=new Vector3(0,1.45f,AFlight.CenterLaunchZ);var endSlow=new PitchPath("ball",0,1.22f).Start;float d=AFlight.FastDuration(start,endSlow);float velocity=Vector3.Distance(AFlight.Fast(start,endSlow,.12f,d),AFlight.Fast(start,endSlow,.13f,d))/.01f;if(Mathf.Abs(velocity/AFlight.BaselineSpeed-3)>.01f)throw new Exception("Not three times baseline");
  if(Vector3.Distance(AFlight.Fast(start,endSlow,d,d),endSlow)>.0001f)throw new Exception("Fast approach join");
  for(int n=3;n<=8;n+=(n==3?2:3))for(int i=0;i<n;i++){var o=Challenge.MagicOffset(i,n);if(Mathf.Abs(o.x)>.421f||Mathf.Abs(o.y)>.261f)throw new Exception("Magic group outside reachable envelope");}
  // Every pearl stays independently aimable, including every endpoint variant.
  foreach(int count in new[]{3,5,8})for(int i=0;i<count;i++){
   var offset=Challenge.MagicOffset(i,count);
   for(int j=i+1;j<count;j++)if(Vector3.Distance(offset,Challenge.MagicOffset(j,count))<.199f)throw new Exception("Magic pearls too tightly packed");
   for(int variant=0;variant<=32;variant++){
    var route=new PitchPath("magic",0,1.34f,variant);
    var endpoint=route.End+offset;
    if(Mathf.Abs(endpoint.x)>.61f||endpoint.y<1.05f||endpoint.y>1.63f)throw new Exception("Magic endpoint out of reach");
    for(float t=route.SlowEntryTime+.02f;t<route.Duration-.03f;t+=.02f){
     var before=route.At(t)-route.At(t-.01f);var after=route.At(t+.01f)-route.At(t);
     if(Vector3.Angle(before,after)>2)throw new Exception("Magic sweep has an unexplained kink");
    }
   }
  }
  Debug.Log("AGGRESSIVE_TRICK_PASS: targeted near speed 0.6m/s; magic gaps at least 0.20m; all 33 endpoint variations reachable; smooth magic sweep");
  var stationary=new Vector3(.039f+.07f-.00001f,.555f,0);if(!BatGeometry.SweepSphere(stationary,stationary,.07f,Vector3.zero,Vector3.up,Vector3.zero,Vector3.up,out _))throw new Exception("Stationary bat no longer works");
  foreach(var kind in new[]{"ball","cake","poison","boomerang","magic","bomb","bullets","chest"}){
   float minX=10,maxX=-10,minY=10,maxY=-10;
   for(int variation=1;variation<=32;variation++){
    var path=new PitchPath(kind,0,1.22f,variation);minX=Mathf.Min(minX,path.End.x);maxX=Mathf.Max(maxX,path.End.x);minY=Mathf.Min(minY,path.End.y);maxY=Mathf.Max(maxY,path.End.y);
    if(Mathf.Abs(path.End.x)>.181f||Mathf.Abs(path.End.y-1.22f)>.081f)throw new Exception("Variable pitch outside batting area");
    for(float time=.02f;time<path.Duration;time+=.02f){var at=path.At(time);if(at.z<.479f||Mathf.Abs(at.x)>1.1f||at.y<.8f||at.y>2.4f)throw new Exception("Variable trick leaves front volume");if(Vector3.Distance(at,path.At(time-.001f))>(time>=path.SlowEntryTime+.02f?(AFlight.NearSpeed(kind)+.016f)*.001f:(AFlight.FarSpeed(kind)+.03f)*.001f))throw new Exception("Variable pitch teleports or speeds up");}
   }if(maxX-minX<.32f||maxY-minY<.14f)throw new Exception("Final batting position still fixed");
  }
  // A bounded endpoint and constant speed do not detect a kink: check direction too.
  for(int variant=0;variant<=32;variant++){
   var curve=new PitchPath("cake",0,1.22f,variant);
   for(float time=.03f;time<curve.Duration-.03f;time+=.01f){
    Vector3 before=curve.At(time)-curve.At(time-.02f*AFlight.SlowRangeScale),after=curve.At(time+.02f*AFlight.SlowRangeScale)-curve.At(time);
    if(Vector3.Angle(before,after)>4f)throw new Exception("Cake direction snaps: "+time);
   }
   Vector3 tail=curve.End-curve.At(curve.Duration-.1f);
   if(Mathf.Abs(tail.x)<.009f)throw new Exception("Cake forcibly straightens before the bat");
   var uturn=new PitchPath("boomerang",0,1.22f,variant);
   float expectedSide=variant>0&&variant%2==0?1:-1;
   for(float time=uturn.TimeAt(.65f);time<uturn.Duration-.02f;time+=.02f)
    if((uturn.At(time+.015f).x-uturn.At(time).x)*expectedSide<-.00001f)
     throw new Exception("Boomerang adds a return hook after its U-turn");
   var recoil=new PitchPath("poison",0,1.22f,variant);
   // Between the two announced jet changes, acceleration is smooth and has no extra kink.
   foreach(var interval in new[]{new Vector2(.24f,.84f),new Vector2(.86f,.998f)}){
    for(float phase=interval.x;phase<interval.y;phase+=.004f){
     Vector3 a=recoil.At(recoil.TimeAt(phase-.001f)),b=recoil.At(recoil.TimeAt(phase)),c=recoil.At(recoil.TimeAt(phase+.001f));
     if(Vector3.Angle(b-a,c-b)>3f)throw new Exception("Poison force changes without a cue");
    }
   }
  }
  Debug.Log("TRAJECTORY_DIRECTION_PASS: 33 cake curves without tangent snaps; 33 U-turns without final hooks; poison 2 cued turns; smooth heavy bomb lob");
  foreach(string kind in new[]{"cake","poison","boomerang","magic","bomb","chest"})for(int variant=0;variant<33;variant++){
   var route=new PitchPath(kind,0,1.22f,variant);
   if(Mathf.Abs(route.Start.z-4.2f)>.001f||route.SlowEntryTime<1f)throw new Exception("Distant trick missing: "+kind);
   var entry=new PitchApproach(new Vector3(0,1.45f,7.38f),route,true);
   if(Vector3.Distance(entry.At(entry.Duration),route.Start)>.0001f)throw new Exception("Approach gap");
   if(Vector3.Angle(entry.At(entry.Duration)-entry.At(entry.Duration-.002f),route.At(.002f)-route.Start)>5f)throw new Exception("Distant trick entry snaps: "+kind);
   if(kind=="poison"){
    if(route.At(route.TimeAt(.23f)).z<3f)throw new Exception("Poison far cue missing");
    float last=route.At(route.TimeAt(.85f)).z;if(last<1f||last>1.1f)throw new Exception("Poison close recoil missing");
   }
   if(kind=="cake"){
    Vector3 a=route.At(route.TimeAt(.79f))-route.At(route.TimeAt(.78f)),b=route.At(route.TimeAt(.94f))-route.At(route.TimeAt(.93f));
    if(Vector3.Angle(a,b)<10)throw new Exception("Cake becomes straight near the player");
   }
   if(kind=="chest"){
    float high=route.At(route.TimeAt(.93f)).y,low=route.At(route.TimeAt(.995f)).y;
    if(high-low<.06f)throw new Exception("Chest height settles before contact");
   }
   if(kind=="bomb"){
    int peaks=0;float lastSlope=0;
    for(int i=1;i<100;i++){float t=i/100f;var a=route.At(route.TimeAt(t-.01f));var b=route.At(route.TimeAt(t));var c=route.At(route.TimeAt(t+.01f));float slope=c.y-b.y;
     if(lastSlope>0&&slope<=0)peaks++;lastSlope=slope;
     if(Mathf.Abs((c.x-b.x)-(b.x-a.x))>.00002f||c.y-2*b.y+a.y>=0||c.z>=b.z)throw new Exception("Bomb is not a descending gravity arc");
    }
    if(peaks!=1)throw new Exception("Bomb must have exactly one natural apex");
   }
  }
  Debug.Log("EARLY_TRICK_PASS: 4.2m start; tangent-matched launch; close slow entry unchanged; poison far and near recoil; cake and chest remain active in batting area; 33 bombs with one gravity apex and no lateral recoil");
  for(float time=2f;time<9;time+=.31f){
   if(Quaternion.Angle(PitchVisuals.Rotation("boomerang",time,8),PitchVisuals.Rotation("boomerang",time+.2f,8))<45)throw new Exception("Boomerang rotation settles");
  }
  Debug.Log("CONTINUOUS_TRICK_PASS: late curve remains curved; poison final recoil at 1.04m; chest hops through the batting zone; boomerang never stops spinning");
  foreach(string kind in new[]{"cake","poison","boomerang","magic","bomb","chest"}){
   float minimum=180;
   for(int variant=0;variant<=32;variant++){
    var route=new PitchPath(kind,0,1.22f,variant);Vector3 first=Vector3.zero;float bend=0;
    for(float time=route.SlowEntryTime+.01f;time<route.Duration-.01f;time+=.01f){
     var at=route.At(time);if(at.z>.95f||at.z<.52f)continue;
     var direction=route.At(time+.005f)-route.At(time-.005f);
     if(first==Vector3.zero)first=direction;else bend=Mathf.Max(bend,Vector3.Angle(first,direction));
    }
    if(bend<8)throw new Exception("Final hand-zone trajectory becomes a line: "+kind+" variant="+variant+" bend="+bend);
    minimum=Mathf.Min(minimum,bend);
   }
   Debug.Log("HAND_ZONE_CURVATURE_PASS "+kind+" min_degrees="+minimum);
  }
  // A curve after the bat already hits is insufficient: check the practical pre-contact zone.
  for(int variant=0;variant<=32;variant++){
   var route=new PitchPath("boomerang",0,1.22f,variant);Vector3 first=Vector3.zero;float bend=0;
   for(float time=route.SlowEntryTime;time<route.Duration-.01f;time+=.005f){
    float z=route.At(time).z;if(z>1.25f||z<.80f)continue;
    var direction=route.At(time+.002f)-route.At(time-.002f);
    if(first==Vector3.zero)first=direction;else bend=Mathf.Max(bend,Vector3.Angle(first,direction));
   }
   if(bend<12)throw new Exception("Boomerang bends only after practical contact: "+variant+" degrees="+bend);
  }
  Debug.Log("PRE_CONTACT_BOOMERANG_PASS: all 33 variants visibly bank before 0.80m");
  var targetSet=new System.Collections.Generic.HashSet<int>();
  foreach(float x in new[]{-.35f,-.10f,.10f,.35f})foreach(float dx in new[]{-.6f,0,.6f}){
   var shot=new OutboundShot(new Vector3(x,1.2f,.8f),new Vector3(dx,.1f,1),Vector3.back,"ball",0);targetSet.Add(Mathf.RoundToInt(shot.end.x));
   if(Mathf.Abs(new Vector2(shot.end.x-shot.start.x,shot.end.z-shot.start.z).magnitude-68)>.001f||shot.end.y>.11f)throw new Exception("Landing or strength invariant broken");
   float z=shot.start.z;for(float time=0;time<shot.seconds;time+=.01f){var at=shot.At(time);if(at.z<z||at.y<.09f)throw new Exception("Returned object comes back or enters ground");z=at.z;}
  }if(targetSet.Count<8)throw new Exception("Return direction still fixed");
  Debug.Log("REACTION_GEOMETRY_PASS: 256 bounded variable pitches; kind-specific 1.8/2.3m/s distant tricks and 0.4/0.6m/s close pace; varied swing-directed 68m landings");
  Debug.Log("GEOMETRY_CHECKS_PASS: 8 reachable paths; 3x fast; kind-specific slow pace; original strong band; stationary bat; separated magic groups");
 }
 static void BandChecks(){
  foreach(string kind in new[]{"ball","cake","poison","boomerang","magic","bomb","bullets","chest"}){
   foreach(var rotation in new[]{Quaternion.identity,Quaternion.Euler(27,-33,61)}){
    var axis=rotation*Vector3.up;var gate=new BatStrikeGate();float s;bool rejected;
    if(gate.Sweep(kind,rotation*new Vector3(0,1.3f,0),rotation*new Vector3(0,.555f,0),rotation,rotation,Vector3.zero,axis,Vector3.zero,axis,out s,out rejected)||!rejected)
     throw new Exception("Spear contact accepted: "+kind);
    if(gate.Sweep(kind,rotation*new Vector3(0,.555f,0),rotation*new Vector3(.02f,.50f,0),rotation,rotation,Vector3.zero,axis,Vector3.zero,axis,out s,out _))
     throw new Exception("Tip penetration became a band hit: "+kind);
    gate.Sweep(kind,rotation*new Vector3(.02f,.50f,0),rotation*new Vector3(1,.555f,0),rotation,rotation,Vector3.zero,axis,Vector3.zero,axis,out s,out _);
    if(gate.Blocked)throw new Exception("Contact does not reset after separation: "+kind);
    if(!gate.Sweep(kind,rotation*new Vector3(1,.555f,0),rotation*new Vector3(.03f,.555f,0),rotation,rotation,Vector3.zero,axis,Vector3.zero,axis,out s,out _)||!AFlight.IsStrong(s))
     throw new Exception("Band side does not hit after separation: "+kind);
    gate=new BatStrikeGate();
    if(gate.Sweep(kind,rotation*new Vector3(1,.10f,0),rotation*new Vector3(.03f,.10f,0),rotation,rotation,Vector3.zero,axis,Vector3.zero,axis,out s,out _))
     throw new Exception("Handle contact accepted: "+kind);
    float offset=kind=="ball"?.108f:kind=="magic"?.100f:kind=="bomb"?.148f:kind=="cake"?.188f:kind=="chest"?.218f:kind=="poison"?.123f:kind=="boomerang"?.230f:.0905f;
    var held=rotation*new Vector3(offset,.555f,0);gate=new BatStrikeGate();
    if(!gate.Sweep(kind,held,held,rotation,rotation,Vector3.zero,axis,Vector3.zero,axis,out s,out _)||!AFlight.IsStrong(s))
     throw new Exception("Stationary visible band rejected: "+kind);
   }
  }
  Debug.Log("BAND_ONLY_PASS: all 8 shapes, rotated poses, tip/handle rejection, continuous spear rejection, separation recovery, stationary band contacts");
 }
}
