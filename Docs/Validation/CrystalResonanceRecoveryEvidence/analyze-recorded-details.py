"""Read closed Unity cases only; separate sources, never infer success from launch."""
from pathlib import Path
import json,collections,sys,statistics
B=Path(sys.argv[1]);O=Path(sys.argv[2]);O.mkdir(parents=True,exist_ok=True)
def rows(p):return [json.loads(s) for s in p.read_text(encoding='utf-8-sig').splitlines() if s.strip()]
def special(formation):return {m.split('/')[0] for m in formation.split(':',1)[1].split('+') if '/Normal/' not in m}
cases=[];visits=[]
for cp in sorted(B.glob('*/case.json')):
 p=cp.parent
 if not (p/'result.json').exists():continue
 config=json.loads(cp.read_text());result=json.loads((p/'result.json').read_text());ev=rows(p/'events.jsonl');ac=rows(p/'actions.jsonl')
 case=dict(config=config,result=result,route=[],sourceBatch=B.name);cases.append(case)
 for visit in sorted({e['visit'] for e in ev}):
  e=[x for x in ev if x['visit']==visit];a=[x for x in ac if x['visit']==visit];f=[x for x in e if x['kind']=='formation'];banks=[x for x in e if x['kind']=='victory_bank']
  if not f:continue
  case['route'].append(e[0]['zone']);dec=[dict(x.split('=',1) for x in q['action'][9:].split(';')) for q in a if q['action'].startswith('decision:')]
  damage=collections.Counter();hits=collections.Counter()
  for q in e:
   if q['kind']=='player_damage':
    name=q['detail'].split('\n')[0].removeprefix('Last hit: ') or 'unattributed legacy';damage[name]+=q['value'];hits[name]+=1
  record=dict(label=p.name,config=config,visit=visit,zone=e[0]['zone'],startDepth=min(x['depth'] for x in f),endDepth=max(x['depth'] for x in f),
   completed=any(x['kind']=='travel_pending' for x in e),seconds=e[-1]['visitSeconds'],damageBySource=dict(damage),hitsBySource=dict(hits),
   cards=[dict(milestone=x['value'],seconds=x['wall']-e[0]['wall']) for x in e if x['kind']=='card_offer'],
   cardsChosen=[x['detail'] for x in e if x['kind']=='card_selected'],
   chargeBanks=[dict(depth=x['depth'],charge=x['charge'],detail=x['detail']) for x in banks],
   death=next((x['detail'] for x in e if x['kind']=='defeat'),None),
   lowAir=dict(decisions=sum(int(d['air']) in (0,1,2) for d in dec),
    knownResponses=sum(int(d['air']) in (0,1,2) and (d['bubbleAvailable']=='True' or d.get('cofferAvailable')=='True') for d in dec),
    chosenResponses=sum(int(d['air']) in (0,1,2) and (d['bubbleChosen']=='True' or d.get('cofferChosen')=='True') for d in dec)),
   finalDecisions=dec[-6:],finalDamage=[x for x in e if x['kind']=='player_damage'][-6:],encounters=[],adjacentSpecialRepeats=[],postCapstones=[])
  for q in f:
   local=[x for x in e if x['depth']==q['depth']];bank=next((x for x in local if x['kind']=='victory_bank'),None);start=next((x for x in local if x['kind']=='encounter_begin'),q)
   record['encounters'].append(dict(depth=q['depth'],formation=q['detail'],capstone=q['capstonePhase']==2,
    seconds=bank['wall']-start['wall'] if bank else None,moves=sum(x['kind']=='move_accepted' for x in local),
    specials=[x['detail'] for x in local if x['kind']=='enemy_special'],damage=sum(x['value'] for x in local if x['kind']=='player_damage'),
    hpStart=q['health'],hpEnd=bank['health'] if bank else None,chargeEnd=bank['charge'] if bank else None))
  for x,y in zip(record['encounters'],record['encounters'][1:]):
   shared=special(x['formation'])&special(y['formation'])
   if shared:record['adjacentSpecialRepeats'].append(dict(previous=x,next=y,shared=sorted(shared)))
   if x['capstone']:record['postCapstones'].append(dict(previous=x,next=y,pressure=bool(special(y['formation']))))
  visits.append(record)
summary=dict(batch=str(B),closedCases=cases,visits=visits,zones={})
for zone in sorted({v['zone'] for v in visits}):
 vs=[v for v in visits if v['zone']==zone];en=[e for v in vs for e in v['encounters']];caps=[e for e in en if e['capstone']];post=[x for v in vs for x in v['postCapstones']];damage=collections.Counter()
 for v in vs:damage.update(v['damageBySource'])
 summary['zones'][zone]=dict(attempted=len(vs),completed=sum(v['completed'] for v in vs),encounters=len(en),
  adjacentPairs=sum(max(0,len(v['encounters'])-1) for v in vs),adjacentSpecialRepeats=sum(len(v['adjacentSpecialRepeats']) for v in vs),
  ordinaryIncomingPairs=sum(not e['capstone'] for v in vs for e in v['encounters'][1:]),
  ordinaryIncomingSpecialRepeats=sum(not e['next']['capstone'] for v in vs for e in v['adjacentSpecialRepeats']),
  postCapstones=len(post),postCapPressure=sum(x['pressure'] for x in post),damageBySource=dict(damage),
  specialCasts=dict(collections.Counter(s for e in en for s in e['specials'])),capstones=caps,
  formations=dict(collections.Counter(e['formation'] for e in en)),
  firstCardSeconds=[v['cards'][0]['seconds'] for v in vs if v['cards']],
  lowAir={k:sum(v['lowAir'][k] for v in vs) for k in ('decisions','knownResponses','chosenResponses')})
(O/'deep-analysis.json').write_text(json.dumps(summary,indent=2),encoding='utf-8')
print('CLOSED',len(cases),'VISITS',len(visits))
for z,d in summary['zones'].items():print(z,json.dumps({k:v for k,v in d.items() if k not in ('formations','capstones','firstCardSeconds')}))
