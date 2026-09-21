using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;

/// <summary>Opt-in cue observation for DungeonPresentationValidation's disposable production Game.</summary>
public static class CombatAudioRuntimeValidation
{
    private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
    private const string SfxKey = "DungeonMatcher.Audio.SfxMuted";

    // Call after ObserveAudio / DelayedMatchPauseCase, while the production board is settled.
    // The enclosing harness owns the disposable account, simulated focus and final report.
    public static IEnumerator Cases(Action<bool, string> check, Action<string> report = null)
    {
        if (!Application.isBatchMode || !Application.isPlaying)
            throw new InvalidOperationException("Audio actor cases require the disposable batch Play Mode harness.");
        var run = RunSession.Current;
        check(run != null && run.Board != null && run.Player != null && run.Waves != null,
            "audio actor cases have production Game owners");
        var audio = run.Board.GetComponent<CombatAudioController>();
        var player = run.Player;
        var ability = player.GetComponent<PlayerAbilityController>();
        var energy = player.GetComponent<PlayerAbilityEnergy>();
        check(audio != null && ability != null && energy != null && CombatAudioController.FeedbackAllowed,
            "audio actor cases start focused, unpaused and bound to the real audio observer");
        check(!AudioPreferences.SfxMuted, "audio actor fixture starts unmuted");
        var enemies = run.Waves.ActiveEnemies.Where(e => e != null && !e.IsDefeated).ToArray();
        check(enemies.Length > 0, "audio actor fixture has a living production enemy");
        var enemy = enemies[0];
        var playerDefinition = player.Definition;
        var playerSnapshot = player.CaptureContinuation();
        var stats = enemies.Select(e => e.RuntimeStats).ToArray();
        var snapshots = enemies.Select((e, i) => e.CaptureContinuation(run.Waves.ContinuationSlot(e))).ToArray();
        var enemyDefinition = enemy.Definition;
        bool hadSfx = PlayerPrefs.HasKey(SfxKey);
        int oldSfx = PlayerPrefs.GetInt(SfxKey);
        float originalTimeScale = Time.timeScale;
        var played = new List<CombatSoundCue>();
        var seen = new HashSet<CombatSoundCue>();
        Action<CombatSoundCue> listener = cue => { played.Add(cue); seen.Add(cue); };
        EnemyPoisonStatus poison = null;
        bool createdPoison = false;
        var savedPoison = new EnemyCombatSnapshot();
        audio.CuePlayed += listener;
        report?.Invoke("AUDIO ACTOR FIXTURE: actual damage/heal/shield callbacks and accepted player casts; temporary 10,000 enemy HP, stopped auto-attacks and disposable account. CuePlayed measures playback requests, not heard audio.");
        try
        {
            foreach (var attack in Object.FindObjectsByType<EnemyAutoAttack>(FindObjectsSortMode.None)) attack.StopAttacking();
            for (int i = 0; i < enemies.Length; i++)
            {
                var s = stats[i];
                Set(enemies[i], "<RuntimeStats>k__BackingField", new EnemyRuntimeStats(s.Wave, s.Level, 10000,
                    s.Damage, s.FollowUpDamage, s.AttackInterval, s.SpecialTurnRequirement, s.DamageMultiplier));
                Set(enemies[i], "currentHealth", 10000);
                Set(enemies[i], "currentShield", 0);
            }
            Set(player, "currentShield", 0);
            yield return Quiet(played);

            check(player.TryTakeDamage(3), "real player damage accepted");
            yield return Expect(played, CombatSoundCue.PlayerHit, check, "player HP hit");
            yield return Quiet(played);
            check(player.Heal(3) == 3, "real player heal accepted");
            yield return Expect(played, CombatSoundCue.Healing, check, "player healing");
            yield return Quiet(played);
            check(player.GrantShield(3) == 3, "real player shield grant accepted");
            yield return Expect(played, CombatSoundCue.ShieldGain, check, "player shield grant");
            yield return Quiet(played);
            int playerHp = player.CurrentHealth;
            check(player.TryTakeDamage(12), "real player shield-overflow hit accepted");
            yield return Expect(played, CombatSoundCue.ShieldHit, check, "player shield overflow");
            yield return Wait(.12f);
            check(player.CurrentShield == 0 && player.CurrentHealth < playerHp, "player overflow actually damaged shield and HP");
            check(!played.Contains(CombatSoundCue.PlayerHit), "player overflow emits no duplicate body-hit cue");

            yield return Quiet(played);
            check(enemy.TryTakeDamage(3), "real enemy damage accepted");
            yield return Expect(played, CombatSoundCue.EnemyHit, check, "enemy HP hit");
            yield return Quiet(played);
            check(enemy.RestoreHealth(3) == 3, "real enemy heal accepted");
            yield return Expect(played, CombatSoundCue.Healing, check, "enemy healing");
            yield return Quiet(played);
            check(enemy.GrantShield(3) == 3, "real enemy shield grant accepted");
            yield return Expect(played, CombatSoundCue.ShieldGain, check, "enemy shield grant");
            yield return Quiet(played);
            int enemyHp = enemy.CurrentHealth;
            check(enemy.TryTakeDamage(12), "real enemy shield-overflow hit accepted");
            yield return Expect(played, CombatSoundCue.ShieldHit, check, "enemy shield overflow");
            yield return Wait(.12f);
            check(enemy.CurrentShield == 0 && enemy.CurrentHealth < enemyHp, "enemy overflow actually damaged shield and HP");
            check(!played.Contains(CombatSoundCue.EnemyHit), "enemy overflow emits no duplicate body-hit cue");

            poison = enemy.GetComponent<EnemyPoisonStatus>();
            createdPoison = poison == null;
            if (createdPoison) poison = enemy.gameObject.AddComponent<EnemyPoisonStatus>();
            poison.CaptureContinuation(savedPoison);
            poison.ClearPoison();
            yield return Quiet(played);
            enemyHp = enemy.CurrentHealth;
            poison.Apply(.18f, .10f, 2);
            yield return Expect(played, CombatSoundCue.PoisonTick, check, "actual scheduled poison HP tick");
            poison.ClearPoison();
            check(enemy.CurrentHealth < enemyHp && !played.Contains(CombatSoundCue.EnemyHit),
                "poison damages HP with its own tick cue and no normal hit cue");
            yield return Quiet(played);
            enemy.GrantShield(10);
            yield return Quiet(played);
            enemyHp = enemy.CurrentHealth;
            poison.Apply(.18f, .10f, 2);
            yield return Expect(played, CombatSoundCue.PoisonTick, check, "actual scheduled shield-absorbed poison tick");
            poison.ClearPoison();
            check(enemy.CurrentHealth == enemyHp && enemy.CurrentShield < 10 && !played.Contains(CombatSoundCue.EnemyHit) &&
                !played.Contains(CombatSoundCue.ShieldHit), "shield-only poison retains one tick cue without ordinary damage cues");
            Set(enemy, "currentShield", 0);

            yield return Quiet(played);
            var miner = UnityEditor.AssetDatabase.LoadAssetAtPath<EnemyDefinition>("Assets/_Game/Data/Enemies/Enemy_Miner.asset");
            check(miner != null && miner.HasSpecialAbility, "enemy effect notification fixture has a production ability definition");
            Set(enemy, "definition", miner);
            enemy.NotifySpecialAbilityEffectApplied();
            yield return Expect(played, CombatSoundCue.EnemyAbility, check, "committed enemy ability notification");
            Set(enemy, "definition", enemyDefinition);
            report?.Invoke("ENEMY ABILITY SCOPE: shared cue exercised through the actual actor committed-effect API with temporary Miner identity; this case does not execute mining gameplay.");

            yield return Quiet(played);
            Time.timeScale = 0;
            player.TryTakeDamage(1);
            yield return Wait(.18f);
            check(played.Count == 0, "paused actor callbacks emit no playback requests");
            Time.timeScale = 1;
            yield return Wait(.18f);
            check(played.Count == 0, "paused actor callbacks leave no deferred cue after resume");
            PlayerPrefs.SetInt(SfxKey, 1); // In-memory preference fixture; enclosing harness owns persistence.
            player.TryTakeDamage(1);
            yield return Wait(.18f);
            check(played.Count == 0, "SFX mute suppresses a real player damage cue");
            PlayerPrefs.SetInt(SfxKey, 0);
            player.TryTakeDamage(1);
            yield return Expect(played, CombatSoundCue.PlayerHit, check, "SFX unmute positive control");

            foreach (string identity in new[] { "Player_Skeleton", "Player_Bardley" })
            {
                ability.CancelActiveAbility();
                yield return Until(() => !run.Board.IsBusy, "board ready before " + identity);
                var definition = Resources.Load<PlayerDefinition>("Players/" + identity);
                check(definition != null, identity + " production definition available");
                player.Initialize(definition);
                ability.RefreshRuntime();
                energy.AddEnergy(energy.MaximumEnergy);
                yield return Quiet(played);
                check(ability.CanActivate, identity + " real ability can activate");
                var expected = identity == "Player_Bardley" ? CombatSoundCue.BardleyAbility : CombatSoundCue.RattlebonesAbility;
                var wrong = identity == "Player_Bardley" ? CombatSoundCue.RattlebonesAbility : CombatSoundCue.BardleyAbility;
                float activatedAt = Time.time;
                check(ability.TryActivate(), identity + " real ability accepted");
                yield return Expect(played, expected, check, identity + " distinct accepted-cast cue");
                check(!played.Contains(wrong), identity + " does not play the other character signature");
                if (identity == "Player_Skeleton")
                {
                    // Scaled time is deliberate: captureDeltaTime can run faster than the wall clock.
                    yield return Until(() => Time.time - activatedAt >= .45f, "Royal signature cooldown elapsed");
                    int before = played.Count(c => c == expected);
                    check(!ability.TryActivate(), "active Royal Decree rejects repeated activation");
                    yield return Wait(.15f);
                    check(played.Count(c => c == expected) == before, "rejected player ability emits no second signature");
                    ability.CancelActiveAbility();
                }
                else
                {
                    yield return Until(() => !run.Board.IsBusy && !ability.IsAbilityActive, "real Bardley ability and board settlement");
                    yield return Wait(.2f);
                    check(played.Contains(CombatSoundCue.GemLand), "real Bardley destruction/refill emits grouped gem landing audio");
                    check(played.Count(c => c == expected) == 1, "Bardley cast emits one signature across its explosions and refill");
                }
            }
            foreach (var cue in new[] { CombatSoundCue.PlayerHit, CombatSoundCue.EnemyHit, CombatSoundCue.Healing,
                CombatSoundCue.ShieldGain, CombatSoundCue.ShieldHit, CombatSoundCue.PoisonTick, CombatSoundCue.EnemyAbility,
                CombatSoundCue.RattlebonesAbility, CombatSoundCue.BardleyAbility, CombatSoundCue.GemLand })
                check(seen.Contains(cue), "actor/ability runtime coverage: " + cue);
            report?.Invoke("ACTOR AUDIO OBSERVED: " + string.Join(", ", seen.OrderBy(c => c).Select(c => c.ToString())));
            report?.Invoke("Combined with the enclosing delayed-match and Explosion/Poison/Shield/Healing burst cases, these callbacks cover all 13 production sound mappings.");
        }
        finally
        {
            audio.CuePlayed -= listener;
            Time.timeScale = originalTimeScale;
            if (hadSfx) PlayerPrefs.SetInt(SfxKey, oldSfx); else PlayerPrefs.DeleteKey(SfxKey);
            if (ability != null) ability.CancelActiveAbility();
            if (player != null) { player.Initialize(playerDefinition); player.RestoreContinuation(playerSnapshot); }
            if (enemy != null) Set(enemy, "definition", enemyDefinition);
            for (int i = 0; i < enemies.Length; i++) if (enemies[i] != null)
            {
                Set(enemies[i], "<RuntimeStats>k__BackingField", stats[i]);
                enemies[i].RestoreContinuation(snapshots[i]);
            }
            if (poison != null)
            {
                poison.ClearPoison();
                if (createdPoison) Object.Destroy(poison);
                else poison.RestoreContinuation(savedPoison);
            }
        }
        // Let the restored character's presentation and last transient sounds settle before capture.
        yield return Wait(.7f);
    }

    private static IEnumerator Expect(List<CombatSoundCue> played, CombatSoundCue cue, Action<bool, string> check, string label)
    {
        yield return Until(() => played.Contains(cue), label + " audio playback request");
        check(played.Count(c => c == cue) == 1, label + " emits exactly one " + cue + " request");
    }
    private static IEnumerator Quiet(List<CombatSoundCue> played)
    {
        yield return Wait(.65f);
        played.Clear();
    }
    private static IEnumerator Wait(float seconds)
    {
        float until = Time.realtimeSinceStartup + seconds;
        while (Time.realtimeSinceStartup < until) yield return null;
    }
    private static IEnumerator Until(Func<bool> predicate, string label)
    {
        float until = Time.realtimeSinceStartup + 30;
        while (!predicate())
        {
            if (Time.realtimeSinceStartup >= until) throw new TimeoutException("Audio runtime case: " + label);
            yield return null;
        }
    }
    private static void Set(object target, string field, object value) =>
        target.GetType().GetField(field, Flags).SetValue(target, value);
}
