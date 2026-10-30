"""Reproduce the separate sky candidate in a temporary folder, never over approved art."""
from pathlib import Path
from PIL import Image
import argparse, json, shutil, subprocess, tempfile

parser=argparse.ArgumentParser()
parser.add_argument('--libresprite', required=True, type=Path)
args=parser.parse_args()
stage=Path(__file__).resolve().parents[1]
work=Path(tempfile.mkdtemp(prefix='SnowStage09Sky-'))
for name in ['Native','Exports']:
    (work/'Stage09'/name).mkdir(parents=True)
shutil.copy2(stage.parent/'Stage02/Exports/AlpinePass.png',work/'AlpinePass.png')
Image.new('RGBA',(384,96)).save(work/'SkyBlank.png')
template=(stage/'Scripts/SkyExtension.js').read_text()
path=json.dumps(work.as_posix()+'/')
(work/'Build.js').write_text(template.replace('__BASE__',path).replace('__WORK__',path))
result=subprocess.run([str(args.libresprite),'--script',str(work/'Build.js')],capture_output=True,timeout=60)
assert result.returncode==0 and b'SNOW9_SKY_PASS' in result.stdout,(result.returncode,result.stdout,result.stderr)
rebuilt=Image.open(work/'Stage09/Exports/SkyExtension.png').convert('RGBA')
reference=Image.open(stage/'Exports/SkyExtension.png').convert('RGBA')
assert rebuilt.size==reference.size and rebuilt.tobytes()==reference.tobytes()
print('Exact native reproduction verified. Temporary editable source and PNG: '+str(work))
