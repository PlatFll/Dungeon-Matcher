from pathlib import Path
import shutil,uuid
F=Path(r'C:/Users/USER/.codex/worktrees/forest-zone-hybrid-rebuild/Dungeon Matcher');R=F/'ArtSource/ForestSurroundRevision/LivingForest';W=Path(__file__).parent
for name,folder in [('LivingForestReviewMotion','ArtPreview'),('LivingForestReviewTests','Editor')]:
 p=F/'Assets/_Game/Scripts'/folder/(name+'.cs');shutil.copy2(W/(name+'.cs'),p)
 p.with_suffix('.cs.meta').write_text('fileFormatVersion: 2\nguid: '+uuid.uuid5(uuid.NAMESPACE_URL,'DungeonMatcher/LivingForest/'+name).hex+'\n')
runner=(F/'ArtSource/ForestSurroundRevision/PolishFinal/Scripts/RunUnity.ps1').read_text().replace('PolishFinal','LivingForest').replace('ForestFinalPolishReviewTests.FinalReview','LivingForestReviewTests.MotionReview')
(R/'Scripts/RunUnity.ps1').write_text(runner)
print('Installed isolated review component and validation suite.')
