using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace VRBat {
public partial class Effects {
 readonly Dictionary<string,Material> festivalMaterials=new Dictionary<string,Material>();readonly List<ParticleSystem> festivalSystems=new List<ParticleSystem>();
 Color FestivalColor(string kind)=>BatGeometry.ColorOf(kind=="cake"?"#FF358D":kind=="poison"?"#35FF63":kind=="magic"?"#24CBFF":kind=="boomerang"?"#FF852A":kind=="bomb"?"#FF491B":"#FFCE32");
 Material FestivalMaterial(string texture){if(festivalMaterials.TryGetValue(texture,out var m))return m;m=new Material(Resources.Load<Shader>("FestivalParticles"));m.SetTexture("_MainTex",texture=="solid"?Texture2D.whiteTexture:V.particles.FirstOrDefault(x=>x&&x.name==texture));m.SetColor("_TintColor",new Color(2,2,2,1));festivalMaterials.Add(texture,m);return m;}
 ParticleSystem FestivalSystem(string texture,float life,bool trails=false){
  var o=new GameObject("Batched celebration "+texture);o.transform.SetParent(game.targets,false);var ps=o.AddComponent<ParticleSystem>();ps.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
  var main=ps.main;main.loop=false;main.duration=life;main.startLifetime=life;main.startSpeed=0;main.maxParticles=800;main.simulationSpace=ParticleSystemSimulationSpace.World;main.useUnscaledTime=true;main.playOnAwake=false;main.gravityModifier=.12f;main.stopAction=ParticleSystemStopAction.Destroy;
  var emission=ps.emission;emission.enabled=false;var shape=ps.shape;shape.enabled=false;
  var color=ps.colorOverLifetime;color.enabled=true;var gradient=new Gradient();gradient.SetKeys(new[]{new GradientColorKey(Color.white,0),new GradientColorKey(Color.white,1)},new[]{new GradientAlphaKey(1,0),new GradientAlphaKey(.8f,.45f),new GradientAlphaKey(0,1)});color.color=gradient;
  var size=ps.sizeOverLifetime;size.enabled=true;size.size=new ParticleSystem.MinMaxCurve(1,new AnimationCurve(new Keyframe(0,.5f),new Keyframe(.12f,1),new Keyframe(1,.1f)));
  var r=ps.GetComponent<ParticleSystemRenderer>();r.sharedMaterial=FestivalMaterial(texture);r.renderMode=ParticleSystemRenderMode.Billboard;r.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
  if(trails){var trail=ps.trails;trail.enabled=true;trail.ratio=.32f;trail.lifetime=.28f;trail.widthOverTrail=new ParticleSystem.MinMaxCurve(.35f);trail.dieWithParticles=true;r.trailMaterial=FestivalMaterial("trace_01");}
  ps.Play();festivalSystems.Add(ps);return ps;
 }
 void Update(){UpdateVoices();for(int i=festivalSystems.Count-1;i>=0;i--){var ps=festivalSystems[i];if(!ps){festivalSystems.RemoveAt(i);continue;}var main=ps.main;main.simulationSpeed=game.effectScale;if(game.paused&&ps.isPlaying)ps.Pause();else if(!game.paused&&ps.isPaused)ps.Play();}}
 void FestivalBurst(Vector3 p,string kind,int count,float speed,float size,float life,float forward,string texture="spark_01",bool trails=false){
  var ps=FestivalSystem(texture,life,trails);Color primary=FestivalColor(kind),secondary=kind=="poison"?BatGeometry.ColorOf("#CFFF57"):kind=="cake"?BatGeometry.ColorOf("#FFE6A1"):kind=="magic"?BatGeometry.ColorOf("#E778FF"):BatGeometry.ColorOf("#FFF4C9");
  for(int i=0;i<count;i++){float a=i*2.399963f,r=.35f+.65f*Hash(i+13);Vector3 velocity=new Vector3(Mathf.Cos(a),Mathf.Sin(a),.1f+Hash(i)*.35f)*speed*r+Vector3.forward*forward;
   var e=new ParticleSystem.EmitParams{position=p,velocity=velocity,startSize=size*(.35f+Hash(i+2)*.65f),startLifetime=life*(.68f+Hash(i+5)*.32f),startColor=Color.Lerp(primary,secondary,Hash(i+21)),rotation=i*137.5f};ps.Emit(e,1);
  }
 }
 public void SpectacleContact(Vector3 p,string kind,bool strong){
  if(V.spectacle)V.spectacle.Pulse(FestivalColor(kind),strong);
  FestivalBurst(p+Vector3.forward*.14f,kind,strong?190:120,strong?4.6f:3.8f,.12f,.64f,5.8f,kind=="magic"||kind=="chest"?"star_01":"spark_01");
  StartCoroutine(ImpactCrown(p,kind,strong));game.Event("spectacle_hit",$"kind={kind};strong={strong}");
 }
 IEnumerator ImpactCrown(Vector3 p,string kind,bool strong){
  var m=FestivalMaterial("light_01");int count=strong?22:15;var rays=new LineRenderer[count];Color c=FestivalColor(kind);
  for(int i=0;i<count;i++){rays[i]=BatGeometry.Line("Explosive impact streak",new[]{p,p},.022f,m,game.targets);rays[i].startColor=c;rays[i].endColor=new Color(c.r,c.g,c.b,0);rays[i].numCapVertices=3;}
  double at=game.effectTime;while(game.effectTime-at<.32){float t=(float)(game.effectTime-at)/.32f;for(int i=0;i<count;i++){float a=i*2*Mathf.PI/count;Vector3 dir=new Vector3(Mathf.Cos(a)*.8f,Mathf.Sin(a)*.7f,1.5f);float radius=Mathf.Sqrt(t)*(.8f+Hash(i)*.4f);rays[i].SetPosition(0,p+Vector3.forward*.10f+dir*Mathf.Max(.07f,radius-.30f));rays[i].SetPosition(1,p+Vector3.forward*.10f+dir*radius);rays[i].widthMultiplier=(strong?.033f:.025f)*(1-t);}
   yield return null;
  }foreach(var ray in rays)Destroy(ray.gameObject);
 }
 IEnumerator GlitterFlight(OutboundShot shot){
  var ps=FestivalSystem(shot.kind=="chest"||shot.kind=="magic"?"star_01":"spark_01",shot.seconds+1);double at=game.effectTime;float next=0;
  while(game.effectTime-at<shot.seconds){float t=(float)(game.effectTime-at);if(t>=next){next+=.06f;Vector3 p=shot.At(t);for(int i=0;i<5;i++){var e=new ParticleSystem.EmitParams{position=p,velocity=shot.side*(Hash(i+(int)(t*100))-.5f)*2+Vector3.up*(Hash(i+8)-.5f),startSize=.055f+p.z*.014f,startLifetime=.6f,startColor=FestivalColor(shot.kind)};ps.Emit(e,1);}}yield return null;}
 }
 IEnumerator SkyRays(Vector3 center,string kind,int part){
  int count=28;var rays=new LineRenderer[count];var mat=FestivalMaterial("solid");Color color=FestivalColor(kind);
  for(int i=0;i<count;i++){rays[i]=BatGeometry.Line("Curved firework ember",new Vector3[8],.16f,mat,game.targets);rays[i].widthCurve=new AnimationCurve(new Keyframe(0,0),new Keyframe(1,1));rays[i].startColor=new Color(color.r,color.g,color.b,0);rays[i].endColor=color;}
  double start=game.effectTime;while(game.effectTime-start<1.6){float t=(float)(game.effectTime-start);for(int i=0;i<count;i++){float a=i*2*Mathf.PI/count+part*.2f;Vector3 direction=new Vector3(Mathf.Cos(a),Mathf.Sin(a),.13f*Mathf.Sin(a*3));
    for(int j=0;j<8;j++){float age=Mathf.Max(0,t-.38f*(1-j/7f));float radius=(18+Hash(i)*3)*(1-Mathf.Exp(-age*1.8f));rays[i].SetPosition(j,center+direction*radius+Vector3.down*age*age*1.8f);}
    rays[i].widthMultiplier=.40f*Mathf.Clamp01((1.6f-t)/.7f);
   }yield return null;}
  foreach(var ray in rays)Destroy(ray.gameObject);
 }
 IEnumerator SkyCelebration(OutboundShot shot,bool strong,int item){
  if(shot.kind=="bullets"&&item!=1)yield break;
  if(V.spectacle)V.spectacle.Pulse(FestivalColor(shot.kind),strong);
  int bursts=strong?3:2;Vector3 source=shot.end;
  for(int j=0;j<bursts;j++){
   Vector3 center=new Vector3(source.x*.25f+8+(j-(bursts-1)*.5f)*14,20+j*1.5f,Mathf.Max(64,source.z)+8+j*3);
   var m=FestivalMaterial("trace_01");var comet=BatGeometry.Line("Celebration sky comet",new[]{source,source},.14f,m,game.targets);comet.startColor=FestivalColor(shot.kind);comet.endColor=Color.white;
   double at=game.effectTime;while(game.effectTime-at<.24){float u=(float)(game.effectTime-at)/.24f;comet.SetPosition(0,Vector3.Lerp(source,center,Mathf.Max(0,u-.18f)));comet.SetPosition(1,Vector3.Lerp(source,center,u));yield return null;}Destroy(comet.gameObject);
   StartCoroutine(SkyRays(center,shot.kind,j));
   FestivalBurst(center,shot.kind,strong?150:105,15+j*1.3f,1.8f,1.6f,0,"spark_01",true);
   FestivalBurst(center,shot.kind,28,7,.95f,1.15f,0,"star_01");Sound("fuse_crackle",-19,.65f+j*.15f,center);
   game.Event("spectacle_sky",$"kind={shot.kind};part={j};z={center.z:F1}");yield return EffectWait(.06f);
  }
 }
}
}
