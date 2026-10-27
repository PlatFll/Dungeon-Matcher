using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Object=UnityEngine.Object;

// Review-only native modular assembly. Does not load/save Game or modify production bindings.
public static class IronveinStagedPreview
{
    const string Root="ArtSource/IronveinStaged";
    const string Art="Assets/_Game/Art/IronveinStaged";
    const string Scene="Assets/_Game/Scenes/ArtPreview_IronveinStaged_Final.unity";
    static Canvas canvas;static Camera camera;
    static RectTransform native,top,boardSides,lower,cells,actors,board,portrait,portraitFrame,boardFrame;
    static int imported,captures;
    [MenuItem("Dungeon Matcher/Art/Ironvein Staged/Build Review")]
    public static void Build()
    {
        Assert.That(Application.isPlaying,Is.False);imported=captures=0;Import();
        EditorSceneManager.OpenScene("Assets/_Game/Scenes/ArtPreview_IronveinStaged_Scaffold.unity");
        canvas=Object.FindFirstObjectByType<Canvas>();camera=Object.FindFirstObjectByType<Camera>();
        var snapshot=JsonUtility.FromJson<IronveinStagedContentCapture.Snapshot>(File.ReadAllText(Root+"/QA/ContentSnapshot.json"));
        board=(RectTransform)canvas.transform.Find("PlayableBoardBounds_NoTileReplacement");
        portrait=(RectTransform)canvas.transform.Find("PortraitInterior_PlaceholderMask");
        var boardStart=board.anchoredPosition;var boardSize=board.rect.size;var portraitStart=portrait.anchoredPosition;var portraitSize=portrait.rect.size;
        Assert.That((board.rect.size-snapshot.board.size).sqrMagnitude,Is.LessThan(.001f));
        foreach(Transform t in canvas.transform)if(t.name.StartsWith("Blank")||t.name.StartsWith("GridGuide")||t.name=="ExistingBattleFloorGuide")t.gameObject.SetActive(false);
        native=NativeGroup("IronveinNativeBackground");native.SetAsFirstSibling();
        Solid(native,"NeutralReviewBacking",new Rect(0,0,360,640));
        top=NativeGroup("IronveinTopModules");top.SetSiblingIndex(1);BuildTop();
        // Overfill inside the existing portrait mask. Its opening and layout remain measured production values.
        foreach(Transform t in portrait)t.gameObject.SetActive(false);
        var backing=Rect(portrait,"OverfilledNativeRecess",new Rect(-16,-16,portrait.rect.width+32,portrait.rect.height+32));
        var backImage=backing.gameObject.AddComponent<Image>();backImage.color=new Color32(16,19,31,255);backImage.raycastTarget=false;
        portrait.SetAsLastSibling();
        boardSides=NativeGroup("IronveinBoardSideModules");BuildSides();
        lower=NativeGroup("IronveinLowerModules");BuildLower(640);
        var frameLayer=NativeGroup("IronveinNativeFrames");
        if(Has("BoardCornerTL"))boardFrame=MakeFrame(frameLayer,board,"Board",90);
        if(Has("PortraitCornerTL"))portraitFrame=MakeFrame(frameLayer,portrait,"Portrait",60);
        cells=Rect(canvas.transform,"LockedBoardCellsAndGems",new Rect(0,0,720,1280));
        var cellRoot=Rect(cells,"Cells",new Rect(0,0,720,1280));
        foreach(var p in snapshot.cells)Piece(cellRoot,p);
        var gemRoot=Rect(cells,"Gems",new Rect(0,0,720,1280));
        foreach(var p in snapshot.gems.Where(p=>p.name.StartsWith("Gem_")))Piece(gemRoot,p);
        Assert.That(cellRoot.childCount,Is.EqualTo(64));Assert.That(gemRoot.childCount,Is.EqualTo(64));
        actors=Rect(canvas.transform,"LockedActorsAndBattleUI",new Rect(0,0,720,1280));
        foreach(var g in snapshot.graphics.Where(g=>!g.path.Contains("/Player")))Graphic(actors,g);
        var sharedPlayer=JsonUtility.FromJson<IronveinStagedContentCapture.Snapshot>(File.ReadAllText(Root+"/QA/SharedPlayerSnapshot.json"));
        foreach(var g in sharedPlayer.graphics.Where(g=>g.path.Contains("/Player")))Graphic(actors,g);
        canvas.transform.Find("UnchangedSharedUIShells_StaticReviewOnly").SetAsLastSibling();
        Assert.That(board.anchoredPosition,Is.EqualTo(boardStart));Assert.That(board.rect.size,Is.EqualTo(boardSize));
        Assert.That(portrait.anchoredPosition,Is.EqualTo(portraitStart));Assert.That(portrait.rect.size,Is.EqualTo(portraitSize));
        Assert.That(Object.FindObjectsByType<BoardController>(FindObjectsSortMode.None),Is.Empty);
        Assert.That(EditorBuildSettings.scenes.Any(s=>s.path==Scene),Is.False);
        foreach(var f in canvas.GetComponentsInChildren<ResponsiveModularFrameFitter>())f.RefreshFrame();
        CheckNative();Canvas.ForceUpdateCanvases();CheckSharedShells();Assert.That(EditorSceneManager.SaveScene(canvas.gameObject.scene,Scene),Is.True);
        Capture(1280);
        // Tall layout uses the actual Ironvein measurements recorded by the preflight.
        ((RectTransform)canvas.transform).sizeDelta=new Vector2(720,1600);
        native.sizeDelta=new Vector2(360,800);native.GetChild(0).GetComponent<RectTransform>().sizeDelta=new Vector2(360,800);
        top.anchoredPosition+=Vector2.up*318;portrait.anchoredPosition+=Vector2.up*318;actors.anchoredPosition+=Vector2.up*318;
        board.anchoredPosition+=Vector2.up*192;cells.anchoredPosition+=Vector2.up*192;boardSides.anchoredPosition+=Vector2.up*192;
        foreach(RectTransform child in canvas.transform.Find("UnchangedSharedUIShells_StaticReviewOnly"))if(child.anchoredPosition.y>798)child.anchoredPosition+=Vector2.up*318;
        foreach(var f in canvas.GetComponentsInChildren<ResponsiveModularFrameFitter>())f.RefreshFrame();
        BuildLower(800);camera.orthographicSize=800;camera.transform.position=new Vector3(360,800,-10);Capture(1600);
        File.WriteAllText(Root+"/QA/UnityAssembly.json",JsonUtility.ToJson(new Result{status="PASS",importedAssets=imported,captures=captures,
            boardPosition=boardStart,boardSize=boardSize,portraitPosition=portraitStart,portraitSize=portraitSize,
            lockedCells=64,lockedGems=64,sourcePixelScales=new[]{1,2,3},nativeImportsExact=true,sceneExcludedFromBuild=true,
            note="Actual Unity camera renders of an art-only scene. Measured live Ironvein cells/gems/enemies and current shared Dungeon player/HUD graphics copied read-only at their existing rects. New environment is separate tiled native art; production Game remains unchanged. No device or gameplay integration claim."},true));
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);EditorSceneManager.OpenScene(Scene);Debug.Log("IRONVEIN_STAGED_ASSEMBLY_PASS");
    }
    [Serializable] class ShellProbe { public ShellGraphic[] pieces; }
    [Serializable] class ShellGraphic { public string name; public Rect rect; }
    static void CheckSharedShells(){
        var shell=canvas.transform.Find("UnchangedSharedUIShells_StaticReviewOnly");
        var data=JsonUtility.FromJson<ShellProbe>(File.ReadAllText(Root+"/QA/SharedShellSnapshot.json"));
        foreach(var g in data.pieces){var rect=shell.GetComponentsInChildren<RectTransform>().FirstOrDefault(r=>r.name==g.name.Replace('/','_'));Assert.That(rect,Is.Not.Null,g.name);
            var v=new Vector3[4];rect.GetWorldCorners(v);var actual=UnityEngine.Rect.MinMaxRect(v[0].x,v[0].y,v[2].x,v[2].y);
            Assert.That((actual.position-g.rect.position).sqrMagnitude,Is.LessThan(.01f),g.name+" moved");Assert.That((actual.size-g.rect.size).sqrMagnitude,Is.LessThan(.01f),g.name+" resized");}
        Debug.Log("IRONVEIN_SHARED_HUD_GEOMETRY_PASS "+data.pieces.Length);
    }
    static bool Has(string n)=>File.Exists(Art+"/"+n+".png");
    static void Optional(Transform p,string n,float x,float y,int h=640){if(Has(n))Stamp(p,n,x,y,h);}
    static void BuildTop()
    {
        Stamp(top,"CavernDepth",120,43);
        Optional(top,"Timber",111,113);Optional(top,"Timber",331,113);
        Optional(top,"Lantern",126,113);Optional(top,"Lantern",338,112);
        Optional(top,"LanternSmall",268,114);
        Optional(top,"Wheel",290,135);Optional(top,"Cart",149,145);
    }
    static void BuildSides()
    {
        if(!Has("SupportShaft"))return;
        foreach(float x in new[]{0f,328f}){
            Optional(boardSides,"SupportCap",x,244);TileTop(boardSides,"SupportShaft",new Rect(x,264,32,262));
            Optional(boardSides,"SupportBrace",x,333);Optional(boardSides,"SupportBrace",x,447);
            Optional(boardSides,"SupportRune",x,386);Optional(boardSides,"SupportBase",x,524);
        }
        Optional(boardSides,"Chain",5,264);Optional(boardSides,"Chain",337,264);
        Optional(boardSides,"Lantern",6,313);Optional(boardSides,"Lantern",338,313);
    }
    static void BuildLower(int h)
    {
        for(int i=lower.childCount-1;i>=0;i--)Object.DestroyImmediate(lower.GetChild(i).gameObject);
        lower.sizeDelta=new Vector2(360,h);float reach=board.anchoredPosition.y/2-9;
        if(Has("SupportShaft"))foreach(float x in new[]{0f,328f})Tile(lower,"SupportShaft",new Rect(x,68,32,Mathf.Max(1,reach-68)));
        foreach(string n in new[]{"LowerLeft","LowerRight"})if(Has(n)){var sp=Sprite(n);Stamp(lower,n,n=="LowerLeft"?0:360-sp.rect.width,h-sp.rect.height,h);}
        if(Has("LowerLeft")){
            Optional(lower,"Crate",9,h-38,h);
        }
    }
    static RectTransform MakeFrame(Transform parent,RectTransform opening,string prefix,float pitch)
    {
        var bounds=Rect(parent,prefix+"FrameBounds",new Rect(0,0,320,320));
        var modular=Rect(bounds,"ModularFrame",new Rect(0,0,320,320));modular.anchorMax=Vector2.one;modular.offsetMin=modular.offsetMax=Vector2.zero;
        var names=new[]{"TopLeftCorner","TopRightCorner","BottomLeftCorner","BottomRightCorner","TopEdge","BottomEdge","LeftEdge","RightEdge"};
        var art=new[]{"CornerTL","CornerTR","CornerBL","CornerBR","RailTop","RailBottom","RailLeft","RailRight"};
        for(int i=0;i<names.Length;i++){var im=Rect(modular,names[i],new Rect(0,0,25,25)).gameObject.AddComponent<Image>();im.sprite=Sprite(prefix+art[i]);im.raycastTarget=false;}
        var f=modular.gameObject.AddComponent<ResponsiveModularFrameFitter>();f.ConfigureNativePieces(Sprite(prefix+"Joint"),pitch);f.FollowOpening(opening);
        // Existing shared fitter owns centering and native tile cropping. Probe before restoring the real opening.
        var probe=Rect(canvas.transform,"DisposableFrameSizeProbe",new Rect(50,50,512,512));
        foreach(float w in new[]{512f,600f}){probe.sizeDelta=new Vector2(w,w);f.FollowOpening(probe);Assert.That(modular.Find("TopEdge").GetComponent<Image>().type,Is.EqualTo(Image.Type.Tiled));}
        Object.DestroyImmediate(probe.gameObject);f.FollowOpening(opening);CheckCenters(modular);return bounds;
    }
    static void Piece(Transform parent,IronveinStagedContentCapture.Piece p)
    {var im=Rect(parent,p.name,p.rect).gameObject.AddComponent<Image>();im.sprite=LoadSprite(p.asset,p.sprite);Assert.That(im.sprite.rect.size,Is.EqualTo(p.nativeSize));im.color=p.color;im.raycastTarget=false;}
    static void Graphic(Transform parent,IronveinStagedContentCapture.GraphicRecord g)
    {
        var r=Rect(parent,g.path.Replace('/','_'),g.rect);
        if(g.kind=="image"){var im=r.gameObject.AddComponent<Image>();im.color=g.color;im.raycastTarget=false;if(!string.IsNullOrEmpty(g.asset))im.sprite=LoadSprite(g.asset,g.sprite);im.type=(Image.Type)g.imageType;im.fillAmount=g.fill;im.fillMethod=(Image.FillMethod)g.fillMethod;im.fillOrigin=g.fillOrigin;im.preserveAspect=g.preserveAspect;im.pixelsPerUnitMultiplier=g.ppuMultiplier;}
        else{var t=r.gameObject.AddComponent<TextMeshProUGUI>();t.font=AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(g.asset);t.text=g.text;t.fontSize=g.fontSize;t.color=g.color;t.alignment=(TextAlignmentOptions)g.alignment;t.fontStyle=(FontStyles)g.fontStyle;t.textWrappingMode=TextWrappingModes.NoWrap;t.overflowMode=TextOverflowModes.Overflow;t.raycastTarget=false;}
    }
    static RectTransform NativeGroup(string name){var r=Rect(canvas.transform,name,new Rect(0,0,360,640));r.localScale=new Vector3(2,2,1);return r;}
    static RectTransform Rect(Transform parent,string name,Rect b){var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(parent,false);r.anchorMin=r.anchorMax=r.pivot=Vector2.zero;r.anchoredPosition=b.position;r.sizeDelta=b.size;return r;}
    static void Stamp(Transform parent,string name,float x,float y,int h=640){var s=Sprite(name);var r=Rect(parent,name,new Rect(x,h-y-s.rect.height,s.rect.width,s.rect.height));var im=r.gameObject.AddComponent<Image>();im.sprite=s;im.raycastTarget=false;}
    static void TileTop(Transform p,string n,Rect b){Tile(p,n,new Rect(b.x,640-b.y-b.height,b.width,b.height));}
    static void Tile(Transform p,string n,Rect b){var im=Rect(p,n,b).gameObject.AddComponent<Image>();im.sprite=Sprite(n);im.type=Image.Type.Tiled;im.pixelsPerUnitMultiplier=1;im.raycastTarget=false;}
    static Sprite Sprite(string n)=>LoadSprite(Art+"/"+n+".png",null);
    static Sprite LoadSprite(string path,string name){var s=AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().FirstOrDefault(v=>name==null||v.name==name);Assert.That(s,Is.Not.Null,path+":"+name);return s;}
    static void Import()
    {
        Directory.CreateDirectory(Art);
        foreach(var source in Directory.GetFiles(Root+"/Exports","*.png")){
            string path=Art+"/"+Path.GetFileName(source);File.Copy(source,path,true);AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport);
            var ti=(TextureImporter)AssetImporter.GetAtPath(path);ti.textureType=TextureImporterType.Sprite;ti.spriteImportMode=SpriteImportMode.Single;ti.spritePixelsPerUnit=64;
            ti.filterMode=FilterMode.Point;ti.textureCompression=TextureImporterCompression.Uncompressed;ti.mipmapEnabled=false;ti.npotScale=TextureImporterNPOTScale.None;ti.maxTextureSize=2048;
            ti.alphaSource=TextureImporterAlphaSource.FromInput;ti.alphaIsTransparency=true;ti.isReadable=true;ti.wrapMode=TextureWrapMode.Clamp;
            var settings=new TextureImporterSettings();ti.ReadTextureSettings(settings);settings.spriteMeshType=SpriteMeshType.FullRect;ti.SetTextureSettings(settings);ti.SaveAndReimport();
            var original=new Texture2D(2,2);original.LoadImage(File.ReadAllBytes(source));var actual=AssetDatabase.LoadAssetAtPath<Texture2D>(path);Assert.That(actual.width,Is.EqualTo(original.width));Assert.That(actual.height,Is.EqualTo(original.height));
            var a=actual.GetPixels32();var b=original.GetPixels32();for(int i=0;i<a.Length;i++)Assert.That(a[i].a==b[i].a&&(b[i].a==0||a[i].Equals(b[i])),Is.True,path+" pixel "+i);Object.DestroyImmediate(original);imported++;
        }
    }
    static void Solid(Transform parent,string name,Rect bounds)
    {var im=Rect(parent,name,bounds).gameObject.AddComponent<Image>();im.color=new Color32(16,19,31,255);im.raycastTarget=false;}
    static void CheckNative()
    {
        foreach(var root in new[]{native,top,boardSides,lower})foreach(var im in root.GetComponentsInChildren<Image>())
            if(im.sprite!=null&&im.type==Image.Type.Simple)Assert.That(im.rectTransform.rect.size,Is.EqualTo(im.sprite.rect.size),im.name+" must retain source pixels");
        Assert.That(portrait.GetComponent<RectMask2D>(),Is.Not.Null);
    }
    static void CheckCenters(RectTransform frame)
    {
        Func<string,Vector3> center=n=>{var r=(RectTransform)frame.Find(n);return frame.InverseTransformPoint(r.TransformPoint(r.rect.center));};
        Assert.That(center("LeftEdge").x,Is.EqualTo(center("TopLeftCorner").x).Within(.01f));
        Assert.That(center("RightEdge").x,Is.EqualTo(center("TopRightCorner").x).Within(.01f));
        Assert.That(center("TopEdge").y,Is.EqualTo(center("TopLeftCorner").y).Within(.01f));
        Assert.That(center("BottomEdge").y,Is.EqualTo(center("BottomLeftCorner").y).Within(.01f));
    }
    static void Capture(int h){foreach(int w in new[]{360,720,1080})Render(w,h*w/720,"Unity-"+w+"x"+(h*w/720)+".png");}
    static void Render(int w,int h,string name)
    {
        Canvas.ForceUpdateCanvases();var rt=RenderTexture.GetTemporary(w,h,24,RenderTextureFormat.ARGB32);rt.filterMode=FilterMode.Point;rt.antiAliasing=1;
        var prior=RenderTexture.active;camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;var png=new Texture2D(w,h,TextureFormat.RGBA32,false);png.ReadPixels(new Rect(0,0,w,h),0,0);png.Apply();File.WriteAllBytes(Root+"/Preview/"+name,png.EncodeToPNG());
        RenderTexture.active=prior;camera.targetTexture=null;RenderTexture.ReleaseTemporary(rt);Object.DestroyImmediate(png);captures++;
    }
    [Serializable] class Result{public string status,note;public int importedAssets,captures,lockedCells,lockedGems;public int[] sourcePixelScales;public bool nativeImportsExact,sceneExcludedFromBuild;public Vector2 boardPosition,boardSize,portraitPosition,portraitSize;}
}
