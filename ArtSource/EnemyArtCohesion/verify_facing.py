"""Read-only raster checks for the reviewed whole-cel direction correction.

The visual decision is recorded in facing_corrections.json. These checks prove
the selected operation and scope, not an automatic assessment of character gaze.
"""
from pathlib import Path
from PIL import Image
import hashlib, io, json, subprocess, sys

ROOT=Path(__file__).resolve().parents[2]; BASE=Path(__file__).resolve().parent
rules=json.loads((BASE/'facing_corrections.json').read_text())
production='--production' in sys.argv
jobs=json.loads((BASE/'jobs.json').read_text())
coverage=[]; changes=[]; selected_states=set(); unchanged=0
manifest=json.loads((BASE/'unity-import-manifest.json').read_text())
paths=[p for j in jobs for p in ('ArtSource/EnemyArtCohesion/Sheets/'+j['stem']+'.png',j['path']+'.meta')]
paths += [c['path'] for c in manifest['clips']]
refs=''.join(rules['baseline_commit']+':'+p+'\n' for p in paths).encode()
blobs=subprocess.check_output(['rtk','proxy','git','cat-file','--batch'],input=refs,cwd=ROOT)
baseline={};offset=0
for path in paths:
    end=blobs.index(b'\n',offset);header=blobs[offset:end].split()
    assert len(header)==3 and header[1]==b'blob',(path,header)
    size=int(header[2]);baseline[path]=blobs[end+1:end+1+size];offset=end+size+2
assert offset==len(blobs),'unexpected git batch output'

def git_bytes(path):
    return baseline[path]

for job in jobs:
    path='ArtSource/EnemyArtCohesion/Sheets/'+job['stem']+'.png'
    before_bytes=git_bytes(path)
    before=Image.open(io.BytesIO(before_bytes)).convert('RGBA')
    after_bytes=(ROOT/path).read_bytes();after=Image.open(io.BytesIO(after_bytes)).convert('RGBA')
    if production:assert (ROOT/job['path']).read_bytes()==after_bytes,(job['path'],'production/export mismatch')
    assert before.size==after.size,(path,'dimensions')
    state_rules=rules['actors'].get(job['actor'],{}).get('states',{})
    changed=[];w,h=job['size']
    for i,row in enumerate(job['sprites']):
        selected=[(s,n) for s,n in row['states'] if state_rules.get(s)=='all' or n in state_rules.get(s,[])]
        selected_states.update((job['actor'],s,n) for s,n in selected)
        a=before.crop((i*w,0,(i+1)*w,h));b=after.crop((i*w,0,(i+1)*w,h))
        expected=a.transpose(Image.Transpose.FLIP_LEFT_RIGHT) if selected else a
        assert b.tobytes()==expected.tobytes(),(path,i,'not the exact authorized reflection/unchanged cel')
        if selected:
            assert a.tobytes()!=b.tobytes(),(path,i,'selected symmetric/no-op cel')
            changed.append(dict(cell=i,states=row['states']))
        else:unchanged+=1
    # Same canvas, sprite registration, GUIDs and signed file IDs.
    meta=job['path']+'.meta'
    # Git stores LF while this Windows checkout materializes text as CRLF.
    assert (ROOT/meta).read_bytes().replace(b'\r\n',b'\n')==git_bytes(meta).replace(b'\r\n',b'\n'),(meta,'import metadata changed')
    if not changed:assert before_bytes==after_bytes,(path,'unselected sheet changed')
    if changed:changes.append(dict(actor=job['actor'],stem=job['stem'],path=job['path'],before_sha256=hashlib.sha256(before_bytes).hexdigest(),after_sha256=hashlib.sha256(after_bytes).hexdigest(),frames=changed))

audit=json.loads((ROOT/'Docs/ArtDirection/Audit2026-10/bindings.json').read_text())
for a in audit['actors']:
    if a['zone']=='Players':continue
    aj=[j for j in jobs if j['actor']==a['id']]
    coverage.append(dict(actor=a['id'],states=[s['name'] for s in a['states']],unique_cells=sum(len(j['sprites']) for j in aj),corrected_cells=sum(len(j['frames']) for j in changes if j['actor']==a['id'])))
for actor,rule in rules['actors'].items():
    for state,indices in rule['states'].items():
        selected=[n for a,s,n in selected_states if a==actor and s==state]
        assert selected,(actor,state,'rule matched no cell')
        if indices!='all':assert set(selected)==set(indices),(actor,state,'missing drawing')

for clip in manifest['clips']:
    assert (ROOT/clip['path']).read_bytes().replace(b'\r\n',b'\n')==git_bytes(clip['path']).replace(b'\r\n',b'\n'),(clip['path'],'clip changed')

report=dict(baseline_commit=rules['baseline_commit'],forms_reviewed=len(coverage),unique_cells_reviewed=sum(a['unique_cells'] for a in coverage),changed_sheets=len(changes),changed_cells=sum(len(j['frames']) for j in changes),unchanged_cells=unchanged,exact_horizontal_reflections=True,import_metadata_unchanged=True,unchanged_clips=len(manifest['clips']),visual_review_method='All unique source drawings inspected; selected east-facing cels reflected as complete poses. Frontal transitions and backward leans with a west-facing head are retained.',coverage=coverage,changes=changes)
report['production_pngs_match_exports']=True if production else None
report['text_comparison']='Git-normalized LF; Windows checkout CRLF is ignored for unchanged metadata and clips.'
(BASE/'facing-validation.json').write_text(json.dumps(report,indent=2)+'\n')
print(json.dumps({k:v for k,v in report.items() if k not in ('coverage','changes')}))
