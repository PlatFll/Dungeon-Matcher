"""Register visually reviewed patch decisions; no raster authoring."""
from pathlib import Path
import json,sys
ROOT=Path(__file__).resolve().parents[2];BASE=Path(__file__).resolve().parent
CHECKS=BASE/'edge-output-checks.json'
rejected={
 'BarricadeGuard_Ability-4c8b987235-10':'Duplicates wooden shafts; native edge completion required.',
 'BarricadeGuard_Ability-4c8b987235-11':'Unnecessary added flash particles; native contour cleanup.',
 'CourtMage_Ability-ccd68f17cc-8':'Extends an eastward effect; native west-facing correction.',
 'CourtMage_Ability-ccd68f17cc-9':'Superseded by the adjacent westward release drawing.',
 'CourtMage_Ability-ccd68f17cc-10':'Adds detached streak; native tapered effect completion.',
 'CourtMage_Ability-ccd68f17cc-11':'Adds noisy cyan fragments; native tapered effect completion.',
 'CourtMage_Ability-ccd68f17cc-12':'Adds unrelated particles; native cleanup.',
 'grand_delver_Death-3d5ba92519-6':'Edge contact is detached particles, not missing anatomy.',
 'hammerhead_bruiser_Hit-9b59614538-1':'Edge contact is detached particles; native component cleanup.',
 'Orc_Trailguard_AutoAttack-a128ca25c0-5':'Duplicates forearm and cuff; native rounded cuff completion.',
 'obsidian_sentinel_SlamRelease-6c270a8cb3-3':'Adds a new dust puff; native effect taper.',
 'obsidian_sentinel_SlamRelease-6c270a8cb3-5':'Detached dust only; native cleanup.',
 'RoyalArchbishop_Ability-a6c87da263-10':'Opaque white column artifact; reject.',
 'RoyalArchbishop_Ability-a6c87da263-6':'Unnecessary fragmented effect expansion.',
 'RoyalArchbishop_Ability-a6c87da263-12':'Unnecessary new particles.',
 'RoyalArchbishop_Ability-a6c87da263-14':'Unnecessary new particles.',
 'RoyalArchbishop_Benediction-34274aa6a5-4':'Adds internal ring hatching inconsistent with simple spell planes.',
 'RoyalArchbishop_Benediction-34274aa6a5-6':'Adds a grid of particles; reject.',
 'RoyalArchbishop_Benediction-34274aa6a5-10':'Unnecessary new particles.',
 'RoyalStandardBearer_Ability-a5068cfb53-1':'Duplicates banner; restore only clipped steel finial natively.',
 'RoyalStandardBearer_Ability-a5068cfb53-2':'Duplicates banner; restore only clipped steel finial natively.',
 'RoyalStandardBearer_Ability-a5068cfb53-3':'Duplicates banner; restore only clipped steel finial natively.',
 'RoyalStandardBearer_Ability-a5068cfb53-4':'Duplicates banner; restore only clipped steel finial natively.',
 'RoyalSwordsman_AutoAttack-d46e01b0a3-4':'Rectangular cream extension; native curved trail required.',
 'RoyalSwordsman_AutoAttack-d46e01b0a3-5':'Unnecessary extra debris; native contour continuation.',
 'SiegeSergeant_Ability-1bd3a79105-5':'Duplicates hammer; restore only clipped top contour natively.',
 'SiegeSergeant_Ability-1bd3a79105-12':'Unnecessary extra fragment.',
 'TownMarshal_Ability-085a8c6c66-6':'Enlarges mace head into diamond; use a restrained contour.'
}
if '--record-first-103' in sys.argv:
 checks=json.loads(CHECKS.read_text());assert len(checks)==103,'Review checkpoint changed; do not auto-accept new results.'
 review={r['id']+'-'+r['side']:dict(status='rejected' if r['id'] in rejected else 'accepted',reason=rejected.get(r['id'],'Reviewed before/after at native integer scale; localized contour continuation preserves identity.')) for r in checks}
 (BASE/'repair_review.json').write_text(json.dumps(review,indent=2))
review=json.loads((BASE/'repair_review.json').read_text())
if '--record-last-21' in sys.argv:
 checks=json.loads(CHECKS.read_text());new=[r for r in checks if r['id']+'-'+r['side'] not in review];assert len(new)==21
 for r in new:
  reject=(r['id'].startswith('rivet_gunner_AutoAttack') or r['id'].startswith('rivet_turret_OreChargedAutoAttack') or r['id'].startswith('stonewright_') or r['id']=='rivet_gunner_OreChargedAutoAttack-5eb50f7c6-5')
  review[r['id']+'-'+r['side']]=dict(status='rejected' if reject else 'accepted',reason='Generated extra particles/flames or altered effect geometry; native contour completion instead.' if reject else 'Reviewed localized native-scale edge continuation; prop/contour remains coherent.')
 (BASE/'repair_review.json').write_text(json.dumps(review,indent=2))
overrides=json.loads((BASE/'frame_overrides.json').read_text())
for p in sorted((BASE/'RepairReceipts').glob('*.json')):
 r=json.loads(p.read_text());decision=review.get(r['id']+'-'+r['side'])
 if not decision or decision['status']!='accepted':continue
 pars=r['parameters'];patch=dict(path=r['output'],mask=[pars['mask_'+s] for s in ['x','y','width','height']],reason=decision['reason'])
 override=overrides.setdefault(r['id'],dict(patches=[]))
 override['patches']=[v for v in override['patches'] if v['mask']!=patch['mask']]+[patch]
for r in json.loads((BASE/'smith-repair-jobs.json').read_text()):
 pars=r['args'];overrides[r['id']]=dict(patches=[dict(path=r['output'],mask=[pars['mask_'+s] for s in ['x','y','width','height']],reason='Reviewed full dwarf and hammer at 4x; reconstructed clipped metal contour; zero edits outside mask.')])
# Copies only already-reviewed patches to byte-identical original drawings.
for p in json.loads((BASE/'edge-repair-plan.json').read_text()):
 if p.get('duplicate_of') in overrides:overrides[p['id']]=dict(overrides[p['duplicate_of']],duplicate_of=p['duplicate_of'])
(BASE/'frame_overrides.json').write_text(json.dumps(overrides,indent=2))
print(json.dumps(dict(reviewed=len(review),accepted=sum(v['status']=='accepted' for v in review.values()),overridden_frames=len(overrides))))
