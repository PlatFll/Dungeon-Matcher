from pathlib import Path
from PIL import Image,ImageDraw
import json,hashlib,struct,zlib,xml.etree.ElementTree as ET
P=Path(__file__).resolve().parents[3];R=P/'ArtSource/DungeonPixelLab';D=R/'Diagnostics/Stage05';V=R/'Previews/Stage05'
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
files=sorted((R/'Exports/Stage05').glob('*.png'));ims={f.stem:load(f) for f in files};rows=[];union=set()
assert len(files)==3
for f in files:
 im=ims[f.stem];native=R/'Native/Stage05'/f'{f.stem}.aseprite';cols=sorted(set(pixels(im)));alpha=sorted(set(c[3] for c in cols));opaque=sorted(set(c[:3] for c in cols if c[3]));union.update(opaque)
 assert im.size==(96,112);assert ase(native).tobytes()==im.tobytes();assert set(alpha)=={0,255};assert sha(f)==sha(P/'Assets/_Game/Art/DungeonPixelLab/Gate'/f.name)
 box=im.getbbox();assert box[0]>0 and box[1]>0 and box[2]<96 and box[3]<112
 rows.append(dict(name=f.stem,path=f.relative_to(P).as_posix(),native_source=native.relative_to(P).as_posix(),dimensions=list(im.size),opaque_colors=len(opaque),palette=['#%02X%02X%02X'%c for c in opaque],alpha=alpha,bounds=list(box),sha256=sha(f),ase_sha256=sha(native),native_cel_equals_png=True,status='CANDIDATE_PENDING_USER'))
combined=Image.new('RGBA',(96,112))
for n in ['Gate_Leaf','Gate_Arch','Gate_Hardware']:combined.alpha_composite(ims[n])
assert combined.getbbox()==(10,12,84,108)
assert all(combined.getpixel((x,107))[3]==255 for x in range(10,84))
raw=load(R/'Sources/Stage05/Gate_Raw_PixelLab.png')
assert sha(R/'Sources/Stage05/Gate_Raw_PixelLab.png')=='774d3d73d5bf0ad1e7b05ea799a415c13960c22d17e4cb150084f809b98e6737'
# Outer crown envelope stays at native coordinates. Filled inner holes are not exterior silhouette changes.
for y in range(12,66):
 a=[x for x in range(96) if raw.getpixel((x,y))[3]];b=[x for x in range(96) if combined.getpixel((x,y))[3]]
 assert min(a)==min(b) and max(a)==max(b),(y,'outer crown changed')
protected=json.loads((D/'ProtectedArtHashes.json').read_text());assert all(sha(P/f)==h for f,h in protected.items())
ref=json.loads((R/'Progress/Stage05_Reference_Audit.json').read_text())
for f,k in [('Docs/ArtDirection/Dungeon_Matcher_Art_Direction.txt','guide_sha256'),('Docs/ArtDirection/Golden_Reference_Register.md','register_sha256')]:assert sha(P/f)==ref[k]
for r in ref['references']:
 assert sha(P/r['path'])==r['sha256']
 if 'production_path' in r:assert sha(P/r['production_path'])==r['production_sha256']
manifest=json.loads((R/'Progress/Stage05GatePlacement.json').read_text());prior=json.loads((R/'Progress/Stage04FasciaPlacement.json').read_text());assert [p for p in manifest['placements'] if not p['name'].startswith('Gate_')]==prior['placements']
expected=Image.new('RGBA',(360,640))
for p in sorted(manifest['placements'],key=lambda p:p['layer']):
 n=p['name'];x,y=p['x'],p['y']
 folder=next((f'Exports/Stage0{i}' for prefix,i in [('Wall_',1),('Recess_',2),('Floor_',3),('Fascia_',4),('Gate_',5)] if n.startswith(prefix)),None)
 im=load(R/folder/(n+'.png')) if folder else load(P/'Assets/_Game/Art/DungeonEnvironment/SideViewRebuild/Frames_Player'/(n+'.png'))
 assert x>=0 and y>=0 and x+im.width<=360 and y+im.height<=640
 if n.startswith('Gate_'):assert (x,y)==(177,72)
 expected.alpha_composite(im,(x,y))
marked=expected.copy();dr=ImageDraw.Draw(marked)
for x in [154,234,314]:dr.rectangle((x-10,178,x+9,179),fill=(0,255,255,255))
screens=[]
for scale in [1,3]:
 for label,ex in [('Normal',expected),('Contact',marked),('Restored',expected)]:
  shot=load(V/f'UnityGate-{label}-{scale}x.png');pred=ex.resize((360*scale,640*scale),Image.Resampling.NEAREST);assert shot.size==pred.size
  diff=sum(a!=b for a,b in zip(pixels(shot),pixels(pred)));assert diff==0,(label,scale,diff)
  screens.append(dict(label=label,scale=scale,dimensions=list(shot.size),rgba_differences=diff))
test=ET.parse(D/'UnityTests.xml').getroot();assert test.attrib['result']=='Passed' and int(test.attrib['failed'])==0
checks=dict(assets=rows,combined_palette_count=len(union),combined_palette=['#%02X%02X%02X'%c for c in sorted(union)],combined_bounds=[10,12,84,108],world_occupied_bounds=[187,84,261,180],door_center_x=224,floor_contact_row=180,placed_modules=3,shared_canvas=[96,112],source_straight_extension_rows=12,source_outer_crown_envelope_preserved=True,closed_base_row_width=74,clipped_sprite_bounds=0,protected_art_hashes=len(protected),prior_placements_preserved=len(prior['placements']),reference_hashes_unchanged=True,unity_test=dict(result=test.attrib['result'],passed=test.attrib['passed'],failed=test.attrib['failed']),screens=screens,native_resampling='none',production_scene_and_build_settings='unchanged; asserted in Unity test')
(D/'PixelChecks.json').write_text(json.dumps(checks,indent=2)+'\n')
combined.save(V/'Gate_Assembly_1x.png');combined.resize((384,448),Image.Resampling.NEAREST).save(V/'Gate_Assembly_4x.png')
for name in ['Normal','Contact']:
 crop=load(V/f'UnityGate-{name}-1x.png').crop((0,12,360,230));crop.save(V/f'Unity_Top_{name}_1x.png');crop.resize((1080,654),Image.Resampling.NEAREST).save(V/f'Unity_Top_{name}_3x.png')
contact=Image.new('RGBA',(780,208),(10,13,17,255));dr=ImageDraw.Draw(contact);fg='#e4e2ed'
for x,label,im in [(12,'Raw PixelLab',raw),(148,'Clean assembled',combined),(284,'Stone arch',ims['Gate_Arch']),(420,'Gate leaf',ims['Gate_Leaf']),(556,'Bronze fittings',ims['Gate_Hardware'])]:
 dr.text((x,10),label,fill=fg);contact.alpha_composite(im,(x,30))
dr.text((682,10),'Rattlebones',fill=fg);contact.alpha_composite(load(P/'Docs/ArtDirection/Audit2026-10/media/skeleton-still.png'),(690,30))
dr.text((682,102),'Approved wall',fill=fg);contact.alpha_composite(load(R/'Exports/Stage01/Wall_A_Quiet.png'),(698,116))
dr.text((12,174),'Native 96x112 layers / +12 authored rows in straight piers and bars / no scaling of the crown',fill=fg)
dr.text((12,190),f'{len(union)} meaningful colors combined / binary alpha / one level floor anchor / unchanged approved art',fill=fg)
contact.save(V/'Contact_1x.png');contact.resize((1560,416),Image.Resampling.NEAREST).save(V/'Contact_2x.png')
print(json.dumps(dict(assets=len(rows),palette_counts=[r['opaque_colors'] for r in rows],combined_colors=len(union),screens=screens,protected_art=len(protected)),indent=2))
