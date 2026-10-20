"""Verify visual event coverage without changing production gameplay."""
import csv,json,pathlib,sys,tarfile,hashlib
root=pathlib.Path(sys.argv[1]);rows=list(csv.DictReader(next(root.glob('A-*.csv')).open()))
def detail(r):return dict(x.split('=',1) for x in r['detail'].split(';') if '=' in x)
contacts=[r for r in rows if r['event']=='contact'];hits=[r for r in rows if r['event']=='spectacle_hit'];skies=[r for r in rows if r['event']=='spectacle_sky']
assert len(hits)==len(contacts)
kinds={'ball','cake','poison','boomerang','magic','bomb','bullets','chest'}
assert {detail(r)['kind'] for r in hits}==kinds
assert {detail(r)['kind'] for r in skies}==kinds
assert all(float(detail(r)['z'])>=70 for r in skies)
assert rows[-1]['event']=='verification_end'
assert not any(x in (root/'player.log').read_text() for x in ['Exception:','Shader error','NullReferenceException','MissingReferenceException'])
with tarfile.open('Documentation/Revisions/Spectacle/BeforeSpectacle.tgz') as t:
 for name in ['OutboundShot.cs','BatGeometry.cs']:
  p=pathlib.Path('Assets/VRBat/Scripts')/name
  assert hashlib.sha256(t.extractfile(str(p)).read()).digest()==hashlib.sha256(p.read_bytes()).digest(),name
report={'passed' :True,'kinds':sorted(kinds),'contact_accents':len(hits),'sky_bursts':len(skies),'bat_geometry_and_outbound_sources_unchanged':True,'timing_changes':'shorter cooldown; separate presentation clock during slow motion'}
(root/'spectacle_assertions.json').write_text(json.dumps(report,indent=2));print(report)
