"""Copy actual review evidence and compact executed-test records into the PR."""
from pathlib import Path
import argparse,hashlib,json,shutil,xml.etree.ElementTree as ET

ROOT=Path(__file__).resolve().parents[1]
parser=argparse.ArgumentParser();parser.add_argument('--result',action='append',default=[])
args=parser.parse_args()
out=ROOT/'Docs/Validation/Ironvein';out.mkdir(parents=True,exist_ok=True)
proofs=[]
for source in sorted((ROOT/'.utmp/Ironvein/Captures').glob('phase09-*.png')):
    proofs.append((source,out/source.name))
proofs.append((ROOT/'.utmp/Ironvein/ArtReview/native-comparison.png',out/'native-comparison.png'))
proofs.append((ROOT/'.utmp/Ironvein/native-delivery-check.json',out/'native-delivery-check.json'))
for source in sorted((ROOT/'.utmp/Ironvein/Pacing').glob('*')):
    if source.suffix in {'.csv','.txt'}:proofs.append((source,out/source.name))
records=[]
for source,target in proofs:
    shutil.copy2(source,target)
    records.append({'file':str(target.relative_to(ROOT)).replace('\\','/'),'source':str(source.relative_to(ROOT)).replace('\\','/'),
        'sha256':hashlib.sha256(target.read_bytes()).hexdigest()})
(out/'evidence-files.json').write_text(json.dumps(records,indent=2)+'\n',encoding='utf-8')
for supplied in args.result:
    path=ROOT/supplied;root=ET.parse(path).getroot()
    tests=[]
    for node in root.findall('.//test-case'):
        item={key:node.get(key) for key in ('fullname','result','duration','start-time','end-time','asserts')}
        failure=node.find('failure/message')
        if failure is not None:item['failure']=failure.text
        tests.append(item)
    result={'source':supplied,'sha256':hashlib.sha256(path.read_bytes()).hexdigest(),
        'summary':{k:root.get(k) for k in ('result','total','passed','failed','skipped','duration')},'tests':tests}
    (out/(path.stem+'.json')).write_text(json.dumps(result,indent=2)+'\n',encoding='utf-8')
print(json.dumps({'copied_proofs':len(records),'result_files':len(args.result),'folder':str(out)}))
