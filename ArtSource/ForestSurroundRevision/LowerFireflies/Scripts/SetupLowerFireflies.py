from pathlib import Path
import hashlib,json,shutil,uuid
F=Path(r'C:/Users/USER/.codex/worktrees/forest-zone-hybrid-rebuild/Dungeon Matcher')
W=Path(__file__).parent
R=F/'ArtSource/ForestSurroundRevision/LowerFireflies'
assert not R.exists(), 'Do not overwrite an existing checkpoint'
for child in ['Scripts','Diagnostics','Previews']: (R/child).mkdir(parents=True,exist_ok=True)
sha=lambda p: hashlib.sha256(p.read_bytes()).hexdigest()
old=F/'ArtSource/ForestSurroundRevision/WaterfallCorrection'
refs=json.loads((old/'Diagnostics/ReferenceHashes.json').read_text())
for p,h in refs.items(): assert sha(F/p)==h,p
for p in ['Docs/ArtDirection/Dungeon_Matcher_Art_Direction.txt','Docs/ArtDirection/Golden_Reference_Register.md','Assets/_Game/Art/CombatIdles/Rattlebones_Idle.png','Assets/_Game/Art/EnchantedForestStaged/Stage03/Firefly_Mote.png']:
 refs[p]=sha(F/p)
assert refs['Assets/_Game/Art/CombatIdles/Rattlebones_Idle.png']=='59b04929e9fdc15ae9f1d70fd3c85c6ea5362bdac479eb12f45cfcbb0efe4306'
(R/'Diagnostics/ReferenceHashes.json').write_text(json.dumps(refs,indent=2))
preserve=json.loads((old/'Diagnostics/PreservedFiles.json').read_text())
for p in preserve: assert sha(F/p)==preserve[p],p
for path in [old,F/'Assets/_Game/Art/ForestSurroundRevision/WaterfallCorrection']:
 for p in path.rglob('*'):
  if p.is_file(): preserve[p.relative_to(F).as_posix()]=sha(p)
for p in ['Assets/_Game/Scenes/ArtPreview_ForestSurround_Waterfall.unity','Assets/_Game/Scenes/ArtPreview_ForestSurround_Waterfall.unity.meta','Assets/_Game/Scripts/Editor/ForestWaterfallRevisionTests.cs','Assets/_Game/Scripts/Editor/ForestWaterfallRevisionTests.cs.meta']:
 preserve[p]=sha(F/p)
(R/'Diagnostics/PreservedFiles.json').write_text(json.dumps(preserve,indent=2))
p=F/'Assets/_Game/Scripts/Editor/ForestFireflyReviewTests.cs'
shutil.copy2(W/p.name,p)
p.with_suffix('.cs.meta').write_text('fileFormatVersion: 2\nguid: '+uuid.uuid5(uuid.NAMESPACE_URL,'dungeon-matcher/forest-lower-firefly-review').hex+'\n')
runner=(old/'Scripts/RunUnity.ps1').read_text().replace('WaterfallCorrection','LowerFireflies').replace('ForestWaterfallRevisionTests.WaterfallReview','ForestFireflyReviewTests.FireflyReview')
(R/'Scripts/RunUnity.ps1').write_text(runner)
shutil.copy2(p,R/'Scripts'/p.name)
shutil.copy2(W/'SetupLowerFireflies.py',R/'Scripts/SetupLowerFireflies.py')
print(json.dumps(dict(preserved_files=len(preserve),references_verified=len(refs),test=str(p)),indent=2))
