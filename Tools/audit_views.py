import subprocess,json
from pathlib import Path
r=Path(__file__).resolve().parents[1];cli=str(Path.home()/'.unity/bin/unity')
def call(name,*args):
 p=subprocess.run([cli,'command',name,'--project-path',str(r),'--caller','plugin','--skill','unity-cli',*args,'--format','json'],capture_output=True,text=True)
 d=json.loads(p.stdout);assert d.get('success'),d;return d
for height in [1.35,1.65,1.90]:
 for left in [False,True]:
  code=f'''var g=UnityEngine.Object.FindFirstObjectByType<VRBat.BatGame>();g.enabled=false;g.height={height}f;g.left={str(left).lower()};g.ResetGame(true);g.view.menu.SetActive(false);g.view.Lamps(0);g.view.score.text="0";g.view.time.text="90";g.view.band.color=VRBat.BatGeometry.ColorOf("#B7C4C8");g.view.eye.transform.SetPositionAndRotation(new UnityEngine.Vector3(0,{height}f,0),UnityEngine.Quaternion.Euler(20,0,0));g.view.bat.SetPositionAndRotation(g.grip,UnityEngine.Quaternion.FromToRotation(UnityEngine.Vector3.up,g.axis));return "ready";'''
  call('eval',code)
  name=f'Audit-B-{height:.2f}-'+('left' if left else 'right')+'.png'
  call('screenshot','--view','game','--width','1920','--height','1080','--output','Documentation/Verification/'+name)
  print(name,flush=True)
for name,pos,rot in [('top','0,4,.7','90,0,0'),('side','-3,1.7,.7','15,90,0')]:
 call('eval',f'var g=UnityEngine.Object.FindFirstObjectByType<VRBat.BatGame>();g.height=1.65f;g.left=false;g.ResetGame(true);g.view.menu.SetActive(false);g.view.eye.transform.SetPositionAndRotation(new UnityEngine.Vector3({pos}),UnityEngine.Quaternion.Euler({rot}));return "ready";')
 call('screenshot','--view','game','--width','1920','--height','1080','--output','Documentation/Verification/Audit-B-'+name+'.png')
 print(name,flush=True)
