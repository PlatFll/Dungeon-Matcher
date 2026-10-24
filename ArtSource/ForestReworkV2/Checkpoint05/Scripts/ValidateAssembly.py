from pathlib import Path
from PIL import Image,ImageDraw
import json,hashlib,sys,shutil,xml.etree.ElementTree as ET
p=Path(sys.argv[1]);o=p/'ArtSource/ForestReworkV2/Checkpoint05';a=p/'ArtSource/ForestReworkV2/Checkpoint04';t=p/'ArtSource/ForestReworkV2/Checkpoint03'
def read(f):return Image.open(f).convert('RGBA')
full=read(o/'Previews/Unity-Full-720x1360.png');double=read(o/'Previews/Unity-Full-1440x2720.png')
old=read(p/'ArtSource/ForestHybrid/Stage05/Previews/Unity-Full-720x1360.png');top=read(t/'Previews/Unity-After-720x480.png');board=read(a/'Previews/Unity-After-720x704.png')
assert full.size==(720,1360) and double.size==(1440,2720)
assert full.resize(double.size,Image.Resampling.NEAREST).tobytes()==double.tobytes(),'Full scene not exact integer 2x'
assert full.crop((0,0,720,480)).tobytes()==top.tobytes(),'Approved top/portrait changed'
assert full.crop((104,528,616,1040)).tobytes()==board.crop((104,96,616,608)).tobytes()==old.crop((104,528,616,1040)).tobytes(),'Approved cells/gems changed'
# The old board's lower ivy extended below y=1104 beside (not over) the energy
# bar. Its removal belongs to approved checkpoint 4. Reconstruct only those
# source-alpha pixels using the unchanged continuous backing, then compare all
# lower pixels, including every actual HUD pixel. Do not mask away UI changes.
oldUses=json.loads((p/'ArtSource/ForestHybrid/Stage05/Diagnostics/Unity-720x1360.json').read_text())['images']
lowerExpected=old.crop((0,1104,720,1360));legacy=Image.new('L',(720,256))
bgNative=read(p/'Assets/_Game/Art/ForestHybrid/Stage02/Portrait_Inset.png');bg=bgNative.resize((bgNative.width*2,bgNative.height*2),Image.Resampling.NEAREST)
for u in oldUses:
    r=u['screenRect']
    if u['path'].endswith('/ForestHybrid/Stage03/Board_Ivy_Lower.png') and r['y']<256:
        im=read(p/u['path']);im=im.resize((im.width*2,im.height*2),Image.Resampling.NEAREST);left=round(r['x']);topY=1360-round(r['y']+r['height'])
        for y in range(im.height):
            for x in range(im.width):
                sx=left+x;sy=topY+y
                if sy>=1104 and im.getpixel((x,y))[3]:
                    lowerExpected.putpixel((sx,sy-1104),bg.getpixel((sx%bg.width,sy%bg.height)));legacy.putpixel((sx,sy-1104),255)
assert full.crop((0,1104,720,1360)).tobytes()==lowerExpected.tobytes(),'Shared HUD/backing differs beyond the removed legacy board vine'
legacyCount=sum(1 for c in legacy.get_flattened_data() if c)
# Independent visible-pixel mask from approved board module placements, not the new builder.
mask=Image.new('L',(720,704));uses=json.loads((a/'Diagnostics/Unity-720x704.json').read_text())['images']
for u in uses:
    if '/ForestReworkV2/Checkpoint04/' not in u['path']:continue
    im=read(p/u['path']);r=u['rect'];x=round(r['x']);y=704-round(r['y'])-round(r['height']);w=round(r['width']);h=round(r['height'])
    if u['type']=='Tiled':
        assert set(im.getchannel('A').get_flattened_data())=={255};ImageDraw.Draw(mask).rectangle((x,y,x+w-1,y+h-1),fill=255)
    else:mask.paste(im.getchannel('A').resize((w,h),Image.Resampling.NEAREST),(x,y))
actual=full.crop((0,432,720,1136));changed=sum(1 for s,n,m in zip(board.get_flattened_data(),actual.get_flattened_data(),mask.get_flattened_data()) if m and s!=n)
assert changed==0,('Approved board frame pixels changed',changed)
probe=read(o/'Previews/Unity-MaskProbe-720x1360.png');evidence=json.loads((o/'Diagnostics/Unity-720x1360.json').read_text());r=evidence['portraitMask']
box=(round(r['x']),1360-round(r['y']+r['height']),round(r['x']+r['width']),1360-round(r['y']))
hits=0
for y in range(1360):
    for x in range(720):
        c=full.getpixel((x,y));q=probe.getpixel((x,y));assert c[:3]!=(255,0,255),('Camera gap',x,y)
        if c!=q:
            hits+=1;assert box[0]<=x<box[2] and box[1]<=y<box[3],('Portrait mask spill',x,y)
assert hits>1000,'Mask probe did not render'
structure=read(o/'Previews/Unity-Structure-720x1360.png');empty=read(a/'Previews/Unity-Empty-720x704.png')
assert structure.crop((104,528,616,1040)).tobytes()==empty.crop((104,96,616,608)).tobytes(),'Empty cells differ'
assert evidence['rosterCount']==3
xml=ET.parse(o/'Diagnostics/UnityTests.xml').getroot();assert xml.attrib['result']=='Passed' and xml.attrib['failed']=='0'
checks={'dimensions':[720,1360],'exact_2x':[1440,2720],'approved_top_720x480_exact':True,'original_board_and_64px_cells_gems_exact':True,'approved_board_frame_changed_pixels':changed,'shared_lower_HUD_pixels_exact':True,'lower_region_exception':'Removed old board ivy reveals exact original continuous backing; all other pixels identical','removed_legacy_vine_alpha_pixels_below_1104':legacyCount,'portrait_mask':box,'portrait_mask_spill_pixels':0,'mask_probe_changed_pixels_inside':hits,'camera_gap_pixels':0,'static_forest_roster_samples':3,'empty_cells_exact':True,'unity_tests_passed':int(xml.attrib['passed'])}
(o/'Diagnostics/PixelValidation.json').write_text(json.dumps(checks,indent=2))
shutil.copy2(p/'ArtSource/ForestHybrid/Stage05/Previews/Unity-Full-720x1360.png',o/'Previews/Unity-Before-720x1360.png')
if Path(__file__).resolve()!=(o/'Scripts/ValidateAssembly.py').resolve():shutil.copy2(__file__,o/'Scripts/ValidateAssembly.py')
print(json.dumps(checks,indent=2))
