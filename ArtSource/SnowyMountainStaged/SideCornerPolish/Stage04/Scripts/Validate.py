"""Recheck the saved Stage 4 evidence without altering source art or screenshots."""
from pathlib import Path
from PIL import Image,ImageChops
import hashlib,json,xml.etree.ElementTree as ET
R=Path(__file__).resolve().parents[5];S=R/'ArtSource/SnowyMountainStaged/SideCornerPolish/Stage04'
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
report=json.loads((S/'QA/Validation.json').read_text())
for path,digest in json.loads((S/'QA/ProtectedSourceHashes.json').read_text()).items():
 assert sha(R/path)==digest,path
for path,digest in report['unity']['source_sha256'].items():assert sha(R/path)==digest,path
for a in report['approved_assets']:
 p=R/a['path'];im=Image.open(p).convert('RGBA')
 assert sha(p)==a['sha256'] and list(im.size)==a['size'],a['path']
 assert set(im.getchannel('A').get_flattened_data())<={0,255}
for m in json.loads((S/'Preview/Measurements.json').read_text())['items']:
 key=m['size'];a=Image.open(S/f'Preview/Before/{key}.png').convert('RGB');b=Image.open(S/f'Preview/After/{key}.png').convert('RGB')
 d=ImageChops.difference(a,b);w,h=a.size;f=m['boardFrame'];p=m['nativePixelScale']
 boxes=[(0,0,w,h-round(f['y']+f['height'])),(round(f['x']+13*p),h-round(f['y']+f['height']-13*p),round(f['x']+f['width']-13*p),h-round(f['y']+13*p))]
 assert all(d.crop(box).getbbox() is None for box in boxes),key
xml=ET.parse(S/'QA/UnityTestResults.xml').getroot()
assert xml.attrib['result']=='Passed' and xml.attrib['failed']=='0'
print('PASS: five protected-region comparisons, approved sources, tested fixture hashes and Unity XML.')
