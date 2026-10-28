from pathlib import Path
import shutil
F=Path(r'C:/Users/USER/.codex/worktrees/drowned-court-pixellab-v2/Dungeon Matcher')
R=F/'ArtSource/IronveinBelowTopHUD/Revision03'
s=(F/'Assets/_Game/Scripts/Editor/IronveinBelowTopHUDRevision02.cs').read_text()
s=s.replace('Revision02','Revision03').replace('Revision 02 cave floor and block separator','Revision 03 broad walking surface')
s=s.replace('ArtPreview_IronveinBelowTopHUD_Revision01.unity','ArtPreview_IronveinBelowTopHUD_Revision02.unity')
s=s.replace('var oldFloor=canvas.transform.Find("Revision01_CombatFloor")','var oldFloor=canvas.transform.Find("Revision02_CombatFloor")')
s=s.replace('var removed=new[]{oldFloor};','var removed=oldFloor.Cast<Transform>().Where(t=>t.name.StartsWith("WalkingSurface")).ToArray();Assert.That(removed.Length,Is.EqualTo(12));\n        var separator=oldFloor.Cast<Transform>().Where(t=>t.name.StartsWith("StoneBlock")).ToArray();Assert.That(separator.Length,Is.EqualTo(9));Assert.That(separator.All(t=>t.gameObject.activeSelf),Is.True);')
s=s.replace('new Rect(0,(640-225)*2,720,(225-186)*2)','new Rect(0,(640-206)*2,720,32*2)')
s=s.replace('oldFloor.gameObject.SetActive(false);','foreach(var walkingTile in removed)walkingTile.gameObject.SetActive(false);')
s=s.replace('Prefix+"CombatFloor"','Prefix+"WalkingSurface"')
start=s.index('        // More depth behind')
end=s.index('        Canvas.ForceUpdateCanvases();',start)
s=s[:start]+'''        // User explicitly locked the enemy floor line. Extend backward only.
        // Join the approved Court rear/front slab courses as a native 64x32 tile.
        for(int i=0;i<6;i++){
            float x=i*64,w=Mathf.Min(64,360-x);if(w<=0)continue;
            Tile("WalkingSlab"+i,"WalkingSlab"+"ABC"[i%3],x,174,w,32);
        }
        Assert.That(oldFloor.gameObject.activeSelf,Is.True);
        Assert.That(separator.All(t=>t.gameObject.activeSelf),Is.True);
'''+s[end:]
s=s.replace('assets=4','assets=3').replace('hidden=1','hidden=12').replace('IRONVEIN_REVISION02_PASS','IRONVEIN_REVISION03_PASS')
s=s.replace('Changed pixel outside authorized floor/props/crystal rectangles:','Changed pixel outside walking-surface band:')
ns=s.index('note="Actual static art-only Unity renders.')
ne=s.index('"},true)',ns)
s=s[:ns]+'note="Actual static art-only Unity renders. Original Graphic geometry and references, enemy floor line, health bars, board, and approved separator remain exact. Only the twelve prior walking-surface placements are hidden. Three new 64x32 walking slabs extend the surface backward to y=174; front edge remains y=206. Point/native additions. Source scenes and production Game remain unchanged; not live combat or device testing.'+s[ne:]
s=s.replace('p.StartsWith("IronveinNativeFrames/")','p.StartsWith("Revision02_CombatFloor/StoneBlock")||p.StartsWith("IronveinNativeFrames/")')
(F/'Assets/_Game/Scripts/Editor/IronveinBelowTopHUDRevision03.cs').write_bytes(s.encode())
runner=(F/'ArtSource/IronveinBelowTopHUD/Scripts/RunRevision02.ps1').read_text().replace('Revision02','Revision03')
(F/'ArtSource/IronveinBelowTopHUD/Scripts/RunRevision03.ps1').write_bytes(runner.encode())
shutil.copyfile(__file__,R/'Scripts/InstallPreview.py')
print('Installed Revision03 preview builder: six native tiles, original separator and actors locked')
