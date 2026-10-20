from pathlib import Path
from PIL import Image,ImageDraw
import json,hashlib,struct,zlib,xml.etree.ElementTree as ET
P=Path(__file__).resolve().parents[3];R=P/'ArtSource/DungeonPixelLab';D=R/'Diagnostics/Stage09';V=R/'Previews/Stage09'
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def load(p):return Image.open(p).convert('RGBA')
def pixels(im):return list(im.get_flattened_data())
def ase(p):
 b=p.read_bytes();w,h,dep=struct.unpack_from('<HHH',b,8);assert dep==32;out=Image.new('RGBA',(w,h));size=struct.unpack_from('<I',b,128)[0];pos=144
 while pos<128+size:
  n,typ=struct.unpack_from('<IH',b,pos)
  if typ==0x2005:
   layer,x,y,op,ct=struct.unpack_from('<HhhBH',b,pos+6);assert layer==0 and op==255 and ct in (0,2)
   cw,ch=struct.unpack_from('<HH',b,pos+22);buf=b[pos+26:pos+n];buf=zlib.decompress(buf) if ct==2 else buf;out.paste(Image.frombytes('RGBA',(cw,ch),buf),(x,y))
  pos+=n
 return out
sizes={'Clutter_Barrel':(36,42),'Clutter_Skull':(24,22),'Clutter_Bones':(26,12)}
ims={n:load(R/'Exports/Stage09'/f'{n}.png') for n in sizes};rows=[];union=set()
assert len(list((R/'Exports/Stage09').glob('*.png')))==3
for n,im in ims.items():
 f=R/'Exports/Stage09'/f'{n}.png';native=R/'Native/Stage09'/f'{n}.aseprite';cols=sorted(set(pixels(im)));alpha=sorted(set(c[3] for c in cols));opaque=sorted(set(c[:3] for c in cols if c[3]));union.update(opaque)
 assert im.size==sizes[n];assert ase(native).tobytes()==im.tobytes();assert set(alpha)=={0,255};assert sha(f)==sha(P/'Assets/_Game/Art/DungeonPixelLab/Clutter'/f.name)
 box=im.getbbox();assert box[0]>0 and box[1]>0 and box[2]<im.width and box[3]<im.height
 rows.append(dict(name=n,path=f.relative_to(P).as_posix(),native_source=native.relative_to(P).as_posix(),dimensions=list(im.size),opaque_colors=len(opaque),palette=['#%02X%02X%02X'%c for c in opaque],alpha=alpha,bounds=list(box),sha256=sha(f),ase_sha256=sha(native),native_cel_equals_png=True,status='CANDIDATE_PENDING_USER'))
raw=load(R/'Sources/Stage09/Barrel_Raw_PixelLab.png');bones=load(R/'Sources/Stage09/Bones_Raw_PixelLab.png');assert raw.size==bones.size==(64,64)
assert raw.crop((14,18,50,60)).getchannel('A').tobytes()==ims['Clutter_Barrel'].getchannel('A').tobytes()
skull_source=Image.new('RGBA',(24,22));skull_source.paste(bones.crop((8,28,30,50)),(0,0))
assert skull_source.getchannel('A').tobytes()==ims['Clutter_Skull'].getchannel('A').tobytes()
loose_source=Image.new('RGBA',(26,12));loose_source.paste(bones.crop((30,38,55,50)),(1,0))
assert loose_source.getchannel('A').tobytes()==ims['Clutter_Bones'].getchannel('A').tobytes()
wood_changes=0
for y in range(42):
 for x in range(36):
  a=raw.getpixel((x+14,y+18));b=ims['Clutter_Barrel'].getpixel((x,y))
  if a[3] and a[:3] not in [(98,94,113),(61,59,80)]:assert a==b;wood_changes+=a!=b
def components(im):
 left={(x,y) for y in range(im.height) for x in range(im.width) if im.getpixel((x,y))[3]};counts=[]
 while left:
  q=[left.pop()];count=0
  while q:
   x,y=q.pop();count+=1
   for dx in [-1,0,1]:
    for dy in [-1,0,1]:
     n=(x+dx,y+dy)
     if n in left:left.remove(n);q.append(n)
  counts.append(count)
 return sorted(counts)
cc={n:components(im) for n,im in ims.items()};assert len(cc['Clutter_Barrel'])==len(cc['Clutter_Skull'])==1 and len(cc['Clutter_Bones'])==2;assert min(cc['Clutter_Bones'])>20
protected=json.loads((D/'ProtectedArtHashes.json').read_text());assert all(sha(P/f)==h for f,h in protected.items())
ref=json.loads((R/'Progress/Stage09_Reference_Audit.json').read_text())
for f,k in [('Docs/ArtDirection/Dungeon_Matcher_Art_Direction.txt','guide_sha256'),('Docs/ArtDirection/Golden_Reference_Register.md','register_sha256')]:assert sha(P/f)==ref[k]
for r in ref['references']:
 assert sha(P/r['path'])==r['sha256']
 if 'production_path' in r:assert sha(P/r['production_path'])==r['production_sha256']
manifest=json.loads((R/'Progress/Stage09ClutterPlacement.json').read_text());prior=json.loads((R/'Progress/Stage08HangingPlacement.json').read_text());assert manifest['placements'][:215]==prior['placements'];assert len(manifest['placements'])==218
def compose(placements):
 result=Image.new('RGBA',(360,640))
 for p in sorted(placements,key=lambda p:p['layer']):
  n=p['name'];x,y=p['x'],p['y'];folder=next((f'Exports/Stage0{i}' for prefix,i in [('Wall_',1),('Recess_',2),('Floor_',3),('Fascia_',4),('Gate_',5),('Support_',6),('Torch_',7),('Hanging_',8),('Clutter_',9)] if n.startswith(prefix)),None)
  im=load(R/folder/(n+'.png')) if folder else load(P/'Assets/_Game/Art/DungeonEnvironment/SideViewRebuild/Frames_Player'/(n+'.png'))
  assert x>=0 and y>=0 and x+im.width<=360 and y+im.height<=640
  result.alpha_composite(im,(x,y))
 return result
ui_reserved=[(130,15,239,46),(175,44,187,56),(254,23,312,51),(314,22,350,52),(9,20,108,210)]
standing=[(x-10,124,x+10,180) for x in [154,234,314]]
contact=[]
for p in manifest['placements'][215:]:
 im=ims[p['name']];box=im.getbbox();world=[p['x']+box[0],p['y']+box[1],p['x']+box[2],p['y']+box[3]];assert world[3]==180
 for y in range(im.height):
  for x in range(im.width):
   if im.getpixel((x,y))[3]:assert all(not (l<=x+p['x']<r and t<=y+p['y']<b) for l,t,r,b in ui_reserved+standing)
 contact.append(dict(name=p['name'],world_occupied_bounds=world,floor_contact_y=180))
expected=compose(manifest['placements']);unlit=compose([p for p in manifest['placements'] if not p['name'].startswith('Torch_Light_')]);before=compose(prior['placements'])
assert expected.crop((0,0,360,140)).tobytes()==before.crop((0,0,360,140)).tobytes()
assert expected.crop((0,180,360,640)).tobytes()==before.crop((0,180,360,640)).tobytes()
marked=expected.copy();dr=ImageDraw.Draw(marked)
for x in [154,234,314]:dr.rectangle((x-10,178,x+9,179),fill=(0,255,255,255))
screens=[]
for scale in [1,3]:
 for label,ex in [('Normal',expected),('Unlit',unlit),('Contact',marked),('Restored',expected)]:
  shot=load(V/f'UnityClutter-{label}-{scale}x.png');pred=ex.resize((360*scale,640*scale),Image.Resampling.NEAREST);assert shot.size==pred.size
  diff=sum(a!=b for a,b in zip(pixels(shot),pixels(pred)));assert diff==0,(label,scale,diff)
  screens.append(dict(label=label,scale=scale,dimensions=list(shot.size),rgba_differences=diff))
test=ET.parse(D/'UnityTests.xml').getroot();assert test.attrib['result']=='Passed' and int(test.attrib['failed'])==0
checks=dict(assets=rows,combined_palette_count=len(union),combined_palette=['#%02X%02X%02X'%c for c in sorted(union)],raw_source_sha256=sha(R/'Sources/Stage09/Barrel_Raw_PixelLab.png'),raw_bones_sha256=sha(R/'Sources/Stage09/Bones_Raw_PixelLab.png'),barrel_skull_silhouettes_preserved=True,barrel_wood_pixels_unchanged=True,bone_fragments='Exact fuller raw silhouettes restored at native pitch; only coloring cleaned, alpha mask identical.',connected_components=cc,contact=contact,UI_reserved_rects=ui_reserved,diagnostic_20px_standing_lanes=standing,UI_and_standing_lane_overlap_pixels=0,placed_modules=3,clipped_sprite_bounds=0,protected_art_hashes=len(protected),prior_placements_preserved=215,reference_hashes_unchanged=True,unity_test=dict(result=test.attrib['result'],passed=test.attrib['passed'],failed=test.attrib['failed']),screens=screens,native_resampling='none',production_scene_and_build_settings='unchanged; asserted in Unity test',coverage_limit='Actual characters/devices not tested; standing lanes are 20px diagnostics, not full actor silhouette certification.')
(D/'PixelChecks.json').write_text(json.dumps(checks,indent=2)+'\n')
for name in ['Normal','Unlit','Contact']:
 crop=load(V/f'UnityClutter-{name}-1x.png').crop((0,0,360,242));crop.save(V/f'Unity_Top_{name}_1x.png');crop.resize((1080,726),Image.Resampling.NEAREST).save(V/f'Unity_Top_{name}_3x.png')
before.crop((0,0,360,242)).save(V/'Before_Top_1x.png')
sheet=Image.new('RGBA',(740,160),(20,22,32,255));dr=ImageDraw.Draw(sheet);fg='#e4e2ed'
for x,label,im in [(12,'Raw barrel',raw),(122,'Raw bones',bones),(234,'Clean barrel',ims['Clutter_Barrel']),(350,'Skull',ims['Clutter_Skull']),(438,'Bones',ims['Clutter_Bones']),(550,'Rattlebones',load(P/'Docs/ArtDirection/Audit2026-10/media/skeleton-still.png'))]:
 dr.text((x,10),label,fill=fg);sheet.alpha_composite(im,(x,32))
dr.text((12,116),'Native pixels / binary alpha / wood, dull iron and matte bone / no source resampling',fill=fg)
dr.text((12,138),'Raw bones restored: identical silhouette, colors cleaned. Other approved art unchanged.',fill=fg)
sheet.save(V/'Contact_1x.png');sheet.resize((1480,320),Image.Resampling.NEAREST).save(V/'Contact_2x.png')
board=Image.new('RGBA',(740,270),(10,13,17,255));dr=ImageDraw.Draw(board);dr.text((8,8),'Approved Stage 08',fill=fg);dr.text((380,8),'Stage 09 / actual Unity',fill=fg)
board.alpha_composite(before.crop((0,0,360,242)),(8,24));board.alpha_composite(load(V/'Unity_Top_Normal_1x.png'),(380,24));board.save(V/'Comparison_1x.png')
print(json.dumps(dict(assets=len(rows),palette_counts=[r['opaque_colors'] for r in rows],combined_colors=len(union),screens=len(screens),all_screens_zero_RGBA_difference=True,protected_art=len(protected),contacts=contact,overlaps=0)))
