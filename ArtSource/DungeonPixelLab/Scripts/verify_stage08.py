from pathlib import Path
from PIL import Image,ImageDraw
import json,hashlib,struct,zlib,xml.etree.ElementTree as ET
P=Path(__file__).resolve().parents[3];R=P/'ArtSource/DungeonPixelLab';D=R/'Diagnostics/Stage08';V=R/'Previews/Stage08'
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
sizes={'Hanging_Banner':(36,64),'Hanging_Chain_Anchor':(12,12),'Hanging_Chain_Repeat':(12,12),'Hanging_Chain_End':(12,12)}
ims={n:load(R/'Exports/Stage08'/f'{n}.png') for n in sizes};rows=[];union=set()
assert len(list((R/'Exports/Stage08').glob('*.png')))==4
for n,im in ims.items():
 f=R/'Exports/Stage08'/f'{n}.png';native=R/'Native/Stage08'/f'{n}.aseprite';cols=sorted(set(pixels(im)));alpha=sorted(set(c[3] for c in cols));opaque=sorted(set(c[:3] for c in cols if c[3]));union.update(opaque)
 assert im.size==sizes[n];assert ase(native).tobytes()==im.tobytes();assert set(alpha)=={0,255};assert sha(f)==sha(P/'Assets/_Game/Art/DungeonPixelLab/Hanging'/f.name)
 box=im.getbbox();assert box[0]>0 and box[2]<im.width
 if n=='Hanging_Banner':assert box[1]>0 and box[3]<im.height
 # Chain modules intentionally connect on y boundaries; assembled endpoints have clearance.
 rows.append(dict(name=n,path=f.relative_to(P).as_posix(),native_source=native.relative_to(P).as_posix(),dimensions=list(im.size),opaque_colors=len(opaque),palette=['#%02X%02X%02X'%c for c in opaque],alpha=alpha,bounds=list(box),sha256=sha(f),ase_sha256=sha(native),native_cel_equals_png=True,status='CANDIDATE_PENDING_USER'))
raw=load(R/'Sources/Stage08/Hanging_Raw_PixelLab.png');seed=load(R/'Sources/Stage08/Chain_Raw_PixelLab.png');assert raw.size==(96,64) and seed.size==(16,64)
banner=ims['Hanging_Banner'];assert banner.getchannel('A').tobytes()==raw.crop((30,0,66,64)).getchannel('A').tobytes()
crest=[]
for y in range(9,64):
 for x in range(36):
  a=raw.getpixel((x+30,y));b=banner.getpixel((x,y))
  if a[3] and a[:3] in [(190,154,121),(157,119,102)]:assert a==b;crest.append((x,y))
cx=(min(x for x,y in crest)+max(x for x,y in crest))/2;assert abs(cx-17.5)<=1
repeat=ims['Hanging_Chain_Repeat'];anchor=ims['Hanging_Chain_Anchor'];end=ims['Hanging_Chain_End']
assert repeat.crop((0,0,12,1)).tobytes()==repeat.crop((0,11,12,12)).tobytes()==anchor.crop((0,11,12,12)).tobytes()==end.crop((0,0,12,1)).tobytes()
assert all(repeat.getpixel((x,y))[3]==0 for x in [5,6] for y in [2,3,4,5])
chain=Image.new('RGBA',(12,72));chain.alpha_composite(anchor)
for y in [12,24,36,48]:chain.alpha_composite(repeat,(0,y))
chain.alpha_composite(end,(0,60));assert chain.getbbox()==(2,2,10,68)
# One connected 4-neighbor metal strand, holes remain transparent.
occupied={(x,y) for y in range(72) for x in range(12) if chain.getpixel((x,y))[3]};seen=set();queue=[next(iter(occupied))]
while queue:
 p=queue.pop()
 if p in seen:continue
 seen.add(p);x,y=p
 queue.extend(n for n in [(x-1,y),(x+1,y),(x,y-1),(x,y+1)] if n in occupied and n not in seen)
assert seen==occupied
protected=json.loads((D/'ProtectedArtHashes.json').read_text());assert all(sha(P/f)==h for f,h in protected.items())
ref=json.loads((R/'Progress/Stage08_Reference_Audit.json').read_text())
for f,k in [('Docs/ArtDirection/Dungeon_Matcher_Art_Direction.txt','guide_sha256'),('Docs/ArtDirection/Golden_Reference_Register.md','register_sha256')]:assert sha(P/f)==ref[k]
for r in ref['references']:
 assert sha(P/r['path'])==r['sha256']
 if 'production_path' in r:assert sha(P/r['production_path'])==r['production_sha256']
manifest=json.loads((R/'Progress/Stage08HangingPlacement.json').read_text());prior=json.loads((R/'Progress/Stage07TorchPlacement.json').read_text());assert manifest['placements'][:200]==prior['placements'];assert len(manifest['placements'])==215
def compose(placements):
 result=Image.new('RGBA',(360,640))
 for p in sorted(placements,key=lambda p:p['layer']):
  n=p['name'];x,y=p['x'],p['y'];folder=next((f'Exports/Stage0{i}' for prefix,i in [('Wall_',1),('Recess_',2),('Floor_',3),('Fascia_',4),('Gate_',5),('Support_',6),('Torch_',7),('Hanging_',8)] if n.startswith(prefix)),None)
  im=load(R/folder/(n+'.png')) if folder else load(P/'Assets/_Game/Art/DungeonEnvironment/SideViewRebuild/Frames_Player'/(n+'.png'))
  assert x>=0 and y>=0 and x+im.width<=360 and y+im.height<=640
  result.alpha_composite(im,(x,y))
 return result
reserved=[(130,15,239,46),(175,44,187,56),(254,23,312,51),(314,22,350,52)]
for p in manifest['placements'][200:]:
 im=ims[p['name']]
 for y in range(im.height):
  for x in range(im.width):
   if im.getpixel((x,y))[3]:assert all(not (l<=x+p['x']<r and t<=y+p['y']<b) for l,t,r,b in reserved)
portrait=next(p for p in manifest['placements'][200:] if p['name']=='Hanging_Banner' and p['layer']==35)
assert 18<=portrait['x'] and portrait['x']+36<=98 and 30<=portrait['y'] and portrait['y']+64<=197
expected=compose(manifest['placements']);unlit=compose([p for p in manifest['placements'] if not p['name'].startswith('Torch_Light_')]);before=compose(prior['placements'])
assert expected.crop((0,122,360,640)).tobytes()==before.crop((0,122,360,640)).tobytes()
marked=expected.copy();dr=ImageDraw.Draw(marked)
for x in [154,234,314]:dr.rectangle((x-10,178,x+9,179),fill=(0,255,255,255))
screens=[]
for scale in [1,3]:
 for label,ex in [('Normal',expected),('Unlit',unlit),('Contact',marked),('Restored',expected)]:
  shot=load(V/f'UnityHanging-{label}-{scale}x.png');pred=ex.resize((360*scale,640*scale),Image.Resampling.NEAREST);assert shot.size==pred.size
  diff=sum(a!=b for a,b in zip(pixels(shot),pixels(pred)));assert diff==0,(label,scale,diff)
  screens.append(dict(label=label,scale=scale,dimensions=list(shot.size),rgba_differences=diff))
test=ET.parse(D/'UnityTests.xml').getroot();assert test.attrib['result']=='Passed' and int(test.attrib['failed'])==0
checks=dict(assets=rows,combined_palette_count=len(union),combined_palette=['#%02X%02X%02X'%c for c in sorted(union)],raw_source_sha256=sha(R/'Sources/Stage08/Hanging_Raw_PixelLab.png'),raw_chain_sha256=sha(R/'Sources/Stage08/Chain_Raw_PixelLab.png'),banner_silhouette_and_crest_preserved=True,crest_center_x=cx,banner_count=3,chain_count=2,chain_repeat_pitch=12,chain_seams_equal=True,chain_single_connected_component=True,chain_assembly_bounds=list(chain.getbbox()),chain_raw_limitation='PixFlux source is a clipped two-pixel strand; face-on holes, anchor and termination substantially reconstructed in LibreSprite.',style_reference_limitation='PixFlux has no style-reference input; actual golden images inspected for comparison. Banner Pro Flash received actual native Rattlebones.',future_ui_reserved_rects=reserved,future_ui_overlap_pixels=0,placed_modules=15,clipped_sprite_bounds=0,protected_art_hashes=len(protected),prior_placements_preserved=200,reference_hashes_unchanged=True,unity_test=dict(result=test.attrib['result'],passed=test.attrib['passed'],failed=test.attrib['failed']),screens=screens,native_resampling='none',production_scene_and_build_settings='unchanged; asserted in Unity test')
(D/'PixelChecks.json').write_text(json.dumps(checks,indent=2)+'\n')
chain.save(V/'Chain_Assembly_1x.png');chain.resize((48,288),Image.Resampling.NEAREST).save(V/'Chain_Assembly_4x.png')
for name in ['Normal','Unlit','Contact']:
 crop=load(V/f'UnityHanging-{name}-1x.png').crop((0,0,360,242));crop.save(V/f'Unity_Top_{name}_1x.png');crop.resize((1080,726),Image.Resampling.NEAREST).save(V/f'Unity_Top_{name}_3x.png')
before.crop((0,0,360,242)).save(V/'Before_Top_1x.png')
contact=Image.new('RGBA',(740,160),(20,22,32,255));dr=ImageDraw.Draw(contact);fg='#e4e2ed'
for x,label,im in [(12,'Raw banner',raw),(126,'Raw chain',seed),(228,'Clean banner',banner),(344,'Clean chain',chain),(450,'Modules',anchor),(550,'Rattlebones',load(P/'Docs/ArtDirection/Audit2026-10/media/skeleton-still.png'))]:
 dr.text((x,10),label,fill=fg);contact.alpha_composite(im,(x,32))
contact.alpha_composite(repeat,(474,32));contact.alpha_composite(end,(498,32))
dr.text((12,120),'Native pixels / binary alpha / one banner size / complete ends / 12px chain repeat',fill=fg)
dr.text((12,140),'Raw chain required substantial native repair. Approved art and prior placements unchanged.',fill=fg)
contact.save(V/'Contact_1x.png');contact.resize((1480,320),Image.Resampling.NEAREST).save(V/'Contact_2x.png')
board=Image.new('RGBA',(740,270),(10,13,17,255));dr=ImageDraw.Draw(board);dr.text((8,8),'Approved Stage 07',fill=fg);dr.text((380,8),'Stage 08 / actual Unity',fill=fg)
board.alpha_composite(before.crop((0,0,360,242)),(8,24));board.alpha_composite(load(V/'Unity_Top_Normal_1x.png'),(380,24));board.save(V/'Comparison_1x.png')
print(json.dumps(dict(assets=len(rows),palette_counts=[r['opaque_colors'] for r in rows],combined_colors=len(union),screens=len(screens),all_screens_zero_RGBA_difference=True,protected_art=len(protected),crest_center=cx,chain_connected=True,UI_overlaps=0)))
