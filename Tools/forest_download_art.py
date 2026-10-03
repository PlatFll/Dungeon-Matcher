"""Download only recorded PixelLab results; keep original frames immutable."""
import argparse, json, urllib.request
from concurrent.futures import ThreadPoolExecutor
from pathlib import Path
from PIL import Image, ImageDraw

ROOT = Path(__file__).resolve().parents[1]
ART = ROOT / 'ArtSource/Forest/Production'

def download(row):
    folder = ART / 'Raw' / row['name'] / row['state']
    folder.mkdir(parents=True, exist_ok=True)
    for i, url in enumerate(row['download_urls']):
        assert url.startswith('https://api.pixellab.ai/mcp/images/')
        p = folder / f'{i:02}.png'
        if not p.exists():
            with urllib.request.urlopen(url, timeout=60) as response:
                data = response.read()
            p.write_bytes(data)
        Image.open(p).verify()
    frames = [Image.open(folder/f'{i:02}.png').convert('RGBA') for i in range(len(row['download_urls']))]
    w,h = frames[0].size
    sheet = Image.new('RGBA', (w*len(frames),h))
    for i, frame in enumerate(frames): sheet.paste(frame,(i*w,0))
    sheet.save(folder/'sheet.png')
    preview = Image.new('RGB', (w*len(frames)*2,h*2+24),'#777777')
    preview.paste(sheet.resize((sheet.width*2,h*2),Image.Resampling.NEAREST),(0,24),sheet.resize((sheet.width*2,h*2),Image.Resampling.NEAREST))
    ImageDraw.Draw(preview).text((4,4),row['name']+' / '+row['state']+' / RAW native frames at 2x',fill='white')
    preview.save(folder/'inspection.png')
    return f'{row["name"]}/{row["state"]}: {len(frames)} frames {w}x{h}'

if __name__ == '__main__':
    rows=json.loads((ART/'completed-jobs.json').read_text(encoding='utf-8'))
    with ThreadPoolExecutor(max_workers=4) as pool:
        for result in pool.map(download,rows): print(result)
