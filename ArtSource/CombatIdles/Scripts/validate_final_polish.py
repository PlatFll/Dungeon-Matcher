"""Prove the final art edits stay inside the user-authorized pixels/frames."""
import json,hashlib
from inspect_idles import ROOT,read_ase

report={'status':'Historical final-cheek/scarf pass; checked against Originals/BeforeCombatActions'}
cheeks={(21,31),(22,31),(23,32),(22,33),(23,33),(24,33),(38,33),(39,33),(40,33),(24,34),(26,34),(27,34),(37,34)}
for name in ['Farmer','PanVillager']:
    before,old_ms=read_ase(ROOT/'Originals/BeforeFinalPolish'/f'{name}_Idle.aseprite')
    after,ms=read_ase(ROOT/'Originals/BeforeCombatActions'/f'{name}_Idle.aseprite')
    assert ms==old_ms==[130]*9
    changes=[]
    for f,(a,b) in enumerate(zip(before,after)):
        diff={(x,y) for y in range(64) for x in range(64) if a.getpixel((x,y))!=b.getpixel((x,y))}
        if name=='Farmer':
            assert diff==(cheeks if f in (0,8) else set()),(name,f,diff)
        else:
            head=[0,0,1,2,4,5,4,2,0][f]
            assert all(43<=x<=48 and 25+head<=y<=31+head for x,y in diff),(name,f,diff)
            assert all(b.getpixel((x,y))[3]==0 or b.getpixel((x,y))[:3] in {(10,13,17),(91,81,69),(62,49,42),(159,144,122),(154,133,111)} for x,y in diff)
        changes.append(len(diff))
    assert after[0].tobytes()==after[8].tobytes(),'first/last seam'
    report[name]={'changed_pixels_per_frame':changes,'outside_authorized_scope':0,'durations_ms':ms}
previous=json.loads((ROOT/'TurnBasedValidation.json').read_text())['unchanged_character_files']
for name in ['Rattlebones','Bardley']:
    assert all(hashlib.sha256((ROOT/'Originals/BeforeCombatActions'/f'{name}_Idle.{ext}').read_bytes()).hexdigest()==digest for ext,digest in previous[name].items())
    report[name]={'all_delivery_files_unchanged':True}
(ROOT/'FinalPolishValidation.json').write_text(json.dumps(report,indent=2)+'\n')
print(json.dumps(report,indent=2))
