"""Compare actual first-visit cases by seed/profile; never substitute deaths for completions.
Usage: python Tools/Compare-CrystalResonance.py --baseline SUMMARY [SUMMARY ...] --candidate SUMMARY --output DIR
Later baseline summaries take precedence for repeated profiles (e.g. a corrected policy).
"""
import argparse,csv,json
from pathlib import Path

def key(row):
    return tuple(row[name] for name in ('zone','seed','character','level','policy'))

def main():
    p=argparse.ArgumentParser(description=__doc__)
    p.add_argument('--baseline',nargs='+',required=True,type=Path)
    p.add_argument('--candidate',required=True,type=Path)
    p.add_argument('--output',required=True,type=Path)
    a=p.parse_args();baseline={}
    for path in a.baseline:
        for row in json.loads(path.read_text())['visits']:
            if row['visit']==0 and 'arrival only' not in row['outcome']:baseline[key(row)]=row
    candidate=json.loads(a.candidate.read_text());records=[]
    for row in candidate['visits']:
        if row['visit']!=0 or 'arrival only' in row['outcome']:continue
        old=baseline.get(key(row))
        record={name:row[name] for name in ('label','zone','seed','character','level','policy')}
        record.update(baselineBatch=old['batch'] if old else None,candidateBatch=row['batch'],
            baselineOutcome=old['outcome'] if old else 'new seed/profile',candidateOutcome=row['outcome'],
            baselineSeconds=old['wallSeconds'] if old else None,candidateSeconds=row['wallSeconds'],
            completedTimeDelta=round(row['wallSeconds']-old['wallSeconds'],3) if old and old['completed'] and row['completed'] else None,
            baselineEncounters=old['bankedEncounters'] if old else None,candidateEncounters=row['bankedEncounters'],
            baselineFirstCard=old['firstCardSeconds'] if old else None,candidateFirstCard=row['firstCardSeconds'],
            baselineHealth=old['health'] if old else None,candidateHealth=row['health'],
            baselineCapstones=old['capstones'] if old else [],candidateCapstones=row['capstones'])
        records.append(record)
    a.output.mkdir(parents=True,exist_ok=True)
    payload=dict(method='First visits matched by exact zone/seed/character/level/policy; deaths separate; only completed pairs receive a duration delta.',
        baselineSummaries=[str(p) for p in a.baseline],candidateSummary=str(a.candidate),cases=records)
    (a.output/'comparison.json').write_text(json.dumps(payload,indent=2)+'\n',encoding='utf-8')
    columns=[k for k in records[0] if 'Capstones' not in k] if records else []
    with (a.output/'comparison.csv').open('w',encoding='utf-8',newline='') as f:
        w=csv.DictWriter(f,fieldnames=columns,extrasaction='ignore');w.writeheader();w.writerows(records)
    lines=['# Constant charge candidate: actual input comparison','',payload['method'],'',
        '| Zone / policy / level / seed | Baseline outcome / seconds | Candidate outcome / seconds | Completed delta s | Encounters before / after | First card before / after s |',
        '|---|---|---|---|---|---|']
    for r in records:
        lines.append(f"| {r['zone']} / {r['policy']} / L{r['level']} / {r['seed']} | {r['baselineOutcome']} / {r['baselineSeconds']} | {r['candidateOutcome']} / {r['candidateSeconds']} | {r['completedTimeDelta']} | {r['baselineEncounters']} / {r['candidateEncounters']} | {r['baselineFirstCard']} / {r['candidateFirstCard']} |")
    lines+=['','Policies: 0 novice, 1 typical, 2 skilled, 3 defensive, 4 aggressive.',
        'Elapsed time can include different natural formations/boards after changed progression. This is not a controlled claim that only animation time changed.',
        'Do not pool account levels or include failed visits in completed-visit pacing medians.']
    (a.output/'comparison.md').write_text('\n'.join(lines)+'\n',encoding='utf-8')
    print(json.dumps(dict(cases=len(records),completedPairs=sum(r['completedTimeDelta'] is not None for r in records),newProfiles=sum(r['baselineBatch'] is None for r in records)),indent=2))
if __name__=='__main__':main()
