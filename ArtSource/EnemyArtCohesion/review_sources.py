"""Read-only sprite inspection layouts. Never writes production pixels."""
from pathlib import Path
from PIL import Image, ImageDraw
from collections import Counter
import json,sys
ROOT=Path(__file__).resolve().parents[2]
OUT=ROOT/'.utmp/EnemyArtCohesion'
data=json.loads((ROOT/'Docs/ArtDirection/Audit2026-10/bindings.json').read_text(encoding='utf-8'))
jobs=json.loads((Path(__file__).parent/'jobs.json').read_text(encoding='utf-8'))
originals={j['path']:j['original'] for j in jobs}
OUT.mkdir(parents=True,exist_ok=True)
names=sys.argv[1:] or ['seismic_smith','lantern_warden','puffer_sentinel','snapvine']
for actor in data['actors']:
 if actor['id'] not in names:continue
 refs=[('Ready',0,actor['still_binding'])]+[(s['name'],i,f['sprite']) for s in actor['states'] for i,f in enumerate(s['frames'])]
 pages=[refs[i:i+24] for i in range(0,len(refs),24)]
 palette=Counter()
 for p,rows in enumerate(pages):
  w=max(r[2]['size'][0] for r in rows);h=max(r[2]['size'][1] for r in rows)
  cellw,cellh=max(w*3+12,180),h*3+28
  board=Image.new('RGB',(cellw*4,cellh*((len(rows)+3)//4)),(35,38,51));draw=ImageDraw.Draw(board)
  for i,(state,f,s) in enumerate(rows):
   im=Image.open(ROOT/originals.get(s['path'],s['path'])).convert('RGBA').crop(s['crop_top_left'])
   palette.update(tuple(c[:3]) for c in im.getdata() if c[3])
   x,y=i%4*cellw,i//4*cellh
   if i%2:draw.rectangle((x,y,x+cellw-1,y+cellh-1),fill=(219,215,203))
   board.paste(im.resize((im.width*3,im.height*3),Image.Resampling.NEAREST),(x+6,y+22),im.resize((im.width*3,im.height*3),Image.Resampling.NEAREST))
   draw.text((x+6,y+4),f'{state} {f+1}',fill=(255,200,85) if not i%2 else (0,0,0))
  board.save(OUT/f'{actor["id"]}-inspect-{p:02}.png')
 (OUT/f'{actor["id"]}-palette.json').write_text(json.dumps({'#%02X%02X%02X'%k:v for k,v in palette.most_common()},indent=2))
 print(actor['id'],len(refs),len(palette))
