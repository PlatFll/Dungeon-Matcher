using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class BoardController
{
    private readonly Dictionary<int,FrostGemSnapshot> frostGems=new Dictionary<int,FrostGemSnapshot>();
    private int nextFrostSeedMove;
    private FrostwakeRules FrostRules => RunSession.Current?.Zone?.Definition?.frostwakeRules;
    public event Action FrostChanged;
    public FrostStage IceStage(Gem gem) => gem!=null && frostGems.TryGetValue(gem.BoardIdentity,out var ice)?ice.stage:FrostStage.None;
    public Gem FindGemIdentity(int identity)
    {
        if(identity<=0 || gems==null)return null;
        for(int y=0;y<height;y++)for(int x=0;x<width;x++)
            if(gems[x,y]!=null && gems[x,y].BoardIdentity==identity)return gems[x,y];
        return null;
    }
    private int FrostMove => Math.Max(completedValidPlayerMoves,CombatMoveClock.EffectAction);
    private void PublishFrost()
    {
        if(gems!=null)for(int y=0;y<height;y++)for(int x=0;x<width;x++)
        {
            var gem=gems[x,y];if(gem==null)continue;
            var stage=IceStage(gem);var view=gem.GetComponent<FrostwakeGemView>();
            if(view==null && stage!=FrostStage.None)view=gem.gameObject.AddComponent<FrostwakeGemView>();
            view?.Show(stage,FrostRules,frozenGemOverlaySprite);
        }
        FrostChanged?.Invoke();
    }
    private void RemoveFrost(Gem gem)
    {if(gem!=null && frostGems.Remove(gem.BoardIdentity))PublishFrost();}

    public bool TryQueueEncasedGem(EnemyActor owner,FrostStage stage,Action<Gem> completed,Func<bool> cancelled=null)
    {
        if(owner==null || owner.IsDefeated || gems==null || stage<FrostStage.Frost || stage>FrostStage.Bifrost)return false;
        var request=new BoardMutationRequest {Kind=BoardMutationKind.EncaseGem,OwnerActor=owner,
            IceStage=stage,IsCancelled=cancelled};
        request.Completed=ok=>completed?.Invoke(ok?request.TargetGem:null);
        EnqueueBoardMutation(request);TryStartBoardMutationProcessor();return true;
    }
    private Gem EncaseSafeGem(FrostStage stage)
    {
        if(RestrictionCount>=BalanceV1.Current.maximumGlobalChains)return null;
        var candidates=BuildSafePinnableGemList();
        candidates.RemoveAll(g=>g==null || g.SpecialType!=GemSpecialType.None || IsProtectedWarningTarget(g));
        if(candidates.Count==0)return null;
        var target=candidates[GameplayRandom.Range(0,candidates.Count)];
        frostGems[target.BoardIdentity]=new FrostGemSnapshot {identity=target.BoardIdentity,stage=stage,lastAdvancedMove=FrostMove};
        CancelPointerInteraction(target);PublishFrost();return target;
    }
    private void ExecuteEncaseGem(BoardMutationRequest request)
    {request.TargetGem=EncaseSafeGem(request.IceStage);request.Succeeded=request.TargetGem!=null;}

    public bool TryQueueConsumeEncasedGem(EnemyActor owner,int identity,Action<FrostStage> impact,Action<bool> completed,Func<bool> cancelled)
    {
        var target=FindGemIdentity(identity);
        if(owner==null || owner.IsDefeated || IceStage(target)==FrostStage.None)return false;
        EnqueueBoardMutation(new BoardMutationRequest {Kind=BoardMutationKind.ConsumeEncasedGem,OwnerActor=owner,
            TargetGem=target,IceImpact=impact,Completed=completed,IsCancelled=cancelled});
        TryStartBoardMutationProcessor();return true;
    }
    private IEnumerator ExecuteConsumeEncasedGem(BoardMutationRequest request)
    {
        var gem=request.TargetGem;var stage=IceStage(gem);
        if(!IsGemStillOnBoard(gem) || stage==FrostStage.None)yield break;
        // Consume before gameplay callbacks. No player reward or retaliation.
        frostGems.Remove(gem.BoardIdentity);request.Succeeded=true;
        request.IceImpact?.Invoke(stage);PublishFrost();
        yield return ClearMatches(new HashSet<Gem>{gem},null);
        yield return ResolveEnvironmentalBoardChange();
    }
    private void ExecuteFrostEnvironment(int move)
    {
        var rules=FrostRules;if(rules==null)return;
        bool changed=false;
        foreach(var ice in frostGems.Values)changed|=ice.Advance(move,rules.movesPerStage);
        if(nextFrostSeedMove<=0)nextFrostSeedMove=move+Mathf.Max(1,rules.seedCadence);
        if(move>=nextFrostSeedMove)
        {
            nextFrostSeedMove=move+Mathf.Max(1,rules.seedCadence);
            if(frostGems.Count<rules.maximumEncasedGems)EncaseSafeGem(FrostStage.Frost);
        }
        if(changed)PublishFrost();
    }
    private void DamageFrostFromClears(HashSet<Gem> clears,HashSet<Gem> ignored,bool manual)
    {
        if(clears==null || frostGems.Count==0)return;
        bool changed=false;int retaliations=0;
        foreach(var ice in new List<FrostGemSnapshot>(frostGems.Values))
        {
            var gem=FindGemIdentity(ice.identity);
            if(gem==null){frostGems.Remove(ice.identity);changed=true;continue;}
            bool direct=clears.Contains(gem) && (ignored==null || !ignored.Contains(gem));
            bool adjacent=false;
            if(!direct)foreach(var cleared in clears)
                if(cleared!=null && (ignored==null || !ignored.Contains(cleared)) &&
                    Mathf.Abs(cleared.Column-gem.Column)+Mathf.Abs(cleared.Row-gem.Row)==1){adjacent=true;break;}
            if(!direct && !adjacent)continue;
            if(direct)
            {
                if(manual && ice.stage==FrostStage.Bifrost)retaliations++;
                frostGems.Remove(ice.identity);
            }
            else {ice.Hit(FrostMove);if(ice.stage==FrostStage.None)frostGems.Remove(ice.identity);}
            changed=true;
        }
        // Ordinary direct matching is the only dangerous ice clear. A nearby
        // clear thaws one stage; cascades, specials and free skills are safe.
        if(retaliations>0 && FrostRules!=null)
        {
            var player=combatController?.PlayerActor;
            for(int i=0;i<retaliations;i++)player?.TryTakeDamage(FrostRules.bifrostRetaliation);
            if(player!=null && !player.IsDefeated)player.Statuses.Apply(FrostRules.frostbite);
        }
        if(changed)PublishFrost();
    }
    private List<FrostGemSnapshot> CaptureFrost()
    {
        var values=new List<FrostGemSnapshot>();foreach(var ice in frostGems.Values)values.Add(ice.Copy());
        values.Sort((a,b)=>a.identity.CompareTo(b.identity));return values;
    }
    private void RestoreFrost(BoardCombatSnapshot saved)
    {
        frostGems.Clear();nextFrostSeedMove=saved.nextFrostSeedMove;
        if(saved.frost!=null)foreach(var ice in saved.frost)
            if(ice!=null && FindGemIdentity(ice.identity)!=null && ice.stage>=FrostStage.Frost && ice.stage<=FrostStage.Bifrost)
                frostGems[ice.identity]=ice.Copy();
        PublishFrost();
    }
    private void ReconcileFrostMemory(BoardCombatSnapshot saved)
    {
        // Present-day ice state follows surviving identity; photographs cannot
        // restore solved ice or rewind a boss's response window.
        saved.frost=CaptureFrost();saved.frost.RemoveAll(ice=>!saved.cells.Exists(c=>c.hasGem && c.identity==ice.identity));
        saved.nextFrostSeedMove=nextFrostSeedMove;
    }
}
