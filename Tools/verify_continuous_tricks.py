"""Regress the actual near-player movement, not merely a successful far trick."""
import csv,pathlib,json,math,sys
root=pathlib.Path(sys.argv[1]);rows=list(csv.DictReader(next(root.glob('A-*.csv')).open()))
def f(r):return dict(x.split('=',1) for x in r['detail'].split(';') if '=' in x)
def angle(a,b):
 d=sum(x*y for x,y in zip(a,b));return math.degrees(math.acos(max(-1,min(1,d/(math.sqrt(sum(x*x for x in a))*math.sqrt(sum(x*x for x in b)))))))
results=[]
for hit in rows:
 if hit['event']!='contact' or hit['phase']!='play' or hit['kind'] not in ['cake','poison','boomerang','chest','bomb']:continue
 track=[r for r in rows if r['event']=='flight_sample' and r['phase']==hit['phase'] and r['task']==hit['task'] and float(r['time'])<float(hit['time']) and .80<float(f(r)['z'])<1.40]
 assert len(track)>=3,(hit['kind'],'no near evidence')
 points=[[float(f(r)[k]) for k in ['x','y','z']] for r in track]
 report={'kind':hit['kind'],'near_samples':len(track)}
 if hit['kind']=='boomerang':
  qs=[[float(f(r)[k]) for k in ['qx','qy','qz','qw']] for r in track]
  turn=2*math.degrees(math.acos(min(1,abs(sum(x*y for x,y in zip(qs[0],qs[-1]))))))
  assert turn>20,turn;report['near_rotation_degrees']=turn
 if hit['kind']=='cake':
  a=[y-x for x,y in zip(points[0],points[1])];b=[y-x for x,y in zip(points[-2],points[-1])];bend=angle(a,b)
  assert bend>4,bend;report['near_bend_degrees']=bend
 if hit['kind']=='poison':
  cue=next(r for r in rows if r['event']=='recoil_cue' and r['phase']==hit['phase'] and r['task']==hit['task'] and f(r)['part']=='1')
  nearest=min(track,key=lambda r:abs(float(r['time'])-float(cue['time'])))
  assert 1<float(f(nearest)['z'])<1.4
  assert float(hit['time'])-float(cue['time'])>.30
  report['near_recoil_cue_z']=float(f(nearest)['z'])
 if hit['kind']=='chest':
  height=max(p[1] for p in points)-min(p[1] for p in points);lid=max(float(f(r)['lid']) for r in track)-min(float(f(r)['lid']) for r in track)
  assert height>.06 and lid>.25,(height,lid);report.update(near_height_change=height,near_lid_change=lid)
 if hit['kind']=='bomb':assert points[0][1]-points[-1][1]>.12
 results.append(report)
assert {r['kind'] for r in results}=={'cake','poison','boomerang','chest','bomb'}
assert rows[-1]['event']=='verification_end'
(root/'continuous_tricks_assertions.json').write_text(json.dumps({'passed':True,'near_motion':results},indent=2));print(json.dumps(results,indent=2))
