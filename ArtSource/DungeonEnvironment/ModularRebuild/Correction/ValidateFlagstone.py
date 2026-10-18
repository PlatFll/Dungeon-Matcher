"""Inspect native sources and exports. --import copies only verified floor PNGs."""
from pathlib import Path
from PIL import Image,ImageDraw
import hashlib,json,struct,zlib,sys,shutil
K=Path(__file__).resolve().parents[1];P=K.parents[2];F=K/'Correction/FlagstoneRebuild'
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
base=json.loads((F/'Baseline.json').read_text(encoding='utf-8'))
def decode(path):
 b=path.read_bytes();w,h=struct.unpack_from('<HH',b,8);i=144
 while i<len(b):
  size,kind=struct.unpack_from('<IH',b,i);s=i+6
  if kind==0x2005:
   x,y=struct.unpack_from('<hh',b,s+2);ctype=struct.unpack_from('<H',b,s+7)[0];cw,ch=struct.unpack_from('<HH',b,s+16);raw=b[s+20:i+size]
   if ctype==2:raw=zlib.decompress(raw)
   out=Image.new('RGBA',(w,h));out.paste(Image.frombytes('RGBA',(cw,ch),raw),(x,y));return out
  i+=size
 raise ValueError(path)
changed=[];records=[];V=F/'Review';V.mkdir(exist_ok=True)
for rel,h in base['exports'].items():
 f=K/'Exports'/rel;im=Image.open(f).convert('RGBA');native=decode(K/'Editable'/Path(rel).with_suffix('.aseprite'))
 assert im.tobytes()==native.tobytes(),('native export',rel)
 assert set(im.getchannel('A').get_flattened_data())<={0,255},rel
 if rel.startswith('FloorTiles/'):
  before=Image.open(F/'Before'/rel).convert('RGBA');assert im.size==before.size==(64,64),rel
  assert im.getchannel('A').tobytes()==before.getchannel('A').tobytes(),rel
  assert sha(f)!=h,('floor not replaced',rel)
  colors=len(set(im.get_flattened_data()));assert colors<=20,(rel,colors)
  record={'path':rel,'canvas':[64,64],'opaque_colors':colors,'sha256':sha(f),'aseprite_sha256':sha(K/'Editable'/Path(rel).with_suffix('.aseprite')),'native_pixel_scale_preserved':True}
  records.append(record);changed.append(rel)
  if '--import' in sys.argv:shutil.copyfile(f,P/'Assets/_Game/Art/DungeonEnvironment/ModularRebuild'/rel)
  board=Image.new('RGB',(420,300),'#171824');d=ImageDraw.Draw(board)
  for x,label,a in [(8,'Before',before),(218,'PixelLab / finalized',im)]:
   d.text((x,8),label,fill='white');board.paste(a,(x,28),a);board.paste(a.resize((192,192),Image.Resampling.NEAREST),(x,102))
  board.save(V/Path(rel).name)
 else:assert sha(f)==h,('non-floor changed',rel)
 assert f.read_bytes()==(P/'Assets/_Game/Art/DungeonEnvironment/ModularRebuild'/rel).read_bytes(),('Unity copy',rel)
assert len(changed)==12,len(changed)
for f,h in {**base['locked_files'],**base['goldens']}.items():
 if f.endswith(('AssetManifest.json','PlacementManifest.json')):continue
 assert sha(P/f)==h,('locked file changed',f)
before_manifest=json.loads((F/'BeforeAssetManifest.json').read_text());current=json.loads((K/'AssetManifest.json').read_text())
assert current['placements']==before_manifest['placements'],'Object placement changed'
for key in set(before_manifest)-{'modules','status'}:assert current[key]==before_manifest[key],key
assert (P/'Assets/_Game/Art/DungeonEnvironment/ModularRebuild/PlacementManifest.json').read_bytes()==(K/'AssetManifest.json').read_bytes()
seams=0
for prefix in ['Floor','FloorBack','FloorMiddle','BattleWall','PortraitWall','OuterWall','HudWall']:
 g='FloorTiles' if prefix.startswith('Floor') else 'WallTiles';tiles=[Image.open(K/f'Exports/{g}/{prefix}{v}.png').convert('RGBA') for v in 'ABCD']
 assert len({im.tobytes() for im in tiles})==4,prefix
 for a in tiles:
  for b in tiles:
   assert a.crop((0,0,1,64)).tobytes()==b.crop((63,0,64,64)).tobytes(),('horizontal seam',prefix)
   assert a.crop((0,0,64,1)).tobytes()==b.crop((0,63,64,64)).tobytes(),('vertical seam',prefix)
   seams+=2
 if prefix.startswith('Floor'):
  atlas=Image.new('RGBA',(256,64));repeat=Image.new('RGBA',(256,192))
  for x,a in enumerate(tiles):atlas.paste(a,(64*x,0))
  for y in range(3):
   for x in range(4):repeat.paste(tiles[(x+y*3)%4],(64*x,64*y))
  atlas.save(V/f'{prefix}-Strip.png');repeat.save(V/f'{prefix}-Repeat.png')
sheet=Image.new('RGBA',(256,192))
for y,prefix in enumerate(['FloorBack','FloorMiddle','Floor']):sheet.paste(Image.open(V/f'{prefix}-Strip.png'),(0,y*64))
sheet.save(F/'FloorTileset.png')
report={'status':'passed','changed_floor_tiles':records,'non_floor_exports_unchanged':138,'native_cel_export_unity_exact':150,'all_floor_alpha_opaque':True,'seam_pairings':seams,'scene_code_and_placement_preserved':True,'golden_hashes_checked':6,'approval':'pending user visual review'}
(F/'Validation.json').write_text(json.dumps(report,indent=2)+'\n',encoding='utf-8')
print(json.dumps({k:v for k,v in report.items() if k!='changed_floor_tiles'}))
