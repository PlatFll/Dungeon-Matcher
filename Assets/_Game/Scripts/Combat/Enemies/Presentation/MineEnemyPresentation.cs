using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Observes ore and authored contacts; never owns a combat effect or timer.</summary>
public sealed class MineEnemyPresentation : MonoBehaviour
{
    private EnemyActor actor;
    private EnemyAutoAttack attack;
    private EnemyOrePower ore;
    private MineEnemyAbility kit;
    private Image spark;
    private bool powered;
    private float pulseUntil;
    public void Initialize(EnemyActor owner)
    {
        actor=owner;attack=GetComponent<EnemyAutoAttack>();ore=GetComponent<EnemyOrePower>();kit=GetComponent<MineEnemyAbility>();
        if(attack!=null)attack.AttackResolved+=Contact;
        if(ore!=null){powered=ore.IsPowered;ore.Changed+=PowerChanged;}
        if(kit!=null)kit.BossPhaseChanged+=Phase;
        actor.AbilityCastCommitted+=Cast;actor.SpecialMotionRequested+=Motion;
    }
    private void PowerChanged()
    {
        bool next=ore.IsPowered;
        if(next && !powered && CombatAudioController.FeedbackAllowed)
        {pulseUntil=Time.time+.35f;CombatAudioController.PlayMechanism(CombatSoundCue.MineOre);}
        powered=next;
    }
    private void Contact(EnemyAutoAttack _,int damage,bool followup)
    {
        CombatAudioController.PlayMechanism(actor.Definition.mineBasicSound);
        if(ore?.IsCurrentSequencePowered==true && CombatAudioController.FeedbackAllowed)pulseUntil=Time.time+.25f;
    }
    private void Cast(EnemyActor _,string name)
    {
        if(actor.Definition.SpecialAbilityKind==EnemySpecialAbilityKind.Faultline)
            CombatAudioController.PlayMechanism(CombatSoundCue.MineHammer);
        else if(actor.Definition.SpecialAbilityKind==EnemySpecialAbilityKind.SiegeMachinist)
            CombatAudioController.PlayMechanism(CombatSoundCue.MinePiston);
    }
    private void Motion(EnemyActor _)
    {
        if(actor.SpecialMotionState=="SlamRelease")StartCoroutine(SlamContact(actor.SpecialMotionId));
    }
    private IEnumerator SlamContact(int id)
    {
        yield return actor.WaitForSpecialMotionBeat(id);
        if(actor.IsSpecialMotionCurrent(id))CombatAudioController.PlayMechanism(CombatSoundCue.MineHammer);
    }
    private void Phase(MineBossPhase phase,bool transition)
    {
        if(!transition || !CombatAudioController.FeedbackAllowed)return;
        pulseUntil=Time.time+.3f;
        CombatAudioController.PlayMechanism(phase==MineBossPhase.PilotFoot?CombatSoundCue.MineMechBreak:CombatSoundCue.MinePiston);
    }
    private void LateUpdate()
    {
        if(actor==null || actor.IsDefeated || Time.time>=pulseUntil || !CombatAudioController.FeedbackAllowed)
        {if(spark!=null)spark.enabled=false;return;}
        var frames=GameplayThemeSkin.Current?.mineOreFrames;if(frames==null || frames.Length==0)return;
        if(spark==null)
        {
            var go=new GameObject("OreContactSpark",typeof(RectTransform),typeof(CanvasRenderer),typeof(Image));
            go.transform.SetParent(transform,false);spark=go.GetComponent<Image>();spark.raycastTarget=false;spark.preserveAspect=true;
            var rect=spark.rectTransform;rect.anchorMin=rect.anchorMax=new Vector2(.5f,0);rect.pivot=new Vector2(.5f,0);
            rect.anchoredPosition=new Vector2(-34,34);rect.sizeDelta=new Vector2(64,64);
        }
        spark.enabled=true;spark.sprite=frames[PresentationPreferences.ReducedMotion?0:(int)(Time.time*24)%frames.Length];
        spark.color=new Color(1,1,1,PresentationPreferences.ReducedMotion?.4f:.65f);
    }
    private void OnDisable(){StopAllCoroutines();pulseUntil=0;if(spark!=null)spark.enabled=false;}
    private void OnDestroy()
    {
        if(attack!=null)attack.AttackResolved-=Contact;
        if(ore!=null)ore.Changed-=PowerChanged;
        if(kit!=null)kit.BossPhaseChanged-=Phase;
        if(actor!=null){actor.AbilityCastCommitted-=Cast;actor.SpecialMotionRequested-=Motion;}
    }
}
