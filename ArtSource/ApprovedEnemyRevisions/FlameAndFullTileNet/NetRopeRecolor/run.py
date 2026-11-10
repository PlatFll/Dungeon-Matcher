"""Execute the native LibreSprite recolor, verify exact pixels, then bind the copy."""
from pathlib import Path
from PIL import Image
import hashlib,json,shutil,subprocess,sys
HERE=Path(__file__).resolve().parent
ROOT=HERE.parents[3]
subprocess.run([sys.executable,str(HERE/'prepare.py')],cwd=ROOT,check=True)
startup=subprocess.STARTUPINFO();startup.dwFlags|=subprocess.STARTF_USESHOWWINDOW;startup.wShowWindow=0
run=subprocess.run(['C:/Users/USER/Downloads/libresprite-development-windows-x86_64/libresprite.exe',
                    '--script',str(HERE/'recolor.js')],cwd=ROOT,startupinfo=startup,
                   stdout=subprocess.PIPE,stderr=subprocess.STDOUT,timeout=60)
(HERE/'LibreSprite.log').write_bytes(run.stdout)
assert run.returncode==0 and b'NET_ROPE_RECOLOR_COMPLETE' in run.stdout,run.stdout.decode(errors='replace')
before=Image.open(HERE/'Before.png').convert('RGBA')
after=Image.open(HERE/'MarineSnare.png').convert('RGBA')
roundtrip=Image.open(HERE/'Roundtrip.png').convert('RGBA')
mapping={(10,13,17):(10,13,17),(36,63,72):(74,44,28),(66,102,107):(122,77,46),
         (113,145,142):(122,77,46),(169,187,182):(176,122,67),(176,160,184):(184,200,190)}
assert before.size==after.size==roundtrip.size==(64,64)
assert before.getchannel('A').tobytes()==after.getchannel('A').tobytes()
assert after.tobytes()==roundtrip.tobytes()
for old,new in zip(before.get_flattened_data(),after.get_flattened_data()):
    assert old==new if old[3]==0 else new==(*mapping[old[:3]],255)
reference=json.loads((HERE/'reference.json').read_text())
assert hashlib.sha256((ROOT/reference['source']).read_bytes()).hexdigest()==reference['sha256']
runtime=ROOT/'Assets/_Game/Resources/Combat/ApprovedRevisions/MarineSnare.png'
for target in [runtime,HERE.parent/'MarineSnare.png']:shutil.copy2(HERE/'MarineSnare.png',target)
shutil.copy2(HERE/'MarineSnare.ase',HERE.parent/'MarineSnare.ase')
manifest_path=HERE.parent/'manifest.json';manifest=json.loads(manifest_path.read_text())
manifest['net']['opaque_colors']=len({p for p in after.get_flattened_data() if p[3]})
manifest['net']['recolor']={'reference':'NetRopeRecolor/reference.json','palette_map':{
    '#%02X%02X%02X'%a:'#%02X%02X%02X'%b for a,b in mapping.items()},'mask_exact':True,
    'all_pixel_changes_match_map':True,'libresprite_reopen_exact':True,'generations':0}
manifest['files']={p.relative_to(HERE.parent).as_posix():hashlib.sha256(p.read_bytes()).hexdigest()
                   for p in sorted(HERE.parent.rglob('*.png')) if 'Preview' not in p.parts}
manifest_path.write_text(json.dumps(manifest,indent=2)+'\n')
(HERE/'checks.json').write_text(json.dumps({'canvas':[64,64],'opaque_colors':5,'binary_alpha':True,
    'mask_exact':True,'geometry_exact':True,'mapped_pixels_exact':True,'libresprite_reopen_exact':True,
    'enemy_reference_unchanged':True,'before_sha256':hashlib.sha256((HERE/'Before.png').read_bytes()).hexdigest(),
    'after_sha256':hashlib.sha256((HERE/'MarineSnare.png').read_bytes()).hexdigest()},indent=2)+'\n')
print('LibreSprite recolor verified: 64x64, 5 sampled colors, exact alpha/geometry and native reopen. No credits spent.')
