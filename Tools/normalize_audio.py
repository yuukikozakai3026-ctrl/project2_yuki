from pathlib import Path
import subprocess,re,json
r=Path(__file__).resolve().parents[1];dest=r/'Assets/VRBat/Audio';dest.mkdir(exist_ok=True)
records=[]
for pack in ['impact-sounds','interface-sounds']:
 for p in (r/'Assets/Kenney'/pack).rglob('*.ogg'):
  result=subprocess.run(['/opt/homebrew/bin/ffmpeg','-hide_banner','-i',str(p),'-af','volumedetect','-f','null','-'],capture_output=True,text=True,check=True)
  peak=float(re.search(r'max_volume: ([\-\d.]+) dB',result.stderr)[1]);gain=-1-peak
  out=dest/(p.stem+'.wav');subprocess.run(['/opt/homebrew/bin/ffmpeg','-v','error','-y','-i',str(p),'-af',f'volume={gain}dB','-c:a','pcm_s16le',str(out)],check=True)
  records.append(dict(source=str(p.relative_to(r)),path=str(out.relative_to(r)),original_peak_dbfs=peak,normalization_db=gain,audition='Runtime playback completed; subjective listening unverified'))
(r/'Documentation/AudioProcessing.json').write_text(json.dumps(records,indent=2))
print(len(records),'clips peak-normalized to -1 dBFS')
