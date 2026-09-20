"""Historical refinement checks: archived Farmer/Pan and unchanged archived Bardley."""
import hashlib,json,sys
from inspect_idles import ROOT,read_ase,color_counts

candidate='--candidate' in sys.argv
def frames(name):
    path=ROOT/'Review'/(name+'_Correction.aseprite') if candidate else ROOT/('Originals/BeforeTurnBasedPass' if name=='Farmer' else 'Originals/BeforeCombatActions')/(name+'_Idle.aseprite')
    return read_ase(path)[0]
def source(name):return read_ase(ROOT/'Originals/FamilyPass1'/(name+'_Idle.aseprite'))[0]
def rgb(im,x,y):return '#%02X%02X%02X'%im.getpixel((x,y))[:3]
def delta(values):return [values[(i+1)%9]-values[i] for i in range(9)]
palettes=json.loads((ROOT/'Scripts/palettes.json').read_text())
report={'scope':'Historical pre-turn-based Farmer/Pan revision; archived Bardley facial correction.'}
farmer=frames('Farmer');ref=source('Farmer')[0]
part_boxes={'tines':(0,23,19,39),'left_grip':(20,39,26,43),'right_grip':(41,49,46,53),'lower_shaft':(43,53,57,60)}
offsets=[]
for im in farmer:
    # Recover motion independently from the topmost pitchfork pixel.
    top=min(y for y in range(20,31) for x in range(0,10) if im.getpixel((x,y))[3])
    dy=top-23;offsets.append(dy)
    for part,(x0,y0,x1,y1) in part_boxes.items():
        for y in range(y0,y1):
            for x in range(x0,x1):
                p=ref.getpixel((x,y))
                if p[3]:assert im.getpixel((x,y+dy))==p,(part,x,y,dy)
    assert im.crop((20,60,43,64)).tobytes()==ref.crop((20,60,43,64)).tobytes(),'boot drift'
    assert set(color_counts([im]))<=set('#'+h for h in palettes['Farmer'])
head_tops=[min(y for y in range(0,20) for x in range(20,50) if im.getpixel((x,y))[3]) for im in farmer]
# The later Farmer-only correction restores original nod drawings. Do not impose
# the superseded two-pixel head cap; inspect its poses with validate_farmer_head.py.
assert max(abs(d) for d in delta(offsets))<=1
assert max(offsets)-min(offsets)==3
report['Farmer']={'head_top_y':head_tops,'head_travel_px':max(head_tops)-min(head_tops),
    'pitchfork_and_grips_dy':offsets,'all_checked_prop_and_grip_pixels_translate_rigidly':True,'boot_pixels_unchanged':True}

bard=frames('Bardley');old=source('Bardley');tops=[];body_tops=[];old_tops=[]
greens={'#176747','#4A9B3F','#86C83C','#D5FFAD'}
for before,after in zip(old,bard):
    a=min(y for y in range(25,39) if rgb(before,39,y)=='#0A0D11')
    b=min(y for y in range(25,39) if rgb(after,39,y)=='#0A0D11')
    old_tops.append(a);tops.append(b)
    glyph={(x,y):before.getpixel((x,y)) for y in range(a,a+7) for x in range(26,43) if rgb(before,x,y) in {'#0A0D11','#FDF5E5'}}
    moved={(x,y+b-a):p for (x,y),p in glyph.items()}
    assert all(after.getpixel(pos)==p for pos,p in moved.items()),'face pixel changed'
    actual={(x,y):after.getpixel((x,y)) for y in range(b,b+7) for x in range(26,43) if rgb(after,x,y) in {'#0A0D11','#FDF5E5'}}
    assert actual==moved,'added facial pixel'
    allowed=set(glyph)|set(moved)
    assert all(before.getpixel((x,y))==after.getpixel((x,y)) for y in range(64) for x in range(64) if (x,y) not in allowed),'body changed outside face repair'
    assert before.getchannel('A').tobytes()==after.getchannel('A').tobytes(),'body silhouette changed'
    assert all(rgb(after,x,y) in greens for x,y in set(glyph)-set(moved)),'non-slime fill'
    body_tops.append(min(y for y in range(5,25) for x in range(40,51) if rgb(after,x,y) in greens))
    assert set(color_counts([after]))<=set('#'+h for h in palettes['Bardley'])
relative=[a-b for a,b in zip(tops,body_tops)]
assert max(abs(d) for d in delta(tops))<=1
assert max(relative)-min(relative)<=1
report['Bardley']={'eye_top_before_y':old_tops,'eye_top_after_y':tops,'body_curl_top_y':body_tops,
    'eye_to_body_curl_offset_px':relative,'max_face_step_before_px':max(map(abs,delta(old_tops))),
    'max_face_step_after_px':max(map(abs,delta(tops))), 'eye_catchlight_smile_pixels_preserved':True,
    'body_alpha_and_non_face_pixels_preserved':True}
for name,digest in {'Rattlebones':'77750e6542abb6131be3d4816048b8b6a80ed27ff506340e3c197e0d5cf74d4d',
                    'PanVillager':'396a2c70e6c268b85a7dcc4685d939f5840bf3e39abb4e241751b2fc2ad738b6'}.items():
    path=ROOT/('Originals/BeforeTurnBasedPass' if name=='PanVillager' else '')/(name+'_Idle.aseprite')
    assert hashlib.sha256(path.read_bytes()).hexdigest()==digest
    report[name]={'editable_file_unchanged':True}
out=ROOT/'Review/RefinementValidation.json' if candidate else ROOT/'RefinementValidation.json'
out.write_text(json.dumps(report,indent=2)+'\n')
print(json.dumps(report,indent=2))
