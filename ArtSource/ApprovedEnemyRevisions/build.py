"""Native effect candidates; no generation or edits to approved source artwork."""
from pathlib import Path
import hashlib, json, subprocess, re
from PIL import Image, ImageDraw
ROOT=Path(__file__).resolve().parents[2]
HERE=Path(__file__).resolve().parent
OUT=ROOT/'Assets/_Game/Resources/Combat/ApprovedRevisions'
INK='#0A0D11'
def line(d,points,color,width=1): d.line(points,fill=color,width=width)
net=Image.new('RGBA',(32,32));d=ImageDraw.Draw(net)
cords=[[(5,7),(10,4),(18,5),(26,12),(27,20),(22,26),(14,27),(5,20),(4,12),(5,7)],
       [(7,9),(22,24)],[(10,6),(25,21)],[(6,20),(20,6)],[(9,25),(25,9)]]
for points in cords: line(d,points,INK,3)
for points in cords: line(d,points,'#42666b')
for points in [[(5,7),(10,4),(18,5)],[(4,12),(5,17)],[(7,9),(12,14)],[(6,20),(12,14)]]:line(d,points,'#7b9b97')
for x,y,dx,dy in [(5,8,-1,-1),(23,8,1,-1),(27,18,1,0),(22,26,1,1),(8,25,-1,1),(4,17,-1,0)]:
    d.polygon([(x-2,y),(x+1,y-2),(x+3*dx,y+3*dy),(x+1,y+2)],fill=INK)
    line(d,[(x,y),(x+2*dx,y+2*dy)],'#7c658f');d.point((x,y),'#b0a0b8')
for x,y in [(10,4),(26,12),(14,27)]:
    d.rectangle((x-1,y-1,x+1,y+1),fill=INK);d.point((x,y),'#a9bbb6')
ember=Image.new('RGBA',(32,32));d=ImageDraw.Draw(ember)
d.polygon([(5,21),(8,19),(24,19),(27,22),(25,28),(7,28),(4,25)],fill=INK)
d.polygon([(6,22),(10,20),(23,20),(25,22),(23,26),(8,26)],fill='#41475e')
for box in [(6,22,11,24),(13,20,19,22),(21,22,25,24)]:d.rectangle(box,fill='#788299')
line(d,[(7,26),(23,26)],'#242d40',2)
d.polygon([(11,22),(9,16),(12,12),(13,7),(17,3),(17,9),(21,12),(23,17),(20,23)],fill=INK)
d.polygon([(12,22),(11,16),(14,11),(15,7),(16,11),(20,13),(21,17),(19,22)],fill='#b85037')
d.polygon([(13,21),(12,17),(15,12),(15,16),(18,13),(20,17),(18,22)],fill='#ee9b43')
d.polygon([(14,21),(14,18),(16,15),(16,18),(18,18),(17,22)],fill='#ffe7a0')
line(d,[(10,24),(20,24)],'#b07b52')
line(d,[(11,22),(11,24)],INK);line(d,[(20,22),(20,24)],INK)
libre=Path('C:/Users/USER/Downloads/libresprite-development-windows-x86_64/libresprite.exe')
manifest={};template=(ROOT/'Assets/_Game/Art/DrownedCourt/thorny_snare.png.meta').read_text()
for name,im,guid in [('MarineSnare',net,'189bafbb194a47bf9d1be6cd04c88421'),('HearthEmber',ember,'695d03ce24364970b15ee3b9b6e16b92')]:
    raw=HERE/(name+'.png');im.save(raw)
    source=HERE/(name+'.ase');export=OUT/(name+'.png')
    subprocess.run([str(libre),'-b',str(raw),'--save-as',str(source)],check=True)
    subprocess.run([str(libre),'-b',str(source),'--save-as',str(export)],check=True)
    assert Image.open(export).convert('RGBA').tobytes()==im.tobytes()
    meta=re.sub(r'guid: [0-9a-f]+',f'guid: {guid}',template,count=1)
    # Single-sprite imports must not inherit another asset's sub-sprite IDs.
    meta=re.sub(r'^  internalIDToNameTable:\n.*?(?=^  externalObjects:)', '  internalIDToNameTable: []\n',meta,flags=re.S|re.M)
    meta=re.sub(r'^    sprites:\n.*?(?=^    outline:)', '    sprites: []\n', meta, flags=re.S|re.M)
    meta=re.sub(r'    spriteID: [0-9a-f]+',f'    spriteID: {guid}',meta)
    meta=re.sub(r'    nameFileIdTable:\n.*?  mipmapLimitGroupName:', '    nameFileIdTable: {}\n  mipmapLimitGroupName:',meta,flags=re.S)
    (OUT/(name+'.png.meta')).write_text('\n'.join(line.rstrip() for line in meta.splitlines())+'\n')
    colors=set(im.getdata());assert {c[3] for c in colors}<={0,255}
    manifest[name]={'size':im.size,'opaque_colors':len([c for c in colors if c[3]]),'bounds':im.getbbox(),
        'sha256':hashlib.sha256(export.read_bytes()).hexdigest(),'libresprite_roundtrip_pixels_equal':True}
refs={'Rattlebones':'Docs/ArtDirection/Audit2026-10/media/skeleton-still.png','Farmer':'Docs/ArtDirection/EnemyCohesion2026-10/farmer-still.png','Pan':'Docs/ArtDirection/EnemyCohesion2026-10/pan_villager-still.png'}
manifest['references']={n:{'path':p,'sha256':hashlib.sha256((ROOT/p).read_bytes()).hexdigest()} for n,p in refs.items()}
(HERE/'manifest.json').write_text(json.dumps(manifest,indent=2))
review=Image.new('RGBA',(640,230),'#202330');rd=ImageDraw.Draw(review)
for i,(label,im) in enumerate([('Old snare',Image.open(ROOT/'Assets/_Game/Art/DrownedCourt/thorny_snare.png').convert('RGBA')),('Marine net',net),('Hearth Ember',ember)]):
    x=15+210*i;rd.text((x,8),label,fill='white');review.alpha_composite(im,(x,30));review.alpha_composite(im.resize((160,160),Image.Resampling.NEAREST),(x+38,48))
review.save(HERE/'Review.png');print(json.dumps(manifest,indent=2))
