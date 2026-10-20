from pathlib import Path
import json,hashlib
r=Path(__file__).resolve().parents[1]
d=json.loads((r/'Documentation/AssetAcquisition.json').read_text())
for p in d:
 for f in p['files']: assert hashlib.sha256((r/f['path']).read_bytes()).hexdigest()==f['sha256']
print(sum(len(p['files']) for p in d),'acquired files verified;',sum(not f['matches_reference'] for p in d for f in p['files']),'changed from supplied reference')
