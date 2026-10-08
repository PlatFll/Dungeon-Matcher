using UnityEngine;
using UnityEngine.UI;

/// <summary>Optional phase artwork follows the same authoritative, persistent boss actor.</summary>
[DisallowMultipleComponent]
public sealed class MineBossPresentation : MonoBehaviour
{
    private EnemyActor actor;
    private MineEnemyAbility kit;
    private Animator animator;
    private Image portrait;
    private RuntimeAnimatorController firstController;
    private Sprite firstSprite;

    public void Initialize(EnemyActor owner,MineEnemyAbility source)
    {
        actor=owner;kit=source;
        var visual=transform.Find("VisualRoot");
        animator=visual!=null?visual.GetComponent<Animator>():null;
        portrait=visual!=null?visual.GetComponent<Image>():null;
        firstController=actor.Definition.AnimationControllerOverride;
        firstSprite=actor.Definition.StaticVisualSprite;
        if(kit==null) return;
        kit.BossPhaseChanged+=PhaseChanged;
        PhaseChanged(kit.BossPhase,false);
    }

    private void PhaseChanged(MineBossPhase phase,bool showTransition)
    {
        var definition=actor.Definition;
        var controller=phase==MineBossPhase.PilotFoot?definition.minePilotController:
            phase==MineBossPhase.SecondMech?definition.mineReserveController:firstController;
        var sprite=phase==MineBossPhase.PilotFoot?definition.minePilotSprite:
            phase==MineBossPhase.SecondMech?definition.mineReserveSprite:firstSprite;
        if(portrait!=null && sprite!=null) portrait.sprite=sprite;
        if(animator==null || controller==null) return;
        animator.runtimeAnimatorController=controller;animator.enabled=true;
        animator.Rebind();
        string state=showTransition?(phase==MineBossPhase.PilotFoot?"Eject":"Remount"):"Idle";
        int hash=Animator.StringToHash("Base Layer."+state);
        animator.Play(animator.HasState(0,hash)?hash:Animator.StringToHash("Base Layer.Idle"),0,0);
        animator.Update(0);
        // Continue binds directly to the saved phase. It never replays ejection,
        // summons an actor, grants rewards or advances the remount deadline.
    }

    private void OnDestroy()
    {
        if(kit!=null) kit.BossPhaseChanged-=PhaseChanged;
    }
}
