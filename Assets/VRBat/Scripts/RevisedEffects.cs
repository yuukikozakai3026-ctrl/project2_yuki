using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace VRBat {
public partial class Effects {
 public void Dissolve(GameObject actor,float seconds){if(actor)StartCoroutine(DissolveRoutine(actor,seconds));}
 IEnumerator DissolveRoutine(GameObject actor,float seconds){
  var p=actor.transform.position;var scale=actor.transform.localScale;double start=game.effectTime;var materials=new List<Material>();var colors=new List<Color>();
  foreach(var renderer in actor.GetComponentsInChildren<Renderer>()){var m=new Material(renderer.sharedMaterial);m.SetFloat("_Surface",1);m.SetFloat("_SrcBlend",5);m.SetFloat("_DstBlend",10);m.SetFloat("_ZWrite",0);m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");m.renderQueue=3000;m.SetShaderPassEnabled("ShadowCaster",false);renderer.sharedMaterial=m;materials.Add(m);colors.Add(m.GetColor("_BaseColor"));}
  foreach(var trail in actor.GetComponentsInChildren<TrailRenderer>())trail.emitting=false;
  while(actor&&game.effectTime-start<seconds){float t=(float)(game.effectTime-start)/seconds;actor.transform.position=p+Vector3.up*(.12f*t)+Vector3.forward*(.05f*t);actor.transform.localScale=scale*(1+.12f*t);for(int i=0;i<materials.Count;i++){var c=colors[i];c.a=1-Mathf.SmoothStep(0,1,t);materials[i].SetColor("_BaseColor",c);}yield return null;}
  if(actor)Destroy(actor);foreach(var m in materials)Destroy(m);
 }
 public void SoftMiss(GameObject actor,Vector3 velocity){if(actor)StartCoroutine(MissAway(actor,velocity));}
 IEnumerator MissAway(GameObject actor,Vector3 velocity){var p=actor.transform.position;var scale=actor.transform.localScale;double start=game.effectTime;while(actor&&game.effectTime-start<.42){float t=(float)(game.effectTime-start)/.42f;actor.transform.position=p+velocity*(t*.42f);actor.transform.localScale=scale*(1-t);yield return null;}if(actor)Destroy(actor);}
 public void RecoilCue(GameObject actor,Vector3 direction,string kind,Challenge flight,float forceAt,float stopAt){if(actor)StartCoroutine(RecoilJet(actor,direction,kind,flight,forceAt,stopAt));}
 IEnumerator RecoilJet(GameObject actor,Vector3 direction,string kind,Challenge flight,float forceAt,float stopAt){
  bool poison=kind=="poison";Sound(poison?"poison_jet":"fuse_crackle",poison?-12:-14,1);
  var mat=poison?new Material(V.particleTemplate):V.Glow("#FFCA75",1.6f);
  if(poison){foreach(var tex in V.particles)if(tex&&tex.name=="smoke_01")mat.SetTexture("_BaseMap",tex);mat.SetColor("_BaseColor",new Color(.62f,1.3f,.84f,.85f));}
  var jet=BatGeometry.Line(poison?"Visible gas jet":"Fuse reaction spark",new[]{Vector3.zero,Vector3.zero},1,mat,game.targets);
  jet.startWidth=poison?.018f:.004f;jet.endWidth=poison?.13f:.001f;jet.startColor=new Color(1,1,1,.95f);jet.endColor=new Color(1,1,1,0);
  var core=BatGeometry.Sphere("Recoil ignition",Vector3.zero,Vector3.one*.03f,V.Glow(poison?"#A4EFC3":"#FFD17A",1.3f),game.targets);
  var rays=new List<LineRenderer>();if(!poison)for(int i=0;i<5;i++)rays.Add(BatGeometry.Line("Fuse pin spark",new[]{Vector3.zero,Vector3.zero},.005f,mat,game.targets));
  double start=game.now,startFx=game.effectTime;float nextPuff=0;
  while(actor&&!flight.done&&game.now<flight.routeAt+stopAt){float t=(float)(game.effectTime-startFx);float local=(float)(game.now-flight.routeAt);
   if(local>forceAt+.06f){
    float h=Mathf.Min(.04f,Mathf.Min(local,flight.path.Duration-local)*.45f);var center=flight.path.At(local);var acceleration=h>.003f?flight.path.At(local+h)-2*center+flight.path.At(local-h):Vector3.zero;
    if(acceleration.sqrMagnitude>1e-12f)direction=Vector3.Slerp(direction,game.D(-acceleration.normalized),Mathf.Clamp01(game.effectDelta*14));
   }
   Vector3 p=poison?actor.transform.TransformPoint(new Vector3(0,.15f,0)):actor.transform.position+new Vector3(.07f,.23f,.07f);float pulse=Mathf.Clamp01((float)(game.now-start)/.06f)*(.42f+.58f*Mathf.Exp(-t*4));core.transform.position=p;core.transform.localScale=Vector3.one*(poison?.018f:.037f)*pulse;
   jet.SetPosition(0,p);jet.SetPosition(1,p+direction*(poison?.40f:.12f)*Mathf.Clamp01(t/.07f));jet.widthMultiplier=pulse;
   for(int i=0;i<rays.Count;i++){float a=i*Mathf.PI*2/5+t*3;Vector3 d=new Vector3(Mathf.Cos(a),Mathf.Sin(a),0);rays[i].SetPosition(0,p+d*.022f);rays[i].SetPosition(1,p+d*(.055f+.055f*pulse));rays[i].widthMultiplier=.005f*pulse;}
   if(t>=nextPuff){nextPuff+=.09f;for(int i=0;i<3;i++){Vector3 spread=new Vector3((i-1)*.16f,(i%2-.5f)*.17f,0);StartCoroutine(Particle(p,direction*(poison?.85f:.6f)+spread,poison?"smoke_01":"spark_01",poison?.19f:.044f,poison?.5f:.28f,poison?new Color(.65f,1.12f,.79f,.72f):new Color(1.4f,.85f,.30f,1)));}}
   yield return null;
  }Destroy(jet.gameObject);foreach(var ray in rays)Destroy(ray.gameObject);Destroy(core.GetComponent<Renderer>().sharedMaterial);Destroy(core);Destroy(mat);
 }
}
}
