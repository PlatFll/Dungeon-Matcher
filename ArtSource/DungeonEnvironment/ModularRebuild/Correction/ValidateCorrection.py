"""Read-only asset inspection and derived review boards; never writes production pixels."""
from pathlib import Path
from PIL import Image,ImageDraw
import hashlib,json,struct,zlib,html
K=Path(__file__).resolve().parents[1];P=K.parents[2];R=K/'Correction';V=R/'Review'
V.mkdir(exist_ok=True)
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def decode(path):
 b=path.read_bytes();w,h=struct.unpack_from('<HH',b,8);i=144
 while i<len(b):
  size,kind=struct.unpack_from('<IH',b,i);s=i+6
  if kind==0x2005:
   x,y=struct.unpack_from('<hh',b,s+2);ctype=struct.unpack_from('<H',b,s+7)[0];cw,ch=struct.unpack_from('<HH',b,s+16);raw=b[s+20:i+size]
   if ctype==2:raw=zlib.decompress(raw)
   out=Image.new('RGBA',(w,h));out.paste(Image.frombytes('RGBA',(cw,ch),raw),(x,y));return out
  i+=size
 raise ValueError('Missing cel '+str(path))
manifest=json.loads((K/'AssetManifest.json').read_text());records=[];groups={};articles=[]
for rec in manifest['modules']:
 g,n=rec['group'],rec['name'];file=K/f'Exports/{g}/{n}.png';im=Image.open(file).convert('RGBA');ase=decode(K/f'Editable/{g}/{n}.aseprite')
 assert im.size==(rec['width'],rec['height']),n
 assert im.tobytes()==ase.tobytes(),('ASE/export',n)
 assert set(im.getchannel('A').getdata())<={0,255},n
 beforefile=R/f'Before/{g}/{n}.png';before=Image.open(beforefile).convert('RGBA') if beforefile.exists() else None
 changed=before is None or im.tobytes()!=before.tobytes();colors=len({p[:3] for p in im.getdata() if p[3]})
 record=dict(rec,sha256=sha(file),opaque_colors=colors,alpha=sorted(set(im.getchannel('A').getdata())),bounds=im.getbbox(),ase_png_exact=True,change_status='new' if before is None else 'changed' if changed else 'retained',visual_approval='pending user review')
 records.append(record);groups.setdefault(g,[]).append((n,im))
 width=max(220,im.width*3+30);height=max(im.height*4+85,140);board=Image.new('RGB',(width*2,height),'#252331');d=ImageDraw.Draw(board)
 for idx,(label,img) in enumerate([('Rejected PR 200 module',before),('Corrected native module',im)]):
  x=idx*width+10;d.text((x,8),n,fill='white');d.text((x,24),label,fill='#bdb4d2')
  if img:
   board.paste(img,(x,48),img);big=img.resize((img.width*3,img.height*3),Image.Resampling.NEAREST);board.paste(big,(x,im.height+66),big)
  else:d.text((x,58),'New separate sprite',fill='white')
 dest=V/'Assets'/g;dest.mkdir(parents=True,exist_ok=True);board.save(dest/f'{n}.png')
 articles.append(f'<article><h3>{g} / {n}</h3><p>{record["change_status"]}; {im.width}×{im.height}; {colors} opaque colors; binary alpha; native cel/export exact.</p><p>{html.escape(rec["changes"])}</p><img src="../Correction/Review/Assets/{g}/{n}.png"></article>')
 assert (P/f'Assets/_Game/Art/DungeonEnvironment/ModularRebuild/{g}/{n}.png').read_bytes()==file.read_bytes(),('Unity copy',n)
for g,items in groups.items():
 positions=[];x=12;y=30;row=0
 for n,im in items:
  cw=max(220,im.width*3+20);ch=im.height*3+40
  if x+cw>1200:x=12;y+=row;row=0
  positions.append((n,im,x,y));x+=cw;row=max(row,ch)
 board=Image.new('RGB',(1200,y+row+12),'#252331');d=ImageDraw.Draw(board)
 for n,im,x,y in positions:
  d.text((x,y),n,fill='white');big=im.resize((im.width*3,im.height*3),Image.Resampling.NEAREST);board.paste(big,(x,y+20),big)
 board.save(V/f'{g}.png')
seams={}
for prefix in ['Floor','FloorBack','FloorMiddle','PortraitWall','BattleWall','OuterWall','HudWall']:
 g='FloorTiles' if prefix.startswith('Floor') else 'WallTiles';tiles=[Image.open(K/f'Exports/{g}/{prefix}{v}.png').convert('RGBA') for v in 'ABCD']
 assert len({im.tobytes() for im in tiles})==4,prefix
 for a in tiles:
  assert a.size==(64,64)
  for b in tiles:
   assert a.crop((0,0,1,64)).tobytes()==b.crop((63,0,64,64)).tobytes(),prefix
   assert a.crop((0,0,64,1)).tobytes()==b.crop((0,63,64,64)).tobytes(),prefix
 grid=Image.new('RGBA',(256,256))
 for y in range(4):
  for x in range(4):grid.paste(tiles[(x+y*3)%4],(x*64,y*64))
 grid.save(V/f'{prefix}-repeat.png');seams[prefix]={'horizontal_pairs':16,'vertical_pairs':16,'distinct_variants':4}
goldens={'Docs/ArtDirection/Audit2026-10/media/skeleton-still.png':'705c4c8379d56a262e287d256664db8d180a79917f4a84289bbb813730b9616c','Docs/ArtDirection/EnemyCohesion2026-10/farmer-still.png':'6189bc2cfe01bab84bafada24a7b129a3fd8c2225f629eb8580386f461109e2d','Docs/ArtDirection/EnemyCohesion2026-10/pan_villager-still.png':'e1be89f8231483df5db6739dd2cae7bef83e11aaa9b028e4cb1b60ea55aba0ca'}
for path,expected in goldens.items():assert sha(P/path)==expected,path
report={'assets':records,'seams':seams,'golden_hashes':goldens,'dimensions_alpha_native_source_export_unity_copy':'pass','generation_cost':json.loads((K/'PixelLabUsage.json').read_text())['actual_generations'],'artistic_approval':'Technical checks do not establish visual approval. See category comparisons and current Unity captures.'}
(R/'TechnicalChecks.json').write_text(json.dumps(report,indent=2)+'\n');(K/'LibraryManifest.json').write_text(json.dumps(report,indent=2)+'\n')
style='<style>body{background:#171821;color:#eee;font:16px system-ui;margin:24px}article{padding:16px;margin:20px 0;background:#252331}img{image-rendering:pixelated;max-width:100%}a{color:#cbb6ff}</style>'
(K/'Review/Assets.html').write_text('<!doctype html><meta charset="utf-8">'+style+'<h1>Every native module — actual before/after</h1><p>Technical checks and visual approval are separate. Retained assets are explicitly labeled; new artwork is not falsely presented as previously approved.</p>'+''.join(articles),encoding='utf-8')
print(json.dumps({'modules':len(records),'changed':sum(r['change_status']=='changed' for r in records),'new':sum(r['change_status']=='new' for r in records),'retained':sum(r['change_status']=='retained' for r in records),'native_export_unity_alpha':'pass','seam_pairings':len(seams)*32,'goldens_unchanged':True}))
