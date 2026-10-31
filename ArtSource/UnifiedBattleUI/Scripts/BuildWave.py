from pathlib import Path
from PIL import Image
import json,subprocess,uuid,hashlib
r=Path(r'C:/Users/USER/.codex/worktrees/drowned-court-pixellab-v2/Dungeon Matcher')
o=r/'ArtSource/UnifiedBattleUI';dst=r/'Assets/_Game/Resources/UI/BattlePresentation/Unified'
dst.mkdir(exist_ok=True);(o/'Roundtrip').mkdir(exist_ok=True)
# PixelLab candidate A. Native center columns are extended, never resampled.
src=Image.open(o/'Raw/WaveCandidate0.png').convert('RGBA')
body=Image.new('RGBA',(144,34));body.paste(src.crop((32,4,43,38)),(0,0));body.paste(src.crop((100,4,111,38)),(133,0))
for x in range(11,133):body.paste(src.crop((52,4,53,38)),(x,0))
pendant=src.crop((64,28,80,47))
body.save(o/'Native/WaveBodyPrepared.png');pendant.save(o/'Native/WavePendantPrepared.png')
js='var root='+json.dumps(o.as_posix()+'/')+';\n'
for name in ['WaveBody','WavePendant']:
 js+="app.open(root+'Native/"+name+"Prepared.png');var spr=app.activeSprite;spr.layer(0).name='PixelLab native stone: clean repeated rails and intact ornament';spr.commit();spr.saveAs(root+'Native/"+name+".ase',true);spr.saveAs(root+'Exports/"+name+".png',true);app.activeDocument.close();app.open(root+'Native/"+name+".ase');app.activeSprite.saveAs(root+'Roundtrip/"+name+".png',true);app.activeDocument.close();\n"
js+="console.log('WAVE_NATIVE_ROUNDTRIP_COMPLETE');app.command.Exit();"
(o/'Scripts/FinishWave.js').write_text(js)
si=subprocess.STARTUPINFO();si.dwFlags|=subprocess.STARTF_USESHOWWINDOW;si.wShowWindow=0
run=subprocess.run([r'C:/Users/USER/Downloads/libresprite-development-windows-x86_64/libresprite.exe','--script',str(o/'Scripts/FinishWave.js')],startupinfo=si,stdout=subprocess.PIPE,stderr=subprocess.STDOUT,timeout=60)
(o/'QA/LibreSprite.log').write_bytes(run.stdout);assert run.returncode==0 and b'WAVE_NATIVE_ROUNDTRIP_COMPLETE' in run.stdout
qa=[]
for name in ['WaveBody','WavePendant']:
 a=Image.open(o/f'Exports/{name}.png').convert('RGBA');b=Image.open(o/f'Roundtrip/{name}.png').convert('RGBA');assert a.tobytes()==b.tobytes()
 assert set(a.getchannel('A').getdata())<={0,255}
 (dst/f'{name}.png').write_bytes((o/f'Exports/{name}.png').read_bytes())
 meta=dst/f'{name}.png.meta'
 if not meta.exists():
  meta.write_text('fileFormatVersion: 2\nguid: '+uuid.uuid4().hex+'\nTextureImporter:\n  serializedVersion: 13\n  mipmaps:\n    enableMipMap: 0\n  isReadable: 1\n  textureSettings:\n    filterMode: 0\n    aniso: 1\n    wrapU: 1\n    wrapV: 1\n  nPOTScale: 0\n  spriteMode: 1\n  spriteMeshType: 0\n  spritePixelsToUnits: 64\n  spritePivot: {x: 0.5, y: 0.5}\n  spriteBorder: '+('{x: 11, y: 10, z: 11, w: 10}' if name=='WaveBody' else '{x: 0, y: 0, z: 0, w: 0}')+'\n  alphaIsTransparency: 1\n  textureType: 8\n  textureShape: 1\n  maxTextureSize: 2048\n  platformSettings:\n  - serializedVersion: 4\n    buildTarget: DefaultTexturePlatform\n    maxTextureSize: 2048\n    textureFormat: -1\n    textureCompression: 0\n    crunchedCompression: 0\n')
 qa.append(dict(name=name,size=a.size,colors=len({p[:3] for p in a.getdata() if p[3]}),binary_alpha=True,libresprite_roundtrip_exact=True,sha256=hashlib.sha256((dst/f'{name}.png').read_bytes()).hexdigest()))
(o/'QA/WaveNative.json').write_text(json.dumps(qa,indent=2)+'\n')
board=Image.new('RGBA',(180,75),(25,28,38,255));board.alpha_composite(body,(18,10));board.alpha_composite(pendant,(82,34));board.resize((720,300),Image.Resampling.NEAREST).save(o/'Preview/WaveDetail.png')
(o/'Scripts/BuildWave.py').write_bytes(Path(__file__).read_bytes())
print(json.dumps(qa))
