from pathlib import Path
import shutil,uuid
F=Path(r'C:/Users/USER/.codex/worktrees/forest-zone-hybrid-rebuild/Dungeon Matcher');R=F/'ArtSource/ForestSurroundRevision/WaterfallCorrection';W=Path(__file__).parent
p=F/'Assets/_Game/Scripts/Editor/ForestWaterfallRevisionTests.cs';shutil.copy2(W/p.name,p)
p.with_suffix('.cs.meta').write_text('fileFormatVersion: 2\nguid: '+uuid.uuid5(uuid.NAMESPACE_URL,'DungeonMatcher/ForestWaterfallRevisionTests').hex+'\n')
runner=(F/'ArtSource/ForestSurroundRevision/LivingForest/Scripts/RunUnity.ps1').read_text().replace('LivingForest','WaterfallCorrection').replace('WaterfallCorrectionReviewTests.MotionReview','ForestWaterfallRevisionTests.WaterfallReview')
(R/'Scripts/RunUnity.ps1').write_text(runner)
shutil.copy2(p,R/'Scripts'/p.name)
print('Installed focused copied-scene builder and matched-time comparison test.')
