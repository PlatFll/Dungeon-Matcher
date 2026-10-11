using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class DungeonAttenderRuntime : SequentialBoardAbilityRuntime
{
    private Vector2Int? position, retry;
    private Vector2Int direction;
    public override bool Supports(CharacterAbilityDefinition definition) => definition is DungeonAttenderAbilityDefinition;
    protected override PlayerBoardImpactSequence CreateSequence(CharacterAbilityDefinition definition)
    {
        var ability=(DungeonAttenderAbilityDefinition)definition;
        position=null;retry=null;direction=Vector2Int.zero;
        return new PlayerBoardImpactSequence {Count=ability.MovementBudget,LeadSeconds=ability.JumpSeconds,
            StepSeconds=ability.StepSeconds,Select=SelectStep,
            Impacted=(step,cell,survived)=>{retry=survived?cell:(Vector2Int?)null;if(!survived)position=cell;}};
    }
    public static int Priority(PlayerBoardImpactTarget target, GemType affinity)
    {
        if(target.Barrier)return 0;
        if(target.Vine || target.Frozen)return 1;
        if(target.Gem==null)return 5;
        if(target.Gem.SpecialType!=GemSpecialType.None)return 2;
        return target.Gem.Type==affinity?3:4;
    }
    private Vector2Int? SelectStep(int step,IReadOnlyList<PlayerBoardImpactTarget> targets)
    {
        if(retry.HasValue)
            foreach(var t in targets)if(t.Cell==retry.Value)return t.Cell;
        var choices=new List<Vector2Int>();int best=int.MaxValue;
        if(!position.HasValue)
        {
            foreach(var t in targets)
            {
                int score=Priority(t,Player.Definition.AffinityGemType);if(score>=5)continue;
                if(score<best){best=score;choices.Clear();}if(score==best)choices.Add(t.Cell);
            }
        }
        else
        {
            var cells=new Dictionary<Vector2Int,PlayerBoardImpactTarget>();
            foreach(var t in targets)cells[t.Cell]=t;
            foreach(var dir in new[]{Vector2Int.left,Vector2Int.right,Vector2Int.down,Vector2Int.up})
            {
                var next=position.Value+dir;if(!cells.ContainsKey(next))continue;
                int score=500;
                for(int distance=1;distance<=Mathf.Max(Board.Width,Board.Height);distance++)
                {
                    if(!cells.TryGetValue(position.Value+dir*distance,out var target))break;
                    score=Mathf.Min(score,Priority(target,Player.Definition.AffinityGemType)*100+distance);
                    if(target.Barrier)break;
                }
                // Straight-line continuity breaks ties without hiding priority.
                score=score*2+(dir==direction?0:1);
                if(score<best){best=score;choices.Clear();}if(score==best)choices.Add(next);
            }
        }
        if(choices.Count==0)return null;
        var chosen=choices[GameplayRandom.Range(0,choices.Count)];
        if(position.HasValue)direction=chosen-position.Value;
        return chosen;
    }
}
