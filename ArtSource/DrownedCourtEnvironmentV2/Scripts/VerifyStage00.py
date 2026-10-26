"""Read-only verification of the preserved files and Stage 00 receipts."""
from pathlib import Path
import hashlib, json, xml.etree.ElementTree as ET
root=Path(__file__).resolve().parents[1]
project=root.parents[1]
before=json.loads((root/'QA/ProtectedBefore.json').read_text())
changed=[p for p,v in before.items() if not (project/p).exists() or hashlib.sha256((project/p).read_bytes()).hexdigest()!=v['sha256']]
test=ET.parse(root/'QA/PreflightTests.xml').getroot()
geometry=json.loads((root/'QA/Geometry.json').read_text())['cases']
assert not changed,changed
assert test.get('result')=='Passed'
assert len(geometry)==6 and all(not c['errors'] for c in geometry)
for h in {c['height'] for c in geometry}:
 cases=[c for c in geometry if c['height']==h]
 for c in cases[1:]:
  for area in ['board','portrait']:
   for dimension in ['width','height']:
    assert abs(c[area][dimension]-cases[0][area][dimension])<.1
print(f'{len(before)} protected files identical; Unity preflight passed; six baseline captures with shared dimensions.')
