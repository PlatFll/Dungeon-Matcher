"""Original synthesized, short playful cues; deterministic source, no samples."""
from pathlib import Path
import math,random,wave,struct,json,hashlib
rate=44100
out=Path(__file__).resolve().parents[1]/'Assets/_Game/Resources/Audio/Combat'
specs={'AttenderJump':(.22,170,430,.16),'AttenderStep':(.12,100,48,.4),'AttenderCrash':(.26,125,40,.65),'KoboldSpin':(.38,720,1150,.18),'KoboldShot':(.12,390,110,.75),'KoboldRicochet':(.09,1900,900,.08)}
records=[]
for index,(name,(seconds,start,end,noise)) in enumerate(specs.items()):
    rng=random.Random(41200+index);samples=[];phase=0;low=0
    for i in range(round(seconds*rate)):
        t=i/rate;p=t/seconds;phase+=2*math.pi*(start+(end-start)*p)/rate
        n=rng.uniform(-1,1);low=.75*low+.25*n
        body=math.sin(phase)*.7+math.sin(phase*2.03)*.2
        tick=1
        if name=='KoboldSpin':tick=.22+.78*max(0,math.cos(t*2*math.pi*24))**12
        envelope=min(1,t/.002)*(1-p)**2
        value=((1-noise)*body+noise*(low if name.startswith('Attender') else n))*envelope*tick
        samples.append(value)
    peak=max(abs(s) for s in samples);gain=.68/max(.001,peak)
    data=b''.join(struct.pack('<h',int(s*gain*32767)) for s in samples)
    path=out/(name+'.wav')
    with wave.open(str(path),'wb') as w:w.setnchannels(1);w.setsampwidth(2);w.setframerate(rate);w.writeframes(data)
    records.append(dict(name=name,seconds=seconds,rate=rate,channels=1,peak=.68,sha256=hashlib.sha256(path.read_bytes()).hexdigest()))
record=Path(__file__).resolve().parents[1]/'ArtSource/FrostwakePlayables/production/audio.json'
record.write_text(json.dumps(records,indent=2));print('Rendered six original mono cues; 0.68 peak, faded endpoints.')
