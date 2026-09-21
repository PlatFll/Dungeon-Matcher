"""Refresh production fingerprints; refuse to change captured-source hashes."""
from pathlib import Path
import json,hashlib
root=Path(__file__).resolve().parents[1]
def fingerprint(p):
    data=p.read_bytes()
    return {'sha256':hashlib.sha256(data).hexdigest(),'bytes':len(data)}
def write_manifest(base,files):
    path=base/'Manifest.json'
    old=json.loads(path.read_text()) if path.exists() else {}
    for name,entry in old.items():
        assert fingerprint(base/name)==entry,('Preserved source changed',name)
    current={p.relative_to(base).as_posix():fingerprint(p) for p in sorted(files)}
    path.write_text(json.dumps(current,indent=2)+'\n')
captured=root/'Originals/IdleRefinement'
write_manifest(captured,[p for p in captured.iterdir() if p.suffix in ('.aseprite','.png')])
path=root/'AssetManifest.json'
entries=json.loads(path.read_text())
for name in entries:
    entries[name]=fingerprint(root/name)
entries['Scripts/refinement_materials.json']=fingerprint(root/'Scripts/refinement_materials.json')
path.write_text(json.dumps(dict(sorted(entries.items())),indent=2)+'\n')
originals=json.loads((root/'Originals/Manifest.json').read_text())
assert all(fingerprint(root/'Originals'/name)==entry for name,entry in originals.items())
print('Fingerprints refreshed; earlier supplied originals remain byte-identical.')
