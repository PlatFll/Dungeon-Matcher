"""Check native artifacts and additive Unity references without changing pixels."""
from pathlib import Path
import hashlib,json,re,subprocess
from PIL import Image

ROOT=Path(__file__).resolve().parents[1]
source=ROOT/'ArtSource/Ironvein'
checked=0
stills=json.loads((source/'Production/selected-technical.json').read_text(encoding='utf-8'))
for entry in stills:
    path=ROOT/entry['file'];im=Image.open(path).convert('RGBA')
    assert hashlib.sha256(path.read_bytes()).hexdigest()==entry['sha256'],path
    assert list(im.size)==entry['canvas'],path
    assert sorted(set(im.getchannel('A').get_flattened_data()))==entry['alpha'],path
    assert len({p[:3] for p in im.get_flattened_data() if p[3]})==entry['opaque_colors'],path
    assert entry['source_mask_equal'],path
    checked+=1
clips=json.loads((source/'Production/motion-technical.json').read_text(encoding='utf-8'))
for clip in clips:
    path=source/'Motion'/clip['name']/(clip['state']+'.png')
    assert hashlib.sha256(path.read_bytes()).hexdigest()==clip['sheet_sha256'],path
    assert clip['scale']==1 and clip['alpha']==[0,255],path
    assert len(clip['source_sha256'])==clip['frameCount'],path
    checked+=1

# Resolve GUIDs from actual .meta files, including pre-existing shared references.
guids={}
for path in list((ROOT/'Assets').rglob('*.meta'))+list((ROOT/'Library/PackageCache').rglob('*.meta')):
    found=re.search(r'^guid: ([a-f0-9]{32})$',path.read_text(encoding='utf-8'),re.M)
    if found:guids.setdefault(found[1],[]).append(path)
duplicates={g:[str(p.relative_to(ROOT)) for p in paths] for g,paths in guids.items() if len(paths)>1 and any('Ironvein' in str(p) for p in paths)}
assert not duplicates,duplicates
files=subprocess.check_output(['git','diff','--name-only','a2bcf86e7d8d92707ac3fa857e72305c2f115824'],cwd=ROOT,text=True).splitlines()
files+=subprocess.check_output(['git','ls-files','--others','--exclude-standard','Assets/_Game'],cwd=ROOT,text=True).splitlines()
refs=0
for name in set(files):
    path=ROOT/name
    if not path.exists() or path.suffix not in {'.asset','.prefab','.controller','.anim'} or 'Ironvein' not in name and 'ironvein-excavation' not in name:continue
    for guid in re.findall(r'guid: ([a-f0-9]{32})',path.read_text(encoding='utf-8')):
        if guid.startswith('0000000000000000'):continue
        assert guid in guids,(name,guid)
        refs+=1
report=dict(stills=len(stills),clips=len(clips),hashes_checked=checked,resolved_unity_guid_references=refs,
    duplicate_ironvein_guids=len(duplicates),status='PASS',limitations='Static source/reference checks; not user visual or device approval.')
out=ROOT/'.utmp/Ironvein/native-delivery-check.json';out.write_text(json.dumps(report,indent=2)+'\n',encoding='utf-8')
print(json.dumps(report))
