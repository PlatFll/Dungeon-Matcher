"""Read-only refinement scope checks against the captured original ability."""
from pathlib import Path
import json,sys
from PIL import Image
root=Path(__file__).resolve().parents[1]
sys.path.insert(0,str(root.parent/'CombatIdles/Scripts'))
from inspect_idles import read_ase
before,_=read_ase(root/'Originals/IdleRefinement/BarricadeVillager_Ability_Before.aseprite')
after,_=read_ase(root/'BarricadeVillager_Ability.aseprite')
changed=[]
for f,(a,b) in enumerate(zip(before,after)):
    points=[(x,y) for y in range(a.height) for x in range(a.width) if a.getpixel((x,y))!=b.getpixel((x,y))]
    if points:
        assert 2<=f<=6,(f,'unexpected changed ability frame')
        assert all(41<=x<=68 and 58<=y<=63 for x,y in points),(f,'outside knee area')
        changed.append({'frame':f+1,'changed_pixels':len(points),'bounds':[min(x for x,y in points),min(y for x,y in points),max(x for x,y in points)+1,max(y for x,y in points)+1]})
    assert a.crop((0,0,96,58)).tobytes()==b.crop((0,0,96,58)).tobytes()
    assert a.crop((0,58,41,64)).tobytes()==b.crop((0,58,41,64)).tobytes()
for character in ['Miner','BasketVillager','BarricadeVillager']:
    frames,_=read_ase(root/(character+'_Idle.aseprite'))
    floor=frames[0].crop((0,frames[0].height-2,frames[0].width,frames[0].height))
    assert all(im.crop((0,im.height-2,im.width,im.height)).tobytes()==floor.tobytes() for im in frames)
    reference=Image.open(root/(character+'_Reference.png')).convert('RGBA')
    assert frames[0].tobytes()==reference.tobytes()
for sheet in root.glob('*.png'):
    if sheet.stem.endswith('_Reference'):continue
    folder='CombatIdles' if sheet.stem.endswith('_Idle') else 'CombatActions'
    imported=root.parents[1]/'Assets/_Game/Art'/folder/sheet.name
    assert sheet.read_bytes()==imported.read_bytes(),sheet.name
report={'ability_knee_changes':changed,'upper_body_and_left_contact_pixels_unchanged':True,'idle_bottom_two_rows_exactly_planted':True,'ready_references_unchanged':True,'all_eight_unity_sheets_exact':True}
(root/'RefinementValidation.json').write_text(json.dumps(report,indent=2)+'\n')
print('PASS: knee edits confined to frames 3–7/lower-leg area; protected ability artwork intact; exact planted soles, ready references and Unity sheet copies.')
