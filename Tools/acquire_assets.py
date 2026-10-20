from pathlib import Path
import urllib.request,re,json,hashlib,zipfile,datetime,concurrent.futures
root=Path(__file__).resolve().parents[1]
manifest=json.loads((root/'Documentation/assets/asset_manifest.json').read_text())
def acquire(pack):
    page=urllib.request.urlopen(pack['official_url']).read().decode()
    url=re.search(r"href=['\"]([^'\"]+\.zip)['\"]",page)[1]
    data=urllib.request.urlopen(url).read(); archive=root/'SourceArchives'/f"{pack['slug']}.zip";archive.parent.mkdir(exist_ok=True);archive.write_bytes(data)
    z=zipfile.ZipFile(archive); dest=root/'Assets/Kenney'/pack['slug']; dest.mkdir(parents=True,exist_ok=True)
    rows=[]
    for f in pack['files']:
        p=f['path_in_archive']; raw=z.read(p); out=dest/p;out.parent.mkdir(parents=True,exist_ok=True);out.write_bytes(raw)
        rows.append(dict(source=p,path=str(out.relative_to(root)),sha256=hashlib.sha256(raw).hexdigest(),matches_reference=hashlib.sha256(raw).hexdigest()==f['sha256'],use=f['use'],status='downloaded'))
    lic=next(p for p in z.namelist() if p.endswith('License.txt')); (dest/'License.txt').write_bytes(z.read(lic))
    assert 'CC0' in z.read(lic).decode()
    print(pack['slug'],len(rows),'selected files',flush=True)
    return dict(slug=pack['slug'],official_url=pack['official_url'],retrieved_utc=datetime.datetime.now(datetime.timezone.utc).isoformat(),download_url=url,archive_sha256=hashlib.sha256(data).hexdigest(),license='CC0',files=rows)
with concurrent.futures.ThreadPoolExecutor(max_workers=4) as pool: records=list(pool.map(acquire,manifest['packs']))
(root/'Documentation/AssetAcquisition.json').write_text(json.dumps(records,ensure_ascii=False,indent=2))
font=root/'Assets/Fonts';font.mkdir(exist_ok=True)
for name in ['NotoSansJP[wght].ttf','OFL.txt']:
    url='https://raw.githubusercontent.com/google/fonts/main/ofl/notosansjp/'+urllib.parse.quote(name)
    raw=urllib.request.urlopen(url).read();(font/name).write_bytes(raw)
print('Font acquired',flush=True)
