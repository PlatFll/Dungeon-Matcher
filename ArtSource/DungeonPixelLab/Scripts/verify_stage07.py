from pathlib import Path
from PIL import Image,ImageDraw
import json,hashlib,struct,zlib,xml.etree.ElementTree as ET
P=Path(__file__).resolve().parents[3];R=P/'ArtSource/DungeonPixelLab';D=R/'Diagnostics/Stage07';V=R/'Previews/Stage07'
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
sizes={'Torch_Fixture':(32,56),'Torch_Flame':(32,56),'Torch_Light_Left':(64,72),'Torch_Light_Right':(56,72)}
ims={n:load(R/'Exports/Stage07'/f'{n}.png') for n in sizes};rows=[];union=set()
assert len(list((R/'Exports/Stage07').glob('*.png')))==len(sizes)
for n,im in ims.items():
 f=R/'Exports/Stage07'/f'{n}.png';native=R/'Native/Stage07'/f'{n}.aseprite';cols=sorted(set(pixels(im)));alpha=sorted(set(c[3] for c in cols));opaque=sorted(set(c[:3] for c in cols if c[3]));union.update(opaque)
 assert im.size==sizes[n];assert ase(native).tobytes()==im.tobytes();assert set(alpha)=={0,255};assert sha(f)==sha(P/'Assets/_Game/Art/DungeonPixelLab/Torch'/f.name)
 box=im.getbbox();assert box[0]>0 and box[1]>0 and box[2]<im.width and box[3]<im.height,(n,box)
 rows.append(dict(name=n,path=f.relative_to(P).as_posix(),native_source=native.relative_to(P).as_posix(),dimensions=list(im.size),opaque_colors=len(opaque),palette=['#%02X%02X%02X'%c for c in opaque],alpha=alpha,bounds=list(box),sha256=sha(f),ase_sha256=sha(native),native_cel_equals_png=True,status='CANDIDATE_PENDING_USER'))
raw=load(R/'Sources/Stage07/Torch_Raw_PixelLab.png');assert raw.size==(64,64)
combined=Image.new('RGBA',(32,56));combined.alpha_composite(ims['Torch_Fixture']);combined.alpha_composite(ims['Torch_Flame'])
# Native source alignment retained; only one upper ember relocated, colors/iron shading cleaned.
for y in range(30,55):
 for x in range(16,48):assert bool(raw.getpixel((x,y))[3])==bool(ims['Torch_Fixture'].getpixel((x-16,y-4))[3])
cool={(10,13,17),(32,32,44),(61,59,80)}
assert {p[:3] for p in pixels(ims['Torch_Fixture'].crop((0,29,32,56))) if p[3]}<=cool
assert len({p[:3] for p in pixels(ims['Torch_Flame']) if p[3]})==4
protected=json.loads((D/'ProtectedArtHashes.json').read_text());assert all(sha(P/f)==h for f,h in protected.items())
ref=json.loads((R/'Progress/Stage07_Reference_Audit.json').read_text())
for f,k in [('Docs/ArtDirection/Dungeon_Matcher_Art_Direction.txt','guide_sha256'),('Docs/ArtDirection/Golden_Reference_Register.md','register_sha256')]:assert sha(P/f)==ref[k]
for r in ref['references']:
 assert sha(P/r['path'])==r['sha256']
 if 'production_path' in r:assert sha(P/r['production_path'])==r['production_sha256']
manifest=json.loads((R/'Progress/Stage07TorchPlacement.json').read_text());prior=json.loads((R/'Progress/Stage06SupportPlacement.json').read_text());assert [p for p in manifest['placements'] if not p['name'].startswith('Torch_')]==prior['placements'];assert len(manifest['placements'])==200
def compose(placements):
 result=Image.new('RGBA',(360,640))
 for p in sorted(placements,key=lambda p:p['layer']):
  n=p['name'];x,y=p['x'],p['y'];folder=next((f'Exports/Stage0{i}' for prefix,i in [('Wall_',1),('Recess_',2),('Floor_',3),('Fascia_',4),('Gate_',5),('Support_',6),('Torch_',7)] if n.startswith(prefix)),None)
  im=load(R/folder/(n+'.png')) if folder else load(P/'Assets/_Game/Art/DungeonEnvironment/SideViewRebuild/Frames_Player'/(n+'.png'))
  assert x>=0 and y>=0 and x+im.width<=360 and y+im.height<=640
  result.alpha_composite(im,(x,y))
 return result
expected=compose(manifest['placements']);unlit=compose([p for p in manifest['placements'] if not p['name'].startswith('Torch_Light_')]);before=compose(prior['placements'])
marked=expected.copy();dr=ImageDraw.Draw(marked)
for x in [154,234,314]:dr.rectangle((x-10,178,x+9,179),fill=(0,255,255,255))
# Local bounce changes only existing stone color fields; mortar and contours stay untouched.
base=compose([p for p in prior['placements'] if p['layer']<=14]);allowed={(40,40,63),(53,50,79),(48,41,64),(65,52,81),(56,45,85),(78,61,108),(103,83,125),(36,31,58)}
light_stats=[]
for n,ox in [('Torch_Light_Left',96),('Torch_Light_Right',304)]:
 im=ims[n];lit=0
 for y in range(im.height):
  for x in range(im.width):
   if not im.getpixel((x,y))[3]:continue
   assert base.getpixel((ox+x,68+y))[:3] in allowed,(n,x,y)
   lit+=1
 light_stats.append(dict(name=n,opaque_pixels=lit,world_bounds=[ox+im.getbbox()[0],68+im.getbbox()[1],ox+im.getbbox()[2],68+im.getbbox()[3]],mortar_preserved=True))
assert expected.crop((176,0,284,640)).tobytes()==before.crop((176,0,284,640)).tobytes()
assert expected.crop((0,142,360,640)).tobytes()==before.crop((0,142,360,640)).tobytes()
# Bright source remains distinctly above the nearby brick values, not a washed-out field.
def lum(p):return .2126*p[0]+.7152*p[1]+.0722*p[2]
flame_peak=max(lum(p) for p in pixels(ims['Torch_Flame']) if p[3]);bounce_peak=max(lum(p) for n in ['Torch_Light_Left','Torch_Light_Right'] for p in pixels(ims[n]) if p[3]);assert flame_peak>bounce_peak+80
screens=[]
for scale in [1,3]:
 for label,ex in [('Normal',expected),('Unlit',unlit),('Contact',marked),('Restored',expected)]:
  shot=load(V/f'UnityTorch-{label}-{scale}x.png');pred=ex.resize((360*scale,640*scale),Image.Resampling.NEAREST);assert shot.size==pred.size
  diff=sum(a!=b for a,b in zip(pixels(shot),pixels(pred)));assert diff==0,(label,scale,diff)
  screens.append(dict(label=label,scale=scale,dimensions=list(shot.size),rgba_differences=diff))
test=ET.parse(D/'UnityTests.xml').getroot();assert test.attrib['result']=='Passed' and int(test.attrib['failed'])==0
checks=dict(assets=rows,combined_palette_count=len(union),combined_palette=['#%02X%02X%02X'%c for c in sorted(union)],raw_source_sha256=sha(R/'Sources/Stage07/Torch_Raw_PixelLab.png'),torch_assembly_dimensions=[32,56],torch_occupied_bounds=list(combined.getbbox()),torch_count=2,placed_modules=6,light_overlays=light_stats,fixture_silhouette_preserved=True,iron_body_opaque_colors=3,flame_opaque_colors=4,flame_peak_luma=round(flame_peak,2),light_peak_luma=round(bounce_peak,2),central_combat_area_unchanged=[176,0,284,640],floor_and_lower_area_unchanged_from_y=142,clipped_sprite_bounds=0,protected_art_hashes=len(protected),prior_placements_preserved=len(prior['placements']),reference_hashes_unchanged=True,unity_test=dict(result=test.attrib['result'],passed=test.attrib['passed'],failed=test.attrib['failed']),screens=screens,native_resampling='none',production_scene_and_build_settings='unchanged; asserted in Unity test')
(D/'PixelChecks.json').write_text(json.dumps(checks,indent=2)+'\n')
combined.save(V/'Torch_Assembly_1x.png');combined.resize((128,224),Image.Resampling.NEAREST).save(V/'Torch_Assembly_4x.png')
for name in ['Normal','Unlit','Contact']:
 crop=load(V/f'UnityTorch-{name}-1x.png').crop((0,0,360,242));crop.save(V/f'Unity_Top_{name}_1x.png');crop.resize((1080,726),Image.Resampling.NEAREST).save(V/f'Unity_Top_{name}_3x.png')
before.crop((0,0,360,242)).save(V/'Before_Top_1x.png')
contact=Image.new('RGBA',(760,194),(10,13,17,255));dr=ImageDraw.Draw(contact);fg='#e4e2ed'
for x,label,im in [(12,'Raw PixelLab',raw),(126,'Clean torch',combined),(238,'Fixture',ims['Torch_Fixture']),(340,'Flame',ims['Torch_Flame']),(434,'Wall bounce',ims['Torch_Light_Left']),(548,'Pillar bounce',ims['Torch_Light_Right']),(668,'Rattlebones',load(P/'Docs/ArtDirection/Audit2026-10/media/skeleton-still.png'))]:
 dr.text((x,10),label,fill=fg);contact.alpha_composite(im,(x,34))
dr.text((12,144),'Native still kit / 3 iron shades + 4 fire colors / local stone ramps / binary alpha',fill=fg)
dr.text((12,164),'Same front-facing fixture at both positions; no mirrored lighting. Approved source artwork unchanged.',fill=fg)
contact.save(V/'Contact_1x.png');contact.resize((1520,388),Image.Resampling.NEAREST).save(V/'Contact_2x.png')
board=Image.new('RGBA',(740,270),(10,13,17,255));dr=ImageDraw.Draw(board);dr.text((8,8),'Torches, light overlays hidden',fill=fg);dr.text((380,8),'Stage 07 / actual Unity with local light',fill=fg)
board.alpha_composite(load(V/'Unity_Top_Unlit_1x.png'),(8,24));board.alpha_composite(load(V/'Unity_Top_Normal_1x.png'),(380,24));board.save(V/'Light_Comparison_1x.png')
print(json.dumps(dict(assets=len(rows),palette_counts=[r['opaque_colors'] for r in rows],combined_colors=len(union),screens=screens,protected_art=len(protected),light=light_stats),indent=2))
