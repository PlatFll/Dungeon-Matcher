from pathlib import Path
from PIL import Image,ImageDraw,ImageFont
import csv,json,hashlib,re,shutil
F=Path(r'C:/Users/USER/.codex/worktrees/drowned-court-pixellab-v2/Dungeon Matcher');R=F/'ArtSource/DrownedCourtEnvironmentV2';P=R/'Preview/Stage04';W=Path(__file__).parent
m=json.loads((R/'Native/Stage04/Manifest.json').read_text());q=json.loads((R/'QA/Stage04/Verification.json').read_text());assert q['status']=='PASS'
font=ImageFont.truetype('C:/Windows/Fonts/consola.ttf',18)
for mode in ['Actors','Hud']:
 before=Image.open(P/('Unity-Before-'+('Hud-' if mode=='Hud' else '')+'720x1280.png')).convert('RGBA')
 after=Image.open(P/f'Unity-{mode}-720x1280.png').convert('RGBA');after.crop((0,0,720,484)).save(P/f'Unity-Upper-{mode}.png')
 sheet=Image.new('RGBA',(1464,530),(9,20,29,255));d=ImageDraw.Draw(sheet)
 d.text((6,7),'Before: accepted Stage 03',font=font,fill='white');d.text((744,7),'Stage 04: modular portrait and clipped recess',font=font,fill='white')
 sheet.alpha_composite(before.crop((0,0,720,484)),(0,36));sheet.alpha_composite(after.crop((0,0,720,484)),(744,36));sheet.save(P/f'Before-After-{mode}.png')
 after.crop((10,45,226,457)).save(P/f'Portrait-Unity-{mode}.png')
 after.crop((10,45,226,457)).resize((432,824),Image.Resampling.NEAREST).save(P/f'Portrait-Unity-{mode}-2x.png')
lineage_path=R/'RawPixelLab/Stage04/Lineage.json';lineage=json.loads(lineage_path.read_text())
lineage.update(balance_after=590.2,cumulative_generations=52,status='CANDIDATE_PENDING_USER_REVIEW',cleanup='LibreSprite native pixel edits; no scaling or interpolation. Exact socket/barnacle source crops, native repeat profiles, four independently lit corner orientations, quiet seamless recess edges.',raw_sha256={n:hashlib.sha256((R/f'RawPixelLab/Stage04/{n}.png').read_bytes()).hexdigest() for n in ['CornerMotif','RecessSeed']})
lineage_path.write_text(json.dumps(lineage,indent=2)+'\n')
statepath=R/'Progress/PIPELINE_STATE.json';state=json.loads(statepath.read_text());state.update(current_stage='04',active_stage='04',status='COMPLETE_PENDING_USER_REVIEW',total_pixellab_spending=52,generation_spend=52);state['stages']['04']='COMPLETE_PENDING_USER_REVIEW';statepath.write_text(json.dumps(state,indent=2)+'\n')
def loadcsv(p):
 with p.open(newline='',encoding='utf-8') as f:r=csv.DictReader(f);return r.fieldnames,list(r)
def writecsv(p,fields,rows):
 with p.open('w',newline='',encoding='utf-8') as f:w=csv.DictWriter(f,fields);w.writeheader();w.writerows(rows)
cat=R/'Progress/ASSET_CATALOG.csv';fields,rows=loadcsv(cat);rows=[r for r in rows if r['stage']!='04']
for r in rows:
 if r['stage']=='03':r.update(style_status='USER_ACCEPTED',user_approval='Continue to Stage04 / 2026-10-25')
for a in m['assets']:
 n=a['name'];raw='RecessSeed' if n.startswith('Recess') else 'CornerMotif' if n not in ['AlgaeLeft','AlgaeRight','CoralSprig'] else ''
 rows.append(dict(asset_id='DC-PORTRAIT-'+n.upper(),stage='04',category='Portrait',description=n+' modular portrait piece',source_method='LibreSprite native authoring' if not raw else 'PixelLab + LibreSprite',native_width=a['width'],native_height=a['height'],palette_count=a['colors'],alpha='opaque' if a['alpha']==[255] else 'binary',anchor='full native canvas; existing shared frame fitter',raw_source=f'RawPixelLab/Stage04/{raw}.png' if raw else '',native_source=f'Native/Stage04/{n}.ase',export_png=f'Exports/Stage04/{n}.png',unity_asset=f'Assets/_Game/Art/DrownedCourtEnvironmentV2/Stage04/{n}.png',style_status='CANDIDATE',tech_status='PASS',user_approval='PENDING',comments='Stage04 art-only scene. Shared 195x406 portrait preserved; no production binding. Source study CornerMotif and optional RecessB retained.'))
writecsv(cat,fields,rows)
ledger=R/'Progress/PIXELLAB_JOB_LEDGER.csv';fields,rows=loadcsv(ledger);rows=[r for r in rows if r['stage']!='04']
for n in ['CornerMotif','RecessSeed']:
 final='RecessA' if n=='RecessSeed' else n
 rows.append(dict(stage='04',date_local='2026-10-25',category=n,provider_tool_or_endpoint='PixelLab create_image_pro_flash',job_id=lineage['results'][n]['job_id'],prompt_source_file='RawPixelLab/Stage04/Lineage.json',reference_input_sha256=lineage['reference_hashes']['Docs/ArtDirection/Audit2026-10/media/skeleton-still.png'],requested_width=64,requested_height=64,quoted_cost=5,actual_billed_equivalents=5,result_path=f'RawPixelLab/Stage04/{n}.png',raw_sha256=lineage['raw_sha256'][n],selected='CANDIDATE_AFTER_LIBRESPRITE',why_selected_or_rejected='Controlled metal/stone motif and dark quiet masonry. Native module and seamless-edge edits.',native_file=f'Native/Stage04/{final}.ase',unity_import_path=f'Assets/_Game/Art/DrownedCourtEnvironmentV2/Stage04/{final}.png',notes='600.20 -> 590.20 remaining; 10 stage / 52 cumulative. No later-stage generation.'))
writecsv(ledger,fields,rows)
report='''# Stage 04 — underwater portrait frame and recess

**Candidate complete; awaiting user review. Stage 05 has not started. No production integration or merge.**

## Result and preserved geometry

Sixteen editable native assets: four 15×15 stone corners, four 7-pixel-thick repeatable rails, a 9×9 dull silver socket, two 64×64 recess tiles, barnacles, two sparse algae stems, a coral sprig and the cleaned cornerstone material study. Fourteen distinct pieces are used in the scene; RecessB and CornerMotif remain alternate/study assets.

The shared portrait outer rectangle remains **195×406** at (21,825), and the board stays **545.099×545.099**. The existing ResponsiveModularFrameFitter is reused without modification. Corners remain 15×15 native pixels while rails repeat; no mirrored lighting or stretched corner artwork. Its existing rounding maps the decorative native width 97.5 to 98. This one-reference-pixel decorative overhang does not change the portrait's layout rectangle or touch/UI bounds.

The dark fill overflows its inset mask by 128 reference pixels on every side. The mask overlaps under the opaque rails, eliminating fill gaps without tinting the battle environment. Marine accents attach outside the portrait, with no plant over Rattlebones's face. No banner was added.

## Generation and native work

Two original PixelLab Pro Flash 64×64 sources, five generations each. **10 generations spent; 600.20 → 590.20 remaining; 52 cumulative**. Actual verified Rattlebones PNG supplied as the rendering reference; anatomy and palette not copied. Raw files, complete prompts, job receipts and hashes are preserved in RawPixelLab/Stage04/Lineage.json.

LibreSprite authored the final .ase files and exact PNG exports. Strong cyan bevels were subdued, isolated recess noise removed, repeat boundaries repaired, and the original metal socket/barnacle pixels reused in separately authored native pieces. Whole source silhouettes were retained. No resampling, blur, flattening of the scene, or production replacement. Recess tiles use four opaque colors; the complete material family uses 21 meaningful opaque colors including metal, coral and algae.

## Executed verification

- Tools/Validate-Unity.ps1 **passed** with Unity 6000.3.19f1. Validator log preserved in QA/Stage04/UnityValidation.log.
- Preview builder passed import pixel/alpha equality, shared geometry, layer-order and native frame checks. Frame tested at three sizes without corner scaling; rail/socket centerlines stay aligned.
- All **16 LibreSprite sources** reopened and exported with exact RGBA equality. Unity PNGs byte-identical to exports; imported opaque pixels and alpha exact. Point, 64 PPU, FullRect, uncompressed, no mipmaps or NPOT resampling.
- **20 edge pairings** passed for recess repeats, rails and corner exits. Independent rendered corner comparison matches source pixels at 2× exactly.
- Magenta overflow probe: **54,656 changed pixels, zero outside the inset mask**. No recess leak into the environment.
- Six review sizes: 360×640, 360×800, 720×1280, 720×1600, 1080×1920, 1080×2400. Native environment/frame pixels use uniform 1×, 2× or 3× mapping. Normal/tall upper renders match.
- Before capture exactly equals accepted Stage03. All pixels outside the portrait/accent bounds are unchanged. **280 accepted Stage01–03 files and 7,839 protected production/settings files** remain byte-identical, along with five current authority/reference files.
- Only three old serialized scene objects differ in the new scene: canvas child list, portrait-mask child list, and hiding its previous flat recess. New scene has no BoardController and is excluded from build settings.
- An explicit Unity play-mode snapshot test loaded current Game with a disposable profile and passed (1/1). It captured the nine existing player/affinity/HP graphics at their measured positions and source sizes. These are replayed read-only in the HUD inspection mode. No production scene/save was written. Shield was not visible in that starting profile.

## Scope of the screenshots

Actual Unity **art-only camera renders**, not an integrated playable zone. The default contrast mode retains the same actor overlays as accepted Stage03. The separate HUD mode shows Rattlebones and the player HUD at the captured production 720×1280 sizes (Rattlebones 64×64); its preview camera proportionally scales that snapshot at other sizes. This does not certify the existing production actor/UI pixel mapping at every device size. No live combat, AIR/countdown behavior, safe-area input, pause/flooding/resume or Android/device test was performed. Those remain integration checks. No ambient effects were introduced.

## Artistic judgment — subjective

**8/10 for this portrait stage.** The cool architectural rails and dull sockets read as the Court counterpart of the established shared frame family. The quiet broad masonry planes support the ivory skull, red cape and bright affinity gem. Sparse attached growth provides underwater identity without obstructing the player. The mockup's fuller surrounding coral and lower scenery belong to later stages; the scene is intentionally unfinished outside this stage. Candidate status is retained for the user's visual decision.

Review: Preview/Stage04/Review.html. Manifest: Native/Stage04/Manifest.json. Exact checks: QA/Stage04/Verification.json. Builder assertions: QA/Stage04/UnityPreview.json. Disposable HUD test: QA/Stage04/HudSnapshotTests.xml.
'''
(R/'Stage04Report.md').write_text(report,encoding='utf-8')
table=''.join(f'<tr><td>{a["name"]}</td><td>{a["width"]} × {a["height"]}</td><td>{a["colors"]}</td><td><a href="../../Exports/Stage04/{a["name"]}.png">PNG</a> · <a href="../../Native/Stage04/{a["name"]}.ase">LibreSprite</a></td></tr>' for a in m['assets'])
html='''<!doctype html><html lang="en"><meta charset="utf-8"><meta name="viewport" content="width=device-width"><title>Drowned Court · Stage 04 portrait</title>
<style>body{background:#09141d;color:#e0eaed;font:16px system-ui;margin:28px;line-height:1.55}h1,h2{color:#a6dbd8}p{max-width:1100px}a{color:#91dbd1}section{background:#12232d;border:1px solid #2e4b56;border-radius:8px;padding:18px;margin:24px 0}.scroll{overflow:auto}.pair{display:flex;align-items:start;gap:24px}.pair>div{flex-shrink:0}img{image-rendering:pixelated;display:block;max-width:none}button,select{font:inherit;background:#213e4b;border:1px solid #517781;color:white;padding:8px;margin:0 8px 12px 0;cursor:pointer}td,th{text-align:left;padding:6px 18px;border-bottom:1px solid #36505c}table{border-collapse:collapse}.badge{color:#c1dea7}small{color:#a4bcc6}</style>
<h1>Drowned Court · Stage 04</h1><p class="badge">Modular portrait frame and clipped masonry recess · candidate for review</p><p>Teal carved stone, centered dull-silver sockets and restrained marine growth. Approved backgrounds, architecture, platform and shared UI are unchanged. <b>10 PixelLab generations spent · 590.20 remain.</b> No merge or production integration.</p>
<section><h2>Before / after — actual Unity renders</h2><select id="mode"><option value="Actors">Same Stage03 character contrast overlays</option><option value="Hud">Measured production player HUD snapshot</option></select><div class="scroll"><img id="pair" src="Before-After-Actors.png" width="1464" height="530"></div><p>Existing portrait geometry is retained. The frame uses four native corners and tiled rails; the inset fill deliberately overflows beneath its mask.</p><p><small>Art-only preview. HUD mode preserves actual production player/affinity/HP graphic sizes and positions at 720×1280. It is a static snapshot, not live combat. The board frame and surrounding scenery are later stages.</small></p></section>
<section><h2>Full Unity view</h2><select id="resolution"><option>720x1280</option><option>360x640</option><option>1080x1920</option><option>720x1600</option><option>360x800</option><option>1080x2400</option></select><div class="scroll"><img id="capture" src="Unity-Actors-720x1280.png"></div><p><small>Only new frame/environment pixels are asserted at uniform 1×, 2× and 3×. Production safe-area behavior and actor/UI mapping remain integration-stage checks.</small></p></section>
<section><h2>Native artwork and repeat study</h2><button onclick="zoom(1)">Native 1×</button><button onclick="zoom(2)">Nearest-neighbor 2×</button><button onclick="zoom(4)">Nearest-neighbor 4×</button><div class="scroll pair native"><div><img data-w="112" data-h="215" src="Portrait-Native.png"></div><div><img data-w="480" data-h="410" src="Native-Contact.png"><br><img data-w="256" data-h="128" src="Recess-Joins-Native.png"></div></div><p>16 native files, including the material study and alternate recess. Art remains modular in Unity. Recess tiles use four colors; all materials together use 21 opaque colors.</p></section>
<section><h2>Raw PixelLab sources / LibreSprite cleanup</h2><div class="scroll"><img src="Source-Comparison-3x.png"></div><p>Original sources are preserved. The socket and barnacles retain their native source pixels. Rails, aligned corners, marine stems and repeat boundaries were authored in LibreSprite.</p></section>
<section><h2>Reference comparison</h2><div class="scroll pair"><div><p>Pack portrait reference — composition guide</p><img src="../../Brief/ProductionPack/reference_crops/02_Left_Portrait_Reference.png"></div><div><p>Unity portrait · production HUD snapshot · 2× crop</p><img src="Portrait-Unity-Hud-2x.png"></div></div><p>The reference informs cool stone, dull hardware and exterior growth. The game's existing player scale and layout remain authoritative.</p></section>
<section><h2>Overflow mask proof</h2><div class="scroll"><img src="Unity-Mask-Probe-720x1280.png"></div><p>The fill extends 128 reference pixels beyond each mask edge. All 54,656 magenta pixels remain inside the inset; zero leaks. The corners stay square across three resize tests.</p></section>
<section><h2>Native files</h2><table><tr><th>Asset</th><th>Native size</th><th>Opaque colors</th><th>Files</th></tr>TABLE</table></section>
<section><h2>Verification and scope</h2><p>Unity validator passed; 16 exact LibreSprite roundtrips, 20 exact edge pairings and import equivalence passed. 280 accepted stage files and 7,839 protected production/settings files remain unchanged. The before capture equals Stage03; pixels outside the portrait remain identical.</p><p><b>Artistic judgment: 8/10</b> for this portrait stage, pending your review. No later stages have begun.</p><p><a href="../../Stage04Report.md">Full report and limitations</a> · <a href="../../QA/Stage04/Verification.json">Pixel checks</a> · <a href="../../QA/Stage04/UnityPreview.json">Unity checks</a> · <a href="../../RawPixelLab/Stage04/Lineage.json">Generation lineage</a></p></section>
<script>const mode=document.getElementById('mode'),res=document.getElementById('resolution');function update(){document.getElementById('pair').src='Before-After-'+mode.value+'.png';document.getElementById('capture').src='Unity-'+mode.value+'-'+res.value+'.png'}mode.onchange=res.onchange=update;function zoom(n){document.querySelectorAll('.native img').forEach(i=>{i.width=+i.dataset.w*n;i.height=+i.dataset.h*n})}</script></html>'''.replace('TABLE',table)
(P/'Review.html').write_text(html,encoding='utf-8')
for relative in re.findall(r'(?:src|href)="([^"#]+)"',html):assert (P/relative).resolve().exists(),relative
for mode in ['Actors','Hud']:
 for res in ['360x640','720x1280','1080x1920','360x800','720x1600','1080x2400']:assert (P/f'Unity-{mode}-{res}.png').exists()
shutil.copyfile(W/'BuildReview04.py',R/'Scripts/Stage04/BuildReview04.py');shutil.copyfile(W/'Handoff04.py',R/'Scripts/Stage04/Handoff04.py')
(R/'QA/Stage04/HandoffChecks.json').write_text(json.dumps(dict(status='PASS',gallery_links='all existing',resolutions=6,mode_variants=2,stage_spend=10,balance_after=590.2,unity_validator='PASS',hud_snapshot_test='1 passed',stage05='NOT_STARTED'),indent=2)+'\n')
print('Stage04 report, gallery, catalog, ledger and state complete. All static gallery links and 12 view combinations exist.')
