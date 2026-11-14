from pathlib import Path
from PIL import Image,ImageDraw
import subprocess,json,shutil,hashlib
root=Path(__file__).resolve().parents[3];base=root/'ArtSource/BombReadability/Candidates'
libre=r'C:/Users/USER/Downloads/libresprite-development-windows-x86_64/libresprite.exe'
selection={'Poison':[0,3,6,14],'Healing':[0,1,9,15],'Directional':[0,5,11,15],'Star':[2,7,13]}
manifest=[]
for family,indices in selection.items():
    for label,i in zip('ABCD',indices):
        im=Image.open(base/'Raw'/family/f'{i:02}.png').convert('RGBA');im.putalpha(im.getchannel('A').point(lambda a:255 if a>=128 else 0))
        # The native dark socket cluster gives its placement; preserve the surrounding silhouette.
        cx,cy=32,32
        if family!='Star':
            points=[(x,y) for y in range(24,47) for x in range(23,41) if max(im.getpixel((x,y))[:3])<45 and im.getpixel((x,y))[3]>0]
            cx=round(sum(x for x,y in points)/len(points));cy=round(sum(y for x,y in points)/len(points))
            d=ImageDraw.Draw(im);d.ellipse((cx-13,cy-13,cx+13,cy+13),fill='#10151B');d.ellipse((cx-12,cy-12,cx+12,cy+12),fill='#53616A');d.arc((cx-12,cy-12,cx+12,cy+12),185,285,fill='#81919A',width=1);d.ellipse((cx-11,cy-11,cx+11,cy+11),fill='#171F29')
        else:
            # Quiet white center, restrained pastel reflections; retain authored silhouette/facets.
            for y in range(64):
                for x in range(64):
                    r,g,b,a=im.getpixel((x,y))
                    if a and min(r,g,b)>150:
                        if y<30: c=(255,253,235) if x<31 else (237,252,255)
                        else:c=(224,232,250) if x>31 else (246,230,240)
                        if min(r,g,b)>210:c=(255,255,247)
                        im.putpixel((x,y),(*c,a))
        name=family+label
        dst=base/'Cleaned'/f'{name}.png';dst.parent.mkdir(exist_ok=True);im.save(dst)
        edit=base/'Editable'/f'{name}.aseprite';edit.parent.mkdir(exist_ok=True)
        subprocess.run([libre,'-b',str(dst),'--save-as',str(edit)],check=True,capture_output=True)
        check=base/'Cleaned'/f'{name}-roundtrip.png';subprocess.run([libre,'-b',str(edit),'--save-as',str(check)],check=True,capture_output=True)
        assert Image.open(check).convert('RGBA').tobytes()==im.tobytes();check.unlink()
        entry={'id':name,'family':family,'raw_index':i,'size':[64,64],'socket':[cx,cy] if family!='Star' else None,'alpha':'binary','status':'candidate, not approved','sha256':hashlib.sha256(dst.read_bytes()).hexdigest()}
        manifest.append(entry)
        if family=='Directional':
            # Matched mechanical orientation; only a quarter turn, no resampling.
            column=im.transpose(Image.Transpose.ROTATE_90);column.save(base/'Cleaned'/f'{name}-Column.png');entry['column_socket']=[cy,63-cx]
sheet=Image.new('RGB',(960,960),'#20232e');d=ImageDraw.Draw(sheet)
for row,family in enumerate(selection):
    for col,entry in enumerate(e for e in manifest if e['family']==family):
        x=col*240;y=row*240;d.text((x+8,y+3),entry['id'],fill='white');im=Image.open(base/'Cleaned'/f"{entry['id']}.png");socket=entry['socket']
        if socket:
            gem=Image.open(root/'Assets/_Game/Art/Gems/Small gems/Gesm16/Emerald16.png').convert('RGBA');im.alpha_composite(gem,(socket[0]-8,socket[1]-8))
        sheet.paste(im.resize((192,192),Image.Resampling.NEAREST),(x+24,y+20),im.resize((192,192),Image.Resampling.NEAREST));sheet.paste(im,(x+88,y+175),im)
sheet.save(base/'CandidateReview.png');(base/'selection.json').write_text(json.dumps(manifest,indent=2))
# The new white star is previewed on the review branch; casing selection remains pending.
star=root/'Assets/_Game/Art/Board/SpecialOverlays/StarColorBomb.png'
if not star.exists():
    found=list(root.glob('Assets/**/StarColorBomb.png'));assert len(found)==1;star=found[0]
original=base/'OriginalStarColorBomb.png'
if not original.exists():shutil.copy2(star,original)
shutil.copy2(base/'Cleaned/StarB.png',star)
print('Prepared 12 socketed casing candidates, paired directions, and 3 white stars in LibreSprite. StarB on review branch.')
