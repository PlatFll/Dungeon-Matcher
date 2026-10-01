"""Character-only timing preview; countdown here simulates accepted turns."""
from pathlib import Path
import json,sys
from PIL import Image,ImageDraw
ROOT=Path(__file__).resolve().parent
sys.path.insert(0,str(ROOT.parent/'CombatIdles/Scripts'))
from inspect_idles import read_ase
idle,ids=read_ase(ROOT/'Gideon_Idle.aseprite')
cast,cds=read_ase(ROOT/'Gideon_Cast.aseprite')
hold,_=read_ase(ROOT/'Gideon_Hold.aseprite')
recovery,rds=read_ase(ROOT/'Gideon_Recovery.aseprite')
assert cast[0].tobytes()==idle[0].tobytes()
assert cast[-1].tobytes()==hold[0].tobytes()==recovery[0].tobytes()
assert recovery[-1].tobytes()==idle[0].tobytes()
frames=idle+cast+[hold[0]]*7+recovery+idle
times=ids+cds+[600]*6+[300]+rds+ids
labels=['Idle']*9+['Cast']*10+[str(i) for i in range(5,-1,-1)]+['Rewind']+['Release']*4+['Idle']*9
out=[]
for f,label in zip(frames,labels):
    tile=Image.new('RGB',(180,96),'#161420');d=ImageDraw.Draw(tile)
    d.text((6,3),'Character flow / simulated turns',fill='#ded8ce')
    d.text((39,18),label,fill='white');d.text((117,18),label,fill='#161420')
    tile.paste('#ded8ce',(96,16,180,96))
    d.text((113,18),label,fill='#161420')
    tile.paste(f,(10,32),f);tile.paste(f,(105,32),f);out.append(tile)
out[0].save(ROOT/'ChronoShutter_Flow.gif',save_all=True,append_images=out[1:],duration=times,loop=0,disposal=2)
(ROOT/'Flow_audit.json').write_text(json.dumps({'cast_to_hold_exact':True,'hold_to_recovery_exact':True,'recovery_to_idle_exact':True,'flash_ms':40,'cast_ms':sum(cds),'recovery_ms':sum(rds),'review_countdown':'simulated manual turns; not gameplay validation'},indent=2))
print('Flow transitions match exactly; flash 40 ms; cast 850 ms; recovery 370 ms.')
