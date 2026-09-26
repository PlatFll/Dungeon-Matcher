"""Read-only correction checks against preserved stills and native part drawings."""
from pathlib import Path
import sys
ROOT=Path(__file__).resolve().parents[1]
sys.path.insert(0,str(ROOT.parent/'CombatIdles/Scripts'))
from inspect_idles import read_ase

HELMETS={'RoyalLancer','RoyalArbalist'}
def check_still(name,original,ready):
    same=original.getchannel('A').tobytes()==ready.getchannel('A').tobytes()
    if name not in HELMETS:
        assert same,(name,'original silhouette changed')
        return {'static_mask_unchanged':True,'helmet_resized':False}
    before=read_ase(ROOT/'BeforeCorrection/Recolored'/f'{name}.aseprite')[0][0]
    changed=[]
    for y in range(64):
        for x in range(64):
            if before.getpixel((x,y))!=ready.getpixel((x,y)):
                assert 13<=x<54 and 0<=y<42,(name,'change outside authorized helmet region',x,y)
                changed.append((x,y))
    assert changed,(name,'helmet was not enlarged')
    oldcolors={p for p in before.get_flattened_data() if p[3]}
    assert {p for p in ready.get_flattened_data() if p[3]}==oldcolors
    return {'static_mask_unchanged':same,'helmet_resized':True,'helmet_edit_bounds':[min(x for x,y in changed),min(y for x,y in changed),max(x for x,y in changed)+1,max(y for x,y in changed)+1]}

# Occlusion-safe regions inspected on the separated native drawings. These
# cover the exposed rigid surfaces, including both crossbow lower stirrups.
# The head can legitimately cover upper grips during the compressed pose.
def exposed(name,x,y):
    if name in ('CrossbowGuard','RoyalArbalist'):return y>=43 or x<17
    if name=='ShieldKnight':return x>=48 or y>=43
    if name=='Minotaur':return y>=40
    if name=='TownMarshal':return x<17 or x>50
    if name=='SpearGuard':return x<19 or y>=44
    if name=='SwordKnight':return x>=48 or y>=43
    if name in ('RoyalLancer','SpearKnight','SpearGuard','RoyalArcanist','RoyalMage','RoyalStandardBearer'):return x<21 or y>=44
    return x<21 or y>=43

def check_parts(name,frames):
    parts,_=read_ase(ROOT/'Review/Correction/RigParts'/f'{name}.aseprite')
    result={}
    for label,index in [('prop',3),('shield',4)]:
        part=parts[index]
        pixels=[(x,y,part.getpixel((x,y))) for y in range(64) for x in range(64) if part.getpixel((x,y))[3] and exposed(name,x,y)]
        if not pixels:continue
        offsets=[]
        for f,im in enumerate(frames):
            candidates=[]
            for dy in range(-3,4):
                for dx in range(-2,3):
                    wrong=[(x,y) for x,y,c in pixels if not (0<=x+dx<64 and 0<=y+dy<64 and im.getpixel((x+dx,y+dy))==c)]
                    candidates.append((len(wrong),dx,dy,wrong))
            mismatch,dx,dy,wrong=min(candidates)
            assert not mismatch,(name,label,f+1,'rigid exposed pixels differ',mismatch,wrong[:16])
            # No source pixel of the rigid part is allowed outside the canvas,
            # including the upper region omitted only for head occlusion.
            for y in range(64):
                for x in range(64):
                    if part.getpixel((x,y))[3]:assert 0<=x+dx<64 and 0<=y+dy<64,(name,label,f+1,'clipping',x,y)
            offsets.append([dx,dy])
        result[label]={'exposed_pixels_checked_per_frame':len(pixels),'translation_only':True,'no_part_pixels_outside_canvas':True,'offsets':offsets}
    # Inspect the foot pixels remaining visible in the body drawing. A rigid
    # bow may pass in front of a sole; that overlap must be the exact bow color.
    for f,im in enumerate(frames):
        for x in range(64):
            expected=parts[1].getpixel((x,63))
            if not expected[3]:continue
            covered=False
            for label,index in [('prop',3),('shield',4)]:
                if label not in result:continue
                dx,dy=result[label]['offsets'][f];sx,sy=x-dx,63-dy
                if 0<=sx<64 and 0<=sy<64:
                    c=parts[index].getpixel((sx,sy))
                    if c[3]:expected=c;covered=True
            assert im.getpixel((x,63))==expected,(name,f+1,'body sole moved',x,covered)
    result['body_soles_fixed_with_exact_prop_occlusion']=True
    return result

if __name__=='__main__':
    for path in sorted((ROOT/'Idles').glob('*_Idle.aseprite')):
        name=path.stem.removesuffix('_Idle');frames,_=read_ase(path)
        result=check_parts(name,frames)
        print(name,{k:v['exposed_pixels_checked_per_frame'] for k,v in result.items() if isinstance(v,dict)})
    print('PASS: exposed rigid shapes retain exact pixels under translation; complete parts stay on-canvas; body soles fixed with exact prop occlusion.')
