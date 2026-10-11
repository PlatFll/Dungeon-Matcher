"""Install the user-approved PixelLab color edits, preserving Unity identities."""
from pathlib import Path
from PIL import Image
import hashlib, json, shutil

ROOT=Path(__file__).resolve().parents[1]
BASE=ROOT/'ArtSource/Frostwake'; REV=BASE/'face-revision'
def sha(path): return hashlib.sha256(path.read_bytes()).hexdigest()
def read(path): return json.loads(path.read_text(encoding='utf-8'))
def write(path,data): path.write_text(json.dumps(data,indent=2),encoding='utf-8')
approval=read(REV/'approval-status.json')
assert approval['allApproved'] is True, 'Requires explicit approval of the final corrections'
verified=read(REV/'verification.json'); sources=read(REV/'sources.json')
specs=read(BASE/'production/technical-checks.json')
expected={a['actor']:a for a in verified['actors']}
operations=[]; preserved={}; before=[]
for actor in sources:
    name=actor['actor']
    if name not in expected: continue
    folder=REV/'candidates'/name
    packed=Image.open(REV/(name+'-source.png')).convert('RGBA')
    guards={(p[0],p[1]):p for d in read(REV/(name+'-decisions.json')) for p in d['pixels']}
    for item in actor['items']:
        src=folder/('Still.png' if item['index']==0 else f"cels/{item['index']:02}.png")
        dst=ROOT/item['path']; original=Image.open(dst).convert('RGBA'); corrected=Image.open(src).convert('RGBA')
        assert original.size==corrected.size==(128,104)
        assert sha(dst)==item['sha256'], 'Source changed since review: '+str(dst)
        x,y=item['offset']; baseline=packed.crop((x,y,x+128,y+104))
        assert baseline.tobytes()==original.tobytes()
        for yy in range(104):
            for xx in range(128):
                a,b=original.getpixel((xx,yy)),corrected.getpixel((xx,yy))
                assert a[3]==b[3]
                if a!=b:
                    p=guards[x+xx,y+yy]
                    assert a==tuple(bytes.fromhex(p[2][1:])) and b==tuple(bytes.fromhex(p[3][1:]))
        if original.tobytes()!=corrected.tobytes():operations.append((src,dst))
        if item['index']==0:
            assert (REV/'before'/(name+'.png')).read_bytes()==dst.read_bytes()
            operations.append((src,BASE/'approved'/(name+'.png')))
            preserved[str(dst.relative_to(ROOT))+'.meta']=sha(dst.with_suffix('.png.meta'))
    for clip in expected[name]['clips']:
        state=clip['state']; src=folder/(state+'.png'); assert sha(src)==clip['sha256']
        production=BASE/'production'/name/(state+'.png')
        imported=ROOT/'Assets/_Game/Art/Frostwake/Motion'/(name+'_'+state+'.png')
        assert production.read_bytes()==imported.read_bytes()
        assert Image.open(src).size==Image.open(imported).size
        backup=REV/'before-motion'/name/(state+'.png')
        assert not backup.exists() or backup.read_bytes()==production.read_bytes()
        before.append((production,backup))
        operations.extend([(src,production),(src,imported)])
        preserved[str(imported.relative_to(ROOT))+'.meta']=sha(imported.with_suffix('.png.meta'))
for folder in ['Assets/_Game/Animations/Frostwake','Assets/_Game/Data/Frostwake']:
    for path in (ROOT/folder).rglob('*'):
        if path.is_file():preserved[path.relative_to(ROOT).as_posix()]=sha(path)
# Read every source and guard before the first mutation; retain baseline sheets.
for src,dst in before: dst.parent.mkdir(parents=True,exist_ok=True);shutil.copyfile(src,dst)
for src,dst in operations: shutil.copyfile(src,dst)
manifest=read(BASE/'approved/manifest.json')
for actor in manifest:
    name=actor['id']
    if name not in expected:continue
    still=BASE/'approved'/(name+'.png')
    actor['previous_approval']={k:actor[k] for k in ['approved_source','approved_sha256','approval','sha256','source_crop','offset']}
    actor.update(approved_source=f'ArtSource/Frostwake/face-revision/candidates/{name}/Still.png',
                 approved_sha256=sha(still),sha256=sha(still),source_crop=[0,0,128,104],offset=[0,0],
                 approval='User explicitly approved all final eye/skin/hair corrections and game integration, 2026-10-11')
    colors=sorted({p for p in Image.open(still).convert('RGBA').get_flattened_data() if p[3]})
    palette=Image.new('RGBA',(len(colors),1));palette.putdata(colors);palette.save(BASE/'approved'/(name+'-palette.png'))
write(BASE/'approved/manifest.json',manifest)
palette_checks=read(BASE/'motion-palette-checks.json')
changed_paths={dst.resolve() for src,dst in operations}
for item in palette_checks:
    path=BASE/item['path']
    if path.resolve() not in changed_paths:continue
    im=Image.open(path).convert('RGBA');item['sha256']=sha(path)
    item['colors']=len({c for c in im.get_flattened_data() if c[3]})
write(BASE/'motion-palette-checks.json',palette_checks)
assert all(sha(ROOT/p)==h for p,h in preserved.items())
write(REV/'integration.json',dict(approval=approval['source'],baseCommit='c2869ddc09f6b7a021b7dbd492ba683303a651b1',
    unityStills=7,unitySheets=43,sourceCopies=len(operations),preservedBindings=preserved,
    outputs=[dict(path=dst.relative_to(ROOT).as_posix(),sha256=sha(dst)) for src,dst in operations]))
print('Installed 7 approved stills and 43 sheets; native source cels updated; all GUIDs, clips and controllers preserved.')
