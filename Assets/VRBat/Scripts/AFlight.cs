using System;
using UnityEngine;
namespace VRBat {
// Distances are metres. Incoming path timing is independent of the presentation clock; XR stays real time.
public static class AFlight {
 public const float CenterLaunchZ=7.38f, SideLaunchZ=6.98f;
 public const float BaselineSpeed=(7.38f-.86f)/1.6f;
 public const float FastSpeed=BaselineSpeed*3f, SlowSpeed=.40f, EndZ=.48f;
 public const float OriginalSlowZ=2.0f, SlowRangeScale=2f/3f, SlowZ=EndZ+(OriginalSlowZ-EndZ)*SlowRangeScale;
 public const float TrickStartZ=4.2f, TrickSpeed=1.8f;
 public static bool Challenging(string kind)=>kind=="boomerang"||kind=="poison"||kind=="magic";
 public static float NearSpeed(string kind)=>Challenging(kind)?.60f:SlowSpeed;
 public static float FarSpeed(string kind)=>Challenging(kind)?2.3f:TrickSpeed;
 public static bool EarlyTrick(string kind)=>kind=="magic"||kind=="cake"||kind=="poison"||kind=="boomerang"||kind=="bomb"||kind=="chest";
 public const float FarTargetShift=3.69f; // retained only for legacy effects, unused by revised A
 public const float ReturnDistance=68f, ReturnSeconds=2.6f;
 public static bool IsStrong(float s)=>s>=.43f&&s<=.68f;
 public static Vector3 ReturnPosition(Vector3 start,Vector3 direction,float elapsed){
  float t=Mathf.Clamp01(elapsed/ReturnSeconds);
  return start+direction*(ReturnDistance*t)+Vector3.up*(7f*4*t*(1-t));
 }
 public static Vector3 Fast(Vector3 from,Vector3 to,float elapsed,float duration){
  // A short brake, after a full-speed segment, makes the transition visible without overshoot.
  const float brake=.055f;float length=Vector3.Distance(from,to),cruise=duration-brake;
  float travelled=elapsed<=cruise?FastSpeed*elapsed:FastSpeed*cruise+FastSpeed*(elapsed-cruise)-.5f*(FastSpeed-SlowSpeed)/brake*Mathf.Pow(elapsed-cruise,2);
  return Vector3.Lerp(from,to,Mathf.Clamp01(travelled/length));
 }
 public static float FastDuration(Vector3 from,Vector3 to)=>(Vector3.Distance(from,to)-.055f*(FastSpeed+SlowSpeed)*.5f)/FastSpeed+.055f;
}
// Arc length timing preserves each kind's chosen pace around curves and recoil corners.
public sealed class PitchPath {
 const int Samples=600;
 readonly Vector3[] points=new Vector3[Samples+1];readonly float[] distances=new float[Samples+1],times=new float[Samples+1];
 public float CruiseSpeed {get;private set;}
 public float SlowEntryTime {get;private set;}
 public Vector3 SlowEntry=>At(SlowEntryTime);
 public Vector3 StartTangent=>(points[1]-points[0]).normalized;
 public float Length=>distances[Samples];public float Duration=>times[Samples];
 public Vector3 Start=>points[0];public Vector3 End=>points[Samples];
 public PitchPath(string kind,int lane,float height,int variant=0){
  CruiseSpeed=AFlight.FarSpeed(kind);
  for(int i=0;i<=Samples;i++){
   points[i]=VariedShape(kind,i/(float)Samples,lane,height,variant);
   if(i>0){float segment=Vector3.Distance(points[i],points[i-1]);distances[i]=distances[i-1]+segment;
    float z=(points[i].z+points[i-1].z)*.5f;float speed=Mathf.Lerp(AFlight.NearSpeed(kind),AFlight.FarSpeed(kind),Mathf.SmoothStep(0,1,Mathf.InverseLerp(AFlight.SlowZ,AFlight.SlowZ+.45f,z)));
    times[i]=times[i-1]+segment/(AFlight.EarlyTrick(kind)?speed:AFlight.NearSpeed(kind));
    if(points[i-1].z>AFlight.SlowZ&&points[i].z<=AFlight.SlowZ)SlowEntryTime=Mathf.Lerp(times[i-1],times[i],Mathf.InverseLerp(points[i-1].z,points[i].z,AFlight.SlowZ));
   }
  }
 }
 public float TimeAt(float u){float a=Mathf.Clamp01(u)*Samples;int n=Mathf.Min(Samples-1,(int)a);return Mathf.Lerp(times[n],times[n+1],a-n);}
 public float ProgressAt(float elapsed){float time=Mathf.Clamp(elapsed,0,Duration);int lo=0,hi=Samples;while(hi-lo>1){int m=(lo+hi)/2;if(times[m]<time)lo=m;else hi=m;}return Mathf.Lerp(lo,hi,Mathf.InverseLerp(times[lo],times[hi],time))/Samples;}
 public Vector3 At(float elapsed){float time=Mathf.Clamp(elapsed,0,Duration);int lo=0,hi=Samples;while(hi-lo>1){int m=(lo+hi)/2;if(times[m]<time)lo=m;else hi=m;}return Vector3.Lerp(points[lo],points[hi],Mathf.InverseLerp(times[lo],times[hi],time));}
 // A low-discrepancy sequence varies the finish without moving it after launch.
 public static Vector3 FinishOffset(int variant){if(variant==0)return Vector3.zero;return new Vector3((Mathf.Repeat(variant*.618034f,1)-.5f)*.36f,(Mathf.Repeat(variant*.414214f,1)-.5f)*.16f,0);}
 static Vector3 VariedShape(string kind,float t,int lane,float h,int variant){
  Vector3 d=FinishOffset(variant),p=Shape(kind,t,lane,h);
  // Uniformly shrink each special route around its reachable finish, preserving its shape.
  if(kind!="ball"){var anchor=new Vector3(lane*.22f,h,AFlight.EndZ);p=anchor+(p-anchor)*AFlight.SlowRangeScale;}
  // Start the characteristic route farther away without enlarging its lateral reach.
  if(AFlight.EarlyTrick(kind))p.z=AFlight.EndZ+(p.z-AFlight.EndZ)*(AFlight.TrickStartZ-AFlight.EndZ)/(AFlight.SlowZ-AFlight.EndZ);
  if(kind=="bomb")return new Vector3(lane*.22f+d.x*t,h+.45f*(1-t)+1.8f*t*(1-t)+d.y*t,AFlight.EndZ+(AFlight.TrickStartZ-AFlight.EndZ)*Mathf.Pow(1-t,1.45f));
  if(kind=="ball"){float z=p.z;float f=(z-AFlight.EndZ)/((lane==0?AFlight.CenterLaunchZ:AFlight.SideLaunchZ)-AFlight.EndZ);return p+d*(1-f);}
  if(variant>0&&(kind=="cake"||kind=="boomerang"||kind=="poison")){float sign=variant%2==0?-1:1;p.x=lane*.22f+(p.x-lane*.22f)*sign*(.82f+.18f*Mathf.Repeat(variant*.75488f,1));}
  // Each announced jet changes the force. Keep curving under that force, without homing.
  if(kind=="poison"){
   float lastImpulse=.85f;
   return p+new Vector3(d.x*Mathf.InverseLerp(lastImpulse,1,t),d.y*t,0);
  }
  if(kind=="cake")return p+d*t;
  if(kind=="boomerang")return p+d*Mathf.SmoothStep(0,1,Mathf.Min(1,t/.60f));
  return p+d*Mathf.SmoothStep(0,1,t);
 }
 public static Vector3 Shape(string kind,float t,int lane,float h){
  const float EndForLine=AFlight.EndZ;float x=lane*.22f,z=Mathf.Lerp(AFlight.OriginalSlowZ,AFlight.EndZ,t);float y=h;
  // Curvature continues through the batting area; never cut it off to force a final line.
  float envelope=Mathf.Sin(Mathf.PI*(kind=="cake"?t*t*t:t));
  if(kind=="ball"){z=AFlight.EndZ+(z-AFlight.EndZ)*AFlight.SlowRangeScale;float f=(z-EndForLine)/( (lane==0?AFlight.CenterLaunchZ:AFlight.SideLaunchZ)-EndForLine);x=lane*(.22f+(1.65f-.22f)*f);y=h+(1.45f-h)*f;}
  else if(kind=="cake"){x+=.84f*envelope;y+=.15f*envelope;}
  else if(kind=="magic"){x+=.26f*Mathf.Sin(4*Mathf.PI*t);y+=.075f*(Mathf.Cos(4*Mathf.PI*t)-1);}
  else if(kind=="boomerang"){
   // The returning arm is a banked arc, ending in a lateral sweep rather than a straight lead-in.
   Vector3 p3=new Vector3(.70f,h+.10f,2.16f),c1=new Vector3(.69f,h+.10f,2.25f),c2=new Vector3(.48f,h+.10f,.75f),finish=new Vector3(0,h,.48f),curve;
   if(t>=.60f){float u=(t-.60f)/.40f,v=1-u;curve=v*v*v*p3+3*v*v*u*c1+3*v*u*u*c2+u*u*u*finish;curve.x=.70f-.03f*u-.67f*Mathf.Pow(u,8);}
   else {
    Vector3 p2=new Vector3(.91f,h+.12f,1.88f);
    Vector3[] p={new Vector3(.10f,h,2),new Vector3(.72f,h+.08f,1.64f),p2,p3,p2+3*(c1-p3)};
    float q=t*5;int n=Mathf.Min(2,(int)q);float f=q-n;Vector3 a=p[Mathf.Max(0,n-1)],b=p[n],c=p[n+1],d=p[n+2];
    curve=.5f*((2*b)+(-a+c)*f+(2*a-5*b+4*c-d)*f*f+(-a+3*b-3*c+d)*f*f*f);
   }
   curve.x*=1.28f;return new Vector3(x,0,0)+curve;
  }else if(kind=="poison"){
   if(t>=.85f){float u=(t-.85f)/.15f;x+=.74f*(1-u)*(1+.65f*u);}
   else if(t>=.23f){float u=(t-.23f)/.62f;x+=.74f*(.4f*u+.6f*u*u);}
  
  }else if(kind=="chest"){
   float phase=6.5f*Mathf.PI*t;y+=.18f*Mathf.Cos(phase);x+=.14f*(Mathf.Sin(phase)-t);
  }
  return new Vector3(x,y,z);
 }
 static float Piecewise(float t,float[] times,float[] values){for(int i=1;i<times.Length;i++)if(t<=times[i])return Mathf.Lerp(values[i-1],values[i],Mathf.InverseLerp(times[i-1],times[i],t));return values[values.Length-1];}
}
// A tangent-matched lead-in avoids a sharp steering change when the distant trick begins.
public sealed class PitchApproach {
 const int N=96;readonly Vector3[] p=new Vector3[N+1];readonly float[] d=new float[N+1];readonly float endSpeed,brake;public float Duration {get;private set;}
 public PitchApproach(Vector3 from,PitchPath path,bool curved){
  endSpeed=curved?path.CruiseSpeed:AFlight.SlowSpeed;brake=curved?.22f:.055f;float chord=Vector3.Distance(from,path.Start);
  Vector3 direction=(path.Start-from).normalized,a=from+direction*(chord/3),b=path.Start-(curved?path.StartTangent:direction)*(chord/3);
  for(int i=0;i<=N;i++){float t=i/(float)N,u=1-t;p[i]=u*u*u*from+3*u*u*t*a+3*u*t*t*b+t*t*t*path.Start;if(i>0)d[i]=d[i-1]+Vector3.Distance(p[i-1],p[i]);}
  Duration=(d[N]-brake*(AFlight.FastSpeed+endSpeed)*.5f)/AFlight.FastSpeed+brake;
 }
 public Vector3 At(float time){float cruise=Duration-brake,x=time<=cruise?AFlight.FastSpeed*time:AFlight.FastSpeed*cruise+AFlight.FastSpeed*(time-cruise)-.5f*(AFlight.FastSpeed-endSpeed)/brake*Mathf.Pow(time-cruise,2);x=Mathf.Clamp(x,0,d[N]);int lo=0,hi=N;while(hi-lo>1){int m=(lo+hi)/2;if(d[m]<x)lo=m;else hi=m;}return Vector3.Lerp(p[lo],p[hi],Mathf.InverseLerp(d[lo],d[hi],x));}
}

}
