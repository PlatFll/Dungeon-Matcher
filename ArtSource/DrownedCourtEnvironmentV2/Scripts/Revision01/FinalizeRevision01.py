from pathlib import Path
from PIL import Image,ImageDraw,ImageFont
import json,hashlib,shutil,sys,csv
F=Path(r'C:/Users/USER/.codex/worktrees/drowned-court-pixellab-v2/Dungeon Matcher');R=F/'ArtSource/DrownedCourtEnvironmentV2';P=R/'Preview/Revision01';Q=R/'QA/Revision01';W=Path(__file__).parent
qa=json.loads((Q/'Verification.json').read_text());assert qa['status']=='PASS'
shutil.copyfile(sys.argv[1],Q/'UnityValidation.log')
data=json.loads((R/'Native/Revision01/Manifest.json').read_text());sha=lambda p:hashlib.sha256(p.read_bytes()).hexdigest()
ref=Path(r'C:/Users/USER/AppData/Local/Temp/codex-clipboard-8a5a81f7-8dff-48a7-b0b2-0b85c872ed5a.png');shutil.copyfile(ref,P/'ReferenceMockup.png')
lineage=dict(status='REVIEW_CANDIDATE',scope='Background palette only; redesign left/right vertical board surroundings only. Stage09 not started.',generation_cost=0,prior_total_cost=89,tool='LibreSprite native JavaScript pixel editing and .ase exports',mockup=dict(path=ref.as_posix(),sha256=sha(ref)),edit_target='Assets/_Game/Scenes/ArtPreview_DrownedCourtV2_Stage08.unity',coral_source=dict(path='Exports/Stage06/CoralColony.png',sha256=sha(R/'Exports/Stage06/CoralColony.png'),method='Native branching silhouette re-used with per-material colors, original detached rock foot replaced by an integral stone socket. No scaling.'),recolors='Native/Revision01/RecolorPlan.json',authority_hashes=json.loads((Q/'AuthorityHashes.json').read_text()),original_sources='Preserved byte-for-byte; see QA/Revision01/AcceptedStagesHashes.json')
(R/'Native/Revision01/Lineage.json').write_text(json.dumps(lineage,indent=2)+'\n')
report=f'''# Focused revision 01 — background palette and integrated board sides

Candidate for user review. No merge, gameplay integration or Stage09 work.

## Scope

The narrow columns and separately placed Stage06/08 plants were visually detached. The revision uses wider fluted stone sections, recessed ledges, kelp wrapping across the column faces, coral growing from dark sockets, and small shell/barnacle contacts. Fourteen separate native modules preserve the current gutters and frame dimensions. The left and right sides vary their colony locations and kelp direction without changing the board.

Thirty-seven additive background exports use exact role-based palette maps: clearer cyan/aqua distant light, deeper blue water shadows, and more saturated teal foreground ruins. The background silhouettes, alpha, dimensions, placement and pixel clusters are unchanged. The player recess and existing lower reefs are recolored, not redesigned. Board/portrait frame artwork, wave/Guide/Settings controls, actors, cells, shared bottom HUD, text and all object transforms are preserved.

## Executed validation

- LibreSprite authoring and all 51 ASE-to-PNG roundtrips passed; binary alpha, exact native import colors, Point/FullRect/64PPU/uncompressed/unmipped settings.
- Unity 6000.3.19f1 produced 30 static captures across 360×640, 360×800, 720×1280, 720×1600, 1080×1920 and 1080×2400.
- All six before captures equal Stage08. Isolated protected-region renders match before/after exactly. {qa['unchanged_UI_actor_frame_tile_pixels']:,} opaque UI/actor/frame/tile samples remain identical in the final scenes.
- Side modules map at exact integer 1×/2×/3×. No source-pixel occlusion, frame overlap, crop, board encroachment or missing edge pixels.
- {qa['exact_recolored_assets']} exact palette-only recolors preserve masks and geometry. {qa['protected_production_files']} protected production files, {qa['accepted_stage_files']} prior-stage files and five authorities remain byte-identical.
- Tools/Validate-Unity.ps1 completed successfully. Full validator log: QA/Revision01/UnityValidation.log.

These are actual art-only Unity camera captures, not gameplay, Android or physical-device tests. The existing tall-preview spacer regions are retained and recolored; changing their dimensions is outside this request.

## Source and cost

No new PixelLab calls or credits. Existing Stage06 PixelLab coral silhouette reused; structural modules and exact recolors authored in LibreSprite. Prior cumulative spending remains 89 generations. Original approved assets and prior scenes remain available, unchanged.

Scene: Assets/_Game/Scenes/ArtPreview_DrownedCourtV2_Revision01.unity. Gallery: Preview/Revision01/Review.html. All final artwork remains pending user approval.
'''
(R/'Revision01Report.md').write_text(report,encoding='utf-8')
font=ImageFont.truetype('C:/Windows/Fonts/consola.ttf',16);board=Image.new('RGB',(1440,1320),'#061f2b');d=ImageDraw.Draw(board)
for i,(n,label) in enumerate([('Unity-Before-720x1280.png','Current scene'),('Unity-Revised-720x1280.png','Focused revision: palette and board sides')]):
 d.text((i*720+12,10),label,font=font,fill='white');board.paste(Image.open(P/n).convert('RGB'),(i*720,40))
board.save(P/'Before-After.png')
html='''<!doctype html><html lang="en"><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>Drowned Court — focused revision</title><style>body{background:#061f2b;color:#e6f3f3;font:16px system-ui;margin:24px}img{image-rendering:pixelated;max-width:none}.row{display:flex;gap:20px;overflow:auto}button{padding:8px;margin:4px;background:#125368;color:white;border:1px solid #3b9baa}a{color:#8ed9de}</style><h1>Drowned Court · focused revision</h1><p>Background recolor and integrated board-side architecture only. Existing scene layout and shared artwork preserved. Art-only Unity review; no merge.</p><div id="sizes"></div><div class="row"><div><p>Before</p><img id="before" src="Unity-Before-360x640.png"></div><div><p>Revised</p><img id="after" src="Unity-Revised-360x640.png"></div></div><p><a href="../../Revision01Report.md">Validation and scope</a> · <a href="../../QA/Revision01/Verification.json">Exact checks</a></p><script>for(const s of ['360x640','360x800','720x1280','720x1600','1080x1920','1080x2400']){let b=document.createElement('button');b.textContent=s;b.onclick=()=>{document.getElementById('before').src='Unity-Before-'+s+'.png';document.getElementById('after').src='Unity-Revised-'+s+'.png'};document.getElementById('sizes').append(b)}</script></html>'''
(P/'Review.html').write_text(html,encoding='utf-8')
statepath=R/'Progress/PIPELINE_STATE.json';state=json.loads(statepath.read_text());state['focused_revision_01']=dict(status='COMPLETE_PENDING_USER_REVIEW',scope='Background recolor and integrated board sides',report='Revision01Report.md',preview='Preview/Revision01/Review.html',generation_spend=0);statepath.write_text(json.dumps(state,indent=2)+'\n')
catalog=R/'Progress/ASSET_CATALOG.csv'
with catalog.open(newline='',encoding='utf-8-sig') as f:reader=csv.DictReader(f);fields=reader.fieldnames;rows=[row for row in reader if row['stage']!='Revision01']
for a in data['assets']:
 n=a['name'];row={k:'' for k in fields};row.update(stage='Revision01',style_status='CANDIDATE',user_approval='PENDING')
 vals=[f'DC-REV01-{n.upper()}','Revision01','BackgroundRecolor' if n.startswith('S0') else 'IntegratedSides',n,'LibreSprite native edit',a['width'],a['height'],len(set(c for c in Image.open(R/f'Exports/Revision01/{n}.png').convert('RGBA').get_flattened_data() if c[3])),'binary','native canvas; unchanged layout','Existing native sources; see lineage',f'Native/Revision01/{n}.ase',f'Exports/Revision01/{n}.png',f'Assets/_Game/Art/DrownedCourtEnvironmentV2/Revision01/{n}.png','CANDIDATE','PASS','PENDING','Focused revision; original assets preserved; zero generation cost.']
 assert len(vals)==len(fields);rows.append(dict(zip(fields,vals)))
with catalog.open('w',newline='',encoding='utf-8') as f:writer=csv.DictWriter(f,fieldnames=fields);writer.writeheader();writer.writerows(rows)
for n in ['FinalizeRevision01.py','RunUnityRevision01.ps1','BuildRevision01.py']:
 if (W/n).resolve()!=(R/'Scripts/Revision01'/n).resolve():shutil.copy2(W/n,R/'Scripts/Revision01'/n)
print('Focused revision report, gallery, lineage and checkpoint complete; no stage advancement.')
