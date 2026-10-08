"""Download recorded PixelLab jobs and report source pixels without resampling."""
import json, urllib.request
from pathlib import Path
from PIL import Image

root = Path(__file__).resolve().parent
ledger = json.loads((root / 'GenerationLedger.json').read_text())
folder = root / 'Sources'
folder.mkdir(exist_ok=True)
report = []
for job in ledger['jobs']:
    path = folder / (job['id'] + '.png')
    if not path.exists():
        urllib.request.urlretrieve(job['download'], path)
    image = Image.open(path).convert('RGBA')
    colors = {p[:3] for p in image.get_flattened_data() if p[3]}
    report.append(dict(id=job['id'], size=image.size, bounds=image.getbbox(), colors=len(colors), alpha=sorted(set(image.getchannel('A').get_flattened_data()))))
(root / 'SourceChecks.json').write_text(json.dumps(report, indent=2))
print(json.dumps(report))

selected = root / 'Selected'
selected.mkdir(exist_ok=True)
final_jobs = {}
for job in json.loads((root / 'Corrections.json').read_text()):
    final_jobs[job.get('selectedAs', job['id'])] = job
for name, job in final_jobs.items():
    path = selected / (name + '.png')
    if not path.exists():
        urllib.request.urlretrieve('https://api.pixellab.ai/mcp/images/' + job['jobId'] + '/download', path)
sword = selected / 'Sword.png'
if not sword.exists():
    urllib.request.urlretrieve('https://api.pixellab.ai/mcp/pixel-tools/31c06a24-8a6a-40d8-bd06-ed0b3cedb4c1/image.png', sword)
