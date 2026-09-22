using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

public sealed class BalanceV1Tests
{
    private string path;
    private IDisposable profile, mastery;
    private readonly List<GameObject> objects = new List<GameObject>();
    private AccountProgression Account => AccountProgression.Current;
    private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

    [SetUp] public void SetUp()
    {
        path = Path.GetFullPath(".utmp/BalanceTests/" + Guid.NewGuid().ToString("N") + ".json");
        profile = AccountProgression.UseDisposableProfile(path);
        mastery = GemMasterySettings.UseTemporaryLoadout(GemMasteryLoadout.Default);
    }
    [TearDown] public void TearDown()
    {
        for (int i = objects.Count - 1; i >= 0; i--) if (objects[i] != null) UnityEngine.Object.DestroyImmediate(objects[i]);
        objects.Clear(); mastery?.Dispose(); profile?.Dispose();
    }
    private void Seed(int gold=10000, int bardley=1, int skeleton=1, int potions=0, int bombs=0)
    {
        profile.Dispose();
        var save = new AccountSave { gold=gold, potions=potions, bombs=bombs };
        save.characters.Add(new CharacterProgress { id="bardley", level=bardley });
        save.characters.Add(new CharacterProgress { id="skeleton", level=skeleton });
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        File.WriteAllText(path, JsonUtility.ToJson(save));
        profile = AccountProgression.UseDisposableProfile(path);
    }
    private PlayerActor Player(string id)
    {
        var go = new GameObject("Balance test player"); objects.Add(go);
        var player = go.AddComponent<PlayerActor>();
        player.Initialize(Resources.Load<PlayerDefinition>("Players/Player_" + (id=="bardley"?"Bardley":"Skeleton")));
        return player;
    }
    private RunUpgradeRuntime Runtime(PlayerActor player)
    {
        var go = new GameObject("Balance test upgrades"); objects.Add(go);
        var waves=go.AddComponent<WaveController>();
        var runtime=go.AddComponent<RunUpgradeRuntime>();
        runtime.Configure(Resources.Load<RunUpgradeCatalog>("RunUpgrades/PrototypeRunUpgradeCatalog"),player,waves);
        return runtime;
    }
    private static RunUpgradeDefinition Card(string name) => Resources.Load<RunUpgradeDefinition>("RunUpgrades/"+name);
    private static T Data<T>(string path) where T:UnityEngine.Object => AssetDatabase.LoadAssetAtPath<T>("Assets/_Game/"+path+".asset");

    [Test] public void FreshProfileHasDirectionalBombsAndCrystalButMasteryBombsStayLocked()
    {
        Assert.That(Account.Gold,Is.Zero);
        foreach(GemSpecialType type in Enum.GetValues(typeof(GemSpecialType)))
            Assert.That(Account.IsUnlocked(type),Is.EqualTo(type==GemSpecialType.ColorCrystal||type==GemSpecialType.RowBomb||type==GemSpecialType.ColumnBomb),type.ToString());
        foreach(GemMasteryShape shape in Enum.GetValues(typeof(GemMasteryShape)))
        {
            Assert.That(GemMasterySettings.GetReward(shape),Is.EqualTo(GemMasteryReward.ColorCrystal));
            Assert.That(GemMasterySettings.SetReward(shape,GemMasteryReward.PoisonBomb),Is.False);
        }
    }
    [Test] public void CharacterLevelsAreIndependentAndSharedUnlocksUseHighestLevel()
    {
        Seed(); int initial=Account.Gold;
        Assert.That(Account.TryLevelUp("bardley"),Is.True);
        Assert.That(Account.Gold,Is.EqualTo(initial-25));
        Assert.That(Account.Level("bardley"),Is.EqualTo(2));
        Assert.That(Account.Level("skeleton"),Is.EqualTo(1));
        Assert.That(Account.IsUnlocked(GemSpecialType.RowBomb),Is.True);
        Assert.That(Account.TryLevelUp("skeleton"),Is.True);
        Assert.That(Account.IsUnlocked(GemSpecialType.PoisonBomb),Is.False,"levels must not sum");
        for(int level=2;level<7;level++)Assert.That(Account.TryLevelUp("bardley"),Is.True);
        foreach(var type in new[]{GemSpecialType.PoisonBomb,GemSpecialType.HealingBomb,GemSpecialType.ShieldBomb})Assert.That(Account.IsUnlocked(type),Is.True);
        var loaded=new AccountProgression(path);
        Assert.That(loaded.Level("bardley"),Is.EqualTo(7)); Assert.That(loaded.Level("skeleton"),Is.EqualTo(2));
        Assert.That(loaded.IsUnlocked(GemSpecialType.ShieldBomb),Is.True);
        Assert.That(loaded.Gold,Is.EqualTo(Account.Gold));
    }
    [TestCase("bardley", "skeleton")]
    [TestCase("skeleton", "bardley")]
    public void EitherCharacterUnlocksMasteryForTheOtherAtThreeFiveAndSeven(string upgraded,string other)
    {
        Seed();
        var notifications=new List<GemSpecialType>();Account.Unlocked+=notifications.Add;
        using(CharacterSelectionSettings.UseTemporarySelection(other))
        {
            for(int level=1;level<=7;level++)
            {
                if(level>1)Assert.That(Account.TryLevelUp(upgraded),Is.True);
                Assert.That(Account.Level(other),Is.EqualTo(1));
                foreach(var pair in new[]{(GemMasteryReward.PoisonBomb,3),(GemMasteryReward.HealBomb,5),(GemMasteryReward.ShieldBomb,7)})
                {
                    Assert.That(Account.IsUnlocked(pair.Item1),Is.EqualTo(level>=pair.Item2),$"{pair.Item1} at level {level}");
                    GemMasterySettings.SetReward(GemMasteryShape.LShape,GemMasteryReward.ColorCrystal);
                    Assert.That(GemMasterySettings.SetReward(GemMasteryShape.LShape,pair.Item1),Is.EqualTo(level>=pair.Item2));
                }
            }
        }
        CollectionAssert.AreEqual(new[]{GemSpecialType.PoisonBomb,GemSpecialType.HealingBomb,GemSpecialType.ShieldBomb},notifications);
    }
    [TestCase("bardley", "skeleton")]
    [TestCase("skeleton", "bardley")]
    public void ResetPersistsOnlySelectedLevelAndKeepsEarnedUnlocksAndInventory(string reset,string other)
    {
        Seed(bardley:7,skeleton:7,potions:4,bombs:6);
        Assert.That(Account.SetEquipped(ConsumableKind.Bomb,true),Is.True);
        GemMasterySettings.SetReward(GemMasteryShape.CrossShape,GemMasteryReward.ShieldBomb);
        int notifications=0;Account.Unlocked+=_=>notifications++;
        Assert.That(Account.TryResetLevel(reset),Is.True);
        Assert.That(Account.Level(reset),Is.EqualTo(1));Assert.That(Account.Level(other),Is.EqualTo(7));
        Assert.That(Account.TryResetLevel(other),Is.True);
        var loaded=new AccountProgression(path);
        Assert.That(loaded.HighestLevel,Is.EqualTo(1));Assert.That(loaded.Gold,Is.EqualTo(10000),"no refund or fee");
        Assert.That(loaded.Owned(ConsumableKind.HealthPotion),Is.EqualTo(4));Assert.That(loaded.Owned(ConsumableKind.Bomb),Is.EqualTo(6));
        Assert.That(loaded.Equipped(ConsumableKind.Bomb),Is.True);
        foreach(var type in new[]{GemSpecialType.PoisonBomb,GemSpecialType.HealingBomb,GemSpecialType.ShieldBomb})Assert.That(loaded.IsUnlocked(type),Is.True);
        Assert.That(GemMasterySettings.GetReward(GemMasteryShape.CrossShape),Is.EqualTo(GemMasteryReward.ShieldBomb));
        Assert.That(Player(reset).MaximumHealth,Is.EqualTo(reset=="bardley"?80:100));
        for(int level=1;level<7;level++)Assert.That(Account.TryLevelUp(reset),Is.True);
        Assert.That(notifications,Is.Zero,"reset/relevel must not issue already earned unlocks again");
    }
    [Test] public void ResetRejectsInvalidCharactersLevelOneAndActiveRuns()
    {
        Seed(bardley:7);
        Assert.That(Account.TryResetLevel(null),Is.False);Assert.That(Account.TryResetLevel("unknown"),Is.False);
        Assert.That(Account.TryResetLevel("skeleton"),Is.False);
        string run=Account.BeginRun("bardley");string before=File.ReadAllText(path);
        Assert.That(Account.TryResetLevel("bardley"),Is.False);
        Assert.That(Account.Level("bardley"),Is.EqualTo(7));Assert.That(Account.ActiveRun.id,Is.EqualTo(run));
        Assert.That(File.ReadAllText(path),Is.EqualTo(before));
    }
    [Test] public void FailedResetDoesNotPublishOrChangeDiskOrMemory()
    {
        Seed(bardley:7);string before=File.ReadAllText(path);int changed=0;Account.Changed+=()=>changed++;
        using(var locked=new FileStream(path+".tmp",FileMode.Create,FileAccess.ReadWrite,FileShare.None))
        {
            LogAssert.Expect(LogType.Error,new System.Text.RegularExpressions.Regex("Could not save account"));
            Assert.That(Account.TryResetLevel("bardley"),Is.False);
        }
        Assert.That(changed,Is.Zero);Assert.That(Account.Level("bardley"),Is.EqualTo(7));
        Assert.That(File.ReadAllText(path),Is.EqualTo(before));
        Assert.That(Account.TryResetLevel("bardley"),Is.True);Assert.That(changed,Is.EqualTo(1));
    }
    [Test] public void PreviouslyEarnedLowerThresholdUnlocksRemainOwnedOnLoad()
    {
        Seed(bardley:5);
        var save=JsonUtility.FromJson<AccountSave>(File.ReadAllText(path));
        save.unlocked.Add(GemSpecialType.HealingBomb);save.unlocked.Add(GemSpecialType.ShieldBomb);
        File.WriteAllText(path,JsonUtility.ToJson(save));
        var loaded=new AccountProgression(path);
        Assert.That(loaded.IsUnlocked(GemSpecialType.HealingBomb),Is.True);
        Assert.That(loaded.IsUnlocked(GemSpecialType.ShieldBomb),Is.True);
    }
    [TestCase("bardley",1,80,10f,40)]
    [TestCase("bardley",5,112,12.6f,52)]
    [TestCase("bardley",20,232,22.35f,97)]
    [TestCase("skeleton",1,100,11f,45)]
    [TestCase("skeleton",5,140,13.2f,57)]
    [TestCase("skeleton",20,290,21.45f,102)]
    public void PermanentStatsRecalculateFromUnmodifiedAssets(string id,int level,int hp,float damage,int shield)
    {
        Seed(bardley:level,skeleton:level);var player=Player(id);var def=player.Definition;
        int immutableHp=def.BaseMaxHealth;
        Assert.That(player.MaximumHealth,Is.EqualTo(hp)); Assert.That(player.GemDamage,Is.EqualTo(damage).Within(.001f));
        Assert.That(player.MaximumShield,Is.EqualTo(shield)); Assert.That(player.AbilityDamageMultiplier,Is.EqualTo(def.AbilityMultiplierAtLevel(level)));
        player.Initialize(def);Assert.That(player.MaximumHealth,Is.EqualTo(hp));Assert.That(def.BaseMaxHealth,Is.EqualTo(immutableHp));
        Assert.That(Resources.Load<PlayerDefinition>("Players/Player_Bardley").ActiveAbility.EnergyCost,Is.EqualTo(80),"production cost; testing override retired");
    }
    [Test] public void PurchasesEquipmentAndTenOwnedThreeUsesAreAtomicAndIndependent()
    {
        Seed(potions:10,bombs:10);int gold=Account.Gold;
        Assert.That(Account.TryPurchase(ConsumableKind.HealthPotion),Is.True);
        Assert.That(Account.Gold,Is.EqualTo(gold-BalanceV1.Current.potionPrice));
        Assert.That(Account.Owned(ConsumableKind.HealthPotion),Is.EqualTo(11));
        Assert.That(Account.SetEquipped(ConsumableKind.HealthPotion,true),Is.True);
        Assert.That(Account.SetEquipped(ConsumableKind.Bomb,true),Is.True);
        string run=Account.BeginRun("skeleton");
        Assert.That(Account.Charges(run,ConsumableKind.HealthPotion),Is.EqualTo(3));
        for(int i=0;i<3;i++)Assert.That(Account.TrySpendCharge(run,ConsumableKind.Bomb),Is.True);
        Assert.That(Account.Owned(ConsumableKind.Bomb),Is.EqualTo(7));
        Assert.That(Account.Charges(run,ConsumableKind.HealthPotion),Is.EqualTo(3));
        Assert.That(Account.TrySpendCharge(run,ConsumableKind.Bomb),Is.False);
        Assert.That(Account.TrySpendCharge("wrong run",ConsumableKind.HealthPotion),Is.False);
        Assert.That(Account.SetEquipped(ConsumableKind.Bomb,false),Is.False);
        Assert.That(Account.TryPurchase(ConsumableKind.Bomb),Is.False);
        Assert.That(Account.FinalizeRun(run,"Quit"),Is.True);
        run=Account.BeginRun("skeleton");Assert.That(Account.Charges(run,ConsumableKind.Bomb),Is.EqualTo(3));
    }
    [TestCase("Defeat")][TestCase("Retry")][TestCase("Quit to Menu")][TestCase("Victory")]
    public void RewardsCreditExactlyOnceAndPartialWavesNeverCount(string reason)
    {
        string run=Account.BeginRun("bardley");
        for(int wave=1;wave<=10;wave++)Assert.That(Account.RecordWave(run,wave,wave==7,false),Is.True);
        Assert.That(Account.RecordWave(run,10,false,false),Is.False);
        Assert.That(Account.RecordWave(run,12,false,false),Is.False);
        Assert.That(Account.Gold,Is.Zero,"preview must not credit the wallet");
        var preview=Account.PreviewReward(reason);
        Assert.That(preview.progressGold,Is.EqualTo(62));Assert.That(preview.milestoneGold,Is.EqualTo(12));Assert.That(preview.bestGold,Is.EqualTo(20));
        Assert.That(Account.FinalizeRun(run,reason),Is.True);
        Assert.That(Account.Gold,Is.EqualTo(94)); Assert.That(Account.FinalizeRun(run,reason),Is.False);
        Assert.That(new AccountProgression(path).Gold,Is.EqualTo(94));
    }
    [Test] public void InterruptedRunSurvivesUntilExplicitSettlementAndKingFirstClearCannotRepeat()
    {
        string run=Account.BeginRun("skeleton");
        for(int wave=1;wave<=30;wave++)Assert.That(Account.RecordWave(run,wave,new[]{7,12,18,24}.Contains(wave),wave==30),Is.True);
        var preview=Account.PreviewReward("interrupted");
        var reloaded=new AccountProgression(path);Assert.That(reloaded.Gold,Is.Zero);
        Assert.That(reloaded.ActiveRun.id,Is.EqualTo(run));
        Assert.That(reloaded.FinalizeRun(run,"Victory"),Is.True);
        Assert.That(new AccountProgression(path).Gold,Is.EqualTo(preview.Total));
        Assert.That(reloaded.ActiveRun,Is.Null);
        run=reloaded.BeginRun("skeleton");
        for(int wave=1;wave<=30;wave++)Assert.That(reloaded.RecordWave(run,wave,false,wave==30),Is.True);
        Assert.That(reloaded.PreviewReward("repeat").firstClearGold,Is.Zero);
        Assert.That(reloaded.PreviewReward("repeat").bestGold,Is.Zero);
    }
    [Test] public void FailedDiskWriteDoesNotSpendWalletOrInventory()
    {
        Seed(potions:10);Assert.That(Account.SetEquipped(ConsumableKind.HealthPotion,true),Is.True);
        string run=Account.BeginRun("bardley");
        using(var locked=new FileStream(path+".tmp",FileMode.Create,FileAccess.ReadWrite,FileShare.None))
        {
            LogAssert.Expect(LogType.Error,new System.Text.RegularExpressions.Regex("Could not save account"));
            Assert.That(Account.TrySpendCharge(run,ConsumableKind.HealthPotion),Is.False);
        }
        Assert.That(Account.Owned(ConsumableKind.HealthPotion),Is.EqualTo(10));
        Assert.That(Account.Charges(run,ConsumableKind.HealthPotion),Is.EqualTo(3));
        Assert.That(Account.TrySpendCharge(run,ConsumableKind.HealthPotion),Is.True);
    }
    [Test] public void ShieldOverflowAndAegisCapDeliverUsefulValueAndReset()
    {
        Seed(bardley:5,skeleton:7);GemMasterySettings.SetReward(GemMasteryShape.CrossShape,GemMasteryReward.ShieldBomb);
        var player=Player("bardley");var runtime=Runtime(player);
        Assert.That(runtime.TryApply(Card("RunUpgrade_AegisReservoir"),5),Is.True);
        Assert.That(player.MaximumShield,Is.EqualTo(68));
        Assert.That(RunUpgradeResolver.ResolveShieldBombShield(33,runtime),Is.EqualTo(43));
        player.GrantShield(999);Assert.That(player.CurrentShield,Is.EqualTo(68));
        int hp=player.CurrentHealth;player.TryTakeDamage(100);
        Assert.That(player.CurrentShield,Is.Zero);Assert.That(player.CurrentHealth,Is.EqualTo(hp-7),"25% reduction, then shield, then HP overflow");
        runtime.ResetRun();Assert.That(player.MaximumShield,Is.EqualTo(52));Assert.That(player.MaximumHealth,Is.EqualTo(112));
    }
    [Test] public void CardTradeoffsStackCapsAndEligibilityRespectActualMechanics()
    {
        Seed(bardley:5);var player=Player("bardley");var runtime=Runtime(player);
        Assert.That(runtime.IsEligible(Card("RunUpgrade_CorrosiveFormula"),player,5),Is.False,"unlocked but unequipped");
        GemMasterySettings.SetReward(GemMasteryShape.LShape,GemMasteryReward.PoisonBomb);
        Assert.That(runtime.IsEligible(Card("RunUpgrade_CorrosiveFormula"),player,5),Is.True);
        Assert.That(runtime.IsEligible(Card("Prototype_ManaSpark"),player,5),Is.True,"production cost supports energy builds");
        var glass=Card("RunUpgrade_GlassCannon");
        Assert.That(runtime.TryApply(glass,5),Is.True);Assert.That(player.MaximumHealth,Is.EqualTo(95));
        Assert.That(runtime.TryApply(glass,5),Is.False);runtime.ResetRun();Assert.That(player.MaximumHealth,Is.EqualTo(112));
    }
    [Test] public void EveryRecipeHasValidMembersAndFitsBudgetAndSlots()
    {
        var spawn=Data<WaveSpawnProfile>("Data/Balance/WaveSpawnProfile_Standard");
        var db=Data<EnemyDatabase>("Data/Enemies/EnemyDatabase_Main");
        Assert.That(spawn.Recipes.Count,Is.GreaterThanOrEqualTo(16));
        foreach(var recipe in spawn.Recipes)
        {
            int eligible=0;
            for(int wave=recipe.minimumWave;wave<=recipe.maximumWave;wave++)
                if(recipe.TryBuild(wave,3,db,new HashSet<EnemyDefinition>(),out var members))
                {
                    eligible++;Assert.That(members.Sum(e=>e.ThreatCost),Is.LessThanOrEqualTo(spawn.ThreatBudget(wave)),recipe.id+" wave "+wave);
                    Assert.That(members.Count,Is.InRange(1,3));
                }
            Assert.That(eligible,Is.GreaterThan(0),recipe.id);
            Assert.That(recipe.TryBuild(recipe.minimumWave-1,3,db,new HashSet<EnemyDefinition>(),out _),Is.False);
        }
    }
    [Test] public void ConstrainedRandomFormationsAndUniqueEscortedMilestonesStayInBudget()
    {
        var spawn=Data<WaveSpawnProfile>("Data/Balance/WaveSpawnProfile_Standard");
        var db=Data<EnemyDatabase>("Data/Enemies/EnemyDatabase_Main");
        var go=new GameObject("Encounter tests");go.SetActive(false);objects.Add(go);var controller=go.AddComponent<WaveController>();
        Set(controller,"waveSpawnProfile",spawn);Set(controller,"enemyDatabase",db);
        for(int seed=1;seed<=100;seed++)
        {
            var seen=new HashSet<EnemyDefinition>();var rng=new System.Random(seed);Set(controller,"encounterRandom",rng);
            for(int wave=1;wave<=30;wave++)
            {
                var plan=spawn.CreatePlan(wave,rng);var leader=spawn.SelectMilestone(wave,rng,seen,out int count);
                if(leader!=null)
                {
                    Assert.That(seen.Add(leader),Is.True);Assert.That(count,Is.InRange(2,3));
                    var categories=new List<EnemyCategory>{leader.Category};while(categories.Count<count)categories.Add(EnemyCategory.Normal);
                    plan=new WaveSpawnPlan(wave,"test milestone",categories);
                }
                Set(controller,"currentWave",wave);Set(controller,"selectedMilestoneLeader",leader);
                typeof(WaveController).GetProperty("CurrentPlan").SetValue(controller,plan);
                var formation=(List<EnemyDefinition>)typeof(WaveController).GetMethod("BuildEncounter",Flags).Invoke(controller,new object[]{plan.EnemyCount});
                var actual=formation.Where(e=>e!=null).ToList();
                Assert.That(actual.Count,Is.InRange(leader!=null?2:1,3),"seed "+seed+" wave "+wave);
                Assert.That(actual.Sum(e=>e.ThreatCost),Is.LessThanOrEqualTo(spawn.ThreatBudget(wave)));
                Assert.That(actual.Count(e=>e.IsBoardDisruptor),Is.LessThanOrEqualTo(2));
                Assert.That(actual.Count(e=>e.IsSupport),Is.LessThanOrEqualTo(1));
                if(leader!=null)Assert.That(actual,Does.Contain(leader));
            }
            Assert.That(seen.Count,Is.EqualTo(5));
        }
    }
    [Test] public void LegacySelectionsMigrateWithoutWritingOrSummingProgress()
    {
        var legacy=new Dictionary<string,int>
        {
            {"DungeonMatcher.GemMastery.v1.LShape",(int)GemMasteryReward.PoisonBomb},
            {"DungeonMatcher.GemMastery.v1.CrossShape",(int)GemMasteryReward.ShieldBomb},
            {"DungeonMatcher.GemMastery.v1.TShape",999}
        };
        var save=new AccountSave();
        typeof(AccountProgression).GetMethod("MigrateLegacySelections",BindingFlags.Static|BindingFlags.NonPublic)
            .Invoke(null,new object[]{save,(Func<string,bool>)legacy.ContainsKey,(Func<string,int>)(key=>legacy[key])});
        Directory.CreateDirectory(Path.GetDirectoryName(path));File.WriteAllText(path,JsonUtility.ToJson(save));
        var migrated=new AccountProgression(path);
        Assert.That(migrated.Level("bardley"),Is.EqualTo(1));Assert.That(migrated.Level("skeleton"),Is.EqualTo(1));
        Assert.That(migrated.IsUnlocked(GemSpecialType.PoisonBomb),Is.True);Assert.That(migrated.IsUnlocked(GemSpecialType.ShieldBomb),Is.True);
        Assert.That(migrated.IsUnlocked(GemSpecialType.HealingBomb),Is.False);Assert.That(migrated.IsUnlocked(GemSpecialType.RowBomb),Is.True);
        Assert.That(legacy.Count,Is.EqualTo(3));Assert.That(legacy["DungeonMatcher.GemMastery.v1.TShape"],Is.EqualTo(999));
    }
    [Test] public void BackupRecoveryPreservesWholeTransactionsAndCorruptOriginals()
    {
        Seed();Assert.That(Account.TryPurchase(ConsumableKind.Bomb),Is.True);
        File.WriteAllText(path,"invalid primary");var recovered=new AccountProgression(path);
        Assert.That(recovered.Gold,Is.EqualTo(10000));Assert.That(recovered.Owned(ConsumableKind.Bomb),Is.Zero);
        File.WriteAllText(path+".bak","invalid backup");Assert.Throws<IOException>(()=>new AccountProgression(path));
        Assert.That(File.ReadAllText(path),Is.EqualTo("invalid primary"));Assert.That(File.ReadAllText(path+".bak"),Is.EqualTo("invalid backup"));
    }
    [Test] public void FailedPurchaseAndLevelUpLeaveWalletAndStatsUnchanged()
    {
        Seed();using(var locked=new FileStream(path+".tmp",FileMode.Create,FileAccess.ReadWrite,FileShare.None))
        {
            LogAssert.Expect(LogType.Error,new System.Text.RegularExpressions.Regex("Could not save account"));
            Assert.That(Account.TryPurchase(ConsumableKind.Bomb),Is.False);
            LogAssert.Expect(LogType.Error,new System.Text.RegularExpressions.Regex("Could not save account"));
            Assert.That(Account.TryLevelUp("bardley"),Is.False);
        }
        Assert.That(Account.Gold,Is.EqualTo(10000));Assert.That(Account.Owned(ConsumableKind.Bomb),Is.Zero);Assert.That(Account.Level("bardley"),Is.EqualTo(1));
    }
    [Test] public void ExistingRoyalDataAndMechanicAssertionsMatchBalanceV1()=>RoyalMilestoneValidation.Run();
    private static void Set(object target,string name,object value)=>target.GetType().GetField(name,Flags).SetValue(target,value);
}
