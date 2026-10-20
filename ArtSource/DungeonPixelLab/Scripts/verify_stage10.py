from pathlib import Path
from PIL import Image,ImageDraw
import json,hashlib,struct,zlib,xml.etree.ElementTree as ET
P=Path(__file__).resolve().parents[3];R=P/'ArtSource/DungeonPixelLab';D=R/'Diagnostics/Stage10';V=R/'Previews/Stage10'
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
sizes={'Portrait_Rail_Top':(24,8),'Portrait_Rail_Bottom':(24,11),'Portrait_Rail_Left':(9,20),'Portrait_Rail_Right':(10,20),'Portrait_Corner_TL':(14,14),'Portrait_Corner_TR':(13,14),'Portrait_Corner_BL':(14,16),'Portrait_Corner_BR':(13,16),'Portrait_Clamp':(8,8),'Portrait_HP_Shell':(64,10),'Portrait_Heart_Existing':(13,15)}
ims={n:load(R/'Exports/Stage10'/f'{n}.png') for n in sizes};rows=[]
for n,im in ims.items():
 f=R/'Exports/Stage10'/f'{n}.png';native=R/'Native/Stage10'/f'{n}.aseprite'
 assert im.size==sizes[n] and ase(native).tobytes()==im.tobytes(),n
 assert set(im.getchannel('A').get_flattened_data())=={0,255},n
 assert sha(f)==sha(P/'Assets/_Game/Art/DungeonPixelLab/PortraitFrame'/f.name),n
 cols=sorted(set(c[:3] for c in pixels(im) if c[3]))
 rows.append(dict(name=n,dimensions=list(im.size),bounds=im.getbbox(),opaque_colors=len(cols),palette=['#%02X%02X%02X'%c for c in cols],alpha=[0,255],sha256=sha(f),ase_sha256=sha(native),native_cel_equals_png=True,status='CANDIDATE_PENDING_USER_REVIEW' if 'Existing' not in n else 'EXISTING_ICON_UNCHANGED'))
assert ims['Portrait_Heart_Existing'].tobytes()==load(P/'Assets/_Game/Art/DungeonEnvironment/SideViewRebuild/Hud/PlayerHeart.png').tobytes()
for n in ['Portrait_Rail_Top','Portrait_Rail_Bottom']:
 a=ims[n];assert a.crop((0,0,1,a.height)).tobytes()==a.crop((a.width-1,0,a.width,a.height)).tobytes()
for n in ['Portrait_Rail_Left','Portrait_Rail_Right']:
 a=ims[n];assert a.crop((0,0,a.width,1)).tobytes()==a.crop((0,a.height-1,a.width,a.height)).tobytes()
protected=json.loads((D/'ProtectedArtHashes.json').read_text())
assert all(sha(P/f)==h for f,h in protected.items()),'Protected previous art changed'
reference=json.loads((R/'Progress/Stage10_Reference_Audit.json').read_text())
for f,k in [('Docs/ArtDirection/Dungeon_Matcher_Art_Direction.txt','guide_sha256'),('Docs/ArtDirection/Golden_Reference_Register.md','register_sha256')]:assert sha(P/f)==reference[k]
for r in reference['references']:
 assert sha(P/r['path'])==r['sha256']
 if 'production_path' in r:assert sha(P/r['production_path'])==r['production_sha256']
m=json.loads((R/'Progress/Stage10PortraitPlacement.json').read_text());prior=json.loads((R/'Progress/Stage09ClutterPlacement.json').read_text())
nonframe=[p for p in prior['placements'] if not p['name'].startswith('Player')]
assert m['placements'][:210]==nonframe and len(m['placements'])==250
def module(n):
 family=next((i for prefix,i in [('Wall_',1),('Recess_',2),('Floor_',3),('Fascia_',4),('Gate_',5),('Support_',6),('Torch_',7),('Hanging_',8),('Clutter_',9),('Portrait_',10)] if n.startswith(prefix)),None)
 return load(R/f'Exports/Stage{family:02}'/(n+'.png')) if family else load(P/'Assets/_Game/Art/DungeonEnvironment/SideViewRebuild/Frames_Player'/(n+'.png'))
def compose(placements):
 out=Image.new('RGBA',(360,640))
 for p in sorted(placements,key=lambda p:p['layer']):
  im=module(p['name']);assert p['x']>=0 and p['y']>=0 and p['x']+im.width<=360 and p['y']+im.height<=640
  out.alpha_composite(im,(p['x'],p['y']))
 return out
expected=compose(m['placements']);before=compose(prior['placements'])
assert expected.crop((108,0,360,640)).tobytes()==before.crop((108,0,360,640)).tobytes()
assert expected.crop((0,210,360,640)).tobytes()==before.crop((0,210,360,640)).tobytes()
baseline=compose([p for p in m['placements'] if p['name']!='Recess_Inset'])
full=expected.copy();rattle=load(P/'Docs/ArtDirection/Audit2026-10/media/skeleton-still.png');full.alpha_composite(rattle,(26,116))
ImageDraw.Draw(full).rectangle((29,188,88,191),fill=(198,38,91,255))
half=expected.copy();half.alpha_composite(rattle,(26,116));ImageDraw.Draw(half).rectangle((29,188,58,191),fill=(198,38,91,255))
screens=[];spill=[]
for scale in [1,3]:
 for label,ex in [('Normal',expected),('Baseline',baseline),('PortraitFull',full),('PortraitHalf',half),('Restored',expected)]:
  shot=load(V/f'UnityPortrait-{label}-{scale}x.png');pred=ex.resize((360*scale,640*scale),Image.Resampling.NEAREST)
  assert shot.size==pred.size
  diff=sum(a!=b for a,b in zip(pixels(shot),pixels(pred)));assert diff==0,(label,scale,diff)
  screens.append(dict(label=label,scale=scale,rgba_differences=diff))
 base=load(V/f'UnityPortrait-Baseline-{scale}x.png');masked=load(V/f'UnityPortrait-MaskedProbe-{scale}x.png');unmasked=load(V/f'UnityPortrait-UnmaskedControl-{scale}x.png')
 counts={'masked_outside':0,'masked_inside':0,'unmasked_outside':0}
 for y in range(base.height):
  for x in range(base.width):
   inside=18*scale<=x<98*scale and 30*scale<=y<197*scale
   if base.getpixel((x,y))!=masked.getpixel((x,y)):counts['masked_inside' if inside else 'masked_outside']+=1
   if not inside and base.getpixel((x,y))!=unmasked.getpixel((x,y)):counts['unmasked_outside']+=1
 assert counts['masked_outside']==0 and counts['masked_inside']>10000 and counts['unmasked_outside']>10000
 spill.append(dict(scale=scale,**counts))
test=ET.parse(D/'UnityTests.xml').getroot();assert test.attrib['result']=='Passed' and int(test.attrib['failed'])==0
checks=dict(assets=rows,raw_sha256=sha(R/'Sources/Stage10/Frame_Raw_PixelLab.png'),prior_art_files_preserved=len(protected),prior_non_frame_placements_preserved=210,frame_placements=40,total_placements=250,all_native_cels_equal_independent_exports=True,all_alpha_binary=True,repeat_rail_opposite_edges_identical=True,heart_original_RGBA_unchanged=True,portrait_recess=[18,30,80,167],oversized_probe=[160,247],spill_checks=spill,screens=screens,unity_test={k:test.attrib[k] for k in ['result','passed','failed']},production_scene_and_build_settings='Unchanged; tested',native_scaling='No resampling. 22 rails repeat at authored native dimensions.',HP='Empty shell and separate existing heart. Diagnostic live-width fill rendered at full/half; no values or fill baked into art.',reference_hashes_unchanged=True,coverage_limit='Isolated Unity preview, not production gameplay or device testing.')
(D/'PixelChecks.json').write_text(json.dumps(checks,indent=2)+'\n')
for label in ['Normal','PortraitFull','PortraitHalf','MaskedProbe','UnmaskedControl']:
 shot=load(V/f'UnityPortrait-{label}-1x.png');crop=shot.crop((0,0,360,242));crop.save(V/f'Top_{label}_1x.png');crop.resize((1080,726),Image.Resampling.NEAREST).save(V/f'Top_{label}_3x.png')
before.crop((0,0,360,242)).save(V/'Before_Top_1x.png')
board=Image.new('RGBA',(740,270),(10,13,17,255));dr=ImageDraw.Draw(board);dr.text((8,8),'Previous frame / restored raw bones',fill='white');dr.text((380,8),'Stage 10 / actual Unity',fill='white');board.alpha_composite(before.crop((0,0,360,242)),(8,24));board.alpha_composite(expected.crop((0,0,360,242)),(380,24));board.save(V/'Comparison_1x.png')
board=Image.new('RGBA',(780,232),(20,22,32,255));dr=ImageDraw.Draw(board)
dr.text((12,10),'Raw PixelLab',fill='white');board.alpha_composite(load(R/'Sources/Stage10/Frame_Raw_PixelLab.png'),(12,32))
dr.text((102,10),'Native modular kit',fill='white')
xx=102;yy=32
for n in sizes:
 im=ims[n];board.alpha_composite(im,(xx,yy));dr.text((xx,yy+im.height+3),n.replace('Portrait_','').replace('Existing',''),fill='#cfc9df');yy+=im.height+22
 if yy>160:yy=32;xx+=120
dr.text((440,10),'Actual Unity: unchanged source at 1x',fill='white');board.alpha_composite(full.crop((8,19,109,211)),(440,32));board.alpha_composite(rattle,(650,110));dr.text((640,90),'Golden source',fill='white')
board.save(V/'Contact_1x.png');board.resize((1560,464),Image.Resampling.NEAREST).save(V/'Contact_2x.png')
rawbones=load(R/'Sources/Stage09/Bones_Raw_PixelLab.png');fat=Image.new('RGBA',(26,12));fat.paste(rawbones.crop((30,38,55,50)),(1,0))
boneboard=Image.new('RGBA',(480,94),(20,22,32,255));dr=ImageDraw.Draw(boneboard)
for x,label,im in [(10,'Raw shape',fat),(170,'Rejected thin',load(R/'Diagnostics/Stage09BoneCorrection/Rejected_Thin_Bones.png')),(330,'Restored + recolored',load(R/'Exports/Stage09/Clutter_Bones.png'))]:
 dr.text((x,8),label,fill='white');boneboard.alpha_composite(im.resize((104,48),Image.Resampling.NEAREST),(x,30))
boneboard.save(V/'Bones_Correction_4x.png')
print(json.dumps(dict(assets=len(rows),protected=len(protected),exact_Unity_captures=len(screens),spill=spill,palette_counts=[r['opaque_colors'] for r in rows],test=checks['unity_test'])))
