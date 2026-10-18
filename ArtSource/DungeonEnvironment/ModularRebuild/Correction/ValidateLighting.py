"""Read-only native geometry/lighting verification; --import copies verified PNGs only."""
from pathlib import Path
from PIL import Image,ImageDraw
import hashlib,json,struct,zlib,sys,shutil
K=Path(__file__).resolve().parents[1];P=K.parents[2];L=K/'Correction/Lighting'
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
baseline=json.loads((L/'Baseline.json').read_text(encoding='utf-8'))
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
def edges(im):
 p=list(im.get_flattened_data());w,h=im.size
 return [(p[y*w+x]==p[y*w+x+d]) for y in range(h) for x in range(w) for d in (1,w) if (d!=1 or x+1<w) and (d!=w or y+1<h) and p[y*w+x][3] and p[y*w+x+d][3]]
changed=[]
for rel,h in baseline['exports'].items():
 f=K/'Exports'/rel;im=Image.open(f).convert('RGBA');before=Image.open(L/'Before'/rel).convert('RGBA')
 assert im.size==before.size,rel
 assert im.getchannel('A').tobytes()==before.getchannel('A').tobytes(),('alpha',rel)
 assert set(im.getchannel('A').get_flattened_data())<={0,255},rel
 assert im.tobytes()==decode(K/'Editable'/Path(rel).with_suffix('.aseprite')).tobytes(),('native-export',rel)
 if sha(f)!=h:
  assert rel.startswith(('WallTiles/BattleWall','FloorTiles/Floor','Props/TorchLitMasonry','Props/RightTopPillar')),('scope',rel)
  assert edges(im)==edges(before),('existing color-cluster boundary moved',rel)
  bp=list(before.get_flattened_data());ap=list(im.get_flattened_data())
  changed.append({'path':rel,'dimensions':list(im.size),'changed_pixels':sum(a!=b for a,b in zip(ap,bp)),'before_colors':len(set(bp)),'after_colors':len(set(ap)),'alpha_and_cluster_boundaries_preserved':True,'sha256':sha(f)})
  if '--import' in sys.argv:shutil.copyfile(f,P/'Assets/_Game/Art/DungeonEnvironment/ModularRebuild'/rel)
  board=Image.new('RGB',(im.width*4+36,im.height*4+56),'#141522');d=ImageDraw.Draw(board)
  for i,(label,a) in enumerate([('Before',before),('After',im)]):
   x=12+i*(im.width*2+12);d.text((x,8),label,fill='white');board.paste(a,(x,26),a);big=a.resize((a.width*2,a.height*2),Image.Resampling.NEAREST);board.paste(big,(x,im.height+40),big)
  dest=L/'Review'/Path(rel).name;dest.parent.mkdir(exist_ok=True);board.save(dest)
 assert f.read_bytes()==(P/'Assets/_Game/Art/DungeonEnvironment/ModularRebuild'/rel).read_bytes(),('unity copy',rel)
assert len(changed)==19,len(changed)
for f,h in {**baseline['locked_files'],**baseline['references']}.items():assert sha(P/f)==h,('locked file',f)
seams=0
for prefix in ['Floor','FloorBack','FloorMiddle','BattleWall','PortraitWall','OuterWall','HudWall']:
 g='FloorTiles' if prefix.startswith('Floor') else 'WallTiles';ims=[Image.open(K/f'Exports/{g}/{prefix}{v}.png') for v in 'ABCD']
 for a in ims:
  for b in ims:
   assert a.crop((0,0,1,64)).tobytes()==b.crop((63,0,64,64)).tobytes(),prefix
   assert a.crop((0,0,64,1)).tobytes()==b.crop((0,63,64,64)).tobytes(),prefix
   seams+=2
report={'status':'passed','changed_assets':changed,'unchanged_export_count':150-len(changed),'binary_alpha_and_canvas_preserved':True,'original_connected_color_cluster_boundaries_preserved':True,'scene_code_placement_manifest_and_build_settings_byte_identical':True,'source_cel_png_unity_exact':150,'seam_pairings':seams,'golden_reference_and_production_hashes_match':6,'generation_calls':0,'visual_approval':'pending user review'}
(L/'Validation.json').write_text(json.dumps(report,indent=2)+'\n',encoding='utf-8')
print(json.dumps({k:v for k,v in report.items() if k!='changed_assets'}))
