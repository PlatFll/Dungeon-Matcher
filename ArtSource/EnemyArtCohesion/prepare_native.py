"""Prepare immutable inputs and native LibreSprite edit jobs; no pixel mutation."""
from pathlib import Path
from PIL import Image
import json, hashlib, shutil, re

ROOT=Path(__file__).resolve().parents[2]
OUT=Path(__file__).resolve().parent
TEMP=ROOT/'.utmp/EnemyArtCohesion'
SOURCE=OUT/'Originals'
for p in (TEMP,SOURCE,OUT/'Native',OUT/'Sheets',OUT/'Candidates'):p.mkdir(parents=True,exist_ok=True)
data=json.loads((ROOT/'Docs/ArtDirection/Audit2026-10/bindings.json').read_text(encoding='utf-8'))
by_path={}
for actor in data['actors']:
    if actor['zone']=='Players':continue
    refs=[('Ready',0,actor['still_binding'],130)]
    refs += [(s['name'],i,f['sprite'],round(f['duration']*1000)) for s in actor['states'] for i,f in enumerate(s['frames'])]
    for state,index,sd,duration in refs:
        job=by_path.setdefault(sd['path'],dict(actor=actor['id'],path=sd['path'],sprites={}))
        assert job['actor']==actor['id'],sd['path']
        row=job['sprites'].setdefault(str(sd['fileID']),dict(source=sd,states=[],duration=duration))
        if row['states']==[['Ready',0]] and state!='Ready':row['duration']=duration
        if [state,index] not in row['states']:row['states'].append([state,index])
jobs=[]
for path,job in by_path.items():
    p=ROOT/path;sd=next(iter(job['sprites'].values()))['source']
    token=hashlib.sha256(path.encode()).hexdigest()[:10]
    stem=p.stem+'-'+token
    original=SOURCE/(stem+'.png')
    if not original.exists():
        assert hashlib.sha256(p.read_bytes()).hexdigest()==sd['sha256'],path
        shutil.copy2(p,original);shutil.copy2(p.with_name(p.name+'.meta'),SOURCE/(stem+'.png.meta'))
    assert hashlib.sha256(original.read_bytes()).hexdigest()==sd['sha256']
    sprites=sorted(job['sprites'].values(),key=lambda r:(r['source']['crop_top_left'][1],r['source']['crop_top_left'][0]))
    dims={tuple(r['source']['size']) for r in sprites};assert len(dims)==1,(path,dims)
    ow,oh=dims.pop();padding=[24,20,24,4]
    if job['actor']=='grand_delver_reserve' and 'Remount' in path:padding=[88,28,88,4]
    nw,nh=ow+padding[0]+padding[2],oh+padding[1]+padding[3]
    # Preserve every imported source cell, including unused cells, instead of
    # allowing an atlas rewrite to destroy a serialized sprite reference.
    meta=original.with_suffix('.png.meta').read_text(encoding='utf-8-sig')
    ids=re.findall(r'^      internalID: (-?\d+)',meta,re.M)
    if re.search(r'^  spriteMode: 2$',meta,re.M):
        assert set(ids)==set(job['sprites']),(path,'unbound imported rectangles require explicit review')
    jobs.append(dict(actor=job['actor'],path=path,stem=stem,original=original.relative_to(ROOT).as_posix(),
        source_sha256=sd['sha256'],old_size=[ow,oh],size=[nw,nh],padding=padding,sprites=sprites))
(OUT/'jobs.json').write_text(json.dumps(jobs,indent=2),encoding='utf-8')
body=(OUT/'native_edit.js').read_text(encoding='utf-8')
rules=json.loads((OUT/'material_rules.json').read_text(encoding='utf-8'))
overrides=json.loads((OUT/'frame_overrides.json').read_text(encoding='utf-8'))
focal={str(r['states'][0][1]):r for r in json.loads((OUT/'focal-registration.json').read_text())}
facing=json.loads((OUT/'facing_corrections.json').read_text(encoding='utf-8'))['actors']
selected=jobs[:1]
import sys
if '--all' in sys.argv:selected=jobs
elif '--actors' in sys.argv:
    names=sys.argv[sys.argv.index('--actors')+1].split(',');selected=[j for j in jobs if j['actor'] in names]
elif '--from-actor' in sys.argv:
    name=sys.argv[sys.argv.index('--from-actor')+1];start=next(i for i,j in enumerate(jobs) if j['actor']==name);selected=jobs[start:]
script='var ROOT='+json.dumps(ROOT.as_posix()+'/')+';\nvar RULES='+json.dumps(rules,separators=(',',':'))+';\nvar OVERRIDES='+json.dumps(overrides,separators=(',',':'))+';\nvar FOCAL='+json.dumps(focal,separators=(',',':'))+';\nvar JOBS='+json.dumps(selected,separators=(',',':'))+';\n'+body
script='var FACING='+json.dumps(facing,separators=(',',':'))+';\n'+script
(TEMP/'RunNative.js').write_text(script,encoding='utf-8')
(TEMP/'selected-jobs.json').write_text(json.dumps(selected),encoding='utf-8')
print(json.dumps(dict(total_jobs=len(jobs),selected=len(selected),script=str(TEMP/'RunNative.js'))))
