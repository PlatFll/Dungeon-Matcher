"""Preserve PixelLab originals and inspect the native water-surface candidate."""
from pathlib import Path
import concurrent.futures, hashlib, json, urllib.request
from PIL import Image

ROOT = Path(__file__).resolve().parents[2]
HERE = Path(__file__).resolve().parent
RAW = HERE / 'raw'
RAW.mkdir(parents=True, exist_ok=True)
JOB = '31e31e02-dea2-4c0f-bbb6-d6052a65031f'
def download(i):
    p = RAW / f'frame-{i:02}.png'
    if not p.exists():
        urllib.request.urlretrieve(f'https://api.pixellab.ai/mcp/images/{JOB}/download?index={i}', p)
    return p
with concurrent.futures.ThreadPoolExecutor(max_workers=4) as pool:
    paths = list(pool.map(download, range(17)))
board = Image.new('RGB', (512, 320), '#252936')
for i, p in enumerate(paths):
    im=Image.open(p).convert('RGBA'); x=(i%4)*128; y=(i//4)*64
    board.paste(im,(x,y),im)
board.save(HERE/'raw-contact.png')
print(json.dumps([{'frame':p.name, 'size':Image.open(p).size, 'sha256':hashlib.sha256(p.read_bytes()).hexdigest()} for p in paths]))
