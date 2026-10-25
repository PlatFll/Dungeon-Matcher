from pathlib import Path
from PIL import Image,ImageDraw,ImageFont
import hashlib,json
F=Path(r'C:/Users/USER/.codex/worktrees/drowned-court-pixellab-v2/Dungeon Matcher');R=F/'ArtSource/DrownedCourtEnvironmentV2';E=R/'Exports/Stage06';P=R/'Preview/Stage06'
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
assets=[]
for p in sorted(E.glob('*.png')):
 im=Image.open(p).convert('RGBA');assets.append(dict(name=p.stem,width=im.width,height=im.height,colors=len(set(c[:3] for c in im.get_flattened_data() if c[3])),alpha=sorted(set(im.getchannel('A').get_flattened_data())),bbox=list(im.getbbox()),sha256=sha(p)))
placements=[]
def place(n,x,y,role='structure',attachment=None):
 a=next(a for a in assets if a['name']==n);placements.append(dict(name=n,x=x,y=y,width=a['width'],height=a['height'],role=role,attachment=attachment))
for x,sequence in [(0,'ABACABAC'),(329,'ACABAABC')]:
 place('ColumnCap',x,243)
 for i,v in enumerate(sequence):place('ColumnShaft'+v,x,259+i*32)
 place('ColumnShaftShort',x,515);place('ColumnBase',x,533)
for n,x,y in [('ShelfLeft',0,338),('ShelfRight',0,468),('ShelfLeft',329,338),('ShelfRight',329,471)]:place(n,x,y,'shelf')
for n,x,y,point in [('KelpTall',2,277,[18,340]),('CoralColony',0,418,[17,471]),('CoralColony',327,288,[344,341]),('KelpTall',327,409,[343,472]),('BarnaclePatch',19,366,[22,374]),('BarnaclePatch',334,382,[339,392]),('VentOutlet',11,394,[16,399]),('VentOutlet',341,507,[346,512])]:place(n,x,y,'growth' if n in ['KelpTall','CoralColony'] else 'attached_accent',point)
data=dict(stage='06',status='CANDIDATE_PENDING_REVIEW',assets=assets,placements=placements,reference_canvas=[360,640],native_scale_at_720=2,tall_screen_y_offset=63,notes='All side art is behind the unchanged Stage05 frame/cells. Native separate modules. Tall layout shifts these board-attached decorations with the board. Kelp tips are separately registered for a later animation stage; no animation here.')
(R/'Native/Stage06/Manifest.json').write_text(json.dumps(data,indent=2)+'\n')
font=ImageFont.truetype('C:/Windows/Fonts/consola.ttf',11);sheet=Image.new('RGBA',(680,480),(9,20,29,255));d=ImageDraw.Draw(sheet)
for i,a in enumerate(assets):
 x=12+i%5*136;y=8+i//5*120;d.text((x,y),a['name'],font=font,fill='white');im=Image.open(E/(a['name']+'.png')).convert('RGBA');sheet.alpha_composite(im,(x,y+18))
sheet.save(P/'Native-Contact.png');sheet.resize((1360,960),Image.Resampling.NEAREST).save(P/'Native-Contact-2x.png')
# Composition and explicit holdfast evidence only; never used as a Unity sprite.
whole=Image.new('RGBA',(360,640));structure=Image.new('RGBA',whole.size)
for p in placements:
 im=Image.open(E/(p['name']+'.png')).convert('RGBA');whole.alpha_composite(im,(p['x'],p['y']))
 if p['role'] in ['structure','shelf']:structure.alpha_composite(im,(p['x'],p['y']))
whole.save(P/'SideLayers-Native.png');structure.save(P/'StoneStructure-Native.png')
proof=Image.new('RGBA',(134,348),(9,20,29,255));proof.alpha_composite(whole.crop((0,243,40,551)),(8,25));proof.alpha_composite(whole.crop((320,243,360,551)),(86,25));d=ImageDraw.Draw(proof);d.text((8,7),'LEFT',font=font,fill='white');d.text((86,7),'RIGHT',font=font,fill='white')
for p in placements:
 if p['attachment']:
  x,y=p['attachment'];sx=x+8 if x<100 else x-320+86;sy=y-243+25
  d.line((sx-3,sy,sx+3,sy),fill=(242,199,105,255));d.line((sx,sy-3,sx,sy+3),fill=(242,199,105,255))
proof.save(P/'Attachments-Native.png');proof.resize((536,1392),Image.Resampling.NEAREST).save(P/'Attachments-4x.png')
whole.crop((0,243,40,551)).resize((160,1232),Image.Resampling.NEAREST).save(P/'Left-4x.png')
print(json.dumps(dict(assets=len(assets),placed_modules=len(placements),attachment_markers=sum(p['attachment'] is not None for p in placements))))
