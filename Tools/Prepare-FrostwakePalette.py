"""Build shared-palette requests from inspected motion; no pixel alteration."""
from pathlib import Path
from PIL import Image
import json,base64,sys
root=Path(__file__).resolve().parents[1]/'ArtSource/Frostwake';out=root/'palette-requests';out.mkdir(exist_ok=True)
index=[]
for actor in sorted((root/'motion').iterdir()):
    frames=[]
    for clip in sorted(actor.iterdir()):
        source=clip/'clean' if (clip/'clean').exists() else clip/'raw'
        frames.extend(sorted(source.glob('*.png')))
    palette=root/'approved'/(actor.name+'-palette.png')
    for start in range(0,len(frames),19):
        key=actor.name+'-'+str(start//19);subset=frames[start:start+19]
        payload=dict(paths=[str(p.relative_to(root)) for p in subset],images_base64=[base64.b64encode(p.read_bytes()).decode() for p in subset],palette_image_base64=base64.b64encode(palette.read_bytes()).decode())
        (out/(key+'.json')).write_text(json.dumps(payload));index.append(key)
(out/'index.json').write_text(json.dumps(index));print(json.dumps(index))
