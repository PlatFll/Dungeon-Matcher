from pathlib import Path
from PIL import Image, ImageChops, ImageDraw, ImageFont
import json, hashlib, math
F=Path(__file__).resolve().parents[5]
R=F/'ArtSource/SnowyMountainStaged/Stage04/Revision01'
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
baseline=json.loads((R/'QA/StartingHashes.json').read_text())
changed=[p for p,h in baseline.items() if not (F/p).exists() or sha(F/p)!=h]
assert not changed,changed
wall=Image.open(R/'Exports/CobbleWall.png').convert('RGB')
wallcolors=set(wall.get_flattened_data())
# Unity's linear/sRGB PNG round trip truncates a few source channels by one
# byte (for example #5283aa renders #5283a9). This is color encoding, not a
# spatial interpolation tolerance. All larger differences remain failures.
def wallpixel(c):return any(max(abs(x-y) for x,y in zip(c,s))<=1 for s in wallcolors)
rows=json.loads((R/'Preview/Measurements.json').read_text())['items']
result=[]
for row in rows:
    key=row['size'];a=Image.open(R/f'Preview/Before/{key}.png').convert('RGB');b=Image.open(R/f'Preview/After/{key}.png').convert('RGB')
    d=ImageChops.difference(a,b);w,h=a.size;p=row['nativePixelScale'];g=row['geometry']
    limit=math.floor(h-g['feet'][0]-20*p)
    assert not d.crop((0,0,w,limit)).getbbox(),(key,'Upper region changed')
    r=g['portrait'];pad=10*p
    box=(math.ceil(r['x']+pad),math.ceil(h-r['y']-r['height']+pad),math.floor(r['x']+r['width']-pad),math.floor(h-r['y']-pad))
    assert not d.crop(box).getbbox(),(key,'Portrait interior changed')
    # A whole-screen backing remains visible at the left/right screen boundaries
    # and bottom edge, independent of inset safe areas and UI enclosure margins.
    lower=math.ceil(h-row['separatorBottom'])
    edge=[b.getpixel((x,y)) for y in range(lower,h) for x in (0,w-1)]
    edge.extend(b.getpixel((x,h-1)) for x in range(w))
    unmatched=sum(not wallpixel(c) for c in edge)
    assert unmatched==0,(key,'Uncovered lower perimeter pixels',unmatched)
    r=g['board'];pad=36*p/2
    boardbox=(math.ceil(r['x']+pad),math.ceil(h-r['y']-r['height']+pad),math.floor(r['x']+r['width']-pad),math.floor(h-r['y']-pad))
    aa=a.crop(boardbox);bb=b.crop(boardbox)
    nonwallchanges=sum(x!=y and not wallpixel(y) for x,y in zip(aa.get_flattened_data(),bb.get_flattened_data()))
    assert nonwallchanges==0,(key,'Changed board pixels outside new stone backing',nonwallchanges)
    result.append(dict(viewport=key,integerNativeScale=p,geometryUnchanged=True,upperPixelsUnchanged=True,portraitInteriorUnchanged=True,lowerPerimeterUncoveredPixels=unmatched,changedBoardPixelsOutsideWallPalette=nonwallchanges,differenceBounds=d.getbbox()))
(R/'QA/ScreenDifferences.json').write_text(json.dumps(result,indent=2))
(R/'QA/Preservation.json').write_text(json.dumps(dict(protectedFiles=len(baseline),changedFiles=changed),indent=2))
# Exact 720px captures, with labels outside the images. No screenshot painting.
a=Image.open(R/'Preview/Before/720x1280.png').convert('RGB');b=Image.open(R/'Preview/After/720x1280.png').convert('RGB')
board=Image.new('RGB',(1464,1320),'#0c1520');draw=ImageDraw.Draw(board)
font=ImageFont.truetype('C:/Windows/Fonts/segoeui.ttf',20)
draw.text((0,4),'Previous separator / background',fill='#dbeaf2',font=font);draw.text((744,4),'Original PixelLab ledge + continuous stone wall',fill='#dbeaf2',font=font)
board.paste(a,(0,40));board.paste(b,(744,40));board.save(R/'Preview/Unity-BeforeAfter.png')
native=Image.open(R/'Preview/LedgeAndWall-native.png').convert('RGBA')
print(json.dumps(dict(protectedFiles=len(baseline),captures=len(result),allPassed=True)))
