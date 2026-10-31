from pathlib import Path
from PIL import Image,ImageChops
import hashlib,json,xml.etree.ElementTree as ET
R=Path(__file__).resolve().parents[6];S=Path(__file__).resolve().parents[1]
sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
report=json.loads((S/'QA/Validation.json').read_text())
for path,digest in json.loads((S/'QA/ProtectedSourceHashes.json').read_text()).items():assert sha(R/path)==digest,path
for path,digest in report['source_sha256'].items():assert sha(R/path)==digest,path
for m in json.loads((S/'Preview/Measurements.json').read_text())['items']:
 key=m['size'];a=Image.open(S/f'Preview/Before/{key}.png').convert('RGB');b=Image.open(S/f'Preview/After/{key}.png').convert('RGB');w,h=a.size
 d=ImageChops.difference(a,b);f=m['boardFrame'];p=m['nativePixelScale'];y=h-round(f['y']+f['height'])
 for box in [(0,0,w,y),(round(f['x']+10*p),y,round(f['x']+f['width']-10*p),h)]:assert d.crop(box).getbbox() is None,key
xml=ET.parse(S/'QA/UnityTestResults.xml').getroot();assert xml.attrib['result']=='Passed' and xml.attrib['failed']=='0'
print('PASS: unchanged source art and tested fixtures; protected pixels exact at all five viewports; Unity test XML passed.')
