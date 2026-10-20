using UnityEngine;
namespace VRBat {
public class AudioLimiter:MonoBehaviour {public float observedPeak;void OnAudioFilterRead(float[] data,int channels){const float ceiling=.8912509f;for(int i=0;i<data.Length;i++){observedPeak=Mathf.Max(observedPeak,Mathf.Abs(data[i]));data[i]=Mathf.Clamp(data[i],-ceiling,ceiling);}}}
}
