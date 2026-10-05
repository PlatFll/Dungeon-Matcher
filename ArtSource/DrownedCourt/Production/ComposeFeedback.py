from pathlib import Path
import numpy as np,wave,json
root=Path(__file__).resolve().parents[3];out=root/'Assets/_Game/Resources/Audio/Combat';rate=32000
def save(name,signal):
 signal=signal/max(1,float(np.max(np.abs(signal))))*.6
 with wave.open(str(out/(name+'.wav')),'wb') as w:
  w.setnchannels(1);w.setsampwidth(2);w.setframerate(rate);w.writeframes((signal*32767).astype('<i2').tobytes())
t=np.arange(int(rate*.22))/rate
save('AirPickup',np.sin(2*np.pi*(660*t+1800*t*t))*np.exp(-t*15)*(1-np.exp(-t*200)))
t=np.arange(int(rate*.16))/rate
rng=np.random.default_rng(10052026)
save('CofferBreak',(rng.uniform(-1,1,len(t))*.5+np.sin(2*np.pi*180*t)*.45)*np.exp(-t*36))
t=np.arange(int(rate*.34))/rate
save('AirWarning',np.sin(2*np.pi*220*t)*np.exp(-t*8)*(1-np.exp(-t*80))*.7)
(Path(__file__).resolve().parent/'FeedbackProvenance.json').write_text(json.dumps({'author':'Original local oscillator/noise synthesis for this task','external_samples':False,'files':['AirPickup.wav','CofferBreak.wav','AirWarning.wav'],'rate':rate,'peak_ceiling':.6,'human_listening':'pending'},indent=2))
print('Three original feedback cues saved.')
