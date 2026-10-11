using NUnit.Framework;
using UnityEngine;

public sealed class FrostwakePlayableTests
{
    [Test] public void FrostWorsensOnlyOnDistinctIgnoredMovesAndThawResetsGrowth()
    {
        var ice=new FrostGemSnapshot {identity=1,stage=FrostStage.Frost};
        Assert.That(ice.Advance(1,3),Is.False);Assert.That(ice.Advance(1,3),Is.False);
        Assert.That(ice.ignoredMoves,Is.EqualTo(1));
        ice.Advance(2,3);Assert.That(ice.Advance(3,3),Is.True);
        Assert.That(ice.stage,Is.EqualTo(FrostStage.HeavyIce));
        ice.Hit(4);ice.Advance(4,3);
        Assert.That(ice.stage,Is.EqualTo(FrostStage.Frost));Assert.That(ice.ignoredMoves,Is.Zero);
        ice.Advance(5,3);ice.Advance(6,3);ice.Advance(7,3);
        Assert.That(ice.stage,Is.EqualTo(FrostStage.HeavyIce));
    }
    [Test] public void FrostSnapshotRejectsMissingOrDuplicateGemAndInvalidChannel()
    {
        var board=new BoardCombatSnapshot();Assert.That(FrostGemSnapshot.Supports(board),Is.True);
        board.cells.Add(new BoardCellSnapshot {hasGem=true,identity=9});
        var ice=new FrostGemSnapshot {identity=9,stage=FrostStage.Bifrost};board.frost.Add(ice);
        Assert.That(FrostGemSnapshot.Supports(board),Is.True);
        board.frost.Add(ice.Copy());Assert.That(FrostGemSnapshot.Supports(board),Is.False);
        board.frost.RemoveAt(1);ice.identity=10;Assert.That(FrostGemSnapshot.Supports(board),Is.False);
        var cast=new GlacierCleaveSnapshot {channeling=true,targetIdentity=9,deadlineMove=3,sequence=1};
        Assert.That(GlacierCleaveSnapshot.Supports(cast),Is.True);
        cast.lastOutcomeSequence=1;Assert.That(GlacierCleaveSnapshot.Supports(cast),Is.False);
    }
    [Test] public void RicochetBonusIsBoundedAndNeverUsesNegativePriorHits()
    {
        var ability=ScriptableObject.CreateInstance<RicochetAbilityDefinition>();
        var passive=ScriptableObject.CreateInstance<RepeatRicochetPassiveDefinition>();
        try
        {
            Assert.That(ability.Shots,Is.EqualTo(6));
            Assert.That(ability.BaseEnemyDamage+passive.Bonus(-1),Is.EqualTo(5));
            Assert.That(ability.BaseEnemyDamage+passive.Bonus(0),Is.EqualTo(5));
            Assert.That(ability.BaseEnemyDamage+passive.Bonus(1),Is.EqualTo(10));
            Assert.That(ability.BaseEnemyDamage+passive.Bonus(2),Is.EqualTo(15));
            Assert.That(ability.BaseEnemyDamage+passive.Bonus(int.MaxValue),Is.EqualTo(20));
            Assert.That(ability.MaximumSelfRefundFraction,Is.Zero);
        }
        finally {Object.DestroyImmediate(ability);Object.DestroyImmediate(passive);}
    }
    [Test] public void AttenderBudgetAndTargetPriorityRetainObstacleResponse()
    {
        var ability=ScriptableObject.CreateInstance<DungeonAttenderAbilityDefinition>();
        try
        {
            Assert.That(ability.MovementBudget,Is.EqualTo(7));
            var barrier=new PlayerBoardImpactTarget(Vector2Int.zero,null,3,true,false,false);
            var vine=new PlayerBoardImpactTarget(Vector2Int.zero,null,0,false,true,false);
            var empty=new PlayerBoardImpactTarget(Vector2Int.zero,null,0,false,false,false);
            Assert.That(DungeonAttenderRuntime.Priority(barrier,GemType.Emerald),Is.LessThan(DungeonAttenderRuntime.Priority(vine,GemType.Emerald)));
            Assert.That(DungeonAttenderRuntime.Priority(vine,GemType.Emerald),Is.LessThan(DungeonAttenderRuntime.Priority(empty,GemType.Emerald)));
        }
        finally {Object.DestroyImmediate(ability);}
    }
}
