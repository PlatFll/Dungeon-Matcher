from pathlib import Path
from PIL import Image, ImageDraw
import json, hashlib, struct, zlib, statistics, xml.etree.ElementTree as ET
P=Path(__file__).resolve().parents[3];R=P/'ArtSource/DungeonPixelLab';D=R/'Diagnostics/Stage02';V=R/'Previews/Stage02'
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def load(p):return Image.open(p).convert('RGBA')
def pixels(im):return list(im.get_flattened_data())
def decode_ase(p):
 data=p.read_bytes();w,h,depth=struct.unpack_from('<HHH',data,8);assert depth==32
 pos=128;size=struct.unpack_from('<I',data,pos)[0];pos+=16;out=Image.new('RGBA',(w,h))
 while pos<128+size:
  n,kind=struct.unpack_from('<IH',data,pos)
  if kind==0x2005:
   layer,x,y,opacity,ct=struct.unpack_from('<HhhBH',data,pos+6);assert layer==0 and opacity==255 and ct in (0,2)
   cw,ch=struct.unpack_from('<HH',data,pos+22);buf=data[pos+26:pos+n];buf=zlib.decompress(buf) if ct==2 else buf
   out.paste(Image.frombytes('RGBA',(cw,ch),buf),(x,y))
  pos+=n
 return out
rows=[]
for f in sorted((R/'Exports/Stage02').glob('*.png')):
 im=load(f);ase=R/'Native/Stage02'/f'{f.stem}.aseprite';colors=sorted(set(pixels(im)))
 assert decode_ase(ase).tobytes()==im.tobytes();assert {c[3] for c in colors}=={255}
 assert sha(f)==sha(P/'Assets/_Game/Art/DungeonPixelLab/PortraitRecess'/f.name)
 rows.append(dict(name=f.stem,path=f.relative_to(P).as_posix(),native_source=ase.relative_to(P).as_posix(),dimensions=list(im.size),palette=['#%02X%02X%02X'%c[:3] for c in colors],opaque_colors=len(colors),alpha=[255],bounds=list(im.getbbox()),sha256=sha(f),ase_sha256=sha(ase),native_cel_equals_png=True,status='CANDIDATE_PENDING_USER'))
assert [r['dimensions'] for r in rows]==[[56,26],[80,167]]
raw=load(R/'Sources/Stage02/Recess_Raw_PixelLab.png');assert raw.size==(64,64)
protected={}
for name in ['ProtectedWallHashes.json','ProtectedFrameHashes.json']:
 refs=json.loads((D/name).read_text());assert all(sha(P/f)==h for f,h in refs.items()),name
 protected[name]=len(refs)
refs=json.loads((R/'Progress/Stage02_Reference_Audit.json').read_text())
assert sha(P/'Docs/ArtDirection/Dungeon_Matcher_Art_Direction.txt')==refs['guide_sha256']
assert sha(P/'Docs/ArtDirection/Golden_Reference_Register.md')==refs['register_sha256']
for ref in refs['references']:assert sha(P/ref['path'])==ref['sha256']
manifest=json.loads((R/'Progress/Stage02RecessPlacement.json').read_text())
base=Image.new('RGBA',(360,640));panel=load(R/'Exports/Stage02/Recess_Inset.png');tile=load(R/'Exports/Stage02/Recess_Center.png')
frames=[]
for p in manifest['placements']:
 if p['name'].startswith('Wall_'):base.alpha_composite(load(R/f"Exports/Stage01/{p['name']}.png"),(p['x'],p['y']))
 elif p['name'].startswith('Player'):frames.append(p)
def framed(im):
 for p in frames:im.alpha_composite(load(P/f"Assets/_Game/Art/DungeonEnvironment/SideViewRebuild/Frames_Player/{p['name']}.png"),(p['x'],p['y']))
 return im
normal=base.copy();normal.alpha_composite(panel,(18,30));normal=framed(normal);baseline=framed(base.copy())
screens=[]
for scale in (1,3):
 shots={n:load(V/f'UnityRecess-{n}-{scale}x.png') for n in ['Normal','Baseline','MaskedProbe','UnmaskedControl','Restored']}
 for n,expected in [('Normal',normal),('Baseline',baseline),('Restored',normal)]:
  expected=expected.resize((360*scale,640*scale),Image.Resampling.NEAREST)
  diff=sum(a!=b for a,b in zip(pixels(shots[n]),pixels(expected)));assert diff==0,(n,scale,diff)
 x0,y0,x1,y1=18*scale,30*scale,98*scale,197*scale
 outside=inside=negative=0
 for y in range(640*scale):
  for x in range(360*scale):
   inmask=x0<=x<x1 and y0<=y<y1;b=shots['Baseline'].getpixel((x,y))
   changed=shots['MaskedProbe'].getpixel((x,y))!=b
   if inmask:inside+=changed
   else:outside+=changed;negative+=shots['UnmaskedControl'].getpixel((x,y))!=b
 assert outside==0 and inside>12000*scale*scale and negative>10000*scale*scale,(outside,inside,negative)
 screens.append(dict(scale=scale,dimensions=list(shots['Normal'].size),exact_render_differences=0,restored_differences=0,masked_probe_outside_changed_pixels=outside,masked_probe_inside_changed_pixels=inside,unmasked_control_outside_changed_pixels=negative))
def values(im):return [c[0]*.2126+c[1]*.7152+c[2]*.0722 for c in pixels(im)]
def edge_energy(im):
 q=values(im);w,h=im.size;d=[]
 for y in range(h):
  for x in range(w):
   i=y*w+x
   if x+1<w:d.append(abs(q[i+1]-q[i]))
   if y+1<h:d.append(abs(q[i+w]-q[i]))
 return statistics.mean(d)
wall=load(R/'Exports/Stage01/Wall_A_Quiet.png');contrast=dict(recess_mean_luma=statistics.mean(values(tile)),wall_mean_luma=statistics.mean(values(wall)),recess_adjacent_value_change=edge_energy(tile),wall_adjacent_value_change=edge_energy(wall),metric='Rec709 weights on encoded RGB; diagnostic value/readability proxy, not an art score')
assert contrast['recess_mean_luma']<contrast['wall_mean_luma']
assert contrast['recess_adjacent_value_change']<contrast['wall_adjacent_value_change']
test=ET.parse(D/'UnityTests.xml').getroot();assert test.attrib['result']=='Passed' and int(test.attrib['failed'])==0
checks=dict(source_raw_dimensions=list(raw.size),source_raw_sha256=sha(R/'Sources/Stage02/Recess_Raw_PixelLab.png'),source_raw_opaque_colors=len(set(pixels(raw))),assets=rows,protected_hash_counts=protected,reference_hashes_unchanged=True,mask_native_rect=[18,30,80,167],preview_only=True,unity_test=dict(result=test.attrib['result'],passed=test.attrib['passed'],failed=test.attrib['failed']),screens=screens,value_grouping=contrast,native_resampling='none',production_game_scene_and_build_settings='unchanged; asserted in Unity test')
(D/'PixelChecks.json').write_text(json.dumps(checks,indent=2)+'\n')
# These are external inspection compositions only, not game sprites or imported actors.
board=Image.new('RGBA',(500,264),(10,13,17,255));draw=ImageDraw.Draw(board);ink=(223,222,232,255)
draw.text((8,6),'STAGE 02 / portrait recess - native inspection',ink)
draw.text((8,26),'Raw 64x64',ink);board.paste(raw,(8,44))
draw.text((8,123),'Clean repeat',ink);board.paste(tile,(8,142));draw.text((8,180),'56x26 / 5 colors',ink)
draw.text((102,26),'Inset 80x167',ink);board.paste(panel,(102,44))
draw.text((203,26),'Unity crop',ink);board.paste(load(V/'UnityRecess-Normal-1x.png').crop((6,17,112,213)),(203,44))
actor=normal.copy();actor.alpha_composite(load(P/'Docs/ArtDirection/Audit2026-10/media/skeleton-still.png'),(26,112))
draw.text((332,26),'External actor check',ink);board.paste(actor.crop((6,17,112,213)),(345,44))
draw.text((8,248),'Actor pixels unchanged; actor is NOT in Unity preview.',ink)
board.save(V/'Contact_1x.png');board.resize((2000,1056),Image.Resampling.NEAREST).save(V/'Contact_4x.png')
crop=load(V/'UnityRecess-Normal-1x.png').crop((0,12,180,218));crop.save(V/'Unity_PortraitCrop_1x.png');crop.resize((540,618),Image.Resampling.NEAREST).save(V/'Unity_PortraitCrop_3x.png')
repeat=Image.new('RGBA',(112,52))
for y in range(2):
 for x in range(2):repeat.paste(tile,(56*x,26*y))
repeat.save(V/'Repeat_1x.png');repeat.resize((448,208),Image.Resampling.NEAREST).save(V/'Repeat_4x.png')
print(json.dumps(dict(assets=len(rows),colors=[r['opaque_colors'] for r in rows],screens=screens,value_grouping=contrast,protected=protected),indent=2))
