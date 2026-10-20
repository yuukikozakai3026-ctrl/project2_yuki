"""Verify the production contact/event stream, including real bat contacts (no score injection)."""
import csv, pathlib, sys, json
root=pathlib.Path(sys.argv[1]); rows=list(csv.DictReader(next(root.glob('A-*.csv')).open()))
def events(name): return [r for r in rows if r['event']==name]
def detail(row): return dict(p.split('=',1) for p in row['detail'].split(';') if '=' in p)
kinds={'ball','cake','poison','boomerang','magic','bomb','bullets','chest'}
assert {r['kind'] for r in events('contact')}==kinds
assert {int(detail(r)['count']) for r in events('pitch_config') if detail(r).get('kind')=='magic'}=={3,5,8}
assert any(detail(r).get('fake_first')=='True' for r in events('magic_real_contact'))
assert len(events('magic_real_contact'))==3
for r in events('magic_fake_contact'): assert int(detail(r)['score_unchanged'])==int(r['score'])
assert {detail(r)['item'] for r in events('contact') if r['kind']=='bullets'}=={'0','1','2'}
assert all(.42999<=float(detail(r)['axis'])<=.68001 and detail(r)['strong']=='True' for r in events('contact'))
assert all(float(detail(r)['distance'])==68 for r in events('return_begin'))
assert all(float(detail(r)['z'])>=60 for r in events('bomb_far_explosion')) and events('bomb_far_explosion')
assert events('miss') and events('results') and events('verification_end')
assert sum(r['kind']=='poison' for r in events('recoil_cue'))==2
assert not any(r['kind']=='bomb' for r in events('recoil_cue'))
assert events('bomb_lob')
assert all(float(detail(r)['lead'])>=.28 for r in events('recoil_cue'))
log=(root/'player.log').read_text()
assert not any(s in log for s in ('Exception:', 'NullReferenceException','MissingReferenceException','Shader error'))
report={'passed':True,'tested_kinds':sorted(kinds),'results':events('results')[0]['detail'], 'magic_real_contacts':[detail(r) for r in events('magic_real_contact')],'far_explosion':detail(events('bomb_far_explosion')[0])}
(root/'assertions.json').write_text(json.dumps(report,ensure_ascii=False,indent=2))
print(json.dumps(report,ensure_ascii=False,indent=2))
