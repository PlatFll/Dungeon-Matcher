from pathlib import Path
from PIL import Image
import json
S=Path(r'C:/Users/USER/.codex/worktrees/drowned-court-pixellab-v2/Dungeon Matcher/ArtSource/SnowyMountainStaged/Stage04')
im=Image.open(S/'Raw/PlatformSource.png').convert('RGBA');parts={}
for name,box in [('SnowFloor_A',(24,14,88,46)),('SnowFloor_B',(88,14,152,46)),('SnowFloor_C',(184,14,248,46)),('Separator_A',(48,72,112,96)),('Separator_B',(112,72,176,96)),('Separator_C',(176,72,240,96)),('Foundation',(112,64,176,96)),('SnowOverhang_A',(14,59,123,94)),('SnowOverhang_B',(183,67,256,90))]:
 crop=im.crop(box);crop.save(S/'Selected'/(name+'.png'));parts[name]={'sourceRect':box,'size':crop.size}
(S/'Extraction.json').write_text(json.dumps(parts,indent=2))
print('9 native module selections prepared')
