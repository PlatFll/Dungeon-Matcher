"""Checkpoint-one composition study. No Unity assets or scene are modified.

Pillow prepares a deterministic native-pixel plan; LibreSprite applies and
exports every edited pixel. Separate studies remain candidates, not imports.
"""
from pathlib import Path
from PIL import Image, ImageDraw, ImageChops, ImageFont
import hashlib, json, shutil, sys, math

P=Path(sys.argv[1])
O=P/'ArtSource/ForestReworkV2/Checkpoint01'
for d in ['Native','Previews','Scripts','Diagnostics']:(O/d).mkdir(parents=True,exist_ok=True)
scratch=P/'.utmp/forest-v2-direction'
scratch.mkdir(parents=True,exist_ok=True)
old=P/'ArtSource/ForestHybrid/Stage05/Previews'
A=P/'Assets/_Game/Art/ForestHybrid'
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
references=['Docs/ArtDirection/Dungeon_Matcher_Art_Direction.txt','Docs/ArtDirection/Golden_Reference_Register.md','Docs/ArtDirection/Audit2026-10/media/skeleton-still.png','Docs/ArtDirection/EnemyCohesion2026-10/farmer-still.png','Docs/ArtDirection/EnemyCohesion2026-10/pan_villager-still.png','Assets/_Game/Art/CombatIdles/Rattlebones_Idle.png','ArtSource/ForestHybrid/Stage05/Previews/Unity-Full-720x1360.png','ArtSource/ForestHybrid/Pack/references/01_forest_mockup_reference.png','ArtSource/ForestHybrid/Pack/references/02_current_dungeon_zone_reference.png']
reference_hashes={p:sha(P/p) for p in references}
(O/'Diagnostics/ReferenceHashes.json').write_text(json.dumps(reference_hashes,indent=2))
# Whole prior source history and all production imports remain immutable.
protected=[p for folder in ['Assets','ProjectSettings','ArtSource/ForestHybrid','Docs/ArtDirection'] for p in (P/folder).rglob('*') if p.is_file()]
protected_hashes={p.relative_to(P).as_posix():sha(p) for p in protected}
(O/'Diagnostics/ProtectedHashes.json').write_text(json.dumps(protected_hashes,indent=2))
original=Image.open(old/'Unity-Full-720x1360.png').convert('RGBA')
structure=Image.open(old/'Unity-Structure-720x1360.png').convert('RGBA')
study=original.copy()
def load(stage,name):return Image.open(A/stage/(name+'.png')).convert('RGBA')
def put(dst,im,x,y,scale=1):
    if scale!=1:im=im.resize((im.width*scale,im.height*scale),Image.Resampling.NEAREST)
    dst.alpha_composite(im,(x,y))
def tile(dst,im,box,phase=(0,0)):
    x0,y0,x1,y1=box
    area=Image.new('RGBA',(x1-x0,y1-y0))
    for y in range(-phase[1],area.height,im.height):
        for x in range(-phase[0],area.width,im.width):area.alpha_composite(im,(x,y))
    dst.paste(area,(x0,y0))

# A small material/value study on copies of existing far layers, never a new
# flattened background source for Unity. Existing tree/floor geometry is retained.
mapping={
 '#142c35':'#12232f','#1f4148':'#1b3543','#2b5459':'#284c5b',
 '#3b6469':'#366575','#51787d':'#477e89','#6b8c8b':'#63979a',
 '#91aaa4':'#90bcb4','#70846a':'#50765f','#4d6a60':'#3d6153'}
rgb=lambda h:tuple(bytes.fromhex(h.lstrip('#')))
mapping={rgb(k):rgb(v) for k,v in mapping.items()}
top=Image.new('RGBA',(360,240),(18,35,47,255))
placements=json.loads((P/'ArtSource/ForestHybrid/Stage01/Placement.json').read_text())['placements']
for p in sorted(placements,key=lambda p:p['layer']):
    im=Image.open(P/p['path']).convert('RGBA')
    if p['name'].startswith(('Far_','Canopy_','Understory')):
        im.putdata([(*mapping.get(c[:3],c[:3]),c[3]) for c in im.getdata()])
    put(top,im,p['x'],p['y'])
# Restrained far magic: two low-contrast, connected light shapes above the
# actor heads. No glow filter, gradients, particles across the stage or board.
d=ImageDraw.Draw(top)
for x,y in [(203,106),(272,90)]:
    d.polygon([(x-3,y),(x,y-3),(x+3,y),(x,y+4)],fill='#315665')
    d.line([(x-1,y),(x,y-2),(x+1,y)],fill='#629da3',width=1)
    d.point((x,y),fill='#a5c8b8')
for x,y in [(110,52),(323,57)]:put(top,load('Stage04','Lantern_Hanging'),x,y)
study.paste(top.resize((720,480),Image.Resampling.NEAREST),(0,0))
# UI and portrait remain exact screenshot samples; this is direction review,
# not an altered layout or falsely labelled in-game capture.
protected_boxes=[(16,16,220,456),(242,20,462,120),(484,28,624,108),(636,28,708,108)]
for box in protected_boxes:study.paste(original.crop(box),box[:2])
# Preserve every current actor, HP and countdown pixel from the original Unity
# on/off capture pair. This includes all pixels whose colour differs.
op,sp,tp=original.load(),structure.load(),study.load()
for y in range(480):
    for x in range(720):
        if op[x,y]!=sp[x,y]:tp[x,y]=op[x,y]

# Board composition study at 360x680 native environmental scale, applied at2x.
shell=Image.new('RGBA',(360,304))
tile(shell,load('Stage02','Portrait_Inset'),(0,0,360,304),phase=(0,16))
wood=Image.new('RGBA',(360,304))
d=ImageDraw.Draw(wood)
ink='#172326'; shadow='#414035'; base='#69523a'; light='#8a6845'; shine='#b3915f'
def beam(box):
    x0,y0,x1,y1=box
    d.rectangle(box,fill=ink)
    d.rectangle((x0+1,y0+1,x1-1,y1-1),fill=shadow)
    d.rectangle((x0+3,y0+3,x1-3,y1-3),fill=base)
    d.line([(x0+2,y1-2),(x0+2,y0+2),(x1-2,y0+2)],fill=light,width=1)
    d.line([(x1-2,y0+3),(x1-2,y1-2),(x0+3,y1-2)],fill=shadow,width=1)
# Coordinates relative to screen y=240; clear 7px native gap before cells.
beam((32,7,45,297));beam((315,7,328,297))
beam((32,7,328,20));beam((32,284,328,297))
# Quiet diagonal mortise joints, without spiral medals or clamps.
for x,y,sx,sy in [(32,7,1,1),(328,7,-1,1),(32,297,1,-1),(328,297,-1,-1)]:
    d.line([(x+sx,y+sy),(x+11*sx,y+11*sy)],fill=ink,width=1)
    d.line([(x+2*sx,y+sy),(x+11*sx,y+10*sy)],fill=light,width=1)
# Three long quiet grain strokes per long rail, not a repeated speckle texture.
for x in [39,321]:
    for y,length in [(43,42),(132,57),(221,35)]:d.line([(x,y),(x,y+length)],fill=shadow)
for y in [14,291]:
    for x,length in [(94,49),(196,62)]:d.line([(x,y),(x+length,y)],fill=shadow)
shell.alpha_composite(wood)

# Two deliberately connected foliage sweeps. These are separate native motif
# studies, not production assets. Each leaf has one broad shadow and light plane.
leaf=Image.new('RGBA',(64,64))
ld=ImageDraw.Draw(leaf)
ld.line([(12,60),(14,43),(17,29),(25,18),(40,13),(59,12)],fill=ink,width=4)
ld.line([(12,60),(14,43),(17,29),(25,18),(40,13),(59,12)],fill='#3d6153',width=2)
def blade(cx,cy,dx,dy):
    # A pointed bent leaf, 13-19px long, with a readable asymmetric silhouette.
    length=math.hypot(dx,dy); dx,dy=dx/length,dy/length
    vx,vy=-dy,dx
    size=12 if cx==35 else 16
    pts=[(cx,cy),(cx+dx*4+vx*5,cy+dy*4+vy*5),(cx+dx*10+vx*5,cy+dy*10+vy*5),(cx+dx*size,cy+dy*size),(cx+dx*10-vx*5,cy+dy*10-vy*5),(cx+dx*4-vx*4,cy+dy*4-vy*4)]
    ld.polygon(pts,fill=ink)
    pts2=[(cx+dx,cy+dy),(cx+dx*4+vx*4,cy+dy*4+vy*4),(cx+dx*10+vx*4,cy+dy*10+vy*4),(cx+dx*(size-2),cy+dy*(size-2)),(cx+dx*10-vx*4,cy+dy*10-vy*4),(cx+dx*4-vx*3,cy+dy*4-vy*3)]
    ld.polygon(pts2,fill='#466a45')
    ld.polygon([(cx+dx*2,cy+dy*2),(cx+dx*4+vx*4,cy+dy*4+vy*4),(cx+dx*10+vx*4,cy+dy*10+vy*4),(cx+dx*(size-2),cy+dy*(size-2))],fill='#78905b')
    ld.line([(cx+dx*3,cy+dy*3),(cx+dx*10,cy+dy*10)],fill='#365a43')
for a in [(17,35,-.8,-.4),(24,19,-.5,-.85),(35,16,.1,-.65),(43,13,.7,-.4),(14,49,.5,-.65),(23,21,.7,-.25)]:blade(*a)
leaf2=leaf.transpose(Image.Transpose.ROTATE_180)
put(shell,leaf,13,0)
put(shell,leaf2,283,240)
study.paste(shell.resize((720,608),Image.Resampling.NEAREST),(0,480))
# Locked cells/gems are pasted pixel-for-pixel, no downsample or recreation.
board_box=(104,528,616,1040)
study.paste(original.crop(board_box),board_box[:2])
# Preserve the entire lower HUD and all surrounding background exactly.
study.paste(original.crop((0,1088,720,1360)),(0,1088))

items={'DirectionStudy':study,'LeafSweep_A':leaf,'LeafSweep_B':leaf2,'TimberJoinStudy':wood.crop((29,4,50,25))}
plans=[]
for name,im in items.items():
    Image.new('RGBA',im.size).save(scratch/(name+'.png'))
    rows=[]
    for y in range(im.height):
        row=[];x=0
        while x<im.width:
            c=im.getpixel((x,y));end=x+1
            while end<im.width and im.getpixel((end,y))==c:end+=1
            if c[3]:row.append([x,end-x,bytes(c).hex()])
            x=end
        rows.append(row)
    plans.append({'name':name,'width':im.width,'height':im.height,'rows':rows})
    im.save(scratch/(name+'-expected.png'))
plan=json.dumps(plans,separators=(',',':'))
(O/'Scripts/NativePixelPlan.json').write_text(plan)
script='var items='+plan+';\nvar pc=app.pixelColor;\n'
script+='''items.forEach(function(a){
var s=app.open('.utmp/forest-v2-direction/'+a.name+'.png').sprite;
var im=s.layer(0).cel(0).image;
a.rows.forEach(function(row,y){row.forEach(function(r){var h=r[2];var c=pc.rgba(parseInt(h.substr(0,2),16),parseInt(h.substr(2,2),16),parseInt(h.substr(4,2),16),parseInt(h.substr(6,2),16));for(var x=r[0];x<r[0]+r[1];x++)im.putPixel(x,y,c);});});
s.layer(0).name=a.name+' - direction study, not a production import';s.commit();
s.saveAs('ArtSource/ForestReworkV2/Checkpoint01/Native/'+a.name+'.aseprite',true);
s.saveAs('ArtSource/ForestReworkV2/Checkpoint01/Previews/'+a.name+'.png',true);
});'''
(O/'Scripts/AuthorDirection.js').write_text(script)
(O/'Scripts/ReopenDirection.js').write_text('var names='+json.dumps(list(items))+';names.forEach(function(n){var s=app.open("ArtSource/ForestReworkV2/Checkpoint01/Native/"+n+".aseprite").sprite;s.saveAs(".utmp/forest-v2-direction/"+n+"-reopened.png",true);});')
shutil.copy2(__file__,O/'Scripts/PrepareDirection.py')
shutil.copy2('C:/Users/USER/.codex/attachments/4334defd-ad33-49f5-9232-d26abc4eb868/Pasted text.txt',O/'UserBrief.txt')
print(json.dumps({'study_dimensions':study.size,'protected_files':len(protected_hashes),'new_native_studies':list(items),'new_generations':0,'scope':'Checkpoint 1 direction only; no Unity changes'}))
