"""Observe actual centre motion to the hand zone, without mistaking actor spin for curvature."""
import csv,json,math,pathlib,sys
root=pathlib.Path(sys.argv[1]);rows=list(csv.DictReader(next(root.glob('A-*.csv')).open()))
def f(r):return dict(x.split('=',1) for x in r['detail'].split(';') if '=' in x)
def angle(a,b):
 norm=math.sqrt(sum(x*x for x in a)*sum(x*x for x in b));return math.degrees(math.acos(max(-1,min(1,sum(x*y for x,y in zip(a,b))/norm))))
assert rows[-1]['event']=='verification_end'
observing='--normal' not in sys.argv
special=['cake','poison','boomerang','magic','bomb','chest']
if observing:assert not any(r['event']=='contact' and r['kind'] in special for r in rows), 'The six observed special routes must reach the endpoint without bat contact'
results=[]
for kind in ['cake','poison','boomerang','magic','bomb','chest']:
 task=next(r['task'] for r in rows if r['event']=='trick_start' and r['kind']==kind and r['phase']=='play')
 track=[r for r in rows if r['task']==task and r['phase']=='play' and r['event']=='flight_sample' and (.50 if observing else .80)<float(f(r)['z'])<(.95 if observing else 1.25)]
 assert len(track)>=6,(kind,len(track))
 points=[[float(f(r)[k]) for k in ['x','y','z']] for r in track]
 directions=[[q-p for p,q in zip(a,b)] for a,b in zip(points,points[1:])]
 bend=max(angle(directions[0],v) for v in directions[1:]);step=max(angle(a,b) for a,b in zip(directions,directions[1:]))
 assert bend>(12 if observing or kind=='boomerang' else 8),(kind,'still straight before contact',bend)
 if observing or kind!='poison':assert step<15,(kind,'unexplained sudden kink',step)
 assert all(abs(p[0])<.9 and .80<p[1]<1.9 for p in points),(kind,'outside reachable volume')
 results.append(dict(kind=kind,hand_zone_bend_degrees=round(bend,2),max_sample_turn_degrees=round(step,2),samples=len(track),start_z=points[0][2],end_z=points[-1][2]))
assert not any(s in (root/'player.log').read_text() for s in ['Exception:','Shader error','NullReferenceException','MissingReferenceException'])
(root/'hand_motion_assertions.json').write_text(json.dumps(dict(passed=True,centre_motion=results),indent=2));print(json.dumps(results,indent=2))
