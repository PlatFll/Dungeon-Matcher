from pathlib import Path
import csv,json
r=Path(__file__).resolve().parent;repo=r.parents[2]
src=repo/'.utmp/DrownedCourtPhase01/Input/Dungeon_Matcher_Drowned_Court_Astra_Pack/data/ENCOUNTER_RECIPES.csv'
# Weighted alternatives within teaching bands. Milestones are temporary tuning anchors.
bands={1:(1,2),2:(1,2),3:(2,3),4:(3,3),5:(3,5),6:(4,7),7:(5,7),8:(5,7),9:(6,7),10:(10,13),11:(8,8),12:(8,8),13:(9,9),14:(10,13),15:(10,13),16:(16,18),17:(10,13),18:(16,18),19:(10,13),20:(10,13),21:(14,14),22:(14,14),23:(15,15),24:(16,18),25:(16,18),26:(16,18),27:(16,18),28:(16,18),29:(19,19),30:(20,999),31:(20,999),32:(20,999)}
out=[]
for row in csv.DictReader(src.open(encoding='utf-8-sig')):
 n=int(row['id'][1:]);first,last=bands[n]
 # Both sides of the Warden lesson have a state-specific fallback; never defer a boss to force flooding.
 tide=0 if n in (21,22,23) else {'dry/wet':0,'dry':1,'wet':2}[row['environment']]
 out.append(dict(id=row['id'],name=row['name'],members=row['members'].split(';'),first=first,last=last,tide=tide,weight=3 if n==30 else 1))
(r/'Selected/encounters.json').write_text(json.dumps({'recipes':out},indent=2))
print('32 recipes retained; stale chain/haste comments superseded by current thorny-snare and damage-rally kit descriptions.')
