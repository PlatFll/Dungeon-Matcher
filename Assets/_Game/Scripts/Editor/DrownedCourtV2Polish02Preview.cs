using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Object=UnityEngine.Object;

// Candidate-only scene polish. Uses native Unity clips/Animators; no gameplay or production binding changes.
public static class DrownedCourtV2Polish02Preview
{
    const string Root="ArtSource/DrownedCourtEnvironmentV2";
    const string Art="Assets/_Game/Art/DrownedCourtEnvironmentV2/Revision02";
    const string Scene="Assets/_Game/Scenes/ArtPreview_DrownedCourtV2_Revision02.unity";
    const string Motion="Assets/_Game/Art/DrownedCourtEnvironmentV2/Revision02/Motion";
    static Manifest data; static Canvas canvas; static Camera camera; static int captures;
    static readonly List<KeyValuePair<GameObject,AnimationClip>> clips=new();
    [MenuItem("Dungeon Matcher/Art/Drowned Court V2/Build Polish Revision 02")]
    public static void Build()
    {
        Assert.That(Application.isPlaying,Is.False);captures=0;
        data=JsonUtility.FromJson<Manifest>(File.ReadAllText(Root+"/Native/Revision02/Manifest.json"));Import();
        Directory.CreateDirectory(Motion);Directory.CreateDirectory(Root+"/Preview/Revision02/Motion");
        foreach(int height in new[]{640,800})
        {
            clips.Clear();EditorSceneManager.OpenScene("Assets/_Game/Scenes/ArtPreview_DrownedCourtV2_Revision01.unity");
            canvas=Object.FindFirstObjectByType<Canvas>();camera=Object.FindFirstObjectByType<Camera>();
            canvas.transform.Find("Stage01_ReadOnlyActorContrast").gameObject.SetActive(true);
            if(height==800)Tall();
            foreach(int scale in new[]{1,2,3})Render(360*scale,height*scale,"Unity-Before-"+(360*scale)+"x"+(height*scale)+".png");
            var oldRects=canvas.GetComponentsInChildren<RectTransform>(true).ToDictionary(t=>t,t=>RectRecord(t));
            foreach(var image in canvas.GetComponentsInChildren<Image>(true))
            {
                if(!image.sprite)continue;var old=AssetDatabase.GetAssetPath(image.sprite);var replacement=data.replacements.FirstOrDefault(r=>r.oldAsset==old);
                if(replacement!=null)image.sprite=Sprite(replacement.name);
            }
            var masonry=Rect(canvas.transform,"Revision02_QuietMasonry",new Rect(0,0,360,height));masonry.localScale=new Vector3(2,2,1);
            masonry.SetSiblingIndex(canvas.transform.Find("BlankOceanExtent").GetSiblingIndex()+1);masonry.gameObject.AddComponent<RectMask2D>();
            canvas.transform.Find("BlankLowerSurroundings").gameObject.SetActive(false);
            for(int y=240;y<height;y+=32)for(int x=0;x<360;x+=64)
            {
                bool growth=(x==64&&y==height-80)||(x==256&&y==height-112);
                ImageAt(masonry,growth?"MasonryGrowth":"OceanMasonry",x,y,64,32,height);
            }
            var fx=Rect(canvas.transform,"Revision02_Ambient",new Rect(0,0,360,height));fx.localScale=new Vector3(2,2,1);
            fx.SetSiblingIndex(canvas.transform.Find("Stage05_BoardReview").GetSiblingIndex());
            Clam(fx,54,height-44,height);
            Bubble(fx,"TopLeft",false,113,179,85,8,0,4.8f,height);
            Bubble(fx,"TopCenter",true,181,131,59,12,3.1f,6.1f,height);
            Bubble(fx,"TopRight",false,298,148,65,8,4.4f,4.4f,height);
            Bubble(fx,"ArenaEdge",true,346,185,79,12,7.2f,6.2f,height);
            int dy=height==800?63:0;
            Bubble(fx,"BoardLeftHigh",false,1,418+dy,277+dy,12,1.6f,7.5f,height);
            Bubble(fx,"BoardLeftLow",false,2,535+dy,443+dy,8,5.3f,5,height);
            Bubble(fx,"BoardRightHigh",true,346,423+dy,300+dy,12,6.7f,6.5f,height);
            Bubble(fx,"BoardRightLow",false,350,532+dy,451+dy,8,2.8f,4.9f,height);
            Bubble(fx,"BottomLeft",false,84,height-10,height-87,12,1.7f,5.8f,height);
            Bubble(fx,"BottomRight",true,277,height-12,height-84,12,8.2f,6.1f,height);
            Bubble(fx,"ClamReleaseA",false,74,height-30,height-70,12,-4.9f,2.5f,height,true);
            Bubble(fx,"ClamReleaseB",false,80,height-29,height-66,12,-5.7f,2.5f,height,true);
            foreach(var pair in oldRects)Assert.That(RectRecord(pair.Key),Is.EqualTo(pair.Value),pair.Key.name+" layout changed");
            Assert.That(Object.FindObjectsByType<BoardController>(FindObjectsSortMode.None),Is.Empty);
            Assert.That(EditorBuildSettings.scenes.Any(s=>s.path==Scene),Is.False);
            Evaluate(5.9f);
            if(height==640)Assert.That(EditorSceneManager.SaveScene(canvas.gameObject.scene,Scene),Is.True);
            foreach(int scale in new[]{1,2,3})Render(360*scale,height*scale,"Unity-Revised-"+(360*scale)+"x"+(height*scale)+".png");
            // Exact clips sampled for the motion preview, rather than a CSS approximation of the scene.
            if(height==640)for(int f=0;f<288;f++){Evaluate(f/12f);Render(360,640,"Motion/Frame-"+f.ToString("000")+".png");}
            Evaluate(0);Render(360,height,"LoopStart-"+height+".png");Evaluate(24);Render(360,height,"LoopEnd-"+height+".png");
            Evaluate(0);var state0=MotionState();Evaluate(1);var state1=MotionState();Assert.That(state0,Is.Not.EqualTo(state1));
            // Mask probe reuses the already-overfilled portrait clipping structure.
            var recess=canvas.GetComponentsInChildren<Image>().First(im=>im.sprite&&im.sprite.name=="PortraitWallA");var sp=recess.sprite;
            recess.sprite=null;recess.type=Image.Type.Simple;recess.color=Color.magenta;Render(360,height,"PortraitMaskProbe-"+height+".png");recess.sprite=sp;recess.type=Image.Type.Tiled;recess.color=Color.white;
        }
        AssetDatabase.SaveAssets();File.WriteAllText(Root+"/QA/Revision02/UnityPreview.json",JsonUtility.ToJson(new Result{status="PASS",captures=captures,assets=data.assets.Length,motionTracks=13,loopFrames=288,note="Actual art-only Unity renders and sampled native AnimationClips. Existing transforms preserved. No Game scene/gameplay or device test."},true));
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);EditorSceneManager.OpenScene(Scene);Debug.Log("DROWNED_COURT_POLISH02_PASS");
    }
    static string RectRecord(RectTransform r)=>JsonUtility.ToJson(new Bounds{p=r.anchoredPosition,s=r.sizeDelta,scale=r.localScale,rot=r.localRotation,amin=r.anchorMin,amax=r.anchorMax,pivot=r.pivot});
    static void Tall()
    {
        ((RectTransform)canvas.transform).sizeDelta=new Vector2(720,1600);
        foreach(Transform t in canvas.transform){var r=t as RectTransform;if(!r)continue;
            if(t.name=="BlankOceanExtent")r.sizeDelta=new Vector2(720,1600);
            else if(t.name=="BlankLowerSurroundings")r.sizeDelta=new Vector2(712,240);
            else if(t.name=="Stage01_SeparateBackgroundLayers"||t.name=="Stage03_Platform_Candidates"||t.name=="Stage02_Architecture_Candidates"||t.name.StartsWith("PortraitInterior")||t.name=="ExistingBattleFloorGuide"||t.name=="Stage01_ReadOnlyActorContrast"||t.name.StartsWith("Stage04_"))r.anchoredPosition+=Vector2.up*320;
            else if(t.name.StartsWith("GridGuide")||t.name.StartsWith("PlayableBoardBounds")||t.name=="Stage05_BoardReview"||t.name=="Stage06_SideArchitecture")r.anchoredPosition+=Vector2.up*194;
            else if(t.name.StartsWith("UnchangedSharedUIShells"))foreach(RectTransform child in t)if(child.anchoredPosition.y>796)child.anchoredPosition+=Vector2.up*320;
        }
        foreach(var frame in canvas.GetComponentsInChildren<ResponsiveModularFrameFitter>())frame.RefreshFrame();
        var sides=JsonUtility.FromJson<Manifest>(File.ReadAllText(Root+"/Native/Revision01/Manifest.json"));Populate((RectTransform)canvas.transform.Find("Revision01_IntegratedSides"),sides.placementsTall,800,"Assets/_Game/Art/DrownedCourtEnvironmentV2/Revision01","");
        var reef=JsonUtility.FromJson<Manifest>(File.ReadAllText(Root+"/Native/Stage07/Manifest.json"));Populate((RectTransform)canvas.transform.Find("Stage07_LowerReef"),reef.placementsTall,800,"Assets/_Game/Art/DrownedCourtEnvironmentV2/Revision01","S07_");
        camera.orthographicSize=800;camera.transform.position=new Vector3(360,800,-10);
    }
    static void Populate(RectTransform root,Placement[] places,int height,string art,string prefix){for(int i=root.childCount-1;i>=0;i--)Object.DestroyImmediate(root.GetChild(i).gameObject);root.sizeDelta=new Vector2(360,height);foreach(var p in places){var r=Rect(root,p.name,new Rect(p.x,height-p.y-p.height,p.width,p.height));var im=r.gameObject.AddComponent<Image>();im.sprite=AssetDatabase.LoadAssetAtPath<Sprite>(art+"/"+prefix+p.name+".png");Assert.That(im.sprite,Is.Not.Null);im.raycastTarget=false;}}
    static void Bubble(Transform root,string name,bool medium,int x,int y,int target,int period,float phase,float active,int height,bool clam=false)
    {
        int size=medium?12:8;var image=ImageAt(root,medium?"BubbleMedium0":"BubbleSmall0",x,y,size,size,height);image.name=name;
        var clip=new AnimationClip{name=name,frameRate=12};var xs=new List<Keyframe>();var ys=new List<Keyframe>();var alphas=new List<Keyframe>();var sprites=new List<ObjectReferenceKeyframe>();
        for(int f=0;f<=period*12;f++){
            float t=f/12f,q=f==period*12?Mathf.Repeat(phase,period):Mathf.Repeat(t+phase,period);bool on=q<active+.25f;float rise=Mathf.Clamp01(q/active);
            float X=x+Mathf.Round(Mathf.Sin((q+phase)*1.5f)*(medium?1:1));float Y=height-y-size+Mathf.Round((y-target)*rise);
            int state=q>=active+.12f?2:q>=active?1:0;
            xs.Add(new Keyframe(t,X));ys.Add(new Keyframe(t,Y));alphas.Add(new Keyframe(t,on?1:0));if(f<period*12)sprites.Add(new ObjectReferenceKeyframe{time=t,value=Sprite((medium?"BubbleMedium":"BubbleSmall")+state)});
        }
        Curve(clip,typeof(RectTransform),"m_AnchoredPosition.x",xs);Curve(clip,typeof(RectTransform),"m_AnchoredPosition.y",ys);Curve(clip,typeof(Image),"m_Color.a",alphas);AnimationUtility.SetObjectReferenceCurve(clip,EditorCurveBinding.PPtrCurve("",typeof(Image),"m_Sprite"),sprites.ToArray());Bind(image.gameObject,clip,height,period);
    }
    static void Clam(Transform root,int x,int y,int height)
    {
        var im=ImageAt(root,"Clam0",x,y,37,38,height);im.name="Clam";var clip=new AnimationClip{name="Clam",frameRate=12};
        float[] times={0,4,4.2f,4.4f,4.6f,4.8f,7.5f,7.7f,7.9f,8.1f,8.3f,12-1f/12};int[] frames={0,0,1,2,3,4,4,3,2,1,0,0};
        AnimationUtility.SetObjectReferenceCurve(clip,EditorCurveBinding.PPtrCurve("",typeof(Image),"m_Sprite"),times.Select((t,i)=>new ObjectReferenceKeyframe{time=t,value=Sprite("Clam"+frames[i])}).ToArray());Bind(im.gameObject,clip,height,12);
    }
    static void Curve(AnimationClip clip,Type type,string property,List<Keyframe> keys){var curve=new AnimationCurve(keys.ToArray());for(int i=0;i<curve.length;i++){AnimationUtility.SetKeyLeftTangentMode(curve,i,AnimationUtility.TangentMode.Constant);AnimationUtility.SetKeyRightTangentMode(curve,i,AnimationUtility.TangentMode.Constant);}AnimationUtility.SetEditorCurve(clip,EditorCurveBinding.FloatCurve("",type,property),curve);}
    static void Bind(GameObject go,AnimationClip clip,int height,float duration)
    {
        // Sprite keys occupy a full frame. Do not put a sprite key at the end timestamp:
        // that adds 1/12 second to the cycle and desynchronizes the clam release tracks.
        var settings=AnimationUtility.GetAnimationClipSettings(clip);settings.loopTime=true;settings.startTime=0;settings.stopTime=duration;AnimationUtility.SetAnimationClipSettings(clip,settings);
        Assert.That(clip.length,Is.EqualTo(duration).Within(.001f),go.name+" cycle duration");
        string path=Motion+"/"+clip.name+"-"+height+".anim";var existing=AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
        if(existing){EditorUtility.CopySerialized(clip,existing);Object.DestroyImmediate(clip);clip=existing;}else AssetDatabase.CreateAsset(clip,path);
        string cp=Motion+"/"+go.name+"-"+height+".controller";var ctrl=AssetDatabase.LoadAssetAtPath<AnimatorController>(cp);
        if(!ctrl)ctrl=AnimatorController.CreateAnimatorControllerAtPath(cp);var machine=ctrl.layers[0].stateMachine;
        var state=machine.states.Length>0?machine.states[0].state:machine.AddState("Ambient");state.motion=clip;machine.defaultState=state;
        var animator=go.AddComponent<Animator>();animator.runtimeAnimatorController=ctrl;animator.applyRootMotion=false;animator.cullingMode=AnimatorCullingMode.AlwaysAnimate;
        clips.Add(new KeyValuePair<GameObject,AnimationClip>(go,clip));
    }
    static void Evaluate(float t){foreach(var p in clips)p.Value.SampleAnimation(p.Key,Mathf.Repeat(t,p.Value.length));Canvas.ForceUpdateCanvases();}
    static string MotionState()=>string.Join(";",clips.Select(p=>p.Key.GetComponent<RectTransform>().anchoredPosition+":"+p.Key.GetComponent<Image>().sprite.name+":"+p.Key.GetComponent<Image>().color.a));
    static Image ImageAt(Transform p,string name,int x,int y,int w,int h,int height){var r=Rect(p,name,new Rect(x,height-y-h,w,h));var im=r.gameObject.AddComponent<Image>();im.sprite=Sprite(name);im.raycastTarget=false;Assert.That(im.sprite.rect.size,Is.EqualTo(r.rect.size));return im;}
    static RectTransform Rect(Transform p,string n,Rect b){var r=new GameObject(n,typeof(RectTransform)).GetComponent<RectTransform>();r.SetParent(p,false);r.anchorMin=r.anchorMax=r.pivot=Vector2.zero;r.anchoredPosition=b.position;r.sizeDelta=b.size;return r;}
    static Sprite Sprite(string name){var s=AssetDatabase.LoadAssetAtPath<Sprite>(Art+"/"+name+".png");Assert.That(s,Is.Not.Null,name);return s;}
    static void Import(){Directory.CreateDirectory(Art);foreach(var a in data.assets){string p=Art+"/"+a.name+".png";File.Copy(Root+"/Exports/Revision02/"+a.name+".png",p,true);AssetDatabase.ImportAsset(p,ImportAssetOptions.ForceSynchronousImport);var ti=(TextureImporter)AssetImporter.GetAtPath(p);ti.textureType=TextureImporterType.Sprite;ti.spriteImportMode=SpriteImportMode.Single;ti.spritePixelsPerUnit=64;ti.filterMode=FilterMode.Point;ti.textureCompression=TextureImporterCompression.Uncompressed;ti.mipmapEnabled=false;ti.npotScale=TextureImporterNPOTScale.None;ti.alphaSource=TextureImporterAlphaSource.FromInput;ti.alphaIsTransparency=true;ti.isReadable=true;ti.wrapMode=TextureWrapMode.Clamp;var st=new TextureImporterSettings();ti.ReadTextureSettings(st);st.spriteMeshType=SpriteMeshType.FullRect;ti.SetTextureSettings(st);ti.SaveAndReimport();var expected=new Texture2D(2,2);expected.LoadImage(File.ReadAllBytes(p));var actual=AssetDatabase.LoadAssetAtPath<Texture2D>(p);Assert.That(actual.width,Is.EqualTo(a.width));Assert.That(actual.height,Is.EqualTo(a.height));var aa=actual.GetPixels32();var bb=expected.GetPixels32();for(int i=0;i<aa.Length;i++)Assert.That(aa[i].a==bb[i].a&&(bb[i].a==0||aa[i].Equals(bb[i])),Is.True,a.name);Object.DestroyImmediate(expected);}}
    static void Render(int w,int h,string name){Canvas.ForceUpdateCanvases();var rt=RenderTexture.GetTemporary(w,h,24,RenderTextureFormat.ARGB32);rt.filterMode=FilterMode.Point;rt.antiAliasing=1;var prior=RenderTexture.active;camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;var tex=new Texture2D(w,h,TextureFormat.RGBA32,false);tex.ReadPixels(new Rect(0,0,w,h),0,0);tex.Apply();File.WriteAllBytes(Root+"/Preview/Revision02/"+name,tex.EncodeToPNG());captures++;camera.targetTexture=null;RenderTexture.active=prior;RenderTexture.ReleaseTemporary(rt);Object.DestroyImmediate(tex);}
    [Serializable]class Manifest{public Asset[] assets;public Replacement[] replacements;public Placement[] placementsTall;}
    [Serializable]class Asset{public string name;public int width,height;}
    [Serializable]class Replacement{public string name,oldAsset;}
    [Serializable]class Placement{public string name;public int x,y,width,height;}
    [Serializable]class Bounds{public Vector2 p,s,amin,amax,pivot;public Vector3 scale;public Quaternion rot;}
    [Serializable]class Result{public string status,note;public int captures,assets,motionTracks,loopFrames;}
}
