from pathlib import Path
import hashlib,json,shutil,xml.etree.ElementTree as ET
from PIL import Image,ImageChops
F=Path(r'C:/Users/USER/.codex/worktrees/forest-zone-hybrid-rebuild/Dungeon Matcher')
R=F/'ArtSource/ForestReworkV2/FrameFit';W=Path(__file__).parent
result=ET.parse(R/'Diagnostics/Verified.xml').getroot()
assert result.get('result')=='Passed' and result.get('total')=='2',result.attrib
cases=json.loads((R/'Corrected/Geometry.json').read_text())['cases']
assert len(cases)==25 and all(not c['errors'] and c['enemyCount']==3 for c in cases)
a=Image.open(F/'ArtSource/ForestReworkV2/Checkpoint05/Previews/Unity-Full-720x1360.png').convert('RGBA')
b=Image.open(R/'Previews/Unity-Full-720x1360.png').convert('RGBA')
regions={'top':(0,0,720,480),'tiles_and_gems':(104,528,616,1040),'lower_HUD':(0,1104,720,1360)}
for name,box in regions.items():
 assert ImageChops.difference(a.crop(box),b.crop(box)).convert('RGB').getbbox() is None,name
large=Image.open(R/'Previews/Unity-Full-1440x2720.png').convert('RGBA')
assert ImageChops.difference(b.resize(large.size,Image.Resampling.NEAREST),large).convert('RGB').getbbox() is None
protected=json.loads((R/'Diagnostics/ProtectedHashes.json').read_text())
auto={'ProjectSettings/TimeManager.asset','Assets/_Game/Resources/UI/Consumables/SlotDisabled.png.meta','Assets/_Game/Resources/UI/Consumables/SlotHighlighted.png.meta','Assets/_Game/Resources/UI/Consumables/SlotPressed.png.meta'}
changed=[p for p,h in protected.items() if p not in auto and (not (F/p).exists() or hashlib.sha256((F/p).read_bytes()).hexdigest()!=h)]
assert not changed,changed[:20]
baseline=json.loads((R/'Baseline/Geometry.json').read_text())['cases']
rows=[]
for view in dict.fromkeys(c['viewport'] for c in cases):
 v=[c for c in cases if c['viewport']==view];ref=v[0]
 assert all(abs(x['cells']['width']-ref['cells']['width'])<.1 and x['portrait']['width']==ref['portrait']['width'] and x['portrait']['height']==ref['portrait']['height'] for x in v)
 rows.append({'viewport':view,'board_pixels':round(ref['cells']['width'],3),'portrait_pixels':[ref['portrait']['width'],ref['portrait']['height']],'zones':5})
old=ET.parse(R/'Diagnostics/LegacyBaseline.xml').findall('.//test-case');assert len(old)==3 and all(x.get('result')=='Failed' for x in old)
report={'baseline_commit':'0b3f4e7fda0c1baaa3ff5bb84a81a18858f0cb61','unity_tests':2,'live_cases':25,'enemies_per_case':3,'four_edge_gaps':0,'unchanged_preview_regions':list(regions),'exact_2x':True,'protected_files_unchanged':len(protected)-len(auto),'auto_serialized_files_restored':sorted(auto),'geometry':rows,'historical_test_failures_reproduced_on_HEAD':[x.get('fullname') for x in old],'art_generations':0,'production_art_integrated':False,'physical_device_tested':False,'merged':False}
(R/'REPORT.json').write_text(json.dumps(report,indent=2)+'\n')
shutil.copyfile(F/'ArtSource/ForestReworkV2/Checkpoint05/Previews/Unity-Full-720x1360.png',R/'Previews/Before-720x1360.png')
for name,box in regions.items():b.crop(box).save(R/f'Previews/{name}.png')
for name in ['PublishFrameFit.py','RunFrameFit.ps1','ValidateFrameFit.ps1']:
 shutil.copyfile(W/name,R/'Scripts'/name)
table='\n'.join(f"| {v['viewport']} | {v['board_pixels']} x {v['board_pixels']} | {v['portrait_pixels'][0]:g} x {v['portrait_pixels'][1]:g} | 5/5 |" for v in rows)
readme=f'''# Snug Forest frame and shared zone sizing

The 512 x 512 Forest tile opening is unchanged. The old frame used a 24-native-pixel inset although its 23px corner/11px rail geometry needs 17px, leaving 14 screen pixels of empty space per side. `ResponsiveModularFrameFitter.FollowOpening` now derives the target from the actual opening. The trailing foliage moved six native pixels down so it cannot cover a tile. No PNG was edited.

The actual Game scene formerly sized its board and portrait from zone padding, minimum battle heights and skin registrations. One shared layout contract now preserves the Dungeon board sizing across all five zones. Portrait rectangles use the same registration. Legacy player rails repeat native pixels horizontally; skin ornament/rail thickness remains distinctive. This is equal playable opening and portrait slot geometry, not identical leaf/stone silhouettes.

The approved Forest assembly remains art-only. The shared geometry code is exercised in actual Game scenes using each existing production zone skin. New Forest production-art integration is not claimed or authorized here. Main and primary checkout remain untouched; PR #207 stays draft and unmerged.

## Verification

- Repository `Tools/Validate-Unity.ps1` completed successfully with Unity 6000.3.19f1.
- Corrected Forest preview test passes at 720x1360 and 1440x2720; all four rail-to-tile gaps are zero and the second capture is exact nearest-neighbor 2x.
- Tiles/gems, top assembly and entire lower HUD crop are pixel-identical to the approved Checkpoint 5 capture.
- 25 actual Game cases pass the complete presentation validator: five zones at five viewport/safe-area profiles, three enemies each, including each zone's apex where available.
- {len(protected)-len(auto):,} pre-existing protected files retain exact hashes. Four automatic Unity serialization files are restored to Git HEAD. No generations or source art edits.
- At 360x640 only, the legacy HUD controls move nine logical pixels up, narrow HP labels use the existing compact treatment, and the optional instructional caption hides to avoid covering the controls. Normal lower HUD placement/art is unchanged. Board fitting remains uniform fractional on narrow displays, matching the existing approved policy.
- No Android build or physical-device test was performed.

| Viewport | Playable board, screen pixels | Portrait slot, screen pixels | Matching zones |
|---|---|---|---|
{table}

Three older `ForestVisualPlayTests` remain failing and were **not weakened**: the old environment ID assertion expects `forest-woodland` instead of the existing `presentation-forest`; two legacy move-intent assertions require the label bottom above the sprite, while the existing presenter centers it at the sprite top (16 screen pixels of overlap). All three failures were reproduced on unchanged HEAD with this fix temporarily removed. `Diagnostics/LegacyBaseline.xml` and `Diagnostics/FinalMatrix.xml` preserve that evidence. The current three-enemy sizing checks use the real current zone launch and full presentation validator.

Review [the gallery](Review.html), [measured geometry](Corrected/Geometry.json), [machine report](REPORT.json), [final Unity tests](Diagnostics/Verified.xml), and the isolated scene `Assets/_Game/Scenes/ArtPreview_ForestReworkV2_FrameFit.unity`.
'''
(R/'README.md').write_text(readme,encoding='utf-8')
links=' '.join(f'<a href="Corrected/{c["zone"]}-1080x1920.png">{c["zone"]}</a>' for c in cases[:5])
html='''<!doctype html><meta charset="utf-8"><title>Forest snug frame review</title>
<style>body{margin:28px;background:#111b22;color:#dce6dc;font:16px system-ui}h1{font-size:25px}p{max-width:1000px;line-height:1.5}a{color:#a8d6be;margin-right:12px}img{image-rendering:pixelated;vertical-align:top}button{padding:9px 16px;background:#344a49;color:white;border:1px solid #619494;cursor:pointer}figure{margin:20px 0}table{border-collapse:collapse}td,th{padding:8px 18px;border:1px solid #344a49}</style>
<h1>Forest frame — snug fit, unchanged tiles</h1>
<p>Actual Unity art-only preview. Frame meets all four tile edges. Approved art, 64x64 tiles/gems, portrait and lower HUD are unchanged. No merge.</p>
<button onclick="document.getElementById('view').src='Previews/Unity-Full-720x1360.png'">Corrected</button>
<button onclick="document.getElementById('view').src='Previews/Before-720x1360.png'">Before</button>
<figure><img id="view" src="Previews/Unity-Full-720x1360.png" width="720" height="1360"></figure>
<p><a href="Previews/Unity-Full-1440x2720.png">Full 2x Unity capture</a><a href="README.md">Verification and limitations</a><a href="REPORT.json">Measured checks</a></p>
<h2>Actual production sizing checks</h2><p>Existing zone artwork shown here; the new Forest art remains a separate review. All five zones use the same playable board and portrait slot dimensions on the same viewport.</p>'''+links+'<table><tr><th>Viewport</th><th>Board</th><th>Portrait</th><th>Zones</th></tr>'+''.join(f'<tr><td>{v["viewport"]}</td><td>{v["board_pixels"]} square</td><td>{v["portrait_pixels"][0]:g} x {v["portrait_pixels"][1]:g}</td><td>5/5</td></tr>' for v in rows)+'</table><p>Unity compilation and 25 three-enemy layout cases pass. Three historical Forest test failures also reproduce on unchanged HEAD; see the report. No device/build claim.</p>'
(R/'Review.html').write_text(html,encoding='utf-8')
(R/'.gitignore').write_text('Diagnostics/*.log\nDiagnostics/ProtectedHashes.json\nCompact/\n')
print(json.dumps(report,indent=2))
