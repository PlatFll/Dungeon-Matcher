from pathlib import Path
from PIL import Image
import shutil
F=Path(r'C:/Users/USER/.codex/worktrees/drowned-court-pixellab-v2/Dungeon Matcher');R=F/'ArtSource/DrownedCourtEnvironmentV2';W=Path(__file__).parent
sizes={**{n:(25,25) for n in ['CornerTL','CornerTR','CornerBL','CornerBR']},**{n:(32,11) for n in ['RailTop','RailBottom','RailHorizontalWorn']},**{n:(11,32) for n in ['RailLeft','RailRight','RailVerticalWorn']},'Joint':(9,9),'AttachmentSocket':(11,11)}
sizes['InnerShadow']=(1,1)
for n,size in sizes.items():Image.new('RGBA',size).save(R/f'Native/Stage05/Work-{n}.png')
if (W/'AuthorBoard05.js').resolve()!=(R/'Scripts/Stage05/AuthorBoard05.js').resolve():shutil.copyfile(W/'AuthorBoard05.js',R/'Scripts/Stage05/AuthorBoard05.js')
print(str(len(sizes))+' blank native working canvases prepared; original PixelLab seed is read-only.')
