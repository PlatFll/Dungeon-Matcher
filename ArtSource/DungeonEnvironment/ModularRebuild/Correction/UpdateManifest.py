from pathlib import Path
import json
K=Path(__file__).resolve().parents[1]
path=K/'AssetManifest.json';manifest=json.loads(path.read_text());records={a['name']:a for a in manifest['modules']}
for stage,prefix in [('FloorPolish','FLOOR_RECORDS'),('Wall','WALL_RECORDS'),('Decoration','DECORATION_RECORDS'),('Health','HEALTH_RECORDS')]:
 log=K/f'Correction/logs/{stage}.log'
 if not log.exists():continue # A scoped rebuild retains already-registered other families.
 for line in log.read_text().splitlines():
  if line.startswith(prefix+' '):
   for record in json.loads(line[len(prefix)+1:]):records[record['name']]=record
records['FarRightChain']['changes']='Reviewed native chain link alpha; retained export is pixel-identical to the pre-correction module.'
for name in records:
 if name.startswith('FloorMiddle'):records[name]['changes']=records[name]['changes'].replace('Compressed distant courses.','Intermediate-depth courses. Native top-aligned clipping in the assembly.')
manifest['modules']=sorted(records.values(),key=lambda a:(a['group'],a['name']))
manifest['sourceScale']='Native editable reconstruction on the established 470x836 working grid; supplied mockup is 941x1672, not a native source sprite sheet.'
manifest['status']='Corrected art review candidate; prior blanket 9/10 assessments superseded.'
path.write_text(json.dumps(manifest,indent=2)+'\n')
print('Current module records:',len(manifest['modules']))
