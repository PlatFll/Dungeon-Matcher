"""Archive task inputs and inspect native sources; no production image edits."""
from pathlib import Path
import hashlib, json, shutil, zipfile
from PIL import Image

ROOT = Path(__file__).resolve().parent
for d in ['references', 'candidates', 'native', 'validation']:
    (ROOT / d).mkdir(exist_ok=True)
with zipfile.ZipFile('C:/Users/USER/Downloads/Dungeon_Matcher_Menu_UI_Implementation_Pack.zip') as z:
    for n in z.namelist():
        if n.endswith(('.md', '.txt')):
            out = ROOT / 'contract' / Path(n).name
            out.parent.mkdir(exist_ok=True)
            out.write_bytes(z.read(n))
inputs = {
    'MainMenuTarget.png': 'C:/Users/USER/AppData/Local/Temp/codex-clipboard-dcf6fead-f91b-4a0b-a72f-0b0163aeaf09.png',
    'SettingsTarget.png': 'C:/Users/USER/AppData/Local/Temp/codex-clipboard-8f1da86d-c302-4d6c-b591-19666dd7836c.png',
    'Rattlebones.png': 'Docs/ArtDirection/Audit2026-10/media/skeleton-still.png',
}
records = []
for name, source in inputs.items():
    p = Path(source)
    shutil.copyfile(p, ROOT / 'references' / name)
    im = Image.open(p)
    records.append(dict(role=name, path=str(p), sha256=hashlib.sha256(p.read_bytes()).hexdigest(), size=im.size))
for p in [Path('Docs/ArtDirection/Dungeon_Matcher_Art_Direction.txt'), Path('Docs/ArtDirection/Golden_Reference_Register.md')]:
    records.append(dict(role='canonical direction', path=str(p), sha256=hashlib.sha256(p.read_bytes()).hexdigest()))
preserve = [Path('Assets/_Game/Resources/BattleEnvironments/DrownedCourt_Throne.prefab')]
preserve += [Path('Assets/_Game/Resources/UI/Consumables') / (s+'.png.meta') for s in ['SlotDisabled','SlotHighlighted','SlotPressed']]
(ROOT / 'validation' / 'starting-user-files.json').write_text(json.dumps({str(p):hashlib.sha256(p.read_bytes()).hexdigest() for p in preserve}, indent=2))
(ROOT / 'references' / 'lineage.json').write_text(json.dumps({'guide':'1.15','authority':'User confirmed current canonical references on 2026-10-12; supplied (2) files absent. Mockups are composition targets, never production textures.','inputs':records}, indent=2))
print(json.dumps(records, indent=2))
