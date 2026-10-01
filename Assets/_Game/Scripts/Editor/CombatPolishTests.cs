using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

[NonParallelizable]
public sealed class CombatPolishTests
{
    private readonly List<Object> objects = new List<Object>();
    private IDisposable profile;
    private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic;
    [SetUp] public void SetUp() => profile = AccountProgression.UseDisposableProfile(
        Path.GetFullPath(".utmp/CombatPolish/" + Guid.NewGuid().ToString("N") + ".json"));
    [TearDown] public void TearDown()
    {
        for (int i = objects.Count - 1; i >= 0; i--) if (objects[i] != null) Object.DestroyImmediate(objects[i]);
        objects.Clear(); profile.Dispose();
    }

    [TestCase(0,0)] [TestCase(1,5)] [TestCase(7,5)] [TestCase(8,10)]
    [TestCase(31,30)] [TestCase(32.5,35)] [TestCase(98,100)]
    public void CleanAmountsRoundNearestWithPositiveMinimum(double input, int expected) =>
        Assert.That(CombatAmounts.Round(input), Is.EqualTo(expected));

    [Test] public void ShieldBreakGatesPlayerHitAndReentrantShieldGrant()
    {
        var player = Player(); int hp = player.CurrentHealth, hpEvents = 0;
        player.DamageTaken += (_,__) => hpEvents++;
        player.GrantShield(10);
        bool restored = false;
        player.ShieldDamaged += (_,__) => { if (!restored) { restored = true; player.GrantShield(5); } };
        player.TryTakeDamage(30);
        Assert.That(player.CurrentHealth, Is.EqualTo(hp));
        Assert.That(player.CurrentShield, Is.EqualTo(5), "break listeners cannot make this hit consume a second shield pool");
        player.TryTakeDamage(30);
        Assert.That(player.CurrentHealth, Is.EqualTo(hp));
        Assert.That(hpEvents, Is.Zero);
        player.TryTakeDamage(30);
        Assert.That(player.CurrentHealth, Is.EqualTo(hp-30)); Assert.That(hpEvents, Is.EqualTo(1));
    }

    [Test] public void DamageHealingShieldsAndLegacyContinuationRemainClean()
    {
        var player = Player();
        player.RestoreContinuation(new PlayerCombatSnapshot { maximumHealth=113,health=71,maximumShield=52,shield=13 });
        Assert.That(player.MaximumHealth, Is.EqualTo(115)); Assert.That(player.CurrentHealth, Is.EqualTo(70));
        Assert.That(player.MaximumShield, Is.EqualTo(50)); Assert.That(player.CurrentShield, Is.EqualTo(15));
        for (int amount=1;amount<38;amount++)
        {
            player.RestoreToFullHealth(); player.TryTakeDamage(amount); player.Heal(amount/2); player.GrantShield(amount);
            Assert.That(player.CurrentHealth%5, Is.Zero); Assert.That(player.CurrentShield%5, Is.Zero);
        }
    }

    [Test] public void AllEnemyStatsRemainCleanAcrossOpeningAndEndlessWaves()
    {
        var difficulty=AssetDatabase.LoadAssetAtPath<DifficultyProfile>("Assets/_Game/Data/Balance/DifficultyProfile_Standard.asset");
        foreach (string guid in AssetDatabase.FindAssets("t:EnemyDefinition"))
        {
            var definition=AssetDatabase.LoadAssetAtPath<EnemyDefinition>(AssetDatabase.GUIDToAssetPath(guid));
            foreach (int wave in new[]{1,2,7,30,31,100,101,500,1000})
            {
                var stats=difficulty.CalculateStats(definition,wave,1f);
                Assert.That(stats.MaxHealth%5,Is.Zero,definition.name+" HP wave "+wave);
                Assert.That(stats.Damage%5,Is.Zero,definition.name+" damage wave "+wave);
                Assert.That(stats.FollowUpDamage%5,Is.Zero,definition.name+" followup wave "+wave);
            }
        }
    }

    [TestCase(28,true)] [TestCase(30,false)] [TestCase(32,true)] [TestCase(33,false)] [TestCase(100,true)] [TestCase(1000,true)]
    public void EndlessCardCadence(int wave, bool expected) => Assert.That(BalanceV1.Current.OffersCard(wave),Is.EqualTo(expected));

    private PlayerActor Player()
    {
        var go=new GameObject("Clean combat player"); objects.Add(go);
        var player=go.AddComponent<PlayerActor>();
        typeof(PlayerActor).GetField("initializeOnStart",Flags).SetValue(player,false);
        player.Initialize(Resources.Load<PlayerDefinition>("Players/Player_Skeleton")); return player;
    }
}
