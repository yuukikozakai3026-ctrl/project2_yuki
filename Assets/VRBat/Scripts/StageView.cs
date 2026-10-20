using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.XR.Interaction.Toolkit.UI;
namespace VRBat {
public class StageView:MonoBehaviour {
 public bool importedField;
 public Material litTemplate,particleTemplate,uiMaterial;
 public GameObject floor,machine,supply,robot,pipe,ball,cake,plate,chest,strawberry;
 public Font font,boldFont; public Texture2D uiFrame,checkmark; public Texture2D[] particles; public AudioClip[] sounds;
 public StadiumSpectacle spectacle; public Transform near; public Transform bat; public Camera eye; public AudioSource music; public Transform dockHinge; public GameObject bStage,menu,results,hud; public Text score,time; public Material band;
 public Material white,navy,gold,teal,metal,table,wall,fieldFence,farNetMaterial;
 readonly Dictionary<string,GameObject> objects=new Dictionary<string,GameObject>();
 readonly Dictionary<int,Transform[]> shutters=new Dictionary<int,Transform[]>(); readonly Dictionary<int,Transform> muzzleRings=new Dictionary<int,Transform>();
 readonly List<Transform> launcherLegs=new List<Transform>();
 public void Build(){BatGeometry.LitShader=litTemplate.shader;white=BatGeometry.Material("#E7EEF0",true);navy=BatGeometry.Material("#2D4C63");navy.SetFloat("_Cull",0);gold=BatGeometry.Material("#FFC45B",true);teal=Glow("#45E6E3",2.4f);metal=BatGeometry.Material("#36566C");table=BatGeometry.Material("#CAD6D2");wall=BatGeometry.Material("#D7E0DE");band=new Material(BatGeometry.Material("#F6C850",true));
 near=new GameObject("Body height stage").transform;near.SetParent(transform,false);
 if(importedField){fieldFence=BatGeometry.Material("#426960");BuildFloodlights();BuildSkyStars();}
 else BuildBallpark();
 // The brief prioritises a clear field. LaunchCue marks the unchanged launch point
 // briefly at release; no permanent guide or support can overlap an outfield reaction.
 // The robot appears only during a background demonstration, keeping the normal field clear.

 var board=Panel("Score board",new Vector3(-2.85f,2.70f,4.5f),new Vector2(1.60f,.48f),near);hud=board.gameObject;Label(board,"得点",new Vector2(-.48f,.11f),new Vector2(.70f,.08f),.05f);Label(board,"残り",new Vector2(.48f,.11f),new Vector2(.70f,.08f),.05f);score=Label(board,"0",new Vector2(-.48f,-.07f),new Vector2(.70f,.16f),.12f);time=Label(board,"90",new Vector2(.48f,-.07f),new Vector2(.70f,.16f),.12f);score.font=boldFont?boldFont:font;time.font=boldFont?boldFont:font;score.color=BatGeometry.ColorOf("#FFD27D");
 for(int i=0;i<3;i++)objects["lamp"+i]=BatGeometry.Sphere("Start lamp",new Vector3(-2.85f+(i-1)*.16f,3.03f,4.5f),Vector3.one*.08f,gold,near);

 bat=BatGeometry.Bat(white,metal,band).transform;bat.SetParent(transform,false);foreach(var renderer in bat.GetComponentsInChildren<Renderer>())renderer.shadowCastingMode=ShadowCastingMode.Off;
 var cam=new GameObject("Player eye");cam.tag="MainCamera";eye=cam.AddComponent<Camera>();eye.allowHDR=true;eye.fieldOfView=70;eye.nearClipPlane=.03f;eye.farClipPlane=180;eye.clearFlags=CameraClearFlags.SolidColor;eye.backgroundColor=BatGeometry.ColorOf("#07172E");eye.transform.position=new Vector3(0,1.65f,0);eye.transform.rotation=Quaternion.Euler(6,0,0);cam.AddComponent<AudioListener>();cam.AddComponent<AudioLimiter>();var musicObject=new GameObject("Stadium music");musicObject.transform.SetParent(cam.transform,false);music=musicObject.AddComponent<AudioSource>();foreach(var clip in sounds)if(clip&&clip.name=="stadium_pulse")music.clip=clip;music.loop=true;music.spatialBlend=0;music.volume=.18f;if(music.clip)music.Play();SetupPostProcessing(cam);AudioListener.volume=Mathf.Pow(10,-6f/20);
 foreach(var renderer in near.GetComponentsInChildren<Renderer>())renderer.shadowCastingMode=ShadowCastingMode.Off;
 var light=new GameObject("Key light").AddComponent<Light>();light.type=LightType.Directional;light.color=BatGeometry.ColorOf(importedField?"#F2E4C9":"#AFCBFF");light.intensity=importedField?1.15f:.95f;light.shadows=LightShadows.Soft;light.transform.rotation=Quaternion.LookRotation(new Vector3(.4f,-.85f,.35f));RenderSettings.ambientMode=UnityEngine.Rendering.AmbientMode.Flat;RenderSettings.ambientLight=BatGeometry.ColorOf(importedField?"#879A91":"#7896AE")*.85f;
 spectacle=gameObject.AddComponent<StadiumSpectacle>();spectacle.Build(this);
 }
 void BuildBallpark(){
  var turf=BatGeometry.Material("#254B3D");var stripe=BatGeometry.Material("#315945");var dirt=BatGeometry.Material("#675442");var chalk=Glow("#BDD1C8",.95f);var fence=BatGeometry.Material("#426960");fieldFence=fence;
  BatGeometry.Box("Baseball field grass",new Vector3(0,-.10f,40),new Vector3(210,.18f,190),turf,transform);
  for(int z=18;z<135;z+=14)BatGeometry.Box("Mowed grass stripe",new Vector3(0,-.008f,z+3.5f),new Vector3(210,.003f,7),stripe,transform).GetComponent<Collider>().enabled=false;
  BatGeometry.MeshObject("Baseball infield",new List<Vector3>{new Vector3(0,.001f,12),new Vector3(13,.001f,20),new Vector3(0,.001f,40),new Vector3(-13,.001f,20)},new List<int>{0,2,1,0,3,2},dirt,transform);
  BatGeometry.Box("Pitcher mound",new Vector3(0,-.001f,19),new Vector3(3,.008f,3),dirt,transform).GetComponent<Collider>().enabled=false;
  foreach(var p in new[]{new Vector3(-13,0,20),new Vector3(13,0,20),new Vector3(0,0,40)}){
   BatGeometry.Box("Base path earth",p+new Vector3(0,-.001f,0),new Vector3(2,.008f,2),dirt,transform).GetComponent<Collider>().enabled=false;
   BatGeometry.Box("Base",p+new Vector3(0,.006f,0),new Vector3(.38f,.025f,.38f),chalk,transform);
  }
  BatGeometry.Box("Home plate",new Vector3(0,.006f,.42f),new Vector3(.45f,.012f,.45f),chalk,transform);BatGeometry.Line("Home batter box",new[]{new Vector3(-.80f,.012f,-.45f),new Vector3(-.80f,.012f,.95f),new Vector3(.80f,.012f,.95f),new Vector3(.80f,.012f,-.45f)},.016f,chalk,transform);
  foreach(int side in new[]{-1,1}){
   BatGeometry.Line("Foul line",new[]{new Vector3(side*3.4f,.015f,5.5f),new Vector3(side*75,.015f,75)},.04f,chalk,transform);
  }
  for(int i=0;i<24;i++){
   float a0=Mathf.PI*i/24,a1=Mathf.PI*(i+1)/24;
   var p0=new Vector3(Mathf.Cos(a0)*102,0,Mathf.Sin(a0)*102+1.2f);
   var p1=new Vector3(Mathf.Cos(a1)*102,0,Mathf.Sin(a1)*102+1.2f);
   BatGeometry.Line("Outfield fence rail",new[]{p0+Vector3.up*1.5f,p1+Vector3.up*1.5f},.06f,fence,transform);
   BatGeometry.Line("Outfield fence base",new[]{p0+Vector3.up*.18f,p1+Vector3.up*.18f},.04f,fence,transform);
   BatGeometry.Box("Outfield fence post",p0+Vector3.up*.9f,new Vector3(.06f,1.8f,.06f),fence,transform);
  }
  BuildFloodlights();
  BuildSkyStars();
 }
 public Material Glow(string hex,float intensity){var shader=Shader.Find("Universal Render Pipeline/Unlit");var m=new Material(shader);var c=BatGeometry.ColorOf(hex)*intensity;m.SetColor("_BaseColor",c);m.color=c;return m;}
 void SetupPostProcessing(GameObject cam){
  var data=cam.GetComponent<UniversalAdditionalCameraData>();if(!data)data=cam.AddComponent<UniversalAdditionalCameraData>();data.renderPostProcessing=true;data.volumeLayerMask=1;
  var go=new GameObject("Stadium color and bloom");go.transform.SetParent(transform,false);var volume=go.AddComponent<Volume>();volume.isGlobal=true;volume.priority=1;var profile=ScriptableObject.CreateInstance<VolumeProfile>();volume.sharedProfile=profile;
  var bloom=profile.Add<Bloom>(true);bloom.threshold.value=1.1f;bloom.intensity.value=.70f;bloom.scatter.value=.50f;bloom.highQualityFiltering.value=true;
  var tone=profile.Add<Tonemapping>(true);tone.mode.value=TonemappingMode.Neutral;
  var color=profile.Add<ColorAdjustments>(true);color.contrast.value=13;color.saturation.value=19;
  var vignette=profile.Add<Vignette>(true);vignette.intensity.value=.16f;vignette.smoothness.value=.45f;
 }
 void BuildFloodlights(){
  var steel=BatGeometry.Material("#20384C");var lamp=Glow("#B6D5E3",1.35f);
  foreach(int side in new[]{-1,1})foreach(int distance in new[]{0,1}){
   // Keep towers outside the entire outgoing fan, including the widest landing effects.
   float x=side*(distance==0?48:90),z=distance==0?42:83,h=distance==0?15:22;
   BatGeometry.Box("Stadium light tower",new Vector3(x,h/2,z),new Vector3(.23f,h,.23f),steel,transform);
   BatGeometry.Box("Light gantry",new Vector3(x,h,z),new Vector3(3.6f,.18f,.2f),steel,transform);
   for(int i=0;i<5;i++)BatGeometry.Box("Floodlight emitter",new Vector3(x+(i-2)*.65f,h-.15f,z-.15f),new Vector3(.42f,.38f,.12f),lamp,transform);
   if(distance==0){var light=new GameObject("Field floodlight").AddComponent<Light>();light.type=LightType.Spot;light.color=BatGeometry.ColorOf(importedField?"#FFE7C2":"#BEDBFF");light.intensity=importedField?3f:7f;light.range=80;light.spotAngle=90;light.shadows=LightShadows.None;light.transform.SetParent(transform,false);light.transform.position=new Vector3(x,h-.3f,z);light.transform.rotation=Quaternion.LookRotation(new Vector3(-x*.6f,-h,-z*.7f));}
  }
 }
 void BuildSkyStars(){var stars=Glow("#B8DDFF",1.8f);var rng=new System.Random(2867);for(int i=0;i<75;i++){float x=(float)(rng.NextDouble()*220-110),y=(float)(rng.NextDouble()*62+18),z=(float)(rng.NextDouble()*70+100);float d=(float)(rng.NextDouble()*.26+.12);BatGeometry.Sphere("Night sky star",new Vector3(x,y,z),Vector3.one*d,stars,transform);}}
 void Net(Vector3 p,float w,float h,Material netMaterial=null){if(!netMaterial)netMaterial=metal;for(float x=-w/2;x<=w/2+.001f;x+=.1f)BatGeometry.Box("Net strand",p+new Vector3(x,0,0),new Vector3(.006f,h,.006f),netMaterial,near);for(float y=-h/2;y<=h/2+.001f;y+=.1f)BatGeometry.Box("Net strand",p+new Vector3(0,y,0),new Vector3(w,.006f,.006f),netMaterial,near);foreach(float x in new[]{-w/2,w/2})BatGeometry.Box("Net frame",p+new Vector3(x,0,0),new Vector3(.025f,h+.025f,.03f),netMaterial,near);foreach(float y in new[]{-h/2,h/2})BatGeometry.Box("Net frame",p+new Vector3(0,y,0),new Vector3(w,.025f,.03f),netMaterial,near);}
 public GameObject Model(GameObject prefab,string name,Vector3 pos,Vector3 size,bool bottom,Transform parent,int child=-1){var o=BatGeometry.Normalize(prefab,name,size,bottom,parent,false,child);o.transform.localPosition=pos;return o;}
 void Tint(GameObject o,string color){var tint=BatGeometry.ColorOf(color).linear;foreach(var r in o.GetComponentsInChildren<Renderer>()){var mats=r.materials;foreach(var m in mats)m.color=(m.color.linear*tint).gamma;r.materials=mats;}}
 public RectTransform Panel(string name,Vector3 p,Vector2 size,Transform parent){var o=new GameObject(name,typeof(RectTransform),typeof(Canvas));o.transform.SetParent(parent,false);o.transform.localPosition=p;o.transform.localScale=Vector3.one*.001f;var rt=o.GetComponent<RectTransform>();rt.sizeDelta=size*1000;var canvas=o.GetComponent<Canvas>();canvas.renderMode=RenderMode.WorldSpace;canvas.worldCamera=eye?eye:Camera.main;o.AddComponent<TrackedDeviceGraphicRaycaster>();var im=o.AddComponent<Image>();im.raycastTarget=false;im.material=uiMaterial;im.color=BatGeometry.ColorOf("#152A3D");if(uiFrame){im.sprite=Sprite.Create(uiFrame,new Rect(0,0,uiFrame.width,uiFrame.height),new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect,new Vector4(8,8,8,8));im.type=Image.Type.Sliced;}PanelLine(rt,"Gold top line",new Vector2(0,size.y*500-6),new Vector2(size.x*1000-16,7),"#EFC06C");PanelLine(rt,"Cyan bottom line",new Vector2(0,-size.y*500+6),new Vector2(size.x*1000-16,3),"#3AB4C2");return rt;}
 Image PanelLine(RectTransform parent,string name,Vector2 at,Vector2 size,string color){var o=new GameObject(name,typeof(RectTransform),typeof(Image));o.transform.SetParent(parent,false);var rt=o.GetComponent<RectTransform>();rt.anchoredPosition=at;rt.sizeDelta=size;var image=o.GetComponent<Image>();image.material=uiMaterial;image.color=BatGeometry.ColorOf(color);image.raycastTarget=false;return image;}
 public Text Label(RectTransform parent,string text,Vector2 p,Vector2 size,float height){var o=new GameObject(text,typeof(RectTransform),typeof(Text));o.transform.SetParent(parent,false);var rt=o.GetComponent<RectTransform>();rt.anchoredPosition=p*1000;rt.sizeDelta=size*1000;var t=o.GetComponent<Text>();t.font=font;t.material=uiMaterial;t.text=text;t.fontSize=Mathf.RoundToInt(height*1200);t.alignment=TextAnchor.MiddleCenter;t.color=BatGeometry.ColorOf("#F7F4E9");t.horizontalOverflow=HorizontalWrapMode.Overflow;t.verticalOverflow=VerticalWrapMode.Overflow;t.raycastTarget=false;return t;}
 public void Muzzle(int lane,float opening,float pulse=0){}
 public void Lamps(int n){for(int i=0;i<3;i++)if(objects.TryGetValue("lamp"+i,out var lamp)&&lamp)lamp.SetActive(i<n);}
 public void Receiver(float openness){objects["receiverDoor"].transform.localPosition=new Vector3(mirrored?-.58f:.58f,1.24f+openness*.45f,1.272f);}
 public void Check(RectTransform parent,Vector2 position){var o=new GameObject("Selected hand",typeof(RectTransform),typeof(Image));o.transform.SetParent(parent,false);var rt=o.GetComponent<RectTransform>();rt.anchoredPosition=position*1000;rt.sizeDelta=new Vector2(40,40);var image=o.GetComponent<Image>();image.material=uiMaterial;image.sprite=Sprite.Create(checkmark,new Rect(0,0,checkmark.width,checkmark.height),new Vector2(.5f,.5f));image.color=BatGeometry.ColorOf("#F7F4E9");}
 public WorldButton Button(RectTransform parent,string text,Vector2 pos,Vector2 size,System.Action action){var image=PanelLine(parent,text+" button",pos*1000,size*1000,text=="開始"||text=="もう一度"?"#246F80":"#33465A");image.raycastTarget=true;var uiButton=image.gameObject.AddComponent<Button>();uiButton.targetGraphic=image;var colors=uiButton.colors;colors.highlightedColor=new Color(1.25f,1.4f,1.4f,1);uiButton.colors=colors;var t=Label(parent,text,pos,size,.055f);var box=t.gameObject.AddComponent<BoxCollider>();box.size=new Vector3(size.x*1000,size.y*1000,10);var b=t.gameObject.AddComponent<WorldButton>();b.clicked=action;b.BindUI(uiButton);return b;}
 public void AuditBounds(){foreach(var r in GetComponentsInChildren<Renderer>())if(!float.IsFinite(r.bounds.size.x))Debug.LogError("Invalid renderer bounds: "+r.name);}
 public bool mirrored;
 public void Calibrate(float dy,bool left){near.localPosition=Vector3.up*dy;
 foreach(var leg in launcherLegs){if(!leg)continue;var p=leg.localPosition;p.y=.61f-dy*.5f;leg.localPosition=p;var s=leg.localScale;s.y=1.20f+dy;leg.localScale=s;}
 if(left!=mirrored){foreach(Transform t in near){if(t.name=="metal_target"||t.name.StartsWith("Net ")){var p=t.localPosition;p.x=-p.x;t.localPosition=p;}}mirrored=left;}
 }
 public GameObject Target(string kind,bool b,Vector3 p,Transform parent){GameObject o;
 if(kind=="poison"||kind=="boomerang"||kind=="magic"||kind=="bullets")o=PitchVisuals.Create(this,kind,parent);
 else if(kind=="cake"){if(b){o=new GameObject("Cake plate");o.transform.SetParent(parent,false);Model(plate,"Plate",Vector3.zero,new Vector3(.44f,.044407f,.44f),true,o.transform);Model(cake,"Whole cake",new Vector3(0,.03f,0),new Vector3(.34f,.14525f,.34f),true,o.transform);BatGeometry.Box("Support bracket",new Vector3(0,-.0125f,0),new Vector3(.30f,.025f,.26f),metal,o.transform);BatGeometry.Ring("Gimbal",new Vector3(0,-.03f,0),.06f,.008f,white,o.transform,true);BatGeometry.Sphere("Balloon",new Vector3(0,.33f,.08f),Vector3.one*.12f,BatGeometry.Material("#F28B9B"),o.transform);BatGeometry.Line("Balloon string",new[]{new Vector3(0,.03f,.17f),new Vector3(0,.27f,.08f)},.002f,white,o.transform);}else o=Model(cake,"Cake slice",Vector3.zero,new Vector3(.30f,.25633f,.30f),false,parent,0);}
 else if(kind=="chest"){o=Model(chest,"Chest",Vector3.zero,b?new Vector3(.45f,.405f,.45f):new Vector3(.36f,.32f,.34f),false,parent);if(b)foreach(float y in new[]{-.045f,.045f})BatGeometry.Box("Breakable strap",new Vector3(0,y,-.232f),new Vector3(.39f,.045f,.018f),metal,o.transform);}
 else {float d=kind=="bomb"||kind=="firework"?.22f:kind=="bullets"?.13f:.14f;o=Model(ball,kind,Vector3.zero,Vector3.one*d,false,parent);Material mat=kind=="ball"?BatGeometry.Material("#FFF9EE"):kind=="bomb"?BatGeometry.Material("#47546A",true):kind=="firework"?teal:white;foreach(var r in o.GetComponentsInChildren<Renderer>())r.sharedMaterial=mat;
 if(kind=="ball")for(int seam=0;seam<2;seam++){var pts=new Vector3[65];for(int i=0;i<65;i++){float a=i*2*Mathf.PI/64;pts[i]=Quaternion.Euler(0,seam*90,0)*(new Vector3(Mathf.Cos(a),.45f*Mathf.Sin(2*a),Mathf.Sin(a)).normalized*(d/2+.0005f));}BatGeometry.Line("Red stitches",pts,.003f,BatGeometry.Material("#D65357"),o.transform);}
 if(kind=="bomb"){BatGeometry.Line("Fuse",new[]{new Vector3(0,.11f,0),new Vector3(.02f,.19f,.03f),new Vector3(.07f,.23f,.07f)},.012f,gold,o.transform);BatGeometry.Ring("Bomb brass collar",new Vector3(0,.085f,0),.063f,.012f,gold,o.transform,true);BatGeometry.Sphere("Burning fuse tip",new Vector3(.07f,.23f,.07f),Vector3.one*.024f,Glow("#FFD38A",2f),o.transform);}
 if(kind=="firework"){BatGeometry.Lathe("Gold belt",new[]{-.02f,.02f},new[]{.111f,.111f},gold,o.transform);var v=new List<Vector3>{new Vector3(0,0,-.111f)};var tr=new List<int>();for(int i=0;i<10;i++){float a=Mathf.PI/2+i*Mathf.PI/5;v.Add(new Vector3(Mathf.Cos(a)*(i%2==0?.0475f:.0225f),Mathf.Sin(a)*(i%2==0?.0475f:.0225f),-.112f));}for(int i=0;i<10;i++)tr.AddRange(new[]{0,1+(i+1)%10,i+1});BatGeometry.MeshObject("Star",v,tr,gold,o.transform);}}
 o.transform.localPosition=p;foreach(var renderer in o.GetComponentsInChildren<Renderer>())renderer.shadowCastingMode=ShadowCastingMode.Off;return o;}
}
}
