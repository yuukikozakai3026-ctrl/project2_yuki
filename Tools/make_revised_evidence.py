import csv,pathlib,shutil,sys,html
root=pathlib.Path(sys.argv[1]);frames=list(csv.DictReader((root/'frames.csv').open()));rows=list(csv.DictReader(next(root.glob('A-*.csv')).open()));out=root/'screenshots';out.mkdir(exist_ok=True)
names={'ball':'野球ボール','cake':'ケーキ','poison':'毒瓶','boomerang':'ブーメラン','magic':'魔法球','bomb':'爆弾','bullets':'銃弾3発','chest':'宝箱'}
def fields(r):return dict(s.split('=',1) for s in r['detail'].split(';') if '=' in s)
items=[]
def save(label,filename,time):
 f=min(frames,key=lambda x:abs(float(x['game_time'])-time));src=root/'frames'/f"{int(f['frame']):06d}.jpg";shutil.copyfile(src,out/filename);items.append((label,filename,f['game_time']))
for r in rows:
 if r['event']!='pitch_config':continue
 info=fields(r);kind=info['kind'];idx=int(r['task']);prefix=f'{idx:02d}-{kind}';events=[e for e in rows if e['task']==str(idx)]
 slow=next((e for e in events if e['event']=='slow_start'),None);contact=next((e for e in events if e['event']=='contact'),None)
 title=names[kind]+(' '+info['count']+'個' if kind=='magic' else '')
 if slow:
  save(title+' / スロー開始',prefix+'-slow.jpg',float(slow['time'])+.2)
  save(title+' / 飛行中',prefix+'-motion.jpg',float(slow['time'])+float(info['slow_seconds'])*.46)
 if contact:
  save(title+' / 接触直前',prefix+'-before.jpg',float(contact['time'])-.10)
  save(title+' / 打撃直後',prefix+'-hit.jpg',float(contact['time'])+.14)
for n,e in enumerate(e for e in rows if e['event'] in ['recoil_cue','magic_fake_contact','magic_real_contact','bomb_far_explosion','results']):
 save(e['kind']+' / '+e['event'],f'event-{n:02d}-{e["event"]}.jpg',float(e['time'])+.18)
body=''.join(f'<figure><a href="screenshots/{fn}"><img loading="lazy" src="screenshots/{fn}"></a><figcaption>{html.escape(label)} · {time}s</figcaption></figure>' for label,fn,time in items)
(root/'review.html').write_text('''<!doctype html><html lang="ja"><meta charset="utf-8"><meta name="viewport" content="width=device-width"><title>改訂A案の動作確認</title><style>body{max-width:1400px;margin:32px auto;padding:0 24px;background:#0b1524;color:#e9eef7;font:16px system-ui}h1{font-size:26px}video{width:100%;max-height:72vh;background:#000}main{display:grid;grid-template-columns:repeat(auto-fit,minmax(320px,1fr));gap:18px}figure{margin:0}img{width:100%;border-radius:8px}figcaption{padding:8px 0;color:#c4d1df}a{color:#96d9e8}</style><h1>改訂A案 — 8種の飛来と打撃</h1><p>PC自動デモの通常速度映像。実際のバット接触判定を使用。VR実機の到達範囲・装着感は未確認。</p><video controls preload="metadata" src="RevisedA_Full.mp4"></video><h2>連続画像</h2><main>'''+body+'</main></html>')
print(f'{len(items)} screenshots: {out}')
