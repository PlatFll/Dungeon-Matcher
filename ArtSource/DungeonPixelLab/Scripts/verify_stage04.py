from pathlib import Path
import json,hashlib,struct,zlib,itertools,xml.etree.ElementTree as ET
from PIL import Image,ImageDraw
P=Path(__file__).resolve().parents[3];R=P/'ArtSource/DungeonPixelLab';D=R/'Diagnostics/Stage04';V=R/'Previews/Stage04'
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
files=sorted((R/'Exports/Stage04').glob('*.png'));ims={f.stem:load(f) for f in files};rows=[]
assert len(files)==5
for f in files:
 im=ims[f.stem];native=R/'Native/Stage04'/f'{f.stem}.aseprite';cols=sorted(set(pixels(im)));alpha=sorted(set(c[3] for c in cols));opaque=sorted(set(c[:3] for c in cols if c[3]))
 assert im.size==({'Fascia_Left':42,'Fascia_Right':62}.get(f.stem,64),24)
 assert ase(native).tobytes()==im.tobytes();assert set(alpha)=={0,255};assert sha(f)==sha(P/'Assets/_Game/Art/DungeonPixelLab/Fascia'/f.name)
 assert im.getchannel('A').crop((0,0,im.width,22)).getextrema()==(255,255)
 assert im.getchannel('A').crop((0,22,im.width,24)).getextrema()==(0,0)
 rows.append(dict(name=f.stem,path=f.relative_to(P).as_posix(),native_source=native.relative_to(P).as_posix(),dimensions=list(im.size),opaque_colors=len(opaque),palette=['#%02X%02X%02X'%c for c in opaque],alpha=alpha,bounds=list(im.getbbox()),sha256=sha(f),ase_sha256=sha(native),native_cel_equals_png=True,status='CANDIDATE_PENDING_USER'))
mid=['Fascia_Mid_A','Fascia_Mid_B','Fascia_Anchor'];pairs=list(itertools.product(mid,repeat=2))+[('Fascia_Left',n) for n in mid]+[(n,'Fascia_Right') for n in mid]+[('Fascia_Left','Fascia_Right')]
for a,b in pairs:assert ims[a].crop((ims[a].width-1,0,ims[a].width,24)).tobytes()==ims[b].crop((0,0,1,24)).tobytes(),(a,b)
assert len({ims[n].tobytes() for n in mid})==3
protected=json.loads((D/'ProtectedArtHashes.json').read_text());assert all(sha(P/f)==h for f,h in protected.items())
ref=json.loads((R/'Progress/Stage04_Reference_Audit.json').read_text())
for f,k in [('Docs/ArtDirection/Dungeon_Matcher_Art_Direction.txt','guide_sha256'),('Docs/ArtDirection/Golden_Reference_Register.md','register_sha256')]:assert sha(P/f)==ref[k]
for r in ref['references']:
 assert sha(P/r['path'])==r['sha256']
 if 'production_path' in r:assert sha(P/r['production_path'])==r['production_sha256']
for name in ['Floor_Raw_PixelLab.png','Floor_Raw_Metadata.json']:assert sha(R/'Sources/Stage03'/name)==sha(R/'Sources/Stage04'/name)
manifest=json.loads((R/'Progress/Stage04FasciaPlacement.json').read_text());prior=json.loads((R/'Progress/Stage03FloorPlacement.json').read_text());assert [p for p in manifest['placements'] if not p['name'].startswith('Fascia_')]==prior['placements']
expected=Image.new('RGBA',(360,640));fascia=Image.new('RGBA',(360,24));cover=Image.new('L',(360,24));cd=ImageDraw.Draw(cover)
for p in sorted(manifest['placements'],key=lambda p:p['layer']):
 n=p['name'];x,y=p['x'],p['y']
 if n.startswith('Wall_'):im=load(R/f'Exports/Stage01/{n}.png')
 elif n.startswith('Recess_'):im=load(R/f'Exports/Stage02/{n}.png')
 elif n.startswith('Player'):im=load(P/f'Assets/_Game/Art/DungeonEnvironment/SideViewRebuild/Frames_Player/{n}.png')
 elif n.startswith('Floor_'):im=load(R/f'Exports/Stage03/{n}.png')
 elif n.startswith('Fascia_'):
  im=ims[n];assert y==190 and x>=0 and x+im.width<=360
  assert not cover.crop((x,0,x+im.width,24)).getbbox();cd.rectangle((x,0,x+im.width-1,23),fill=255);fascia.alpha_composite(im,(x,0))
 else:raise AssertionError(n)
 assert x>=0 and y>=0 and x+im.width<=360 and y+im.height<=640
 expected.alpha_composite(im,(x,y))
assert cover.getextrema()==(255,255)
assert fascia.getchannel('A').crop((0,0,360,22)).getextrema()==(255,255)
# Full opaque top edge meets the existing floor directly at y190.
for x in range(106,360):
 assert expected.getpixel((x,189))[3]==expected.getpixel((x,190))[3]==255
marked=expected.copy();dr=ImageDraw.Draw(marked)
for x in [154,234,314]:
 for box in [(x-3,154,x+3,160),(x-5,161,x+5,173),(x-4,174,x-2,179),(x+2,174,x+4,179),(x-7,179,x+7,179)]:dr.rectangle(box,fill=(0,255,255,255))
screens=[]
for scale in [1,3]:
 for label,ex in [('Normal',expected),('Contact',marked),('Restored',expected)]:
  shot=load(V/f'UnityFascia-{label}-{scale}x.png');pred=ex.resize((360*scale,640*scale),Image.Resampling.NEAREST);assert shot.size==pred.size
  diff=sum(a!=b for a,b in zip(pixels(shot),pixels(pred)));assert diff==0,(label,scale,diff)
  screens.append(dict(label=label,scale=scale,dimensions=list(shot.size),rgba_differences=diff))
test=ET.parse(D/'UnityTests.xml').getroot();assert test.attrib['result']=='Passed' and int(test.attrib['failed'])==0
checks=dict(assets=rows,legal_pairs=[dict(left=a,right=b,exact_profile_match=True) for a,b in pairs],floor_contact_row=180,fascia_top_row=190,placed_modules=6,fascia_native_rect=[0,190,360,24],opaque_course_height=22,transparent_footer_rows=2,gaps=0,overlaps=0,clipped_sprite_bounds=0,protected_art_hashes=len(protected),prior_placements_preserved=True,reference_hashes_unchanged=True,raw_source_reused_byte_identically=True,unity_test=dict(result=test.attrib['result'],passed=test.attrib['passed'],failed=test.attrib['failed']),screens=screens,native_resampling='none',production_scene_and_build_settings='unchanged; asserted in Unity test')
(D/'PixelChecks.json').write_text(json.dumps(checks,indent=2)+'\n')
fascia.save(V/'FasciaStrip_1x.png');fascia.resize((1440,96),Image.Resampling.NEAREST).save(V/'FasciaStrip_4x.png')
grid=Image.new('RGBA',(310,len(pairs)*32+24),(10,13,17,255));dr=ImageDraw.Draw(grid);dr.text((5,4),'All legal fascia joins',fill='#e4e2ed')
for i,(a,b) in enumerate(pairs):
 y=24+i*32;dr.text((5,y),a.replace('Fascia_','')+' > '+b.replace('Fascia_',''),fill='#e4e2ed');grid.alpha_composite(ims[a],(174,y));grid.alpha_composite(ims[b],(174+ims[a].width,y))
grid.save(V/'LegalPairs_1x.png');grid.resize((620,grid.height*2),Image.Resampling.NEAREST).save(V/'LegalPairs_2x.png')
for name in ['Normal','Contact']:
 crop=load(V/f'UnityFascia-{name}-1x.png').crop((0,12,360,230));crop.save(V/f'Unity_Top_{name}_1x.png');crop.resize((1080,654),Image.Resampling.NEAREST).save(V/f'Unity_Top_{name}_3x.png')
contact=Image.new('RGBA',(700,260),(10,13,17,255));dr=ImageDraw.Draw(contact);fg='#e4e2ed'
dr.text((10,8),'PixelLab source (Stage03)',fill=fg);contact.alpha_composite(load(R/'Sources/Stage04/Floor_Raw_PixelLab.png'),(10,30))
dr.text((10,170),'Side-view support seed',fill=fg)
seed=load(R/'Sources/Stage04/Floor_Raw_PixelLab.png').crop((96,23,112,28))
contact.alpha_composite(seed,(10,190));contact.alpha_composite(seed.resize((64,20),Image.Resampling.NEAREST),(40,184))
dr.text((180,8),'LibreSprite native front-face modules',fill=fg)
for i,n in enumerate(['Fascia_Left','Fascia_Mid_A','Fascia_Mid_B','Fascia_Anchor','Fascia_Right']):
 y=32+i*34;dr.text((180,y),n.replace('Fascia_','')+' '+str(ims[n].width)+'x24',fill=fg);contact.alpha_composite(ims[n],(310,y))
dr.text((520,8),'Rendering reference',fill=fg);contact.alpha_composite(load(P/'Docs/ArtDirection/Audit2026-10/media/skeleton-still.png'),(530,30))
dr.text((520,104),'Approved wall / unchanged',fill=fg);contact.alpha_composite(load(R/'Exports/Stage01/Wall_A_Quiet.png'),(530,124))
dr.text((180,212),'Continuous fascia: 360x24 / five opaque colors / binary alpha',fill=fg);contact.alpha_composite(fascia,(180,230))
contact.save(V/'Contact_1x.png');contact.resize((1400,520),Image.Resampling.NEAREST).save(V/'Contact_2x.png')
print(json.dumps(dict(assets=len(rows),palette_counts=[r['opaque_colors'] for r in rows],legal_pairs=len(pairs),screens=screens,protected_art=len(protected)),indent=2))
