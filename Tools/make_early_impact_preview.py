import pathlib,csv,subprocess,sys
root=pathlib.Path(sys.argv[1]);out=pathlib.Path('/Volumes/Transcend/Unity/VRBatCarnivalWorkshop/A_EarlyTricks_Impact.mp4')
rows=list(csv.DictReader(next(root.glob('A-*.csv')).open()));frames=list(csv.DictReader((root/'frames.csv').open()))
def video_time(t):return int(min(frames,key=lambda x:abs(float(x['game_time'])-t))['frame'])/15
segments=[]
for kind in ['ball','cake','poison','boomerang','bomb']:
 hit=next(x for x in rows if x['event']=='contact' and x['phase']=='play' and x['kind']==kind)
 start=float(hit['time'])-.45 if kind=='ball' else float(next(x for x in rows if x['event']=='trick_start' and x['phase']=='play' and x['task']==hit['task'])['time'])-.15
 segments.append((video_time(start),video_time(float(hit['time'])+.55)))
filters=[];inputs=''
for i,(a,b) in enumerate(segments):
 filters.extend([f'[0:v]trim=start={a}:end={b},setpts=PTS-STARTPTS[v{i}]',f'[0:a]atrim=start={a}:end={b},asetpts=PTS-STARTPTS[a{i}]']);inputs+=f'[v{i}][a{i}]'
filters.append(inputs+f'concat=n={len(segments)}:v=1:a=1[v][a]')
subprocess.run(['/opt/homebrew/bin/ffmpeg','-hide_banner','-loglevel','error','-y','-i',str(root/'HitReactions.mp4'),'-filter_complex',';'.join(filters),'-map','[v]','-map','[a]','-c:v','libx264','-threads','2','-preset','fast','-crf','18','-c:a','aac','-movflags','+faststart',str(out)],check=True)
print('EARLY_IMPACT_PREVIEW',round(sum(b-a for a,b in segments),2),'seconds',out)
