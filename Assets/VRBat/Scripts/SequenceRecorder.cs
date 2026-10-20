using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
namespace VRBat {
// Test-only, first-person 15 fps capture. No simulation speed, camera or input changes.
public sealed class SequenceRecorder:MonoBehaviour {
 readonly HashSet<string> tintCaptured=new HashSet<string>();
 bool tintComparison=System.Array.IndexOf(System.Environment.GetCommandLineArgs(),"--compare-slow-tint")>=0;
 public BatGame game;RenderTexture target;Texture2D frame;int index;double next;string directory;StreamWriter manifest;
 IEnumerator Start(){yield return new WaitForEndOfFrame();directory=Path.Combine(game.evidence,"frames");Directory.CreateDirectory(directory);manifest=new StreamWriter(Path.Combine(game.evidence,"frames.csv")){AutoFlush=true};manifest.WriteLine("frame,game_time,phase,pitch,kind");game.view.eye.gameObject.AddComponent<AudioEvidence>().directory=game.evidence;target=new RenderTexture(1280,720,24);frame=new Texture2D(1280,720,TextureFormat.RGB24,false);next=Time.realtimeSinceStartupAsDouble;
  while(true){yield return new WaitForEndOfFrame();double now=Time.realtimeSinceStartupAsDouble;if(now<next||game.view.eye==null)continue;
   var camera=game.view.eye;var old=camera.targetTexture;camera.targetTexture=target;camera.Render();var active=RenderTexture.active;RenderTexture.active=target;frame.ReadPixels(new Rect(0,0,1280,720),0,0);frame.Apply();RenderTexture.active=active;camera.targetTexture=old;
   CaptureTintComparison(camera);
   byte[] bytes=frame.EncodeToJPG(88);int copies=0;while(next<=now&&copies++<30){manifest.WriteLine($"{index},{game.now:F4},{game.phase},{game.index},{game.current?.kind}");File.WriteAllBytes(Path.Combine(directory,$"{index++:D6}.jpg"),bytes);next+=1d/15;}
  }
 }
 void CaptureTintComparison(Camera camera){
  var c=game.current;var tint=game.slowViewTint;
  if(!tintComparison||c==null||c.done||game.phase!="play"||tint==null||tint.Amount<.999f||game.now-c.slowAt<.65)return;
  string key=c.kind+(c.kind=="magic"?"-"+c.magicCount:"");if(!tintCaptured.Add(key))return;
  string folder=Path.Combine(game.evidence,"TintComparison");Directory.CreateDirectory(folder);
  var old=camera.targetTexture;var active=RenderTexture.active;float amount=tint.Amount;
  for(int variant=-1;variant<3;variant++){
   tint.SetVariant(variant<0?SlowViewTint.SelectedVariant:variant);tint.SetAmount(variant<0?0:amount);
   camera.targetTexture=target;camera.Render();RenderTexture.active=target;frame.ReadPixels(new Rect(0,0,1280,720),0,0);frame.Apply();
   File.WriteAllBytes(Path.Combine(folder,key+"-"+(variant<0?"normal":((char)('A'+variant)).ToString())+".png"),frame.EncodeToPNG());
  }
  tint.SetVariant(SlowViewTint.SelectedVariant);tint.SetAmount(amount);
  // Restore the actual chosen view before writing the ordinary recording frame.
  camera.Render();RenderTexture.active=target;frame.ReadPixels(new Rect(0,0,1280,720),0,0);frame.Apply();camera.targetTexture=old;RenderTexture.active=active;
  game.Event("slow_tint_comparison",$"kind={key};amount={amount:F3};selected={SlowViewTint.SelectedVariant};same_frame=True");
 }
 void OnDestroy(){manifest?.Dispose();if(target){target.Release();Destroy(target);}if(frame)Destroy(frame);}
}
}
