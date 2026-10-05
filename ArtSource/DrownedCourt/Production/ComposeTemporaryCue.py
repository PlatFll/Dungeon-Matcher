"""Original temporary synthesis: no recordings, samples, or outside compositions."""
from pathlib import Path
import numpy as np, wave, json
r=Path(__file__).resolve().parent;repo=r.parents[2]
rate=32000;tempo=100;beat=60/tempo;bars=32;duration=bars*4*beat
mix=np.zeros((int(duration*rate),2),dtype=np.float64)
rng=np.random.default_rng(10052026)
def note(midi,start,length,amp,voice,pan=0):
 n=int(length*rate);t=np.arange(n)/rate;freq=440*2**((midi-69)/12)
 if voice=='reed':s=np.sin(2*np.pi*freq*t)+.24*np.sin(6*np.pi*freq*t);env=np.minimum(t/.035,1)*np.minimum((length-t)/.11,1)*np.exp(-t/2)
 elif voice=='mallet':s=np.sin(2*np.pi*freq*t)+.22*np.sin(2*np.pi*freq*2.76*t);env=np.minimum(t/.006,1)*np.exp(-t*6)
 elif voice=='pluck':s=np.sin(2*np.pi*freq*t)+.2*np.sin(4*np.pi*freq*t);env=np.minimum(t/.008,1)*np.exp(-t*4)
 else:s=np.sin(2*np.pi*(freq*t+freq*.08*(1-np.exp(-t*20))));env=np.minimum(t/.003,1)*np.exp(-t*12)
 signal=s*env*amp;idx=int(start*rate)+np.arange(n)
 # Circular accumulation makes tails cross the loop seam without a restart click.
 np.add.at(mix[:,0],idx%len(mix),signal*(1-pan)*.5);np.add.at(mix[:,1],idx%len(mix),signal*(1+pan)*.5)
progression=[(38,[50,53,57]),(34,[46,50,53]),(41,[53,57,60]),(36,[48,52,55])]
melodies=[[69,65,62,64],[65,62,60,62],[69,72,69,65],[67,64,62,61]]
for bar in range(bars):
 start=bar*4*beat;root,chord=progression[(bar//2)%4]
 note(root,start,beat*3.8,.13,'reed',-.12)
 for b in [0,2]:note(34,start+b*beat,.38,.16,'drum')
 for b in range(4):note(chord[b%3]+12,start+b*beat+beat*.5,beat*1.1,.095,'pluck',.4 if b%2 else -.4)
 motif=melodies[(bar//2)%4]
 if bar%2==0:
  for j,m in enumerate(motif):note(m,start+j*beat,.45 if j<3 else .7,.09,'mallet',-.15)
 elif bar%4==3:note(motif[-1]+12,start+2.5*beat,.8,.05,'mallet',.3)
peak=float(np.max(np.abs(mix)));mix*=.72/max(.72,peak)
out=repo/'Assets/_Game/Audio/Music/Drowned Court - TEMP review.wav';out.parent.mkdir(exist_ok=True,parents=True)
with wave.open(str(out),'wb') as w:w.setnchannels(2);w.setsampwidth(2);w.setframerate(rate);w.writeframes((mix*32767).astype('<i2').tobytes())
(r/'MusicProvenance.json').write_text(json.dumps(dict(title='Drowned Court - TEMP review',status='original temporary cue; listening approval pending',tempo=tempo,bars=bars,duration_seconds=duration,sample_rate=rate,channels=2,composition='Original D-minor modal motifs, soft synthesized reed, shell-like mallet/plucks and deep sine percussion',rights='Created locally for this project from mathematical oscillators; no external samples or melodies used',peak=float(np.max(np.abs(mix))),loop='32-bar circular tail accumulation'),indent=2))
print(str(out),duration,'seconds')
