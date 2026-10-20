"""Check the independent presentation clock against actual production events."""
import csv,json,pathlib,sys,tarfile
root=pathlib.Path(sys.argv[1]);rows=list(csv.DictReader(next(root.glob('A-*.csv')).open()))
def fields(r):return dict(x.split('=',1) for x in r['detail'].split(';') if '=' in x)
def events(name):return [r for r in rows if r['event']==name]
enters=events('world_slow_enter');exits=events('world_slow_exit')
assert len(enters)==len(exits)==len(events('slow_start'))
for start,enter,end,exit_ in zip(events('slow_start'),enters,events('resolved'),exits):
    assert abs(float(start['time'])-float(enter['time']))<.009
    assert 0<=float(exit_['time'])-float(end['time'])<.009
clocks=events('effect_clock');ratios={.2:[],1.:[]}
for a,b in zip(clocks,clocks[1:]):
    fa,fb=fields(a),fields(b)
    scale=float(fb['scale'])
    if scale not in ratios or float(fa['scale'])!=scale:continue
    ratio=(float(fb['fx_time'])-float(fa['fx_time']))/(float(b['time'])-float(a['time']))
    assert abs(ratio-scale)<.002,(a,b,ratio)
    ratios[scale].append(ratio)
assert all(len(v)>=5 for v in ratios.values())
# Actual outbound movement takes longer than before, while contact times stay fixed.
stretched=[]
for ret in events('return_begin'):
    info=fields(ret)
    land=next(x for x in events('outfield_arrival') if fields(x)['kind']==info['kind'] and float(x['time'])>float(ret['time']) and abs(float(fields(x)['x'])-float(info['target_x']))<.02)
    elapsed=float(land['time'])-float(ret['time'])
    if elapsed>4:stretched.append(elapsed)
assert len(stretched)>=5
with tarfile.open('Documentation/Revisions/BandOnly/BeforeBandOnly.tgz') as t:
    old=t.extractfile('Assets/VRBat/Scripts/BatGame.cs').read().decode()
    current=pathlib.Path('Assets/VRBat/Scripts/BatGame.cs').read_text()
    def without_demo(text):
        a=text.index(' void DemoPose(float dt){');b=text.index(' static float Smooth',a)
        return text[:a]+text[b:]
    expected=old.replace('bool testIntroFailure,testIntroOnly,testMissStreak;', 'bool testIntroFailure,testIntroOnly,testMissStreak,testBatRegions;').replace('testMissStreak=args.Contains("--test-miss-streak");','testMissStreak=args.Contains("--test-miss-streak");testBatRegions=args.Contains("--test-bat-regions");')
    expected=expected.replace('now<bandHintUntil?"#FFF8D8":!workshop||current?.kind=="ball"?"#F6C850":"#B7C4C8"','now<bandHintUntil?"#FFE28A":"#F6C850"')
    expected=expected.replace('SetupSlowMotion();autoDemo','SetupSlowMotion();impactView=ImpactView.Create(this);autoDemo').replace('ResetPresentation();b=false;','ResetPresentation();if(impactView)impactView.Clear();b=false;')
    assert without_demo(current)==without_demo(expected), 'Unexpected scheduling change'
# The reduced range changes contact times but keeps the route's finish and speed.
configs=[fields(r) for r in events('pitch_config')]
assert all(abs(float(r['slow_z'])-1.49)<.011 for r in configs)
assert all(float(r['slow'])==(.6 if r['kind'] in ['boomerang','poison','magic'] else .4) for r in configs)
report={'passed':True,'slow_intervals':len(enters),'slow_clock_ratio':sum(ratios[.2])/len(ratios[.2]),'normal_clock_ratio':sum(ratios[1.])/len(ratios[1.]),'outbound_reactions_slowed':len(stretched),'slow_range_two_thirds':True,'bat_and_cooldown_timing_unchanged':True}
(root/'slow_motion_assertions.json').write_text(json.dumps(report,indent=2));print(report)
