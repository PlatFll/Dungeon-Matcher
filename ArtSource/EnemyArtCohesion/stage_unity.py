"""Stage LibreSprite exports and update imported rectangles, preserving Unity IDs.

Pixels are copied unchanged from native exports. This script only edits metadata.
Run only after visual acceptance of the complete candidate set.
"""
from pathlib import Path
from PIL import Image
import json,re,hashlib,shutil,sys
ROOT=Path(__file__).resolve().parents[2];BASE=Path(__file__).resolve().parent
jobs=json.loads((BASE/'jobs.json').read_text())
audit=json.loads((ROOT/'Docs/ArtDirection/Audit2026-10/bindings.json').read_text(encoding='utf-8'))
ledger=BASE/'staged-hashes.json'
staged=json.loads(ledger.read_text()) if ledger.exists() else {}
manifest={'textures':[],'clips':[],'actors':[]}
for j in jobs:
 source=BASE/'Sheets'/(j['stem']+'.png');target=ROOT/j['path'];w,h=j['size'];ground=j['padding'][3]
 assert source.exists(),source
 with Image.open(source) as raw:im=raw.convert('RGBA')
 assert im.size==(w*len(j['sprites']),h),(source,im.size)
 assert max(im.size)<=4096,(source,'atlas exceeds import limit')
 meta=Path(ROOT/j['original']).with_suffix('.png.meta').read_text(encoding='utf-8-sig')
 mode=int(re.search(r'^  spriteMode: (\d+)',meta,re.M)[1])
 sprites=[]
 for i,r in enumerate(j['sprites']):
  b=im.crop((i*w,0,(i+1)*w,h)).getbbox()
  sprites.append(dict(fileID=str(r['source']['fileID']),name=r['source'].get('name',''),x=i*w,y=0,bounds=[b[0],h-b[3],b[2]-b[0],b[3]-b[1]] if b else [0,0,0,0]))
 manifest['textures'].append(dict(actor=j['actor'],path=j['path'],sha256=hashlib.sha256(source.read_bytes()).hexdigest(),width=w,height=h,ground=ground,sprites=sprites))
 meta=re.sub(r'(?m)^  alignment: \d+$','  alignment: 9',meta)
 meta=re.sub(r'(?m)^  spritePivot: .*$',f'  spritePivot: {{x: 0.5, y: {ground/h:.10f}}}',meta)
 meta=re.sub(r'(?m)^(\s+maxTextureSize: )\d+$',r'\g<1>4096',meta)
 if mode==2:
  by_id={str(r['source']['fileID']):i for i,r in enumerate(j['sprites'])}
  head,tail=meta.split('  spriteSheet:',1)
  def update(m):
   block=m.group(0);ident=re.search(r'^      internalID: (-?\d+)$',block,re.M)
   assert ident and ident[1] in by_id,(j['path'],block)
   index=by_id[ident[1]]
   for key,value in [('x',index*w),('y',0),('width',w),('height',h)]:
    block,n=re.subn(r'(?m)^        '+key+r': [^\n]+$',f'        {key}: {value}',block,count=1);assert n==1,(j['path'],key)
   block,n=re.subn(r'(?m)^      alignment: \d+$','      alignment: 9',block,count=1);assert n==1
   block,n=re.subn(r'(?m)^      pivot: .*$',f'      pivot: {{x: 0.5, y: {ground/h:.10f}}}',block,count=1);assert n==1
   return block
  tail,n=re.subn(r'(?ms)^    - serializedVersion: 2\n.*?(?=^    - serializedVersion: 2\n|^    outline:|^    spriteID:|\Z)',update,tail)
  assert n==len(by_id),(j['path'],n,len(by_id))
  meta=head+'  spriteSheet:'+tail
 if '--apply' in sys.argv:
  # Refuse to overwrite intervening production art edits on an initial stage.
  current=hashlib.sha256(target.read_bytes()).hexdigest()
  assert current in (j['source_sha256'],staged.get(j['path']),hashlib.sha256(source.read_bytes()).hexdigest()),('intervening artwork edit',target)
  if current!=hashlib.sha256(source.read_bytes()).hexdigest():shutil.copyfile(source,target)
  metadata=target.with_suffix('.png.meta')
  if metadata.read_text(encoding='utf-8-sig')!=meta:metadata.write_text(meta,encoding='utf-8',newline='\n')
  staged[j['path']]=hashlib.sha256(source.read_bytes()).hexdigest()
  ledger.write_text(json.dumps(staged,indent=2),encoding='utf-8')
for actor in audit['actors']:
 if actor['zone']=='Players':continue
 manifest['actors'].append(dict(id=actor['id'],definition=actor['definition'],controller=actor['controller'],states=[s['name'] for s in actor['states']]))
 for state in actor['states']:
  path=state.get('clip') or state.get('clip_path')
  if path and path not in [x['path'] for x in manifest['clips']]:manifest['clips'].append(dict(path=path,sha256=audit['source_hashes'][path]))
(BASE/'unity-import-manifest.json').write_text(json.dumps(manifest,indent=2),encoding='utf-8')
print(json.dumps(dict(mode='APPLIED' if '--apply' in sys.argv else 'dry run',textures=len(jobs),sprites=sum(len(j['sprites']) for j in jobs),clips=len(manifest['clips']),actors=len(manifest['actors']))))
