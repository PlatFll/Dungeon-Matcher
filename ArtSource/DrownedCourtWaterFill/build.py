"""Exact native cleanup and LibreSprite round-trip; no approved scene art is edited."""
from pathlib import Path
from PIL import Image, ImageDraw
import hashlib, json, struct, subprocess, uuid, zlib

HERE=Path(__file__).resolve().parent
ROOT=HERE.parents[1]
OUT=HERE/'native'; OUT.mkdir(exist_ok=True)
LIBRE='C:/Users/USER/Downloads/libresprite-development-windows-x86_64/libresprite.exe'
frames=[]
for f in range(16):
    src=Image.open(HERE/'raw'/f'frame-{f:02}.png').convert('RGBA')
    dst=Image.new('RGBA',(128,64)); p=src.load(); q=dst.load()
    for x in range(128):
        ys=[y for y in range(12,32) if p[x,y][3] and max(p[x,y][:3])>120]
        crest=min(ys) if ys else 22
        # Quiet four-pixel edge collars join every animated repeat exactly.
        collar=min(x,127-x)
        shift=round((22-crest)*max(0,1-collar/8))
        edge=crest+shift
        for y in range(edge,64):
            q[x,y]=(12,90,101,64)
        for y in range(max(0,edge-5),min(52,64)):
            sy=y-shift
            if not 0<=sy<64: continue
            r,g,b,a=p[x,sy]
            if not a: continue
            light=max(r,g,b)
            if light>195: q[x,y]=(172,231,221,164)
            elif light>135: q[x,y]=(119,205,204,120)
        if collar<3:
            for y in range(64): q[x,y]=(0,0,0,0) if y<22 else ((119,205,204,120) if y==22 else (12,90,101,64))
    dst.save(OUT/f'Surface-{f:02}.png'); frames.append(dst)

def chunk(kind,data): return struct.pack('<IH',len(data)+6,kind)+data
name=b'Surface - native translucent composite'
layer=chunk(0x2004,struct.pack('<HHHHHHB',1,0,0,0,0,0,255)+bytes(3)+struct.pack('<H',len(name))+name)
parts=[]
for i,im in enumerate(frames):
    cel=chunk(0x2005,struct.pack('<HhhBH',0,0,0,255,2)+bytes(7)+struct.pack('<HH',128,64)+zlib.compress(im.tobytes()))
    chunks=(layer if i==0 else b'')+cel
    parts.append(struct.pack('<IHHHHI',16+len(chunks),0xF1FA,2 if i==0 else 1,83,0,0)+chunks)
header=bytearray(128)
struct.pack_into('<IHHHHHHI',header,0,128+sum(map(len,parts)),0xA5E0,16,128,64,32,1,83)
header[32]=0;header[34:36]=struct.pack('<H',256);header[36:38]=b'\x01\x01'
working=HERE/'WaterSurface-working.aseprite'; working.write_bytes(bytes(header)+b''.join(parts))
editable=HERE/'WaterSurface.aseprite'; sheet=HERE/'WaterSurface-strip.png'
subprocess.run([LIBRE,'--batch',str(working),'--save-as',str(editable),'--sheet-type','horizontal','--sheet',str(sheet),'--data',str(HERE/'WaterSurface.json')],check=True)
export=Image.open(sheet).convert('RGBA')
assert export.size==(2048,64),export.size
atlas=Image.new('RGBA',(512,272))
for i,im in enumerate(frames):
    assert export.crop((i*128,0,(i+1)*128,64)).tobytes()==im.tobytes()
    atlas.paste(im,((i%4)*128,(i//4)*64))
draw=ImageDraw.Draw(atlas);draw.rectangle((0,256,3,259),fill='white')
# Compact hollow rings, drawn at native resolution to accompany the generated surface.
draw.line([(10,258),(13,258),(15,260),(15,263),(13,265),(10,265),(8,263),(8,260),(10,258)],fill=(172,231,221,255))
draw.point((10,259),fill=(217,245,231,255))
draw.line([(25,258),(26,258),(27,259),(27,260),(26,261),(25,261),(24,260),(24,259),(25,258)],fill=(172,231,221,255))
target=ROOT/'Assets/_Game/Resources/UI/CourtFlood/WaterAtlas.png';atlas.save(target)
meta=target.with_suffix('.png.meta')
if not meta.exists():
    meta.write_text('fileFormatVersion: 2\nguid: '+uuid.uuid4().hex+'''\nTextureImporter:
  serializedVersion: 13
  mipmaps:
    enableMipMap: 0
    sRGBTexture: 1
  isReadable: 0
  maxTextureSize: 2048
  textureSettings:
    filterMode: 0
    aniso: 0
    wrapU: 1
    wrapV: 1
    wrapW: 1
  nPOTScale: 0
  textureType: 0
  textureShape: 1
  alphaUsage: 1
  alphaIsTransparency: 0
  textureCompression: 0
  platformSettings:
  - serializedVersion: 4
    buildTarget: DefaultTexturePlatform
    maxTextureSize: 2048
    textureFormat: -1
    textureCompression: 0
    overridden: 0
''')
frames[0].save(HERE/'WaterSurface-preview.gif',save_all=True,append_images=frames[1:],duration=80,loop=0,disposal=2)
report={'status':'candidate, not approved or merged','still_job':'57ec6823-93da-4385-8f58-a064b18cb119','still_index':3,
 'animation_job':'31e31e02-dea2-4c0f-bbb6-d6052a65031f','generations':12,'native':[128,64],'frames':16,'fps':12,
 'cleanup':'Remove duplicated last frame; exact teal composite ramp, native seam collars, discrete intentional transparency; no resampling.',
 'libresprite_roundtrip_exact':True,'source_reference_sha256':'705c4c8379d56a262e287d256664db8d180a79917f4a84289bbb813730b9616c',
 'alpha':sorted(set(atlas.getchannel('A').tobytes())), 'atlas_sha256':hashlib.sha256(target.read_bytes()).hexdigest()}
(HERE/'manifest.json').write_text(json.dumps(report,indent=2)+'\n')
print(json.dumps(report,indent=2))
