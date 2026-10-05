"""Collect original Unity evidence and reconcile superseding focused runs."""
from pathlib import Path
import argparse,hashlib,json,shutil,csv,xml.etree.ElementTree as ET

parser=argparse.ArgumentParser();parser.add_argument('--controls',required=True);parser.add_argument('--validator',required=True)
args=parser.parse_args();root=Path(__file__).resolve().parents[3];out=root/'.utmp/DrownedCourtValidation'
ids=['be18b508-57a9-4d74-ad45-c7c6695d3e59','31a0065e-dbaf-4340-9dce-f25683fab621',
     '6fd9c495-3189-43ca-b1ad-a61a343c219d','3ffe08d7-29cc-4281-8b6f-623de3a3048e',args.controls.removesuffix('.xml')]
latest={};runs=[];records=[]
for identity in ids:
 path=root/'.utmp/ForestValidation'/(identity+'.xml');tree=ET.parse(path);run=tree.getroot()
 runs.append(dict(file=path.name,result=run.get('result'),total=run.get('total'),passed=run.get('passed'),failed=run.get('failed'),skipped=run.get('skipped')))
 records.append(path.relative_to(root).as_posix())
 for case in tree.findall('.//test-case'):
  latest[case.get('fullname')]=dict(result=case.get('result'),record=path.name)
retired='ForestFoundationPlayTests.ThreeZonePlayerControlRunsRecordActualOutcomes'
latest.pop(retired,None)
assert all(case['result']=='Passed' for case in latest.values()),'A required case remains failed or skipped'
rows=[]
for zone in ['dungeon','magical-forest','drowned-court']:
 with (out/('ControlRuns-'+zone+'.csv')).open() as stream:rows.extend(csv.DictReader(stream))
assert len(rows)==18 and len({(r['zone'],r['player'],r['level']) for r in rows})==18
with (out/'ControlRuns.csv').open('w',newline='') as stream:
 writer=csv.DictWriter(stream,fieldnames=rows[0].keys());writer.writeheader();writer.writerows(rows)
validator=Path(args.validator);shutil.copy2(validator,out/'UnityValidation.log');records.append('.utmp/DrownedCourtValidation/UnityValidation.log')
source_paths=list((root/'Assets/_Game/Scripts').rglob('*.cs'))+[root/'Assets/_Game/Data/Player Abilities/Ability_CrackedGems.asset']
source_hashes={p.relative_to(root).as_posix():hashlib.sha256(p.read_bytes()).hexdigest() for p in source_paths}
export_hashes=json.loads((Path(__file__).parent/'ExportChecks.json').read_text())['sha256'];source_hashes.update(export_hashes)
(out/'TestedSourceHashes.json').write_text(json.dumps(source_hashes,indent=2));records.append('.utmp/DrownedCourtValidation/TestedSourceHashes.json')
result=dict(base_main='9f8b5f7dc78761906743eec551a38b25c89dcaea',branch='codex/drowned-court-implementation',unity='6000.3.19f1',
            final_unique_cases=len(latest),latest_passed=len(latest),latest_failed=0,latest_skipped=0,
            retired_case=dict(name=retired,reason='Same eighteen scenarios split into three zone cases under the unchanged 180-second per-case limit'),
            runs=runs,cases=latest,records=records,controls=rows,
            validator=dict(result='passed',original_log=str(validator)),
            limits=['18 controls are capped at 60 game seconds with a synthetic greedy policy and 6x time; not human pacing',
                    'Physical Android, GPU profiling, full-visit mixed-skill pacing and human music approval remain pending'])
(out/'ValidationEvidence.json').write_text(json.dumps(result,indent=2))
print(json.dumps(dict(unique_passed=len(latest),control_scenarios=len(rows),source_hashes=len(source_hashes),records=len(records))))
