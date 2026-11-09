"""Copy approved native settings modules without changing any pixel or old source."""
from pathlib import Path
import hashlib, json, re, shutil, uuid
from PIL import Image

HERE = Path(__file__).resolve().parent
ROOT = HERE.parents[2]
SOURCE = Path('C:/Users/USER/.codex/visualizations/2026/10/10/01a127fb-8a65-7d73-9061-df9dc2d9843a/MenuSettingsV2/Settings')
TARGET = ROOT/'Assets/_Game/Resources/UI/GlobalMenu/SettingsV2'
PARTS = {
    5: 'BodyTile HeaderTile EdgeTop EdgeBottom EdgeLeft EdgeRight HeaderRule CornerTL CornerTR CornerBL CornerBR CrownedCrest'.split(),
    6: 'Row IconMusic IconSFX IconMotion IconVibration ToggleOn ToggleOff Divider'.split(),
    7: ['Button'+c+s for c in ('Green','Gray','Red') for s in ('','Hover','Pressed','Disabled')]+['Icon'+n for n in ('Resume','Suspend','Retry','EndRun','Back')],
}
def sha(p): return hashlib.sha256(p.read_bytes()).hexdigest()
def main():
    TARGET.mkdir(parents=True, exist_ok=True)
    template=(ROOT/'Assets/_Game/Resources/UI/GlobalMenu/Play.png.meta').read_text()
    # Single sprite import; no copied per-sprite identity from the old Play asset.
    template=re.sub(r'  internalIDToNameTable:.*?  externalObjects:', '  internalIDToNameTable: []\n  externalObjects:', template, flags=re.S)
    rows=[]
    for stage,names in PARTS.items():
        source=SOURCE/f'Stage{stage:02d}'
        native=HERE/f'Stage{stage:02d}'; native.mkdir(exist_ok=True)
        for name in names:
            png=source/'Clean'/f'{name}.png'; ase=source/'Editable'/f'{name}.aseprite'
            assert png.exists() and ase.exists(), (png,ase)
            im=Image.open(png).convert('RGBA'); assert set(im.getchannel('A').tobytes()) <= {0,255}
            shutil.copy2(png,native/png.name); shutil.copy2(ase,native/ase.name)
            output=TARGET/png.name; shutil.copy2(png,output)
            meta=output.with_suffix('.png.meta')
            if not meta.exists():
                text=re.sub(r'guid: [a-f0-9]+', 'guid: '+uuid.uuid4().hex, template,count=1)
                meta.write_text(text)
            assert sha(png)==sha(output)
            rows.append(dict(name=name,stage=stage,size=list(im.size),pngSha256=sha(png),aseSha256=sha(ase),source=str(png),native=str((native/png.name).relative_to(ROOT)),runtime=str(output.relative_to(ROOT))))
        for file in (f'Stage{stage:02d}-Notes.txt','generation-requests.json','button-layout.json','centering-validation.json'):
            if (source/file).exists(): shutil.copy2(source/file,native/file)
    (HERE/'manifest.json').write_text(json.dumps({'approved':'User authorized actual-game integration and merge on 2026-11-09','assets':rows},indent=2)+'\n')
    print(f'Copied {len(rows)} approved PNG/ASE pairs; runtime PNG bytes unchanged.')
if __name__=='__main__': main()
