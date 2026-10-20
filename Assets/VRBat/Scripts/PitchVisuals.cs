using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
namespace VRBat {
public static class PitchVisuals {
 public static string Tint(string kind)=>kind=="cake"?"#FFE2BD":kind=="poison"?"#80E8AC":kind=="boomerang"?"#FFA950":kind=="magic"?"#C6B6FA":kind=="bomb"?"#FF9F54":kind=="bullets"?"#E9CC8B":kind=="chest"?"#FFCE6B":"#FFF1D8";
 public static GameObject Create(StageView v,string kind,Transform parent){
  var root=new GameObject(kind);root.transform.SetParent(parent,false);
  if(kind=="poison"){
   var glass=BatGeometry.Material("#269B78",true);var liquid=v.Glow("#70DC89",1.15f);
   BatGeometry.Lathe("Emerald poison bottle",new[]{-.14f,-.13f,.025f,.055f,.115f,.12f},new[]{.03f,.085f,.085f,.038f,.038f,.03f},glass,root.transform);
   BatGeometry.Lathe("Cork",new[]{.11f,.15f},new[]{.038f,.034f},BatGeometry.Material("#B38D59"),root.transform);
   var mark=BatGeometry.Sphere("Skull label",new Vector3(0,-.025f,-.084f),new Vector3(.067f,.068f,.009f),v.white,root.transform);
   foreach(float x in new[]{-.016f,.016f})BatGeometry.Sphere("Skull eye",new Vector3(x,-.025f,-.09f),new Vector3(.015f,.019f,.005f),v.navy,root.transform);
   BatGeometry.Ring("Poison liquid rim",new Vector3(0,-.07f,0),.086f,.01f,liquid,root.transform,true);
  }else if(kind=="boomerang"){
   var wood=BatGeometry.Material("#D77932");var cream=BatGeometry.Material("#FFDEA0");
   for(int i=0;i<2;i++){float sign=i==0?-1:1;var wing=BatGeometry.Box("Boomerang wing",new Vector3(sign*.085f,.062f,0),new Vector3(.24f,.066f,.033f),wood,root.transform);wing.transform.localRotation=Quaternion.Euler(0,0,-sign*40);Object.Destroy(wing.GetComponent<Collider>());
    for(int j=0;j<2;j++){var stripe=BatGeometry.Box("Painted wing tip",new Vector3(sign*(.123f+j*.020f),.033f-j*.017f,-.019f),new Vector3(.015f,.064f,.004f),cream,root.transform);stripe.transform.localRotation=wing.transform.localRotation;Object.Destroy(stripe.GetComponent<Collider>());}}
  }else if(kind=="bullets"){
   var casing=BatGeometry.Lathe("Brass bullet",new[]{-.11f,-.10f,.048f,.075f,.11f},new[]{.046f,.052f,.052f,.038f,0},v.gold,root.transform);casing.transform.localRotation=Quaternion.Euler(-90,0,0);
   var band=BatGeometry.Lathe("Bullet casing ring",new[]{-.102f,-.087f},new[]{.054f,.054f},v.metal,root.transform);band.transform.localRotation=casing.transform.localRotation;
  }else if(kind=="magic"){
   BatGeometry.Sphere("Magic pearl",Vector3.zero,Vector3.one*.124f,BatGeometry.Material("#AA8ADC",true),root.transform);
   BatGeometry.Ring("Pearl orbit",Vector3.zero,.069f,.004f,v.Glow("#C1AEF0",1.1f),root.transform);
  }
  return root;
 }
 public static void SetMagic(GameObject actor,bool real,StageView v){var pearl=actor.transform.Find("Magic pearl");if(pearl)pearl.GetComponent<Renderer>().sharedMaterial=BatGeometry.Material(real?"#B5A5DE":"#AD9BDC",true);var orbit=actor.transform.Find("Pearl orbit");if(orbit)orbit.GetComponent<Renderer>().sharedMaterial=v.Glow(real?"#D1C5F0":"#C9BCEE",1.15f);}
 public static TrailRenderer Trail(GameObject actor,string kind,StageView v){var t=actor.AddComponent<TrailRenderer>();t.time=kind=="cake"?.8f:kind=="boomerang"?.65f:kind=="magic"?.20f:.28f;t.minVertexDistance=.014f;t.startWidth=kind=="cake"?.025f:.009f;t.endWidth=0;t.numCornerVertices=4;t.numCapVertices=4;t.material=v.Glow(Tint(kind),1.15f);t.startColor=Color.white;t.endColor=new Color(1,1,1,0);t.shadowCastingMode=ShadowCastingMode.Off;return t;}
 public static Quaternion Rotation(string kind,float time,float duration){
  // Driven by the shared presentation clock. Slow motion reduces rotation, but
  // proximity never switches it off or silently settles the target for the bat.
  if(kind=="boomerang")return Quaternion.Euler(12,15,time*330);
  if(kind=="bomb")return Quaternion.Euler(0,0,-12+time*38);
  if(kind=="cake")return Quaternion.Euler(8,Mathf.Sin(time*.8f)*25,Mathf.Sin(time*.65f)*-30);
  if(kind=="chest")return Quaternion.Euler(Mathf.Sin(time*1.4f)*12,Mathf.Sin(time*1.3f)*60,Mathf.Sin(time)*15);
  if(kind=="poison")return Quaternion.Euler(8,Mathf.Sin(time)*18,Mathf.Sin(time*1.8f)*20);
  return Quaternion.identity;
 }
 public static void Lid(GameObject actor,float amount){var anim=actor.GetComponentInChildren<Animation>();if(!anim)return;foreach(AnimationState s in anim)if(s.name.Contains("open")&&!s.name.Contains("close")){s.enabled=true;s.weight=1;s.normalizedTime=Mathf.Clamp01(amount);anim.Sample();s.enabled=false;return;}}
}
}
