using System;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class EnemyActor : MonoBehaviour
{
    [Header("Runtime Debug Information")]
    [SerializeField]
    private EnemyDefinition definition;

    [SerializeField]
    private GemType assignedGemType;

    [SerializeField]
    private int currentHealth;

    [SerializeField]
    private int currentSpecialTurnCount;

    [SerializeField]
    private bool isSpecialReady;

    [SerializeField]
    private bool isInitialized;

    [SerializeField]
    private bool isDefeated;

    [SerializeField]
    private bool isAutoAttackAnimationActionActive;

    [SerializeField]
    private bool isSpecialAbilityAnimationActionActive;

    public event Action<EnemyActor> Initialized;

    public event Action<EnemyActor, int, int>
        HealthChanged;

    public event Action<EnemyActor, int>
        DamageReceived;

    public event Action<EnemyActor, GemType>
        GemTypeChanged;

    public event Action<EnemyActor, int, int>
        SpecialCounterChanged;

    public event Action<EnemyActor>
        SpecialBecameReady;

    public event Action<EnemyActor>
        SpecialAbilityUsed;

    public event Action<EnemyActor>
        SpecialAbilityImpactReached;

    public event Action<EnemyActor>
        AnimationActionReleased;

    public event Action<EnemyActor>
        Defeated;

    public EnemyDefinition Definition =>
        definition;

    public EnemyRuntimeStats RuntimeStats
    {
        get;
        private set;
    }

    public GemType AssignedGemType =>
        assignedGemType;

    public int CurrentHealth =>
        currentHealth;

    public int MaxHealth =>
        isInitialized
            ? RuntimeStats.MaxHealth
            : 0;

    public int Damage =>
        isInitialized
            ? RuntimeStats.Damage
            : 0;

    public int FollowUpDamage =>
        isInitialized
            ? RuntimeStats.FollowUpDamage
            : 0;

    public int Level =>
        isInitialized
            ? RuntimeStats.Level
            : 0;

    public int Wave =>
        isInitialized
            ? RuntimeStats.Wave
            : 0;

    public float AttackInterval =>
        isInitialized
            ? RuntimeStats.AttackInterval
            : 0f;

    public int SpecialTurnRequirement =>
        isInitialized
            ? RuntimeStats.SpecialTurnRequirement
            : 0;

    public int CurrentSpecialTurnCount =>
        currentSpecialTurnCount;

    public bool HasSpecialAbility =>
        definition != null &&
        definition.HasSpecialAbility;

    public bool IsSpecialReady =>
        isSpecialReady;

    public bool IsInitialized =>
        isInitialized;

    public bool IsDefeated =>
        isDefeated;

    public bool CanReceiveDamage =>
        isInitialized &&
        !isDefeated;

    public bool IsAutoAttackAnimationActionActive =>
        isAutoAttackAnimationActionActive;

    public bool IsSpecialAbilityAnimationActionActive =>
        isSpecialAbilityAnimationActionActive;

    public bool HasAnimationActionInProgress =>
        isAutoAttackAnimationActionActive ||
        isSpecialAbilityAnimationActionActive;

    public float HealthNormalized
    {
        get
        {
            if (!isInitialized ||
                RuntimeStats.MaxHealth <= 0)
            {
                return 0f;
            }

            return Mathf.Clamp01(
                (float)currentHealth /
                RuntimeStats.MaxHealth
            );
        }
    }

    public void Initialize(
        EnemyDefinition enemyDefinition,
        EnemyRuntimeStats runtimeStats,
        GemType gemType)
    {
        if (enemyDefinition == null)
        {
            Debug.LogError(
                "EnemyActor cannot initialize without " +
                "an EnemyDefinition.",
                this
            );

            return;
        }

        /*
         * Gameplay state is authoritative. Establish it before touching any
         * optional animation or visual presenter so a presentation failure on
         * a device can never prevent the enemy from existing in the wave.
         */
        definition = enemyDefinition;
        RuntimeStats = runtimeStats;
        assignedGemType = gemType;

        currentHealth =
            RuntimeStats.MaxHealth;

        currentSpecialTurnCount = 0;
        isSpecialReady = false;
        isDefeated = false;
        isInitialized = true;
        isAutoAttackAnimationActionActive = false;
        isSpecialAbilityAnimationActionActive = false;

        gameObject.name =
            $"{definition.DisplayName}_Level_{RuntimeStats.Level}";

        try
        {
            /*
             * Action animation playback is generic for every enemy. Install
             * the presenter after gameplay initialization so it remains
             * presentation-only.
             */
            EnemyActionAnimationPresenter.EnsureInstalled(
                gameObject
            );

            if (!EnemyVisualPresenter.TryApply(
                    gameObject,
                    definition))
            {
                Debug.LogWarning(
                    $"Could not fully apply the visual profile for " +
                    $"{definition.DisplayName}.",
                    this
                );
            }
        }
        catch (Exception exception)
        {
            Debug.LogError(
                $"Enemy presentation setup failed for " +
                $"{definition.DisplayName}. Gameplay initialization " +
                "will continue.",
                this
            );

            Debug.LogException(
                exception,
                this
            );
        }

        Initialized?.Invoke(this);

        HealthChanged?.Invoke(
            this,
            currentHealth,
            RuntimeStats.MaxHealth
        );

        GemTypeChanged?.Invoke(
            this,
            assignedGemType
        );

        SpecialCounterChanged?.Invoke(
            this,
            currentSpecialTurnCount,
            RuntimeStats.SpecialTurnRequirement
        );
    }

    public bool TryTakeDamage(int amount)
    {
        return TryTakeDamageInternal(
            amount,
            true
        );
    }

    public bool TryTakeDamageWithoutFeedback(
        int amount)
    {
        return TryTakeDamageInternal(
            amount,
            false
        );
    }

    private bool TryTakeDamageInternal(
        int amount,
        bool notifyDamageReceived)
    {
        if (!CanReceiveDamage ||
            amount <= 0)
        {
            return false;
        }

        int previousHealth =
            currentHealth;

        currentHealth =
            Mathf.Max(
                0,
                currentHealth - amount
            );

        int actualDamage =
            previousHealth - currentHealth;

        if (actualDamage <= 0)
        {
            return false;
        }

        /*
         * Normal direct/clear damage uses DamageReceived to drive the
         * existing hit shake. Damage-over-time sources can deliberately
         * suppress that presentation event while still sharing the same
         * authoritative health and defeat path.
         */
        if (notifyDamageReceived)
        {
            DamageReceived?.Invoke(
                this,
                actualDamage
            );
        }

        HealthChanged?.Invoke(
            this,
            currentHealth,
            RuntimeStats.MaxHealth
        );

        if (currentHealth == 0)
        {
            HandleDefeat();
        }

        return true;
    }

    public int RestoreHealth(int amount)
    {
        if (!isInitialized ||
            isDefeated ||
            amount <= 0)
        {
            return 0;
        }

        int previousHealth =
            currentHealth;

        currentHealth =
            Mathf.Min(
                RuntimeStats.MaxHealth,
                currentHealth + amount
            );

        int restoredAmount =
            currentHealth - previousHealth;

        if (restoredAmount > 0)
        {
            HealthChanged?.Invoke(
                this,
                currentHealth,
                RuntimeStats.MaxHealth
            );
        }

        return restoredAmount;
    }

    public void AssignGemType(
        GemType newGemType)
    {
        if (!isInitialized)
        {
            return;
        }

        if (assignedGemType ==
            newGemType)
        {
            return;
        }

        assignedGemType =
            newGemType;

        GemTypeChanged?.Invoke(
            this,
            assignedGemType
        );
    }

    public void RegisterValidPlayerTurn()
    {
        if (!isInitialized ||
            isDefeated ||
            !HasSpecialAbility ||
            isSpecialReady)
        {
            return;
        }

        currentSpecialTurnCount =
            Mathf.Min(
                currentSpecialTurnCount + 1,
                RuntimeStats.SpecialTurnRequirement
            );

        SpecialCounterChanged?.Invoke(
            this,
            currentSpecialTurnCount,
            RuntimeStats.SpecialTurnRequirement
        );

        if (currentSpecialTurnCount >=
            RuntimeStats.SpecialTurnRequirement)
        {
            isSpecialReady = true;

            SpecialBecameReady?.Invoke(this);
        }
    }

    public bool TryBeginAutoAttackAnimationAction()
    {
        if (!isInitialized ||
            isDefeated ||
            HasAnimationActionInProgress)
        {
            return false;
        }

        isAutoAttackAnimationActionActive = true;
        return true;
    }

    public bool TryBeginSpecialAbilityAnimationAction()
    {
        if (!isInitialized ||
            isDefeated ||
            HasAnimationActionInProgress)
        {
            return false;
        }

        isSpecialAbilityAnimationActionActive = true;
        return true;
    }

    public void EndAutoAttackAnimationAction()
    {
        if (!isAutoAttackAnimationActionActive)
        {
            return;
        }

        isAutoAttackAnimationActionActive = false;
        AnimationActionReleased?.Invoke(this);
    }

    public void EndSpecialAbilityAnimationAction()
    {
        if (!isSpecialAbilityAnimationActionActive)
        {
            return;
        }

        isSpecialAbilityAnimationActionActive = false;
        AnimationActionReleased?.Invoke(this);
    }

    public void NotifySpecialAbilityUsed()
    {
        if (!isInitialized ||
            isDefeated ||
            !HasSpecialAbility ||
            !isSpecialReady)
        {
            return;
        }

        SpecialAbilityUsed?.Invoke(this);
    }

    public void NotifySpecialAbilityImpactReached()
    {
        if (!isInitialized ||
            isDefeated ||
            !HasSpecialAbility)
        {
            return;
        }

        SpecialAbilityImpactReached?.Invoke(this);
    }

    public void ResetSpecialCounter()
    {
        if (!isInitialized ||
            !HasSpecialAbility)
        {
            return;
        }

        currentSpecialTurnCount = 0;
        isSpecialReady = false;

        SpecialCounterChanged?.Invoke(
            this,
            currentSpecialTurnCount,
            RuntimeStats.SpecialTurnRequirement
        );
    }

    private void HandleDefeat()
    {
        if (isDefeated)
        {
            return;
        }

        isDefeated = true;
        isSpecialReady = false;

        bool hadAnimationAction =
            HasAnimationActionInProgress;

        isAutoAttackAnimationActionActive = false;
        isSpecialAbilityAnimationActionActive = false;

        if (hadAnimationAction)
        {
            AnimationActionReleased?.Invoke(this);
        }

        Defeated?.Invoke(this);
    }
}
