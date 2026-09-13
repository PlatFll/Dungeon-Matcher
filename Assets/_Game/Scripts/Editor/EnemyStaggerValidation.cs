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
            float miniBoss = Threshold(stagger, definition, EnemyCategory.MiniBoss);
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
        Set(stagger, "remainingStaggerTime", 0f);
        Set(stagger, "remainingImmunityTime", 0f);
        Set(stagger, "remainingBuildupGraceTime", 0f);
        Set(stagger, "activeStaggerDuration", 0f);
        Set(stagger, "isStaggered", false);
        Set(stagger, "staggerMeterNormalized", 0f);
    }

    private static IEnumerator Delay(float seconds)
    {
        float end = Time.realtimeSinceStartup + seconds;
        while (Time.realtimeSinceStartup < end)
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