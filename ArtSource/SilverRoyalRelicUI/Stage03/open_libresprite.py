"""Open the current UI sources for the user in the installed LibreSprite GUI."""
from pathlib import Path
import subprocess,json
here=Path(__file__).resolve().parent
files=sorted((here.parent/'Stage02/Editable').glob('*.ase'))+sorted((here/'Editable').glob('*.ase'))
files += [here/'Inputs/Potion.png',here/'Inputs/Bomb.png',here/'Preview/Family-native.png']
assert len(files)==15 and all(p.is_file() for p in files)
p=subprocess.Popen(['C:/Users/USER/Downloads/libresprite-development-windows-x86_64/libresprite.exe',*[str(p) for p in files]],stdin=subprocess.DEVNULL,stdout=subprocess.DEVNULL,stderr=subprocess.DEVNULL)
print(json.dumps({'pid':p.pid,'opened_file_count':len(files),'last_document':str(files[-1])}))
