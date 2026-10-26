from pathlib import Path
from PIL import Image
import shutil
F=Path(r'C:/Users/USER/.codex/worktrees/drowned-court-pixellab-v2/Dungeon Matcher');R=F/'ArtSource/DrownedCourtEnvironmentV2';W=Path(__file__).parent
for n,size in {'SpongePocket':(28,33),'TridentShard':(18,26)}.items():Image.new('RGBA',size).save(R/f'Native/Stage08/Work-{n}.png')
if W.resolve()!=(R/'Scripts/Stage08').resolve():shutil.copyfile(W/'AuthorDetails08.js',R/'Scripts/Stage08/AuthorDetails08.js')
for name in ['RunNative','RoundTrip','RunUnity']:
 t=(R/f'Scripts/Stage07/{name}07.ps1').read_text().replace('Stage07','Stage08').replace('Reef07','Details08').replace('7 native documents','3 native documents');(R/f'Scripts/Stage08/{name}08.ps1').write_text(t)
print('Stage08 native helpers ready.')
