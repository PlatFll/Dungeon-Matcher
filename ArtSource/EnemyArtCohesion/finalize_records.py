"""Record provenance and current reference extractions; never author sprite pixels."""
from pathlib import Path
import json,hashlib,shutil,re
from PIL import Image
ROOT=Path(__file__).resolve().parents[2];BASE=Path(__file__).resolve().parent
def read(p):return json.loads(p.read_text(encoding='utf-8-sig'))
def write(p,data):p.write_text(json.dumps(data,indent=2,ensure_ascii=False),encoding='utf-8')
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
def saved_input(p):return p if p.exists() else BASE/'RepairInputs'/p.name
def preserve(source,target):
 if source.resolve()!=target.resolve():shutil.copyfile(source,target)
jobs=read(BASE/'jobs.json');review=read(BASE/'repair_review.json');overrides=read(BASE/'frame_overrides.json')
ledger=read(BASE/'generation-ledger.json')
ledger.setdefault('guide_sha256_at_production',sha(ROOT/ledger['guide']))
ledger['authorization']='User requested all enemy cleanup, west-facing animation repair, PixelLab/LibreSprite, integration and merge; retained three-quarter view confirmed.'
ledger['cost_summary']={'paid_jobs':137,'charged_generations':815,'observed_final_remaining':223,'purchase_made':False}
saved=BASE/'RepairInputs';saved.mkdir(exist_ok=True)
for p in sorted((BASE/'RepairReceipts').glob('*.json')):
 r=read(p)
 if not r.get('output'):continue
 source=saved_input(ROOT/r['input']);copy=saved/source.name
 preserve(source,copy)
 r['saved_input']=copy.relative_to(ROOT).as_posix();r['input_sha256']=sha(copy);r['output_sha256']=sha(ROOT/r['output'])
 r['visual_decision']=review[r['id']+'-'+r['side']]
 r['status']='completed';r['charged_generations']=6
 r['endpoint']='inpaint_image_pro_flash';r['image_input_role']='image_base64: existing target cel, only the named edge mask is editable'
 write(p,r)
ledger['masked_repair_receipts']=[p.relative_to(BASE).as_posix() for p in sorted((BASE/'RepairReceipts').glob('*.json')) if read(p).get('output')]
smith=read(BASE/'smith-repair-jobs.json')
for r in smith:
 copy=saved/Path(r['input']).name;preserve(saved_input(ROOT/r['input']),copy)
 r.update(saved_input=copy.relative_to(ROOT).as_posix(),input_sha256=sha(copy),output_sha256=sha(ROOT/r['output']),charged_generations=6,status='completed; visually selected')
write(BASE/'smith-repair-jobs.json',smith)
for r in ledger['jobs']:
 target=saved_input(ROOT/r['target']);r['target_sha256']=sha(target)
 if r.get('reference'):r['reference_sha256']=sha(ROOT/r['reference']);r['reference_role']='Rattlebones actual image: rendering/cluster discipline; target remains identity authority'
 if r['job_id']=='7b09af8f-0e2a-4780-96d1-4844c980bbe6':
  r['status']='selected with native overhead-twig removal; reviewed all nine poses and source player';r['cost']=1
  r['image_input_role']='first_frame_base64: existing approved Snapvine identity'
 if r['job_id']=='9f6d7f57-120b-4cd8-bb87-e8bce559d97a':
  copy=saved/target.name;preserve(target,copy);r['saved_input']=copy.relative_to(ROOT).as_posix()
  override=overrides['seismic_smith_OreChargedAutoAttack-0956c99a07-5'];patch=override.get('patches',[override])[0]
  r['output']=patch['path'];r['cost']=6;r['status']='completed; visually selected'
 for key in ['output']:
  if r.get(key):
   out=ROOT/r[key] if r[key].startswith('ArtSource/') else BASE/r[key]
   if out.exists():r['output_sha256']=sha(out)
ledger['reference_register_at_production_sha256']=ledger.get('reference_register_at_production_sha256',sha(ROOT/'Docs/ArtDirection/Golden_Reference_Register.md'))
write(BASE/'generation-ledger.json',ledger)
for name in ['edge-repair-plan.json','edge-output-checks.json']:
 source=ROOT/'.utmp/EnemyArtCohesion'/name
 if source.exists():preserve(source,BASE/name)
plans=read(BASE/'edge-repair-plan.json');entries=[];unclassified=[]
for p in plans:
 for side in p['sides']:
  key=p.get('duplicate_of',p['id']);decision=review.get(key+'-'+side)
  reason=None
  if decision and decision['status']=='accepted':reason='Selected masked continuation, then native material/facing finishing.'
  elif p['actor']=='seismic_smith':reason='Selected Smith hammer completion; native beard and material finishing.'
  elif p['actor']=='grand_delver_reserve' and any(s[0]=='Remount' for s in p['states']):reason='Native full incoming suit/dolly reconstruction on 288x144 canvas.'
  elif p['actor']=='puffer_sentinel':reason='Westward complete-pose correction / detached seam or hit-particle cleanup; complete silhouettes in expanded canvas.'
  elif p['actor']=='royal_arcanist' and side=='top':reason='Intact outlineless ring apex / small ray endpoint; reviewed complete contour, now clear of canvas edge.'
  elif p['actor']=='siege_machinist' and side=='top':reason='Small isolated spark endpoints, no missing anatomy; retained with transparent clearance.'
  elif p['actor']=='obsidian_sentinel' and side=='right':reason='Native three-pixel continuation of existing dust tips, without adding separate puffs.'
  elif p['actor']=='grand_delver_pilot' and any(s[0]=='Eject' for s in p['states']):reason='Single-pixel metal debris endpoint during suit breakup; intact fragment retained with expanded clearance.'
  elif decision:reason='Rejected generated patch: '+decision['reason']+' See nativeEdgeDetails and source review.'
  if not reason:unclassified.append((p['id'],side))
  entries.append(dict(id=p['id'],actor=p['actor'],side=side,decision=reason,source_fileID=str(p['source']['fileID'])))
write(BASE/'edge-decisions.json',{'scope':'168 colored-edge source entries, including identical poses and multiple sides. Other dark-only contacts were intact outlines; all 270 contacts received expanded margin. Clear outer margins alone do not prove reconstruction.','entries':entries,'unclassified':unclassified})
print('Recorded lineage; unclassified edge cases:',unclassified)
