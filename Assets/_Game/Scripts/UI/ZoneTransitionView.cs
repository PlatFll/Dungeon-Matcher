using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>Crystal/smoke presentation. The caller owns destination selection and saving.</summary>
public sealed class ZoneTransitionView : MonoBehaviour
{
    public static ZoneTransitionView Active { get; private set; }
    public static ZoneTransitionView Create(RunSession owner)
    {
        if(Active!=null) return Active;
        var go=new GameObject("ZoneTravelPresentation",typeof(RectTransform));
        DontDestroyOnLoad(go);
        var view=go.AddComponent<ZoneTransitionView>();view.run=owner;view.controls=owner.GetComponent<RunControlsUI>();
        return view;
    }
    public bool IsPlaying { get; private set; }
    public string Phase { get; private set; } = "Idle";
    public float Cover { get; private set; }
    public bool PreparationSucceeded { get; private set; }
    private RunSession run;
    private RunControlsUI controls;
    private IDisposable input;
    private RectTransform root,crystal;
    private Image veil,crystalImage;
    private TMP_Text title;
    private Canvas canvas;
    private float priorScale,elapsed;
    private readonly List<RectTransform> clouds=new List<RectTransform>(),sparks=new List<RectTransform>();
    private readonly List<Vector2> cloudPositions=new List<Vector2>();

    public bool TryPlay(string destinationName,Func<bool> prepareUnderCover=null,bool revealOnly=false)
    {
        if(IsPlaying) return false;
        if(run==null || run.IsFinished || controls?.GameplayCanvas==null || controls.IsModalOpen ||
            run.Board.IsBusy || run.Board.IsExternalInputBlocked || run.Continuation?.IsRestoring==true) return false;
        IsPlaying=true;Active=this;PreparationSucceeded=false;elapsed=0;
        priorScale=Time.timeScale;Time.timeScale=0;
        input=run.Board.AcquireExternalInputBlock();run.CancelTargeting();
        Build(destinationName);StartCoroutine(Play(prepareUnderCover,revealOnly));return true;
    }
    private static RectTransform Stretch(string name,Transform parent)
    {
        var rect=GameUi.Rect(name,parent,Vector2.zero,Vector2.zero);
        rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=rect.offsetMax=Vector2.zero;return rect;
    }
    private void Build(string destinationName)
    {
        var source=controls.GameplayCanvas;
        root=(RectTransform)transform;root.name="ZoneTransition";
        canvas=root.gameObject.AddComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder=source.sortingOrder+200;
        var scaler=root.gameObject.AddComponent<CanvasScaler>();var original=source.GetComponent<CanvasScaler>();
        if(original!=null) {scaler.uiScaleMode=original.uiScaleMode;scaler.referenceResolution=original.referenceResolution;
            scaler.screenMatchMode=original.screenMatchMode;scaler.matchWidthOrHeight=original.matchWidthOrHeight;scaler.scaleFactor=original.scaleFactor;}
        root.gameObject.AddComponent<GraphicRaycaster>();root.gameObject.AddComponent<Image>().color=Color.clear;
        veil=Stretch("OpaqueSmokeCoverage",root).gameObject.AddComponent<Image>();veil.raycastTarget=false;veil.color=Color.clear;
        var smoke=Resources.Load<Sprite>("UI/Transition/Smoke");
        for(int y=0;y<13;y++) for(int x=0;x<7;x++)
        {
            var cloud=GameUi.Rect("Smoke_"+x+"_"+y,root,Vector2.zero,Vector2.zero);
            var image=cloud.gameObject.AddComponent<Image>();image.sprite=smoke;image.raycastTarget=false;image.color=Color.clear;
            clouds.Add(cloud);cloudPositions.Add(new Vector2(x/6f-.5f+(y%2==0?-.025f:.025f),y/12f-.5f));
        }
        for(int i=0;i<18;i++)
        {
            var spark=GameUi.Rect("PinkMote_"+i,root,new Vector2(3,3),Vector2.zero);
            var image=spark.gameObject.AddComponent<Image>();image.color=Color.clear;image.raycastTarget=false;sparks.Add(spark);
        }
        crystal=GameUi.Rect("SplitStoryGem",root,Vector2.zero,Vector2.zero);
        crystalImage=crystal.gameObject.AddComponent<Image>();crystalImage.sprite=Resources.Load<Sprite>("UI/Finalized/SplitStoryGem");
        crystalImage.preserveAspect=true;crystalImage.raycastTarget=false;
        title=GameUi.Label("Destination",root,destinationName,new Vector2(430,95),Vector2.zero,32);title.color=Color.clear;
    }
    private IEnumerator Play(Func<bool> prepare,bool revealOnly)
    {
        try
        {
            if(!revealOnly)
            {
                Phase="Charging";yield return Animate(.55f,t=>Draw(0,t,0));
                Phase="Covering";yield return Animate(.4f,t=>Draw(t,1,0));
            }
            Phase="Covered";Draw(1,1,0);
            while(controls!=null && controls.IsModalOpen) yield return null;
            try { PreparationSucceeded=prepare==null || prepare(); }
            catch(Exception exception) { Debug.LogException(exception);PreparationSucceeded=false; }
            if(!PreparationSucceeded) title.text="Travel interrupted";
            yield return null;
            // Scene setup and restoration finish under opaque smoke. A failed
            // restore keeps the standard recovery/settings UI above the curtain.
            while(PreparationSucceeded && (RunSession.Current==null || !RunSession.Current.InitialStateReady ||
                RunSession.Current.Continuation.IsRestoring))
            { Rebind();yield return null; }
            Rebind();Time.timeScale=0;
            Phase="Revealing";yield return Animate(.45f,t=>Draw(1-t,1-t,t));
            Phase="Title";yield return Animate(.7f,t=>Draw(0,0,1-t));
        }
        finally { Release(); }
    }
    private void Rebind()
    {
        if(RunSession.Current==null || RunSession.Current==run) return;
        input?.Dispose();run=RunSession.Current;controls=run.GetComponent<RunControlsUI>();
        input=run.Board.AcquireExternalInputBlock();
    }
    private void Update()
    {
        if(IsPlaying && SceneManager.GetActiveScene().name!="Game") {StopAllCoroutines();Release();}
    }
    private IEnumerator Animate(float seconds,Action<float> draw)
    {
        float time=0;
        while(time<seconds)
        {
            if(controls==null || run==null || run.IsFinished) yield break;
            if(!controls.IsModalOpen)
            {time+=Time.unscaledDeltaTime;elapsed+=Time.unscaledDeltaTime;draw(Mathf.Clamp01(time/seconds));}
            yield return null;
        }
        draw(1);
    }
    private void Draw(float cover,float gem,float label)
    {
        if(root==null) return;
        Cover=cover;bool reduced=PresentationPreferences.ReducedMotion;
        float scale=Mathf.Max(.001f,canvas.scaleFactor),pixel=1/scale;
        float native=crystalImage.sprite!=null?crystalImage.sprite.rect.width:40;
        float target=Mathf.Max(1,Mathf.Floor(Screen.width*.30f/native));
        float zoom=reduced?target:Mathf.Max(1,Mathf.Round(Mathf.Lerp(target*.55f,target,gem)));
        crystal.sizeDelta=new Vector2(native,native)*zoom/scale;
        crystal.anchoredPosition=reduced?Vector2.zero:new Vector2(Mathf.Round(Mathf.Sin(elapsed*91)*gem*5),Mathf.Round(Mathf.Cos(elapsed*73)*gem*3))*pixel;
        crystalImage.color=new Color(1,1,1,gem>0?1:0);veil.color=new Color(.20f,.15f,.25f,cover);
        Vector2 extent=new Vector2(Screen.width/scale,Screen.height/scale);
        float size=Mathf.Ceil(Screen.width/6f/64)*64/scale;
        for(int i=0;i<clouds.Count;i++)
        {
            Vector2 end=Vector2.Scale(cloudPositions[i],extent);
            Vector2 start=end+(end.sqrMagnitude>1?end.normalized:new Vector2(0,-1))*size*1.6f;
            var cloud=clouds[i];cloud.sizeDelta=Vector2.one*(size+64*(i%3)/scale);
            cloud.localScale=new Vector3(i%2==0?1:-1,1,1);
            var p=reduced?end:Vector2.Lerp(start,end,cover);
            cloud.anchoredPosition=new Vector2(Mathf.Round(p.x*scale)/scale,Mathf.Round(p.y*scale)/scale);
            cloud.GetComponent<Image>().color=new Color(1,1,1,Mathf.Clamp01(cover*2));
        }
        for(int i=0;i<sparks.Count;i++)
        {
            float angle=i*Mathf.PI*2/sparks.Count,radius=(35+(elapsed*65+i*13)%100)*gem;
            sparks[i].anchoredPosition=new Vector2(Mathf.Round(Mathf.Cos(angle)*radius),Mathf.Round(Mathf.Sin(angle)*radius));
            sparks[i].GetComponent<Image>().color=new Color(1,.4f,.7f,reduced?0:gem*.8f);
        }
        title.rectTransform.anchoredPosition=new Vector2(0,-extent.y*.12f);title.color=new Color(1,.86f,.94f,label);
    }
    private void Release()
    {
        if(!IsPlaying) return;
        IsPlaying=false;Phase="Idle";Cover=0;input?.Dispose();input=null;
        if(Active==this) Active=null;
        clouds.Clear();cloudPositions.Clear();sparks.Clear();
        if(controls!=null && controls.IsModalOpen) controls.RestoreTimeAfterTransition(priorScale);
        else if(Time.timeScale==0) Time.timeScale=priorScale;
        Destroy(gameObject);
    }
    private void OnDisable() {StopAllCoroutines();Release();}
}
