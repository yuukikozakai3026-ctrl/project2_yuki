using UnityEngine;
using UnityEngine.Rendering;
namespace VRBat {
// The same clip-space graphic is drawn into each XR eye. No world surface, parallax,
// camera shake, or head-pose change is used for these short peripheral accents.
public sealed class ImpactView:MonoBehaviour {
 BatGame game;Material material;Mesh mesh;MeshRenderer display;double began=-10;bool visible;int hitIndex,warmupFrames=3;
 public static ImpactView Create(BatGame game){var o=new GameObject("View impact lines");o.transform.SetParent(game.view.eye.transform,false);var effect=o.AddComponent<ImpactView>();effect.game=game;effect.Build();return effect;}
 void Build(){
  mesh=new Mesh{name="Stereo screen quad"};mesh.vertices=new[]{new Vector3(-1,-1,0),new Vector3(1,-1,0),new Vector3(1,1,0),new Vector3(-1,1,0)};mesh.triangles=new[]{0,1,2,0,2,3};mesh.bounds=new Bounds(Vector3.zero,Vector3.one*100000);
  gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;display=gameObject.AddComponent<MeshRenderer>();material=new Material(Resources.Load<Shader>("ViewImpact"));display.sharedMaterial=material;display.shadowCastingMode=ShadowCastingMode.Off;display.receiveShadows=false;display.enabled=false;
 }
 public void Pulse(string kind){began=game.effectTime;hitIndex++;material.SetFloat("_Pattern",hitIndex%7);visible=true;game.Event("view_impact",$"kind={kind};space=stereo_clip;fx_time={began:F5}");}
 public void Clear(){visible=false;display.enabled=false;}
 void LateUpdate(){
  float t=(float)(game.effectTime-began);float alpha=Mathf.SmoothStep(0,1,t/.016f)*(1-Mathf.SmoothStep(.035f,.20f,t));
  display.enabled=warmupFrames-->0||(visible&&!game.paused&&alpha>.001f);
  if(visible&&t>=.20f){visible=false;game.Event("view_impact_end",$"fx_time={game.effectTime:F5}");}
  material.SetFloat("_Pulse",visible?alpha*.78f:0);
 }
 void OnDestroy(){if(material)Destroy(material);if(mesh)Destroy(mesh);}
}
public partial class BatGame {
 public ImpactView impactView;
}
}
