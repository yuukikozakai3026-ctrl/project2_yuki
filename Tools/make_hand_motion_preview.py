import pathlib,csv,subprocess,sys
root=pathlib.Path(sys.argv[1]);observing='--observe' in sys.argv
out=pathlib.Path('/Volumes/Transcend/Unity/VRBatCarnivalWorkshop')/('A_HandMotion_Observation.mp4' if observing else 'A_HandMotion_Preview.mp4')
rows=list(csv.DictReader(next(root.glob('A-*.csv')).open()));frames=list(csv.DictReader((root/'frames.csv').open()))
def video_time(t):return int(min(frames,key=lambda x:abs(float(x['game_time'])-t))['frame'])/15
segments=[]
for kind in ['cake','poison','boomerang','magic','bomb','chest']:
 start=next(r for r in rows if r['event']=='slow_start' and r['phase']=='play' and r['kind']==kind)
 end=next(r for r in rows if r['event']==('resolved' if observing else 'contact') and r['phase']=='play' and r['task']==start['task'])
 segments.append((video_time(float(start['time'])-.15),video_time(float(end['time'])+.25)))
filters=[];inputs=''
for i,(a,b) in enumerate(segments):
 filters.extend([f'[0:v]trim=start={a}:end={b},setpts=PTS-STARTPTS[v{i}]',f'[0:a]atrim=start={a}:end={b},asetpts=PTS-STARTPTS[a{i}]']);inputs+=f'[v{i}][a{i}]'
filters.append(inputs+f'concat=n={len(segments)}:v=1:a=1[v][a]')
subprocess.run(['/opt/homebrew/bin/ffmpeg','-hide_banner','-loglevel','error','-y','-i',str(root/'HitReactions.mp4'),'-filter_complex',';'.join(filters),'-map','[v]','-map','[a]','-c:v','libx264','-threads','2','-preset','fast','-crf','18','-c:a','aac','-movflags','+faststart',str(out)],check=True)
print(out)
