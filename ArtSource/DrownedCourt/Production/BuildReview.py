"""Package actual native assets and previews for offline review. No image generation."""
from pathlib import Path
from PIL import Image,ImageDraw
import json,shutil,hashlib,re,html,zipfile
r=Path(__file__).resolve().parent;repo=r.parents[2];out=r/'Review';out.mkdir(exist_ok=True)
selected=r/'Selected';motion=selected/'Motion';manifest=json.loads((motion/'animation-manifest.json').read_text())
checks=json.loads((motion/'NativeChecks.json').read_text())
ids=list(dict.fromkeys(c['name'] for c in manifest['clips']))
for sub in ['stills','motion','materials','screens','audio','manifests','docs','evidence']:(out/sub).mkdir(exist_ok=True)
for name in ids:shutil.copy2(selected/(name+'.png'),out/'stills')
for p in motion.rglob('*'):
 if p.is_file() and p.suffix in ['.png','.gif']:
  dest=out/'motion'/p.relative_to(motion);dest.parent.mkdir(parents=True,exist_ok=True);shutil.copy2(p,dest)
for p in (selected/'UI').glob('*.png'):shutil.copy2(p,out/'materials')
for p in (repo/'.utmp/DrownedCourtValidation').glob('*.png'):shutil.copy2(p,out/'screens')
for p in (repo/'.utmp/DrownedCourtValidation').glob('*.mp4'):shutil.copy2(p,out/'screens')
for p in (repo/'.utmp/DrownedCourtValidation').glob('*.gif'):shutil.copy2(p,out/'screens')
shutil.copy2(repo/'Assets/_Game/Audio/Music/Drowned Court - TEMP review.wav',out/'audio')
for p in [motion/'NativeChecks.json',motion/'animation-manifest.json',selected/'StillManifest.json',selected/'UI/MaterialManifest.json',r/'MusicProvenance.json',r/'FeedbackProvenance.json',r/'ExportChecks.json']:
 if p.exists():shutil.copy2(p,out/'manifests')
for source,dest in [('Docs/DROWNED_COURT.md','GameplayContract.md'),
                    ('Docs/Validation/DROWNED_COURT.md','CompletionAndValidation.md'),
                    ('Docs/ArtDirection/Drowned_Court_Reference_Register.md','ArtReferenceRegister.md')]:
 shutil.copy2(repo/source,out/'docs'/dest)
for name in ['TravelSoak.txt','PresentationProfile.json','ControlRuns.csv','VideoProvenance.json','ValidationEvidence.json']:
 p=repo/'.utmp/DrownedCourtValidation'/name
 if p.exists():shutil.copy2(p,out/'evidence')
evidence=repo/'.utmp/DrownedCourtValidation/ValidationEvidence.json'
if evidence.exists():
 for source in json.loads(evidence.read_text()).get('records',[]):
  p=repo/source
  if p.exists():shutil.copy2(p,out/'evidence')
(out/'README.txt').write_text('''THE DROWNED COURT — OFFLINE REVIEW
Open Review.html in a browser. All images, animated GIFs, video and temporary
audio are local files in this ZIP. No server or network is needed.

Unity checkout: C:/UnityProjects/Dungeon Matcher
Branch: codex/drowned-court-implementation
Editor: Unity 6000.3.19f1. Open Assets/_Game/Scenes/MainMenu.unity.
Play -> The Drowned Court selects a NEW test run. Continue restores its own zone.
In-run crystal destinations remain random. The testing picker is temporary.

docs/CompletionAndValidation.md maps all twelve phases and lists actual test
evidence and open gates. docs/GameplayContract.md lists the roster and mechanics.
manifests/ contains native checks, source selection and generation accounting.
evidence/ contains original test XML and measured summaries where available.

Only the first four still designs are explicitly user-approved. Later motion,
materials and ten stills are director-selected. Human pacing, music listening,
and physical Android testing remain pending. This package is not a release build.
''',encoding='utf-8')
# A native comparison supplements, but never substitutes for, individual PNG checks.
sheet=Image.new('RGB',(7*136,2*116),(84,84,84));d=ImageDraw.Draw(sheet)
for i,name in enumerate(ids):
 im=Image.open(selected/(name+'.png')).convert('RGBA');x=i%7*136+(136-im.width)//2;y=i//7*116+96-im.height
 sheet.paste(im,(x,y),im);d.text((i%7*136+2,i//7*116+98),name.replace('_',' ')[:22],fill='white')
sheet.save(out/'NativeRoster.png')
jobs=[];sum_cost=0
for p in sorted((r/'Jobs').glob('*.json')):
 if p.stem.endswith('_completion'):continue
 doc=json.loads(p.read_text(encoding='utf-8-sig'));txt=json.dumps(doc)
 # Record only service-reported job and cost fields, never reference image data.
 response=doc.get('result',doc);structured=response.get('structuredContent',{})
 costs=re.findall(r'cost: (\d+) generation',txt);job=re.search(r'job_id: ([a-f0-9-]+)',txt)
 identity=structured.get('job_id') or (job.group(1) if job else None)
 cost=int(costs[-1]) if costs else int(structured.get('estimated_generations',doc.get('quoted_units',0)))
 if identity:jobs.append(dict(record=p.name,job_id=identity,quoted_cost=cost));sum_cost+=cost
ledger=dict(phase='Drowned Court production (separate from Phase 2)',approved_total=300,initial_limit=220,reserved=80,
 starting_remaining=1542,measured_remaining=1360,actual_used=182,unused_initial=38,reserve_used=0,
 no_credit_purchases=True,quoted_job_cost_sum=sum_cost,
 accounting_note='Balance delta is authoritative actual usage. Per-job figures are provider quotes where completion metadata does not disclose a final debit.',jobs=jobs)
(r/'GenerationLedger.json').write_text(json.dumps(ledger,indent=2));shutil.copy2(r/'GenerationLedger.json',out/'manifests')
cards=[]
for name in ids:
 states=[c for c in manifest['clips'] if c['name']==name]
 previews=''.join('<figure><img src="motion/'+name+'/'+c['state']+'.gif"><figcaption>'+c['state']+' · '+str(sum(c['durationsMs']))+' ms</figcaption></figure>' for c in states)
 cards.append('<section><h2>'+name.replace('_',' ').title()+'</h2><a href="stills/'+name+'.png">Native still PNG</a><div class="clips">'+previews+'</div></section>')
screens=''.join('<figure><a href="screens/'+p.name+'"><img class="screen" src="screens/'+p.name+'"></a><figcaption>'+html.escape(p.stem)+'</figcaption></figure>' for p in sorted((out/'screens').glob('*.png')))
page='''<!doctype html><meta charset="utf-8"><title>Drowned Court — production review</title><style>
body{margin:32px auto;max-width:1250px;background:#18262c;color:#e9e1c9;font:16px system-ui;line-height:1.5}h1,h2{color:#f1d19d}a{color:#94deeb}section{padding:20px;background:#243840;margin:20px 0;border-radius:8px}.clips{display:flex;flex-wrap:wrap;gap:14px}figure{margin:8px;text-align:center}figure img{image-rendering:pixelated;width:240px;height:240px;object-fit:contain;background:#545454}figure img.screen{width:216px;height:auto}figcaption{font-size:13px}header img{image-rendering:pixelated;max-width:100%}.note{background:#334047;padding:16px}
</style><header><h1>The Drowned Court</h1><p>Actual PNG sprites and animated GIFs. Open this file locally; no server or internet is required.</p>
<p class="note">The first four designs are user-approved with solid-color eyes. The remaining designs, motion and materials are director-selected for your review. Temporary original music awaits listening approval. Native comparison is a visual aid; individual PNGs and manifests establish dimensions, alpha, colors and timing.</p>
<p>Production usage: <strong>182 / 300</strong> subscription generations; 182 / 220 initial, reserve untouched. Phase 2 usage was separate.</p><p><a href="README.txt">Access instructions</a> · <a href="docs/CompletionAndValidation.md">Phase completion and validation</a> · <a href="docs/GameplayContract.md">Kits and rules</a></p><img src="NativeRoster.png"><p><a href="manifests/GenerationLedger.json">Budget ledger</a> · <a href="manifests/NativeChecks.json">Native motion checks and selected frame indices</a> · <a href="manifests/StillManifest.json">Still provenance</a></p></header>'''+''.join(cards)+'<section><h2>Unity captures</h2><div class="clips">'+screens+'</div><video controls loop muted style="max-width:360px" src="screens/FloodPresentation.mp4"></video><p>Actual Unity rise/drain presentation fixture. Silent; no human playtest claim.</p></section><section><h2>Original temporary cue</h2><audio controls loop src="audio/Drowned Court - TEMP review.wav"></audio><p>Composed locally. No human listening or final music approval is claimed.</p></section>'
(out/'Review.html').write_text(page,encoding='utf-8')
hashes={str(p.relative_to(out)).replace('\\','/'):hashlib.sha256(p.read_bytes()).hexdigest() for p in out.rglob('*') if p.is_file() and p.name!='Hashes.json'}
(out/'Hashes.json').write_text(json.dumps(hashes,indent=2))
zip_path=r/'DrownedCourt_Production_Review.zip'
with zipfile.ZipFile(zip_path,'w',zipfile.ZIP_DEFLATED) as z:
 for p in out.rglob('*'):
  if p.is_file():z.write(p,p.relative_to(out))
print(json.dumps({'clips':len(manifest['clips']),'jobs':len(jobs),'reported_costs':sum_cost,'actual_used':182,'files':len(hashes),'zip_mib':round(zip_path.stat().st_size/1048576,2)}))
