from pathlib import Path
import hashlib,json,subprocess,shutil
F=Path(r'C:/Users/USER/.codex/worktrees/drowned-court-pixellab-v2/Dungeon Matcher');R=F/'ArtSource/IronveinBelowTopHUD/Revision04'
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
head=subprocess.check_output(['git','rev-parse','HEAD'],cwd=F,text=True).strip()
status=subprocess.check_output(['git','status','--porcelain'],cwd=F,text=True)
assert not status.strip(),'Preserve unexpected starting changes: '+status
for d in ['QA','Preview','Scripts']: (R/d).mkdir(parents=True,exist_ok=True)
locks=json.loads((R.parent/'Revision03/QA/ApprovedSourceLock.json').read_text())
for root in [F/'Assets/_Game/Art/IronveinBelowTopHUD/Revision03',R.parent/'Revision03/Exports',R.parent/'Revision03/Native']:
 for p in root.rglob('*'):
  if p.is_file(): locks[p.relative_to(F).as_posix()]=sha(p)
for rel in ['Assets/_Game/Scenes/ArtPreview_IronveinBelowTopHUD_Revision03.unity','Assets/_Game/Scenes/ArtPreview_IronveinBelowTopHUD_Revision03.unity.meta','Assets/_Game/Scripts/Editor/IronveinBelowTopHUDRevision03.cs','Assets/_Game/Scripts/UI/BattleFloorAnchor.cs','Assets/_Game/Scripts/UI/DungeonPresentationArt.Court.cs','Assets/_Game/Scripts/UI/DungeonPresentationArt.Forest.cs','Assets/_Game/Scripts/UI/DungeonPresentationArt.PixelLab.cs']:
 locks[rel]=sha(F/rel)
(R/'QA/ApprovedSourceLock.json').write_text(json.dumps(locks,indent=2))
gold=json.loads((R.parent/'Revision03/QA/GoldenInspection.json').read_text())
for p,h in gold.items():assert sha(F/p)==h,p
(R/'QA/GoldenInspection.json').write_text(json.dumps(gold,indent=2))
(R/'QA/StartingState.json').write_text(json.dumps(dict(head=head,initial_status=status,scope='Align the unchanged walking floor around the shared enemy standing line, with the approved separator directly below. Match other-zone scenery-around-baseline ownership.',generation_spending=0),indent=2))
(R/'QA/LayoutReferences.json').write_text(json.dumps(dict(rule='Scenery follows the common actor/health-bar baseline; no zone-specific enemy position.',references=[dict(path=p,sha256=sha(F/p)) for p in ['Assets/_Game/Scripts/UI/BattleFloorAnchor.cs','Assets/_Game/Scripts/UI/DungeonPresentationArt.Court.cs','Assets/_Game/Scripts/UI/DungeonPresentationArt.Forest.cs','Assets/_Game/Scripts/UI/DungeonPresentationArt.PixelLab.cs','ArtSource/DrownedCourtEnvironmentV2/Integration/After/drowned-court-720x1280.png','ArtSource/DrownedCourtEnvironmentV2/Integration/After/dungeon-720x1280.png']],court='Source floor 208 maps to the live actor feet; front course 200..216 is above the fascia.',dungeon='The 12px walking strip centers the shared 6px standing inset; fascia begins immediately below.',ironvein='Center the existing 32px floor around the unchanged measured sprite-floor pivot. Move the separator to its bottom edge. Do not move health bars, actors or board.'),indent=2))
runner=(R.parent/'Scripts/RunRevision03.ps1').read_text().replace('Revision03','Revision04')
(R.parent/'Scripts/RunRevision04.ps1').write_bytes(runner.encode())
shutil.copyfile(__file__,R/'Scripts/PrepareReview.py')
print('Revision04 baseline captured; approved art and shared baseline ownership locked')
