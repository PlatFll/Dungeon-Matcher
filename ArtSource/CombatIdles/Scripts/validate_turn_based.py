"""Read-only checks of reference continuity, blink phase and rigid iron-pan motion."""
import hashlib,json,sys
from inspect_idles import ROOT,read_ase,color_counts

candidate='--candidate' in sys.argv
def frames(n):return read_ase(ROOT/(f'Review/{n}_TurnBased.aseprite' if candidate else f'{n}_Idle.aseprite'))
def rgb(im,x,y):return '%02X%02X%02X'%im.getpixel((x,y))[:3]
def sha(p):return hashlib.sha256(p.read_bytes()).hexdigest()
palette=json.loads((ROOT/'Scripts/palettes.json').read_text())
reference,_=read_ase(ROOT/'Originals/TurnBasedReference/Farmer_OpenReference.aseprite')
farmer,fd=frames('Farmer');pan,pd=frames('PanVillager')
assert fd==pd==[130]*9
for n,fs in [('Farmer',farmer),('PanVillager',pan)]:
    assert all(im.size==(64,64) for im in fs)
    assert set(color_counts(fs))<=set('#'+c for c in palette[n])
    assert all(p[3] in (0,255) for im in fs for p in im.getdata())
    assert all(im.getbbox()[3]==64 for im in fs),'ground contact'

# Every Farmer pose retains the complete user reference's silhouette, including
# shoulders, torso, arms, prop and legs. No independently delayed head cutout.
source_frames=[4,4,5,6,7,8,6,5,4]
for f,(im,i) in enumerate(zip(farmer,source_frames)):
    # Final user-requested cheek rounding changes three contour pixels in 1/9.
    cheek_alpha={(21,31),(22,33),(24,34)} if f in (0,8) else set()
    assert all(im.getpixel((x,y))[3]==reference[i].getpixel((x,y))[3] for y in range(64) for x in range(64) if (x,y) not in cheek_alpha),'Farmer reference geometry'
farmer_eye=[]
for im,top,bottom in zip(farmer,[26,26,26,28,30,31,28,26,26],[28,28,29,30,32,33,30,29,28]):
    farmer_eye.append(sum(rgb(im,24,y)=='0A0D11' for y in range(top,bottom+1)))
assert farmer_eye==[3,3,4,3,2,1,2,4,3]
farmer_top=[im.getbbox()[1] for im in farmer]
assert farmer_top[:6]==sorted(farmer_top[:6])
assert farmer_top[5:]==sorted(farmer_top[5:],reverse=True),'head reverses direction during rise'

base,_=read_ase(ROOT/'Originals/BeforeTurnBasedPass/PanVillager_Idle.aseprite');base=base[0]
iron={(x,y) for y in range(64) for x in range(64) if base.getpixel((x,y))[3] and rgb(base,x,y) in {'2B313A','55606E','93A1B0'}}
prop=set(iron)
for x,y in iron:
    prop.update((xx,yy) for yy in range(max(0,y-1),min(64,y+2)) for xx in range(max(0,x-1),min(64,x+2)) if base.getpixel((xx,yy))[3] and rgb(base,xx,yy)=='0A0D11')
grip={(x,y) for y in range(42,52) for x in range(37,45) if base.getpixel((x,y))[3]}
offsets=[];pan_eye=[];pan_top=[]
base_iron_top=min(y for x,y in iron)
for im in pan:
    dy=min(y for y in range(64) for x in range(64) if rgb(im,x,y) in {'2B313A','55606E','93A1B0'})-base_iron_top
    offsets.append(dy)
    assert all(im.getpixel((x,y+dy))==base.getpixel((x,y)) for x,y in prop),'iron pan warped'
    assert all(im.getpixel((x,y+dy))==base.getpixel((x,y)) for x,y in grip),'hand disconnected'
    # The rigid foreground pan passes over the left boot on the dip. Check the
    # visible boot pixels separately from this expected foreground occlusion.
    covered={(x,y+dy) for x,y in prop}
    assert all(im.getpixel((x,y))==base.getpixel((x,y)) for y in range(60,64) for x in range(18,47) if (x,y) not in covered),'visible boot drift'
    assert im.crop((19,63,47,64)).tobytes()==base.crop((19,63,47,64)).tobytes(),'sole contact drift'
    head=im.getbbox()[1]-base.getbbox()[1];pan_top.append(im.getbbox()[1])
    pan_eye.append(sum(rgb(im,20,y+head)=='0A0D11' for y in range(22,26)))
assert pan_eye==[4,4,4,4,2,1,2,4,4]
assert pan_top[:6]==sorted(pan_top[:6]) and pan_top[5:]==sorted(pan_top[5:],reverse=True)

unchanged={}
previous=json.loads((ROOT/'Validation.json').read_text())
for n in ['Rattlebones','Bardley']:
    for ext,h in previous[n]['files'].items():
        assert sha(ROOT/f'{n}_Idle.{ext}')==h,(n,ext,'changed')
    unchanged[n]=previous[n]['files']
report={'frame_ms':130,'loop_ms':1170,'canvas':[64,64],
 'Farmer':{'original_reference_frames_one_based':[i+1 for i in source_frames],
   'reference_pose_alpha_preserved_except_authorized_cheek_rounding':True,'head_top_y':farmer_top,'eye_height_px':farmer_eye,
   'closed_eye_frame':6,'reopens_during_recovery':True,'off_palette_pixels':0},
 'PanVillager':{'head_top_y':pan_top,'eye_height_px':pan_eye,'closed_eye_frame':6,
   'reopens_during_recovery':True,'pan_and_grip_offset_y':offsets,'rigid_pan_pixels_preserved':True,
   'grip_pixels_preserved':True,'unoccluded_boot_pixels_preserved':True,'sole_contact_preserved':True,'off_palette_pixels':0},
 'unchanged_character_files':unchanged,
 'visual_review':'Full-speed 1x/4x family playback, lowest pose, recovery, before/after comparison and loop seam.'}
(ROOT/('Review/TurnBasedValidation.json' if candidate else 'TurnBasedValidation.json')).write_text(json.dumps(report,indent=2)+'\n')
print(json.dumps(report,indent=2))
