"""Encode unmodified Unity captures as a silent review video and GIF."""
from pathlib import Path
import sys,subprocess,json
from PIL import Image
r=Path(__file__).resolve().parent;repo=r.parents[2]
sys.path.insert(0,str(repo/'.utmp/media-runtime'))
import imageio_ffmpeg
folder=repo/'.utmp/DrownedCourtValidation';frames=sorted((folder/'FloodFrames').glob('*.png'))
assert len(frames)==24,'Capture the complete Unity fixture before packaging'
result=subprocess.run([imageio_ffmpeg.get_ffmpeg_exe(),'-y','-loglevel','error','-framerate','12','-i',str(folder/'FloodFrames/%03d.png'),'-c:v','libx264','-crf','18','-pix_fmt','yuv420p','-movflags','+faststart',str(folder/'FloodPresentation.mp4')],check=True)
images=[Image.open(p).convert('RGB') for p in frames]
images[0].save(folder/'FloodPresentation.gif',save_all=True,append_images=images[1:],duration=[80,80,90]*8,loop=0)
(folder/'VideoProvenance.json').write_text(json.dumps(dict(source='Unity Game scene, CourtPortraitMaterialsAndFloodPresentationCapture',fixture='Timed rise/drain presentation fixture, not a human playtest',frames=24,fps=12,dimensions=images[0].size,audio='muted',resize=False),indent=2))
print('Encoded 24 original-resolution Unity frames as a two-second silent MP4 and GIF.')
