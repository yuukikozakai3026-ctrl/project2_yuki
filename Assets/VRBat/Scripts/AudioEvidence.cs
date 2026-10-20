using System.IO;
using UnityEngine;
namespace VRBat {
// Test-only listener capture, after the limiter, for normal-speed review footage.
public sealed class AudioEvidence:MonoBehaviour {
 public string directory;readonly object gate=new object();MemoryStream pcm=new MemoryStream();int channels=2,rate;bool closed;
 void Awake(){rate=AudioSettings.outputSampleRate;}
 void OnAudioFilterRead(float[] data,int count){lock(gate){if(closed)return;channels=count;foreach(float f in data){short n=(short)(Mathf.Clamp(f,-1,1)*32767);pcm.WriteByte((byte)n);pcm.WriteByte((byte)(n>>8));}}}
 void OnDestroy(){lock(gate){closed=true;if(string.IsNullOrEmpty(directory))return;using(var w=new BinaryWriter(File.Create(Path.Combine(directory,"audio.wav")))){int length=(int)pcm.Length;w.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));w.Write(length+36);w.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt "));w.Write(16);w.Write((short)1);w.Write((short)channels);w.Write(rate);w.Write(rate*channels*2);w.Write((short)(channels*2));w.Write((short)16);w.Write(System.Text.Encoding.ASCII.GetBytes("data"));w.Write(length);w.Write(pcm.ToArray());}pcm.Dispose();}}
}
}
