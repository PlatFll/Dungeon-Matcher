from pathlib import Path
from PIL import Image,ImageDraw,ImageEnhance
import json,hashlib
r=Path(__file__).resolve().parent;out=r/'Selected/UI';out.mkdir(exist_ok=True)
def raw(n):return Image.open(r/'Raw'/(n+'.png')).convert('RGBA')
def save(im,n):im.save(out/(n+'.png'))
for name in ['court_background','court_floor','coral_column','coral_foreground','general_stone','inset_panel','thorny_snare','air_coffer','pressure_seal']:
 im=raw(name);save(im,name)
bubble=raw('air_bubble');d=ImageDraw.Draw(bubble);d.ellipse((7,7,24,24),fill=(0,0,0,0));save(bubble,'air_bubble')
# Tileable variants retain the approved full-pitch native cell, with subtle exact color changes.
cell=Image.open(r.parent/'Approved/DC_M02_Marine_Cell.png').convert('RGBA')
for i in range(3):
 im=cell.copy();im.putdata([(max(0,min(255,p[0]+i*2)),max(0,min(255,p[1]+i*2)),max(0,min(255,p[2]+i)),255) for p in cell.get_flattened_data()]);save(im,'cell_'+str(i))
source=Image.open(r.parent/'Approved/DC_M01_Marine_Frame.png').convert('RGBA')
# Approved corner/edge pixels are assembled at 1:1, not stretched.
corner=Image.new('RGBA',(80,80));corner.paste(source.crop((0,0,16,16)),(0,0));edge=source.crop((24,0,40,16))
for x in range(16,80,16):corner.paste(edge,(x,0));corner.paste(edge.transpose(Image.Transpose.ROTATE_90),(0,x))
save(corner,'FrameCorner')
# World frame edges span one 64px gem cell. Repeat native pixels across that
# span so their texel scale matches the 80px corner and the board itself.
worldEdge=Image.new('RGBA',(64,16))
for x in range(0,64,16):worldEdge.paste(edge,(x,0))
save(worldEdge,'FrameEdge')
button=raw('marine_button');bbox=button.getbbox();button=button.crop(bbox)
def panel(w,h):
 im=Image.new('RGBA',(w,h));tex=raw('inset_panel')
 for y in range(0,h,64):
  for x in range(0,w,64):im.paste(tex,(x,y))
 top=source.crop((0,0,16,16))
 for x in range(16,w-16,16):im.paste(edge,(x,0),edge);bottom=edge.transpose(Image.Transpose.FLIP_TOP_BOTTOM);im.paste(bottom,(x,h-16),bottom)
 for y in range(16,h-16,16):
  left=edge.transpose(Image.Transpose.ROTATE_90);right=left.transpose(Image.Transpose.FLIP_LEFT_RIGHT);im.paste(left,(0,y),left);im.paste(right,(w-16,y),right)
 for x,y,op in [(0,0,None),(w-16,0,Image.Transpose.FLIP_LEFT_RIGHT),(0,h-16,Image.Transpose.FLIP_TOP_BOTTOM),(w-16,h-16,Image.Transpose.ROTATE_180)]:
  p=top if op is None else top.transpose(op);im.paste(p,(x,y),p)
 return im
save(panel(96,64),'PanelShell');save(panel(128,32),'WavePlaque')
player=panel(146,208)
# These are foreground three-slice borders. Their interior must stay clear
# so the player portrait, HP and affinity remain visible underneath.
ImageDraw.Draw(player).rectangle((16,16,129,191),fill=(0,0,0,0))
save(player.crop((0,0,146,32)),'PlayerTop');save(player.crop((0,64,146,96)),'PlayerMiddle');save(player.crop((0,176,146,208)),'PlayerBottom')
save(button,'ButtonNormal')
for name,factor in [('ButtonHighlighted',1.14),('ButtonPressed',.78),('ButtonDisabled',.55)]:
 adjusted=ImageEnhance.Brightness(button).enhance(factor);adjusted.putalpha(button.getchannel('A'));save(adjusted,name)
for name,factor in [('SupplyNormal',1),('SupplyHighlighted',1.15),('SupplyPressed',.78),('SupplyDisabled',.5)]:
 im=raw('inset_panel');im=ImageEnhance.Brightness(im).enhance(1.5*factor)
 mask=Image.new('L',(64,64));ImageDraw.Draw(mask).rounded_rectangle((1,1,62,62),radius=5,fill=255)
 im.putalpha(mask);save(im,name)
# Preserve the existing gear glyph pixels; replace only its button setting.
repo=r.parents[2]
gearSource=repo/'ArtSource/Forest/Production/Selected/UI/SettingsNormal.png'
gear=Image.open(gearSource).convert('RGBA')
for name,factor in [('SettingsNormal',1),('SettingsHighlighted',1.15),('SettingsPressed',.78),('SettingsDisabled',.55)]:
 im=panel(*gear.size);center=(gear.width//2,gear.height//2)
 # Central glyph extraction is reviewed against the live gear during UI validation.
 glyph=gear.crop((center[0]-15,center[1]-15,center[0]+15,center[1]+15))
 # Keep the existing warm gear only, remove the old green timber setting.
 glyph.putdata([p if p[0]>p[2]*1.35 and p[0]>p[1]*1.08 else (0,0,0,0) for p in glyph.get_flattened_data()])
 im.paste(glyph,(center[0]-15,center[1]-15),glyph);im=ImageEnhance.Brightness(im).enhance(factor);save(im,name)
# The approved standalone semantic ability glyphs have transparent surroundings.
for n in ['AbilityRoyal','AbilityHarmony','AbilityCamera']:
 save(Image.open(repo/('ArtSource/Forest/Production/Selected/UI/'+n+'.png')).convert('RGBA'),n)
energy=Image.open(repo/'Assets/_Game/Resources/UI/Finalized/EnergyFrame.png').convert('RGBA')
marine=[(10,13,17),(35,55,64),(61,89,96),(104,135,138),(184,200,190)]
colors=sorted(set(p[:3] for p in energy.get_flattened_data() if p[3]),key=lambda p:sum(p))
mapping={c:marine[min(4,i*5//len(colors))] for i,c in enumerate(colors)}
energy.putdata([(*mapping[p[:3]],p[3]) if p[3] else p for p in energy.get_flattened_data()]);save(energy,'EnergyFrame')
checks=[]
for p in out.glob('*.png'):
 im=Image.open(p).convert('RGBA');checks.append(dict(file=p.name,size=im.size,alpha=sorted(set(im.getchannel('A').get_flattened_data())),colors=len(set(q[:3] for q in im.get_flattened_data() if q[3])),sha256=hashlib.sha256(p.read_bytes()).hexdigest()))
(out/'MaterialManifest.json').write_text(json.dumps({'operations':'Native crops, tile assembly and exact brightness variants. Air bubble center removed in native ellipse (7,7)-(24,24). No sprite resampling. Generated scene and props otherwise unchanged. UI remains subject to runtime visual QA.','assets':checks},indent=2))
print(len(checks),'native materials prepared')
