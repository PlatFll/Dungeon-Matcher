from pathlib import Path
from PIL import Image
from collections import Counter,deque
import hashlib,json
r=Path.cwd();a=r/'ArtSource/DungeonCornerFoundations'
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
refs=json.loads((a/'ReferenceHashes.json').read_text())
assert all(sha(r/p)==h for p,h in refs.items()),'Existing approved references/guide changed'
report={'reference_hashes_unchanged':True,'libresprite_native_edit':True,'resampling':False,'pixel_lab_generations':31,'assets':{}}
for side in ['Left','Right']:
    path=a/'Exports'/f'Dungeon_Foundation_{side}.png';im=Image.open(path).convert('RGBA')
    colors=Counter(im.get_flattened_data());alpha=sorted(set(p[3] for p in colors))
    mask={(x,y) for y in range(im.height) for x in range(im.width) if im.getpixel((x,y))[3]}
    islands=[];remaining=set(mask)
    while remaining:
        seed=remaining.pop();queue=deque([seed]);component={seed}
        while queue:
            x,y=queue.popleft()
            for p in [(x-1,y),(x+1,y),(x,y-1),(x,y+1)]:
                if p in remaining:remaining.remove(p);component.add(p);queue.append(p)
        islands.append(component)
    islands.sort(key=len,reverse=True)
    assert im.size==(112,128) and alpha==[0,255]
    assert len(islands)==1,[(len(i),list(i)[:5]) for i in islands]
    assert path.read_bytes()==(r/'Assets/_Game/Art/DungeonCornerFoundations'/path.name).read_bytes()
    report['assets'][side]={'size':im.size,'opaque_bounds':im.getbbox(),'opaque_colors':len(colors)-1,'alpha':alpha,'opaque_pixels':len(mask),'connected_components':len(islands),'sha256':sha(path),'palette':sorted('#%02x%02x%02x'%c[:3] for c in colors if c[3])}
print(json.dumps(report,indent=2))
(a/'ArtValidation.json').write_text(json.dumps(report,indent=2)+'\n')
# Make the actual native edit recipe portable; no changes to pixel output.
p=a/'Scripts/BuildNative.js';s=p.read_text();s=s.replace(json.dumps(a.as_posix()+'/'),json.dumps('ArtSource/DungeonCornerFoundations/')).replace(json.dumps((r/'Assets/_Game/Art/DungeonSideArchitecture/Side_Shaft.png').as_posix()),json.dumps('Assets/_Game/Art/DungeonSideArchitecture/Side_Shaft.png'));p.write_text(s)
import shutil
if Path(__file__).resolve()!=(a/'Scripts/validate_foundation_art.py').resolve():
    shutil.copy2(__file__,a/'Scripts/validate_foundation_art.py')
old=a/'Scripts/prepare_foundation_native.py'
if old.exists():old.unlink()
