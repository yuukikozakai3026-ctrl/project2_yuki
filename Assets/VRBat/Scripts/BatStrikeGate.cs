using UnityEngine;
namespace VRBat {
// Only first contact on the painted barrel can launch a target. A rejected touch
// must separate completely before it can become a new hit; pushing the tip through
// an object until it overlaps the band is never a valid strike. No speed threshold.
public sealed class BatStrikeGate {
 public bool Blocked {get;private set;}
 public bool Sweep(string kind,Vector3 from,Vector3 to,Quaternion oldRotation,Quaternion rotation,
  Vector3 oldGrip,Vector3 oldAxis,Vector3 grip,Vector3 axis,out float hitS,out bool rejected){
  rejected=false;hitS=0;
  float travel=Vector3.Distance(from,to)+Vector3.Distance(oldGrip,grip)+.85f*Vector3.Distance(oldAxis,axis)+Quaternion.Angle(oldRotation,rotation)*.004f;
  int steps=Mathf.Clamp(Mathf.CeilToInt(travel/.005f),1,256);
  for(int k=0;k<=steps;k++){
   float t=k/(float)steps;var inverse=Quaternion.Inverse(Quaternion.Slerp(oldRotation,rotation,t));
   var localGrip=inverse*(Vector3.Lerp(oldGrip,grip,t)-Vector3.Lerp(from,to,t));var localAxis=inverse*Vector3.Slerp(oldAxis,axis,t).normalized;
   bool touching=false,band=false;float candidate=0;
   if(kind=="ball"||kind=="magic"||kind=="bomb")Sphere(Vector3.zero,kind=="magic"?.062f:kind=="bomb"?.11f:.07f,localGrip,localAxis,ref touching,ref band,ref candidate);
   else if(kind=="boomerang"){
    for(int j=0;j<=12;j++){float x=(j-6)*.027f;Sphere(new Vector3(x,.14f-Mathf.Abs(x)*.85f,0),.034f,localGrip,localAxis,ref touching,ref band,ref candidate);}
   }else{
    Vector3 size=kind=="cake"?new Vector3(.30f,.25633f,.30f):kind=="chest"?new Vector3(.36f,.32f,.34f):kind=="poison"?new Vector3(.17f,.29f,.17f):new Vector3(.105f,.105f,.22f);
    var box=new Bounds(Vector3.zero,size);touching=BatGeometry.BoxContact(box,localGrip,localAxis,out _);
    float best=float.PositiveInfinity;
    for(int j=0;j<=100;j++){
     float s=Mathf.Lerp(.43f,.68f,j/100f);var p=localGrip+localAxis*s;var delta=box.ClosestPoint(p)-p;float d=delta.sqrMagnitude;
     // A visible side of the band, not an artificial end cap or an embedded axis.
     if(d>.000001f&&d<=.039f*.039f&&Mathf.Abs(Vector3.Dot(delta,localAxis))<.008f&&d<best){best=d;candidate=s;band=true;}
    }
    touching|=band;
   }
   if(!touching){Blocked=false;continue;}
   if(Blocked)continue;
   if(band){hitS=candidate;return true;}
   Blocked=true;rejected=true;
  }
  return false;
 }
 static void Sphere(Vector3 center,float radius,Vector3 grip,Vector3 axis,ref bool touching,ref bool band,ref float candidate){
  if(!BatGeometry.SphereContact(center,radius,grip,axis,out float s,out _))return;
  touching=true;float projection=Vector3.Dot(center-grip,axis);var radial=center-grip-axis*projection;
  if(projection>=.43f-.000001f&&projection<=.68f+.000001f&&radial.sqrMagnitude>.000001f){band=true;candidate=Mathf.Clamp(s,.43f,.68f);}
 }
}
}
