import csv,pathlib,shutil,sys,html
root=pathlib.Path(sys.argv[1]);frames=list(csv.DictReader((root/'frames.csv').open()));rows=list(csv.DictReader(next(root.glob('A-*.csv')).open()));out=root/'reactions';out.mkdir(exist_ok=True)
def fields(r):return dict(x.split('=',1) for x in r['detail'].split(';') if '=' in x)
items=[]
def save(name,time):
    f=min(frames,key=lambda r:abs(float(r['game_time'])-time));file=f'{name}.jpg';shutil.copyfile(root/'frames'/f"{int(f['frame']):06d}.jpg",out/file);items.append((name,file,f['game_time']))
seen=set()
for r in rows:
    if r['event']=='contact' and r['phase']=='play' and r['task'] not in seen:
        seen.add(r['task']);base=f"{int(r['task']):02d}-{r['kind']}";at=float(r['time'])
        for suffix,delta in [('pre',-.2),('touch-before',-.06),('touch',0),('touch-after',.04),('view-impact',.12),('contact',.10),('break',.27),('depart',.55),('flight',1.15),('mid',2.0)]:save(base+'-'+suffix,at+delta)
        landed=next((e for e in rows if e['event']=='outfield_arrival' and fields(e)['kind']==r['kind'] and float(e['time'])>at),None)
        if landed:
            for suffix,delta in [('land',.10),('after',.43),('tail',.9),('sky',1.4),('sky-fade',2.0)]:save(base+'-'+suffix,float(landed['time'])+delta)
for i,e in enumerate(e for e in rows if e['event'] in ['trick_start','slow_start','recoil_cue','magic_fake_contact','magic_real_contact','results']):save(f'context-{i:02d}-{e["event"]}',float(e['time'])+.1)
# Dense chronological incoming frames expose a late kink that isolated hit screenshots miss.
for e in rows:
    if e['event']!='trick_start' or e['phase']!='play' or e['kind'] not in ('cake','poison','bomb','boomerang','magic','chest'):continue
    start=float(e['time']);end=next((float(r['time']) for r in rows if r['phase']=='play' and r['task']==e['task'] and r['event']=='resolved' and float(r['time'])>=start),start)
    for i in range(int((end-start)/.20)+1):save(f'incoming-{e["task"]}-{e["kind"]}-{i:03d}',start+i*.20)
# Enter/hold/exit frames make the time-state indicator reviewable.
for i,e in enumerate(x for x in rows if x['event'] in ['world_slow_enter','world_slow_exit'] and x['phase']=='play'):
    for j,dt in enumerate([-.08,.16,.65]):save(f'slow-{i:02d}-{e["event"]}-{j}',float(e['time'])+dt)
# Overlap sequences verify that the previous return/celebration does not mask the next pitch.
for e in rows:
    if e['event']!='resolved' or e['phase']!='play':continue
    at=float(e['time'])
    following=next((x for x in rows if x['event']=='slow_start' and x['phase']=='play' and float(x['time'])>at),None)
    if following:
        for i in range(17):save(f'overlap-{e["task"]}-{e["kind"]}-{i:02d}',at+.25*i)
body=''.join(f'<figure><a href="reactions/{f}"><img loading="lazy" src="reactions/{f}"></a><figcaption>{html.escape(n)} · {t}s</figcaption></figure>' for n,f,t in items)
(root/'review.html').write_text('<!doctype html><html lang="ja"><meta charset="utf-8"><meta name="viewport" content="width=device-width"><title>打撃反応の検証</title><style>body{max-width:1440px;margin:30px auto;background:#10202c;color:white;font:16px system-ui}main{display:grid;grid-template-columns:repeat(auto-fit,minmax(420px,1fr));gap:16px}figure{margin:0}img,video{width:100%}</style><h1>打撃反応と到達位置の検証</h1><p>実際の接触判定を使うPCデモ。画像は一人称録画の未加工フレーム。</p><video controls src="HitReactions.mp4"></video><main>'+body+'</main>')
print(len(items),'frames selected')
