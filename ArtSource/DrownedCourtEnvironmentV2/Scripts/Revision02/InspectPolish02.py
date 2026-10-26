from pathlib import Path
import re,hashlib,json,difflib
F=Path(r'C:/Users/USER/.codex/worktrees/drowned-court-pixellab-v2/Dungeon Matcher');R=F/'ArtSource/DrownedCourtEnvironmentV2'
authority={'Docs/ArtDirection/Dungeon_Matcher_Art_Direction.txt':'2b702121cd0913bce8f1dca9949718febfb2ebf790d6fd90bc8df683d01894c4','Docs/ArtDirection/Golden_Reference_Register.md':'d905f9d7a53cbaf66ece220609c0731bb9718fd10833e85aa73d2855b6370d4e','Docs/ArtDirection/Audit2026-10/media/skeleton-still.png':'705c4c8379d56a262e287d256664db8d180a79917f4a84289bbb813730b9616c','Docs/ArtDirection/EnemyCohesion2026-10/farmer-still.png':'6189bc2cfe01bab84bafada24a7b129a3fd8c2225f629eb8580386f461109e2d','Docs/ArtDirection/EnemyCohesion2026-10/pan_villager-still.png':'e1be89f8231483df5db6739dd2cae7bef83e11aaa9b028e4cb1b60ea55aba0ca'}
for name,h in authority.items():assert hashlib.sha256((F/name).read_bytes()).hexdigest()==h,name
def blocks(p):
 text='\n'.join(line.rstrip() for line in p.read_text().splitlines())+'\n';parts=re.split(r'(?m)^--- !u!\d+ &(-?\d+)\s*\n',text);return {parts[i]:parts[i+1] for i in range(1,len(parts),2)}
old=blocks(F/'Assets/_Game/Scenes/ArtPreview_DrownedCourtV2_Revision01.unity');new=blocks(F/'Assets/_Game/Scenes/ArtPreview_DrownedCourtV2_Revision02.unity');assert not (old.keys()-new.keys()),'old scene objects lost'
changes=[]
for key in old:
 if old[key]!=new[key]:
  lines=[v for v in difflib.unified_diff(old[key].splitlines(),new[key].splitlines(),n=0) if not v.startswith(('---','+++','@@'))];changes.append({'fileID':key,'lines':lines})
result={'unchanged_authorities':authority,'retained_serialized_blocks':len(old),'added_serialized_blocks':len(new)-len(old),'changed_existing_blocks':changes}
(R/'QA/Revision02/SerializedScope.json').write_text(json.dumps(result,indent=2)+'\n')
print(json.dumps({'retained':len(old),'added':len(new)-len(old),'changes':changes},indent=2))
