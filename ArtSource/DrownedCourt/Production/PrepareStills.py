from pathlib import Path
from PIL import Image
import json,hashlib
r=Path(__file__).resolve().parent
shared='0A0D11 4A2C1C 7A4D2E B07A43 8A5622 C58826 FBEA90 5B5145 9F907A E0D2B8 FDF5E5'
ramps={
'breakwater_captain':'2B3945 45596A 6B7D90 91ADB2 823F53 C66C73 F0B294 4B1127 7C1A30',
'conch_marshal':'823F53 C66C73 F0B294 4B1127 7C1A30 C74A5A',
'lantern_warden':'242D3D 4A3D68 796798 B9A7CF 3D5960 68878A AEC0B4',
'moray_siphoner':'34402A 647039 A1A261 D3CE8A 4B2432 783C4A A96365',
'needlefin_skirmisher':'2B3945 45596A 6B7D90 91ADB2 242D3D 47536A A96326 D68B3D',
'puffer_sentinel':'705B49 A99071 DEC9A4 A96326 D68B3D F0BC7C 823F53 C66C73',
'queen_nacre':'4B1127 7C1A30 C74A5A 823F53 C66C73 F0B294',
'reef_netweaver':'242D3D 47536A 6B7D90 91ADB2 B8C8BE',
'shellback_porter':'713F32 A75B44 D9825C F0B294 2B313A 55606E 93A1B0',
'skittercrab':'705B49 A99071 DEC9A4 823F53 C66C73 F0B294'}
(r/'Selected').mkdir(exist_ok=True);report=[]
for name,colors in ramps.items():
 raw=Image.open(r/'Raw'/(name+'.png')).convert('RGBA');im=raw.copy()
 palette=[tuple(bytes.fromhex(h)) for h in (shared+' '+colors).split()]
 mapping={}
 for p in raw.get_flattened_data():
  if not p[3] or p[:3] in mapping:continue
  mapping[p[:3]]=palette[0] if max(p[:3])<48 else min(palette,key=lambda c:sum((c[k]-p[k])**2 for k in range(3)))
 im.putdata([mapping[p[:3]]+(255,) if p[3] else (0,0,0,0) for p in raw.get_flattened_data()])
 edits=[]
 if name=='breakwater_captain':
  for xy in [(31,16),(31,17)]:im.putpixel(xy,(10,13,17,255));edits.append(xy)
 assert im.getchannel('A').tobytes()==raw.getchannel('A').tobytes()
 # Add transparent motion room without scaling. Baseline is the last source row.
 width=96 if raw.width==96 else 80;height=96 if raw.height==96 else 80
 canvas=Image.new('RGBA',(width,height));bbox=im.getbbox();offset=((width-raw.width)//2,height-1-bbox[3])
 canvas.paste(im,offset)
 canvas.save(r/'Selected'/(name+'.png'))
 report.append(dict(id=name,status='director-selected under authorization to complete production; user has not individually reviewed this PNG',source_size=raw.size,output_size=canvas.size,operation='native material color mapping, solid-eye correction, transparent canvas padding/translation only; no scaling',offset=offset,eye_pixels=edits,colors=len(set(p[:3] for p in canvas.get_flattened_data() if p[3])),alpha=[0,255],mapping={'#%02X%02X%02X'%k:'#%02X%02X%02X'%v for k,v in mapping.items()},sha256=hashlib.sha256((r/'Selected'/(name+'.png')).read_bytes()).hexdigest()))
approved={'reef_spearman':'DC_A01_Reef_Spearman','hammerhead_bruiser':'DC_A02_Hammerhead_Bruiser','pearl_thief':'DC_A03_Pearl_Thief','pearl_cantor':'DC_A04_Pearl_Cantor'}
for name,file in approved.items():
 im=Image.open(r.parent/'Approved'/(file+'.png')).convert('RGBA');canvas=Image.new('RGBA',(80,80));bbox=im.getbbox();offset=(8,79-bbox[3]);canvas.paste(im,offset);canvas.save(r/'Selected'/(name+'.png'))
 report.append(dict(id=name,status='user-approved, solid eyes corrected',source_size=im.size,output_size=canvas.size,offset=offset,operation='transparent canvas padding/translation only; native colors, face, proportions and equipment unchanged'))
(r/'Selected/StillManifest.json').write_text(json.dumps(report,indent=2))
print('Prepared 14 native stills; no generation calls or resizing.')
