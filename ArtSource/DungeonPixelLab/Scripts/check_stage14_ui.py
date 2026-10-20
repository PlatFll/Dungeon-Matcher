from pathlib import Path
from PIL import Image,ImageChops
import json
P=Path(__file__).resolve().parents[3];R=P/'ArtSource/DungeonPixelLab'
def check_ui():
 evidence=json.loads((R/'Diagnostics/Stage14/AssemblyEvidence.json').read_text())
 manifest=json.loads((R/'Progress/Stage13BottomHudPlacement.json').read_text())
 standard=Image.open(R/'Previews/Stage14/Assembly-360x640.png').convert('RGBA')
 tall=Image.open(R/'Previews/Stage14/Assembly-360x800.png').convert('RGBA')
 reports=[]
 for family,offset in [('BoardFrame',80),('BottomHud',160)]:
  mask=Image.new('L',standard.size)
  for u in evidence['sources']:
   if '/'+family+'/' not in u['path'] and not(family=='BottomHud' and u['name']=='NativeIcon' and u['rect']['y']>530):continue
   r=u['rect'];x,y,w,h=[round(r[k]) for k in ['x','y','width','height']]
   src=Image.open(P/u['path']).convert('RGBA');alpha=src.getchannel('A');tile=Image.new('L',(w,h))
   for yy in range(0,h,src.height):
    for xx in range(0,w,src.width):tile.paste(alpha,(xx,yy))
   if u['name']=='OverfilledBacking':
    # Match the actual interior mask, not the source tile's deliberate overfill.
    panel=next(p for p in manifest['panels'] if p.get('backing')==Path(u['path']).stem and p['x']+p['inset']-src.width==x and p['y']+p['inset']-src.height==y)
    inset=panel['inset'];left=panel['x']+inset;top=panel['y']+inset
    tile=tile.crop((left-x,top-y,left-x+panel['width']-2*inset,top-y+panel['height']-2*inset));x,y=left,top
   layer=Image.new('L',standard.size);layer.paste(tile,(x,y));mask=ImageChops.lighter(mask,layer)
  coords=[(x,y) for y in range(640) for x in range(360) if mask.getpixel((x,y))]
  errors=[(x,y) for x,y in coords if standard.getpixel((x,y))!=tall.getpixel((x,y+offset))]
  assert not errors,(family,len(errors),errors[:20])
  assert len(coords)>10000 if family=='BoardFrame' else len(coords)>8000
  # The live label is independent of source sprites. Its exact rendered white
  # glyph pixels must also survive the bottom-anchor translation unchanged.
  if family=='BottomHud':
   glyphs=[(x,y) for y in range(614,630) for x in range(56,304) if standard.getpixel((x,y))[:3]==(255,255,255)]
   assert glyphs
   assert all(standard.getpixel((x,y))==tall.getpixel((x,y+offset)) for x,y in glyphs)
  reports.append(dict(family=family,visible_source_pixels=len(coords),offset=offset,exact=True))
 return reports
if __name__=='__main__':print(json.dumps(check_ui()))
