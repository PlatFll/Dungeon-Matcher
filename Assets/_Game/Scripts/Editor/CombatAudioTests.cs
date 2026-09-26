using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

[NonParallelizable]
public sealed class CombatAudioTests
{
    private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
    private readonly List<UnityEngine.Object> created = new List<UnityEngine.Object>();

    [TearDown]
    public void TearDown()
    {
        for (int i = created.Count - 1; i >= 0; i--)
            if (created[i] != null) UnityEngine.Object.DestroyImmediate(created[i]);
        created.Clear();
    }

    [Test]
    public void AllProductionCuesHaveShortMonoAssetsAndPositiveMixValues()
    {
        foreach (CombatSoundCue cue in Enum.GetValues(typeof(CombatSoundCue)))
        {
            var clip = Resources.Load<AudioClip>("Audio/Combat/" + cue);
            Assert.That(clip, Is.Not.Null, cue.ToString());
            Assert.That(clip.channels, Is.EqualTo(1));
            Assert.That(clip.length, Is.InRange(.05f, .6f));
            Assert.That(CombatSoundMix.Gain(cue), Is.InRange(.01f, .5f));
            Assert.That(CombatSoundMix.MinimumInterval(cue), Is.GreaterThan(0));
        }
    }

    [Test]
    public void SixSimultaneousLoudVoicesRetainHeadroom()
    {
        float requested = CombatSoundMix.Gain(CombatSoundCue.Explosion) * CombatSoundMix.VoiceCount;
        Assert.That(requested * CombatSoundMix.HeadroomScale(requested), Is.EqualTo(.8f).Within(.0001f));
        Assert.That(CombatSoundMix.HeadroomScale(.2f), Is.EqualTo(1));
        Assert.That(CombatSoundMix.Priority(CombatSoundCue.PlayerHit),
            Is.GreaterThan(CombatSoundMix.Priority(CombatSoundCue.GemLand)));
    }

    [Test]
    public void CooldownCoalescesBurstWithoutAccumulatingDelayedPlayback()
    {
        var gate = new FeedbackCooldown();
        Assert.That(gate.TryConsume(0, .1f), Is.True);
        for (int i = 0; i < 64; i++) Assert.That(gate.TryConsume(.05f, .1f), Is.False);
        Assert.That(gate.TryConsume(.1f, .1f), Is.True);
        Assert.That(gate.TryConsume(.2f, .1f), Is.True);
        gate.Reset();
        Assert.That(gate.TryConsume(0, .1f), Is.True);
    }

    [Test]
    public void VibrationPreferenceDoesNotChangeIndependentMusicOrSfx()
    {
        bool had = PlayerPrefs.HasKey(AudioPreferences.VibrationMutedKey);
        int original = PlayerPrefs.GetInt(AudioPreferences.VibrationMutedKey);
        bool music = AudioPreferences.MusicMuted, sfx = AudioPreferences.SfxMuted;
        try
        {
            AudioPreferences.SetVibrationMuted(true);
            Assert.That(AudioPreferences.VibrationMuted, Is.True);
            AudioPreferences.SetVibrationMuted(false);
            Assert.That(AudioPreferences.VibrationMuted, Is.False);
            Assert.That(AudioPreferences.MusicMuted, Is.EqualTo(music));
            Assert.That(AudioPreferences.SfxMuted, Is.EqualTo(sfx));
        }
        finally
        {
            if (had) PlayerPrefs.SetInt(AudioPreferences.VibrationMutedKey, original);
            else PlayerPrefs.DeleteKey(AudioPreferences.VibrationMutedKey);
            PlayerPrefs.Save();
        }
    }

    [Test]
    public void PlayerShieldCueReportsEffectiveGrantOnlyAndRestoreStaysSilent()
    {
        PlayerActor actor = Component<PlayerActor>();
        Set(actor, "isInitialized", true); Set(actor, "maximumHealth", 100); Set(actor, "currentHealth", 100);
        Set(actor, "maximumShield", 30);
        int count = 0, amount = 0;
        actor.ShieldGranted += (_, n) => { count++; amount += n; };
        actor.GrantShield(24); actor.GrantShield(24); actor.GrantShield(24);
        Assert.That(count, Is.EqualTo(2)); Assert.That(amount, Is.EqualTo(30));
        actor.RestoreContinuation(new PlayerCombatSnapshot { health = 100, maximumHealth = 100, shield = 20, maximumShield = 30 });
        Assert.That(count, Is.EqualTo(2));
    }

    [Test]
    public void EnemyGainAndHealCuesExcludeNoOpsAndSnapshotRestore()
    {
        EnemyActor actor = Enemy();
        int heal = 0, shield = 0;
        actor.Healed += (_, n) => heal += n;
        actor.ShieldGranted += (_, n) => shield += n;
        actor.RestoreHealth(12);
        actor.GrantShield(25); actor.GrantShield(25);
        actor.TryTakeDamageWithoutFeedback(60);
        actor.RestoreHealth(12);
        Assert.That(heal, Is.EqualTo(12)); Assert.That(shield, Is.EqualTo(30));
        actor.RestoreContinuation(new EnemyCombatSnapshot { health = 100, shield = 30 });
        Assert.That(heal, Is.EqualTo(12)); Assert.That(shield, Is.EqualTo(30));
    }

    [TestCase(0, 8)]
    [TestCase(30, 6)]
    public void PoisonHasOneSoundCueIncludingShieldAbsorptionWithoutNormalHit(int shield, int total)
    {
        EnemyActor actor = Enemy();
        actor.GrantShield(shield);
        EnemyPoisonStatus poison = actor.gameObject.AddComponent<EnemyPoisonStatus>();
        poison.Apply(7, 1, 8);
        int ticks = 0, normalHits = 0;
        actor.StatusDamageReceived += (_, n) => ticks += n;
        actor.DamageReceived += (_, __) => normalHits++;
        actor.ShieldDamaged += (_, __) => normalHits++;
        typeof(EnemyPoisonStatus).GetMethod("ApplyTick", Flags).Invoke(poison, null);
        Assert.That(ticks, Is.EqualTo(total)); Assert.That(normalHits, Is.Zero);
    }

    [Test]
    public void LethalPoisonStillProducesOneFinalTickCue()
    {
        EnemyActor actor = Enemy(); Set(actor, "currentHealth", 1);
        EnemyPoisonStatus poison = actor.gameObject.AddComponent<EnemyPoisonStatus>();
        poison.Apply(7, 1, 8);
        int ticks = 0;
        actor.StatusDamageReceived += (_, n) => ticks += n;
        typeof(EnemyPoisonStatus).GetMethod("ApplyTick", Flags).Invoke(poison, null);
        Assert.That(actor.IsDefeated, Is.True); Assert.That(ticks, Is.EqualTo(1));
    }

    private EnemyActor Enemy()
    {
        var actor = Component<EnemyActor>();
        var definition = ScriptableObject.CreateInstance<EnemyDefinition>(); created.Add(definition);
        Set(actor, "definition", definition);
        Set(actor, "<RuntimeStats>k__BackingField", new EnemyRuntimeStats(1, 1, 100, 1, 0, 10, 3));
        Set(actor, "currentHealth", 100); Set(actor, "isInitialized", true);
        return actor;
    }
    private T Component<T>() where T : Component
    {
        var go = new GameObject("Audio test " + typeof(T).Name); go.SetActive(false); created.Add(go);
        return go.AddComponent<T>();
    }
    private static void Set(object target, string field, object value) =>
        target.GetType().GetField(field, Flags).SetValue(target, value);
}
