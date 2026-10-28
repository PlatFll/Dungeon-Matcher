from pathlib import Path
from PIL import Image,ImageDraw,ImageChops
import json,hashlib,re,shutil,xml.etree.ElementTree as ET
F=Path(__file__).resolve().parents[3];root=F/'ArtSource/IronveinCornerStations';CW=Path(__file__).parent
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
before=json.loads((root/'QA/ProtectedBefore.json').read_text())
print('Protected manifest type',type(before).__name__)
changes=[k for k,v in before.items() if not (F/k).exists() or sha(F/k)!=v]
expected=['Assets/_Game/Resources/UI/BattlePresentation/IronveinAssembly.asset'];assert changes==expected or set(changes)==set(expected),changes
orig=(root/'QA/IronveinAssemblyBefore.asset.txt').read_text();actual=(F/expected[0]).read_text()
def retained(t):return [b for b in re.split(r'(?=^  - name: )',t,flags=re.M) if '\n    region: IronveinLowerModules\n' not in b and '\n    region: Revision01_LowerCrystals\n' not in b]
assert retained(orig)==retained(actual),'Protected assembly pieces changed'
qa=[]
for p in sorted((root/'Exports').glob('*.png')):
 im=Image.open(p).convert('RGBA');check=Image.open(root/'Roundtrip'/p.name).convert('RGBA');assert im.tobytes()==check.tobytes()
 originalPath=root/'Selected'/p.name
 preserve=None
 if originalPath.exists():
  original=Image.open(originalPath).convert('RGBA');assert im.size==original.size;assert im.getchannel('A').tobytes()==original.getchannel('A').tobytes();preserve=True
 alpha=sorted(set(im.getchannel('A').getdata()));assert alpha==[0,255]
 prod=F/'Assets/_Game/Art/IronveinCornerStations'/p.name;assert prod.read_bytes()==p.read_bytes()
 qa.append(dict(name=p.stem,size=list(im.size),bounds=im.getbbox(),opaqueColors=len(set(c for c in im.getdata() if c[3])),alpha=alpha,roundtrip=True,sourceMaskPreserved=preserve,sha256=sha(p)))
(root/'QA/NativeQA.json').write_text(json.dumps(qa,indent=2),encoding='utf-8')
summary=dict(protectedFiles=len(before)-1,changedExistingFiles=changes,protectedAssemblyPieces=69,nativeSprites=len(qa),approvedSourceFilesOverwritten=0,runtimeCodeChanges=0)
(root/'QA/Preservation.json').write_text(json.dumps(summary,indent=2),encoding='utf-8');print(summary)
receipts=json.loads((root/'Raw/GenerationReceipts.json').read_text());selection=json.loads((root/'Selected/Selection.json').read_text());ledger=[]
for j in receipts:
 t=j['response']['content'][0]['text'];n=j['name'];selected=[f"{v['name']} candidate {v['index']}" for v in selection if v['family']==n]
 ledger.append(dict(name=n,job=re.search(r'job_id: (\S+)',t)[1],cost=int(re.search(r'cost: (\d+)',t)[1]),candidates=int(re.search(r'candidates: (\d+)',t)[1]),selected=selected,status='Selected native source' if selected else 'Rejected: insufficient occupied size for left corner'))
assert sum(j['cost'] for j in ledger)==90
(root/'QA/GenerationLedger.json').write_text(json.dumps(dict(totalGenerations=90,balanceBefore=154.2,balanceAfter=64.2,paidCreditSpendUSD=0,jobs=ledger,textOnly=True,uploadedReferenceImages=0),indent=2),encoding='utf-8')
header='# PixelLab generation ledger\n\n90 subscription generations used. Balance 154.20 → 64.20; paid credit spend $0.00. Nine text-only Pro calls; no mockup/reference uploads.\n\n| Source | Job | Cost | Selection |\n|---|---|---:|---|\n'
for j in ledger:header+=f"| {j['name']} | `{j['job']}` | {j['cost']} | {', '.join(j['selected']) or j['status']} |\n"
header+='\nUnselected candidates remain in Raw/. Specific shape/material rejection reasons are in Selected/Selection.json. The timber extension is native LibreSprite work, with no additional generation.\n'
(root/'GenerationLedger.md').write_text(header,encoding='utf-8')
# Review-only crops of actual Unity captures; source sprites are not edited here.
for variant in ['Before','After']:
 im=Image.open(root/'Preview'/variant/'720x1280.png').convert('RGB');im.crop((0,1056,720,1280)).save(root/'Preview'/f'{variant}Corners.png')
ba=Image.new('RGB',(1440,250),'#111624');d=ImageDraw.Draw(ba)
for x,v in [(0,'Before'),(720,'After')]:
 d.text((x+12,6),v+' - actual Unity Game scene',fill='white');ba.paste(Image.open(root/'Preview'/f'{v}Corners.png'),(x,26))
ba.save(root/'Preview/BeforeAfterCorners.png')
native=Image.new('RGB',(900,700),'#181c28');d=ImageDraw.Draw(native)
for i,q in enumerate(qa):
 im=Image.open(root/'Exports'/f"{q['name']}.png").convert('RGBA');x=(i%4)*225;y=(i//4)*225
 d.text((x+5,y+5),q['name'],fill='white');d.text((x+5,y+19),f"{q['size']} | {q['opaqueColors']} colors",fill='#a7b1c5');native.paste(im,(x+4,y+40),im)
 up=im.resize((im.width*2,im.height*2),Image.Resampling.NEAREST);native.paste(up,(x+80,y+35),up)
native.save(root/'QA/NativeBoard.png')
cards=''.join(f'''<article><h3>{q['name']}</h3><p>{q['size'][0]}×{q['size'][1]} · {q['opaqueColors']} opaque colors · binary alpha</p><div class="asset"><img src="Exports/{q['name']}.png" width="{q['size'][0]}" height="{q['size'][1]}"><img src="Exports/{q['name']}.png" width="{q['size'][0]*3}" height="{q['size'][1]*3}"></div><a href="Native/{q['name']}.ase">Editable LibreSprite source</a></article>''' for q in qa)
html='''<!doctype html><meta charset="utf-8"><title>Ironvein — corner stations review</title><style>
body{background:#101521;color:#e8eafa;font:16px system-ui;margin:28px auto;max-width:1500px;padding:0 24px}h1{font-size:28px}h2{margin-top:36px}p{line-height:1.5;color:#b7c2d8}a{color:#ffa953}img{image-rendering:pixelated}article{background:#1d2433;padding:18px;border-radius:8px;overflow:auto}.assets{display:grid;grid-template-columns:repeat(3,1fr);gap:15px}.asset{display:flex;gap:24px;align-items:end;min-height:220px}.pair{display:flex;gap:20px;overflow:auto}.pair img{width:360px;height:auto}button,select{background:#242d42;color:white;border:1px solid #69748e;padding:8px 14px;margin:8px;cursor:pointer}.wide{max-width:100%;height:auto}strong{color:#ffd48a}
</style><h1>Ironvein · bottom corner stations</h1><p><strong>Review candidate — not merged.</strong> Actual Game scene captures. Left: ore-loading crate on track and connected rock. Right: anchored timber brace, pickaxe and supply crate. All other environment placements and shared UI are preserved.</p>
<h2>Before / revised</h2><img class="wide" src="Preview/BeforeAfterCorners.png"><p>Direct crops from the 720×1280 Unity captures. Captures are separate launches, so idle poses, assigned weakness gems and ambient animation can differ.</p>
<h2>Playable scene at phone sizes</h2><select id="size" onchange="setSize()"><option>720x1280</option><option>1080x1920</option><option>1080x2400</option><option>1080x2400-safe</option><option>360x640</option></select><div class="pair"><div><h3>Before</h3><a id="beforeLink" href="Preview/Before/720x1280.png"><img id="before" src="Preview/Before/720x1280.png"></a></div><div><h3>Revised</h3><a id="afterLink" href="Preview/After/720x1280.png"><img id="after" src="Preview/After/720x1280.png"></a></div></div><p>Open either image for its original capture resolution. Automated Unity testing, not physical-phone testing.</p>
<h2>Native assets · 1× and 3×</h2><div class="assets">'''+cards+'''</div><h2>Evidence</h2><p><a href="README.md">Validation report</a> · <a href="GenerationLedger.md">PixelLab ledger</a> · <a href="QA/Preservation.json">Protected-file checks</a> · <a href="QA/NativeQA.json">Native sprite checks</a></p><p>90 PixelLab generations. Raw candidates, selection reasons and all editable sources are preserved. No source art was deleted or overwritten.</p><script>function setSize(){let n=document.getElementById('size').value;for(let v of ['Before','After']){let id=v.toLowerCase(),url='Preview/'+v+'/'+n+'.png';document.getElementById(id).src=url;document.getElementById(id+'Link').href=url;}}</script>'''
(root/'Review.html').write_text(html,encoding='utf-8')
