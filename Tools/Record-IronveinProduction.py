"""Build an auditable budget/register from saved PixelLab request receipts."""
from pathlib import Path
import csv
import json
import re

ROOT=Path(__file__).resolve().parents[1]
P=ROOT/'ArtSource/Ironvein/Production'
jobs={}
def scan(node,path):
    if isinstance(node,dict):
        if node.get('job_id') and node.get('estimated_generations') is not None:
            jobs.setdefault(node['job_id'],dict(tool='Pro Flash',cost=float(node['estimated_generations']),source=path.name))
        if node.get('type')=='text':
            t=node.get('text','')
            # Pro Flash embeds a JSON receipt before its human-readable polling hint.
            if t.lstrip().startswith('{'):
                try:
                    embedded,_=json.JSONDecoder().raw_decode(t.lstrip());scan(embedded,path)
                except (ValueError,TypeError): pass
            id=re.search(r'job_id[:=]\s*["\']?([0-9a-f-]{36})',t)
            cost=re.search(r'cost:\s*([\d.]+) generation',t)
            if id and cost:
                tool='PixelLab Pixen' if 'model: pixen' in t else 'PixelLab motion'
                jobs.setdefault(id[1],dict(tool=tool,cost=float(cost[1]),source=path.name))
        for value in node.values(): scan(value,path)
    elif isinstance(node,list):
        for value in node: scan(value,path)
for path in sorted(P.glob('*.json')):
    scan(json.loads(path.read_text(encoding='utf-8')),path)
# Older receipts used text-only response shapes; retain their exact quoted costs.
for id,tool,cost,source in [
    ('1db5331f-f3ce-4a2f-abfd-7c5534230cc2','correct_pixelart',.1,'pilot-corrections.json'),
    ('ab6a5799-981b-403f-a6ef-a30a768411d2','correct_pixelart',.1,'pilot-corrections.json'),
    ('fb5068e8-399f-4076-b1cb-233b07c09f5b','animate_image',1,'powered-pilot.json'),
    ('7bd000de-f954-4789-bdba-2a87440a01af','animate_image',1,'powered-pilot.json')]:
    jobs[id]=dict(tool=tool,cost=cost,source=source)
total=sum(r['cost'] for r in jobs.values())
assert total<=300, 'Initial batch allowance exhausted; reserve requires review decision.'
ledger=ROOT/'Docs/IronveinExcavation/GENERATION_LEDGER.csv'
with ledger.open('w',encoding='utf-8',newline='') as f:
    writer=csv.writer(f)
    writer.writerow(['date','phase','tool','request_id','asset','status','subscription_generations','notes'])
    writer.writerow(['2026-10-08','01','get_balance','','service','verified',0,'Initial 1360 remaining / 640 used / 2000 total; purchased credits zero'])
    for id,r in sorted(jobs.items()):
        status='receipt recorded'
        if id=='fb5068e8-399f-4076-b1cb-233b07c09f5b': status='cancelled; refund unconfirmed; budget retained'
        if id in ['1db5331f-f3ce-4a2f-abfd-7c5534230cc2','ab6a5799-981b-403f-a6ef-a30a768411d2']: status='rejected palette noise'
        phase='09' if r['source'].startswith(('scene-','ui-','mechanics-')) else '08'
        writer.writerow(['2026-10-08',phase,r['tool'],id,r['source'],status,r['cost'],'Quoted provider cost; includes cancelled request conservatively; exact edits free'])
summary=dict(ceiling=400,initial=300,reserve=100,committed_quoted=round(total,2),requests=len(jobs),
             note='Provider integer balance rounds fractional usage; no credit purchases. Reserve untouched.')
(P/'budget-summary.json').write_text(json.dumps(summary,indent=2)+'\n',encoding='utf-8')
print(json.dumps(summary))
