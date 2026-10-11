"""Build authored report, offline HTML, reference register and portable review ZIP."""
from pathlib import Path
from PIL import Image, ImageDraw, ImageFont, ImageChops
from assessments import assessments
import json, csv, hashlib, shutil, zipfile, re

OUT=Path(__file__).resolve().parent
ROOT=OUT.parents[2]
def sha(p): return hashlib.sha256(p.read_bytes()).hexdigest()
data=json.loads((OUT/'bindings.json').read_text(encoding='utf-8'))
notes=assessments()
assert set(notes)=={a['id'] for a in data['actors']}
enemy_inventory={p.relative_to(ROOT).as_posix() for p in (ROOT/'Assets').rglob('*.asset')
    if re.search(r'^  enemyId: .+',p.read_text(encoding='utf-8-sig'),re.M)}
audited_enemies={a['definition'] for a in data['actors'] if a['zone'] not in ('Players','Ironvein alternates (disabled)')}
assert enemy_inventory==audited_enemies, {'missing':sorted(enemy_inventory-audited_enemies),'extra':sorted(audited_enemies-enemy_inventory)}
for a in data['actors']:
    r=notes[a['id']]
    states=', '.join(s['name'] for s in a['states'])
    r['affected']='Fallback/ready still and all bound states: '+states+'. Review original timing and entry/exit registration after any approved edit.'
    if r['intervention']=='Keep': r['affected']='None required. Any optional polish named above is a separate proposal; preserve the current still and '+states+'.'
    if a['id']=='snapvine':r['affected']='Death first (especially drawings 2-6); keep the still, Idle, Hit and AutoAttack. Check sprite rectangles and source sheet before editing.'
    if a['id']=='puffer_sentinel':r['affected']='InflatedIdle, InflatedAttack, ChannelStart, ChannelHold, Ability, Release and Interrupt. Keep the normal still; verify normal/inflated entry and exit against Idle, AutoAttack, Hit and Death.'
    a['review']=r

zones=list(dict.fromkeys(a['zone'] for a in data['actors']))
data['boards']=[(z,'boards/'+re.sub('[^a-z0-9]+','-',z.lower()).strip('-')+'.png') for z in zones]
data['screens']=[]
(OUT/'screens').mkdir(exist_ok=True)
for slug in ('dungeon','magical-forest','drowned-court','ironvein-excavation'):
    src=ROOT/'Docs/Validation/UnifiedCombat/Visual'/f'{slug}-1080x1920.png'
    dest=OUT/'screens'/src.name;shutil.copy2(src,dest)
    data['screens'].append(dict(label=slug,path='screens/'+dest.name,source=src.relative_to(ROOT).as_posix(),sha256=sha(src),status='Historical committed validation capture; not a new runtime test'))

refs=[
('skeleton','Primary golden standard — user instruction 2026-10-11','Crisp contours, broad connected shading, clear focal face/prop and grounded compact motion.','Do not copy skeleton anatomy, cyan eyes, royal colors or exact stance to other species.'),
('farmer','Existing supporting reference; retained','Warm human skin, quiet workwear, straw and an identifying improvised tool.','Do not copy the same hat/face/pose to all locals or force all cloth brown.'),
('pan_villager','Existing supporting reference; retained','Simple human expression, restrained cloth and dull iron with one large prop mass.','The pan remains cookware, not a heraldic shield; do not normalize her face/gender presentation.'),
('royal_swordsman','PROPOSED addition — membership pending user review','Large clean armor planes, dark visor and disciplined red/gold accents.','Not universal white armor, closed faces, or a universal melee pose.'),
('elven_mender','PROPOSED addition — membership pending user review','Quiet fabric, exposed readable face, simple support equipment and graceful open-hand posture.','Not mandatory elf anatomy, cream/rose costume or identical casting hands.'),
('orc_trailguard','PROPOSED addition — membership pending user review','Broad nonhuman anatomy with clear skin/leather/iron separation and readable tool acting.','Not mandatory olive skin, muscular width or axe equipment.'),
('pearl_thief','PROPOSED addition — membership pending user review; broader art approval remains separate','Economical whimsical marine anatomy, large quiet head, tiny face and one bright treasure accent.','Not a purple palette, tentacles or a generic cute-face template for every creature.'),
('elven_scout','Specialist reserve — not proposed for core membership','Bowstring/hand relationship and readable lean archer silhouette.','Do not copy its wider idle movement to the whole cast; Mender already covers the core elf/cloth role.'),
]
font=ImageFont.truetype('C:/Windows/Fonts/arial.ttf',17)
small=ImageFont.truetype('C:/Windows/Fonts/arial.ttf',12)
board=Image.new('RGB',(1120,750),'#222636');d=ImageDraw.Draw(board)
d.text((16,12),'REFERENCE ROLES | actual imported ready frames at 3x, nearest-neighbor',font=font,fill='white')
d.text((16,38),'Full canvases, top-left aligned. Additions are proposals; existing art approval and golden membership are separate.',font=small,fill='#cbd0dd')
reg=['# Dungeon Matcher — current golden-reference register','',
'2026-10-11 · canonical guide v1.12. This register identifies images and roles; the canonical art guide owns direction. No production art was changed.',
'', 'Rattlebones is explicitly primary by the current user instruction. Farmer and Pan Villager retain their existing supporting status. Four additional memberships below are proposals for review. Approval of an existing artwork, implementation or merge is not approval of new golden-reference membership.',
'','## Exact current images','',
'Each convenience PNG is a lossless extraction of the currently bound fallback/ready sprite, using its Unity GUID/fileID and imported rectangle. It is not a regenerated interpretation or a new approved replacement. Current motion sources are the linked controller and its complete clip/frame ledger in `Audit2026-10/bindings.json`. Recheck hashes before a future art task; if production/reference authority changes, update this register rather than relying on a dated gallery.',
'','[Labeled comparison board](Audit2026-10/boards/golden-comparison.png) · [Offline gallery](Audit2026-10/Review.html)','']
refdata=[]
byid={a['id']:a for a in data['actors']}
for i,(aid,status,role,avoid) in enumerate(refs):
    a=byid[aid];sd=a['still_binding'];p=OUT/a['still']
    im=Image.open(p).convert('RGBA');x=(i%4)*280;y=80+(i//4)*330
    d.text((x+12,y),a['name'],font=font,fill='white')
    label='PRIMARY' if i==0 else 'EXISTING SUPPORT' if i<3 else 'SPECIALIST RESERVE' if aid=='elven_scout' else 'PROPOSED ADDITION'
    d.text((x+12,y+25),label,font=small,fill='#f1cb7e')
    large=im.resize((im.width*3,im.height*3),Image.Resampling.NEAREST);board.paste(large,(x+12,y+48),large)
    d.text((x+12,y+295),f'{im.width}x{im.height} source; shown at 3x',font=small,fill='#cbd0dd')
    rec=dict(id=aid,status=status,role=role,do_not_copy=avoid,extracted_png='Audit2026-10/'+a['still'],extracted_sha256=sha(p),source=sd,controller=a['controller'])
    refdata.append(rec)
    reg += [f'### {a["name"]}', '', f'**Status:** {status}.', '', f'**Reference for:** {role}', '', f'**Do not copy:** {avoid}', '',
       f'- Current native image: [{p.name}](Audit2026-10/{a["still"]}) ({im.width}×{im.height}).',
       f'- Extracted PNG SHA-256: `{sha(p)}`.',
       f'- Production source: `{sd["path"]}`; SHA-256 `{sd["sha256"]}`.',
       f'- Unity GUID `{sd["guid"]}`, fileID `{sd["fileID"]}`, sprite `{sd["name"]}`.',
       f'- Extraction rectangle in PNG top-left coordinates: `{sd["crop_top_left"]}` (right/bottom exclusive). No scaling, trimming or recentering.',
       f'- Current controller: `{a["controller"]}`.', '']
reg += ['## Approval provenance and historical references','',
'- Farmer/Pan/Rattlebones historical working statuses, exact original hashes and later hand-adjusted animation decisions remain in sections 5, 11 and 14 of the canonical guide. The current reference authority uses the actual bound production versions shown here.',
'- Mender/Scout/Trailguard still identity approval is recorded in `Docs/Forest/ART_REVIEW.md` and the Forest approved manifests. It does not automatically approve all later animations or these proposed reference memberships.',
'- Royal Swordsman belongs to the accepted grounded cast family documented in `ArtSource/RemainingCast/SelectedIdles/README.md`; this audit proposes a specific additional reference role.',
'- Court and Ironvein source-selection/approval records retain their original distinctions. Do not infer approval of all Court art or proposed Pearl Thief membership from a merged implementation.',
'- Historical `references/characters/*.png` entries in the original guide are not present in this checkout. The historical composite exists and was inspected; its original provenance is retained. This register does not fabricate or silently recreate the missing source-pack files.',
'- Bardley and Gideon keep their existing approved/working identity authority. They are not replaced by the enemy shortlist. Bardley historical static placement notes must not be applied blindly to the current grounded animation.',
'','## Future generation lineage','',
'Follow the canonical guide’s actual-image input requirements. Use the referenced PNG bytes in a supported style/reference field; paths or textual mentions alone are insufficient. Record guide version/hash, image hashes, input roles, target approval, endpoint/settings/job lineage and output hashes. The approved target remains identity authority during animation. Proposed memberships above must remain marked pending until the user decides.','']
(OUT.parent/'Golden_Reference_Register.md').write_text('\n'.join(reg),encoding='utf-8')
board.save(OUT/'boards/golden-comparison.png')
(OUT/'golden-references.json').write_text(json.dumps(refdata,indent=2,ensure_ascii=False),encoding='utf-8')

report=(OUT/'OVERVIEW.md').read_text(encoding='utf-8')
for zone in zones:
    report+='\n\n### '+zone+'\n\n| Character | Style | Theme | Animation style | Priority | Intervention |\n|---|---:|---:|---:|---|---|\n'
    for a in (a for a in data['actors'] if a['zone']==zone):
        r=a['review'];report+=f'| [{a["name"]}](Review.html#{a["id"]}) | {r["style"]:g} | {r["theme"]:g} | {r["animation"]:g} | {r["priority"]} | {r["intervention"]} |\n'
    for a in (a for a in data['actors'] if a['zone']==zone):
        r=a['review'];report+=f'\n#### {a["name"]}\n\n**Style {r["style"]:g}/10 · Theme {r["theme"]:g}/10 · Animation style {r["animation"]:g}/10 · {r["priority"]}**\n\n**{r["intervention"]}.** {r["evidence"]}\n\n'
        for label,key in [('Preserve','preserve'),('Stance decision','stance'),('Animation evidence/correction','motion'),('Affected states','affected'),('Golden-reference recommendation','golden')]: report+=f'**{label}:** {r[key]}\n\n'
        report+=f'**Inspected:** fallback/ready plus '+', '.join(s['name'] for s in a['states'])+'.\n\n'
        report+=f'**Binding:** `{a["definition"]}` → `{a["controller"]}`. [All bound drawings]({a["contact_board"]}) · [Native ready image]({a["still"]}). Exact clip/sprite paths and hashes: `bindings.json`.\n'
(OUT/'REPORT.md').write_text(report,encoding='utf-8')
with (OUT/'scores.csv').open('w',newline='',encoding='utf-8-sig') as f:
    w=csv.writer(f);w.writerow(['id','name','zone','style','theme','animation_style','priority','intervention','evidence','preserve','stance','motion','affected_states','golden_recommendation'])
    for a in data['actors']:
        r=a['review'];w.writerow([a['id'],a['name'],a['zone']]+[r[k] for k in ['style','theme','animation','priority','intervention','evidence','preserve','stance','motion','affected','golden']])
template=(OUT/'gallery_template.html').read_text(encoding='utf-8')
(OUT/'Review.html').write_text(template.replace('__DATA__',json.dumps(data,ensure_ascii=False).replace('</','<\\/')),encoding='utf-8')

# Verify all atlas cells are lossless copies of the exact referenced imported rectangles.
images={};checked=0
for a in data['actors']:
    refs_to_check=[(Image.open(OUT/a['still']).convert('RGBA'),a['still_binding'])]
    for s in a['states']:
        atlas=Image.open(OUT/s['media']).convert('RGBA');w,h=s['size']
        assert atlas.size==(w*len(s['frames']),h)
        assert abs(sum(f['duration'] for f in s['frames'])-s['duration'])<0.0001
        for i,f in enumerate(s['frames']):
            sd=f['sprite'];fw,fh=sd['size'];refs_to_check.append((atlas.crop((i*w,0,i*w+fw,fh)),sd))
    for im,sd in refs_to_check:
        p=ROOT/sd['path']
        if sd['path'] not in images:images[sd['path']]=Image.open(p).convert('RGBA')
        original=images[sd['path']].crop(sd['crop_top_left'])
        assert im.size==original.size and im.tobytes()==original.tobytes(),sd['path']
        checked+=1
for path,expected in data['source_hashes'].items(): assert sha(ROOT/path)==expected,path
html=(OUT/'Review.html').read_text(encoding='utf-8')
assert '__DATA__' not in html
for a in data['actors']:
    assert (OUT/a['still']).is_file() and (OUT/a['contact_board']).is_file()
    for s in a['states']:assert (OUT/s['media']).is_file()
verification=dict(source_baseline=data['revision'],actor_count=len(data['actors']),enemy_count=len(enemy_inventory),state_count=sum(len(a['states']) for a in data['actors']),
    drawing_entries=sum(len(s['frames']) for a in data['actors'] for s in a['states']),lossless_cells_checked=checked,
    unchanged_source_hashes_checked=len(data['source_hashes']),guide_version='1.12',guide_sha256=sha(OUT.parent/'Dungeon_Matcher_Art_Direction.txt'),
    all_assessments_present=True,all_media_present=True,positive_durations_and_exposure_sums=True,
    visual_scope='All exported actor stills and all bound drawing contact sheets visually inspected. Serialized timing inspected. Four historical zone captures inspected.',
    runtime_validation='Not run; documentation/inspection artifact pass. No fresh Unity/device playback; no claim of watching every state at speed.',pixellab_generations=0)
(OUT/'verification.json').write_text(json.dumps(verification,indent=2),encoding='utf-8')
zipout=ROOT/'.utmp/ArtAudit/DungeonMatcher-ArtAudit-2026-10-11.zip';zipout.parent.mkdir(parents=True,exist_ok=True)
with zipfile.ZipFile(zipout,'w',zipfile.ZIP_DEFLATED,compresslevel=9) as z:
    z.writestr('START_HERE.html','<!doctype html><meta charset="utf-8"><meta http-equiv="refresh" content="0;url=Docs/ArtDirection/Audit2026-10/Review.html"><a href="Docs/ArtDirection/Audit2026-10/Review.html">Open the offline art review</a>')
    for p in sorted(OUT.rglob('*')):
        if p.is_file() and '__pycache__' not in p.parts:z.write(p,p.relative_to(ROOT).as_posix())
    for p in [OUT.parent/'Dungeon_Matcher_Art_Direction.txt',OUT.parent/'Golden_Reference_Register.md',ROOT/'AGENTS.md']:
        z.write(p,p.relative_to(ROOT).as_posix())
with zipfile.ZipFile(zipout) as z:assert z.testzip() is None
print(json.dumps(dict(**verification,zip=str(zipout),zip_bytes=zipout.stat().st_size),indent=2))
