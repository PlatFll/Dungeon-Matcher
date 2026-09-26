"""Historical preservation checks for the superseded Farmer head-only correction."""
import hashlib,json,sys
from PIL import Image
from inspect_idles import ROOT,read_ase,color_counts

candidate='--candidate' in sys.argv
path=ROOT/('Review/Farmer_HeadRestored.aseprite' if candidate else 'Originals/BeforeTurnBasedPass/Farmer_Idle.aseprite')
before_path=ROOT/'Originals/BeforeHeadRestore/Farmer_Idle.aseprite'
assert hashlib.sha256(before_path.read_bytes()).hexdigest()=='aed04b1edc0e63d378c9c8dd262b20b0684368197253dd5ca3b174ed267c93ef'
before,old_ms=read_ase(before_path)
after,ms=read_ase(path)
original,original_ms=read_ase(ROOT/'Originals/FarmerFluidAnim2.aseprite')
assert ms==old_ms==[130]*9 and original_ms==[130]*11
assert all(im.size==(64,64) for im in after)
palette=json.loads((ROOT/'Scripts/palettes.json').read_text())['Farmer']
assert set(color_counts(after))<=set('#'+h for h in palette)
assert all(p[3] in (0,255) for im in after for p in im.getdata())

# All modified pixels must belong to the head's area. Everything below the
# original lowest chin, including both hands, remains byte-identical per frame.
changes=[]
for a,b in zip(before,after):
    assert a.crop((0,39,64,64)).tobytes()==b.crop((0,39,64,64)).tobytes(),'body/hand/shaft/feet edit'
    q=[(x,y) for y in range(64) for x in range(64) if a.getpixel((x,y))!=b.getpixel((x,y))]
    assert all(y<39 and (y<25 or x>=19) for x,y in q),'edit outside head'
    changes.append(len(q))

# Check the actual rigid tool pixels (a rectangular tine box would also include
# the moving brim above it). Both grip shapes and shaft must share its offset.
ref=before[0]
offsets=[]
boxes={'tines':(0,23,19,39),'left_grip':(20,39,26,43),
       'right_grip':(41,49,46,53),'lower_shaft':(43,53,57,60)}
for im in after:
    top=min(y for y in range(20,31) for x in range(10) if im.getpixel((x,y))[3])
    dy=top-23;offsets.append(dy)
    for part,(x0,y0,x1,y1) in boxes.items():
        for y in range(y0,y1):
            for x in range(x0,x1):
                p=ref.getpixel((x,y))
                if p[3]:assert im.getpixel((x,y+dy))==p,(part,x,y,dy)
assert offsets==[0,0,1,2,3,3,2,1,0]

# Compare the same face region relative to the crown in each frame, removing
# vertical translation. This region ends above the neck and excludes the fork.
def face_drawings(frames):
    return {im.crop((19,im.getbbox()[1]+21,52,im.getbbox()[1]+28)).tobytes() for im in frames}
tops=[im.getbbox()[1] for im in after]
assert len(face_drawings(before))==1,'working base was not the static-head revision'
assert len(face_drawings(after))>1,'single translated face cutout'

unchanged={}
for name,digest in {
 'Rattlebones':'77750e6542abb6131be3d4816048b8b6a80ed27ff506340e3c197e0d5cf74d4d',
 'PanVillager':'396a2c70e6c268b85a7dcc4685d939f5840bf3e39abb4e241751b2fc2ad738b6',
 'Bardley':'9f32f4e922f5a9e0f4cffa5ebd17b9b4bbf018f35a7ac8dd424f3f2fe4089de8'}.items():
    checked_path=ROOT/('Originals/BeforeTurnBasedPass' if name=='PanVillager' else '')/(name+'_Idle.aseprite')
    assert hashlib.sha256(checked_path.read_bytes()).hexdigest()==digest,name
    unchanged[name]=digest
for filename,frames in [('Farmer_Original.png',original),('Farmer_Before.png',before)]:
    strip=Image.open(ROOT/'Comparisons'/filename).convert('RGBA')
    assert strip.size==(len(frames)*64,64)
    assert all(strip.crop((i*64,0,(i+1)*64,64)).tobytes()==im.tobytes() for i,im in enumerate(frames)),filename

report={'scope':'Historical head-only revision preserved in Originals/BeforeTurnBasedPass; superseded by TurnBasedValidation.json.',
 'frames':9,'canvas':[64,64],'frame_ms':ms,'loop_ms':sum(ms),
 'approved_opaque_colors':len(color_counts(after)),'off_palette_pixels':0,
 'head_top_y':tops,'head_changed_pixels_per_frame':changes,
 'distinct_face_drawings_after_removing_translation':len(face_drawings(after)),
 'previous_revision_distinct_face_drawings':len(face_drawings(before)),
 'torso_hands_lower_shaft_and_boots_identical_to_previous_revision':True,
 'pitchfork_and_grips_dy':offsets,'rigid_prop_pixels_preserved':True,
 'comparison_reference_sheets_match_original_files':True,
 'other_character_aseprite_sha256_unchanged':unchanged,
 'visual_review_required':'Head/body rhythm, attached neck and hat, native-size readability and loop seam are reviewed in FarmerHeadReview.html.'}
(ROOT/('Review/FarmerHeadValidation.json' if candidate else 'FarmerHeadValidation.json')).write_text(json.dumps(report,indent=2)+'\n')
print(json.dumps(report,indent=2))
