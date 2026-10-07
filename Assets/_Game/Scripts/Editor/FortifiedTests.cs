using NUnit.Framework;
using UnityEngine;
using System.Reflection;

public sealed class FortifiedTests
{
    private GameObject host,otherHost;
    private EnemyDefinition definition;
    private EnemyActor actor,other;
    [SetUp] public void Setup()
    {
        definition=ScriptableObject.CreateInstance<EnemyDefinition>();
        host=new GameObject("Fortified recipient");actor=host.AddComponent<EnemyActor>();
        otherHost=new GameObject("Intercepted recipient");other=otherHost.AddComponent<EnemyActor>();
        foreach(var target in new[]{actor,other})
        {
            // Same isolated actor setup as EnemyDamageResultTests; no fake portrait.
            Set(target,"definition",definition);Set(target,"<RuntimeStats>k__BackingField",new EnemyRuntimeStats(1,1,100,5,0,5,3));
            Set(target,"currentHealth",100);Set(target,"isInitialized",true);
        }
    }
    private void Set(EnemyActor target,string field,object value)=>typeof(EnemyActor)
        .GetField(field,BindingFlags.Instance|BindingFlags.NonPublic).SetValue(target,value);
    [TearDown] public void Cleanup()
    {Object.DestroyImmediate(host);Object.DestroyImmediate(otherHost);Object.DestroyImmediate(definition);}
    [Test] public void OneStackHalvesOneDirectPacketAndPeriodicDoesNotConsume()
    {
        actor.GrantFortified(2);int before=actor.CurrentHealth;
        actor.ResolveDirectDamage(20);Assert.That(actor.CurrentHealth,Is.EqualTo(before-10));Assert.That(actor.FortifiedStacks,Is.EqualTo(1));
        actor.ResolveDamageWithoutFeedback(10);Assert.That(actor.CurrentHealth,Is.EqualTo(before-20));Assert.That(actor.FortifiedStacks,Is.EqualTo(1));
        actor.ResolveDirectDamage(0);Assert.That(actor.FortifiedStacks,Is.EqualTo(1));
        actor.ResolveDirectDamage(20);Assert.That(actor.CurrentHealth,Is.EqualTo(before-30));Assert.That(actor.FortifiedStacks,Is.Zero);
    }
    [Test] public void ShieldGateAndInterceptionConsumeOnlyTheActualRecipientsOnePearl()
    {
        actor.GrantFortified(2);other.GrantFortified(2);other.GrantShield(5);actor.SetDamageRedirectTarget(other);
        int hp=other.CurrentHealth;actor.ResolveDirectDamage(40);
        Assert.That(actor.FortifiedStacks,Is.EqualTo(2));Assert.That(other.FortifiedStacks,Is.EqualTo(1));
        Assert.That(other.CurrentShield,Is.Zero);Assert.That(other.CurrentHealth,Is.EqualTo(hp));
        actor.ResolveDirectDamage(40);Assert.That(other.CurrentHealth,Is.EqualTo(hp-20));Assert.That(other.FortifiedStacks,Is.Zero);
    }
    [Test] public void CapSaveRestoreAndFullMitigationDoNotSpendExtraPearls()
    {
        Assert.That(actor.GrantFortified(9),Is.EqualTo(2));Assert.That(actor.GrantFortified(1),Is.Zero);
        var saved=actor.CaptureContinuation(0);actor.ResolveDirectDamage(10);actor.RestoreContinuation(saved);
        Assert.That(actor.FortifiedStacks,Is.EqualTo(2));
        actor.IncomingDamageMultiplier=()=>0;actor.ResolveDirectDamage(20);Assert.That(actor.FortifiedStacks,Is.EqualTo(2));
        actor.IncomingDamageMultiplier=null;actor.SetWardedSource(this,()=>true);
        int hp=actor.CurrentHealth;actor.ResolveDirectDamage(40);Assert.That(actor.CurrentHealth,Is.EqualTo(hp-15));
        Assert.That(actor.FortifiedStacks,Is.EqualTo(1));
        actor.RestoreContinuation(new EnemyCombatSnapshot{health=50});Assert.That(actor.FortifiedStacks,Is.Zero,"old saves have no invented buff");
    }
}
