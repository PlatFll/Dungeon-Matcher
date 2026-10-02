"""Verify actual exports/import files and reconcile the recorded service usage."""
from pathlib import Path
import hashlib,json,re
from PIL import Image
import numpy as np
ROOT=Path(__file__).resolve().parents[1]
ART=ROOT/'ArtSource/Forest/Production';OUT=ART/'Selected'
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
ledger=json.loads((ART/'usage-reconciliation.json').read_text(encoding='utf-8'))
for job in ledger['jobs']:
    if job['result'].startswith('{'):
        result=json.loads(job['result'])
        job.update(jobId=result['job_id'],generations=result['estimated_generations'],status='completed',costWasEstimated=True)
assert sum(j['generations'] for j in ledger['jobs'])==ledger['actualUsed']==80
assert ledger['opening']['remaining']-ledger['closing']['remaining']==80
assert len({j['jobId'] for j in ledger['jobs'] if j['jobId']})==51
ledger['note']='Per-job reported/estimated costs reconcile to the actual 80-generation balance change. Four rate-limit refusals were not queued or charged.'
(ART/'usage-reconciliation.json').write_text(json.dumps(ledger,indent=2),encoding='utf-8')
clips=json.loads((OUT/'animation-manifest.json').read_text(encoding='utf-8'))['clips']
report={'clips':len(clips),'frames':0,'importedSheets':0,'paletteAndAlpha':True,'idleContacts':{},'loopSeams':[]}
for clip in clips:
    name,state=clip['name'],clip['state'];ref=Image.open(ROOT/f'ArtSource/Forest/Approved/{name}.png').convert('RGBA')
    a=np.array(ref);palette={tuple(c) for c in a[a[:,:,3]>0,:3]}
    for frame in clip['frames']:
        p=OUT/name/state/f"{frame['index']:02}.png";im=Image.open(p).convert('RGBA');a=np.array(im)
        assert im.size==(clip['width'],clip['height']) and sha(p)==frame['hash']
        assert set(np.unique(a[:,:,3])) <= {0,255}
        assert {tuple(c) for c in a[a[:,:,3]>0,:3]}<=palette
        report['frames']+=1
    if state=='Idle':
        rows=[f['bottomOccupiedRow'] for f in clip['frames']]
        assert set(rows)=={clip['height']-1};report['idleContacts'][name]=rows[0]
    if clip['loop']:
        duplicate=clip['frames'][0]['hash']==clip['frames'][-1]['hash']
        report['loopSeams'].append({'name':name,'state':state,'duplicateEndpoint':duplicate,
            'note':'Approved Phase 3 pilot retained with its documented endpoint hold.' if duplicate and name=='Elven_Mender' else 'Distinct endpoints.'})
    source=OUT/name/(state+'.png');imported=ROOT/f'Assets/_Game/Art/Forest/Production/{name}_{state}.png'
    assert sha(source)==sha(imported)
    meta=imported.with_suffix('.png.meta').read_text(encoding='utf-8')
    for text in ['filterMode: 0','enableMipMap: 0','spritePixelsToUnits: 64','textureCompression: 0','spriteMode: 2']:
        assert text in meta,(imported,text)
    report['importedSheets']+=1
for row in json.loads((OUT/'modular-manifest.json').read_text()):
    p=OUT/row['path'];assert sha(p)==row['hash'];assert Image.open(p).size==(row['width'],row['height'])
    if row['path'].startswith('UI/Log_Cell'):assert np.all(np.array(Image.open(p))[:,:,3]==255)
(ART/'technical-audit.json').write_text(json.dumps(report,indent=2),encoding='utf-8')
print(json.dumps({k:v for k,v in report.items() if k!='loopSeams'}))
