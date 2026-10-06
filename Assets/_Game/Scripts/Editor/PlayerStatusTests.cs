using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

public sealed class PlayerStatusTests
{
    private readonly List<Object> cleanup = new List<Object>();
    private PlayerActor player;
    [SetUp] public void SetUp()
    {
        var go = new GameObject("Status fixture"); cleanup.Add(go);
        player = go.AddComponent<PlayerActor>();
        player.Initialize(Resources.Load<PlayerDefinition>("Players/Player_Skeleton"), 1000);
    }
    [TearDown] public void TearDown()
    {
        foreach (var item in cleanup.AsEnumerable().Reverse()) if (item != null) Object.DestroyImmediate(item);
        cleanup.Clear(); Time.timeScale = 1;
    }
    private PlayerStatusDefinition Status(PlayerStatusKind kind, float multiplier = 1, int damage = 5)
    {
        var value = ScriptableObject.CreateInstance<PlayerStatusDefinition>(); cleanup.Add(value);
        value.kind = kind; value.durationMoves = 3; value.multiplier = multiplier; value.damagePerMove = damage;
        return value;
    }
    private EnemyActor Enemy(long id)
    {
        var go = new GameObject("Fear source"); cleanup.Add(go);
        var actor = go.AddComponent<EnemyActor>(); var stagger = go.AddComponent<EnemyStagger>();
        var data = ScriptableObject.CreateInstance<EnemyDefinition>(); cleanup.Add(data);
        Set(actor, "definition", data);
        Set(actor, "<RuntimeStats>k__BackingField", new EnemyRuntimeStats(1, 1, 1000, 10, 0, 10, 3));
        Set(actor, "currentHealth", 1000); Set(actor, "isInitialized", true);
        Set(stagger, "enemyActor", actor);
        typeof(EnemyActor).GetProperty("PersistentId").SetValue(actor, id);
        return actor;
    }
    private static void Set(object target, string field, object value) => target.GetType()
        .GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);
    [Test] public void CentralEnemyDamageCombinesStatusesAfterRedirectExactlyOnce()
    {
        var a = Enemy(50); var b = Enemy(51);
        var go = new GameObject("Inactive session fixture"); go.SetActive(false); cleanup.Add(go);
        var session = go.AddComponent<RunSession>();
        Set(session, "<Player>k__BackingField", player);
        typeof(RunSession).GetProperty("Current").SetValue(null, session);
        try
        {
            player.Statuses.Apply(Status(PlayerStatusKind.Weakened, .75f));
            player.Statuses.Apply(Status(PlayerStatusKind.Fear, .75f), b);
            Set(a, "damageRedirectTarget", b);
            a.ResolveDirectDamage(80);
            Assert.That(a.CurrentHealth, Is.EqualTo(1000));
            Assert.That(b.CurrentHealth, Is.EqualTo(955));
            b.ResolveDamageWithoutFeedback(80);
            Assert.That(b.CurrentHealth, Is.EqualTo(910), "Player poison damage uses the same modifiers");
        }
        finally { typeof(RunSession).GetProperty("Current").SetValue(null, null); }
    }
    [Test] public void RefreshDoesNotStackAndDifferentDamageChannelsCompose()
    {
        var a = Enemy(1); var b = Enemy(2);
        var weakened = Status(PlayerStatusKind.Weakened, .75f);
        player.Statuses.Apply(weakened); player.Statuses.Apply(weakened);
        Assert.That(player.Statuses.Effects.Count, Is.EqualTo(1));
        player.Statuses.Apply(Status(PlayerStatusKind.Fear, .75f), a);
        Assert.That(player.Statuses.OutgoingMultiplier(a), Is.EqualTo(.5625f));
        Assert.That(player.Statuses.OutgoingMultiplier(b), Is.EqualTo(.75f));
        player.Statuses.BeginMove(1); player.Statuses.CompleteMove(1);
        Assert.That(player.Statuses.Remaining(PlayerStatusKind.Weakened), Is.EqualTo(2));
        player.Statuses.Apply(weakened);
        Assert.That(player.Statuses.Remaining(PlayerStatusKind.Weakened), Is.EqualTo(3));
    }
    [Test] public void FearClearsOnlyForItsDeadOrStaggeredSourceAndCannotBindAReplacement()
    {
        var a = Enemy(10); var b = Enemy(11);
        var fear = Status(PlayerStatusKind.Fear, .75f);
        player.Statuses.Apply(fear, a); player.Statuses.Apply(fear, b);
        a.GetComponent<EnemyStagger>().ApplyStagger(2, 3);
        Assert.That(player.Statuses.OutgoingMultiplier(a), Is.EqualTo(1));
        Assert.That(player.Statuses.OutgoingMultiplier(b), Is.EqualTo(.75f));
        var saved = player.Statuses.Capture();
        b.TryTakeDamage(100000);
        Assert.That(player.Statuses.Has(PlayerStatusKind.Fear), Is.False);
        player.Statuses.Restore(saved, id => id == 12 ? Enemy(12) : null);
        Assert.That(player.Statuses.Has(PlayerStatusKind.Fear), Is.False);
    }
    [Test] public void BurnTicksOncePerCompletedAcceptedMoveAndCanBeExtinguished()
    {
        player.Statuses.Apply(Status(PlayerStatusKind.Burn));
        player.Statuses.BeginMove(1);
        Assert.That(player.CurrentHealth, Is.EqualTo(1000), "Acceptance alone is not another damage pipeline");
        player.Statuses.CompleteMove(1); player.Statuses.CompleteMove(1);
        Assert.That(player.CurrentHealth, Is.EqualTo(995));
        player.Statuses.CompleteMove(2); player.Statuses.CompleteMove(3); player.Statuses.CompleteMove(4);
        Assert.That(player.CurrentHealth, Is.EqualTo(985));
        Assert.That(player.Statuses.Has(PlayerStatusKind.Burn), Is.False);
        player.Statuses.Apply(Status(PlayerStatusKind.Burn));
        Assert.That(player.Statuses.ExtinguishBurn(), Is.True);
        player.Statuses.CompleteMove(5); Assert.That(player.CurrentHealth, Is.EqualTo(985));
    }
    [Test] public void ApplicationDuringAcceptedActionKeepsThreeFutureResponses()
    {
        player.Statuses.BeginMove(1); player.Statuses.Apply(Status(PlayerStatusKind.Burn));
        player.Statuses.CompleteMove(1);
        Assert.That(player.CurrentHealth, Is.EqualTo(1000));
        Assert.That(player.Statuses.Remaining(PlayerStatusKind.Burn), Is.EqualTo(3));
    }
    [Test] public void HealingIncomingDamageAndShieldsKeepSeparateChannels()
    {
        player.Statuses.Apply(Status(PlayerStatusKind.Frostbite, 1.25f));
        player.TryTakeDamage(40); Assert.That(player.CurrentHealth, Is.EqualTo(950));
        player.Statuses.Apply(Status(PlayerStatusKind.Wounded, .5f));
        Assert.That(player.Heal(40), Is.EqualTo(20));
        Assert.That(player.GrantShield(20), Is.EqualTo(20));
        player.TryTakeDamage(100); Assert.That(player.CurrentHealth, Is.EqualTo(970), "Existing shield gate remains authoritative");
    }
    [Test] public void SappedReducesGenerationWithoutChangingStorage()
    {
        var energy = player.gameObject.AddComponent<PlayerAbilityEnergy>(); energy.AddEnergy(20);
        int stored = energy.CurrentEnergy;
        player.Statuses.Apply(Status(PlayerStatusKind.Sapped, .5f));
        Assert.That(energy.CurrentEnergy, Is.EqualTo(stored));
        Assert.That(player.Statuses.GeneratedEnergy(9), Is.EqualTo(4));
        Assert.That(player.Statuses.GeneratedEnergy(-1), Is.Zero);
    }
    [Test] public void SnapshotRoundTripKeepsRemainingMovesSourceAndTickDeduplication()
    {
        var enemy = Enemy(30);
        player.Statuses.Apply(Status(PlayerStatusKind.Fear, .75f), enemy);
        player.Statuses.Apply(Status(PlayerStatusKind.Burn)); player.Statuses.CompleteMove(1);
        var saved = JsonUtility.FromJson<PlayerStatusSnapshot>(JsonUtility.ToJson(player.Statuses.Capture()));
        player.Statuses.Clear(); player.Statuses.Restore(saved, id => id == 30 ? enemy : null);
        player.Statuses.CompleteMove(1); Assert.That(player.CurrentHealth, Is.EqualTo(995));
        Assert.That(player.Statuses.OutgoingMultiplier(enemy), Is.EqualTo(.75f));
        player.Statuses.CompleteMove(2); Assert.That(player.CurrentHealth, Is.EqualTo(990));
        Assert.That(saved.effects.First(e => e.kind == PlayerStatusKind.Burn).remainingMoves, Is.EqualTo(2), "Save data is detached");
        Time.timeScale = 0;
        Assert.That(player.Statuses.Remaining(PlayerStatusKind.Burn), Is.EqualTo(1), "No wall-time expiration");
        player.TryTakeDamage(100000); Assert.That(player.Statuses.Effects, Is.Empty);
        player.TryRevive(100); Assert.That(player.Statuses.Effects, Is.Empty);
    }
}
