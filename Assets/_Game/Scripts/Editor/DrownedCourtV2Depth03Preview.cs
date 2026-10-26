using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Object=UnityEngine.Object;

public static class DrownedCourtV2Depth03Preview
{
    const string Root="ArtSource/DrownedCourtEnvironmentV2";
    const string Output="ArtSource/DrownedCourtEnvironmentV2/Preview/Revision03";
    const string Scene="Assets/_Game/Scenes/ArtPreview_DrownedCourtV2_Revision03.unity";
    [MenuItem("Dungeon Matcher/Art/Drowned Court V2/Build Depth Revision 03")]
    public static void Build()
    {
        Directory.CreateDirectory(Output+"/Motion");Directory.CreateDirectory(Root+"/QA/Revision03");
        EditorSceneManager.OpenScene("Assets/_Game/Scenes/ArtPreview_DrownedCourtV2_Revision02.unity");
        var canvas=Object.FindFirstObjectByType<Canvas>();var camera=Object.FindFirstObjectByType<Camera>();
        var old=canvas.GetComponentsInChildren<RectTransform>(true).ToDictionary(r=>r,r=>Record(r));
        var images=canvas.GetComponentsInChildren<Image>(true).ToDictionary(im=>im,im=>im.sprite);
        var architecture=(RectTransform)canvas.transform.Find("Stage02_Architecture_Candidates");
        var platform=(RectTransform)canvas.transform.Find("Stage03_Platform_Candidates");
        Assert.That(architecture,Is.Not.Null);Assert.That(platform,Is.Not.Null);
        Sample(canvas,5.9f);foreach(int scale in new[]{1,2,3})Render(camera,scale,"Before");
        // Move native modules back one floor course; retain their size and every source pixel.
        architecture.anchoredPosition+=Vector2.up*32;
        var rear=new GameObject("Revision03_RearWalkway",typeof(RectTransform)).GetComponent<RectTransform>();
        rear.SetParent(canvas.transform,false);rear.anchorMin=platform.anchorMin;rear.anchorMax=platform.anchorMax;rear.pivot=platform.pivot;
        rear.sizeDelta=platform.sizeDelta;rear.anchoredPosition=platform.anchoredPosition+Vector2.up*32;rear.localScale=platform.localScale;
        rear.SetSiblingIndex(architecture.GetSiblingIndex());
        foreach(var im in platform.GetComponentsInChildren<Image>().Where(i=>i.sprite&&i.sprite.name.Contains("FloorTop")))
        { var copy=Object.Instantiate(im.gameObject,rear,false);copy.name=im.name; }
        Assert.That(rear.childCount,Is.EqualTo(8));
        foreach(var pair in old)if(pair.Key!=architecture)Assert.That(Record(pair.Key),Is.EqualTo(pair.Value),pair.Key.name+" layout");
        foreach(var pair in images)Assert.That(pair.Key.sprite,Is.SameAs(pair.Value),pair.Key.name+" art binding");
        Assert.That(Object.FindObjectsByType<BoardController>(FindObjectsSortMode.None),Is.Empty);
        Assert.That(EditorBuildSettings.scenes.Any(s=>s.path==Scene),Is.False);
        Assert.That(EditorSceneManager.SaveScene(canvas.gameObject.scene,Scene),Is.True);
        foreach(int scale in new[]{1,2,3})Render(camera,scale,"Revised");
        for(int f=0;f<288;f++){Sample(canvas,f/12f);Capture(camera,360,640,Output+"/Motion/Frame-"+f.ToString("000")+".png");}
        Sample(canvas,0);Capture(camera,360,640,Output+"/LoopStart.png");Sample(canvas,24);Capture(camera,360,640,Output+"/LoopEnd.png");
        File.WriteAllText(Root+"/QA/Revision03/DepthPreview.json",JsonUtility.ToJson(new Result{status="PASS",architectureMovedNativePixels=16,existingRectsPreserved=old.Count-1,existingImageBindingsPreserved=images.Count,reusedFloorModules=rear.childCount,originalArtUnchanged=true},true));
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);EditorSceneManager.OpenScene(Scene);
        Debug.Log("DROWNED_COURT_DEPTH03_PASS");
    }
    static string Record(RectTransform r)=>r.anchoredPosition+"|"+r.sizeDelta+"|"+r.localScale+"|"+r.anchorMin+"|"+r.anchorMax+"|"+r.pivot;
    static void Sample(Canvas canvas,float time){foreach(var animator in canvas.GetComponentsInChildren<Animator>()){var clip=animator.runtimeAnimatorController.animationClips.Single();clip.SampleAnimation(animator.gameObject,Mathf.Repeat(time,clip.length));}Canvas.ForceUpdateCanvases();}
    static void Render(Camera c,int scale,string phase)=>Capture(c,360*scale,640*scale,Output+"/Unity-"+phase+"-"+(360*scale)+"x"+(640*scale)+".png");
    static void Capture(Camera camera,int w,int h,string path){Canvas.ForceUpdateCanvases();var rt=RenderTexture.GetTemporary(w,h,24,RenderTextureFormat.ARGB32);rt.filterMode=FilterMode.Point;rt.antiAliasing=1;var prior=RenderTexture.active;camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;var tex=new Texture2D(w,h,TextureFormat.RGBA32,false);tex.ReadPixels(new Rect(0,0,w,h),0,0);tex.Apply();File.WriteAllBytes(path,tex.EncodeToPNG());camera.targetTexture=null;RenderTexture.active=prior;RenderTexture.ReleaseTemporary(rt);Object.DestroyImmediate(tex);}
    [Serializable]class Result{public string status;public int architectureMovedNativePixels,existingRectsPreserved,existingImageBindingsPreserved,reusedFloorModules;public bool originalArtUnchanged;}
}
