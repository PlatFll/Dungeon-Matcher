"""Emit exact guarded edits for PixelLab; local outputs are masks/inspection only."""
from pathlib import Path
from PIL import Image, ImageDraw
import json, base64

ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT / 'ArtSource/Frostwake/face-revision'
records = json.loads((OUT/'sources.json').read_text())
locations = json.loads((OUT/'face-locations.json').read_text())

def hexcolor(c): return '#'+bytes(c).hex()
def rgb(s): return tuple(bytes.fromhex(s[1:]))+(255,)

def plan(name, item, location):
    # The packed baseline is immutable even after approved edits are imported.
    px,py=item['offset']
    im=Image.open(OUT/f'{name}-source.png').convert('RGBA').crop((px,py,px+128,py+104))
    dx,dy=location['offset']; patches={}
    def box(rect, mapping, offset=True):
        ox,oy=(dx,dy) if offset else (0,0)
        for y in range(max(0,rect[1]+oy),min(104,rect[3]+oy)):
            for x in range(max(0,rect[0]+ox),min(128,rect[2]+ox)):
                old=im.getpixel((x,y)); key=hexcolor(old)[:7]
                if old[3] and key in mapping and key!=mapping[key]:patches[x,y]=[x,y,hexcolor(old),mapping[key]+'ff']
    i=item['index']
    if name=='frostblade_reaver':
        if i not in list(range(21,28))+[29,30]:box([75,56,79,60],{'#d1d8de':'#18191a'})
    elif name=='hearthkeeper':
        if i not in list(range(22,28))+[31,32]:box([67,47,68,50],{'#ebdbbe':'#141415'})
        if i==3:box([66,49,67,50],{'#ebdbbe':'#141415'})
        if 4<=i<=7:box([65,49,67,50],{'#ebdbbe':'#141415'})
    elif name=='iceborn_raider':
        if i not in range(13,19):box([64,60,68,64],{'#d2c5b4':'#19191b','#0f1011':'#19191b','#3f2621':'#19191b'})
    elif name=='jarl_haldra':
        if i not in list(range(18,26))+[28,29]:
            mapping={c:'#101517' for c in ['#101517','#123039','#284b56','#5ab3c9','#d3ecee','#f3ebdc']}
            box([64,40,66,43],mapping);box([72,40,76,43],mapping)
    elif name=='iceborn_champion':
        # Individual eye glints below the helmet, inspected in all 45 poses.
        # Death's closed lids and helmet/beard highlights are not eye interiors.
        points={3:[(3,2),(10,2)],4:[(2,1),(2,2)],5:[(4,1),(4,2)],
                6:[(2,1),(2,2)],7:[(4,2),(11,2)],
                27:[(9,2)],28:[(2,1),(8,1),(9,2)],29:[(9,1),(9,2)],
                30:[(8,0),(8,1)]}.get(i,[(10,1),(10,2)])
        if i not in range(19,26):
            for x,y in points:
                box([55+x,42+y,56+x,43+y],{'#f1e3cb':'#121314','#6a6e6d':'#121314','#d5c6b0':'#121314'})
    elif name=='frostfang_wolf':
        if i!=22:
            # Track the actual blue eye pixels: the eye changes slope in the bite.
            blue=[(x,y) for y in range(104) for x in range(128) if im.getpixel((x,y)) in [rgb('#1a73ab'),rgb('#27c6f2')]]
            if blue:
                x0,y0=min(p[0] for p in blue),min(p[1] for p in blue)
                x1,y1=max(p[0] for p in blue),max(p[1] for p in blue)
                box([x0,y0,x1+1,y1+1],{'#1a73ab':'#27c6f2','#e3e4e8':'#27c6f2','#c1c3d0':'#27c6f2','#21242f':'#27c6f2'},False)
    elif name=='rime_skald':
        # Separate exposed flesh, hair/beard and protected cream hat/clothing.
        head_map={'#895839':'#dfa881','#392921':'#737980','#271e1c':'#464c53','#5a3d2b':'#969ba1'}
        death_head={21:[43,70,72,83],22:[44,72,72,85],23:[43,71,72,85],24:[43,70,72,84],25:[43,70,72,84],26:[43,70,72,84],27:[43,70,72,84]}
        if i in death_head:box(death_head[i],head_map,False)
        else:box([52,46,82,66],head_map)
        # Ear is skin between the cap and sideburn; retain the darker hair boundary.
        if i not in death_head:box([75,54,78,58],{'#895839':'#dfa881','#5a3d2b':'#b58a6e'})
        else:box([68,death_head[i][1]-1,72,death_head[i][1]+3],{'#895839':'#dfa881','#5a3d2b':'#b58a6e'},False)
        if i not in list(range(4,7))+list(range(21,28))+[29,30]:
            box([65,53,69,57],{'#b59875':'#141516','#e4d5b8':'#141516','#f4e7ce':'#141516','#9e7f60':'#141516','#0c0c0d':'#141516'})
            box([56,53,59,57],{'#0c0c0d':'#141516'})
        # Named per-pose hand regions, reviewed below; no global skin-color mapping.
        hands={
            0: [[49,73,56,79],[67,74,72,81]],
            1: [[49,73,56,79],[67,74,72,81]],
            2: [[46,67,54,74],[61,65,67,72]],
            3: [[47,62,55,69],[62,57,69,64]],
            4: [[47,62,55,69],[62,57,69,64]],
            5: [[47,62,55,69],[62,57,69,64]],
            6: [[47,62,55,69],[62,57,69,64]],
            7: [[47,63,55,70],[63,58,70,65]],
            8: [[48,67,56,74],[64,63,71,70]],
            9: [[49,72,56,78],[67,73,73,81]],
            10:[[49,73,56,79],[67,74,72,81]],
            11:[[44,73,51,79],[61,74,67,81]],
            12:[[33,68,41,75],[46,69,52,76]],
            13:[[14,66,23,73],[29,66,38,74]],
            14:[[14,66,23,73],[29,66,38,74]],
            15:[[17,67,26,74],[32,67,41,75]],
            16:[[31,68,40,76],[46,69,54,77]],
            17:[[42,71,50,78],[58,72,66,80]],
            18:[[49,72,56,79],[66,73,73,81]],
            19:[[49,73,56,79],[67,74,72,81]],
            20:[[42,75,50,82],[60,76,67,84]],
            21:[[62,87,68,94]],
            22:[[62,88,69,95]],
            23:[[62,88,69,95]],
            24:[[63,87,69,94]],
            25:[[63,87,69,94]],
            26:[[63,87,69,94]],
            27:[[63,87,69,94]],
            28:[[49,73,56,79],[67,74,72,81]],
            29:[[47,72,55,80],[64,73,72,81]],
            30:[[46,73,54,80],[63,74,71,82]],
            31:[[47,72,55,80],[64,73,72,81]],
            32:[[49,72,56,79],[67,74,72,81]],
            33:[[49,73,56,79],[67,74,72,81]],
            34:[[49,72,56,78],[67,73,72,80]],
            35:[[49,71,56,78],[67,73,72,80]],
            36:[[49,71,56,78],[67,73,72,80]],
            37:[[49,72,56,78],[67,73,72,80]],
        }
        for rect in hands[i]:box(rect,{'#895839':'#dfa881','#5a3d2b':'#b58a6e','#392921':'#9e694f'},False)
    return list(patches.values())

summary={}
for actor in records:
    name=actor['actor']; pixels=[]; cels=[]
    source=Image.open(OUT/f'{name}-source.png').convert('RGBA')
    overlay=source.copy()
    for item, location in zip(actor['items'],locations[name]):
        patches=plan(name,item,location);ox,oy=item['offset']
        cels.append(dict(index=item['index'],patches=patches))
        for x,y,a,b in patches:
            pixels.append([x+ox,y+oy,a,b]);overlay.putpixel((x+ox,y+oy),(255,40,180,255))
    summary[name]=dict(pixels=len(pixels),cels=sum(bool(c['patches']) for c in cels))
    (OUT/f'{name}-regions.json').write_text(json.dumps(cels,indent=2))
    if pixels:
        decisions=[dict(operation='patch_pixels',layer='image',frames=[1],pixels=pixels[start:start+8000],reason='Inspected eye interiors become one flat color; Skald exposed flesh and hair are recolored by material. Preserve all other RGBA pixels and native motion.') for start in range(0,len(pixels),8000)]
        (OUT/f'{name}-decisions.json').write_text(json.dumps(decisions,indent=2))
        # Inspection-only overlay, not a replacement sprite.
        bg=Image.new('RGBA',overlay.size,'#303238');bg.alpha_composite(overlay)
        bg.resize((bg.width*2,bg.height*2),Image.Resampling.NEAREST).save(OUT/f'{name}-selection.png')
(OUT/'planned-changes.json').write_text(json.dumps(summary,indent=2))
print(json.dumps(summary))
