from pathlib import Path
import json,hashlib,subprocess,sys,shutil
F=Path(r'C:/Users/USER/.codex/worktrees/drowned-court-pixellab-v2/Dungeon Matcher');R=F/'ArtSource/IronveinBelowTopHUD/Revision03';W=Path(__file__).parent
log=Path(sys.argv[1]);assert log.exists() and 'Exiting batchmode successfully' in log.read_text(errors='replace')
subprocess.run([r'C:/Users/USER/DungeonMatcherSpriteFoundry/venvs/art/Scripts/python.exe','-X','utf8',str(W/'review_revision03.py')],check=True)
code=F/'Assets/_Game/Scripts/Editor/IronveinBelowTopHUDRevision03.cs'
rep=dict(status='PASS',scope='Broad walking surface only; approved separator locked',required_unity_validator=dict(passed=True,version='6000.3.19f1',log=str(log),code_sha256=hashlib.sha256(code.read_bytes()).hexdigest()),render_check='QA/Unity.json',asset_check='QA/Assets.json',scene_check='QA/SceneDelta.json',pixel_scope_check='QA/PixelScope.json',protected_files=len(json.loads((R/'QA/ApprovedSourceLock.json').read_text())),native_assets=3,preview_sizes=['360x640','720x1280','1080x1920'],walking_depth_before=20,walking_depth_after=32,separator_pixels_unchanged=True,enemy_floor_line_unchanged=True,UI_geometry_unchanged=True,pixellab_generations=0,artistic_verdict='Broad cave-slate slabs make the walking surface clearly readable. The darker surface remains distinct from the approved lighter block separator. Existing enemy feet stay exactly fixed; added floor extends backward.',limitations=['Static art-only Unity camera renders; not production Game or real-device testing.','Existing shared fractional board/UI geometry retained.','The locked feet remain approximately three native pixels behind the approved separator.'],stop='Review only. Do not merge.')
(R/'QA/Validation.json').write_text(json.dumps(rep,indent=2));shutil.copyfile(log,R/'QA/RequiredUnityValidator.log')
for p in [F/'Assets/_Game/Scenes/ArtPreview_IronveinBelowTopHUD_Revision03.unity',F/'Assets/_Game/Scenes/ArtPreview_IronveinBelowTopHUD_Revision03.unity.meta',F/'Assets/_Game/Art/IronveinBelowTopHUD/Revision03.meta',F/'Assets/_Game/Scripts/Editor/IronveinBelowTopHUDRevision03.cs.meta',*((F/'Assets/_Game/Art/IronveinBelowTopHUD/Revision03').glob('*.meta'))]:
 p.write_bytes(('\n'.join(x.rstrip() for x in p.read_text().splitlines())+'\n').encode())
assert hashlib.sha256(code.read_bytes()).hexdigest()==rep['required_unity_validator']['code_sha256']
(R/'CHECKPOINT.md').write_text('''# Review checkpoint

Revision03 complete: three 64x32 broad cave floor slabs, extending backward to y174 while keeping the front edge at y206. The user explicitly required the exact enemy floor line. Enemy feet, health bars, board and all other existing geometry remain fixed. Approved separator y206..225 is pixel-identical.

LibreSprite native copies of the approved PixelLab-derived underwater floor. Zero new generations. All prior scenes/art preserved. Dedicated art-only scene; no production changes. Validation: QA/Validation.json, QA/Unity.json, QA/PixelScope.json. Review: http://127.0.0.1:8899/Revision03/Review.html while the loopback gallery runs. Draft PR215. Stop for review; no merge.
''')
shutil.copyfile(__file__,R/'Scripts/FinishReview.py')
print('Final Revision03 checks PASS; exact separator and floor line; zero new generations')
