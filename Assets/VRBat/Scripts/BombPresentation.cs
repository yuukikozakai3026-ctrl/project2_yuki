using System.Collections;
using UnityEngine;
namespace VRBat {
public partial class Effects {
 public void BombFuse(GameObject actor,Challenge pitch){StartCoroutine(BombFuseRoutine(actor,pitch));}
 IEnumerator BombFuseRoutine(GameObject actor,Challenge pitch){
  double next=game.effectTime;int n=0;var tip=actor.transform.Find("Burning fuse tip");
  game.Event("bomb_lob","rule=gravity_arc;recoil_turns=0");
  while(actor&&!pitch.done){
   if(actor.activeInHierarchy&&game.effectTime>=next){
    float progress=Mathf.Clamp01((float)(game.now-pitch.routeAt)/pitch.path.Duration);next=game.effectTime+Mathf.Lerp(.13f,.055f,progress);
    Vector3 p=tip?tip.position:actor.transform.TransformPoint(new Vector3(.07f,.23f,.07f));
    float a=n++*2.39996f;var velocity=new Vector3(Mathf.Cos(a)*.18f,.16f+Mathf.Repeat(n*.37f,1)*.16f,Mathf.Sin(a)*.18f);
    StartCoroutine(Particle(p,velocity,"spark_01",.020f+progress*.014f,.28f,new Color(1.6f,.85f,.23f,1)));
    if(n%3==0)StartCoroutine(Particle(p,Vector3.up*.17f+Vector3.forward*.05f,"smoke_01",.085f,.52f,new Color(.46f,.48f,.53f,.38f)));
    if(n%7==1)Sound("fuse_crackle",-25+progress*5,.88f,p);
   }
   yield return null;
  }
 }
}
}
