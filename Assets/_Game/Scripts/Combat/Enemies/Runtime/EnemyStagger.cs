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

    [Header("Unified moves: Normal / Special / Miniboss / Boss")]
    [SerializeField] private Vector4 unifiedHealthFractions = new Vector4(.30f, .35f, .30f, .25f);
    [SerializeField] private Vector4 unifiedDecayDamage = new Vector4(10f, 7.5f, 5f, 3f);
    [SerializeField, Min(1)] private int unifiedStaggerMoves = 2;
    [SerializeField, Min(0)] private int unifiedImmunityMoves = 2;
    private int lastUnifiedExpiry = -1;
    public float OffColorDecayDamage => unifiedDecayDamage[(int)enemyActor.Definition.Category];

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

    private int moveLastHit = -1, moveStaggerApplied = -1, moveImmunityApplied = -1;
    private float activeStaggerDuration;
    public void ExpireAcceptedMove(int move)
    {
        if (!CombatMoveClock.MoveEffects || enemyActor == null || enemyActor.IsDefeated) return;
        if (CombatMoveClock.Unified)
        {
            if (move <= lastUnifiedExpiry) return;
            lastUnifiedExpiry = move;
        }
        if (IsStaggered)
        {
            if (move <= moveStaggerApplied) return;
            remainingStaggerTime = Mathf.Max(0, remainingStaggerTime - 1);
            if (remainingStaggerTime == 0) FinishStagger();
        }
        else if (remainingImmunityTime > 0)
        {
            if (move > moveImmunityApplied) remainingImmunityTime = Mathf.Max(0, remainingImmunityTime - 1);
        }
        else if (CombatMoveClock.Unified)
        {
            if (move > moveLastHit) SetMeterNormalized(staggerMeterNormalized - Mathf.Max(0, OffColorDecayDamage) / DamageThreshold);
        }
        else if (move > moveLastHit + 1) SetMeterNormalized(staggerMeterNormalized - .25f);
        remainingBuildupGraceTime = Mathf.Max(0, moveLastHit + 1 - move);
    }
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

    public void CaptureContinuation(EnemyCombatSnapshot saved)
    {
        saved.staggerMeter=staggerMeterNormalized; saved.staggerRemaining=remainingStaggerTime;
        saved.staggerDuration=activeStaggerDuration; saved.staggerImmunity=remainingImmunityTime;
        saved.staggerGrace=remainingBuildupGraceTime;
        saved.staggerHitMove=moveLastHit; saved.staggerAppliedMove=moveStaggerApplied; saved.immunityAppliedMove=moveImmunityApplied;
        saved.staggerDecayMove=lastUnifiedExpiry;
    }
    public void RestoreContinuation(EnemyCombatSnapshot saved)
    {
        remainingStaggerTime=saved.staggerRemaining; activeStaggerDuration=saved.staggerDuration;
        remainingImmunityTime=saved.staggerImmunity; remainingBuildupGraceTime=saved.staggerGrace;
        isStaggered=remainingStaggerTime>0; SetMeterNormalized(saved.staggerMeter);
        moveLastHit=saved.staggerHitMove; moveStaggerApplied=saved.staggerAppliedMove; moveImmunityApplied=saved.immunityAppliedMove;
        lastUnifiedExpiry=saved.staggerDecayMove;
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

        if (CombatMoveClock.MoveEffects) return;
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
        remainingBuildupGraceTime = CombatMoveClock.MoveEffects ? 1 : Mathf.Max(0f, buildupDecayDelay);
        moveLastHit = CombatMoveClock.EffectAction;
        float buildup = RunSession.Current?.Player?.Statuses.StaggerBuildupMultiplier ?? 1f;
        SetMeterNormalized(before + damageAmount / threshold * buildup);

        float added = Mathf.Max(0f, staggerMeterNormalized - before);

        // Fractions such as 50 HP * 0.30 can round just above 15, leaving
        // an exact-threshold hit a floating-point step below a full meter.
        if (staggerMeterNormalized >= 1f || Mathf.Approximately(staggerMeterNormalized, 1f))
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
        return CombatMoveClock.Unified ? remainingStaggerTime : duration;
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
            case EnemyCategory.Miniboss:
                healthFraction = miniBossHealthFraction;
                break;
            case EnemyCategory.Boss:
                healthFraction = bossHealthFraction;
                break;
            default:
                healthFraction = normalHealthFraction;
                break;
        }

        if (CombatMoveClock.Unified) healthFraction = unifiedHealthFractions[(int)enemyActor.Definition.Category];
        return Mathf.Max(1f, enemyActor.MaxHealth * Mathf.Max(0.01f, healthFraction));
    }

    private void StartStagger(float duration)
    {
        if (duration <= 0f || IsStaggerImmune)
        {
            return;
        }

        if (CombatMoveClock.MoveEffects) duration = CombatMoveClock.Unified ? Mathf.Max(1, unifiedStaggerMoves) : 1;
        moveStaggerApplied = CombatMoveClock.EffectAction;
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
        remainingImmunityTime = CombatMoveClock.Unified ? Mathf.Max(0, unifiedImmunityMoves) : CombatMoveClock.MoveEffects ? 2 : PostStaggerImmunitySeconds;
        moveImmunityApplied = CombatMoveClock.EffectAction;
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
        lastUnifiedExpiry = -1;
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
