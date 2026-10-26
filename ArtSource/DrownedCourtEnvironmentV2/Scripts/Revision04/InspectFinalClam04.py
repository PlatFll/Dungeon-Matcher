from pathlib import Path
from PIL import Image,ImageDraw
import json
R=Path(r'C:/Users/USER/.codex/worktrees/drowned-court-pixellab-v2/Dungeon Matcher/ArtSource/DrownedCourtEnvironmentV2');P=R/'Preview/Revision04'
frames=[Image.open(R/f'Exports/Revision04/Clam{c}.png').convert('RGBA') for c in 'ABCDEFGHI']
b=Image.new('RGBA',(1000,430),'#10212c');d=ImageDraw.Draw(b)
for i,im in enumerate(frames):
 x=i%5*200;y=i//5*215;d.text((x+4,y+3),str(i),fill='white');b.alpha_composite(im.resize((192,192),Image.Resampling.NEAREST),(x+4,y+20))
b.save(P/'ClamAnimation-Clean-3x.png')
strip=Image.new('RGBA',(64*len(frames),64))
for i,im in enumerate(frames):strip.paste(im,(64*i,0))
strip.save(R/'Native/Revision04/Clam-FrameStrip.png')
timing=json.loads((R/'Native/Revision04/ClamTiming.json').read_text())['keys'];sequence=[];dur=[]
for i,k in enumerate(timing):
 im=frames[ord(k['sprite'][-1])-65];solid=Image.new('RGB',im.size,'#10212c');solid.paste(im,mask=im.getchannel('A'));sequence.append(solid.resize((256,256),Image.Resampling.NEAREST));dur.append(round(((timing[i+1]['time'] if i+1<len(timing) else 12)-k['time'])*1000))
sequence[0].save(P/'Clam-Preview.gif',save_all=True,append_images=sequence[1:],duration=dur,loop=0,disposal=2)
print('Cleaned frame dimensions, palette counts:',[(im.size,len(set(c for c in im.get_flattened_data() if c[3]))) for im in frames])
