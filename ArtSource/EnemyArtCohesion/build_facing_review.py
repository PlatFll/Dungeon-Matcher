"""Build focused playback with exact pre-correction PNGs embedded from Git."""
from pathlib import Path
import base64, json, subprocess
ROOT=Path(__file__).resolve().parents[2];BASE=Path(__file__).resolve().parent
jobs=json.loads((BASE/'jobs.json').read_text());audit=json.loads((ROOT/'Docs/ArtDirection/Audit2026-10/bindings.json').read_text())
report=json.loads((BASE/'facing-validation.json').read_text());rules=json.loads((BASE/'facing_corrections.json').read_text())
lookup={(j['path'],str(r['source']['fileID'])):(j,n) for j in jobs for n,r in enumerate(j['sprites'])}
changed={(j['stem'],f['cell']) for j in report['changes'] for f in j['frames']};before={}
for j in report['changes']:
    raw=subprocess.check_output(['rtk','proxy','git','show',report['baseline_commit']+':ArtSource/EnemyArtCohesion/Sheets/'+j['stem']+'.png'],cwd=ROOT)
    before[j['stem']]='data:image/png;base64,'+base64.b64encode(raw).decode()
actors=[]
for name in rules['actors']:
    actor=next(a for a in audit['actors'] if a['id']==name);states=[]
    for state in [dict(name='Ready',loop=True,frames=[dict(sprite=actor['still_binding'],duration=.5)])]+actor['states']:
        frames=[]
        for f in state['frames']:
            j,n=lookup[f['sprite']['path'],str(f['sprite']['fileID'])]
            frames.append(dict(stem=j['stem'],x=n*j['size'][0],size=j['size'],duration=f['duration'],changed=(j['stem'],n) in changed))
        states.append(dict(name=state['name'],loop=state['loop'],frames=frames))
    actors.append(dict(id=name,name=actor['name'],states=states,reason=rules['actors'][name]['reason']))
template=(BASE/'facing_review_template.html').read_text(encoding='utf-8')
html=template.replace('/*DATA*/',json.dumps(actors,separators=(',',':'))).replace('/*BEFORE*/',json.dumps(before,separators=(',',':')))
(BASE/'FacingReview.html').write_text(html,encoding='utf-8')
print(json.dumps(dict(actors=len(actors),states=sum(len(a['states']) for a in actors),before_sheets=len(before),bytes=len(html.encode()))))
