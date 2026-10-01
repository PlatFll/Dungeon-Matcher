"""Download authorized PixelLab animation frames without modifying image bytes."""
import concurrent.futures
import hashlib
import json
from pathlib import Path
import re
import sys
import urllib.request

root = Path(__file__).resolve().parent
jobs = json.loads((root / 'jobs.json').read_text(encoding='utf-8'))
for job in jobs:
    if job['name'] not in sys.argv[1:]:
        continue
    job_id = re.search(r'job_id: (\S+)', job['result']['content'][0]['text'])[1]
    destination = root / job['name']
    destination.mkdir(exist_ok=True)
    def fetch(index):
        url = f'https://api.pixellab.ai/mcp/images/{job_id}/download?index={index}'
        path = destination / f'{index:02}.png'
        if not path.exists():
            with urllib.request.urlopen(url, timeout=90) as response:
                data = response.read()
            if not data.startswith(b'\x89PNG\r\n\x1a\n'):
                raise ValueError(f'Not a PNG: {url}')
            path.write_bytes(data)
        return {'index': index, 'url': url, 'sha256': hashlib.sha256(path.read_bytes()).hexdigest()}
    with concurrent.futures.ThreadPoolExecutor(max_workers=4) as pool:
        records = list(pool.map(fetch, range(job['frames'] + 1)))
    (destination / 'downloads.json').write_text(json.dumps(records, indent=2)+'\n', encoding='utf-8')
    print(f"{job['name']}: {len(records)} frames verified", flush=True)
