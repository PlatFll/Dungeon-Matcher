from pathlib import Path
import json,hashlib,subprocess,sys,shutil
F=Path(r'C:/Users/USER/.codex/worktrees/drowned-court-pixellab-v2/Dungeon Matcher');R=F/'ArtSource/IronveinBelowTopHUD/Revision04';W=Path(__file__).parent
log=Path(sys.argv[1]);assert log.exists() and 'Exiting batchmode successfully' in log.read_text(errors='replace')
subprocess.run([r'C:/Users/USER/DungeonMatcherSpriteFoundry/venvs/art/Scripts/python.exe','-X','utf8',str(W/'review_revision04.py')],check=True)
code=F/'Assets/_Game/Scripts/Editor/IronveinBelowTopHUDRevision04.cs';unity=json.loads((R/'QA/Unity.json').read_text())
rep=dict(status='PASS',scope='Center approved walking surface on the shared actor floor, with approved separator directly below',required_unity_validator=dict(passed=True,version='6000.3.19f1',log=str(log),code_sha256=hashlib.sha256(code.read_bytes()).hexdigest()),render_check='QA/Unity.json',source_and_pixel_check='QA/Independent.json',scene_check='QA/SceneDelta.json',reference_inspection='QA/LayoutReferences.json',protected_files=len(json.loads((R/'QA/ApprovedSourceLock.json').read_text())),changed_rectangles=15,translation_native_pixels=unity['shiftedNativePixels'],floor=[unity['floorTop'],unity['floorBottom']],feet=unity['enemyFloorNative'],separator=[unity['floorBottom'],unity['separatorBottom']],artwork_changes=0,pixellab_generations=0,actor_and_UI_unchanged=True,preview_sizes=['360x640','720x1280','1080x1920'],limitations=['Static art-only Unity camera renders; not production Game integration or device testing.','Existing shared fractional board/UI geometry retained.'],stop='Review only. Do not merge.')
(R/'QA/Validation.json').write_text(json.dumps(rep,indent=2));shutil.copyfile(log,R/'QA/RequiredUnityValidator.log')
for rel in ['Assets/_Game/Scenes/ArtPreview_IronveinBelowTopHUD_Revision04.unity','Assets/_Game/Scenes/ArtPreview_IronveinBelowTopHUD_Revision04.unity.meta','Assets/_Game/Scripts/Editor/IronveinBelowTopHUDRevision04.cs.meta']:
 p=F/rel;p.write_bytes(('\n'.join(x.rstrip() for x in p.read_text().splitlines())+'\n').encode())
assert hashlib.sha256(code.read_bytes()).hexdigest()==rep['required_unity_validator']['code_sha256']
(R/'CHECKPOINT.md').write_text('''# Review checkpoint

Revision04 aligns the existing 32px floor around native enemy floor y204. Floor y188..220; separator y220..239. Both moved down 14 native pixels without redrawing/scaling. Actors, HP, countdowns, weakness gems, board, portrait and all other UI remain fixed. Same scenery-around-baseline ownership as Dungeon/Forest/Court. Prior scenes and all artwork preserved.

Actual art-only Unity previews and validation pass at 360x640, 720x1280 and 1080x1920. Source preservation and serialized delta checked. Zero generation spend. Review: http://127.0.0.1:8899/Revision04/Review.html while loopback gallery runs. Existing draft PR215. No production integration or merge; stop for user review.
''')
shutil.copyfile(__file__,R/'Scripts/FinishReview.py')
print('Revision04 final validation PASS; original artwork, actors and UI preserved')
