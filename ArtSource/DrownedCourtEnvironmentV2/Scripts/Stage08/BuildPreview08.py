from pathlib import Path
import shutil
F=Path(r'C:/Users/USER/.codex/worktrees/drowned-court-pixellab-v2/Dungeon Matcher');R=F/'ArtSource/DrownedCourtEnvironmentV2';W=Path(__file__).parent
s=(F/'Assets/_Game/Scripts/Editor/DrownedCourtV2Stage07Preview.cs').read_text().replace('Stage07','Stage08').replace('STAGE07','STAGE08').replace('LowerReef','MarineDetails')
s=s.replace('ArtPreview_DrownedCourtV2_Stage06.unity','ArtPreview_DrownedCourtV2_Stage07.unity')
s=s.replace('Build Stage 06 Preview','Build Stage 08 Preview')
s=s.replace('root.SetSiblingIndex(canvas.transform.Find("Stage06_SideArchitecture").GetSiblingIndex());','root.SetSiblingIndex(canvas.transform.Find("Stage05_BoardReview").GetSiblingIndex());')
s=s.replace('        Populate(root,data.placementsTall,800);','''        var priorReef=JsonUtility.FromJson<Manifest>(File.ReadAllText(Root+"/Native/Stage07/Manifest.json"));
        Populate((RectTransform)canvas.transform.Find("Stage07_LowerReef"),priorReef.placementsTall,800,"Assets/_Game/Art/DrownedCourtEnvironmentV2/Stage07");
        Populate(root,data.placementsTall,800);''')
s=s.replace('Placement[] placements,int nativeHeight)','Placement[] placements,int nativeHeight,string artPath=Art)')
s=s.replace('im.sprite=Sprite(p.name);','im.sprite=AssetDatabase.LoadAssetAtPath<Sprite>(artPath+"/"+p.name+".png");Assert.That(im.sprite,Is.Not.Null);')
s=s.replace('Stage08 uses one additive parent behind the accepted Stage06 architecture; two connected reef masses and repeatable native transitions.','Stage08 adds the selected marine ornament above existing architecture and below the unchanged board frame. Accepted sources and layout are preserved.')
(F/'Assets/_Game/Scripts/Editor/DrownedCourtV2Stage08Preview.cs').write_text(s)
for n in ['AuthorDetails08.js','PrepareNative08.py','ReviewNative08.py','BuildPreview08.py']:
 if (W/n).resolve()!=(R/'Scripts/Stage08'/n).resolve():shutil.copyfile(W/n,R/'Scripts/Stage08'/n)
print('Stage08 additive builder ready.')
