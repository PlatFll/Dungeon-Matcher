using System;
using UnityEngine;

// Shared lifecycle only. Each runtime owns its targeting and presentation;
// BoardController owns the entire accepted sequence and final settlement.
public abstract class SequentialBoardAbilityRuntime : MonoBehaviour,IPlayerAbilityRuntime,IPlayerAbilityPresentation,IPlayerAbilityVisualState
{
    protected BoardController Board;
    protected PlayerActor Player;
    protected WaveController Wave;
    public event Action StateChanged;
    public event Action<int,Vector2Int> Approaching;
    public event Action<int,Vector2Int> Striking;
    public event Action<int,Vector2Int,bool> Impacted;
    public bool IsActive {get;private set;}
    private string animationState="Idle";
    public string AnimationState => IsActive?animationState:"Idle";
    public int AnimationRevision { get; private set; }
    public virtual bool HidePlayerCharacter => false;
    protected virtual string StepAnimation => "Ability";
    public float AnimationNormalizedTime => -1;
    public bool ShowMoveCounter => false;
    public int RemainingMoveCount => 0;
    public abstract bool Supports(CharacterAbilityDefinition definition);
    protected abstract PlayerBoardImpactSequence CreateSequence(CharacterAbilityDefinition definition);
    public bool CanActivate(CharacterAbilityDefinition definition)
    {
        Board=RunSession.Current?.Board ?? FindFirstObjectByType<BoardController>();
        Player=GetComponent<PlayerActor>();
        Wave=FindFirstObjectByType<WaveController>();
        return !IsActive && Supports(definition) && Player!=null && !Player.IsDefeated &&
            Wave!=null && Wave.IsWaveActive && Board!=null && Board.CanAcceptPlayerImpactSequence;
    }
    public bool TryActivate(CharacterAbilityDefinition definition)
    {
        if (!CanActivate(definition)) return false;
        var sequence=CreateSequence(definition);
        var impact=sequence.Impacted;
        sequence.Approaching=(step,cell)=>{Present(StepAnimation);Approaching?.Invoke(step,cell);};
        sequence.Striking=(step,cell)=>Striking?.Invoke(step,cell);
        sequence.Impacted=(step,cell,survived)=>{impact?.Invoke(step,cell,survived);Impacted?.Invoke(step,cell,survived);};
        sequence.Cancelled=()=>!IsActive || Player==null || Player.IsDefeated || !isActiveAndEnabled;
        sequence.Completed=Finish;
        sequence.Recovering=()=>{if(IsActive)Present("Recovery");};
        IsActive=true;
        animationState="Ability";AnimationRevision++;
        if (!Board.TryPlayerImpactSequence(sequence)) {IsActive=false;return false;}
        if (!TryGetComponent<PlayerImpactPresentation>(out _)) gameObject.AddComponent<PlayerImpactPresentation>();
        StateChanged?.Invoke();
        return true;
    }
    public void Cancel() => Finish();
    private void Present(string state) {animationState=state;AnimationRevision++;StateChanged?.Invoke();}
    private void Finish() {if(!IsActive)return;IsActive=false;StateChanged?.Invoke();}
    protected virtual void OnDisable() => Cancel();
}
