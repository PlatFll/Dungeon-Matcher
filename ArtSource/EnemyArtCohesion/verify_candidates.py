"""Verify native cels against PNG exports and measure all final canvas bounds."""
from pathlib import Path
from PIL import Image
import json,struct,zlib,hashlib
ROOT=Path(__file__).resolve().parents[2];BASE=Path(__file__).resolve().parent
jobs=json.loads((BASE/'jobs.json').read_text(encoding='utf-8'));report=[];errors=[]
for j in jobs:
 w,h=j['size'];sheet=BASE/'Sheets'/(j['stem']+'.png');im=Image.open(sheet).convert('RGBA')
 assert im.size==(w*len(j['sprites']),h),(j['stem'],'sheet size')
 assert im.width<=4096 and im.height<=4096,(j['stem'],'atlas would be rescaled')
 ase=(BASE/'Native'/(j['stem']+'.aseprite')).read_bytes();n,aw,ah,depth=struct.unpack_from('<4H',ase,6)
 assert (n,aw,ah,depth)==(len(j['sprites']),w,h,32),j['stem']
 offset=128;cels=[];rows=[]
 for f,row in enumerate(j['sprites']):
  length,magic,chunks,ms=struct.unpack_from('<IHHH',ase,offset);assert magic==0xf1fa and ms==row['duration'],(j['stem'],f,'duration')
  pos=offset+16;cel=None
  for c in range(chunks):
   size,typ=struct.unpack_from('<IH',ase,pos);payload=ase[pos+6:pos+size]
   if typ==0x2005:
    layer,x,y,opacity,kind=struct.unpack_from('<HhhBH',payload,0);assert layer==0 and opacity==255,(j['stem'],f,'layer')
    if kind==1:cel=cels[struct.unpack_from('<H',payload,16)[0]].copy()
    else:
     cw,ch=struct.unpack_from('<HH',payload,16);raw=payload[20:] if kind==0 else zlib.decompress(payload[20:]);assert kind in (0,2)
     patch=Image.frombytes('RGBA',(cw,ch),raw);cel=Image.new('RGBA',(w,h));cel.paste(patch,(x,y))
   pos+=size
  assert cel is not None,(j['stem'],f,'missing cel');cels.append(cel)
  exported=im.crop((f*w,0,(f+1)*w,h));assert exported.tobytes()==cel.tobytes(),(j['stem'],f,'native/export mismatch')
  alphas=set(exported.getchannel('A').get_flattened_data());assert alphas<={0,255},(j['stem'],f,'soft alpha')
  box=exported.getbbox();margins=[box[0],box[1],w-box[2],h-box[3]] if box else None
  if box and min(margins)<1:errors.append([j['stem'],f,margins])
  rows.append(dict(frame=f,states=row['states'],bounds=box,margins=margins))
  offset+=length
 report.append(dict(actor=j['actor'],stem=j['stem'],sha256=hashlib.sha256(sheet.read_bytes()).hexdigest(),native_sha256=hashlib.sha256(ase).hexdigest(),size=j['size'],frames=rows))
(BASE/'candidate-validation.json').write_text(json.dumps(dict(textures=len(jobs),sprites=sum(len(j['sprites']) for j in jobs),native_export_rgba_exact=True,binary_alpha=True,outer_edge_contacts=errors,sources=report),indent=2),encoding='utf-8')
print(json.dumps(dict(textures=len(jobs),sprites=sum(len(j['sprites']) for j in jobs),native_export_rgba_exact=True,binary_alpha=True,outer_edge_contacts=errors)))
assert not errors,'Review outer canvas contacts before import'
