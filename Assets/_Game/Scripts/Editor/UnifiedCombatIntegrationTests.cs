using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

public sealed partial class ForestFoundationPlayTests
{
    [UnityTest] public IEnumerator UnifiedCommandConsumesWholeLancerSequenceOnceAndStaggerCancels()
    {
        yield return LaunchUnified();
        var kingData=AssetDatabase.LoadAssetAtPath<EnemyDefinition>("Assets/_Game/Data/Enemies/Enemy_King.asset");
        var lancerData=AssetDatabase.LoadAssetAtPath<EnemyDefinition>("Assets/_Game/Data/Enemies/Enemy_RoyalLancer.asset");
        Assert.That(Run.Waves.TrySummonEnemy(kingData,out var king),Is.True);
        Assert.That(Run.Waves.TrySummonEnemy(lancerData,out var lancer),Is.True);
        yield return Until(()=>Run.Waves.ActiveEnemies.All(e=>e.GetComponent<EnemyLifecycleVFX>()?.IsSpawning!=true),"royal formation ready");
        // Keep original authored follow-up damage and shield gating.
        Set(Run.Player,"maximumHealth",9995);Set(Run.Player,"currentHealth",9995);
        Set(king.GetComponent<KingEnemyAbility>(),"cycle",1);king.SetSpecialTurnRequirement(1);
        Set(lancer.GetComponent<EnemyAutoAttack>(),"unifiedRemaining",1);
        int hits=0;lancer.GetComponent<EnemyAutoAttack>().AttackResolved+=(_,__,___)=>hits++;
        yield return Move();
        Assert.That(hits,Is.EqualTo(lancer.RuntimeStats.FollowUpDamage>0?2:1));
        Assert.That(lancer.GetComponent<EnemyAutoAttack>().RemainingAttackTime,Is.EqualTo(lancer.GetComponent<EnemyAutoAttack>().EffectiveMoveInterval));
        Assert.That(lancer.GetComponent<EnemyAutoAttack>().HasCommandReservation,Is.False);
        king.GetComponent<KingEnemyAbility>().CommandIssued+=_=>lancer.GetComponent<EnemyStagger>().ApplyStagger(2,2);
        Set(king.GetComponent<KingEnemyAbility>(),"cycle",1);king.SetSpecialTurnRequirement(1);
        int before=hits;yield return Move();Assert.That(hits,Is.EqualTo(before));
        Assert.That(Run.MoveClock.IsBlockingWaveProgression,Is.False);
        Assert.That(lancer.GetComponent<EnemyAutoAttack>().HasCommandReservation,Is.False);
    }

    [UnityTest] public IEnumerator UnifiedCascadeCountsOnceAndWeaknessDamagePreventsDecay()
    {
        yield return LaunchUnified();DurableUnifiedFixture();
        var actor=Run.Waves.ActiveEnemies[0];var stagger=actor.GetComponent<EnemyStagger>();
        var board=Run.Board;var sprites=(Sprite[])Get(board,"gemSprites");
        var color=actor.AssignedGemType;
        for(int x=0;x<board.Width;x++)for(int y=0;y<board.Height;y++)
        {var gem=board.GetGem(x,y);gem.SetSpecialType(GemSpecialType.None);gem.SetType(color,sprites[(int)color]);}
        board.GetGem(2,0).SetType((GemType)(((int)color+1)%6),sprites[((int)color+1)%6]);
        board.GetGem(5,3).SetSpecialType(GemSpecialType.RowBomb);board.GetGem(6,3).SetSpecialType(GemSpecialType.ColumnBomb);
        Set(board,"refillRandom",new SavedRandom(13579));
        int accepted=0,clears=0,deepest=0;board.ValidPlayerMoveAccepted+=_=>accepted++;
        board.BoardClearResolved+=c=>{clears+=c.GemCount;deepest=Math.Max(deepest,c.CascadeDepth);};
        board.StartCoroutine((IEnumerator)Call(board,"TrySwap",board.GetGem(2,0),board.GetGem(2,1)));
        yield return Until(()=>Run.MoveClock.Tick==1 && Run.Continuation.CanCapture,"unified cascades settle");
        Assert.That(accepted,Is.EqualTo(1));Assert.That(clears,Is.GreaterThan(40));Assert.That(deepest,Is.GreaterThanOrEqualTo(2));
        Assert.That((int)Get(stagger,"moveLastHit"),Is.EqualTo(1),"cascade weakness hit belongs to the opening action");
        Assert.That(stagger.StaggerMeterNormalized,Is.GreaterThan(0));
        float meter=stagger.StaggerMeterNormalized;yield return new WaitForSeconds(.4f);
        Assert.That(stagger.StaggerMeterNormalized,Is.EqualTo(meter));
    }

    [UnityTest] public IEnumerator UnifiedEveryRankPriorityAndDeathAreRechecked()
    {
        yield return LaunchUnified();
        var data=AssetDatabase.LoadAssetAtPath<EnemyDefinition>("Assets/_Game/Data/Enemies/Enemy_ShieldKnight.asset");
        Assert.That(Run.Waves.TrySummonEnemy(data,out var left),Is.True);
        Assert.That(Run.Waves.TrySummonEnemy(data,out var right),Is.True);
        yield return Stable();DurableUnifiedFixture();
        var a=UnityEngine.Object.Instantiate(data);var b=UnityEngine.Object.Instantiate(data);
        Set(left,"definition",a);Set(right,"definition",b);
        var order=new List<int>();left.SpecialAbilityUsed+=_=>order.Add(1);right.SpecialAbilityUsed+=_=>order.Add(2);
        try
        {
            foreach(var pair in new[]{new[]{1,2},new[]{2,3},new[]{0,1},new[]{1,1}})
            {
                Set(a,"category",(EnemyCategory)pair[0]);Set(b,"category",(EnemyCategory)pair[1]);
                left.SetSpecialTurnRequirement(1);right.SetSpecialTurnRequirement(1);order.Clear();
                yield return Move();
                CollectionAssert.AreEqual(pair[0]==pair[1]?new[]{1,2}:new[]{2,1},order);
            }
            Set(a,"category",EnemyCategory.Special);Set(b,"category",EnemyCategory.Boss);
            right.SpecialAbilityUsed+=_=>{left.ResolveDamageWithoutFeedback(999999);left.ResolveDamageWithoutFeedback(999999);};
            left.SetSpecialTurnRequirement(1);right.SetSpecialTurnRequirement(1);order.Clear();
            yield return Move();CollectionAssert.AreEqual(new[]{2},order,"dead actor loses its snapshotted opportunity");
            Assert.That(Run.Waves.TrySummonEnemy(data,out var replacement),Is.True);yield return Stable();
            Assert.That(Run.Waves.ContinuationSlot(replacement),Is.EqualTo(1));
            Assert.That(replacement.PersistentId,Is.Not.EqualTo(left.PersistentId));
            Assert.That(replacement.CurrentSpecialTurnCount,Is.Zero);
        }
        finally {if(left!=null)Set(left,"definition",data);Set(right,"definition",data);UnityEngine.Object.Destroy(a);UnityEngine.Object.Destroy(b);}
    }

    [UnityTest] public IEnumerator UnifiedLateWaveStaggerRemainsReachableWithInterleavedWeakness()
    {
        yield return LaunchUnified();
        var actor=Run.Waves.ActiveEnemies[0];var original=actor.Definition;var originalStats=actor.RuntimeStats;
        var stagger=actor.GetComponent<EnemyStagger>();var combat=UnityEngine.Object.FindFirstObjectByType<CombatController>();
        var rows=new List<string>{"enemy,wave,player_level,hp,threshold,off_color_decay,meter_percent_lost,three_gem_damage,alternating_moves_to_stagger"};
        var definitions=AssetDatabase.FindAssets("t:EnemyDefinition",new[]{"Assets/_Game"})
            .Select(g=>AssetDatabase.LoadAssetAtPath<EnemyDefinition>(AssetDatabase.GUIDToAssetPath(g))).ToArray();
        int level=Run.Player.PermanentLevel;
        try
        {
            foreach(var def in definitions)foreach(int wave in new[]{30,70,100,150})foreach(int playerLevel in new[]{1,5})
            {
                Set(actor,"definition",def);var stats=EndlessDifficultyRevisionTests.Profile.CalculateStats(def,wave);
                typeof(EnemyActor).GetProperty("RuntimeStats",Flags).SetValue(actor,stats);
                typeof(PlayerActor).GetProperty("PermanentLevel",Flags).SetValue(Run.Player,playerLevel);
                int hit=CombatAmounts.Round(combat.CalculateGemClearDamage(new BoardClearContext(actor.AssignedGemType,3,0,BoardClearSource.Match)));
                float threshold=stagger.DamageThreshold,decay=stagger.OffColorDecayDamage;
                Assert.That(hit,Is.GreaterThan(decay),def.EnemyId+" earns more than one unanswered move loses");
                float buildup=0;int moves=0;
                while(buildup<threshold && moves<2000){buildup+=hit;moves++;if(buildup<threshold){buildup=Mathf.Max(0,buildup-decay);moves++;}}
                Assert.That(buildup,Is.GreaterThanOrEqualTo(threshold),def.EnemyId);
                rows.Add(FormattableString.Invariant($"{def.EnemyId},{wave},{playerLevel},{stats.MaxHealth},{threshold:0.##},{decay},{100*decay/threshold:0.###},{hit},{moves}"));
            }
        }
        finally
        {
            Set(actor,"definition",original);typeof(EnemyActor).GetProperty("RuntimeStats",Flags).SetValue(actor,originalStats);
            typeof(PlayerActor).GetProperty("PermanentLevel",Flags).SetValue(Run.Player,level);
        }
        Directory.CreateDirectory(".utmp/UnifiedCombat");File.WriteAllLines(".utmp/UnifiedCombat/StaggerBalance.csv",rows);
        yield return null;
    }
}
