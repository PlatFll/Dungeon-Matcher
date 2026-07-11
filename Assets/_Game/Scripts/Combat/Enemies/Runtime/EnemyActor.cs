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

        definition = enemyDefinition;
        RuntimeStats = runtimeStats;
        assignedGemType = gemType;

        currentHealth =
            RuntimeStats.MaxHealth;

        currentSpecialTurnCount = 0;
        isSpecialReady = false;
        isDefeated = false;
        isInitialized = true;

        gameObject.name =
            $"{definition.DisplayName}_Level_{RuntimeStats.Level}";

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

        DamageReceived?.Invoke(
            this,
            actualDamage
        );

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

        Defeated?.Invoke(this);
    }
}