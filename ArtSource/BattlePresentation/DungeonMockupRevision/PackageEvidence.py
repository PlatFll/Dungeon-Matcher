import argparse,hashlib,json,re,shutil,subprocess
import xml.etree.ElementTree as ET
from pathlib import Path

K=Path(__file__).resolve().parent;P=K.parents[2]
parser=argparse.ArgumentParser();parser.add_argument('--tests',type=Path,required=True);parser.add_argument('--validator-log',type=Path,required=True);args=parser.parse_args()
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
report=ET.parse(args.tests).getroot()
assert report.attrib['result']=='Passed' and report.attrib['failed']==report.attrib['skipped']=='0'
log=args.validator_log.read_text(encoding='utf-8',errors='replace')
assert 'Application will terminate with return code 0' in log
assert not re.search(r'error CS\d{4}|Scripts have compiler errors|Compilation failed|Aborting batchmode due to failure',log)
assert args.tests.stat().st_mtime>(K/'build-receipt.json').stat().st_mtime
evidence=K/'evidence';shutil.copyfile(args.tests,evidence/'tests.xml')
shutil.copyfile(args.tests.with_suffix('.log'),K/'logs/presentation-tests.log')
shutil.copyfile(args.validator_log,K/'logs/unity-validation.log')
production={}
for path in (P/'Assets/_Game/Resources/UI/BattlePresentation/ReferenceDungeon').glob('*.png'):
    if path.name.startswith(('Shared','EmptyCell')):continue
    revised=K/'native'/path.name
    if revised.exists():
        assert revised.read_bytes()==path.read_bytes(),path
        production[path.name]=sha(path)
assert 'ForegroundProps.png' in production
for path in (P/'.utmp/ThreeZonePresentation/after').glob('*-layout.txt'):
    if path.stat().st_mtime>(K/'build-receipt.json').stat().st_mtime:
        (evidence/path.name).write_text(path.read_text(encoding='utf-8').rstrip()+'\n',encoding='utf-8')
sources=['Assets/_Game/Scripts/UI/DungeonPresentationArt.cs','Assets/_Game/Scripts/Editor/BattlePresentationImporter.cs','Assets/_Game/Scripts/Editor/ThreeZonePresentationTests.cs']
record={'status':'Passed; user artistic review pending','tests':report.attrib,'unity_version':'6000.3.19f1','validator':{'exit_code':0,'sha256':sha(args.validator_log),'original_log':str(args.validator_log)},'source_hashes':{p:sha(P/p) for p in sources},'revised_runtime_exports':production,'generation_calls':0,'audio_muted':True,'physical_device_tested':False,'captures':{p.name:sha(p) for p in (K/'review').glob('Unity-*.png')}}
(evidence/'validation.json').write_text(json.dumps(record,indent=2)+'\n')
provenance=json.loads((K/'provenance.json').read_text());provenance['status']='Integrated on review branch; native and Unity checks passed; awaiting artistic review; not merged'
(K/'provenance.json').write_text(json.dumps(provenance,indent=2)+'\n')
print(f"Packaged {report.attrib['passed']} passing Unity tests, {len(production)} revised runtime PNGs, and {len(record['captures'])} portrait captures.")
