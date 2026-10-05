"""Verify Court Unity exports and native source references without modifying assets."""
from pathlib import Path
from collections import defaultdict
import hashlib,json,re
from PIL import Image

root=Path(__file__).resolve().parents[3]
assets=root/'Assets'
guid_paths=defaultdict(list)
for path in list(assets.rglob('*.meta'))+list((root/'Library/PackageCache').rglob('*.meta')):
 match=re.search(r'^guid: ([0-9a-f]{32})$',path.read_text(encoding='utf-8-sig'),re.M)
 if match:guid_paths[match.group(1)].append(path)
paths=[]
for folder in ['Animations/DrownedCourt','Art/DrownedCourt','Data/Enemies/DrownedCourt']:
 paths.extend(p for p in (assets/'_Game'/folder).rglob('*') if p.is_file())
for folder,pattern in [('Resources/BattleEnvironments','DrownedCourt*'),('Resources/Zones','DrownedCourt*'),
                       ('Resources/Zones','drowned-court*'),('Audio/Music','Drowned Court*')]:
 paths.extend((assets/'_Game'/folder).glob(pattern))
for name in ['AirPickup','AirWarning','CofferBreak']:
 paths.append(assets/'_Game/Resources/Audio/Combat'/(name+'.wav'))
errors=[];textures=0;references=0;hashes={}
for path in paths:
 if path.suffix=='.meta':continue
 relative=path.relative_to(root).as_posix();hashes[relative]=hashlib.sha256(path.read_bytes()).hexdigest()
 meta=Path(str(path)+'.meta')
 if not meta.exists():errors.append(relative+': missing meta');continue
 meta_text=meta.read_text(encoding='utf-8-sig')
 match=re.search(r'^guid: ([0-9a-f]{32})$',meta_text,re.M)
 if not match or len(guid_paths[match.group(1)])!=1:errors.append(relative+': absent or duplicate GUID')
 if path.suffix=='.png':
  textures+=1
  for field,value in [('enableMipMap','0'),('filterMode','0'),('textureCompression','0')]:
   if not re.search(r'\b'+field+r': '+value+r'\b',meta_text):errors.append(relative+': '+field)
  im=Image.open(path).convert('RGBA')
  if len(im.getchannel('A').getcolors())>2 and path.name!='AbilityRoyal.png':errors.append(relative+': unexpected intermediate alpha')
 if path.suffix in ['.asset','.prefab','.controller','.anim']:
  for guid in re.findall(r'guid: ([0-9a-f]{32})',path.read_text(encoding='utf-8-sig')):
   if guid.startswith('0000000000000000'):continue
   references+=1
   if guid not in guid_paths:errors.append(relative+': unresolved GUID '+guid)
result=dict(exported_files=len(hashes),textures=textures,guid_references=references,errors=errors,sha256=hashes)
output=Path(__file__).parent/'ExportChecks.json';output.write_text(json.dumps(result,indent=2))
print(json.dumps(dict(exported_files=len(hashes),textures=textures,guid_references=references,
                     error_count=len(errors),first_errors=errors[:5],full_record=str(output))))
raise SystemExit(bool(errors))
