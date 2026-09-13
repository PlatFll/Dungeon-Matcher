using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

[InitializeOnLoad]
public static class EnemyStaggerValidation
{
    private const string Pending = "DungeonMatcher.EnemyStaggerValidation";
    private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
    private static readonly Stack<IEnumerator> steps = new Stack<IEnumerator>();

    static EnemyStaggerValidation()
    {
        EditorApplication.playModeStateChanged += HandlePlayModeState;
    }

    [MenuItem("Dungeon Matcher/Validation/Enemy Stagger Meter")]
    public static void Run()
    {
        if (EditorApplication.isPlaying)
        {
            throw new InvalidOperationException("Start Enemy Stagger validation outside Play Mode.");
        }

        SessionState.SetBool(Pending, true);
        EditorApplication.EnterPlaymode();
    }

    private static void HandlePlayModeState(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredPlayMode && SessionState.GetBool(Pending, false))
        {
            SessionState.SetBool(Pending, false);
            steps.Clear();
            steps.Push(Validate());
            EditorApplication.update -= Tick;
            EditorApplication.update += Tick;
        }
        else if (state == PlayModeStateChange.ExitingPlayMode)
        {
            EditorApplication.update -= Tick;
            steps.Clear();
        }
    }

    private static void Tick()
    {
        try
        {
            while (steps.Count > 0)
            {
                IEnumerator step = steps.Peek();
                if (!step.MoveNext())
                {
                    steps.Pop();
                    continue;
                }

                if (step.Current is IEnumerator nested)
                {
                    steps.Push(nested);
                    continue;
                }

                return;
            }

            Debug.Log("Enemy stagger meter validation PASSED.");
            EditorApplication.update -= Tick;
            EditorApplication.ExitPlaymode();
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            EditorApplication.update -= Tick;
            steps.Clear();
            EditorApplication.ExitPlaymode();
        }
    }

    private static IEnumerator Validate()
    {
        yield return WaitUntil(
            () =>
            {
                WaveController waves = UnityEngine.Object.FindFirstObjectByType<WaveController>();
                return waves != null && waves.ActiveEnemies.Count > 0;
            },
            8f,
            "active enemy"
        );

        WaveController waveController = UnityEngine.Object.FindFirstObjectByType<WaveController>();
        EnemyActor enemy = waveController.ActiveEnemies[0];
        EnemyStagger stagger = enemy.GetComponent<EnemyStagger>();
        EnemyAutoAttack autoAttack = enemy.GetComponent<EnemyAutoAttack>();

        Check(stagger != null, "enemy has EnemyStagger");
        Check(autoAttack != null, "enemy has EnemyAutoAttack");

        EnemyDefinition originalDefinition = enemy.Definition;
        EnemyDefinition definition = UnityEngine.Object.Instantiate(originalDefinition);
        definition.name = originalDefinition.name + " (Stagger Validation)";
        Set(definition, "staggerDurationMultiplier", 1f);
        Set(enemy, "definition", definition);

        try
        {
            float normal = Threshold(stagger, definition, EnemyCategory.Normal);
            float special = Threshold(stagger, definition, EnemyCategory.Special);
            float miniBoss = Threshold(stagger, definition, EnemyCategory.Miniboss);
            float boss = Threshold(stagger, definition, EnemyCategory.Boss);
            Check(normal < special && special < miniBoss && miniBoss < boss,
                "category thresholds increase Normal < Special < MiniBoss < Boss");

            Set(stagger, "buildupDecayDelay", 0.12f);
            Set(stagger, "buildupDecayDuration", 0.25f);
            Set(stagger, "baseStaggerDuration", 0.30f);
            Set(definition, "category", EnemyCategory.Normal);
            Reset(stagger);

            float threshold = stagger.DamageThreshold;
            float added = stagger.RegisterDamage(Mathf.Max(1, Mathf.CeilToInt(threshold * 0.35f)));
            Check(added > 0f && stagger.StaggerMeterNormalized > 0f && !stagger.IsStaggered,
                "partial damage builds meter without immediate stagger");

            float held = stagger.StaggerMeterNormalized;
            yield return Delay(0.06f);
            Check(stagger.StaggerMeterNormalized >= held - 0.01f,
                "meter holds during hit grace window");
            yield return Delay(0.14f);
            Check(stagger.StaggerMeterNormalized < held,
                "meter decays after grace window");

            Reset(stagger);
            stagger.RegisterDamage(Mathf.CeilToInt(stagger.DamageThreshold));
            Check(stagger.IsStaggered && Mathf.Approximately(stagger.StaggerMeterNormalized, 1f),
                "full meter begins stagger");
            Check(autoAttack.IsPausedByStagger, "auto attack pauses while staggered");

            yield return Delay(0.12f);
            Check(stagger.IsStaggered && stagger.StaggerMeterNormalized < 1f && stagger.StaggerMeterNormalized > 0f,
                "stagger meter drains while staggered");
            yield return Delay(0.25f);
            Check(!stagger.IsStaggered && stagger.StaggerMeterNormalized <= 0.001f,
                "stagger ends when meter empties");
            Check(stagger.RemainingImmunityTime > 4.8f && stagger.RemainingImmunityTime <= EnemyStagger.PostStaggerImmunitySeconds,
                "five-second post-stagger immunity begins");
            Check(stagger.RegisterDamage(Mathf.CeilToInt(stagger.DamageThreshold)) <= 0f,
                "immunity blocks immediate re-stagger buildup");

            Set(stagger, "remainingImmunityTime", 0.01f);
            yield return Delay(0.03f);
            Check(stagger.RegisterDamage(1) > 0f,
                "buildup resumes after immunity expires");

            Reset(stagger);
            Set(enemy, "currentShield", 0);
            int healthBefore = enemy.CurrentHealth;
            Check(enemy.TryTakeDamage(1), "normal damage accepted");
            Check(stagger.StaggerMeterNormalized > 0f,
                "normal feedback-producing damage builds stagger");
            Check(enemy.CurrentHealth <= healthBefore, "normal damage resolved");

            Reset(stagger);
            Check(enemy.TryTakeDamageWithoutFeedback(1), "feedback-suppressed damage accepted");
            Check(stagger.StaggerMeterNormalized <= 0.001f,
                "feedback-suppressed damage does not build stagger");

            yield return null;
            EnemyHitFlashPresenter hitFlash = enemy.GetComponent<EnemyHitFlashPresenter>();
            Check(hitFlash != null, "one-shot hit flash presenter installed");

            EnemySlotUI slot = enemy.GetComponentInParent<EnemySlotUI>();
            Check(slot != null, "enemy remains bound to a slot");
            EnemyWeaknessIndicatorUI weakness = slot.GetComponentInChildren<EnemyWeaknessIndicatorUI>(true);
            Check(weakness != null, "weakness indicator exists");
            EnemyStaggerWeaknessFillUI fill = weakness.GetComponent<EnemyStaggerWeaknessFillUI>();
            Check(fill != null, "weakness stagger fill presenter installed");
            Image fillImage = weakness.transform.Find("StaggerWhiteFill")?.GetComponent<Image>();
            Check(fillImage != null && fillImage.type == Image.Type.Filled &&
                  fillImage.fillMethod == Image.FillMethod.Vertical &&
                  fillImage.fillOrigin == (int)Image.OriginVertical.Bottom,
                "weakness white meter fills vertically from bottom");

            yield return ValidateDamageAndIndependence(waveController, enemy, definition);
        }
        finally
        {
            Set(enemy, "definition", originalDefinition);
            UnityEngine.Object.Destroy(definition);
        }
    }

    private static float Threshold(EnemyStagger stagger, EnemyDefinition definition, EnemyCategory category)
    {
        Set(definition, "category", category);
        return stagger.DamageThreshold;
    }

    private static void Reset(EnemyStagger stagger)
    {
        stagger.ClearStagger();
        Set(stagger, "remainingStaggerTime", 0f);
        Set(stagger, "remainingImmunityTime", 0f);
        Set(stagger, "remainingBuildupGraceTime", 0f);
        Set(stagger, "activeStaggerDuration", 0f);
        Set(stagger, "isStaggered", false);
        Set(stagger, "staggerMeterNormalized", 0f);
    }

    private static IEnumerator ValidateDamageAndIndependence(
        WaveController waves, EnemyActor enemy, EnemyDefinition definition)
    {
        // Runtime fixtures only: preserve authored assets and use real actor,
        // combat, slot and attack systems, including the scene's legacy bridge.
        Set(waves, "advanceWavesAutomatically", false);
        EnemySlotUI secondSlot = Array.Find(
            UnityEngine.Object.FindObjectsByType<EnemySlotUI>(FindObjectsSortMode.None),
            candidate => candidate.CurrentEnemy == null);
        Check(secondSlot != null, "second enemy slot available");
        EnemyActor second = (EnemyActor)typeof(WaveController)
            .GetMethod("CreateEnemy", Flags).Invoke(waves,
                new object[] { definition, secondSlot, (GemType)(((int)enemy.AssignedGemType + 1) % 6) });
        Check(second != null, "second enemy spawned through production path");
        ((List<EnemyActor>)typeof(WaveController).GetField("activeEnemies", Flags)
            .GetValue(waves)).Add(second);
        // 30% of 50 HP is exactly 15 damage. This catches floating-point
        // thresholds that look full but fail the transition at the boundary.
        enemy.Initialize(definition, new EnemyRuntimeStats(1, 1, 50, 1, 0, 10f, 1000), enemy.AssignedGemType);
        enemy.TryTakeDamage(15);
        Check(enemy.GetComponent<EnemyStagger>().IsStaggered,
            "exact 30 percent of 50 HP triggers stagger at 15 damage");
        Reset(enemy.GetComponent<EnemyStagger>());
        enemy.Initialize(definition, new EnemyRuntimeStats(1, 1, 50, 1, 0, 10f, 1000), enemy.AssignedGemType);
        enemy.TryTakeDamage(5);
        enemy.TryTakeDamage(5);
        Check(!enemy.GetComponent<EnemyStagger>().IsStaggered, "below threshold remains buildup");
        enemy.TryTakeDamage(5);
        Check(enemy.GetComponent<EnemyStagger>().IsStaggered,
            "consecutive hits trigger at the exact threshold");
        Reset(enemy.GetComponent<EnemyStagger>());
        var stats = new EnemyRuntimeStats(1, 1, 1000, 1, 0, 10f, 1000);
        enemy.Initialize(definition, stats, enemy.AssignedGemType);
        second.Initialize(definition, stats, second.AssignedGemType);
        var a = enemy.GetComponent<EnemyStagger>();
        var b = second.GetComponent<EnemyStagger>();
        Set(a, "buildupDecayDelay", 1.25f);
        Set(a, "buildupDecayDuration", 2.5f);
        Set(a, "baseStaggerDuration", 2.5f);
        yield return Delay(1f); // Allow slot materialization and attack startup.

        float[] fractions = { .30f, .45f, .65f, .90f };
        for (int i = 0; i < fractions.Length; i++)
            Near(Threshold(a, definition, (EnemyCategory)i), 1000f * fractions[i],
                "authored category threshold " + (EnemyCategory)i);
        Set(definition, "category", EnemyCategory.Normal);

        var combat = UnityEngine.Object.FindFirstObjectByType<CombatController>();
        Check(combat.ResolveFixedGemDamage(new BoardClearContext(
            enemy.AssignedGemType, 1, 0, BoardClearSource.Match), 1),
            "production gem damage accepted");
        Check(!a.IsStaggered, "ordinary gem hit cannot bypass the buildup threshold");
        Near(a.StaggerMeterNormalized, 1f / a.DamageThreshold,
            "production gem damage contributes once");
        Reset(a);

        enemy.TryTakeDamage(30);
        Near(a.StaggerMeterNormalized, .1f, "HP damage proportional buildup");
        enemy.TryTakeDamage(60);
        Near(a.StaggerMeterNormalized, .3f, "larger consecutive hit accumulates proportionally");
        Near(b.StaggerMeterNormalized, 0f, "other enemy unaffected by hits");
        Reset(a);
        enemy.GrantShield(30);
        int hp = enemy.CurrentHealth, shield = enemy.CurrentShield;
        enemy.TryTakeDamage(10);
        Near(a.StaggerMeterNormalized, (hp - enemy.CurrentHealth + shield - enemy.CurrentShield) / a.DamageThreshold,
            "effective shield damage contributes after mitigation");
        Reset(a);
        Set(enemy, "currentShield", 3);
        hp = enemy.CurrentHealth;
        enemy.TryTakeDamage(20);
        Near(a.StaggerMeterNormalized, (hp - enemy.CurrentHealth + 3) / a.DamageThreshold,
            "shield overflow adds shield and HP loss exactly once");
        Reset(a);
        enemy.GrantShield(30);
        enemy.TryTakeDamageWithoutFeedback(100);
        Near(a.StaggerMeterNormalized, 0f, "suppressed shield and HP damage do not build meter");

        enemy.TryTakeDamage(30);
        yield return Delay(.8f);
        second.TryTakeDamage(60);
        yield return Delay(.6f);
        Check(a.StaggerMeterNormalized < .1f && a.StaggerMeterNormalized > 0f,
            "first enemy drains after its own grace");
        Near(b.StaggerMeterNormalized, .2f, "second enemy retains its independent grace");
        float before = a.StaggerMeterNormalized;
        enemy.TryTakeDamage(30);
        Near(a.StaggerMeterNormalized, before + .1f, "hit while draining resumes buildup");
        float refreshed = a.StaggerMeterNormalized;
        yield return Delay(.8f);
        Near(a.StaggerMeterNormalized, refreshed, "valid hit refreshes the full grace window");
        Check(b.StaggerMeterNormalized < .2f, "second enemy decays independently");

        Reset(a);
        Reset(b);
        int starts = 0;
        a.StaggerApplied += (_, __, ___) => starts++;
        enemy.TryTakeDamage(100);
        Check(!a.IsStaggered, "partial threshold does not stagger");
        enemy.TryTakeDamage(200);
        Check(a.IsStaggered && starts == 1, "exact threshold starts one stagger");
        Near(a.StaggerMeterNormalized, 1f, "stagger starts full");
        EnemyAutoAttack attack = enemy.GetComponent<EnemyAutoAttack>();
        Check(attack.IsRunning && attack.IsPausedByStagger, "running attack paused");
        float attackTime = attack.RemainingAttackTime;
        yield return Delay(.6f);
        Near(attack.RemainingAttackTime, attackTime, "attack countdown remains frozen");
        float draining = a.StaggerMeterNormalized;
        enemy.TryTakeDamage(1);
        Near(a.StaggerMeterNormalized, draining, "hit during stagger cannot refill");
        Check(starts == 1, "hit during stagger cannot trigger another stagger");
        second.TryTakeDamage(30);
        Check(!b.IsStaggered && !b.IsStaggerImmune && b.StaggerMeterNormalized > 0f,
            "other enemy can build during first stagger");
        yield return WaitUntil(() => !a.IsStaggered, 4f, "stagger end");
        Near(a.StaggerMeterNormalized, 0f, "stagger empties at end");
        Check(a.RemainingImmunityTime > 4.9f, "immunity begins at five seconds");
        float immunityStart = Time.time;
        enemy.TryTakeDamage(1);
        Near(a.StaggerMeterNormalized, 0f, "immunity blocks buildup while damage works");
        Check(!attack.IsPausedByStagger, "attack released after stagger");
        Reset(b);
        second.TryTakeDamage(300);
        Check(b.IsStaggered && a.IsStaggerImmune && !a.IsStaggered,
            "second enemy staggers during first enemy immunity");
        yield return WaitUntil(() => !b.IsStaggered, 4f, "second stagger end");
        Check(b.RemainingImmunityTime > a.RemainingImmunityTime + 2f,
            "immunity timers independent");
        yield return WaitUntil(() => Time.time - immunityStart >= 4.8f, 6f, "late immunity");
        Check(a.IsStaggerImmune, "immunity still active before five seconds");
        enemy.TryTakeDamage(1);
        Near(a.StaggerMeterNormalized, 0f, "meter stays empty throughout immunity");
        yield return WaitUntil(() => !a.IsStaggerImmune, 1f, "five second immunity expiry");
        Check(Mathf.Abs(Time.time - immunityStart - 5f) < .12f,
            "immunity lasts five game seconds within frame tolerance");
        enemy.TryTakeDamage(1);
        Check(a.StaggerMeterNormalized > 0f && b.IsStaggerImmune,
            "buildup resumes while other enemy remains immune");

        enemy.Initialize(definition, stats, enemy.AssignedGemType);
        Check(!a.IsStaggerImmune && a.StaggerMeterNormalized == 0f,
            "reinitialized enemy starts empty without immunity");
        enemy.TryTakeDamage(300);
        enemy.TryTakeDamage(10000);
        Check(enemy.IsDefeated && !a.IsStaggered && !a.IsStaggerImmune && a.StaggerMeterNormalized == 0f,
            "death safely clears stagger and timers");
        second.Initialize(definition, stats, second.AssignedGemType);
        Check(!b.IsStaggerImmune && b.StaggerMeterNormalized == 0f,
            "reinitialization clears immunity");
        yield return null;
    }

    private static void Near(float actual, float expected, string message)
    {
        Check(Mathf.Abs(actual - expected) < .002f,
            message + $" (expected {expected}, actual {actual})");
    }

    private static IEnumerator Delay(float seconds)
    {
        float end = Time.time + seconds;
        while (Time.time < end)
        {
            yield return null;
        }
    }

    private static IEnumerator WaitUntil(Func<bool> predicate, float timeout, string label)
    {
        float end = Time.realtimeSinceStartup + timeout;
        while (!predicate())
        {
            if (Time.realtimeSinceStartup >= end)
            {
                throw new InvalidOperationException("Timed out waiting for " + label);
            }
            yield return null;
        }
    }

    private static void Set(object target, string field, object value)
    {
        FieldInfo info = target.GetType().GetField(field, Flags);
        if (info == null)
        {
            throw new MissingFieldException(target.GetType().Name, field);
        }
        info.SetValue(target, value);
    }

    private static void Check(bool condition, string message)
    {
        if (!condition)
        {
            throw new InvalidOperationException("Enemy stagger: " + message);
        }
    }
}
