using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Object=UnityEngine.Object;

// Stage 01 only: duplicate the accepted assembly and mark a non-rendering edit boundary.
public static class IronveinBelowTopHUDStage01
{
    const string Root="ArtSource/IronveinBelowTopHUD/Stage01";
    const string Source="Assets/_Game/Scenes/ArtPreview_IronveinStaged_Final.unity";
    const string Target="Assets/_Game/Scenes/ArtPreview_IronveinBelowTopHUD_Stage01.unity";
    static Canvas canvas;
    static Camera camera;

    [MenuItem("Dungeon Matcher/Art/Ironvein Below Top HUD/Stage 01 Foundation")]
    public static void Build()
    {
        Assert.That(Application.isPlaying,Is.False);
        Assert.That(File.Exists(Target),Is.False,"Foundation already exists; preserve the working copy.");
        Directory.CreateDirectory(Root+"/Preview");Directory.CreateDirectory(Root+"/QA");
        EditorSceneManager.OpenScene(Source);Bind();
        var sizes=new[]{360,720,1080};
        var baseline=sizes.Select(w=>Capture(w,"Source")).ToArray();
        Assert.That(AssetDatabase.CopyAsset(Source,Target),Is.True);
        EditorSceneManager.OpenScene(Target);Bind();
        var board=(RectTransform)canvas.transform.Find("PlayableBoardBounds_NoTileReplacement");
        var portrait=(RectTransform)canvas.transform.Find("PortraitInterior_PlaceholderMask");
        var top=canvas.transform.Find("IronveinTopModules");
        var actors=canvas.transform.Find("LockedActorsAndBattleUI");
        var playerFrame=canvas.transform.Find("IronveinNativeFrames/PortraitFrameBounds");
        Assert.That(top,Is.Not.Null);Assert.That(actors,Is.Not.Null);Assert.That(playerFrame,Is.Not.Null);
        Canvas.ForceUpdateCanvases();
        float protectedMinY=float.MaxValue;
        foreach(var root in new[]{top,actors,portrait,playerFrame})
            foreach(var graphic in root.GetComponentsInChildren<Graphic>())
                protectedMinY=Mathf.Min(protectedMinY,WorldRect(graphic.rectTransform).yMin);
        // Keep one native-pixel guard row below all upper visuals, aligned to the 2x source grid.
        float lowerTop=Mathf.Floor(protectedMinY/2)*2-2;
        Assert.That(lowerTop,Is.GreaterThan(WorldRect(board).yMax));
        var lower=EmptyRect("LOWER_ONLY_StageBoundary_NoArtwork",new Rect(0,0,720,lowerTop));
        lower.gameObject.AddComponent<RectMask2D>(); // Future scenery belongs here, behind existing UI.
        lower.SetSiblingIndex(1);
        EmptyRect("LOCKED_TOP_KeepCurrentArtwork",new Rect(0,lowerTop,720,1280-lowerTop));
        Assert.That(Object.FindObjectsByType<BoardController>(FindObjectsSortMode.None),Is.Empty);
        Assert.That(EditorBuildSettings.scenes.Any(s=>s.path==Target),Is.False);
        Assert.That(EditorSceneManager.SaveScene(canvas.gameObject.scene,Target),Is.True);
        for(int i=0;i<sizes.Length;i++){
            var revised=Capture(sizes[i],"Unity");
            CollectionAssert.AreEqual(baseline[i],revised,"Stage 01 must not change any rendered pixel at "+sizes[i]);
        }
        var report=new Report{status="PASS",sourceScene=Source,workingScene=Target,sourcePixelsUnchanged=true,
            sourceWidth=720,sourceHeight=1280,firstEditableScreenRow=1280-lowerTop,
            firstEditableNativeRow=(1280-lowerTop)/2,protectedGraphicsMinWorldY=protectedMinY,
            editableBoundsWorld=new Rect(0,0,720,lowerTop),boardBoundsWorld=WorldRect(board),portraitBoundsWorld=WorldRect(portrait),
            capturedWidths=sizes,sceneExcludedFromBuild=true,
            note="Entire source and duplicate screenshots match, not just the locked top. The empty lower mask marks scope and renders no artwork. Actual Unity art-only preview; no generation, no gameplay integration, no merge."};
        File.WriteAllText(Root+"/QA/Foundation.json",JsonUtility.ToJson(report,true));
        Debug.Log("IRONVEIN_LOWER_STAGE01_PASS boundary_screen_row="+report.firstEditableScreenRow+" native="+report.firstEditableNativeRow);
    }
    static void Bind(){canvas=Object.FindFirstObjectByType<Canvas>();camera=Object.FindFirstObjectByType<Camera>();Assert.That(canvas,Is.Not.Null);Assert.That(camera,Is.Not.Null);Canvas.ForceUpdateCanvases();}
    static Rect WorldRect(RectTransform r){var c=new Vector3[4];r.GetWorldCorners(c);return Rect.MinMaxRect(c[0].x,c[0].y,c[2].x,c[2].y);}
    static RectTransform EmptyRect(string name,Rect b){var r=new GameObject(name,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(canvas.transform,false);r.anchorMin=r.anchorMax=r.pivot=Vector2.zero;r.anchoredPosition=b.position;r.sizeDelta=b.size;return r;}
    static Color32[] Capture(int w,string prefix)
    {
        int h=w*1280/720;Canvas.ForceUpdateCanvases();var rt=RenderTexture.GetTemporary(w,h,24,RenderTextureFormat.ARGB32);rt.filterMode=FilterMode.Point;rt.antiAliasing=1;
        var prior=RenderTexture.active;camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;
        var image=new Texture2D(w,h,TextureFormat.RGBA32,false);image.ReadPixels(new Rect(0,0,w,h),0,0);image.Apply();var pixels=image.GetPixels32();
        File.WriteAllBytes(Root+"/Preview/"+prefix+"-"+w+"x"+h+".png",image.EncodeToPNG());
        RenderTexture.active=prior;camera.targetTexture=null;RenderTexture.ReleaseTemporary(rt);Object.DestroyImmediate(image);return pixels;
    }
    [Serializable] class Report{public string status,sourceScene,workingScene,note;public bool sourcePixelsUnchanged,sceneExcludedFromBuild;public int sourceWidth,sourceHeight;public float firstEditableScreenRow,firstEditableNativeRow,protectedGraphicsMinWorldY;public Rect editableBoundsWorld,boardBoundsWorld,portraitBoundsWorld;public int[] capturedWidths;}
}
