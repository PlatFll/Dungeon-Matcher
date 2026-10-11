"""Inspect disconnected hit particles; compose exact cels, propose guarded editor patches."""
from pathlib import Path
from PIL import Image,ImageDraw
import json,base64
root=Path(__file__).resolve().parents[1]/'ArtSource/Frostwake'
files=sorted(root.glob('motion/*/Hit/raw/*.png'))
out=root/'cleanup';out.mkdir(exist_ok=True)
sheet=Image.new('RGBA',(1024,104*((len(files)+7)//8)));manifest=[];patches=[]
for i,p in enumerate(files):
    im=Image.open(p).convert('RGBA');ox=(i%8)*128;oy=(i//8)*104;sheet.paste(im,(ox,oy))
    original=Image.open(root/'approved'/(p.parents[2].name+'.png')).convert('RGBA')
    pixels=im.load();remaining={(x,y) for y in range(104) for x in range(128) if pixels[x,y][3]}
    components=[]
    while remaining:
        start=remaining.pop();component={start};stack=[start]
        while stack:
            x,y=stack.pop()
            for dx,dy in [(0,1),(0,-1),(1,0),(-1,0)]:
                n=(x+dx,y+dy)
                if n in remaining:remaining.remove(n);component.add(n);stack.append(n)
        components.append(component)
    components.sort(key=len,reverse=True)
    # Proposals only: review clusters before handing exact patches to PixelLab.
    stray=set().union(*(c for c in components[1:] if len(c)<=45)) if len(components)>1 else set()
    # Reviewed clips contain saturated red impact spikes absent from the stills.
    # Preserve the Hearthkeeper's intentional handheld flame below the face.
    for y in range(104):
        for x in range(128):
            r,g,b,a=pixels[x,y]
            if a and r>=190 and g<100 and b<100 and not ('hearthkeeper' in str(p) and y>55 and x>38):
                stray.add((x,y))
    if p.stem in ('00','04'):stray.clear() # exact supplied endpoints, including disconnected prop tips
    patch=[]
    for x,y in sorted(stray):
        r,g,b,a=pixels[x,y]
        source=original.getpixel((x,y))
        if source[3] and not (r>=190 and g<100 and b<100):continue
        replacement='#'+bytes(source).hex() if source[3] else '#00000000'
        patch.append([x+ox,y+oy,'#'+bytes(pixels[x,y]).hex(),replacement])
    manifest.append(dict(path=str(p.relative_to(root)),rect=[ox,oy,128,104],stray=len(stray),components=[len(c) for c in components]))
    patches.extend(patch)
sheet.save(out/'hit-source.png')
(out/'hit-manifest.json').write_text(json.dumps(manifest,indent=2))
(out/'hit-proposal.json').write_text(json.dumps(patches))
review=sheet.convert('RGBA');bg=Image.new('RGBA',sheet.size,'#303238');bg.alpha_composite(review);d=ImageDraw.Draw(bg)
for x,y,_,_ in patches:d.point((x,y),fill='#ff00ff')
bg.convert('RGB').save(out/'hit-proposal-review.png')
print(json.dumps(dict(cels=len(files),patchPixels=len(patches),sheet=sheet.size)))
