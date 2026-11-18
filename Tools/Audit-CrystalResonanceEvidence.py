"""Audit recorded observations and native journals, not a substitute for Unity replay.
Usage: python Tools/Audit-CrystalResonanceEvidence.py BATCH --output REPORT.json
Only finished cases are included; incomplete cases are explicitly listed.
"""
import argparse,collections,csv,gzip,hashlib,json
from pathlib import Path

def lines(path):
    return [json.loads(x) for x in path.read_text(encoding='utf-8-sig').splitlines() if x.strip()]

def main():
    p=argparse.ArgumentParser(description=__doc__);p.add_argument('batch',type=Path);p.add_argument('--output',required=True,type=Path)
    a=p.parse_args();records=[];failures=[];incomplete=[]
    for case in sorted(a.batch.glob('*/case.json')):
        folder=case.parent
        if not (folder/'result.json').exists():incomplete.append(folder.name);continue
        events=lines(folder/'events.jsonl');actions=lines(folder/'actions.jsonl')
        visits=list(csv.DictReader((folder/'visits.csv').open()))
        local=[]
        def check(ok,message):
            if not ok:local.append(message)
        banks=[e for e in events if e['kind']=='victory_bank']
        check(len(banks)==len({e['encounter'] for e in banks}),'duplicate encounter bank')
        check(all(0<=e['charge']<=100 and 0<=e['pending']<=3 for e in events),'charge/pending bounds')
        check(all(x['score']<=y['score'] for x,y in zip(events,events[1:])),'score decreased')
        leaders=set();repeated=[]
        for e in events:
            if e['kind']!='formation':continue
            members=[x.split('/') for x in e['detail'].split(':',1)[1].split('+')]
            ids={m[0] for m in members}
            for identity,category,*_ in members:
                if category not in ('Miniboss','Boss'):continue
                if identity in leaders and not (identity=='royal_arcanist' and 'king' in ids):repeated.append(identity)
                leaders.add(identity)
            if e['zone']=='dungeon' and e['depth']>=13:
                check(not ids.intersection({'farmer','basket_villager','pan_villager','miner','barricade_villager'}),'late Dungeon local')
        check(not repeated,'repeated named leaders: '+','.join(repeated))
        for visit in visits:
            if visit['completed']!='true':continue
            selected=[e for e in events if e['visit']==int(visit['visit'])]
            scheduled=[e for e in selected if e['kind']=='capstone_scheduled']
            defeated=[e for e in selected if e['kind']=='capstone_defeated']
            travel=[e for e in selected if e['kind']=='travel_pending']
            check(len(scheduled)==len(defeated)==len(travel)==1,'completed visit lacks one full capstone/travel sequence')
            if scheduled and defeated and travel:
                check(scheduled[0]['detail']==defeated[0]['detail'],'different capstone defeated')
                check(scheduled[0]['wall']<=defeated[0]['wall']<=travel[0]['wall'],'capstone/travel ordering')
                check(travel[0]['charge']==100 and travel[0]['capstonePhase']==3,'travel without 100/defeated seal')
            picks=[e['value'] for e in selected if e['kind']=='card_selected']
            check(picks==[12,40,70],'completed visit draft sequence: '+str(picks))
        requested=collections.Counter()
        for action in actions:
            name=action['action'].split(':',1)[0]
            if name in ('swap','ability','potion','bomb','card'):requested[name]+=1
        native=collections.Counter();boundaries=0
        journal=folder/'native-journal.jsonl.gz'
        try:
            with gzip.open(journal,'rt',encoding='utf-8') as stream:
                for line in stream:
                    boundary=json.loads(line);boundaries+=1
                    tape=boundary['tape'];checkpoint=boundary['checkpoint']
                    check(tape['boardFeelVersion']==1 and checkpoint['version']==2,'unexpected clock/tape profile')
                    accepted=[act for frame in tape['frames'] for act in frame['actions']]
                    check(bool(accepted),'native boundary missing accepted action')
                    if accepted:native[{0:'swap',1:'ability',2:'potion',3:'bomb',4:'card',5:'refine'}[accepted[-1]['kind']]]+=1
        except (OSError,ValueError,KeyError) as error:local.append('journal parse: '+str(error))
        check(native==requested,'requested vs durable accepted commands differ: '+str((dict(requested),dict(native))))
        records.append(dict(case=folder.name,nativeBoundaries=boundaries,nativeActions=dict(native),
                            journalSha256=hashlib.sha256(journal.read_bytes()).hexdigest(),failures=local))
        failures.extend(folder.name+': '+item for item in local)
    result=dict(batch=str(a.batch),kind='offline recorded-evidence audit; NOT replay execution',
                finishedCases=len(records),incompleteCases=incomplete,failures=failures,cases=records)
    a.output.parent.mkdir(parents=True,exist_ok=True);a.output.write_text(json.dumps(result,indent=2)+'\n',encoding='utf-8')
    print(json.dumps(dict(finishedCases=len(records),incomplete=incomplete,failures=failures,
                         nativeBoundaries=sum(r['nativeBoundaries'] for r in records)),indent=2))
    raise SystemExit(bool(failures))
if __name__=='__main__':main()
