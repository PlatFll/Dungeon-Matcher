from pathlib import Path
from PIL import Image, ImageDraw
import hashlib,json,subprocess,uuid,shutil,re,sys
R=Path(sys.argv[1]); A=R/'ArtSource/StoneAbilityUI/MaterialRevision'
D=R/'Assets/_Game/Resources/UI/BattlePresentation/Stone'
pal=[tuple(bytes.fromhex(h)) for h in ['0a0d11','202129','343438','484646','595955','6f6d66','7b7871','8d867c','aaa397']]
def remap(im,fn):
 out=Image.new('RGBA',im.size);out.putdata([(*fn(p[:3]),255) if p[3] else (0,0,0,0) for p in im.get_flattened_data()]);return out
def stone(p):return min(pal,key=lambda c:sum((a-b)**2 for a,b in zip(c,p)))
def read(path):return Image.open(A/path).convert('RGBA')
def extend_horizontal(im,extra):
 # Add native copies of a quiet central column; never scale corner artwork.
 mid=im.width//2;out=Image.new('RGBA',(im.width+extra,im.height))
 out.paste(im.crop((0,0,mid,im.height)),(0,0))
 column=im.crop((mid,0,mid+1,im.height))
 for x in range(mid,mid+extra):out.paste(column,(x,0))
 out.paste(im.crop((mid,0,im.width,im.height)),(mid+extra,0));return out
def fit_modules(im,canvas,draw_size):
 im=im.crop(im.getbbox());im=extend_horizontal(im,draw_size[0]-im.width)
 im=extend_horizontal(im.transpose(Image.Transpose.TRANSPOSE),draw_size[1]-im.height).transpose(Image.Transpose.TRANSPOSE)
 out=Image.new('RGBA',canvas);out.paste(im,((canvas[0]-im.width)//2,(canvas[1]-im.height)//2));return out
prepared={};cleanup={}
for name,key,index,canvas,draw in [('AbilityPlate','Ability',0,(176,66),(170,58)),('WaveBody','Wave',0,(176,48),(172,44)),('Supply','Supply',6,(64,64),(60,60)),('Settings','Settings',3,(56,56),(48,48))]:
 src=read(f'Raw/{key}_{index:02}.png');im=fit_modules(remap(src,stone),canvas,draw)
 if name=='WaveBody':
  # Enclosed transparent label window becomes an opaque quiet charcoal recess.
  seen=set();todo=[(88,24)]
  while todo:
   x,y=todo.pop()
   if (x,y) in seen or not(0<=x<im.width and 0<=y<im.height) or im.getpixel((x,y))[3]:continue
   seen.add((x,y));todo.extend([(x-1,y),(x+1,y),(x,y-1),(x,y+1)])
  assert all(0<x<175 and 0<y<47 for x,y in seen),'Window must be enclosed'
  for p in seen:im.putpixel(p,(*pal[1],255))
 prepared[name]=im
 cleanup[name]={'source':f'Raw/{key}_{index:02}.png','source_bounds':src.getbbox(),'output_bounds':im.getbbox(),'method':'native central column/row repetitions, original corner shapes, no resampling; material palette mapping','palette_map':{'#%02x%02x%02x'%p:'#%02x%02x%02x'%stone(p) for p in {p[:3] for p in src.get_flattened_data() if p[3]}}}
prepared['AbilitySocket']=remap(prepared['AbilityPlate'],lambda c:pal[max(0,pal.index(c)-3)])
rgb=lambda h:tuple(bytes.fromhex(h))
tone={rgb(a):rgb(b) for a,b in [('262333','202129'),('3c374a','343438'),('524d61','484646'),('6f6b7e','6f6d66'),('a7a1b6','aaa397'),('c4bdd4','c4c2bc'),('e3daf5','e3e1db'),('f6f4ef','f6f4ef')]}
for name in ['CrownedSkull','Bull','Revolver','Flute']:
 for suffix in ['', 'Ready']:
  key=name+suffix;src=read('Inputs/'+key+'.png');im=remap(src,lambda c:tone[c])
  assert im.getchannel('A').tobytes()==src.getchannel('A').tobytes()
  prepared[key]=im;cleanup[key]={'source':'Inputs/'+key+'.png','method':'exact palette-only material mapping; alpha and all engraving shapes unchanged'}
for name,im in prepared.items():im.save(A/'Native'/f'{name}Prepared.png')
js='var root='+json.dumps(A.as_posix()+'/')+';\n'
for name in prepared:
 js+=f"app.open(root+'Native/{name}Prepared.png');var spr=app.activeSprite;var im=spr.layer(0).cel(0).image;var pc=app.pixelColor;\n"
 js+="for(var y=0;y<im.height;y++)for(var x=0;x<im.width;x++){var c=im.getPixel(x,y);if(pc.rgbaA(c)==0)im.putPixel(x,y,pc.rgba(0,0,0,0));}\n"
 js+=f"spr.layer(0).name='PixelLab carved stone: native modules and palette';spr.commit();spr.saveAs(root+'Native/{name}.ase',true);spr.saveAs(root+'Exports/{name}.png',true);app.activeDocument.close();app.open(root+'Native/{name}.ase');app.activeSprite.saveAs(root+'Roundtrip/{name}.png',true);app.activeDocument.close();\n"
js+="console.log('CARVED_STONE_ROUNDTRIP_COMPLETE');app.command.Exit();"
(A/'Scripts/FinishStone.js').write_text(js)
si=subprocess.STARTUPINFO();si.dwFlags|=subprocess.STARTF_USESHOWWINDOW;si.wShowWindow=0
run=subprocess.run([r'C:/Users/USER/Downloads/libresprite-development-windows-x86_64/libresprite.exe','--script',str(A/'Scripts/FinishStone.js')],startupinfo=si,stdout=subprocess.PIPE,stderr=subprocess.STDOUT,timeout=60)
(A/'QA/LibreSprite.txt').write_bytes(run.stdout);assert run.returncode==0 and b'CARVED_STONE_ROUNDTRIP_COMPLETE' in run.stdout,run.stdout
qa=[];template=(D/'AbilityPlate.png.meta').read_text()
for name,im in prepared.items():
 p=A/'Exports'/f'{name}.png';out=Image.open(p).convert('RGBA');check=read('Roundtrip/'+p.name)
 assert out.tobytes()==check.tobytes()==im.tobytes(),name
 assert set(out.getchannel('A').get_flattened_data())<={0,255},name
 shutil.copy2(p,D/p.name)
 if name=='WaveBody':
  meta=D/(p.name+'.meta');text=meta.read_text() if meta.exists() else re.sub(r'guid: \w+','guid: '+uuid.uuid4().hex,template,count=1)
  text=re.sub(r'spriteBorder: \{[^}]+\}','spriteBorder: {x: 20, y: 14, z: 20, w: 14}',text);meta.write_text(text)
 qa.append(dict(name=name,size=im.size,bounds=im.getbbox(),colors=len({p[:3] for p in im.get_flattened_data() if p[3]}),binary_alpha=True,roundtrip_exact=True,sha256=hashlib.sha256(p.read_bytes()).hexdigest()))
for name in ['MinotaurCharge']:
 assert (A/'Inputs'/f'{name}.png').read_bytes()==(D/f'{name}.png').read_bytes()
(A/'QA/NativeValidation.json').write_text(json.dumps(qa,indent=2));(A/'QA/Cleanup.json').write_text(json.dumps(cleanup,indent=2))
board=Image.new('RGBA',(440,650),(26,28,37,255));draw=ImageDraw.Draw(board)
draw.text((10,8),'BEFORE',fill='white');draw.text((230,8),'REVISED - NATIVE PIXELS',fill='white')
for row,name in enumerate(['WaveBody','Settings','Supply','AbilityPlate','CrownedSkull','Bull','Revolver','Flute']):
 y=42+row*75
 for col in range(2):
  x=10+col*220;im=read('Inputs/'+name+'.png') if col==0 else prepared[name]
  if name in ['CrownedSkull','Bull','Revolver','Flute']:
   plate=read('Inputs/AbilityPlate.png') if col==0 else prepared['AbilityPlate'];board.alpha_composite(plate,(x,y));board.alpha_composite(im,(x+64,y+13))
  else:board.alpha_composite(im,(x,y))
  draw.text((x,y-13),name,fill='#c5c7d0')
board.save(A/'Preview/ControlsNative.png');board.resize((880,1300),Image.Resampling.NEAREST).save(A/'Preview/Controls2x.png')
(A/'Scripts/BuildArt.py').write_bytes(Path(__file__).read_bytes())
print(json.dumps(qa))
