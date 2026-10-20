using UnityEngine;
namespace VRBat {
public sealed class ManualPitch {
 public readonly PlannedPitch plan; public readonly GameObject actor;public bool done;public Vector3 previous;
 readonly BatGame game;float nextSample;double fuseAt;
 public ManualPitch(BatGame g,PlannedPitch p){game=g;plan=p;previous=p.source;actor=g.view.Target(p.bomb?"bomb":"ball",false,g.W(previous),g.targets);PitchVisuals.Trail(actor,p.bomb?"bomb":"ball",g.view);g.fx.LaunchCue(g.W(previous));}
 public void Step(float dt){
  if(done)return;float time=(float)(game.now-game.runAt);var at=plan.At(time);actor.transform.position=game.W(at);
  actor.transform.Rotate(0,0,30*dt*game.effectScale,Space.Self);
  if(plan.bomb&&game.effectTime>=fuseAt){fuseAt=game.effectTime+.13;var tip=actor.transform.Find("Burning fuse tip");if(tip)game.fx.Burst(tip.position,"spark_01",2,.016f,.25f,.20f,new Color(1,.65f,.15f));}
  if(game.BatAvailable&&at.z<2&&BatGeometry.SweepSphere(previous,at,plan.bomb?.11f:.07f,
    game.LocalGrip(game.oldGrip),game.LocalAxis(game.oldAxis),game.LocalGrip(game.grip),game.LocalAxis(game.axis),out float s)){
   var point=game.W(at);bool band=AFlight.IsStrong(s);var movement=(game.grip+game.axis*s)-(game.oldGrip+game.oldAxis*s);float speed=movement.magnitude/dt;
   if(plan.bomb){game.BombContact(point);Object.Destroy(actor);}
   else {int points=game.tuning.Points(speed,band);game.Award(points,point,band);game.success[0]++;game.fx.Hit("ball",point,band);
    game.fx.HitFar(actor,point,"ball",Vector3.zero,band,movement/dt,game.D(at-previous),plan.id+1,0);
    game.Event("manual_hit",$"id={plan.id};speed={speed:F3};bonus={(band?game.tuning.bandSpeedBonus:0):F3};points={points};axis={s:F3}","ball");}
   done=true;return;
  }
  if(time>=plan.Arrival){done=true;Object.Destroy(actor);game.Event("manual_miss",$"id={plan.id}",plan.bomb?"bomb":"ball");}
  previous=at;
  if(time>=nextSample){nextSample=time+.05f;game.Event("manual_flight",$"id={plan.id};x={at.x:F4};y={at.y:F4};z={at.z:F4}",plan.bomb?"bomb":"ball");}
 }
}
}
