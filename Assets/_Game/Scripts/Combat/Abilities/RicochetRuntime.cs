using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class RicochetRuntime : SequentialBoardAbilityRuntime
{
    private readonly Dictionary<long,int> hits=new Dictionary<long,int>();
    protected override string StepAnimation => "Fire";
    public event Action<int,Vector2Int,EnemyActor,int> RicochetHit;
    public override bool Supports(CharacterAbilityDefinition definition) => definition is RicochetAbilityDefinition;
    protected override PlayerBoardImpactSequence CreateSequence(CharacterAbilityDefinition definition)
    {
        var ability=(RicochetAbilityDefinition)definition;hits.Clear();
        return new PlayerBoardImpactSequence {Count=ability.Shots,LeadSeconds=ability.SpinSeconds,
            StepSeconds=ability.ShotSeconds,RecoverySeconds=ability.RecoverySeconds,Select=SelectTarget,
            Impacted=(step,cell,survived)=>Ricochet(step,cell,ability)};
    }
    private Vector2Int? SelectTarget(int step,IReadOnlyList<PlayerBoardImpactTarget> candidates)
    {
        var legal=new List<Vector2Int>();
        foreach(var t in candidates)if(t.Barrier || t.Gem!=null)legal.Add(t.Cell);
        return legal.Count>0?legal[GameplayRandom.Range(0,legal.Count)]:(Vector2Int?)null;
    }
    private void Ricochet(int step,Vector2Int cell,RicochetAbilityDefinition ability)
    {
        var living=new List<EnemyActor>();
        foreach(var enemy in Wave.ActiveEnemies)if(enemy!=null && enemy.IsInitialized && enemy.CanReceiveDamage)living.Add(enemy);
        if(living.Count==0)return;
        var target=living[GameplayRandom.Range(0,living.Count)];
        // Central actor interception is one hop. Count the actual recipient,
        // including shield-gated hits, rather than its protected commander.
        var recipient=target.DamageRedirectTarget ?? target;
        hits.TryGetValue(recipient.PersistentId,out int priorHits);
        var passive=Player.Definition.PassiveAbility as RepeatRicochetPassiveDefinition;
        int damage=RunUpgradeResolver.ResolveAbilityDamage(ability.BaseEnemyDamage+(passive?.Bonus(priorHits)??0),ability);
        damage=RunUpgradeResolver.ResolveEnemyDamage(damage,recipient);
        var result=target.ResolveDirectDamage(damage);
        if(!result.Applied || result.Recipient==null)return;
        hits[result.Recipient.PersistentId]=priorHits+1;
        RicochetHit?.Invoke(step,cell,result.Recipient,result.HealthDamage+result.ShieldDamage);
    }
}
