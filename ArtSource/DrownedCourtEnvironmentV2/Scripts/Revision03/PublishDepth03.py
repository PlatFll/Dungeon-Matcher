from pathlib import Path
from PIL import Image,ImageDraw
import json,hashlib,shutil
F=Path(__file__).resolve().parents[4];R=F/'ArtSource/DrownedCourtEnvironmentV2';P=R/'Preview/Revision03'
board=Image.new('RGB',(744,276),(11,21,30));d=ImageDraw.Draw(board)
for x,phase,label in [(8,'Before','BEFORE'),(380,'Revised','REVISED - ruins set back')]:
 d.text((x,7),label,fill='white');board.paste(Image.open(P/f'Unity-{phase}-360x640.png').crop((0,0,360,244)).convert('RGB'),(x,24))
board.save(P/'Depth-Before-After.png')
html=(R/'Preview/Revision02/Review.html').read_text(encoding='utf-8')
start=html.index('<h1>');end=html.index('<p><button',start)
html=html[:start]+'''<h1>Drowned Court — ruins set farther back</h1><p>The upper stonework moves back by 16 native pixels. One additional course of the existing floor tiles grounds its bases. Every original sprite, actor, frame, board cell and control remains unchanged. This is the revised Unity art scene; the new environment remains pending production integration and approval.</p>'''+html[end:]
html=html.replace('Before — Revision 01','Before — Revision 02')
start=html.index('<section><h2>Native source assets');end=html.index('<script>',start)
html=html[:start]+'''<section><h2>Verification</h2><p><a href="../../Revision03Report.md">Floor alignment and preservation report</a> · <a href="../../QA/Revision03/Diagnostics/Measurements.json">Actual Game measurements</a></p><p>No new artwork or PixelLab spending. All source PNGs and LibreSprite documents are retained byte-for-byte.</p></section>'''+html[end:]
html=html.replace(' · <a href="Unity-Revised-1080x2400.png">1080×2400 tall still</a>','').replace(' · <a href="Underwater-Motion.gif">GIF motion preview</a>','').replace('../../Revision02Report.md','../../Revision03Report.md')
(P/'Review.html').write_text(html,encoding='utf-8')
print('Published focused before/after board and lossless Unity-frame gallery.')
