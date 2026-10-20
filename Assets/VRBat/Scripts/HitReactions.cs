using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
namespace VRBat {
public partial class Effects {
 double lastApplause=-10;
 Material FadeMaterial(string tint,float emission=1){var m=V.Glow(tint,emission);m.SetFloat("_Surface",1);m.SetFloat("_SrcBlend",5);m.SetFloat("_DstBlend",10);m.SetFloat("_ZWrite",0);m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");m.renderQueue=3000;return m;}
 public void HitFar(GameObject actor,Vector3 p,string kind,Vector3 offset,bool strong,Vector3 swing,Vector3 incoming,int variant,int item){
  foreach(var trail in actor.GetComponentsInChildren<TrailRenderer>()){trail.Clear();trail.emitting=false;}
  var shot=new OutboundShot(p,swing,incoming,kind,variant);
  game.Event("return_begin",$"kind={kind};strong={strong};distance={AFlight.ReturnDistance:F1};target_x={shot.end.x:F3};target_y={shot.end.y:F3};target_z={shot.end.z:F3};item={item}");
  StartCoroutine(ReactionFlight(actor,shot,strong,variant,item));
 }
 public void BulletVolley(Vector3 p){StartCoroutine(VolleyAccent(p));}
 IEnumerator VolleyAccent(Vector3 p){
  yield return EffectWait(.06f);Sound("confirmation_001",-20,1.45f,p);
  var m=FadeMaterial("#FFE3A0",1.15f);var arc=BatGeometry.Line("Three bullet volley accent",new Vector3[12],.012f,m,game.targets);double start=game.effectTime;
  while(game.effectTime-start<.18f){float t=(float)(game.effectTime-start)/.18f;for(int i=0;i<12;i++){float u=i/11f;arc.SetPosition(i,p+new Vector3((u-.5f)*.44f,Mathf.Sin(u*Mathf.PI)*.07f,.22f+t*.5f));}arc.widthMultiplier=.012f*(1-t);yield return null;}Destroy(arc.gameObject);Destroy(m);
 }
 IEnumerator ContactAccent(Vector3 p,string kind,bool strong){
  // Concentration marks occupy a small volume on the far side of contact, not the screen.
  bool metal=kind=="bullets"||kind=="bomb",soft=kind=="cake"||kind=="poison";
  if(metal)Burst(p+Vector3.forward*.10f,"spark_01",strong?12:7,.022f,3.3f,.23f,BatGeometry.ColorOf("#F8BC63"));
  if(soft||kind=="bullets")yield break;
  var m=FadeMaterial(PitchVisuals.Tint(kind),1.5f);var rays=new List<LineRenderer>();
  int count=kind=="magic"?5:3;
  for(int i=0;i<count;i++){float a=(i+.24f)*2*Mathf.PI/count;var d=new Vector3(Mathf.Cos(a)*.34f,Mathf.Sin(a)*.24f,.65f);rays.Add(BatGeometry.Line("Short spatial contact accent",new[]{p+d*.12f,p+d*.40f},.009f,m,game.targets));}
  double at=game.effectTime;
  while(game.effectTime-at<.18){float t=(float)(game.effectTime-at)/.18f;for(int i=0;i<count;i++){float a=(i+.24f)*2*Mathf.PI/count;var d=new Vector3(Mathf.Cos(a)*.34f,Mathf.Sin(a)*.24f,.65f);rays[i].SetPosition(0,p+d*(.12f+t*.35f));rays[i].SetPosition(1,p+d*(.40f+t*.35f));rays[i].widthMultiplier=.009f*(1-t);}yield return null;}
  foreach(var r in rays)Destroy(r.gameObject);Destroy(m);
  if(strong&&kind!="bullets")StartCoroutine(ForwardConfetti(p+Vector3.forward*.3f,kind,7,.42f));
 }
 IEnumerator ReactionFlight(GameObject original,OutboundShot shot,bool strong,int variant,int item){
  string kind=shot.kind;Vector3 p=shot.start;
  // A few frames of local deformation make the break readable before pieces depart.
  Vector3 scale=original.transform.localScale;double compress=game.effectTime;
  while(original&&game.effectTime-compress<.065){float t=(float)(game.effectTime-compress)/.065f;original.transform.localScale=Vector3.Scale(scale,new Vector3(1+.12f*Mathf.Sin(t*Mathf.PI),1-.16f*Mathf.Sin(t*Mathf.PI),1));yield return null;}
  if(!original)yield break;original.transform.localScale=scale;
  GameObject hero=original;foreach(var r in original.GetComponentsInChildren<Renderer>())r.shadowCastingMode=ShadowCastingMode.Off;
  if(kind=="cake"){
   Destroy(original);hero=CakePiece("Large cake slice",.24f);hero.transform.position=p;
   for(int i=0;i<22;i++){float size=.035f+.045f*Hash(i+variant*9);var o=i%4==0?V.Model(V.strawberry,"Tumbling strawberry",p,Vector3.one*(size*.95f),false,game.targets):CakePiece("Broken sponge and cream",size);StartCoroutine(Debris(o,shot,i,1.1f+.25f*Hash(i+7),false));}
   for(int i=0;i<5;i++)StartCoroutine(Particle(p+Vector3.forward*.08f,new Vector3((Hash(i)-.5f)*1.3f,.25f+Hash(i+2),2.4f),"smoke_01",.09f,.35f,new Color(1,.89f,.72f,.5f)));
  }else if(kind=="poison"){
   Destroy(original);hero=GlassShard("Leading green glass shard",.17f);
   for(int i=0;i<24;i++){var shard=GlassShard("Poison glass",.035f+Hash(i)*.04f);StartCoroutine(Debris(shard,shot,i,.75f+.3f*Hash(i),false));}
   for(int i=0;i<5;i++)StartCoroutine(Particle(p+Vector3.forward*.30f,new Vector3((Hash(i+1)-.5f)*.9f,.3f+Hash(i)*.3f,3.4f),"smoke_01",.20f,.48f,new Color(.27f,.85f,.50f,.36f)));
  }else if(kind=="chest"){
   PitchVisuals.Lid(original,1);Sound("confirmation_001",-13,1.0f,p);
   for(int i=0;i<34;i++){GameObject treasure=i%5==0?BatGeometry.Gem("Tumbling gemstone",new Vector3(.08f,.13f,.07f),i%2==0?V.teal:V.white,game.targets):BatGeometry.Lathe("Spinning coin",new[]{-.008f,-.007f,.007f,.008f},new[]{0f,.047f,.047f,0},V.gold,game.targets,12);StartCoroutine(Debris(treasure,shot,i,1.45f+Hash(i)*.4f,true));}
   var gem=BatGeometry.Gem("Hero emerald",new Vector3(.26f,.36f,.24f),V.teal,game.targets);StartCoroutine(TreasureHero(gem,shot));
  }
  var material=FadeMaterial(PitchVisuals.Tint(kind),kind=="magic"?2.8f:2.0f);
  StartCoroutine(GlitterFlight(shot));
  int samples=30;var trail=BatGeometry.Line("Outbound "+kind+" ribbon",new Vector3[samples],.02f,material,game.targets);trail.numCornerVertices=3;trail.numCapVertices=3;trail.widthCurve=new AnimationCurve(new Keyframe(0,0),new Keyframe(1,1));
  foreach(var r in hero.GetComponentsInChildren<Renderer>())r.shadowCastingMode=ShadowCastingMode.Off;
  LineRenderer outer=null;Material outerMat=null;if(strong&&(kind=="ball"||kind=="magic")){outerMat=FadeMaterial(PitchVisuals.Tint(kind),.8f);outer=BatGeometry.Line("Strong hit second tail",new Vector3[samples],.01f,outerMat,game.targets);outer.widthCurve=trail.widthCurve;}
  double start=game.effectTime;float next=.12f,nextSample=.25f;bool far=false,wind=false;
  while(hero&&game.effectTime-start<shot.seconds){float t=(float)(game.effectTime-start);Vector3 at=shot.At(t);hero.transform.position=at;if(t>=nextSample){nextSample+=.5f;game.Event("reaction_sample",$"kind={kind};elapsed={t:F3};x={at.x:F3};y={at.y:F3};z={at.z:F3}");}
   if(kind=="boomerang")hero.transform.Rotate(0,0,680*game.effectDelta,Space.Self);else if(kind=="cake"||kind=="poison"||kind=="chest")hero.transform.Rotate(100*game.effectDelta,85*game.effectDelta,40*game.effectDelta);
   float tail=kind=="boomerang"?.75f:kind=="magic"?.34f:kind=="bomb"?.18f:kind=="cake"?.32f:kind=="poison"?.23f:.18f;
   float width=kind=="cake"?.080f:kind=="poison"?.060f:kind=="boomerang"?.10f:kind=="bullets"?.055f:.075f;
   tail*=Mathf.Lerp(.12f,1,Mathf.Clamp01(t/.70f));if(kind=="boomerang")tail=Mathf.Min(tail,Mathf.Lerp(.09f,.70f,Mathf.InverseLerp(.85f,1.5f,t)));
   trail.enabled=kind!="poison"&&kind!="chest"&&(kind!="cake"||at.z>8);if(outer)outer.enabled=trail.enabled;
   trail.widthMultiplier=width*Mathf.Lerp(.15f,1,Mathf.InverseLerp(3,18,at.z))*(1+Mathf.Min(at.z,55)*.045f);
   for(int i=0;i<samples;i++){Vector3 q=shot.At(Mathf.Max(0,t-tail*(1-i/(samples-1f))));trail.SetPosition(i,q);if(outer)outer.SetPosition(i,q+shot.side*(.025f+at.z*.002f));}
   if(outer)outer.widthMultiplier=trail.widthMultiplier*.35f;
   if(!wind&&t>.26){wind=true;Sound(kind=="boomerang"?"boomerang_whoosh":"flight_air",-22,kind=="bullets"?1.3f:1,at);}
   if(at.z>35&&!far){far=true;game.Event("return_far",$"kind={kind};z={at.z:F2}");}
   if(t>=next){next+=kind=="bomb"?.07f:.16f;
    if(kind=="bomb")StartCoroutine(Particle(at, -shot.direction*3+Vector3.up*.2f,"spark_01",.07f+at.z*.005f,.30f,new Color(1.6f,.65f,.10f,1)));
    else if(kind=="poison"&&t<2.7f)StartCoroutine(Particle(at,Vector3.up*.5f,"smoke_01",.18f+at.z*.045f,.46f,new Color(.32f,.9f,.53f,.40f)));
    else if(kind=="magic")StartCoroutine(Particle(at,-shot.side*.4f,"star_01",.06f+at.z*.003f,.30f,new Color(.55f,.88f,1,.65f)));
   }yield return null;
  }
  if(hero){hero.transform.position=shot.end;Destroy(hero);}
  Destroy(trail.gameObject);Destroy(material);if(outer){Destroy(outer.gameObject);Destroy(outerMat);}
  game.Event("outfield_arrival",$"kind={kind};x={shot.end.x:F2};y={shot.end.y:F2};z={shot.end.z:F2}");
  StartCoroutine(OutfieldReaction(shot,strong,variant,item));
 }
 static float Hash(int i)=>Mathf.Repeat(Mathf.Sin(i*127.1f+41.7f)*43758.5453f,1);
 Material cakeSponge,cakeCream,glassMaterial;
 Material CakeMaterial(){if(!cakeSponge)cakeSponge=BatGeometry.Material("#895541");return cakeSponge;}
 Material CakeCream(){if(!cakeCream)cakeCream=BatGeometry.Material("#D9B0D9");return cakeCream;}
 GameObject GlassShard(string name,float size){
  if(!glassMaterial)glassMaterial=BatGeometry.Material("#6EC5AD",true);
  var points=new List<Vector3>{new Vector3(-.36f,-.5f,0),new Vector3(.42f,-.28f,.08f),new Vector3(.10f,.60f,0),new Vector3(-.05f,.18f,.13f)};for(int i=0;i<points.Count;i++)points[i]*=size;
  return BatGeometry.MeshObject(name,points,new List<int>{0,2,1,0,1,3,1,2,3,2,0,3},glassMaterial,game.targets);
 }
 GameObject CakePiece(string name,float size){
  var o=new GameObject(name);o.transform.SetParent(game.targets,false);
  BatGeometry.Box("Sponge",Vector3.zero,new Vector3(size,size*.48f,size*.75f),CakeMaterial(),o.transform);
  BatGeometry.Box("Cream filling",new Vector3(0,size*.04f,0),new Vector3(size*1.01f,size*.12f,size*.77f),CakeCream(),o.transform);
  BatGeometry.Box("Icing",new Vector3(0,size*.28f,0),new Vector3(size*1.03f,size*.10f,size*.8f),CakeCream(),o.transform);
  var berry=V.Model(V.strawberry,"Strawberry",new Vector3(size*.15f,size*.40f,0),Vector3.one*size*.36f,false,o.transform);
  foreach(var c in o.GetComponentsInChildren<Collider>())Destroy(c);return o;
 }
 IEnumerator Debris(GameObject o,OutboundShot shot,int i,float life,bool treasure){
  foreach(var r in o.GetComponentsInChildren<Renderer>())r.shadowCastingMode=ShadowCastingMode.Off;
  double start=game.effectTime;float side=(Hash(i+4)-.5f)*2,up=Hash(i+18);Vector3 v=shot.direction*(treasure?8:6)+shot.side*side*(treasure?1.6f:1.3f)+Vector3.up*(.4f+up*2.2f);Vector3 scale=o.transform.localScale;
  while(o&&game.effectTime-start<life){float t=(float)(game.effectTime-start);o.transform.position=shot.start+Vector3.forward*.09f+v*t+Vector3.down*(.65f*t*t);o.transform.Rotate(new Vector3(110+Hash(i)*130,180,85)*game.effectDelta);o.transform.localScale=scale*Mathf.Clamp01((life-t)/.24f);yield return null;}if(o)Destroy(o);
 }
 IEnumerator TreasureHero(GameObject gem,OutboundShot shot){
  foreach(var r in gem.GetComponentsInChildren<Renderer>())r.shadowCastingMode=ShadowCastingMode.Off;
  double start=game.effectTime;var m=FadeMaterial("#71E9B8",1.25f);var trail=BatGeometry.Line("Hero emerald tail",new[]{shot.start,shot.start},.025f,m,game.targets);
  while(gem&&game.effectTime-start<shot.seconds+.30f){float t=(float)(game.effectTime-start),u=Mathf.Clamp01(t/(shot.seconds+.30f));Vector3 at=shot.At(u*shot.seconds)+shot.side*(Mathf.Sin(u*Mathf.PI)*1.3f)+Vector3.up*Mathf.Sin(u*Mathf.PI)*.8f;gem.transform.position=at;gem.transform.Rotate(80*game.effectDelta,130*game.effectDelta,0);trail.SetPosition(0,shot.At(Mathf.Max(0,u*shot.seconds-.025f))+shot.side*(Mathf.Sin(u*Mathf.PI)*1.3f)+Vector3.up*Mathf.Sin(u*Mathf.PI)*.8f);trail.SetPosition(1,at);yield return null;}
  if(gem)Destroy(gem);Destroy(trail.gameObject);Destroy(m);
 }
 IEnumerator ForwardConfetti(Vector3 p,string kind,int count,float life){
  var m=FadeMaterial(PitchVisuals.Tint(kind),1);var list=new List<GameObject>();
  for(int i=0;i<count;i++){var o=BatGeometry.Box("Forward confetti",p,new Vector3(.012f,.026f,.003f),m,game.targets);Destroy(o.GetComponent<Collider>());list.Add(o);}
  double start=game.effectTime;while(game.effectTime-start<life){float t=(float)(game.effectTime-start);for(int i=0;i<count;i++){list[i].transform.position=p+new Vector3((Hash(i)-.5f)*.8f,.1f+Hash(i+9)*.4f,1.6f+Hash(i+4))*t+Vector3.down*t*t*.3f;list[i].transform.Rotate(160*game.effectDelta,260*game.effectDelta,90*game.effectDelta);}var color=m.GetColor("_BaseColor");color.a=Mathf.Clamp01((life-t)/.15f);m.SetColor("_BaseColor",color);yield return null;}foreach(var o in list)Destroy(o);Destroy(m);
 }
 IEnumerator OutfieldReaction(OutboundShot shot,bool strong,int variant,int item){
  Vector3 p=shot.end;string kind=shot.kind;
  if(kind=="bomb"){
   game.Event("bomb_far_explosion",$"z={p.z:F2}");Sound("impactPunch_heavy_000",-5,.52f,p);Sound("impactMetal_heavy_000",-15,.65f,p);
   StartCoroutine(Blast(p,strong));
  }else if(kind=="magic"){
   Sound("magic_dissolve",-12,.70f,p);StartCoroutine(MagicBloom(p,strong));
  }else if(kind=="cake"){
   Sound("impactSoft_heavy_000",-13,.7f,p);StartCoroutine(CreamSplash(p));
  }else if(kind=="poison"){
   Sound("poison_jet",-15,.72f,p);for(int i=0;i<6;i++)StartCoroutine(Particle(p+Vector3.up*.2f,new Vector3((Hash(i)-.5f)*2,2.4f+Hash(i+4),(Hash(i+9)-.5f)*1.3f),"smoke_01",12.0f,1.3f,new Color(.37f,1.0f,.54f,.68f)));
  }else if(kind=="chest"){
   Sound("confirmation_001",-12,.83f,p);StartCoroutine(GoldenFountain(p,strong));
  }else if(kind=="bullets"){
   Sound("impactMetal_light_000",-15,1.1f+item*.15f,p);StartCoroutine(MetalLandingFan(p,item));
   for(int i=0;i<5;i++)StartCoroutine(Particle(p,new Vector3((Hash(i+item)-.5f)*6,3.5f+Hash(i)*3,1),"spark_01",.8f,.60f,new Color(1,.78f,.36f,1)));
  }else{
   Sound("impactSoft_heavy_000",-14,.65f,p);
   if(kind=="boomerang")StartCoroutine(SpinLanding(p,shot.side));
   else for(int i=0;i<5;i++)StartCoroutine(Particle(p,new Vector3((Hash(i)-.5f)*2.5f,4.5f+Hash(i+5)*2.0f,.3f),"smoke_01",12.0f,1.3f,new Color(1.04f,.79f,.48f,.72f)));
  }
  if(kind=="ball"||kind=="cake"||kind=="poison")StartCoroutine(FieldRipple(p,kind=="poison"?"#78C98F":"#B2BC8B",strong?4.2f:3.2f,.65f));
  yield return EffectWait(.20f);
  if(game.effectTime-lastApplause>.55){lastApplause=game.effectTime;Sound("crowd_claps",strong?-15:-20,1,new Vector3(Mathf.Sign(p.x)*32,3,70));game.Event("crowd_response",$"kind={kind};strong={strong}");}
  StartCoroutine(SkyCelebration(shot,strong,item));
 }
 IEnumerator FieldRipple(Vector3 p,string tint,float radius,float duration){
  p.y=.035f;var m=FadeMaterial(tint,.85f);var ring=BatGeometry.Ring("Turf impact ripple",p,1,.10f,m,game.targets,true);double start=game.effectTime;
  while(game.effectTime-start<duration){float t=(float)(game.effectTime-start)/duration;ring.transform.localScale=Vector3.one*Mathf.Lerp(.2f,radius,t);ring.GetComponent<LineRenderer>().widthMultiplier=.10f*(1-t);yield return null;}Destroy(ring);Destroy(m);
 }
 IEnumerator MagicBloom(Vector3 p,bool strong){
  var m=FadeMaterial("#57CAE7",1.1f);var lines=new List<LineRenderer>();for(int i=0;i<6;i++)lines.Add(BatGeometry.Line("Magic constellation petal",new Vector3[24],.16f,m,game.targets));double start=game.effectTime;
  while(game.effectTime-start<1.1){float t=(float)(game.effectTime-start)/1.1f;Vector3 center=p+Vector3.up*(.5f+6*Mathf.SmoothStep(0,1,Mathf.Clamp01(t/.35f)));
   for(int j=0;j<6;j++){float a=j*Mathf.PI/3+t*.25f;Vector3 radial=new Vector3(Mathf.Cos(a),Mathf.Sin(a),0),tangent=new Vector3(-Mathf.Sin(a),Mathf.Cos(a),0);
    for(int k=0;k<24;k++){float u=k/23f;float r=Mathf.Sin(u*Mathf.PI)*(strong?8.5f:7.0f)*Mathf.Sqrt(Mathf.Clamp01(t*2));lines[j].SetPosition(k,center+radial*r+tangent*(Mathf.Sin(u*2*Mathf.PI)*.70f*Mathf.Sqrt(t))+Vector3.forward*(u*.8f));}
    lines[j].widthMultiplier=.18f*Mathf.Clamp01((1-t)/.35f);
   }yield return null;
  }foreach(var l in lines)Destroy(l.gameObject);Destroy(m);
 }
 IEnumerator Blast(Vector3 p,bool strong){
  // Irregular hot fragments expand, then smoke rises; no opaque white disk.
  Sound("fuse_crackle",-13,.55f,p);StartCoroutine(FieldRipple(new Vector3(p.x,0,p.z),"#CF9C68",strong?7:5,1));
  for(int j=0;j<3;j++){
   for(int i=0;i<9;i++){float a=Hash(i+j*9)*Mathf.PI*2;Vector3 v=new Vector3(Mathf.Cos(a)*(8+Hash(i)*3),Mathf.Sin(a)*7+2.5f,Hash(i+8)*2);StartCoroutine(Particle(p,v,"smoke_01",2.6f+j*.6f,.50f,new Color(1.6f,.38f+j*.13f,.065f,.78f)));StartCoroutine(Particle(p,v*1.5f,"spark_01",.75f,.90f,new Color(1.5f,.78f,.16f,1)));}
   yield return EffectWait(.075f);
  }
  for(int i=0;i<7;i++)StartCoroutine(Particle(p,new Vector3((Hash(i)-.5f)*2,1.3f+Hash(i+4),.4f),"smoke_01",3.0f,1.0f,new Color(.43f,.36f,.30f,.40f)));
 }
 IEnumerator SpinLanding(Vector3 p,Vector3 side){
  // Rotational impact sweeps dust along the turf, in the spin direction.
  for(int j=0;j<4;j++){Vector3 at=p+side*(j*2.0f);for(int i=0;i<2;i++)StartCoroutine(Particle(at,side*(2.8f+i)+Vector3.up*(1.7f+i*.5f),"smoke_01",6.5f,1.0f,new Color(.93f,.70f,.40f,.62f)));yield return EffectWait(.10f);}
 }
 IEnumerator CreamSplash(Vector3 p){
  var m=FadeMaterial("#F3CDE4",1.1f);var jets=new LineRenderer[11];for(int i=0;i<11;i++){jets[i]=BatGeometry.Line("Cream splash ribbon",new Vector3[12],.30f,m,game.targets);jets[i].numCapVertices=4;jets[i].widthCurve=new AnimationCurve(new Keyframe(0,.2f),new Keyframe(.7f,1),new Keyframe(1,.15f));}
  double start=game.effectTime;while(game.effectTime-start<1.05){float t=(float)(game.effectTime-start);for(int i=0;i<11;i++){Vector3 v=new Vector3((i-5)*3.0f,8.5f+Hash(i)*6,(Hash(i+8)-.5f)*2);for(int k=0;k<12;k++){float u=k/11f;float time=Mathf.Max(0,t-.15f+u*.15f);jets[i].SetPosition(k,p+Vector3.up*.12f+v*time+Vector3.down*(3*time*time));}jets[i].widthMultiplier=.30f*Mathf.Clamp01((1.05f-t)/.3f);}yield return null;}foreach(var jet in jets)Destroy(jet.gameObject);Destroy(m);
 }
 IEnumerator MetalLandingFan(Vector3 p,int item){
  var m=FadeMaterial("#FF641E",1.0f);var rays=new LineRenderer[4];for(int i=0;i<4;i++)rays[i]=BatGeometry.Line("Outfield metal ricochet",new[]{p,p},.18f,m,game.targets);double start=game.effectTime;
  while(game.effectTime-start<.8f){float t=(float)(game.effectTime-start);for(int i=0;i<4;i++){Vector3 v=new Vector3((i-1.5f)*6+item*.5f,6+Hash(i+item)*6,1.4f);float begin=Mathf.Max(0,t-.13f);rays[i].SetPosition(0,p+v*begin+Vector3.down*begin*begin);rays[i].SetPosition(1,p+v*t+Vector3.down*t*t);rays[i].widthMultiplier=.25f*Mathf.Clamp01((.8f-t)/.30f);}yield return null;}foreach(var ray in rays)Destroy(ray.gameObject);Destroy(m);
 }
 IEnumerator GoldenFountain(Vector3 p,bool strong){for(int j=0;j<3;j++){for(int i=0;i<12;i++)StartCoroutine(Particle(p,new Vector3((Hash(i+j*12)-.5f)*20,13+Hash(i+2)*6,(Hash(i+8)-.5f)*3),"star_01",1.8f,strong?1.5f:1.3f,new Color(1.4f,1,.28f,1)));yield return EffectWait(.10f);}}
 IEnumerator FarCelebration(Vector3 p,int variant){
  // One secondary reaction, offset beyond the landing. No common full-screen celebration.
  Vector3 at=p+new Vector3(Mathf.Sign(p.x+.01f)*4,3,5);game.Event("far_secondary",$"x={at.x:F2};z={at.z:F2}");
  for(int i=0;i<9;i++){float a=Hash(i+variant)*Mathf.PI*2;StartCoroutine(Particle(at,new Vector3(Mathf.Cos(a)*(2+Hash(i)),Mathf.Sin(a)*2.5f,Hash(i+4)),variant%2==0?"star_01":"spark_01",.75f,.75f,new Color(.8f,1.2f,1.3f,.8f)));}yield return null;
 }
}
}
