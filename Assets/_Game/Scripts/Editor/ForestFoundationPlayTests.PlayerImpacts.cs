using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public sealed partial class ForestFoundationPlayTests
{
    private PlayerAbilityController InstallSequenceAbility(CharacterAbilityDefinition ability,CharacterPassiveDefinition passive=null)
    {
        var definition=Object.Instantiate(Run.Player.Definition);
        Set(definition,"activeAbility",ability);Set(definition,"passiveAbility",passive);
        Set(Run.Player,"definition",definition);
        var controller=Run.Player.GetComponent<PlayerAbilityController>();controller.RefreshRuntime();
        return controller;
    }
    [UnityTest] public IEnumerator AttenderPaysOnceRetriesDurableObstacleAndRespectsCardDamage()
    {
        yield return Launch();QuietKitFixture();var board=Run.Board;
        var ability=ScriptableObject.CreateInstance<DungeonAttenderAbilityDefinition>();
        Set(ability,"movementBudget",3);
        var controller=InstallSequenceAbility(ability);var energy=Run.Player.GetComponent<PlayerAbilityEnergy>();energy.AddEnergy(90);
        Assert.That(board.TryQueuePlaceBarricades(Run.Waves.ActiveEnemies[0],1,1,3,EnemyBarricadeStyle.Wood),Is.True);
        yield return Stable();var target=board.CollectPlayerImpactTargets().Single(t=>t.Barrier).Cell;
        var impacts=new System.Collections.Generic.List<Vector2Int>();
        var runtime=Run.Player.GetComponent<DungeonAttenderRuntime>();
        runtime.Impacted+=(step,cell,survived)=>impacts.Add(cell);
        int energyBefore=energy.CurrentEnergy;
        Assert.That(controller.TryActivate(),Is.True);Assert.That(energy.CurrentEnergy,Is.EqualTo(energyBefore-controller.RequiredEnergy));
        Assert.That(controller.TryActivate(),Is.False);
        yield return Stable();Assert.That(impacts,Is.EqualTo(new[]{target,target,target}));
        Assert.That(board.CollectPlayerImpactTargets().Any(t=>t.Barrier),Is.False);
        Assert.That(board.CompletedValidPlayerMoves,Is.Zero);
        var upgrade=RunUpgradeRuntime.Current.Catalog.Upgrades.First(u=>u.UpgradeId=="prototype_siegebreaker");
        Assert.That(RunUpgradeRuntime.Current.TryApply(upgrade,5),Is.True);
        Assert.That(RunUpgradeResolver.ResolveBarricadeDurabilityDamage(1),Is.EqualTo(2));
        Set(ability,"movementBudget",2);energy.AddEnergy(90);impacts.Clear();
        Assert.That(board.TryQueuePlaceBarricades(Run.Waves.ActiveEnemies[0],1,1,3,EnemyBarricadeStyle.Wood),Is.True);
        yield return Stable();Assert.That(controller.TryActivate(),Is.True);yield return Stable();
        Assert.That(impacts.Count,Is.EqualTo(2));
        Assert.That(board.CollectPlayerImpactTargets().Any(t=>t.Barrier),Is.False,"the live +1 card reduces level 3 from three collisions to two");
        Object.Destroy(ability);
    }
    [UnityTest] public IEnumerator RicochetSixHitsUsePerRecipientPassiveResetNextCastAndSpendOnce()
    {
        yield return Launch();QuietKitFixture();var actor=Enemy("orc_trailguard");
        foreach(var enemy in Run.Waves.ActiveEnemies.ToArray())if(enemy!=actor)enemy.TryTakeDamage(20000);
        Set(actor,"currentHealth",10000);yield return Stable();
        var ability=ScriptableObject.CreateInstance<RicochetAbilityDefinition>();
        var passive=ScriptableObject.CreateInstance<RepeatRicochetPassiveDefinition>();
        var controller=InstallSequenceAbility(ability,passive);var energy=Run.Player.GetComponent<PlayerAbilityEnergy>();
        var hits=new System.Collections.Generic.List<int>();var runtime=Run.Player.GetComponent<RicochetRuntime>();
        runtime.RicochetHit+=(step,cell,recipient,damage)=>{Assert.That(recipient,Is.SameAs(actor));hits.Add(damage);};
        for(int cast=0;cast<2;cast++)
        {
            hits.Clear();energy.AddEnergy(90);int energyBefore=energy.CurrentEnergy;Assert.That(controller.TryActivate(),Is.True);
            Assert.That(energy.CurrentEnergy,Is.EqualTo(energyBefore-controller.RequiredEnergy));yield return Stable();
            Assert.That(hits,Is.EqualTo(new[]{5,10,15,20,20,20}));
        }
        Assert.That(Run.Board.CompletedValidPlayerMoves,Is.Zero);
        Object.Destroy(ability);Object.Destroy(passive);
    }
    [UnityTest] public IEnumerator SequenceAbilityRejectsBlockedBoardWithoutSpendingAndPausesBetweenImpacts()
    {
        yield return Launch();QuietKitFixture();var ability=ScriptableObject.CreateInstance<RicochetAbilityDefinition>();
        var controller=InstallSequenceAbility(ability);var energy=Run.Player.GetComponent<PlayerAbilityEnergy>();energy.AddEnergy(90);
        int energyBefore=energy.CurrentEnergy;
        using(Run.Board.AcquireExternalInputBlock())
        {Assert.That(controller.TryActivate(),Is.False);Assert.That(energy.CurrentEnergy,Is.EqualTo(energyBefore));}
        int impacts=0;Run.Player.GetComponent<RicochetRuntime>().Impacted+=(step,cell,survived)=>impacts++;
        Assert.That(controller.TryActivate(),Is.True);
        Time.timeScale=0;yield return new WaitForSecondsRealtime(.4f);Assert.That(impacts,Is.Zero);
        Time.timeScale=1;yield return Stable();Assert.That(impacts,Is.EqualTo(6));
        Object.Destroy(ability);
    }
    [UnityTest] public IEnumerator PlayerImpactSequenceRetainsOwnershipAndDoesNotAdvanceMoves()
    {
        yield return Launch();QuietKitFixture();
        int moves=Run.Board.CompletedValidPlayerMoves, impacts=0, completed=0;
        var board=Run.Board;var cell=new Vector2Int(0,0);var gem=board.GetGem(0,0);
        Assert.That(board.TryPlayerImpactSequence(new PlayerBoardImpactSequence {Count=1,
            StepSeconds=.05f,Select=(step,targets)=>cell,
            Impacted=(step,target,survived)=>{Assert.That(board.IsBusy,Is.True);impacts++;},
            Completed=()=>completed++}),Is.True);
        Assert.That(board.IsBusy,Is.True);Assert.That(impacts,Is.Zero,"effects start after acceptance");
        Assert.That(board.TryPlayerImpactSequence(new PlayerBoardImpactSequence {Count=1,Select=(step,targets)=>cell}),Is.False);
        yield return Stable();
        Assert.That(impacts,Is.EqualTo(1));Assert.That(completed,Is.EqualTo(1));
        Assert.That(board.GetGem(0,0),Is.Not.SameAs(gem));
        Assert.That(board.CompletedValidPlayerMoves,Is.EqualTo(moves));
    }

    [UnityTest] public IEnumerator PlayerImpactThreeHitBarrierSurvivesFirstAndBreaksOnThird()
    {
        yield return Launch();QuietKitFixture();var board=Run.Board;var owner=Run.Waves.ActiveEnemies[0];
        Assert.That(board.TryQueuePlaceBarricades(owner,1,1,3,EnemyBarricadeStyle.Wood),Is.True);
        yield return Stable();
        var target=board.CollectPlayerImpactTargets().Single(t=>t.Barrier);var cell=target.Cell;
        for(int hit=1;hit<=3;hit++)
        {
            bool survived=false;
            Assert.That(board.TryPlayerImpactSequence(new PlayerBoardImpactSequence {Count=1,
                Select=(step,targets)=>cell,Impacted=(step,at,remaining)=>survived=remaining}),Is.True);
            yield return Stable();
            Assert.That(survived,Is.EqualTo(hit<3));
            if(hit<3)Assert.That(board.CollectPlayerImpactTargets().Single(t=>t.Cell==cell).Durability,Is.EqualTo(3-hit));
            else Assert.That(board.GetGem(cell.x,cell.y),Is.Not.Null,"one authoritative refill restores the opened cell");
        }
    }

    [UnityTest] public IEnumerator PlayerImpactCancellationSettlesAndNeverDuplicatesCompletion()
    {
        yield return Launch();QuietKitFixture();var board=Run.Board;
        bool cancel=false;int impacts=0,completed=0;
        Assert.That(board.TryPlayerImpactSequence(new PlayerBoardImpactSequence {Count=7,StepSeconds=.05f,
            Select=(step,targets)=>targets.First(t=>t.Gem!=null).Cell,
            Cancelled=()=>cancel,Impacted=(step,cell,survived)=>{impacts++;cancel=true;},Completed=()=>completed++}),Is.True);
        yield return Stable();
        Assert.That(impacts,Is.EqualTo(1));Assert.That(completed,Is.EqualTo(1));Assert.That(board.IsBusy,Is.False);
    }

    [UnityTest] public IEnumerator AttenderPrioritizesAndClearsVineWithoutAPlayerMove()
    {
        yield return Launch();QuietKitFixture();var board=Run.Board;
        foreach(var enemy in Run.Waves.ActiveEnemies)Set(enemy,"currentHealth",10000);
        var target=board.GetGem(3,3);int identity=target.BoardIdentity;
        board.QueueEnvironmentalVine(target);yield return Stable();
        Assert.That(board.IsCellVined(3,3),Is.True);
        var ability=ScriptableObject.CreateInstance<DungeonAttenderAbilityDefinition>();Set(ability,"movementBudget",1);
        var controller=InstallSequenceAbility(ability);Run.Player.GetComponent<PlayerAbilityEnergy>().AddEnergy(100);
        Vector2Int? hit=null;Run.Player.GetComponent<DungeonAttenderRuntime>().Impacted+=(step,cell,survived)=>hit=cell;
        Assert.That(controller.TryActivate(),Is.True);yield return Stable();
        Assert.That(hit,Is.EqualTo(new Vector2Int(3,3)));Assert.That(board.FindGemIdentity(identity),Is.Null);
        Assert.That(board.VineCount,Is.Zero);Assert.That(board.CompletedValidPlayerMoves,Is.Zero);
        Object.Destroy(ability);
    }

    [UnityTest] public IEnumerator PlayerImpactClearsBifrostSafelyAndChainsSpecialsBeforeSettlement()
    {
        yield return Launch();QuietKitFixture();var board=Run.Board;
        foreach(var enemy in Run.Waves.ActiveEnemies)Set(enemy,"currentHealth",10000);
        var zone=Run.Zone.Definition;var previous=zone.frostwakeRules;
        var rules=ScriptableObject.CreateInstance<FrostwakeRules>();zone.frostwakeRules=rules;
        try
        {
            Gem frozen=null;
            Assert.That(board.TryQueueEncasedGem(Run.Waves.ActiveEnemies[0],FrostStage.Bifrost,g=>frozen=g),Is.True);
            yield return Stable();Assert.That(frozen,Is.Not.Null);
            int identity=frozen.BoardIdentity,health=Run.Player.CurrentHealth;
            var cell=new Vector2Int(frozen.Column,frozen.Row);
            Assert.That(board.TryPlayerImpactSequence(new PlayerBoardImpactSequence {Count=1,Select=(step,targets)=>cell}),Is.True);
            yield return Stable();Assert.That(board.FindGemIdentity(identity),Is.Null);
            Assert.That(Run.Player.CurrentHealth,Is.EqualTo(health),"ability clears never retaliate from Bifrost");
            Assert.That(Run.Player.Statuses.Has(PlayerStatusKind.Frostbite),Is.False);
            var row=board.GetGem(0,0);row.SetSpecialType(GemSpecialType.RowBomb);
            var column=board.GetGem(1,0);column.SetSpecialType(GemSpecialType.ColumnBomb);
            var crystal=board.GetGem(2,0);crystal.SetSpecialType(GemSpecialType.ColorCrystal);
            int[] consumed={row.BoardIdentity,column.BoardIdentity,crystal.BoardIdentity,board.GetGem(1,board.Height-1).BoardIdentity};
            int completed=0;
            Assert.That(board.TryPlayerImpactSequence(new PlayerBoardImpactSequence {Count=1,
                Select=(step,targets)=>Vector2Int.zero,Completed=()=>completed++}),Is.True);
            yield return Stable();
            foreach(int id in consumed)Assert.That(board.FindGemIdentity(id),Is.Null,"special chain consumed identity "+id);
            Assert.That(completed,Is.EqualTo(1));Assert.That(board.CompletedValidPlayerMoves,Is.Zero);
            for(int y=0;y<board.Height;y++)for(int x=0;x<board.Width;x++)Assert.That(board.GetGem(x,y),Is.Not.Null);
        }
        finally {zone.frostwakeRules=previous;Object.Destroy(rules);}
    }
}
