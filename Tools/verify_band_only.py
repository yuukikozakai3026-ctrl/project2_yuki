"""Validate real runtime contacts and an end-on negative probe."""
import csv,json,pathlib,sys
root=pathlib.Path(sys.argv[1]);rows=list(csv.DictReader(next(root.glob('A-*.csv')).open()))
def fields(r):return dict(x.split('=',1) for x in r['detail'].split(';') if '=' in x)
contacts=[r for r in rows if r['event']=='contact']
assert contacts and all(.42999<=float(fields(r)['axis'])<=.68001 for r in contacts)
assert all(fields(r)['strong']=='True' for r in contacts)
assert rows[-1]['event']=='verification_end'
negative='Negative' in root.name
if negative:
    first=[r for r in rows if r['phase']=='play' and r['task']=='0']
    assert any(r['event']=='invalid_bat_contact' for r in first)
    assert any(r['event']=='resolved' and r['detail']=='miss' for r in first)
    assert not any(r['event'] in ['contact','points','return_begin'] for r in first)
    assert all(int(r['score'])==0 for r in first)
    assert any(r['event']=='flight_sample' and .85<float(fields(r)['z'])<1 for r in first)
else:
    assert {r['kind'] for r in contacts}=={'ball','cake','poison','boomerang','magic','bomb','bullets','chest'}
    for r in rows:
        if r['event']=='resolved' and r['phase']=='play' and r['detail']=='miss':
            assert any(x in root.name for x in ('Aligned','Suite','Visible')) and r['task']=='11',r
report={'passed':True,'band_contacts':len(contacts),'band_axis_min':min(float(fields(r)['axis']) for r in contacts),'band_axis_max':max(float(fields(r)['axis']) for r in contacts),'tip_probe_rejected_without_score_or_return':negative}
(root/'band_only_assertions.json').write_text(json.dumps(report,indent=2));print(report)
