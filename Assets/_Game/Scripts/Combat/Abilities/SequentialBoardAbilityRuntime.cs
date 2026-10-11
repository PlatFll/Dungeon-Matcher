using System;
using UnityEngine;

// Shared lifecycle only. Each runtime owns its targeting and presentation;
// BoardController owns the entire accepted sequence and final settlement.
public abstract class SequentialBoardAbilityRuntime : MonoBehaviour,IPlayerAbilityRuntime,IPlayerAbilityPresentation
{
    protected BoardController Board;
    protected PlayerActor Player;
    protected WaveController Wave;
    public event Action StateChanged;
    public event Action<int,Vector2Int> Approaching;
    public event Action<int,Vector2Int,bool> Impacted;
    public bool IsActive {get;private set;}
    public virtual string AnimationState => IsActive?"Ability":"Idle";
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
        sequence.Approaching=(step,cell)=>Approaching?.Invoke(step,cell);
        sequence.Impacted=(step,cell,survived)=>{impact?.Invoke(step,cell,survived);Impacted?.Invoke(step,cell,survived);};
        sequence.Cancelled=()=>!IsActive || Player==null || Player.IsDefeated || !isActiveAndEnabled;
        sequence.Completed=Finish;
        IsActive=true;
        if (!Board.TryPlayerImpactSequence(sequence)) {IsActive=false;return false;}
        StateChanged?.Invoke();
        return true;
    }
    public void Cancel() => Finish();
    private void Finish() {if(!IsActive)return;IsActive=false;StateChanged?.Invoke();}
    protected virtual void OnDisable() => Cancel();
}
