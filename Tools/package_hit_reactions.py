"""Package the verified player and untouched runtime recordings for the external drive."""
import csv,hashlib,pathlib,shutil,subprocess,sys,zipfile
run=pathlib.Path(sys.argv[1]);delivery=pathlib.Path('/Volumes/Transcend/Unity/VRBatCarnivalWorkshop');base=pathlib.Path('Builds/Windows')
doc=pathlib.Path(sys.argv[2]) if len(sys.argv)>2 else pathlib.Path('Documentation/Verification/Verification_Hit_Reactions_2026-09-29.md')
docname='Verification_HandMotion.md' if 'HandMotion' in doc.name else 'Verification_SlowTint.md' if 'SlowTint' in doc.name else 'Verification_AggressiveTricks.md' if 'AggressiveTricks' in doc.name else 'Verification_ContinuousTricks.md' if 'ContinuousTricks' in doc.name else 'Verification_EarlyImpact.md' if 'EarlyImpact' in doc.name else 'Verification_BandOnly.md' if 'BandOnly' in doc.name else 'Verification_Spectacle.md' if len(sys.argv)>2 else 'Verification_Hit_Reactions.md'
archive=pathlib.Path('Builds/BatCarnival_WindowsVR.zip')
with zipfile.ZipFile(archive,'w',zipfile.ZIP_DEFLATED,compresslevel=6) as z:
 for p in sorted(base.rglob('*')):
  if p.is_file() and not any(x.startswith('._') or 'DoNotShip' in x for x in p.parts):z.write(p,p.relative_to(base))
with zipfile.ZipFile(archive) as z:
 assert z.testzip() is None
 assert 'BatCarnival.exe' in z.namelist() and any('openxr' in x.lower() for x in z.namelist())
ffmpeg='/opt/homebrew/bin/ffmpeg'
if not (run/'HitReactions.mp4').exists():subprocess.run([ffmpeg,'-hide_banner','-loglevel','error','-y','-framerate','15','-i',str(run/'frames/%06d.jpg'),'-i',str(run/'audio.wav'),'-c:v','libx264','-threads','2','-preset','fast','-crf','20','-pix_fmt','yuv420p','-c:a','aac','-b:a','160k','-shortest','-movflags','+faststart',str(run/'HitReactions.mp4')],check=True)
rows=list(csv.DictReader(next(run.glob('A-*.csv')).open()));frames=list(csv.DictReader((run/'frames.csv').open()))
event=lambda name:next(float(x['time']) for x in rows if x['event']==name and x['kind']=='cake' and x['phase']=='play')
frame_time=lambda t:int(min(frames,key=lambda x:abs(float(x['game_time'])-t))['frame'])/15
start=frame_time(event('slow_start')-.6);duration=frame_time(event('contact')+.65)-start
subprocess.run([ffmpeg,'-hide_banner','-loglevel','error','-y','-ss',str(start),'-i',str(run/'HitReactions.mp4'),'-t',str(duration),'-c:v','libx264','-threads','2','-preset','fast','-crf','18','-c:a','aac','-movflags','+faststart',str(delivery/'A_Curve_Check.mp4')],check=True)
for src,name in [(archive,'BatCarnival_WindowsVR.zip'),(run/'HitReactions.mp4','A_Revised_Playthrough.mp4'),(doc,docname)]:
 tmp=delivery/(name+'.new');shutil.copy2(src,tmp);assert hashlib.sha256(src.read_bytes()).digest()==hashlib.sha256(tmp.read_bytes()).digest();tmp.replace(delivery/name)
shutil.copytree(run/'reactions',delivery/'VisualReview/reactions',dirs_exist_ok=True)
(delivery/'VisualReview/review.html').write_text((run/'review.html').read_text().replace('src="HitReactions.mp4"','src="../A_Revised_Playthrough.mp4"'))
p=delivery/'README.md';p.write_text('\n'.join(x for x in p.read_text().splitlines() if 'A_AllTypes_LeftView.mp4' not in x)+'\n')
# The alternate-view recording remains in project verification history; avoid a stale public movie.
old=delivery/'A_AllTypes_LeftView.mp4'
if old.exists():old.unlink()
names=['BatCarnival_WindowsVR.zip','A_Revised_Playthrough.mp4','A_Curve_Check.mp4',docname]
lines=[hashlib.sha256((delivery/n).read_bytes()).hexdigest()+'  '+n for n in names]
(delivery/'SHA256SUMS.txt').write_text('\n'.join(lines)+'\n');print('\n'.join(lines))
