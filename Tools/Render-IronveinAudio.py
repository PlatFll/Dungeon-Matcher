"""Original temporary Ironvein score and mechanism sounds; no sampled recordings."""
import json, math, wave
from pathlib import Path
import numpy as np

ROOT=Path(__file__).resolve().parents[1]
OUT=ROOT/'ArtSource/Ironvein/Audio';OUT.mkdir(parents=True,exist_ok=True)
SR=44100;BPM=96;BEAT=60/BPM;BARS=48;N=round(BARS*4*BEAT*SR)
rng=np.random.default_rng(410009);mix=np.zeros((N,2),np.float32);score=[];reports=[]
def save(name,a):
    a=np.asarray(a,dtype=np.float32);a-=a.mean(axis=0)
    peak=float(np.max(np.abs(a)));a*=.65/max(peak,1e-6)
    pcm=np.round(a*32767).astype('<i2');channels=1 if a.ndim==1 else a.shape[1]
    with wave.open(str(OUT/(name+'.wav')),'wb') as w:
        w.setnchannels(channels);w.setsampwidth(2);w.setframerate(SR);w.writeframes(pcm.tobytes())
    reports.append(dict(name=name,seconds=len(a)/SR,channels=channels,peak=float(np.max(np.abs(a))),rms=float(np.sqrt(np.mean(a*a))),clipped_samples=int(np.sum(np.abs(pcm)>=32767)),boundary_step=float(np.max(np.abs(a[-1]-a[0])))))
def note(kind,pitch,at,duration,gain,pan=0):
    score.append(dict(instrument=kind,midi=pitch,beat=at,duration=duration,gain=gain,pan=pan))
    t=np.arange(round(duration*BEAT*SR),dtype=np.float32)/SR;f=440*2**((pitch-69)/12);phase=2*np.pi*f*t
    if kind=='low wire':
        s=(np.sin(phase)+.19*np.sin(phase*2)+.08*np.sin(phase*3))*np.exp(-t*2.3)
    elif kind=='ore bell':
        s=np.sin(phase)*np.exp(-t*3)+.23*np.sin(phase*2.76)*np.exp(-t*6)
    elif kind=='tapped iron':
        s=(np.sin(phase)+.31*np.sin(phase*1.48)+.16*np.sin(phase*2.09))*np.exp(-t*18)
    elif kind=='wooden sleeper':
        noise=rng.normal(0,1,len(t)).astype(np.float32);s=(np.sin(2*np.pi*110*t)+noise*.18)*np.exp(-t*35)
    else:
        s=(np.sin(phase)+.10*np.sin(phase*3))*np.exp(-t*.5)
    env=np.minimum(1,t/.008)*np.minimum(1,(t[-1]-t)/.06)
    s=np.asarray(s*env*gain,np.float32);ix=(np.arange(len(t))+round(at*BEAT*SR))%N
    l=math.sqrt((1-pan)/2);r=math.sqrt((1+pan)/2);mix[ix,0]+=s*l;mix[ix,1]+=s*r
    for delay,amount in [(.13,.11),(.27,.07),(.41,.035)]:
        dest=(ix+round(delay*SR))%N;mix[dest,0]+=s*r*amount;mix[dest,1]+=s*l*amount
chords=[(45,48,52),(41,45,48),(43,47,50),(40,43,47),(45,48,52),(48,52,55),(43,47,50),(40,44,47)]
melodies=[(69,72,76,74,72),(65,69,72,76,72),(67,71,74,72,71),(64,67,71,69,67)]
for bar in range(BARS):
    root=bar*4;c=chords[bar%8];section=bar//16
    note('low wire',c[0]-12,root,3.4,.075,-.05)
    for k,degree in enumerate((0,2,1,2)):
        note('tapped iron',c[degree]+12,root+k+.25,.55,.027 if k%2 else .038,(-.3,.3)[k%2])
    for beat in (0,1.5,2,3.5):note('wooden sleeper',36,root+beat,.2,.026,-.15 if beat<2 else .15)
    if section!=1 and bar%4<3:
        for k,p in enumerate(melodies[(bar//4)%4]):note('ore bell',p,root+k*.65,.95,.022,.24)
    elif section==1 and bar%2==0:
        note('quiet pipe',c[1]+12,root+.5,2.7,.027,-.2)
    if bar%8==7:note('ore bell',c[2]+24,root+3,1.4,.016,-.4)
save('Ironvein Rail Lanterns - TEMP review',mix)

def mechanism(name,length,kind):
    t=np.arange(round(length*SR),dtype=np.float32)/SR;noise=rng.normal(0,1,len(t)).astype(np.float32)
    # All one-shots have click-free ramps; timbre is synthesized, never sampled.
    if kind=='drill':s=(np.sin(2*np.pi*(90*t+170*t*t))+.32*np.sin(2*np.pi*720*t)+noise*.16)*(1+.15*np.sin(2*np.pi*38*t))
    elif kind=='large':s=(np.sin(2*np.pi*72*t)+.35*np.sin(2*np.pi*145*t)+noise*.35)*(1+.25*np.sin(2*np.pi*24*t))
    elif kind=='ore':s=np.sin(2*np.pi*830*t)*np.exp(-t*9)+.4*np.sin(2*np.pi*1320*t)*np.exp(-t*15)
    elif kind=='fuse':s=np.concatenate(([0],np.diff(noise)))*.35*(.65+.35*np.sin(2*np.pi*21*t))
    elif kind=='blast':s=noise*np.exp(-t*7)+.75*np.sin(2*np.pi*(74*t-22*t*t))*np.exp(-t*9)
    elif kind=='crack':s=noise*np.exp(-t*28)+.23*np.sin(2*np.pi*240*t)*np.exp(-t*17)
    elif kind=='rail':s=(np.sin(2*np.pi*340*t)+.6*np.sin(2*np.pi*570*t))*np.exp(-t*35)+noise*.22*np.exp(-t*45)
    elif kind=='piston':s=noise*.3*np.exp(-t*5)+np.sin(2*np.pi*(145*t-50*t*t))*np.exp(-t*12)
    else:s=np.sin(2*np.pi*116*t)*np.exp(-t*17)+noise*.2*np.exp(-t*27)
    s*=np.minimum(1,t/.003)*np.minimum(1,(t[-1]-t)/.025)
    save(name,s)
for row in [('MineRail',.17,'rail'),('MineSmallDrill',.33,'drill'),('MineLargeDrill',.56,'large'),('MineStoneHarden',.22,'crack'),('MineStoneBreak',.29,'crack'),('MineFuse',.32,'fuse'),('MineBlast',.52,'blast'),('MineOre',.3,'ore'),('MineRivet',.14,'piston'),('MineHammer',.27,'hammer'),('MinePiston',.32,'piston'),('MineMechBreak',.58,'blast')]:mechanism(*row)
(OUT/'score.json').write_text(json.dumps(dict(title='Ironvein Rail Lanterns',bpm=BPM,bars=BARS,notes=score),indent=2)+'\n')
(OUT/'render-report.json').write_text(json.dumps(dict(status='Original temporary cue and sounds; listening review pending',source='Tools/Render-IronveinAudio.py; original notes and synthesis, no samples, copyrighted score, external service or paid generation',sample_rate=SR,files=reports),indent=2)+'\n')
print(json.dumps(dict(files=len(reports),music_seconds=reports[0]['seconds'],clipped_samples=sum(r['clipped_samples'] for r in reports))))
