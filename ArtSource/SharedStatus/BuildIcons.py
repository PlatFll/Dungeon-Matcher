"""Hand-authored 16px status glyphs. No resampling, generation service or antialiasing."""
from pathlib import Path
from PIL import Image, ImageDraw, ImageFilter
import json, re, uuid

ROOT=Path(__file__).resolve().parents[2]
OUT=ROOT/'Assets/_Game/Resources/UI/PlayerStatuses'
OUT.mkdir(parents=True,exist_ok=True)
DARK=(10,13,17,255)
COLORS={
    'Weakened':('#91a9b9','#d9e2e4','#536775'), 'Burn':('#df743b','#f6d46a','#963f36'),
    'Sapped':('#9872b5','#dac2ed','#514263'), 'Wounded':('#c25968','#f0a39e','#783342'),
    'Fear':('#d6bc76','#f4e2ad','#806a48'), 'Frostbite':('#76c9db','#d9f2ef','#377590'),
    'Slippery':('#58b9a8','#b8e7d1','#316968')}
def meta(path,body=''):
    p=Path(str(path)+'.meta')
    if p.exists(): return re.search(r'guid: (\w+)',p.read_text()).group(1)
    guid=uuid.uuid4().hex;p.write_text('fileFormatVersion: 2\nguid: '+guid+'\n'+body)
    return guid
meta(OUT,'folderAsset: yes\nDefaultImporter:\n  externalObjects: {}\n')
checks=[];sheet=Image.new('RGBA',(16*7,16),(67,69,76,255))
for n,(name,(mid,light,shade)) in enumerate(COLORS.items()):
    im=Image.new('RGBA',(16,16));d=ImageDraw.Draw(im)
    if name=='Weakened':
        d.polygon([(3,11),(5,9),(7,11),(5,13)],fill=shade)
        d.line([(3,8),(7,12)],fill=mid,width=2)
        d.polygon([(6,7),(10,3),(12,2),(12,5),(9,8)],fill=mid)
        d.line([(8,6),(11,3)],fill=light,width=1)
        d.line([(5,9),(7,7)],fill=light,width=1)
    elif name=='Burn':
        d.polygon([(3,11),(3,8),(5,5),(6,8),(8,2),(10,5),(10,8),(12,6),(13,10),(11,13),(5,13)],fill=mid)
        d.polygon([(6,11),(8,7),(9,9),(10,10),(9,13),(7,13)],fill=light)
        d.line([(4,10),(5,12),(6,13)],fill=shade,width=1)
    elif name=='Sapped':
        d.polygon([(8,2),(12,6),(8,10),(4,6)],fill=mid)
        d.polygon([(8,3),(10,6),(8,7),(6,6)],fill=light)
        d.line([(8,10),(8,13)],fill=shade,width=2)
        d.line([(5,11),(8,14),(11,11)],fill=mid,width=1)
    elif name=='Wounded':
        d.polygon([(3,4),(6,3),(8,5),(10,3),(13,4),(13,7),(8,13),(3,7)],fill=mid)
        d.line([(4,5),(6,4)],fill=light,width=1)
        d.line([(5,11),(11,4)],fill=DARK,width=3)
        d.line([(5,11),(11,4)],fill=light,width=1)
    elif name=='Fear':
        d.polygon([(2,8),(5,5),(11,5),(14,8),(11,11),(5,11)],fill=mid)
        d.line([(5,6),(10,6)],fill=light,width=1)
        d.rectangle((7,6,9,10),fill=shade);d.rectangle((8,6,8,10),fill=DARK)
        d.line([(3,3),(4,2)],fill=light);d.line([(12,2),(13,3)],fill=light)
    elif name=='Frostbite':
        d.line([(8,2),(8,13)],fill=light,width=2)
        d.line([(3,5),(12,11)],fill=mid,width=2);d.line([(3,11),(12,5)],fill=mid,width=2)
        d.line([(5,2),(8,4),(11,2)],fill=light)
        d.line([(3,4),(3,7),(5,7)],fill=light);d.line([(11,8),(13,8),(13,12)],fill=shade)
        d.polygon([(8,6),(10,8),(8,10),(6,8)],fill=light)
    else:
        d.polygon([(3,3),(8,3),(8,6),(11,6),(11,4),(14,7),(11,10),(11,8),(6,8),(6,5),(3,5)],fill=mid)
        d.line([(3,3),(7,3)],fill=light)
        d.line([(2,10),(5,10),(6,11),(9,11)],fill=light)
        d.line([(4,13),(7,13),(8,14),(12,14)],fill=shade)
    alpha=im.getchannel('A');outline=alpha.filter(ImageFilter.MaxFilter(3))
    finished=Image.new('RGBA',(16,16),DARK);finished.putalpha(outline);finished.alpha_composite(im)
    p=OUT/(name+'.png');finished.save(p)
    guid=meta(p,'''TextureImporter:
  externalObjects: {}
  mipmaps:
    enableMipMap: 0
  isReadable: 0
  textureSettings:
    filterMode: 0
    aniso: 0
    mipBias: 0
    wrapU: 1
    wrapV: 1
  npotScale: 0
  textureFormat: 1
  maxTextureSize: 2048
  spriteMode: 1
  spritePixelsToUnits: 100
  spriteMeshType: 0
  spritePivot: {x: 0.5, y: 0.5}
  alphaIsTransparency: 1
  textureType: 8
  textureCompression: 0
''')
    data=ROOT/'Assets/_Game/Resources/PlayerStatuses'/(name+'.asset')
    data.write_text(re.sub(r'  icon: .*',f'  icon: {{fileID: 21300000, guid: {guid}, type: 3}}',data.read_text()))
    colors=set(finished.getdata());assert set(finished.getchannel('A').getdata())=={0,255}
    checks.append(dict(name=name,width=16,height=16,opaquePalette=len({c for c in colors if c[3]}),alpha=[0,255]))
    sheet.alpha_composite(finished,(n*16,0))
review=Path(__file__).parent;sheet.convert('RGB').save(review/'NativeIcons.png')
(review/'NativeChecks.json').write_text(json.dumps(checks,indent=2))
print('Created seven native 16x16 icons with binary alpha.')
