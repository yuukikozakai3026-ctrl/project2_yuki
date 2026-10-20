using UnityEngine;
namespace VRBat {
// Strength is deliberately absent: the same contact produces exactly the same flight.
public sealed class OutboundShot {
 public readonly Vector3 start,end,direction,side;public readonly float seconds,arc,bend;public readonly string kind;
 public OutboundShot(Vector3 contact,Vector3 swing,Vector3 incoming,string type,int variation){
  kind=type;start=contact;Vector3 motion=swing.sqrMagnitude>.04f?swing.normalized:Vector3.zero;
  float azimuth=Mathf.Clamp(contact.x*.72f+motion.x*.32f+incoming.normalized.x*.16f,-.49f,.49f);
  direction=new Vector3(Mathf.Sin(azimuth),0,Mathf.Cos(azimuth));side=new Vector3(direction.z,0,-direction.x);
  end=start+direction*AFlight.ReturnDistance;end.y=kind=="bomb"?3.5f:.10f;
  seconds=kind=="bullets"?2.45f:kind=="chest"?3.5f:3.15f;
  arc=(kind=="boomerang"?9:kind=="bullets"?3.2f:kind=="chest"?5.5f:kind=="cake"?3.8f:kind=="poison"?4.4f:kind=="bomb"?4.8f:6.5f)+Mathf.Clamp(motion.y,-.5f,.5f)*2;
  bend=kind=="boomerang"?Mathf.Sign(azimuth+.001f)*9:kind=="magic"?Mathf.Sign(azimuth+.001f)*2.5f:0;
 }
 public Vector3 At(float elapsed){float t=Mathf.Clamp01(elapsed/seconds),f=Mathf.Pow(t,1.18f);return Vector3.Lerp(start,end,f)+Vector3.up*(arc*4*f*(1-f))+side*(Mathf.Sin(Mathf.PI*f)*bend);}
}
}
