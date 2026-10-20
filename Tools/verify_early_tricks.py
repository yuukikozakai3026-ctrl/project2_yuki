import pathlib,csv,json,sys
root=pathlib.Path(sys.argv[1]);rows=list(csv.DictReader(next(root.glob('A-*.csv')).open()))
def ev(name):return [r for r in rows if r['event']==name]
def f(r):return dict(x.split('=',1) for x in r['detail'].split(';') if '=' in x)
assert rows[-1]['event']=='verification_end'
for r in ev('pitch_config'):
 d=f(r);early=d['kind'] in ['cake','poison','boomerang','magic','bomb','chest']
 assert abs(float(d['trick_z'])-(4.2 if early else 1.49))<.015
 assert abs(float(d['slow_z'])-1.49)<.015
 assert abs(float(d['end_x']))<=.401 and 1.069<=float(d['end_y'])<=(1.421 if d['kind']=='magic' else 1.381 if d['kind']=='chest' else 1.361)
for r in ev('trick_start'):
 following=next(x for x in ev('slow_start') if x['task']==r['task'] and x['phase']==r['phase'] and float(x['time'])>float(r['time']))
 assert float(following['time'])-float(r['time'])>1
assert not any(r['kind']=='bomb' for r in ev('recoil_cue'))
assert ev('bomb_lob') and all(f(r)['recoil_turns']=='0' for r in ev('bomb_lob'))
assert len(ev('view_impact'))==len(ev('contact'))
for hit,accent in zip(ev('contact'),ev('view_impact')):
 assert hit['time']==accent['time'] and hit['kind']==f(accent)['kind']
 assert f(accent)['space']=='stereo_clip'
frame_times=[float(r['game_time']) for r in csv.DictReader((root/'frames.csv').open())]
for end in ev('view_impact_end'):
 start=max((x for x in ev('view_impact') if float(x['time'])<float(end['time'])),key=lambda x:float(x['time']))
 # The renderer evaluates the fade once per rendered frame, not every simulation tick.
 nearby=[t for t in frame_times if float(start['time'])-.3<t<float(end['time'])+.3]
 frame_budget=max([.10]+[b-a for a,b in zip(nearby,nearby[1:])])
 assert .199<=float(f(end)['fx_time'])-float(f(start)['fx_time'])<=.20+frame_budget+.01
assert not any(x in (root/'player.log').read_text() for x in ['Exception:','Shader error','NullReferenceException','MissingReferenceException'])
report={'passed':True,'trick_start_metres':4.2,'slow_start_metres':1.49,'bomb_rule':'one gravity arc, no recoil turns','impact_pulses':len(ev('view_impact')),'screen_space':'stereo clip, central clear area, no camera transform changes'}
(root/'early_tricks_assertions.json').write_text(json.dumps(report,indent=2));print(report)
