"""Original temporary travelling-hall cue. Synthesized instruments, no samples."""
from pathlib import Path
import json,math,wave
import numpy as np
root=Path(__file__).resolve().parents[1]/'ArtSource/Frostwake/audio';root.mkdir(exist_ok=True)
sr=44100;beat=60/84;bars=32;count=round(bars*4*beat*sr);mix=np.zeros((count,2),np.float32);score=[]
def note(kind,pitch,at,length,gain,pan):
    t=np.arange(round(length*beat*sr),dtype=np.float32)/sr;freq=440*2**((pitch-69)/12);p=2*np.pi*freq*t
    if kind=='plucked wire':s=(np.sin(p)+.15*np.sin(p*2)+.07*np.sin(p*3))*np.exp(-t*2.8)
    elif kind=='wood flute':s=(np.sin(p)+.12*np.sin(p*3))*np.exp(-t*.5)
    elif kind=='ice bell':s=(np.sin(p)+.16*np.sin(p*2.75))*np.exp(-t*4)
    else:s=np.sin(2*np.pi*(72*t-20*t*t))*np.exp(-t*17)
    env=np.minimum(1,t/.015)*np.minimum(1,(t[-1]-t)/.09);s*=env*gain
    idx=(np.arange(len(t))+round(at*beat*sr))%count
    mix[idx,0]+=s*math.sqrt((1-pan)/2);mix[idx,1]+=s*math.sqrt((1+pan)/2)
    for delay,level in [(.19,.12),(.38,.065)]:
        target=(idx+round(delay*sr))%count;mix[target,0]+=s*level*.6;mix[target,1]+=s*level*.7
    score.append(dict(instrument=kind,pitch=pitch,beat=at,length=length,gain=gain))
chords=[(38,45,50),(41,48,53),(36,43,48),(38,45,50),(43,50,55),(41,48,53),(36,43,48),(38,45,50)]
melody=[62,65,67,69,67,65,62,60]
for bar in range(bars):
    start=bar*4;c=chords[bar%8]
    note('plucked wire',c[0],start,3.7,.052,-.15)
    for i in range(4):note('plucked wire',c[(i+1)%3]+12,start+i+.25,.85,.022,.25 if i%2 else -.25)
    for i in (0,2):note('hide drum',36,start+i,.45,.022,0)
    if bar%4<3:
        note('wood flute',melody[bar%8],start+.5,1.8,.024,-.2)
        note('wood flute',melody[(bar+2)%8],start+2.6,1.1,.019,-.2)
    if bar%8 in (3,7):note('ice bell',c[2]+24,start+3,1.8,.016,.35)
mix-=mix.mean(axis=0);mix*=.48/max(float(np.abs(mix).max()),1e-8)
# Circularly rendered delays keep the loop continuous. Taper the tiny boundary residual.
edge=round(.025*sr);mix[:edge]*=np.linspace(0,1,edge)[:,None];mix[-edge:]*=np.linspace(1,0,edge)[:,None]
pcm=np.round(mix*32767).astype('<i2');path=root/'Frostwake Wandering Halls - TEMP review.wav'
with wave.open(str(path),'wb') as f:f.setnchannels(2);f.setsampwidth(2);f.setframerate(sr);f.writeframes(pcm.tobytes())
(root/'score.json').write_text(json.dumps(dict(status='original temporary cue, listening review pending',bpm=84,seconds=len(mix)/sr,peak=float(np.abs(mix).max()),clippedSamples=int((np.abs(pcm)>=32767).sum()),boundaryStep=float(np.abs(mix[-1]-mix[0]).max()),notes=score),indent=2))
print(f'{path.name}: {len(mix)/sr:.2f}s stereo; no clipped samples.')
