"""Read-only enlarged coordinate layout for artist-directed cel patches."""
from pathlib import Path
from PIL import Image,ImageDraw
import json,sys
root=Path(__file__).resolve().parents[2]
data=json.loads((root/'Docs/ArtDirection/Audit2026-10/bindings.json').read_text())
originals={j['path']:j['original'] for j in json.loads((root/'ArtSource/EnemyArtCohesion/jobs.json').read_text())}
actor=next(a for a in data['actors'] if a['id']==sys.argv[1])
state=next(s for s in actor['states'] if s['name']==sys.argv[2])
indices=[int(x) for x in sys.argv[3:]] or list(range(len(state['frames'])))
for i in indices:
 s=state['frames'][i]['sprite'];im=Image.open(root/originals.get(s['path'],s['path'])).convert('RGBA').crop(s['crop_top_left']);k=8
 out=Image.new('RGB',(im.width*k+32,im.height*k+32),(58,59,67));out.paste(im.resize((im.width*k,im.height*k),Image.Resampling.NEAREST),(24,24),im.resize((im.width*k,im.height*k),Image.Resampling.NEAREST));d=ImageDraw.Draw(out)
 for x in range(0,im.width,8):d.text((24+x*k,4),str(x),fill='white');d.line((24+x*k,24,24+x*k,im.height*k+24),fill=(80,80,90))
 for y in range(0,im.height,8):d.text((1,24+y*k),str(y),fill='white');d.line((24,24+y*k,im.width*k+24,24+y*k),fill=(80,80,90))
 out.save(root/f'.utmp/EnemyArtCohesion/grid-{actor["id"]}-{state["name"]}-{i}.png')
