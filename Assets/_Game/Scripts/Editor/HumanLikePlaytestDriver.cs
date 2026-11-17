#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum ResonancePlaystyle { Novice, Typical, Skilled, Defensive, Aggressive }
/// <summary>Visible-state policy. Submits the normal accepted-action path, never modifies combat state.</summary>
public sealed class HumanLikePlaytestDriver
{
    public readonly ResonancePlaystyle Style;
    private readonly System.Random decisions;
    private float nextAction, cardReady = -1;
    public int Swaps { get; private set; }
    public int Abilities { get; private set; }
    public int CardPicks { get; private set; }
    public Action<string> ActionLog;
    public HumanLikePlaytestDriver(ResonancePlaystyle style,int seed) { Style=style; decisions=new System.Random(seed ^ 0x591a); }
    public void Step(RunSession run)
    {
        if(run==null || !run.InitialStateReady || run.IsFinished || run.Continuation.IsRestoring || Time.timeScale<=0) return;
        var draft=run.Waves.GetComponent<RunUpgradeCoordinator>();
        if(draft!=null && draft.IsBlockingWaveProgression && run.Continuation.CanCapture)
        {
            var snapshot=run.Continuation.Capture();
            if(snapshot.draft.Count==0)return;
            if(cardReady<0) cardReady=Time.time+6+(float)decisions.NextDouble()*5;
            if(Time.time<cardReady)return;
            var catalog=RunUpgradeRuntime.Current.Catalog;
            var theme=Style==ResonancePlaystyle.Defensive || Style==ResonancePlaystyle.Novice ? RunUpgradeTheme.Survival :
                Style==ResonancePlaystyle.Aggressive ? RunUpgradeTheme.Ability : RunUpgradeTheme.Board;
            string pick=snapshot.draft.OrderByDescending(id=>catalog.Upgrades.First(d=>d.UpgradeId==id).Theme==theme).First();
            if(draft.SelectRecordedCard(pick)) { CardPicks++;ActionLog?.Invoke("card:"+pick);cardReady=-1;nextAction=Time.time+Delay(); }
            return;
        }
        cardReady=-1;
        if(Time.time<nextAction || !run.Waves.IsWaveActive || !run.Continuation.CanCapture || run.Board.IsExternalInputBlocked) return;
        if(run.Player.CurrentHealth<run.Player.MaximumHealth*(Style==ResonancePlaystyle.Defensive?.65f:.35f) && run.TryUsePotion())
        { ActionLog?.Invoke("potion");nextAction=Time.time+.35f;return; }
        var ability=run.Player.GetComponent<PlayerAbilityController>();
        if(ability!=null && ability.CanActivate && (Style!=ResonancePlaystyle.Novice || decisions.NextDouble()<.4) && ability.TryActivate())
        { Abilities++;ActionLog?.Invoke("ability");nextAction=Time.time+.45f;return; }
        var options=run.Board.GetImmediateResponses(); if(options.Count==0)return;
        var warnings=run.Board.CaptureContinuation(run.Waves.ContinuationOwnerSlot).warnings;
        var ranked=options.Select(o=>new KeyValuePair<BoardController.ResponseOption,float>(o,Score(run,o,warnings)))
            .OrderByDescending(p=>p.Value).ToArray();
        int rank=decisions.NextDouble()<(Style==ResonancePlaystyle.Novice?.35:.12)?decisions.Next(Math.Min(3,ranked.Length)):0;
        var chosen=ranked[rank].Key;
        if(!run.Board.IsHintMoveStillValid(chosen.Source,chosen.Target))return;
        if(Style==ResonancePlaystyle.Aggressive && run.Waves.ActiveEnemies.Count>=2 && run.CanUse(ConsumableKind.Bomb) &&
            decisions.NextDouble()<.12 && run.TryUseBomb(chosen.Target))
        { ActionLog?.Invoke("bomb:"+chosen.Target.Column+","+chosen.Target.Row);nextAction=Time.time+Delay();return; }
        int x=chosen.Source.Column,y=chosen.Source.Row,tx=chosen.Target.Column,ty=chosen.Target.Row;
        // The same TrySwap command and BeforePlayerSwap journal as touch input; no direct board writes.
        run.Board.ReplayPlayerSwap(x,y,tx,ty); Swaps++;
        ActionLog?.Invoke("swap:"+x+","+y+">"+tx+","+ty);nextAction=Time.time+Delay();
    }
    private float Delay()
    {
        float low=Style==ResonancePlaystyle.Novice?1:Style==ResonancePlaystyle.Defensive?1.5f:
            Style==ResonancePlaystyle.Skilled?.4f:Style==ResonancePlaystyle.Aggressive?.5f:.7f;
        float high=Style==ResonancePlaystyle.Novice?3:Style==ResonancePlaystyle.Defensive?3:Style==ResonancePlaystyle.Skilled?1.5f:2;
        return Mathf.Lerp(low,high,(float)decisions.NextDouble());
    }
    private float Score(RunSession run,BoardController.ResponseOption option,List<BoardWarningSnapshot> warnings)
    {
        bool novice=Style==ResonancePlaystyle.Novice, defensive=Style==ResonancePlaystyle.Defensive;
        float value=option.Clears.Count+(option.CreatesSpecial?(novice?2:18):0)+(option.UsesSpecial?(novice?5:24):0);
        if(option.BreaksObstacle)value+=novice?1:12;
        foreach(var gem in option.Clears)
        {
            if(gem==null)continue;
            if(gem.Type==run.Player.Definition.AffinityGemType && run.Player.CurrentHealth<run.Player.MaximumHealth)
                value+=defensive?9:3;
            foreach(var enemy in run.Waves.ActiveEnemies)
            {
                if(enemy==null || enemy.IsDefeated || enemy.AssignedGemType!=gem.Type)continue;
                value+=novice?3:enemy.Definition.IsSupport?7:4;
                if(!novice && enemy.GetComponent<EnemyChannelRuntime>()?.IsChanneling==true)value+=8;
            }
            var cell=option.FinalCell(gem);int index=cell.y*run.Board.Width+cell.x;
            foreach(var warning in warnings)
                if(!warning.environmental && warning.kind!=2 && warning.targets.Contains(index)) value+=novice?2:18;
        }
        // Only visible options and current telegraphs are scored; never refill RNG or future encounters.
        return value+(float)decisions.NextDouble()*(novice?8:2);
    }
}
#endif
