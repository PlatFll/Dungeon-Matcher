from pathlib import Path
import shutil
F=Path(r'C:/Users/USER/.codex/worktrees/drowned-court-pixellab-v2/Dungeon Matcher');R=F/'ArtSource/DrownedCourtEnvironmentV2';W=Path(__file__).parent
p=F/'Assets/_Game/Scripts/Editor/DrownedCourtV2Stage06Preview.cs';s=p.read_text().replace('Stage06','Stage07').replace('STAGE06','STAGE07').replace('SideArchitecture','LowerReef')
s=s.replace('ArtPreview_DrownedCourtV2_Stage05.unity','ArtPreview_DrownedCourtV2_Stage06.unity')
start=s.index('        foreach(var p in data.placements)');end=s.index('        Assert.That(board.rect.size',start)
s=s[:start]+'''        Populate(root,data.placements,640);
        Assert.That(root.childCount,Is.EqualTo(data.placements.Length));
'''+s[end:]
s=s.replace('root.SetSiblingIndex(canvas.transform.Find("Stage05_BoardReview").GetSiblingIndex());','root.SetSiblingIndex(canvas.transform.Find("Stage06_SideArchitecture").GetSiblingIndex());')
s=s.replace('t.name=="Stage07_LowerReef")r.anchoredPosition','t.name=="Stage06_SideArchitecture")r.anchoredPosition')
s=s.replace('        camera.orthographicSize=800;', '        Populate(root,data.placementsTall,800);\n        camera.orthographicSize=800;')
at=s.index('    static void Capture(')
s=s[:at]+'''    static void Populate(RectTransform root,Placement[] placements,int nativeHeight)
    {
        for(int i=root.childCount-1;i>=0;i--)Object.DestroyImmediate(root.GetChild(i).gameObject);
        root.sizeDelta=new Vector2(360,nativeHeight);
        foreach(var p in placements)
        {
            var r=Rect(root,p.name+"_"+p.x+"_"+p.y,new Rect(p.x,nativeHeight-p.y-p.height,p.width,p.height));
            var im=r.gameObject.AddComponent<Image>();im.sprite=Sprite(p.name);im.raycastTarget=false;
            Assert.That(im.sprite.rect.size,Is.EqualTo(r.rect.size));Assert.That(r.localScale,Is.EqualTo(Vector3.one));
            Assert.That(r.localRotation,Is.EqualTo(Quaternion.identity));
        }
    }
'''+s[at:]
s=s.replace('public Placement[] placements;','public Placement[] placements,placementsTall;')
s=s.replace('Stage07 uses one additive parent behind the existing board frame/cells.','Stage07 uses one additive parent behind the accepted Stage06 architecture; two connected reef masses and repeatable native transitions.')
(F/'Assets/_Game/Scripts/Editor/DrownedCourtV2Stage07Preview.cs').write_text(s)
for name in ['AuthorReef07.js','PrepareNative07.py','ReviewNative07.py']:
 if (W/name).resolve()!=(R/'Scripts/Stage07'/name).resolve():shutil.copyfile(W/name,R/'Scripts/Stage07'/name)
print('Stage07 additive Unity preview builder prepared.')
