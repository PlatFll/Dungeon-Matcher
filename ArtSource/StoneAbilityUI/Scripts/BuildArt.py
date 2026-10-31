from pathlib import Path
from PIL import Image,ImageDraw
import hashlib,json,subprocess,uuid,shutil,re,sys
R=Path(__file__).resolve().parents[3]
A=R/'ArtSource/StoneAbilityUI';D=R/'Assets/_Game/Resources/UI/BattlePresentation/Stone'
D.mkdir(exist_ok=True);(A/'Roundtrip').mkdir(exist_ok=True)
palette=['0a0d11','262333','3c374a','524d61','6f6b7e','838091','948fa4','a7a1b6','c4bdd4','e3daf5']
rgb=lambda h:tuple(bytes.fromhex(h))
pal=list(map(rgb,palette));prepared={};maps={}
def remap(im,fn):
 out=Image.new('RGBA',im.size);out.putdata([(*fn(p[:3]),255) if p[3] else (0,0,0,0) for p in im.get_flattened_data()]);return out
def read(k,i):return Image.open(A/'Raw'/f'{k}_{i:02}.png').convert('RGBA')
def stone(p):return min(pal,key=lambda c:sum((a-b)**2 for a,b in zip(c,p)))
for name,key,index in [('AbilityPlate','PlateV2',0),('Supply','Supply',8),('Settings','Settings',20)]:
 source=read(key,index);im=remap(source,stone)
 # Remove just the extra lower-left scatter in the broad stone face. Keep the
 # generated bevels, sockets, contour, and other small stone clusters unchanged.
 if name=='AbilityPlate':
  d=ImageDraw.Draw(im);d.rectangle((22,39,44,47),fill=(*rgb('838091'),255))
 prepared[name]=im
 maps[name]={'source':f'Raw/{key}_{index:02}.png','color_map':{'#%02x%02x%02x'%p:'#%02x%02x%02x'%stone(p) for p in set(source.convert('RGB').get_flattened_data())}}
prepared['AbilitySocket']=remap(prepared['AbilityPlate'],lambda c:pal[max(0,pal.index(c)-3)])
def engraving(name,im,is_bull=False):
 # Keep the source's exact shape. Connected cut shadows plus a one-pixel lower
 # chisel lip read as incised stone, not a brightly pasted character sticker.
 def tone(p):
  v=sum(p)/3
  if is_bull:v=.3*p[0]+.59*p[1]+.11*p[2]
  return rgb('262333' if v<48 else '3c374a' if v<95 else '524d61' if v<165 else '6f6b7e')
 cut=remap(im,tone)
 pad=Image.new('RGBA',(48,40));x=(48-im.width)//2;y=(40-im.height)//2;pad.paste(cut,(x,y))
 lip=Image.new('RGBA',pad.size)
 for yy in range(1,40):
  for xx in range(1,48):
   if pad.getpixel((xx-1,yy-1))[3] and not pad.getpixel((xx,yy))[3]:lip.putpixel((xx,yy),(*rgb('a7a1b6'),255))
 lip.alpha_composite(pad);prepared[name]=lip
 def ready(p):
  return rgb('3c374a' if p==rgb('262333') else 'c4bdd4' if p==rgb('3c374a') else 'e3daf5' if p==rgb('524d61') else 'f6f4ef')
 prepared[name+'Ready']=remap(lip,ready)
for name,key,index in [('CrownedSkull','Skull',1),('Revolver','Revolver',0),('Flute','Flute',0)]:engraving(name,read(key,index))
small=A/'Raw/BullSmall_00.png'
if small.exists():
 index=int(sys.argv[1]) if len(sys.argv)>1 else 0
 bull=read('BullSmall',index)
 # Actual character outline and intentional warm fur/horn material preserved.
 bull=remap(bull,lambda c:rgb('0a0d11') if max(c)<32 else c)
 color=Image.new('RGBA',(40,40));color.paste(bull,(4,4));prepared['MinotaurCharge']=color
 engraving('Bull',bull,True)
 maps['Bull']={'source':f'Raw/BullSmall_{index:02}.png','native_scale':'unchanged, transparent padding only','engraving':'same colored head pixel shapes with incised stone material'}
for name,im in prepared.items():im.save(A/'Native'/f'{name}Prepared.png')
js='var root='+json.dumps(A.as_posix()+'/')+';\n'
for name in prepared:
 js+=f"app.open(root+'Native/{name}Prepared.png');var spr=app.activeSprite;var im=spr.layer(0).cel(0).image;var pc=app.pixelColor;\n"
 js+="for(var y=0;y<im.height;y++)for(var x=0;x<im.width;x++){var c=im.getPixel(x,y);if(pc.rgbaA(c)==0)im.putPixel(x,y,pc.rgba(0,0,0,0));}\n"
 js+=f"spr.layer(0).name='PixelLab stone and engraving: native palette cleanup';spr.commit();spr.saveAs(root+'Native/{name}.ase',true);spr.saveAs(root+'Exports/{name}.png',true);app.activeDocument.close();app.open(root+'Native/{name}.ase');app.activeSprite.saveAs(root+'Roundtrip/{name}.png',true);app.activeDocument.close();\n"
js+="console.log('STONE_NATIVE_ROUNDTRIP_COMPLETE');app.command.Exit();"
(A/'Scripts/FinishStone.js').write_text(js)
si=subprocess.STARTUPINFO();si.dwFlags|=subprocess.STARTF_USESHOWWINDOW;si.wShowWindow=0
run=subprocess.run([r'C:/Users/USER/Downloads/libresprite-development-windows-x86_64/libresprite.exe','--script',str(A/'Scripts/FinishStone.js')],startupinfo=si,stdout=subprocess.PIPE,stderr=subprocess.STDOUT,timeout=60)
(A/'QA/LibreSprite.log').write_bytes(run.stdout);assert run.returncode==0 and b'STONE_NATIVE_ROUNDTRIP_COMPLETE' in run.stdout,run.stdout
guids={};qa=[]
template=(R/'Assets/_Game/Resources/UI/BattlePresentation/Unified/WaveBody.png.meta').read_text()
for name,im in prepared.items():
 p=A/'Exports'/f'{name}.png';out=Image.open(p).convert('RGBA');check=Image.open(A/'Roundtrip'/p.name).convert('RGBA')
 assert out.tobytes()==check.tobytes()==im.tobytes(),name
 assert set(out.getchannel('A').get_flattened_data())<={0,255},name
 shutil.copy2(p,D/p.name);meta=D/(p.name+'.meta')
 if not meta.exists():
  text=re.sub(r'guid: \w+',lambda m:'guid: '+uuid.uuid4().hex,template,count=1)
  text=text.replace('spriteBorder: {x: 11, y: 10, z: 11, w: 10}','spriteBorder: {x: 0, y: 0, z: 0, w: 0}')
  meta.write_text(text)
 guids[name]=re.search(r'guid: (\w+)',meta.read_text()).group(1)
 qa.append(dict(name=name,size=im.size,bounds=im.getbbox(),colors=len({p[:3] for p in im.get_flattened_data() if p[3]}),binary_alpha=True,roundtrip_exact=True,sha256=hashlib.sha256(p.read_bytes()).hexdigest()))
for name,asset in [('CrownedSkull','RoyalDecree'),('Bull','DungeonAttender'),('Revolver','Ricochet'),('Flute','CrackedGems')]:
 if name not in guids:continue
 p=R/f'Assets/_Game/Data/Player Abilities/Ability_{asset}.asset';text=p.read_text()
 for field,source in [('stoneEngraving',name),('readyStoneEngraving',name+'Ready')]+([('boardAvatar','MinotaurCharge')] if name=='Bull' else []):
  value=f'  {field}: {{fileID: 21300000, guid: {guids[source]}, type: 3}}'
  text=re.sub(r'^  '+field+':.*$',value,text,flags=re.M) if re.search(r'^  '+field+':',text,re.M) else text.rstrip()+'\n'+value+'\n'
 p.write_text(text)
(A/'QA/NativeValidation.json').write_text(json.dumps(qa,indent=2));(A/'QA/Cleanup.json').write_text(json.dumps(maps,indent=2))
# Native and 3x comparison uses only nearest-neighbor enlargement.
board=Image.new('RGBA',(400,390),(26,28,37,255));draw=ImageDraw.Draw(board)
for i,name in enumerate([n for n in ['CrownedSkull','Bull','Revolver','Flute'] if n in prepared]):
 for col in range(2):
  x=col*200+12;y=i*90+22;board.alpha_composite(prepared['AbilityPlate'],(x,y));board.alpha_composite(prepared[name+('Ready' if col else '')],(x+64,y+13));draw.text((x,y-15),name+(' READY' if col else ' CHARGING'),fill='white')
board.save(A/'Preview/StoneControlsNative.png');board.resize((800,780),Image.Resampling.NEAREST).save(A/'Preview/StoneControls2x.png')
(A/'Scripts/BuildArt.py').write_bytes(Path(__file__).read_bytes())
print(json.dumps(qa))
