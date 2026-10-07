"""Native Court board objects. Direct pixel authoring, no resampling or AI generations."""
from pathlib import Path
from PIL import Image, ImageDraw
import json, hashlib

ROOT=Path(__file__).resolve().parents[1]
OUT=ROOT/'ArtSource/DrownedCourt/RosterRevision'
OUT.mkdir(parents=True,exist_ok=True)
# Shell ramp is sampled from the current 32px marine coffer. The new pearl ramp
# is a deliberate bright-pink magical accent requested for this revision.
ink='#0a0d11'; shell_dark='#264762'; shell_mid='#4f89ab'; shell_light='#d4d5ba'; white='#feffff'
pink_dark='#6d274d'; pink_mid='#ba4976'; pink='#f385bb'; pink_light='#ffdcf4'
assets={}
def pearl(canvas,box):
    d=ImageDraw.Draw(canvas);x,y,w,h=box
    d.ellipse((x,y,x+w-1,y+h-1),fill=ink)
    d.ellipse((x+1,y+1,x+w-2,y+h-2),fill=pink_dark)
    d.ellipse((x+1,y+1,x+w-3,y+h-4),fill=pink_mid)
    d.ellipse((x+2,y+1,x+w-4,y+h-6),fill=pink)
    d.ellipse((x+3,y+2,x+w//2+1,y+h//2-1),fill=pink_light)
    d.line((x+4,y+2,x+w//2,y+2),fill=white)
def plate(im,side):
    d=ImageDraw.Draw(im)
    def poly(points,color):d.polygon([(31-x if side else x,y) for x,y in points],fill=color)
    poly([(4,9),(7,7),(10,10),(11,14),(10,19),(12,24),(9,27),(5,24),(2,20),(2,14)],ink)
    poly([(5,10),(7,9),(9,12),(9,19),(10,23),(8,25),(5,22),(4,18),(4,13)],shell_dark)
    poly([(5,11),(7,10),(8,13),(7,18),(8,23),(6,21),(5,17)],shell_mid)
    poly([(5,11),(7,10),(7,13),(6,14),(5,18),(4,17),(4,14)],shell_light)
    poly([(5,11),(6,10),(6,12),(5,13)],white)
    poly([(8,18),(9,19),(10,23),(9,24),(8,22)],shell_light)

base=Image.new('RGBA',(32,32));pearl(base,(7,4,19,22));d=ImageDraw.Draw(base)
# A small open scalloped cradle leaves most of the pearl exposed at level one.
d.polygon([(5,22),(8,20),(11,24),(16,25),(21,23),(24,20),(27,22),(26,26),(22,28),(10,28),(6,26)],fill=ink)
d.polygon([(7,23),(8,22),(11,26),(16,27),(21,25),(24,22),(25,23),(24,25),(21,27),(11,27),(8,25)],fill=shell_mid)
d.line([(7,23),(10,25),(12,26),(17,26),(21,24),(24,22)],fill=shell_light,width=1)
d.point((8,22),fill=white);d.point((24,22),fill=white)
assets['PearlCoffer_L1']=base
armored=base.copy();plate(armored,False);plate(armored,True);assets['PearlCoffer_L2']=armored
exposed=Image.new('RGBA',(32,32));pearl(exposed,(7,4,19,22));assets['ExposedPearl']=exposed
orb=Image.new('RGBA',(12,12));pearl(orb,(1,1,10,10));assets['TributePearl']=orb
fragment=Image.new('RGBA',(8,8));d=ImageDraw.Draw(fragment)
d.polygon([(1,1),(4,0),(6,2),(5,5),(2,7),(0,4)],fill=ink)
d.polygon([(1,2),(4,1),(5,2),(3,5),(2,5)],fill=shell_mid)
d.line([(1,2),(3,1),(4,1),(3,3)],fill=shell_light,width=1);assets['ShellFragment']=fragment
burst=Image.new('RGBA',(12,12));d=ImageDraw.Draw(burst)
for a in [(5,0,6,3),(5,8,6,11),(0,5,3,6),(8,5,11,6)]:d.rectangle(a,fill=pink)
d.rectangle((4,4,7,7),fill=pink_light);assets['PearlPop']=burst
micro=Image.new('RGBA',(6,6));d=ImageDraw.Draw(micro)
d.line([(2,0),(4,0),(5,1),(5,3),(4,4),(2,4),(1,3),(1,1),(2,0)],fill=shell_mid,width=1)
d.line([(2,1),(3,0),(4,0)],fill=white,width=1);assets['WaterMicroBubble']=micro

records=[]
for name,im in assets.items():
    assert set(p[3] for p in im.getdata())<={0,255}
    path=OUT/(name+'.png');im.save(path)
    records.append(dict(name=name,native=list(im.size),colors=len({p for p in im.getdata() if p[3]}),
        alpha=[0,255],sha256=hashlib.sha256(path.read_bytes()).hexdigest()))
preview=Image.new('RGB',(896,260),'#242831');d=ImageDraw.Draw(preview)
for i,(name,im) in enumerate(assets.items()):
    x=i*128;d.text((x+4,10),name,fill='#fff1db')
    preview.paste(im,(x+48,48),im)
    zoom=im.resize((im.width*3,im.height*3),Image.Resampling.NEAREST)
    preview.paste(zoom,(x+(128-zoom.width)//2,110),zoom)
preview.save(OUT/'PearlReview.png')
(OUT/'pearls.json').write_text(json.dumps(dict(status='Review candidates; not user approved',method='Direct native pixel authoring; zero generations',assets=records),indent=2)+'\n')
print(json.dumps(records,indent=2))
