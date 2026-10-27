from pathlib import Path
import shutil
F=Path(r'C:/Users/USER/.codex/worktrees/drowned-court-pixellab-v2/Dungeon Matcher');R=F/'ArtSource/IronveinBelowTopHUD/Stage06'
old=(F/'Assets/_Game/Scripts/Editor/IronveinBelowTopHUDStage05.cs').read_text()
s=old.replace('Stage05','Stage06').replace('Stage 05','Stage 06').replace('Stage 05 Surroundings','Stage 06 HUD seating').replace('Stage04.unity','Stage05.unity').replace('Stage06_Surroundings_PreviewOnly','Stage06_HUDSeating_PreviewOnly')
s=s.replace('// A new review-only scene replaces only the old side-support placements.','// Separate native support art behind the unchanged HUD in a new review scene.')
a=s.index('        var side=');b=s.index('        Canvas.ForceUpdateCanvases();',a)
s=s[:a]+'''        var protectedStates=canvas.GetComponentsInChildren<Graphic>(true).Select(g=>PathOf(g.transform)+"|"+g.gameObject.activeSelf).OrderBy(v=>v).ToArray();
        Assert.That(EditorSceneManager.SaveScene(canvas.gameObject.scene,Target),Is.True);Bind();
        native=NewRect(canvas.transform,Group,new Rect(0,0,360,640));native.localScale=new Vector3(2,2,1);
        // Behind original lower props and the shared controls: artwork is never covered.
        native.SetSiblingIndex(canvas.transform.Find("IronveinLowerModules").GetSiblingIndex());
        Tile("UpperSeatingRail","SeatRail",64,570,232,7);
        Tile("LowerSeatingRail","SeatRail",64,611,232,7);
        Tile("LeftOuterPost","SeatPost",64,577,7,34);
        Tile("RightOuterPost","SeatPost",289,577,7,34);
        Stamp("LeftCorbel","CorbelRight",46,604);
        Stamp("RightCorbel","CorbelLeft",282,604);
        Stamp("LeftUpperBolt","SeatBolt",64,571);
        Stamp("RightUpperBolt","SeatBolt",291,571);
        Stamp("LeftLowerBolt","SeatBolt",64,611);
        Stamp("RightLowerBolt","SeatBolt",291,611);
''' +s[b:]
s=s.replace('&&!replaced.Contains(g.transform)','')
s=s.replace('bool sides=(p.x<34||p.x>=326)&&p.y>=242&&p.y<574;\n                bool course=p.y>=541&&p.y<557;\n                Assert.That(sides||course,Is.True,"Changed pixel outside Stage06 environment: "+w+" "+p);','bool seating=p.x>=46&&p.x<314&&p.y>=570&&p.y<618;\n                bool feet=(p.x>=46&&p.x<72||p.x>=288&&p.x<314)&&p.y>=618&&p.y<636;\n                Assert.That(seating||feet,Is.True,"Changed pixel outside Stage06 seating: "+w+" "+p);')
s=s.replace('assets=9,disabledOldSupportPlacements=14','assets=5,disabledOldSupportPlacements=0')
s=s.replace('Actual art-only Unity renders. Only side-support placements and the low rock course beneath the board change. Approved frame, drill and separator opaque pixels match. Top, board, HUD and original assets remain exact. Not live gameplay/device testing; excluded from builds.','Actual art-only Unity renders. Only environmental seating behind the bottom controls changes. Every existing Graphic geometry, reference and active state is unchanged. Approved opaque UI and props match pixel-for-pixel. Top and board remain exact; not live gameplay/device testing; excluded from builds.')
s=s.replace('IRONVEIN_LOWER_STAGE05_PASS assets=9 protected_pixels=exact old_supports_hidden=14','IRONVEIN_LOWER_STAGE06_PASS assets=5 protected_pixels=exact all_existing_objects_unchanged')
s=s.replace('Stage 06 Surroundings','Stage 06 HUD seating')
s=s.replace('p.StartsWith("Stage02_Separator_PreviewOnly/")','p.StartsWith("Stage02_Separator_PreviewOnly/")||p.StartsWith("Stage05_Surroundings_PreviewOnly/")||p.StartsWith("IronveinLowerModules/")||p.StartsWith("IronveinBoardSideModules/")||p.StartsWith("UnchangedSharedUIShells_StaticReviewOnly/")||p.StartsWith("LockedActorsAndBattleUI/")')
s=s.replace('Approved opaque frame/drill/separator pixel changed','Approved opaque UI or environment pixel changed')
(F/'Assets/_Game/Scripts/Editor/IronveinBelowTopHUDStage06.cs').write_text(s)
runner=(R.parent/'Scripts/RunStage05.ps1').read_text().replace('Stage05','Stage06')
(R.parent/'Scripts/RunStage06.ps1').write_text(runner)
target=R/'Scripts/PrepareUnityHelper.py'
if Path(__file__).resolve()!=target.resolve():shutil.copyfile(__file__,target)
print('Stage06 helper ready; all existing objects preserved, 10 new support Images, strict lower seating bounds')
