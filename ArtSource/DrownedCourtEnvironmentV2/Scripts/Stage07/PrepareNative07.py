from pathlib import Path
from PIL import Image
import shutil
F=Path(r'C:/Users/USER/.codex/worktrees/drowned-court-pixellab-v2/Dungeon Matcher');R=F/'ArtSource/DrownedCourtEnvironmentV2';W=Path(__file__).parent
for n,size in {'Seafloor':(32,6),'SeafloorShort':(8,6),'RubbleA':(16,8),'RubbleB':(12,6)}.items():Image.new('RGBA',size).save(R/f'Native/Stage07/Work-{n}.png')
if W.resolve()!=(R/'Scripts/Stage07').resolve():shutil.copyfile(W/'AuthorReef07.js',R/'Scripts/Stage07/AuthorReef07.js')
for name in ['RunNative','RoundTrip','RunUnity']:
 src=R/f'Scripts/Stage06/{name}06.ps1';target=R/f'Scripts/Stage07/{name}07.ps1';t=src.read_text().replace('Stage06','Stage07').replace('Side06','Reef07').replace('17 native documents','7 native documents');target.write_text(t)
print('Stage07 native authoring scripts ready.')
