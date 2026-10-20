"""Production log assertions for variable pitches and staged hit reactions."""
import csv,pathlib,sys,json,math
root=pathlib.Path(sys.argv[1]);rows=list(csv.DictReader(next(root.glob('A-*.csv')).open()))
def d(r):return dict(x.split('=',1) for x in r['detail'].split(';') if '=' in x)
def e(name):return [r for r in rows if r['event']==name]
configs=[d(r) for r in e('pitch_config') if r['phase']=='play']
assert all(abs(float(x['end_x']))<=.401 and 1.069<=float(x['end_y'])<=(1.421 if x['kind']=='magic' else 1.381 if x['kind']=='chest' else 1.361) for x in configs)
assert len({x['end_x'] for x in configs})>=7
returns=e('return_begin');arrivals=e('outfield_arrival');kinds={'ball','cake','poison','boomerang','magic','bomb','bullets','chest'}
assert {d(r)['kind'] for r in arrivals}==kinds
assert all(float(d(r)['distance'])==68 for r in returns)
assert len({round(float(d(r)['target_x'])) for r in returns})>=6
for r in returns:
    a=d(r);at=float(r['time']);land=next(x for x in arrivals if d(x)['kind']==a['kind'] and float(x['time'])>at and abs(float(d(x)['x'])-float(a['target_x']))<.02)
    assert float(d(land)['z'])>59
    assert abs(float(d(land)['x'])-float(a['target_x']))<.02
# The next pitch now starts while the prior outbound animation continues.
waits=[]
for r in e('resolved'):
    if r['phase']!='play':continue
    at=float(r['time'])
    future=[x for x in e('pitch_config') if x['phase']=='play' and float(x['time'])>at]
    if not future:continue
    elapsed=float(future[0]['time'])-at
    expected=.45 if r['detail']=='miss' or r['kind']=='bullets' else .75 if r['kind']=='chest' else .60
    assert abs(elapsed-expected)<.012,(r,elapsed,expected)
    waits.append(elapsed)
assert waits and max(waits)<.77
assert len(e('crowd_response'))>=8
assert e('reaction_sample') and all(float(d(r)['z'])>.48 for r in e('reaction_sample'))
assert not any(x in (root/'player.log').read_text() for x in ['Exception:','NullReferenceException','MissingReferenceException','Shader error'])
report={'passed':True,'pitch_endpoints':[{'kind':x['kind'],'x':x['end_x'],'y':x['end_y']} for x in configs], 'outfield_targets':[d(r) for r in returns], 'cooldowns_seconds':waits,'staged_arrivals':len(arrivals),'crowd_responses':len(e('crowd_response'))}
(root/'reaction_assertions.json').write_text(json.dumps(report,ensure_ascii=False,indent=2));print('REACTIONS_PASS',len(configs),'variable pitches;',len(arrivals),'completed outfield reactions')
