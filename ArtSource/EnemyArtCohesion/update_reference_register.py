"""Extract bound current references losslessly and refresh their exact register."""
from pathlib import Path
import json,re,hashlib
from PIL import Image
ROOT=Path(__file__).resolve().parents[2];BASE=Path(__file__).resolve().parent
audit=json.loads((ROOT/'Docs/ArtDirection/Audit2026-10/bindings.json').read_text(encoding='utf-8'))
jobs=json.loads((BASE/'jobs.json').read_text(encoding='utf-8'));lookup={}
for j in jobs:
 for i,r in enumerate(j['sprites']):lookup[j['path'],str(r['source']['fileID'])]=(j,i)
register=ROOT/'Docs/ArtDirection/Golden_Reference_Register.md';text=register.read_text(encoding='utf-8')
out=ROOT/'Docs/ArtDirection/EnemyCohesion2026-10';out.mkdir(exist_ok=True)
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
for ident in ['farmer','pan_villager','royal_swordsman','elven_mender','orc_trailguard','pearl_thief','elven_scout']:
 a=next(a for a in audit['actors'] if a['id']==ident);s=a['still_binding'];j,i=lookup[s['path'],str(s['fileID'])];w,h=j['size'];p=out/(ident+'-still.png')
 Image.open(ROOT/s['path']).crop((i*w,0,(i+1)*w,h)).save(p)
 pattern=r'(### [^\n]+\n(?:(?!### ).)*?)- Current native image: \['+re.escape(ident)+r'-still.png\].*?(?=\n- Current controller:)'
 m=re.search(pattern,text,re.S);assert m,ident
 details=f'- Current native image: [{ident}-still.png](EnemyCohesion2026-10/{ident}-still.png) ({w}×{h}).\n- Extracted PNG SHA-256: `{sha(p)}`.\n- Production source: `{s["path"]}`; SHA-256 `{sha(ROOT/s["path"])}`.\n- Unity GUID `{s["guid"]}`, fileID `{s["fileID"]}`, sprite `{s["name"]}`.\n- Extraction rectangle in PNG top-left coordinates: `[{i*w}, 0, {(i+1)*w}, {h}]` (right/bottom exclusive). Full expanded canvas; no scaling/trimming. Unity source-floor pivot: ({w/2:g}, 4).'
 text=text[:m.start()]+m[1]+details+text[m.end():]
text=text.replace('2026-10-11 · canonical guide v1.12. This register identifies images and roles; the canonical art guide owns direction. No production art was changed.','2026-10-11 · canonical guide v1.13. This register identifies current images and roles; the canonical art guide owns direction. Enemy images now reflect the user-authorized cohesion/canvas pass. Rattlebones remains unchanged.')
text=text.replace('Current motion sources are the linked controller and its complete clip/frame ledger in `Audit2026-10/bindings.json`.','Current motion sources are the linked controller and `ArtSource/EnemyArtCohesion/jobs.json` / `unity-import-manifest.json`. `Audit2026-10/bindings.json` remains the immutable pre-edit ledger, with old rectangles and hashes.')
text=text.replace('[Labeled comparison board](Audit2026-10/boards/golden-comparison.png) · [Offline gallery](Audit2026-10/Review.html)','[Original audit comparison board](Audit2026-10/boards/golden-comparison.png) · [Current enemy before/after player](../../ArtSource/EnemyArtCohesion/Review.html)')
text+='\n## 2026-10-11 production update\n\nThe user authorized cleanup, west-facing animation repairs, canvas expansion and integration. Current extracted images above follow those bound exports; original references and approval history remain in the immutable audit. This implementation does not approve the four proposed golden memberships. All player art, including Rattlebones, is byte-identical.\n'
register.write_text(text,encoding='utf-8')
print('Updated seven enemy references; Rattlebones and all membership statuses preserved.')
