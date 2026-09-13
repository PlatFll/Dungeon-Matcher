using System;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(EnemyActor))]
public sealed class EnemyStagger : MonoBehaviour
{
    public const float PostStaggerImmunitySeconds = 5f;

    [Header("Stagger Buildup (first-pass tuning)")]
    [SerializeField, Min(0f)]
    [Tooltip("Seconds after the latest hit before an incomplete stagger meter begins draining.")]
    private float buildupDecayDelay = 1.25f;

    [SerializeField, Min(0.05f)]
    [Tooltip("Seconds required for a full, non-staggered meter to drain after the grace period.")]
    private float buildupDecayDuration = 2.5f;

    [SerializeField, Min(0.05f)]
    [Tooltip("Baseline stagger duration once the meter reaches full. EnemyDefinition's stagger duration multiplier still applies.")]
    private float baseStaggerDuration = 2.5f;

    [Header("Damage Required To Stagger")]
    [SerializeField, Min(0.01f)]
    private float normalHealthFraction = 0.30f;

    [SerializeField, Min(0.01f)]
    private float specialHealthFraction = 0.45f;

    [SerializeField, Min(0.01f)]
    private float miniBossHealthFraction = 0.65f;

    [SerializeField, Min(0.01f)]
    private float bossHealthFraction = 0.90f;

    [Header("Runtime Debug Information")]
    [SerializeField, Range(0f, 1f)]
    private float staggerMeterNormalized;

    [SerializeField]
    private float remainingStaggerTime;

    [SerializeField]
    private float remainingImmunityTime;

    [SerializeField]
    private float remainingBuildupGraceTime;

    [SerializeField]
    private bool isStaggered;

    private float activeStaggerDuration;
    private EnemyActor enemyActor;
    private bool hitFlashPresenterInstalled;
    private bool weaknessFillPresenterInstalled;

    public event Action<EnemyStagger, float, float> StaggerApplied;
    public event Action<EnemyStagger> StaggerEnded;
    public event Action<EnemyStagger, float> MeterChanged;

    public EnemyActor EnemyActor => enemyActor;
    public float RemainingStaggerTime => Mathf.Max(0f, remainingStaggerTime);
    public float RemainingImmunityTime => Mathf.Max(0f, remainingImmunityTime);
    public float StaggerMeterNormalized => Mathf.Clamp01(staggerMeterNormalized);
    public bool IsStaggered => isStaggered && remainingStaggerTime > 0f;
    public bool IsStaggerImmune => IsStaggered || remainingImmunityTime > 0f;
    public float DamageThreshold => CalculateDamageThreshold();

    private void Awake()
    {
        enemyActor = GetComponent<EnemyActor>();
    }

    private void OnEnable()
    {
        if (enemyActor == null)
        {
            enemyActor = GetComponent<EnemyActor>();
        }

        Subscribe();
        TryInstallPresentation();
    }

    private void Subscribe()
    {
        if (enemyActor == null)
        {
            return;
        }

        enemyActor.Initialized -= HandleEnemyInitialized;
        enemyActor.Initialized += HandleEnemyInitialized;
        enemyActor.DamageReceived -= HandleHealthDamage;
        enemyActor.DamageReceived += HandleHealthDamage;
        enemyActor.ShieldDamaged -= HandleShieldDamage;
        enemyActor.ShieldDamaged += HandleShieldDamage;
        enemyActor.Defeated -= HandleEnemyDefeated;
        enemyActor.Defeated += HandleEnemyDefeated;
    }

    private void Unsubscribe()
    {
        if (enemyActor == null)
        {
            return;
        }

        enemyActor.Initialized -= HandleEnemyInitialized;
        enemyActor.DamageReceived -= HandleHealthDamage;
        enemyActor.ShieldDamaged -= HandleShieldDamage;
        enemyActor.Defeated -= HandleEnemyDefeated;
    }

    private void Update()
    {
        TryInstallPresentation();

        if (enemyActor == null ||
            !enemyActor.IsInitialized ||
            enemyActor.IsDefeated)
        {
            return;
        }

        float deltaTime = Mathf.Max(0f, Time.deltaTime);

        if (IsStaggered)
        {
            remainingStaggerTime = Mathf.Max(0f, remainingStaggerTime - deltaTime);

            float normalized = activeStaggerDuration > 0f
                ? remainingStaggerTime / activeStaggerDuration
                : 0f;
            SetMeterNormalized(normalized);

            if (remainingStaggerTime <= 0f)
            {
                FinishStagger();
            }

            return;
        }

        if (remainingImmunityTime > 0f)
        {
            remainingImmunityTime = Mathf.Max(0f, remainingImmunityTime - deltaTime);
            SetMeterNormalized(0f);
            return;
        }

        if (staggerMeterNormalized <= 0f)
        {
            return;
        }

        if (remainingBuildupGraceTime > 0f)
        {
            remainingBuildupGraceTime = Mathf.Max(0f, remainingBuildupGraceTime - deltaTime);
            return;
        }

        float drainPerSecond = 1f / Mathf.Max(0.05f, buildupDecayDuration);
        SetMeterNormalized(staggerMeterNormalized - drainPerSecond * deltaTime);
    }

    /// <summary>
    /// Adds stagger pressure from one resolved, feedback-producing damage event.
    /// Health and shield damage both contribute; damage-over-time paths that
    /// deliberately suppress feedback do not emit these events and therefore
    /// do not build stagger.
    /// </summary>
    public float RegisterDamage(int damageAmount)
    {
        if (!CanBuildStagger() || damageAmount <= 0)
        {
            return 0f;
        }

        float threshold = CalculateDamageThreshold();
        if (threshold <= 0f)
        {
            return 0f;
        }

        float before = staggerMeterNormalized;
        remainingBuildupGraceTime = Mathf.Max(0f, buildupDecayDelay);
        SetMeterNormalized(before + damageAmount / threshold);

        float added = Mathf.Max(0f, staggerMeterNormalized - before);

        if (staggerMeterNormalized >= 1f)
        {
            float multiplier = enemyActor.Definition != null
                ? Mathf.Max(0f, enemyActor.Definition.StaggerDurationMultiplier)
                : 1f;

            if (multiplier > 0f)
            {
                StartStagger(Mathf.Max(0.05f, baseStaggerDuration * multiplier));
            }
        }

        return added;
    }

    /// <summary>
    /// Compatibility API for explicit mechanics that intentionally force a
    /// stagger. Normal damage should use the buildup meter instead. Forced
    /// stagger cannot extend an active stagger or bypass the post-stagger lockout.
    /// </summary>
    public float ApplyStagger(float durationToAdd, float maximumStoredDuration)
    {
        if (enemyActor == null ||
            !enemyActor.IsInitialized ||
            enemyActor.IsDefeated ||
            enemyActor.Definition == null ||
            durationToAdd <= 0f ||
            maximumStoredDuration <= 0f ||
            IsStaggerImmune)
        {
            return 0f;
        }

        float multiplier = Mathf.Max(
            0f,
            enemyActor.Definition.StaggerDurationMultiplier
        );

        if (multiplier <= 0f)
        {
            return 0f;
        }

        float duration = Mathf.Min(durationToAdd, maximumStoredDuration) * multiplier;
        if (duration <= 0f)
        {
            return 0f;
        }

        StartStagger(duration);
        return duration;
    }

    public void ClearStagger()
    {
        if (IsStaggered)
        {
            FinishStagger();
            return;
        }

        ResetMeterAndTimers(clearImmunity: false);
    }

    private bool CanBuildStagger()
    {
        return enemyActor != null &&
               enemyActor.IsInitialized &&
               !enemyActor.IsDefeated &&
               enemyActor.Definition != null &&
               enemyActor.Definition.StaggerDurationMultiplier > 0f &&
               !IsStaggerImmune;
    }

    private float CalculateDamageThreshold()
    {
        if (enemyActor == null ||
            !enemyActor.IsInitialized ||
            enemyActor.Definition == null ||
            enemyActor.MaxHealth <= 0)
        {
            return 0f;
        }

        float healthFraction;
        switch (enemyActor.Definition.Category)
        {
            case EnemyCategory.Special:
                healthFraction = specialHealthFraction;
                break;
            case EnemyCategory.MiniBoss:
                healthFraction = miniBossHealthFraction;
                break;
            case EnemyCategory.Boss:
                healthFraction = bossHealthFraction;
                break;
            default:
                healthFraction = normalHealthFraction;
                break;
        }

        return Mathf.Max(1f, enemyActor.MaxHealth * Mathf.Max(0.01f, healthFraction));
    }

    private void StartStagger(float duration)
    {
        if (duration <= 0f || IsStaggerImmune)
        {
            return;
        }

        isStaggered = true;
        activeStaggerDuration = duration;
        remainingStaggerTime = duration;
        remainingBuildupGraceTime = 0f;
        SetMeterNormalized(1f);

        StaggerApplied?.Invoke(this, duration, remainingStaggerTime);
    }

    private void FinishStagger()
    {
        bool wasStaggered = isStaggered;

        remainingStaggerTime = 0f;
        activeStaggerDuration = 0f;
        isStaggered = false;
        remainingBuildupGraceTime = 0f;
        remainingImmunityTime = PostStaggerImmunitySeconds;
        SetMeterNormalized(0f);

        if (wasStaggered)
        {
            StaggerEnded?.Invoke(this);
        }
    }

    private void SetMeterNormalized(float value)
    {
        float clamped = Mathf.Clamp01(value);
        if (Mathf.Approximately(staggerMeterNormalized, clamped))
        {
            staggerMeterNormalized = clamped;
            return;
        }

        staggerMeterNormalized = clamped;
        MeterChanged?.Invoke(this, staggerMeterNormalized);
    }

    private void HandleHealthDamage(EnemyActor enemy, int damageAmount)
    {
        if (enemy == enemyActor)
        {
            RegisterDamage(damageAmount);
        }
    }

    private void HandleShieldDamage(EnemyActor enemy, int damageAmount)
    {
        if (enemy == enemyActor)
        {
            RegisterDamage(damageAmount);
        }
    }

    private void HandleEnemyInitialized(EnemyActor enemy)
    {
        if (enemy == enemyActor)
        {
            ResetMeterAndTimers(clearImmunity: true);
        }
    }

    private void HandleEnemyDefeated(EnemyActor enemy)
    {
        if (enemy == enemyActor)
        {
            bool wasStaggered = isStaggered;
            ResetMeterAndTimers(clearImmunity: true);
            if (wasStaggered)
            {
                StaggerEnded?.Invoke(this);
            }
        }
    }

    private void ResetMeterAndTimers(bool clearImmunity)
    {
        remainingStaggerTime = 0f;
        activeStaggerDuration = 0f;
        remainingBuildupGraceTime = 0f;
        isStaggered = false;
        if (clearImmunity)
        {
            remainingImmunityTime = 0f;
        }
        SetMeterNormalized(0f);
    }

    private void TryInstallPresentation()
    {
        if (!hitFlashPresenterInstalled)
        {
            hitFlashPresenterInstalled = EnemyHitFlashPresenter.EnsureInstalled(gameObject);
        }

        if (!weaknessFillPresenterInstalled)
        {
            weaknessFillPresenterInstalled = EnemyStaggerWeaknessFillUI.TryInstall(this);
        }
    }

    private void OnDisable()
    {
        Unsubscribe();
        ResetMeterAndTimers(clearImmunity: true);
        hitFlashPresenterInstalled = false;
        weaknessFillPresenterInstalled = false;
    }

    private void OnValidate()
    {
        buildupDecayDelay = Mathf.Max(0f, buildupDecayDelay);
        buildupDecayDuration = Mathf.Max(0.05f, buildupDecayDuration);
        baseStaggerDuration = Mathf.Max(0.05f, baseStaggerDuration);
        normalHealthFraction = Mathf.Max(0.01f, normalHealthFraction);
        specialHealthFraction = Mathf.Max(normalHealthFraction, specialHealthFraction);
        miniBossHealthFraction = Mathf.Max(specialHealthFraction, miniBossHealthFraction);
        bossHealthFraction = Mathf.Max(miniBossHealthFraction, bossHealthFraction);
    }
}