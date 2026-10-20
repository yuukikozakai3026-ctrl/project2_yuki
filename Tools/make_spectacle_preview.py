import pathlib,csv,subprocess,sys
root=pathlib.Path(sys.argv[1]);dst=pathlib.Path('/Volumes/Transcend/Unity/VRBatCarnivalWorkshop/A_Spectacle_Preview.mp4')
rows=list(csv.DictReader(next(root.glob('A-*.csv')).open()));frames=list(csv.DictReader((root/'frames.csv').open()))
start=next(float(x['time']) for x in rows if x['event']=='slow_start' and x['kind']=='ball' and x['phase']=='play')-.5
end=next(float(x['time']) for x in rows if x['event']=='outfield_arrival' and 'kind=cake;' in x['detail'] and float(x['time'])>start)+2.2
frame_time=lambda t:int(min(frames,key=lambda x:abs(float(x['game_time'])-t))['frame'])/15
first=frame_time(start);length=frame_time(end)-first
subprocess.run(['/opt/homebrew/bin/ffmpeg','-hide_banner','-loglevel','error','-y','-ss',str(first),'-i',str(root/'HitReactions.mp4'),'-t',str(length),'-c:v','libx264','-threads','2','-preset','fast','-crf','18','-c:a','aac','-movflags','+faststart',str(dst)],check=True)
print('PREVIEW',round(length,2),'seconds',dst)
