"""Build an offline before/after player from exact sprites and clip exposures."""
from pathlib import Path
import json
ROOT=Path(__file__).resolve().parents[2];BASE=Path(__file__).resolve().parent
jobs=json.loads((BASE/'jobs.json').read_text(encoding='utf-8'));audit=json.loads((ROOT/'Docs/ArtDirection/Audit2026-10/bindings.json').read_text(encoding='utf-8'))
lookup={}
for j in jobs:
 for i,r in enumerate(j['sprites']):lookup[j['path'],str(r['source']['fileID'])]=(j,i)
actors=[]
for a in audit['actors']:
 if a['zone']=='Players':continue
 states=[]
 for s in a['states']:
  frames=[]
  for i,f in enumerate(s['frames']):
   j,n=lookup[f['sprite']['path'],str(f['sprite']['fileID'])]
   frames.append(dict(after='Sheets/'+j['stem']+'.png',x=n*j['size'][0],size=j['size'],padding=j['padding'],duration=f['duration']))
  states.append(dict(name=s['name'],loop=s['loop'],duration=s['duration'],before='../../Docs/ArtDirection/Audit2026-10/'+s['media'],size=s['size'],frames=frames))
 actors.append(dict(id=a['id'],name=a['name'],zone=a['zone'],states=states))
template=(BASE/'review_template.html').read_text(encoding='utf-8')
(BASE/'Review.html').write_text(template.replace('/*DATA*/',json.dumps(actors,separators=(',',':'))),encoding='utf-8')
print('Built playback gallery:',len(actors),'forms,',sum(len(a['states']) for a in actors),'states')
