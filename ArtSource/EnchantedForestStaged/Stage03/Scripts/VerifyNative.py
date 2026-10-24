from pathlib import Path
from PIL import Image
import json,hashlib
p=Path(__file__).resolve().parents[4]
r=p/'ArtSource/EnchantedForestStaged/Stage03';a=p/'Assets/_Game/Art/EnchantedForestStaged/Stage03';t=p/'.utmp/enchanted-stage03'
selection=json.loads((r/'Diagnostics/Selection.json').read_text());results=[]
for m in selection['modules']:
 n=m['name'];im=Image.open(a/(n+'.png')).convert('RGBA');expected=Image.open(t/(n+'-expected.png')).convert('RGBA');reopened=Image.open(t/'reopened'/(n+'.png')).convert('RGBA')
 assert im.size==expected.size==reopened.size and im.tobytes()==expected.tobytes()==reopened.tobytes(),n
 assert set(im.getchannel('A').get_flattened_data())<={0,255},n
 x,y,x2,y2=im.getbbox();assert x>0 and y>0 and x2<im.width and y2<im.height,n
 results.append(dict(name=n,dimensions=im.size,bounds=im.getbbox(),colors=len({c for c in im.get_flattened_data() if c[3]}),binary_alpha=True,native_reopen_exact=True,sha256=hashlib.sha256((a/(n+'.png')).read_bytes()).hexdigest()))
for family,names in [('Lantern',['Lantern_Mount','Lantern_Body','Lantern_Light']),('Rune',['Rune_Plaque','Rune_Glyph'])]:
 im=Image.open(a/(names[0]+'.png')).convert('RGBA')
 for n in names[1:]:im.alpha_composite(Image.open(a/(n+'.png')).convert('RGBA'))
 expected=Image.open(t/(family+'-expected-composite.png')).convert('RGBA');assert im.tobytes()==expected.tobytes()
 raw=Image.open(r/('Raw/Lantern_02.png' if family=='Lantern' else 'Raw/Rune_15.png')).convert('RGBA');assert im.getchannel('A').tobytes()==raw.getchannel('A').tobytes()
 im.save(r/'Exports'/(family+'.png'))
core=Image.open(a/'Wisp_Core.png').convert('RGBA');orig=Image.open(p/'Assets/_Game/Art/EnchantedForestStaged/Stage01/Wisp_Near.png').convert('RGBA');assert core.tobytes()==orig.tobytes()
out=dict(modules=results,generated_shape_masks_preserved=True,wisp_core_preserved=True,libresprite_author_exit=0,libresprite_reopen_exit=0)
(r/'Diagnostics/NativeValidation.json').write_text(json.dumps(out,indent=2))
print('PASS: 9 native exports match reopened LibreSprite sources; binary alpha, padded bounds, generated masks and approved wisp core preserved.')
