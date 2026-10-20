using System.Collections.Generic;
using UnityEngine;
namespace VRBat {
// The stadium is scenery: it never changes a pitch, a hit, or the camera pose.
public sealed class StadiumSpectacle:MonoBehaviour {
 StageView view;BatGame game;readonly List<Material> leds=new List<Material>();readonly List<LineRenderer> beams=new List<LineRenderer>();
 readonly Color[] palette={BatGeometry.ColorOf("#34DCF1"),BatGeometry.ColorOf("#FD548F"),BatGeometry.ColorOf("#FFC456"),BatGeometry.ColorOf("#9274FF")};
 double pulseAt=-20;Color pulseColor=Color.white;float pulsePower;
 public void Pulse(Color color,bool strong){pulseAt=game.effectTime;pulseColor=color;pulsePower=strong?1.0f:.7f;}
 public void ResetPulse(){pulseAt=-20;}
 public void Build(StageView v){
  view=v;game=GetComponent<BatGame>();var root=new GameObject("Festival stadium terraces").transform;root.SetParent(transform,false);
  Material structure=BatGeometry.Material("#122C49"),seat=BatGeometry.Material("#436879");
  for(int i=0;i<8;i++)leds.Add(v.Glow(i%4==0?"#34DCF1":i%4==1?"#FD548F":i%4==2?"#FFC456":"#9274FF",1.4f));
  for(int i=0;i<56;i++){
   float a=(-78+i*156f/55)*Mathf.Deg2Rad;Vector3 radial=new Vector3(Mathf.Sin(a),0,Mathf.Cos(a));
   Quaternion facing=Quaternion.Euler(0,a*Mathf.Rad2Deg,0);
   for(int row=0;row<3;row++){
    float tierHeight=3+row*2;var tier=BatGeometry.Box("Stepped grandstand",radial*(111+row*2)+Vector3.up*tierHeight*.5f,new Vector3(5.2f,tierHeight,3.3f),structure,root);tier.transform.rotation=facing;
    Vector3 p=radial*(108.9f+row*2)+Vector3.up*(2.4f+row*2);
    var strip=BatGeometry.Box("Chasing festival ribbon",p,new Vector3(4.5f,.36f,.3f),leds[(i+row*2)%8],root);strip.transform.rotation=facing;
    for(int j=0;j<3;j++){var crowd=BatGeometry.Box("Audience color mosaic",p+new Vector3(Mathf.Cos(a)*(j-1)*1.2f,.8f,-Mathf.Sin(a)*(j-1)*1.2f),new Vector3(.7f,.72f,.3f),j==1?seat:leds[(i+row+j)%8],root);crowd.transform.rotation=facing;}
   }
   if(i%4==0){BatGeometry.Box("Pennant support",radial*116+Vector3.up*4.9f,new Vector3(.16f,9.8f,.16f),structure,root);var cap=BatGeometry.Box("Upper celebration pennant",radial*116+Vector3.up*11,new Vector3(1.3f,2.4f,.12f),leds[i%8],root);cap.transform.rotation=facing;}
  }
  foreach(var c in root.GetComponentsInChildren<Collider>())Destroy(c);
  foreach(var r in root.GetComponentsInChildren<Renderer>())r.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
  StaticBatchingUtility.Combine(root.gameObject);
  for(int i=0;i<8;i++){
   var m=new Material(Resources.Load<Shader>("FestivalParticles"));m.SetTexture("_MainTex",Texture2D.whiteTexture);m.SetFloat("_EdgeSoftness",1.8f);m.SetColor("_TintColor",palette[i%4]*1.2f);
   var beam=BatGeometry.Line("Slow sweeping sky light",new Vector3[2],1,m,root);beam.widthCurve=new AnimationCurve(new Keyframe(0,.65f),new Keyframe(1,5));beam.startColor=new Color(1,1,1,.12f);beam.endColor=new Color(1,1,1,0);beams.Add(beam);
   float angle=(i<4?-1:1)*(38+i%4*3)*Mathf.Deg2Rad;Vector3 fixture=new Vector3(Mathf.Sin(angle)*112,9,Mathf.Cos(angle)*112);BatGeometry.Box("Sky light support",fixture-Vector3.up*2,new Vector3(.23f,4,.23f),structure,root);var aperture=new Material(m);aperture.SetFloat("_EdgeSoftness",0);aperture.SetColor("_TintColor",palette[i%4]*2.5f);
   BatGeometry.Sphere("Visible sky projector lens",fixture,Vector3.one*.9f,aperture,root);
   var housing=BatGeometry.Box("Sky projector housing",fixture-Vector3.up*.45f,new Vector3(1.4f,.65f,1.4f),structure,root);
  }
 }
 void LateUpdate(){if(game==null)return;float t=(float)game.effectTime,energy=pulsePower*Mathf.Exp(-Mathf.Max(0,(float)(game.effectTime-pulseAt))*1.8f);
  for(int i=0;i<leds.Count;i++){float wave=.5f+.5f*Mathf.Sin(t*1.9f-i*.8f);Color color=Color.Lerp(palette[i%4],pulseColor,energy*.30f);leds[i].SetColor("_BaseColor",color*(.55f+wave*.65f+energy*.9f));}
  for(int i=0;i<beams.Count;i++){float side=i<4?-1:1;float angle=side*(38+i%4*3)*Mathf.Deg2Rad;Vector3 p=new Vector3(Mathf.Sin(angle)*112,9,Mathf.Cos(angle)*112);beams[i].SetPosition(0,p);beams[i].SetPosition(1,new Vector3(side*(10+Mathf.Sin(t*.28f+i*.7f)*22),32+i%4*3,98));beams[i].startColor=new Color(1,1,1,.22f+energy*.14f);}
 }
}
}
