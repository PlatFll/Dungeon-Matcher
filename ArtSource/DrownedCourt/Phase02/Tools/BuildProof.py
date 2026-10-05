from pathlib import Path
from PIL import Image,ImageDraw,ImageFont,ImageChops
from collections import Counter
import hashlib,json,csv,shutil,html,random
ROOT=Path(__file__).resolve().parents[1]; REPO=ROOT.parents[2]
for folder in ("Review","References","Manifests"):
    (ROOT/folder).mkdir(exist_ok=True)
FONT=REPO/"Assets/_Game/Fonts/Thaleah/ThaleahFat.ttf"
def font(n):return ImageFont.truetype(str(FONT),n)
def load(p):return Image.open(p).convert("RGBA")
def scale(im,n):return im.resize((im.width*n,im.height*n),Image.Resampling.NEAREST)
def paste(dst,im,xy):dst.paste(im,xy,im)
def label(dst,xy,text,size=24,color="#F3E9CE",center=False):
    d=ImageDraw.Draw(dst);d.fontmode="1";f=font(size)
    if center:xy=(xy[0]-d.textlength(text,font=f)//2,xy[1])
    d.text(xy,text,font=f,fill=color)
def rect(dst,box,fill,outline=None,width=1):
    ImageDraw.Draw(dst).rectangle(box,fill=fill,outline=outline,width=width)
names=["DC_A01_Reef_Spearman","DC_A02_Hammerhead_Bruiser","DC_A03_Pearl_Thief","DC_A04_Pearl_Cantor"]
titles=["Reef Spearman","Hammerhead Bruiser","Pearl Thief","Pearl Cantor"]
refs={
"Elven_Scout":"ArtSource/Forest/Approved/Elven_Scout.png",
"Orc_Trailguard":"ArtSource/Forest/Approved/Orc_Trailguard.png",
"Snapvine":"ArtSource/Forest/RosterConcepts/Snapvine.png",
"Elven_Mender":"ArtSource/Forest/Approved/Elven_Mender.png",
"Farmer":"ArtSource/CombatIdles/References/Farmer_RoundFace.png",
"Bardley_Palette_Only":"ArtSource/CombatIdles/References/Bardley_Palette_Reference_Placement_Pending.png",
"Rattlebones":"Assets/_Game/Art/CombatIdles/Rattlebones_Idle.png"}
for name,path in refs.items():shutil.copyfile(REPO/path,ROOT/"References"/(name+".png"))
sources=[dict(id=name,path=path,sha256=hashlib.sha256((REPO/path).read_bytes()).hexdigest(),use="Existing reference; unchanged bytes") for name,path in refs.items()]
gems=[]
for name in ("Amber_tri","Amethyst","Emerald","Ruby","Sapphire (1)","Topaz"):
    path=REPO/"Assets/_Game/Art/Gems"/(name+".png")
    shutil.copyfile(path,ROOT/"References"/(name+".png"))
    gems.append(load(path)); sources.append(dict(id=name,path=str(path.relative_to(REPO)).replace("\\","/"),sha256=hashlib.sha256(path.read_bytes()).hexdigest(),use="Existing gem; exact pixels preserved in proof"))
# Actual PNG measurements. No inferred sizes from composites.
def measure(path):
    im=load(path); pixels=list(im.get_flattened_data()); colors=Counter(p[:3] for p in pixels if p[3])
    alpha=Counter(p[3] for p in pixels)
    return dict(file=str(path.relative_to(ROOT)).replace("\\","/"),sha256=hashlib.sha256(path.read_bytes()).hexdigest(),width=im.width,height=im.height,bounds_exclusive=im.getbbox(),visible_rgb_colors=len(colors),alpha_values=sorted(alpha),opaque_pixels=alpha[255],transparent_pixels=alpha[0],palette=[dict(hex="#%02X%02X%02X"%p,pixels=n) for p,n in colors.most_common()])
metrics={}
for path in sorted((ROOT/"Candidates").glob("*.png")):
    m=measure(path);m["id"]=path.stem;m["approval"]="CANDIDATE — user review pending"
    m["raw"]=measure(ROOT/"Raw"/path.name)
    if path.stem in names:
        im=load(path);raw=load(ROOT/"Raw"/path.name)
        assert im.size==(64,64) and set(m["alpha_values"])=={0,255}
        assert im.getchannel("A").tobytes()==raw.getchannel("A").tobytes()
        assert any(p["hex"]=="#0A0D11" for p in m["palette"])
        m["mask_identical_to_raw"]=True;m["native_resampling"]=False
        m["contact_note"]="Two planted contacts; no ground disk. Native lowest opaque row "+str(m["bounds_exclusive"][3]-1)
    metrics[path.stem]=m
(ROOT/"Manifests"/"CandidateManifest.json").write_text(json.dumps(list(metrics.values()),indent=2),encoding="utf8")
(ROOT/"Manifests"/"SourceRegister.json").write_text(json.dumps(sources,indent=2),encoding="utf8")
with (ROOT/"Manifests"/"TechnicalChecks.csv").open("w",newline="",encoding="utf8") as f:
    w=csv.writer(f);w.writerow(["candidate","native_width","native_height","bounds_exclusive","colors","alpha","raw_colors","mask_preserved","status","sha256"])
    for id,m in metrics.items():w.writerow([id,m["width"],m["height"],m["bounds_exclusive"],m["visible_rgb_colors"],m["alpha_values"],m["raw"]["visible_rgb_colors"],m.get("mask_identical_to_raw","n/a"),m["approval"],m["sha256"]])
# Neutral native comparison sheet and integer nearest enlargement.
pairs=[("Elven_Scout",names[0]),("Orc_Trailguard",names[1]),("Snapvine",names[2]),("Elven_Mender",names[3])]
native=Image.new("RGB",(640,272),"#707070")
for n,(ref,candidate) in enumerate(pairs):
    gx=(n%2)*320;gy=(n//2)*136
    label(native,(gx+12,gy+7),ref.replace("_"," ").upper()+" / "+titles[n].upper(),12)
    paste(native,load(ROOT/"References"/(ref+".png")),(gx+48,gy+37))
    paste(native,load(ROOT/"Candidates"/(candidate+".png")),(gx+196,gy+37))
    label(native,(gx+80,gy+109),"EXISTING",12,center=True)
    label(native,(gx+228,gy+109),"CANDIDATE",12,center=True)
native.save(ROOT/"Review"/"Native_Comparison_1x.png")
scale(native,3).save(ROOT/"Review"/"Native_Comparison_3x.png")
hero=Image.new("RGB",(1168,390),"#626262")
label(hero,(20,12),"DROWNED COURT / FOUR STILL CANDIDATES / 4X NEAREST",20)
for n,id in enumerate(names):
    paste(hero,scale(load(ROOT/"Candidates"/(id+".png")),4),(16+n*288,60))
    label(hero,(16+n*288,329),titles[n].upper(),20)
    label(hero,(16+n*288,354),"64X64  /  "+str(metrics[id]["visible_rgb_colors"])+" COLORS",16,color="#DBDBDB")
hero.save(ROOT/"Review"/"Four_Candidates_4x.png")
# Readable frame assembly: native crops from the one generated L segment.
frame=load(ROOT/"Candidates"/"DC_M01_Marine_Frame.png")
cap=frame.crop((2,2,16,16))
top=frame.crop((17,2,57,10))
def framed(dst,box,bg="#223D40",mult=1):
    x,y,w,h=box;rect(dst,(x,y,x+w-1,y+h-1),bg)
    c=scale(cap,mult);e=scale(top,mult);t=e.transpose(Image.Transpose.ROTATE_90)
    for xx in range(x+c.width,x+w-c.width,e.width):
        part=e.crop((0,0,min(e.width,x+w-c.width-xx),e.height))
        paste(dst,part,(xx,y));paste(dst,part.transpose(Image.Transpose.FLIP_TOP_BOTTOM),(xx,y+h-e.height))
    for yy in range(y+c.height,y+h-c.height,t.height):
        part=t.crop((0,0,t.width,min(t.height,y+h-c.height-yy)))
        paste(dst,part,(x,yy));paste(dst,part.transpose(Image.Transpose.FLIP_LEFT_RIGHT),(x+w-t.width,yy))
    paste(dst,c,(x,y));paste(dst,c.transpose(Image.Transpose.FLIP_LEFT_RIGHT),(x+w-c.width,y))
    paste(dst,c.transpose(Image.Transpose.FLIP_TOP_BOTTOM),(x,y+h-c.height))
    paste(dst,c.transpose(Image.Transpose.ROTATE_180),(x+w-c.width,y+h-c.height))
# Detect only the flat returned padding on the environment. Crop, never resize.
env=load(ROOT/"Candidates"/"DC_E01_Reefgate_Study.png")
mask=Image.new("L",env.size);mask.putdata([255 if not(min(p[:3])>230 and max(p[:3])-min(p[:3])<20) else 0 for p in env.get_flattened_data()])
envbox=mask.getbbox(); cropped=env.crop(envbox);cropped.save(ROOT/"Review"/"Reefgate_Native_Crop.png")
cell=load(ROOT/"Candidates"/"DC_M02_Marine_Cell.png")
assert all(p[3]==255 for p in cell.get_flattened_data())
board_cell=scale(cell,2) # 32 native -> 64 proof pitch, exact nearest-neighbor.
# Keep real HP, ranks and supply pixels.
ui=REPO/"Assets/_Game/Resources/UI/Finalized"
hpstart=load(REPO/"ArtSource/Forest/UI/Health_Start.png")
hpmid=load(REPO/"ArtSource/Forest/UI/Health_Middle.png")
hpend=load(REPO/"ArtSource/Forest/UI/Health_End.png")
hpfill=load(ui/"HealthFill.png")
def health(dst,x,y,w,text,badge):
    paste(dst,hpstart,(x,y))
    for xx in range(x+12,x+w-12):paste(dst,hpmid,(xx,y))
    paste(dst,hpend,(x+w-12,y))
    for xx in range(x+8,x+w-8):paste(dst,hpfill,(xx,y+8))
    paste(dst,load(ui/(badge+"Badge.png")),(x-12,y))
    label(dst,(x+w//2+4,y+2),text,20,center=True)
def gem_frame(dst,gem,x,y):
    rect(dst,(x+5,y+5,x+58,y+58),"#21383B","#B19D71",2)
    paste(dst,gem,(x,y))
def sprite_on_floor(dst,id,center,floor,mult=2):
    im=load(ROOT/"Candidates"/(id+".png"));b=im.getbbox()
    paste(dst,scale(im,mult),(center-im.width*mult//2,floor-b[3]*mult))
def screen(wet):
    out=Image.new("RGBA",(640,1152),"#152A30")
    # Local study backing, broad marine masonry drawn at integer pixel coordinates.
    d=ImageDraw.Draw(out)
    for row,y in enumerate(range(0,1152,64)):
        for x in range(-96 if row%2 else 0,640,96):
            d.rectangle((x+2,y+2,x+92,y+60),fill=("#203A3D" if (row+x//96)%3 else "#284144"))
            d.line((x+5,y+5,x+87,y+5),fill="#324C4B",width=2)
    framed(out,(16,16,608,386),"#21383B",2)
    # Small composition, cropped only; repeated edge column fills any remaining frame.
    area=Image.new("RGBA",(464,224),"#17383D")
    e=scale(cropped,2)
    paste(area,e,((464-e.width)//2,224-e.height))
    if wet:
        veil=Image.new("RGBA",area.size,(29,122,139,40))
        area=Image.alpha_composite(area,veil)
        water=ImageDraw.Draw(area)
        for x,y,r in [(35,32,3),(123,62,2),(391,18,2),(436,96,3)]:
            water.ellipse((x-r,y-r,x+r,y+r),outline=(113,186,192,120),width=1)
        water.line((0,10,464,10),fill=(92,158,162,170),width=2)
    paste(out,area,(156,74))
    framed(out,(25,76,128,294),"#162F35",1)
    # Player/bottom inset finish deliberately differs from the general backing.
    for y in range(100,349,40):d.line((38,y,139,y),fill="#315151",width=2)
    player=scale(load(ROOT/"References/Rattlebones.png").crop((0,0,64,64)),2);paste(out,player,(25,150))
    gem_frame(out,gems[2],57,85)
    health(out,39,287,101,"100/100","Player")
    centers=[229,381,533];actors=[names[0],names[1],names[2]]
    for n,(cx,id) in enumerate(zip(centers,actors)):
        label(out,(cx,113),["HIT 4.8S","HIT 6.0S","HIT 6.0S"][n],20,center=True)
        if n==2:label(out,(cx,78),"2",32,"#FDF5E5",True)
        sprite_on_floor(out,id,cx,284,2)
        health(out,cx-57,294,116,["60/60","110/110","65/65"][n],"Special" if n==2 else "Normal")
        gem_frame(out,gems[[1,0,4][n]],cx-32,327)
    framed(out,(223,26,208,43),"#203438",1)
    label(out,(327,33),"WAVE 1",28,center=True)
    # Existing control retained; settings screen itself is outside this proof.
    paste(out,load(ui/"SettingsNormal.png"),(560,26))
    label(out,(46,39),"DROWNED COURT",20)
    framed(out,(48,411,544,38),"#162E35",1)
    if wet:
        label(out,(76,419),"AIR",24)
        for n in range(5):
            color="#ABDED0" if n<3 else "#28464F"
            rect(out,(174+n*40,423,205+n*40,437),color,"#09232D",2)
            if n<3:rect(out,(178+n*40,425,201+n*40,427),"#E7F3D8")
        label(out,(427,419),"TIDE 10",24)
    else:label(out,(320,419),"THE TIDE IS LOW",24,"#92AAA2",True)
    framed(out,(52,452,536,536),"#142A2C",1)
    # Board location is IDENTICAL dry and wet; filled opaque cells never expose backing.
    rng=random.Random(261005)
    grid=[[rng.randrange(6) for x in range(8)] for y in range(8)]
    for y,row in enumerate(grid):
        for x,g in enumerate(row):
            xy=(64+x*64,464+y*64);paste(out,board_cell,xy)
            # Water affects only cell backing; approved gem pixels remain byte-identical.
            if wet:
                patch=Image.new("RGBA",(64,64),(20,104,117,19));out.alpha_composite(patch,xy)
            paste(out,gems[g],xy)
    if wet:
        # Oxygen outlines are local layout placeholders, not produced gameplay/VFX assets.
        for x,y in ((2,2),(5,5)):
            xx=64+x*64;yy=464+y*64
            d.ellipse((xx+1,yy+1,xx+62,yy+62),outline="#A7E9E6",width=2)
            d.arc((xx+4,yy+4,xx+59,yy+59),196,259,fill="#F7F8DE",width=2)
    framed(out,(16,1000,608,136),"#162F35",2)
    for y in (1038,1090):d.line((34,y,606,y),fill="#315151",width=2)
    # Same teal ENERGY semantic, no numeric Energy 100/100 label.
    framed(out,(244,1014,152,22),"#14252F",1)
    rect(out,(256,1022,384,1028),"#173C49")
    rect(out,(256,1022,317,1028),"#299CB1")
    framed(out,(258,1053,124,57),"#284C50",1)
    label(out,(320,1071),"ABILITY",24,center=True)
    for cx,name,count in ((139,"Potion","2"),(501,"Bomb","3")):
        rect(out,(cx-32,1051,cx+31,1114),"#697E6C","#B7B59A",2)
        icon=scale(load(REPO/"Assets/_Game/Resources/UI/Consumables"/(name+".png")),2)
        paste(out,icon,(cx-24,1059))
        label(out,(cx+24,1097),count,20,color="#FFFFFF")
    return out.convert("RGB")
dry=screen(False);wet=screen(True)
# Explicit NOT UNITY labels are baked into separate headers, so the screen is not passed as a capture.
for mode,im in (("Dry",dry),("Flooded",wet)):
    framed_image=Image.new("RGB",(640,1192),"#102329")
    label(framed_image,(16,8),"NOT UNITY / "+mode.upper()+" MATERIAL & LAYOUT STUDY",20)
    framed_image.paste(im,(0,40));framed_image.save(ROOT/"Review"/("Screen_"+mode+".png"))
pair=Image.new("RGB",(1300,1240),"#102329")
label(pair,(16,8),"THE DROWNED COURT / DRY + FLOODED / NOT UNITY",24)
pair.paste(load(ROOT/"Review/Screen_Dry.png").convert("RGB"),(0,40))
pair.paste(load(ROOT/"Review/Screen_Flooded.png").convert("RGB"),(660,40))
pair.save(ROOT/"Review"/"Dry_Flooded_Proof.png")
# Material swatch sheet.
mat=Image.new("RGB",(960,560),"#606060")
label(mat,(24,14),"MATERIAL CANDIDATES / NATIVE PIXELS AT INTEGER ENLARGEMENT",24)
paste(mat,scale(frame,4),(24,68));label(mat,(24,338),"FRAME / 64X64 AT 4X",20)
paste(mat,scale(cell,4),(326,68));label(mat,(326,218),"CELL / 32X32 AT 4X",20)
for yy in range(3):
    for xx in range(3):paste(mat,scale(cell,2),(502+xx*64,68+yy*64))
label(mat,(502,278),"3X3 TILED / OPAQUE GROUT",20)
paste(mat,scale(cropped,2),(480,318))
mat.save(ROOT/"Review"/"Material_Proof.png")
checks=dict(generation_requests=7,quoted_units=39,balance_before=1581,balance_after=1542,actual_batch_units=39,initial_ceiling=40,reserve_remaining=20,total_unused=21,credits_spent=0,
character_native_sizes="4 x 64x64",character_alpha="binary; original masks byte-identical",character_palette_counts=[metrics[n]["visible_rgb_colors"] for n in names],
environment_raw_size=list(env.size),environment_proof_native_crop=list(envbox),proof_kind="NOT UNITY, local composition; no gameplay implementation",
proof_screen_pixel_size=[640,1152],board_origin=[64,464],board_pitch=64,board_size=[512,512],board_fixed_between_modes=True,board_cells_fully_opaque=True,
gem_native_size=[64,64],gem_resampling=False,character_proof_scale=2,cell_proof_scale=2,environment_proof_scale=2,player_source_native_crop=[0,0,64,64],font_rendering="Thaleah, raster text only; font file excluded from delivery",
ui_caveats=["AIR/TIDE placement is a layout proposal, not runtime validated.","Water wash and oxygen rings are local proof overlays, not production VFX.","Marine frame is one generated corner assembled by native crops/rotation/repetition.","General and inset backing patterns are local material-study placeholders, not a complete production asset kit.","Original environment has pale padding; proof removes only the outer padding by native crop."])
(ROOT/"Manifests"/"ReviewChecks.json").write_text(json.dumps(checks,indent=2),encoding="utf8")
print(json.dumps(dict(candidates=len(metrics),character_colors=checks["character_palette_counts"],environment_crop=envbox,board=[64,464,512,512],proofs=["Screen_Dry.png","Screen_Flooded.png","Native_Comparison_1x.png","Native_Comparison_3x.png"]),indent=2))

