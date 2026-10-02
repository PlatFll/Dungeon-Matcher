"""Original procedural score and synthesized instruments, for listening review.

No samples, external compositions, models or paid services are used.
"""
import json, math, wave
from pathlib import Path
import numpy as np

ROOT=Path(__file__).resolve().parents[1]
OUT=ROOT/'ArtSource/Forest/Production/Audio'
OUT.mkdir(parents=True,exist_ok=True)
SR=44100
BPM=112
BEAT=60/BPM
BARS=64
LENGTH=BARS*4*BEAT
N=round(LENGTH*SR)
mix=np.zeros((N,2),dtype=np.float32)
rng=np.random.default_rng(5731)
score=[]

def note(kind,pitch,beat,duration,gain,pan=0):
    score.append(dict(instrument=kind,midi=pitch,beat=beat,duration=duration,gain=gain,pan=pan))
    t=np.arange(round(duration*BEAT*SR),dtype=np.float32)/SR
    freq=440*2**((pitch-69)/12)
    phase=2*np.pi*freq*t
    if kind=='plucked wood':
        sound=sum(np.sin(phase*k+.04*k)*np.exp(-t*(3.5+k*.7))/(k**1.8) for k in range(1,8))
        env=np.minimum(1,t/.004)*np.minimum(1,(t[-1]-t)/.05)
    elif kind=='reed':
        phase=phase+.028*np.sin(2*np.pi*5*t)*np.minimum(1,t/.25)
        sound=np.sin(phase)+.20*np.sin(2*phase)+.075*np.sin(3*phase)
        env=np.minimum(1,t/.08)*np.minimum(1,(t[-1]-t)/.12)*(.8+.2*np.exp(-t*4))
    elif kind=='bass':
        sound=np.sin(phase)+.15*np.sin(2*phase)
        env=np.minimum(1,t/.012)*np.exp(-t*1.8)*np.minimum(1,(t[-1]-t)/.08)
    elif kind=='seed chime':
        sound=np.sin(phase)*np.exp(-t*4)+.25*np.sin(phase*2.75)*np.exp(-t*7)
        env=np.minimum(1,t/.005)*np.minimum(1,(t[-1]-t)/.07)
    else:
        noise=rng.normal(0,1,len(t)).astype(np.float32)
        if kind=='frame drum': sound=np.sin(2*np.pi*(76*t+2*np.sin(t*12)))*np.exp(-t*21)+noise*.15*np.exp(-t*44)
        else: sound=np.concatenate(([0],np.diff(noise)))*np.exp(-t*65)*.3
        env=np.minimum(1,t/.003)*np.minimum(1,(t[-1]-t)/.02)
    mono=np.asarray(sound*env*gain,dtype=np.float32)
    indices=(np.arange(len(t))+round(beat*BEAT*SR))%N
    left=math.sqrt((1-pan)/2);right=math.sqrt((1+pan)/2)
    mix[indices,0]+=mono*left;mix[indices,1]+=mono*right
    # A small original stereo room is circular so its tails also loop.
    for delay,amount in [(0.11,.11),(.19,.075),(.31,.04)]:
        echo=(indices+round(delay*SR))%N
        mix[echo,0]+=mono*right*amount;mix[echo,1]+=mono*left*amount

chords=[(50,53,57),(43,47,50),(52,55,59),(45,48,52), (50,53,57),(48,52,55),(43,47,50),(45,48,52)]
motifs=[(74,77,76,69,72,74),(79,77,74,71,74,76),(76,79,83,81,79,76),(81,79,76,72,74,69)]
for bar in range(BARS):
    section=bar//16; chord=chords[bar%8];start=bar*4
    note('bass',chord[0]-12,start,2.9,.095,-.08)
    note('bass',chord[0]-5,start+3,.8,.055,.06)
    pattern=[0,2,1,2,0,1,2,1]
    for step,degree in enumerate(pattern):
        pitch=chord[degree]+12+(12 if section==2 and step in (3,7) else 0)
        note('plucked wood',pitch,start+step*.5,.85,.045 if step%2 else .06,(-.35,.28)[step%2])
    # Two phrases, an airy middle passage, then a varied return.
    if section!=2 and bar%4 in (0,1,2):
        melody=motifs[(bar//4+section)%4]
        for step,pitch in enumerate(melody):
            note('reed',pitch,start+step*.5+.25,.65,.036 if section==0 else .043,.18)
    elif section==2 and bar%2==0:
        for step,degree in enumerate((2,1,0)):
            note('seed chime',chord[degree]+24,start+step*1.25,1.4,.040,-.2)
    if bar%8==7: note('seed chime',chord[2]+24,start+3,1.2,.034,.45)
    for position in (0,2): note('frame drum',40,start+position,.4,.067,0)
    for position in (.75,1.5,2.75,3.5): note('seed shaker',60,start+position,.18,.033,-.25 if position<2 else .25)

mix-=mix.mean(axis=0)
peak=float(np.max(np.abs(mix)))
mix*=.68/max(peak,1e-8)
pcm=np.round(mix*32767).astype('<i2')
destination=ROOT/'Assets/_Game/Audio/Music/Forest Lanterns - TEMP review.wav'
destination.parent.mkdir(parents=True,exist_ok=True)
with wave.open(str(destination),'wb') as w:
    w.setnchannels(2);w.setsampwidth(2);w.setframerate(SR);w.writeframes(pcm.tobytes())
(OUT/'score.json').write_text(json.dumps(dict(title='Forest Lanterns',status='Original temporary cue; listening approval pending',bpm=BPM,bars=BARS,notes=score),indent=2))
report=dict(duration_seconds=N/SR,sample_rate=SR,channels=2,peak=float(np.max(np.abs(mix))),rms=float(np.sqrt(np.mean(mix*mix))),clipped_samples=int(np.sum(np.abs(pcm)>=32767)),loop_boundary_step=float(np.max(np.abs(mix[-1]-mix[0]))),max_adjacent_step=float(np.max(np.abs(np.diff(mix,axis=0)))),source='Original notes and local synthesis in Tools/render_forest_temp_music.py; no external recordings or paid services.',status='Temporary review cue, not final music approval')
(OUT/'render-report.json').write_text(json.dumps(report,indent=2))
print(json.dumps(report,indent=2))
