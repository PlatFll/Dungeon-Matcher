from pathlib import Path
import shutil,json
F=Path(r'C:/Users/USER/.codex/worktrees/drowned-court-pixellab-v2/Dungeon Matcher');R=F/'ArtSource/DrownedCourtEnvironmentV2';W=Path(__file__).parent
# Strip whitespace only on newly created serialized files; no originals are normalized.
paths=[F/'Assets/_Game/Scenes/ArtPreview_DrownedCourtV2_Revision02.unity',F/'Assets/_Game/Scenes/ArtPreview_DrownedCourtV2_Revision02.unity.meta',F/'Assets/_Game/Scripts/Editor/DrownedCourtV2Polish02Preview.cs.meta',F/'Assets/_Game/Art/DrownedCourtEnvironmentV2/Revision02.meta']
paths+=list((F/'Assets/_Game/Art/DrownedCourtEnvironmentV2/Revision02').rglob('*.meta'))+list((F/'Assets/_Game/Art/DrownedCourtEnvironmentV2/Revision02/Motion').glob('*.anim'))+list((F/'Assets/_Game/Art/DrownedCourtEnvironmentV2/Revision02/Motion').glob('*.controller'))
for p in paths:p.write_text('\n'.join(v.rstrip() for v in p.read_text().splitlines())+'\n')
for n in ['InspectPolish02.py','FinalizePolish02.py','RunUnityPolish02.ps1','RestoreIncidental02.py']:
 if (W/n).resolve()!=(R/'Scripts/Revision02'/n).resolve():shutil.copy2(W/n,R/'Scripts/Revision02'/n)
print('New serialized-file whitespace normalized; helper scripts preserved.')
