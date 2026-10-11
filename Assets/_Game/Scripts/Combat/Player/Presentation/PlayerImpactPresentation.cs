using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Observes accepted sequence beats. No targets, damage, randomness or board
// mutations are owned by this view; missing sprites never block completion.
[DisallowMultipleComponent]
public sealed class PlayerImpactPresentation : MonoBehaviour
{
    private PlayerActor player;
    private PlayerAbilityController controller;
    private SequentialBoardAbilityRuntime runtime;
    private BoardController board;
    private DungeonAttenderAbilityDefinition attender;
    private RicochetAbilityDefinition ricochet;
    private SpriteRenderer avatar,marker;
    private Vector3 from,to,lastDirection=Vector3.right;
    private float elapsed,duration;
    private bool travelling,bounce,lastSurvived;
    private readonly List<GameObject> effects=new List<GameObject>();
    public bool HasBoardAvatar => avatar!=null && avatar.enabled;
    public int PresentedImpacts {get;private set;}

    private void OnEnable()
    {
        player=GetComponent<PlayerActor>();controller=GetComponent<PlayerAbilityController>();
        if(controller!=null)controller.AbilityActivated+=Activated;
    }
    private void OnDisable()
    {
        if(controller!=null)controller.AbilityActivated-=Activated;
        Unbind();Clear();
    }
    private void Activated()
    {
        Unbind();Clear();
        foreach(var candidate in GetComponents<SequentialBoardAbilityRuntime>())
            if(candidate.IsActive){runtime=candidate;break;}
        if(runtime==null)return;
        board=RunSession.Current?.Board??FindFirstObjectByType<BoardController>();
        attender=player.ActiveAbility as DungeonAttenderAbilityDefinition;
        ricochet=player.ActiveAbility as RicochetAbilityDefinition;
        runtime.Approaching+=Approaching;runtime.Striking+=Striking;runtime.Impacted+=Impacted;
        runtime.StateChanged+=StateChanged;
        if(runtime is RicochetRuntime gunner)gunner.RicochetHit+=Ricochet;
        PresentedImpacts=0;
        CombatAudioController.PlayMechanism(attender!=null?CombatSoundCue.AttenderJump:CombatSoundCue.KoboldSpin);
        if(attender!=null)StartCoroutine(LeapFromPortrait());
    }
    private void Unbind()
    {
        if(runtime!=null)
        {
            runtime.Approaching-=Approaching;runtime.Striking-=Striking;runtime.Impacted-=Impacted;
            runtime.StateChanged-=StateChanged;
            if(runtime is RicochetRuntime gunner)gunner.RicochetHit-=Ricochet;
        }
        runtime=null;
    }
    private SpriteRenderer MakeBoardSprite(string name,Sprite sprite,int order)
    {
        if(board==null || sprite==null)return null;
        var go=new GameObject(name);go.transform.SetParent(board.transform,false);
        var sr=go.AddComponent<SpriteRenderer>();sr.sprite=sprite;sr.sortingLayerName="Gems";sr.sortingOrder=order;
        sr.maskInteraction=SpriteMaskInteraction.VisibleInsideMask;effects.Add(go);return sr;
    }
    private Vector3 Position(Vector2Int cell)=>board.GetCellLocalPosition(cell.x,cell.y);
    private Vector3 Snap(Vector3 point)
    {
        float pixels=64f/Mathf.Max(.01f,board.CellSize);
        return new Vector3(Mathf.Round(point.x*pixels)/pixels,Mathf.Round(point.y*pixels)/pixels,point.z);
    }
    private void Approaching(int step,Vector2Int cell)
    {
        if(board==null)return;
        if(attender!=null)
        {
            if(avatar==null)
            {
                avatar=MakeBoardSprite("AttenderBoardAvatar",attender.BoardMarker,65);
                marker=MakeBoardSprite("AttenderLandingMarker",attender.BoardMarker,64);
                if(avatar!=null)avatar.transform.localScale=Vector3.one*2;
                if(marker!=null)marker.transform.localScale=Vector3.one*2;
            }
            to=Position(cell);from=step==0?to+Vector3.up*board.CellSize:avatar!=null?avatar.transform.localPosition:to;
            if(step>0 && (to-from).sqrMagnitude>.001f)lastDirection=(to-from).normalized;
            bounce=step>0 && lastSurvived;elapsed=0;
            duration=step==0?attender.LandingSeconds:attender.StepSeconds;
            travelling=true;
            if(avatar!=null){avatar.enabled=true;avatar.transform.localPosition=Snap(from);}
            if(marker!=null){marker.enabled=step==0;marker.transform.localPosition=to;}
        }
        else if(ricochet!=null)
        {
            var image=FindPlayerImage();
            if(image!=null)StartCoroutine(Tracer(ScreenPoint(image.rectTransform),BoardScreen(cell),ricochet.ShotSeconds));
        }
    }
    private void Update()
    {
        if(!travelling || avatar==null || board==null)return;
        elapsed+=Time.deltaTime;float t=duration<=0?1:Mathf.Clamp01(elapsed/duration);
        Vector3 point;
        if(bounce)
        {
            var recoil=from-lastDirection*board.CellSize*.22f;
            point=t<.4f?Vector3.Lerp(from,recoil,t/.4f):Vector3.Lerp(recoil,to,(t-.4f)/.6f);
        }
        else point=Vector3.Lerp(from,to,t);
        avatar.transform.localPosition=Snap(point);
        if(marker!=null && marker.enabled)marker.color=new Color(1,.85f,.4f,Mathf.FloorToInt(t*4)%2==0?.75f:.35f);
    }
    private void Striking(int step,Vector2Int cell)
    {
        PresentedImpacts++;
        if(board==null)return;
        if(attender!=null)
        {
            travelling=false;if(avatar!=null)avatar.transform.localPosition=Position(cell);
            if(marker!=null)marker.enabled=false;
            CombatAudioController.PlayMechanism(step==0?CombatSoundCue.AttenderCrash:CombatSoundCue.AttenderStep);
            StartCoroutine(Burst(cell,attender.ImpactFrames));
        }
        else
        {
            CombatAudioController.PlayMechanism(CombatSoundCue.KoboldShot);
            StartCoroutine(Burst(cell,ricochet?.ImpactFrames));
        }
    }
    private void Impacted(int step,Vector2Int cell,bool survived)=>lastSurvived=survived;
    private void Ricochet(int step,Vector2Int cell,EnemyActor enemy,int damage)
    {
        if(board==null || enemy==null)return;
        CombatAudioController.PlayMechanism(CombatSoundCue.KoboldRicochet);
        var target=enemy.transform as RectTransform;
        if(target!=null)StartCoroutine(Tracer(BoardScreen(cell),ScreenPoint(target),.08f));
    }
    private IEnumerator Burst(Vector2Int cell,Sprite[] frames)
    {
        if(frames==null || frames.Length==0 || board==null)yield break;
        var sr=MakeBoardSprite("PlayerImpactBurst",frames[0],68);if(sr==null)yield break;
        sr.transform.localPosition=Position(cell);
        for(int i=0;i<frames.Length;i++)
        {
            if(sr==null)yield break;sr.sprite=frames[i];yield return new WaitForSeconds(.04f);
        }
        if(sr!=null){effects.Remove(sr.gameObject);Destroy(sr.gameObject);}
    }
    private Image FindPlayerImage()
    {
        foreach(var image in FindObjectsByType<Image>(FindObjectsSortMode.None))if(image.name=="PlayerCharacter")return image;
        return null;
    }
    private IEnumerator LeapFromPortrait()
    {
        var source=FindPlayerImage();if(source==null || source.canvas==null)yield break;
        var canvas=source.canvas.rootCanvas;
        var go=new GameObject("AttenderLeap",typeof(RectTransform),typeof(Image),typeof(Animator));effects.Add(go);
        var rect=(RectTransform)go.transform;rect.SetParent(canvas.transform,false);
        var corners=new Vector3[4];source.rectTransform.GetWorldCorners(corners);
        Vector3 a=canvas.transform.InverseTransformPoint(corners[0]),b=canvas.transform.InverseTransformPoint(corners[2]);
        rect.sizeDelta=new Vector2(b.x-a.x,b.y-a.y);rect.anchoredPosition=(a+b)*.5f;
        var image=go.GetComponent<Image>();image.sprite=source.sprite;image.raycastTarget=false;image.preserveAspect=true;
        var animator=go.GetComponent<Animator>();animator.runtimeAnimatorController=player.Definition.BattleAnimatorController;
        animator.Play("Base Layer.Ability",0,0);animator.Update(0);
        Vector2 start=rect.anchoredPosition;
        for(float elapsed=0;elapsed<attender.JumpSeconds;elapsed+=Time.deltaTime)
        {
            float t=elapsed/Mathf.Max(.01f,attender.JumpSeconds);
            rect.anchoredPosition=start+Vector2.up*(PresentationPreferences.ReducedMotion?0:rect.rect.height*t*t);
            GameplayPixelGrid.Snap(rect);
            image.color=new Color(1,1,1,Mathf.Clamp01((1-t)*4));yield return null;
        }
        effects.Remove(go);Destroy(go);
    }
    private Vector2 BoardScreen(Vector2Int cell)=>Camera.main!=null?(Vector2)Camera.main.WorldToScreenPoint(board.transform.TransformPoint(Position(cell))):Vector2.zero;
    private static Vector2 ScreenPoint(RectTransform rect)
    {
        var canvas=rect.GetComponentInParent<Canvas>();
        return RectTransformUtility.WorldToScreenPoint(canvas!=null && canvas.renderMode!=RenderMode.ScreenSpaceOverlay?canvas.worldCamera:null,rect.TransformPoint(rect.rect.center));
    }
    private IEnumerator Tracer(Vector2 start,Vector2 end,float seconds)
    {
        var image=FindPlayerImage();var canvas=image!=null?image.canvas.rootCanvas:null;
        if(canvas==null)yield break;
        var go=new GameObject("RicochetTracer",typeof(RectTransform),typeof(Image));effects.Add(go);
        var rect=(RectTransform)go.transform;rect.SetParent(canvas.transform,false);rect.sizeDelta=new Vector2(4,4);
        var dot=go.GetComponent<Image>();dot.color=new Color32(245,200,65,255);dot.raycastTarget=false;
        Camera camera=canvas.renderMode==RenderMode.ScreenSpaceOverlay?null:canvas.worldCamera;
        for(float t=0;t<seconds;t+=Time.deltaTime)
        {
            Vector2 point=PresentationPreferences.ReducedMotion?end:Vector2.Lerp(start,end,Mathf.Clamp01(t/Mathf.Max(.01f,seconds)));
            RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)canvas.transform,point,camera,out var local);
            rect.anchoredPosition=local;GameplayPixelGrid.Snap(rect);yield return null;
        }
        effects.Remove(go);Destroy(go);
    }
    private void StateChanged()
    {
        if(runtime==null || !runtime.IsActive){Clear();return;}
        if(runtime.AnimationState=="Recovery")
        {travelling=false;if(avatar!=null)avatar.enabled=false;if(marker!=null)marker.enabled=false;}
    }
    private void Clear()
    {
        StopAllCoroutines();foreach(var effect in effects)if(effect!=null)Destroy(effect);
        effects.Clear();avatar=null;marker=null;travelling=false;lastSurvived=false;
    }
}
