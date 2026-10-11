"""Download completed, authorized PixelLab outputs. Never modifies sprite pixels."""
from pathlib import Path
import json,urllib.request,hashlib
root=Path(__file__).resolve().parents[1]
art=root/'ArtSource/Frostwake';out=art/'candidates';out.mkdir(exist_ok=True)
for job in json.loads((art/'completed-stills.json').read_text()):
    data=job['result']
    assert data['status']=='completed',job['id']
    url=data['download_url'];assert url.startswith('https://api.pixellab.ai/')
    p=out/(job['id']+'.png')
    if not p.exists():urllib.request.urlretrieve(url,p)
    print(job['id'],hashlib.sha256(p.read_bytes()).hexdigest())
eye=root/'ArtSource/FrostwakePlayables/eye-revision/candidate-sheet.png'
if not eye.exists():urllib.request.urlretrieve('https://api.pixellab.ai/mcp/pixel-tools/c44e74e6-2eef-40ba-afce-161ab736d449/image.png',eye)
clean=art/'palette-clean';clean.mkdir(exist_ok=True)
for job in json.loads((art/'palette-results.json').read_text()):
    p=clean/(job['id']+'.png')
    if not p.exists():urllib.request.urlretrieve(job['result']['download_url'],p)
