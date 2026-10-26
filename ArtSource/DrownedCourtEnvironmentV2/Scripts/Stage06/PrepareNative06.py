from pathlib import Path
from PIL import Image
import shutil,json
F=Path(r'C:/Users/USER/.codex/worktrees/drowned-court-pixellab-v2/Dungeon Matcher');R=F/'ArtSource/DrownedCourtEnvironmentV2';W=Path(__file__).parent
sizes={'ColumnCap':(31,16),'ColumnBase':(31,18),**{'ColumnShaft'+v:(31,32) for v in 'ABC'},'ShelfLeft':(31,12),'ShelfRight':(31,12),'KelpTall':(37,68),'KelpTip':(37,22),'KelpStem':(37,48),'CoralColony':(38,57),'BarnaclePatch':(9,13),'VentOutlet':(11,11)}
sizes['ColumnShaftShort']=(31,18)
sizes.update(KelpTall=(31,68),KelpTip=(31,22),KelpStem=(31,48),CoralColony=(33,57))
for n,size in sizes.items():Image.new('RGBA',size).save(R/f'Native/Stage06/Work-{n}.png')
if W.resolve()!=(R/'Scripts/Stage06').resolve():shutil.copyfile(W/'AuthorSide06.js',R/'Scripts/Stage06/AuthorSide06.js')
print(len(sizes),'native working canvases prepared.')
