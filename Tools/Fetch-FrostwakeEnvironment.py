"""Download completed PixelLab native environment sources without modifying pixels."""
from pathlib import Path
from urllib.request import urlopen
import json
root=Path(__file__).resolve().parents[1]/'ArtSource/Frostwake'
jobs=[]
for manifest in ['environment-queue.json','environment-layout-revisions.json','environment-full-revisions.json']:
    if (root/manifest).exists():jobs.extend(json.loads((root/manifest).read_text()))
for job in jobs:
    result=job.get('completed')
    if not result: continue
    path=root/'environment'/(job['id']+'.png');path.parent.mkdir(exist_ok=True)
    if not path.exists():
        url=result.get('download_url') or result['download_urls'][0]
        with urlopen(url,timeout=120) as response: path.write_bytes(response.read())
    print(path.name)
