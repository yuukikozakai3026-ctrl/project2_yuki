"""Verify actual near-zone velocities, cue lead time and separated magic contacts."""
import csv,json,math,pathlib,statistics,sys
root=pathlib.Path(sys.argv[1]);rows=list(csv.DictReader(next(root.glob('A-*.csv')).open()))
def f(r):return dict(x.split('=',1) for x in r['detail'].split(';') if '=' in x)
assert rows[-1]['event']=='verification_end'
results=[]
for cfg in [r for r in rows if r['event']=='pitch_config' and r['phase']=='play']:
 kind=f(cfg)['kind'];target=.6 if kind in ['boomerang','poison','magic'] else .4
 assert abs(float(f(cfg)['slow'])-target)<.001
 track=[r for r in rows if r['event']=='flight_sample' and r['phase']==cfg['phase'] and r['task']==cfg['task'] and .8<float(f(r)['z'])<1.4]
 speeds=[]
 for a,b in zip(track,track[1:]):
  pa,pb=f(a),f(b);speeds.append(math.sqrt(sum((float(pb[k])-float(pa[k]))**2 for k in ['x','y','z']))/(float(b['time'])-float(a['time'])))
 assert len(speeds)>=2,(kind,'insufficient near samples')
 pace=statistics.median(speeds);assert abs(pace-target)<.025,(kind,pace)
 results.append(dict(kind=kind,measured_near_speed=round(pace,3),samples=len(track)))
for r in rows:
 if r['event']=='recoil_cue':assert float(f(r)['lead'])>=.28
 if r['event']=='contact':assert .42999<=float(f(r)['axis'])<=.68001
magic=[r for r in rows if r['event']=='magic_real_contact'];assert len(magic)>=2
assert {int(f(r)['count']) for r in rows if r['event']=='pitch_config' and f(r)['kind']=='magic'}=={3,5,8}
assert any(f(r)['fake_first']=='True' for r in magic), 'Fake must remain hittable without awarding a real hit'
assert not any(x in (root/'player.log').read_text() for x in ['Exception:','Shader error','NullReferenceException','MissingReferenceException'])
report=dict(passed=True,speed_multiplier=1.5,near_pace=results,magic_contacts=[f(r) for r in magic],cue_lead_min=.28)
(root/'aggressive_tricks_assertions.json').write_text(json.dumps(report,indent=2));print(json.dumps(report,indent=2))
