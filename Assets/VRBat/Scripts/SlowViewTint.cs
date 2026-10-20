using UnityEngine;
using UnityEngine.Rendering;
namespace VRBat {
// Uniform stereo view tint; no camera motion, blur, or colour replacement.
public sealed class SlowViewTint:MonoBehaviour {
 Material material;Mesh mesh;MeshRenderer display;
 public float Amount {get;private set;}
 public const int SelectedVariant=2;
 public static readonly float[] Strengths={.55f,1f,1.65f};
 public static SlowViewTint Create(Camera camera){
  var root=new GameObject("Slow motion view tint");root.transform.SetParent(camera.transform,false);
  var tint=root.AddComponent<SlowViewTint>();tint.Build();return tint;
 }
 void Build(){
  mesh=new Mesh{name="Slow tint stereo quad"};mesh.vertices=new[]{new Vector3(-1,-1,0),new Vector3(1,-1,0),new Vector3(1,1,0),new Vector3(-1,1,0)};mesh.triangles=new[]{0,1,2,0,2,3};mesh.bounds=new Bounds(Vector3.zero,Vector3.one*100000);
  gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;display=gameObject.AddComponent<MeshRenderer>();
  material=new Material(Resources.Load<Shader>("SlowViewTint"));display.sharedMaterial=material;display.shadowCastingMode=ShadowCastingMode.Off;display.receiveShadows=false;display.enabled=false;SetVariant(SelectedVariant);
 }
 public void SetVariant(int variant){material.SetFloat("_Strength",Strengths[Mathf.Clamp(variant,0,2)]);}
 public void SetAmount(float amount){Amount=Mathf.Clamp01(amount);material.SetFloat("_Amount",Amount);display.enabled=Amount>.0001f;}
 void OnDestroy(){if(material)Destroy(material);if(mesh)Destroy(mesh);}
}
}
