"""Summarize actual x1 Unity proxy evidence, never synthesize completed visits.
Usage: python Tools/Analyze-CrystalResonance.py BATCH [BATCH ...] --output DIRECTORY
Only combine batches whose source manifests/configuration are intentionally comparable.
The report keeps failed, censored and unfinished cases separate from completed timings.
"""
import argparse,collections,csv,hashlib,json,statistics
from pathlib import Path

def read_rows(path):
    if not path.exists(): return []
    return [json.loads(line) for line in path.read_text(encoding='utf-8-sig').splitlines() if line.strip()]

def quantile(values,p):
    values=sorted(values);i=(len(values)-1)*p;lo=int(i);hi=min(lo+1,len(values)-1)
    return round(values[lo]+(values[hi]-values[lo])*(i-lo),3)

def distribution(rows,key):
    values=[r[key] for r in rows if r.get(key) is not None]
    if not values:return None
    return dict(n=len(values),median=round(statistics.median(values),3),p25=quantile(values,.25),
                p75=quantile(values,.75),minimum=round(min(values),3),maximum=round(max(values),3))

def main():
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument('batches',nargs='+',type=Path);parser.add_argument('--output',required=True,type=Path)
    args=parser.parse_args();visits=[];cases=[];sources=[]
    for batch in args.batches:
        manifest=batch/'source-manifest.json'
        sources.append(dict(batch=str(batch.resolve()),manifestSha256=hashlib.sha256(manifest.read_bytes()).hexdigest() if manifest.exists() else None,
                            runnerErrors=(batch/'errors.txt').exists(),completion=(batch/'completion.txt').read_text() if (batch/'completion.txt').exists() else 'RUNNING/INCOMPLETE'))
        for casepath in sorted(batch.glob('*/case.json')):
            folder=casepath.parent;case=json.loads(casepath.read_text());events=read_rows(folder/'events.jsonl');actions=read_rows(folder/'actions.jsonl')
            result=json.loads((folder/'result.json').read_text()) if (folder/'result.json').exists() else dict(outcome='RUNNING/INCOMPLETE')
            cases.append(dict(batch=batch.name,**case,**result))
            if not (folder/'visits.csv').exists():continue
            for row in csv.DictReader((folder/'visits.csv').open()):
                visit=int(row['visit']);selected=[e for e in events if e['visit']==visit]
                if not selected:continue
                banks=[e for e in selected if e['kind']=='victory_bank'];offers=[e for e in selected if e['kind']=='card_offer']
                formations=[e for e in selected if e['kind']=='formation'];capstones=[e for e in selected if e['kind']=='capstone_scheduled']
                bankparts=[dict(part.split('=',1) for part in e['detail'].split(';')) for e in banks]
                enemies=set();recipes=[]
                for e in formations:
                    recipe,members=e['detail'].split(':',1);recipes.append(recipe)
                    enemies.update(member.split('/')[0] for member in members.split('+'))
                durations=[]
                for defeated in [e for e in selected if e['kind']=='capstone_defeated']:
                    start=next((e for e in selected if e['kind']=='encounter_begin' and e['depth']==defeated['depth']),None)
                    if start:durations.append(round(defeated['wall']-start['wall'],3))
                decisions=[]
                for a in actions:
                    if a['visit']==visit and a['action'].startswith('decision:'):
                        decisions.append(dict(part.split('=',1) for part in a['action'][9:].split(';')))
                depth_events=[e for e in selected if e['kind'] in ('formation','victory_bank','encounter_begin')] or selected
                record=dict(batch=batch.name,label=case['label'],seed=case['seed'],character=case['character'],level=case['level'],policy=case['policy'],
                    zone=row['zone'],visit=visit,completed=row['completed']=='true',wallSeconds=float(row['wall_seconds']),
                    outcome='completed' if row['completed']=='true' else result['outcome'],startDepth=min(e['depth'] for e in depth_events),endDepth=max(e['depth'] for e in depth_events),
                    health=int(row['hp']),score=int(row['score']),bankedEncounters=len(banks),acceptedSwaps=sum(e['kind']=='move_accepted' for e in selected),
                    acceptedAbilities=sum(a['visit']==visit and a['action']=='ability' for a in actions),
                    acceptedPotions=sum(a['visit']==visit and a['action']=='potion' for a in actions),
                    acceptedBombs=sum(a['visit']==visit and a['action'].startswith('bomb:') for a in actions),
                    baseCharge=sum(int(p['base']) for p in bankparts),masteryCharge=sum(int(p['mastery']) for p in bankparts),overflow=sum(int(p['overflow']) for p in bankparts),
                    firstCardSeconds=round(offers[0]['wall']-selected[0]['wall'],3) if offers else None,
                    combatSeconds=selected[-1]['combatSeconds'],draftSeconds=selected[-1]['draftSeconds'],travelSeconds=selected[-1]['travelSeconds'],
                    playerDamage=sum(e['value'] for e in selected if e['kind']=='player_damage'),enemySpecials=sum(e['kind']=='enemy_special' for e in selected),
                    specialEnemies=sorted(set(e['detail'] for e in selected if e['kind']=='enemy_special')),uniqueEnemies=sorted(enemies),
                    recipes=recipes,distinctRecipes=len(set(recipes)),immediateRecipeRepeats=sum(a==b for a,b in zip(recipes,recipes[1:])),
                    cards=[e['detail'] for e in selected if e['kind']=='card_selected'],
                    capstones=[dict(id=e['detail'],kind=e['value'],scheduledAfterDepth=e['depth'],
                        encounterDepth=next((f['depth'] for f in formations if f['wall']>=e['wall'] and f['detail'].split(':',1)[0]==e['detail']),None)) for e in capstones],capstoneSeconds=durations,
                    telegraphDecisionPoints=sum(int(d['warnings'])>0 for d in decisions),
                    warningWithoutImmediateMarkedMatch=sum(int(d['warnings'])>0 and d['counterAvailable']!='True' for d in decisions),
                    visibleCounterOpportunities=sum(d['counterAvailable']=='True' for d in decisions),visibleCountersChosen=sum(d['counterChosen']=='True' for d in decisions),
                    lowAirOpportunities=sum(int(d['air']) in (0,1,2) and d['bubbleAvailable']=='True' for d in decisions),
                    lowAirResponses=sum(int(d['air']) in (0,1,2) and d['bubbleChosen']=='True' for d in decisions),
                    lowAirCofferOpportunities=sum(int(d['air']) in (0,1,2) and d.get('cofferAvailable')=='True' for d in decisions),
                    lowAirCofferResponses=sum(int(d['air']) in (0,1,2) and d.get('cofferChosen')=='True' for d in decisions),
                    cofferObservationRecorded=any('cofferAvailable' in d for d in decisions))
                # Policy-v2 diagnostics describe visible heuristics, not proven
                # tactical errors. Missing legacy fields stay explicitly absent.
                record['policyVersion']=result.get('policyVersion','legacy-full-scan')
                record['diagnosticStart']=case.get('diagnosticStart',case['zone']!='dungeon')
                modern=[d for d in decisions if 'noticed' in d]
                cast_decisions=[dict(part.split('=',1) for part in a['action'].split(':',1)[1].split(';'))
                    for a in actions if a['visit']==visit and a['action'].startswith('abilityDecision:')]
                card_decisions=[dict(part.split('=',1) for part in a['action'].split(':',1)[1].split(';'))
                    for a in actions if a['visit']==visit and a['action'].startswith('cardDecision:')]
                record['decisionQuality']=dict(
                    sampledMoves=len(modern),
                    meanAttentionFraction=round(statistics.mean(int(d['noticed'])/int(d['legal']) for d in modern),3) if modern else None,
                    heuristicSuboptimalMoves=sum(float(d['heuristicGap'])>=5 for d in modern),
                    heuristicGapTotal=round(sum(float(d['heuristicGap']) for d in modern),2),
                    markedCounterOverlooked=sum(d['counterAvailable']=='True' and d['counterNoticed']!='True' for d in modern),
                    markedCounterNotChosen=sum(d['counterAvailable']=='True' and d['counterChosen']!='True' for d in modern),
                    markedCounterSeenButNotChosen=sum(d['counterNoticed']=='True' and d['counterChosen']!='True' for d in modern),
                    priorityOpportunities=sum(d['priorityAvailable']=='True' for d in modern),
                    priorityNotChosen=sum(d['priorityAvailable']=='True' and d['priorityChosen']!='True' for d in modern),
                    specialCreatesSelected=sum(d['createsSpecial']=='True' for d in modern),
                    specialUsesSelected=sum(d['usesSpecial']=='True' for d in modern),
                    visibleFollowupSelections=sum(int(d['visibleFollowups'])>0 for d in modern),
                    smallRemainderCasts=sum(d['smallRemainder']=='True' for d in cast_decisions),
                    castsWithMarkedAnswerAvailable=sum(d['counterAvailable']=='True' for d in cast_decisions),
                    castsAtLowAir=sum(d['urgentAir']=='True' for d in cast_decisions),
                    offPreferenceCards=sum(d['offPreference']=='True' for d in card_decisions))
                record['encounterHistory']=[]
                for formation in formations:
                    bank=next((e for e in banks if e['depth']==formation['depth']),None)
                    local=[e for e in selected if e['depth']==formation['depth']]
                    beginning=next((e for e in local if e['kind']=='encounter_begin'),formation)
                    record['encounterHistory'].append(dict(depth=formation['depth'],formation=formation['detail'],
                        completed=bank is not None,seconds=round(bank['wall']-beginning['wall'],3) if bank else None,
                        acceptedSwaps=sum(e['kind']=='move_accepted' for e in local),
                        enemySpecials=[e['detail'] for e in local if e['kind']=='enemy_special'],
                        healthAtFormation=formation['health'],healthAtBank=bank['health'] if bank else None,
                        damage=sum(e['value'] for e in local if e['kind']=='player_damage')))
                record['deathCause']=next((e['detail'] for e in selected if e['kind']=='defeat'),None)
                if not record['completed'] and result['outcome']=='visit limit' and record['acceptedSwaps']==0 and record['bankedEncounters']==0:
                    record['outcome']='arrival only; next visit not attempted'
                record['startTier']=sum(record['startDepth']>=cut for cut in (6,13,25,45))
                record['endTier']=sum(record['endDepth']>=cut for cut in (6,13,25,45))
                visits.append(record)
    groups=collections.defaultdict(list)
    for row in visits:
        if row['completed']:
            groups[(row['zone'],'all','all')].append(row);groups[(row['zone'],str(row['policy']),str(row['level']))].append(row)
    stats=[]
    for (zone,policy,level),rows in sorted(groups.items()):
        stats.append(dict(zone=zone,policy=policy,level=level,timing=distribution(rows,'wallSeconds'),firstCard=distribution(rows,'firstCardSeconds'),
                          encounters=distribution(rows,'bankedEncounters'),manualSwaps=distribution(rows,'acceptedSwaps'),
                          levels=sorted(set(r['level'] for r in rows)),characters=sorted(set(r['character'] for r in rows)),
                          independentSeeds=len(set(r['seed'] for r in rows))))
    depth_groups=collections.defaultdict(list)
    coverage=collections.defaultdict(lambda:dict(completed=0,defeats=0,censored=0,seeds=set(),policies=set(),tiers=set(),naturalRevisits=0))
    for row in visits:
        c=coverage[row['zone']]
        if row['completed']:
            c['completed']+=1;c['seeds'].add(row['seed']);c['policies'].add(row['policy'])
            c['tiers'].update(range(row['startTier'],row['endTier']+1))
            earlier=[v for v in visits if v['batch']==row['batch'] and v['label']==row['label'] and v['visit']<row['visit'] and v['zone']==row['zone']]
            if earlier:c['naturalRevisits']+=1
            depth_groups[(row['zone'],row['policy'],row['level'],row['startTier'])].append(row)
        elif row['outcome']=='defeat':c['defeats']+=1
        elif row['outcome']=='censored duration limit':c['censored']+=1
    for c in coverage.values():
        for key in ('seeds','policies','tiers'):c[key]=sorted(c[key])
    depth_distributions=[dict(zone=z,policy=p,level=l,startTier=t,timing=distribution(rows,'wallSeconds'))
        for (z,p,l,t),rows in sorted(depth_groups.items())]
    args.output.mkdir(parents=True,exist_ok=True)
    payload=dict(sources=sources,cases=cases,visits=visits,distributions=stats,depthDistributions=depth_distributions,coverage=dict(coverage),
        limitations=['Actual Unity Game-scene legal-input proxy, not human enjoyment approval.',
        'Completed wall time includes thinking, card reading and travel; incomplete visits are excluded from timing distributions.',
        'First-card interval is measured from first observation of that visit; incoming reveal may overlap it.',
        'Enemy telegraph response counters cover visible marked-gem responses; they do not certify every attack as fair.',
        'Accepted swaps, abilities and consumables are separate; card picks do not consume a combat action.',
        'Warning-without-immediate-marked-match counts include environmental/cell warnings and alternative counters; they are not proof of unavoidable damage.',
        'Different levels/characters must be separated before interpreting skill effects.',
        'Policy-v2 suboptimal/priority/cast flags are visible heuristic diagnostics, not proof of a tactical mistake; specials or killing a caster can answer threats indirectly.',
        'Policy-v2 attention does not model touch error, memory, reading comprehension or human enjoyment. The skilled policy sees all visible legal responses, never hidden refill or future offers.',
        'Fresh-run IDs and capstone seeds are captured in initial-account/native journals. The named seed initializes board and encounter streams; matching named seeds alone does not prove identical whole runs.'])
    (args.output/'summary.json').write_text(json.dumps(payload,indent=2),encoding='utf-8')
    columns=['batch','label','seed','character','level','policy','zone','visit','completed','outcome','startDepth','endDepth',
             'wallSeconds','firstCardSeconds','bankedEncounters','acceptedSwaps','acceptedAbilities','acceptedPotions','acceptedBombs',
             'health','playerDamage','baseCharge','masteryCharge','overflow','enemySpecials','distinctRecipes','immediateRecipeRepeats',
             'visibleCounterOpportunities','visibleCountersChosen','lowAirOpportunities','lowAirResponses',
             'cofferObservationRecorded','lowAirCofferOpportunities','lowAirCofferResponses','policyVersion','diagnosticStart','deathCause']
    with (args.output/'visits.csv').open('w',encoding='utf-8',newline='') as stream:
        writer=csv.DictWriter(stream,fieldnames=columns,extrasaction='ignore');writer.writeheader();writer.writerows(visits)
    lines=['# Actual Unity proxy measurements','', '| Zone | Policy | Level | Completed | Seeds | Median seconds | p25 / p75 | Min / max |','|---|---|---|---:|---:|---:|---|---|']
    for s in stats:
        t=s['timing'];lines.append(f"| {s['zone']} | {s['policy']} | {s['level']} | {t['n']} | {s['independentSeeds']} | {t['median']} | {t['p25']} / {t['p75']} | {t['minimum']} / {t['maximum']} |")
    outcomes=collections.Counter(c['outcome'] for c in cases)
    lines+=['','Case outcomes: '+json.dumps(dict(outcomes)), '',
            'Policies: 0 novice, 1 typical, 2 skilled, 3 defensive, 4 aggressive.', '',
            '| Zone | Completed | Death visits | Censored | Independent seeds | Policies | Natural revisits |',
            '|---|---:|---:|---:|---:|---|---:|']
    for zone,c in sorted(coverage.items()):
        lines.append(f"| {zone} | {c['completed']} | {c['defeats']} | {c['censored']} | {len(c['seeds'])} | {c['policies']} | {c['naturalRevisits']} |")
    lines+=['',*payload['limitations']]
    (args.output/'summary.md').write_text('\n'.join(lines)+'\n',encoding='utf-8')
    print(json.dumps(dict(cases=len(cases),outcomes=dict(outcomes),completedVisits=sum(r['completed'] for r in visits),output=str(args.output.resolve())),indent=2))

if __name__=='__main__':main()
