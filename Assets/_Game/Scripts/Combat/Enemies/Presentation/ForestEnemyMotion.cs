using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Optional sprite playback follows authoritative channel outcomes.</summary>
[DefaultExecutionOrder(11100)]
[DisallowMultipleComponent]
public sealed class ForestEnemyMotion : MonoBehaviour
{
    private EnemyActor actor;
    private Animator animator;
    private EnemyChannelRuntime mender;
    private ForestMilestoneEnemyAbility milestone;
    private RootbinderEnemyAbility roots;
    private bool wasChanneling;
    private string pendingState;
    private float nextStateAt;
    private void Start()
    {
        actor=GetComponent<EnemyActor>();animator=transform.Find("VisualRoot")?.GetComponent<Animator>();
        mender=GetComponent<EnemyChannelRuntime>();milestone=GetComponent<ForestMilestoneEnemyAbility>();roots=GetComponent<RootbinderEnemyAbility>();
        if(actor!=null) { actor.DamageReceived+=Hit;actor.Healed+=Healed; }
        if(mender!=null) mender.Changed+=StateChanged;
        if(milestone!=null) milestone.Changed+=StateChanged;
        if(roots!=null) roots.Changed+=StateChanged;
        StateChanged();
    }
    private bool Channeling => mender?.IsChanneling==true || milestone?.IsPreparing==true || roots?.IsWarning==true;
    private string Outcome => mender!=null?mender.Outcome:milestone!=null?milestone.Outcome:roots?.Outcome;
    private void StateChanged()
    {
        if(actor==null || actor.IsDefeated) return;
        bool casting=Channeling;
        if(casting && !wasChanneling) PlayThen("ChannelStart","ChannelHold");
        else if(!casting && wasChanneling)
        {
            bool released=Outcome=="Healed" || Outcome=="Renewed" || Outcome=="Planted" || Outcome=="Surged" || Outcome=="Harvested";
            PlayThen(released?"Release":"Interrupt","Idle");
            if(!released) Burst(GameplayThemeSkin.Current?.interruptEffect);
        }
        wasChanneling=casting;
    }
    private void LateUpdate()
    {
        if(actor==null || actor.IsDefeated || Time.timeScale<=0) return;
        // An owned action supersedes any earlier hit recovery. Its presenter
        // decides whether to resume idle or a held warning pose.
        if(actor.HasAnimationActionInProgress) { pendingState=null;return; }
        if(pendingState!=null && Time.time>=nextStateAt && !actor.HasAnimationActionInProgress)
        {
            string state=pendingState;pendingState=null;
            if(state=="ChannelHold" && !Channeling) return;
            Play(state);
        }
    }
    private void Hit(EnemyActor owner,int amount)
    {
        // Small hits cannot cancel a pending heal or replace an owned attack.
        if(amount>0 && !actor.IsDefeated && !actor.HasAnimationActionInProgress && !Channeling &&
            string.IsNullOrEmpty(actor.SpecialIdleState))
            PlayThen("Hit","Idle");
    }
    private void Healed(EnemyActor owner,int amount) { if(amount>0) Burst(GameplayThemeSkin.Current?.healEffect); }
    private void Burst(Sprite sprite)
    {
        if(sprite==null || !isActiveAndEnabled) return;
        StartCoroutine(ShowBurst(sprite));
    }
    private IEnumerator ShowBurst(Sprite sprite)
    {
        var visual=transform.Find("VisualRoot") as RectTransform;
        var character=visual!=null?visual.GetComponent<Image>():null;
        if(character?.sprite==null) yield break;
        float pixel=visual.rect.width/character.sprite.rect.width;
        var rect=GameUi.Rect("ForestEffect",visual,sprite.rect.size*pixel,new Vector2(0,8*pixel));
        var image=rect.gameObject.AddComponent<Image>();image.sprite=sprite;image.raycastTarget=false;
        float began=Time.time;
        while(Time.time<began+.45f && rect!=null)
        {
            float t=(Time.time-began)/.45f;
            rect.anchoredPosition=new Vector2(0,(8+Mathf.Round(t*10))*pixel);
            image.color=new Color(1,1,1,1-t);yield return null;
        }
        if(rect!=null) Destroy(rect.gameObject);
    }
    private bool Play(string state)
    {
        int hash=Animator.StringToHash("Base Layer."+state);
        if(animator==null || !animator.isActiveAndEnabled || animator.runtimeAnimatorController==null || !animator.HasState(0,hash)) return false;
        animator.Play(hash,0,0);return true;
    }
    private void PlayThen(string state,string next)
    {
        if(!Play(state)) return;
        pendingState=next;nextStateAt=Time.time+Duration(state);
    }
    public float Duration(string state)
    {
        if(animator==null) animator=transform.Find("VisualRoot")?.GetComponent<Animator>();
        if(animator?.runtimeAnimatorController!=null)
            foreach(var clip in animator.runtimeAnimatorController.animationClips)
                if(clip.name.EndsWith("_"+state)) return Mathf.Clamp(clip.length,.05f,1.2f);
        return .35f;
    }
    public float PlayDeath() { pendingState=null;return Play("Death")?Duration("Death"):0; }
    private void OnDestroy()
    {
        if(actor!=null) { actor.DamageReceived-=Hit;actor.Healed-=Healed; }
        if(mender!=null) mender.Changed-=StateChanged;
        if(milestone!=null) milestone.Changed-=StateChanged;
        if(roots!=null) roots.Changed-=StateChanged;
    }
}
