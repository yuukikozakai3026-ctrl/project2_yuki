import pathlib,csv,shutil,sys
root=pathlib.Path(sys.argv[1]);rows=list(csv.DictReader(next(root.glob('A-*.csv')).open()));frames=list(csv.DictReader((root/'frames.csv').open()));out=root/'hand-motion';out.mkdir(exist_ok=True)
for task in sorted({r['task'] for r in rows if r['event']=='trick_start' and r['phase']=='play'},key=int):
 track=[]
 for r in rows:
  if r['task']!=task or r['phase']!='play' or r['event']!='flight_sample':continue
  f=dict(x.split('=',1) for x in r['detail'].split(';') if '=' in x)
  if float(f['z'])<1.3:track.append(r)
 last=-100
 for i,r in enumerate(track):
  if float(r['time'])-last<.09:continue
  last=float(r['time']);frame=min(frames,key=lambda f:abs(float(f['game_time'])-last));shutil.copy2(root/'frames'/f"{int(frame['frame']):06d}.jpg",out/f"{int(task):02d}-{r['kind']}-{i:03d}.jpg")
print(len(list(out.glob('*.jpg'))),'hand-zone screenshots')
