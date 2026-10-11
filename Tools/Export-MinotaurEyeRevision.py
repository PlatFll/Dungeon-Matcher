"""Verify PixelLab's exact edits, then losslessly unpack into additive revision sources."""
from pathlib import Path
from PIL import Image,ImageDraw
import hashlib,json,shutil
root=Path(__file__).resolve().parents[1];base=root/'ArtSource/FrostwakePlayables';out=base/'eye-revision'
source=Image.open(out/'source-sheet.png').convert('RGBA');result=Image.open(out/'candidate-sheet.png').convert('RGBA')
assert source.size==result.size
decisions=json.loads((out/'decisions.json').read_text());allowed={(p[0],p[1]):p for p in decisions[0]['pixels']}
diff=[]
for y in range(source.height):
    for x in range(source.width):
        a,b=source.getpixel((x,y)),result.getpixel((x,y))
        assert a[3]==b[3]
        if a!=b:
            assert (x,y) in allowed,(x,y)
            expected=tuple(bytes.fromhex(allowed[x,y][3][1:]));assert b==expected
            diff.append([x,y])
records=[]
for item in json.loads((out/'source-manifest.json').read_text()):
    path=root/item['path'];assert hashlib.sha256(path.read_bytes()).hexdigest()==item['sha256']
    x,y=item['offset'];w,h=item['size'];cel=result.crop((x,y,x+w,y+h))
    target=out/'Minotaur.png' if item['index']==0 else out/path.parent.parent.name/path.name
    target.parent.mkdir(exist_ok=True);cel.save(target)
    records.append({'source':item['path'],'path':str(target.relative_to(root)),'sha256':hashlib.sha256(target.read_bytes()).hexdigest()})
shutil.copyfile(out/'Minotaur.png',root/'Assets/_Game/Art/FrostwakePlayables/Minotaur_Portrait.png')
review=Image.new('RGB',(640,370),'#303238');draw=ImageDraw.Draw(review)
for i,p in enumerate([base/'locked/Minotaur.png',out/'Minotaur.png']):
    im=Image.open(p).convert('RGBA').resize((320,320),Image.Resampling.NEAREST);review.paste(im,(i*320,35),im)
draw.text((14,10),'Before',fill='white');draw.text((334,10),'Darker amber + black eye contour',fill='white')
review.save(out/'comparison.png')
(out/'verification.json').write_text(json.dumps({'provider':'PixelLab pixelart_workbench','result_id':'c44e74e6-2eef-40ba-afce-161ab736d449','paid_generations':0,'changed_pixels':len(diff),'alpha_unchanged':True,'outside_eye_regions_unchanged':True,'original_sources_preserved':True,'outputs':records},indent=2))
print(f'Verified {len(diff)} changed eye pixels across {len(records)} cels; all other pixels and alpha unchanged.')
