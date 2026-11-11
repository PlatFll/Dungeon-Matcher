from pathlib import Path
import json, urllib.request
root=Path(__file__).resolve().parent
for row in json.loads((root/'GenerationResults.json').read_text()):
    path=root/'Raw'/(row['id']+'.png')
    if not path.exists(): urllib.request.urlretrieve(row['download_url'],path)
    print(row['id'],path.stat().st_size,row['charged_generations'])
