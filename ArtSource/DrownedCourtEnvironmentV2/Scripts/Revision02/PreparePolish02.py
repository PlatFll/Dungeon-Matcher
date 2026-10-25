from pathlib import Path
from PIL import Image
import json,shutil
F=Path(r'C:/Users/USER/.codex/worktrees/drowned-court-pixellab-v2/Dungeon Matcher');R=F/'ArtSource/DrownedCourtEnvironmentV2';W=Path(__file__).parent
for n in ['PortraitWallA','PortraitWallB']:Image.new('RGBA',(64,64)).save(R/f'Native/Revision02/Work-{n}.png')
for i in range(5):Image.new('RGBA',(37,38)).save(R/f'Native/Revision02/Work-Clam{i}.png')
for p in (R/'Native/Revision02').glob('Work-*.png'):
 if not p.stem.endswith('-canvas'):shutil.copyfile(p,p.with_name(p.stem+'-canvas.png'))
plan=json.loads((R/'Native/Revision02/CoralCleanup.json').read_text());(R/'Scripts/Revision02/AuthorPolish02.js').write_text((W/'AuthorPolish02.js').read_text().replace('__CLEANUP__',json.dumps(plan)))
ps=(W/'RunLibreRevision01.ps1').read_text().replace('Revision01','Revision02').replace("'RecolorBackground'","'AuthorPolish02'")
(W/'RunLibrePolish02.ps1').write_text(ps)
shutil.copyfile(W/'AuthorClam02.js',R/'Scripts/Revision02/AuthorClam02.js')
print('Native stage 1-4 script ready.')
