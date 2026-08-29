using System;
using UnityEngine;

public sealed class PlayerDamageContext
{
    public int OriginalAmount { get; }

    public int Amount { get; set; }

    public UnityEngine.Object Source { get; }

    public bool IsCancelled { get; private set; }

    public PlayerDamageContext(
        int amount,
        UnityEngine.Object source)
    {
        OriginalAmount = Mathf.Max(0, amount);
        Amount = OriginalAmount;
        Source = source;
    }

    public void Cancel()
    {
        IsCancelled = true;
        Amount = 0;
    }
}

[DisallowMultipleComponent]
public sealed class PlayerActor : MonoBehaviour
{
    [Header("Prototype Startup")]
    [SerializeField]
    private PlayerDefinition startingDefinition;

    [SerializeField]
    private bool initializeOnStart = true;

    [Header("Shield")]
    [SerializeField, Min(1)]
    [Tooltip(
        "Maximum shield the player may hold at once. Shield Bombs top this " +
        "resource up but can never stack beyond this cap."
    )]
    private int maximumShield = 30;

    [SerializeField, Range(0f, 0.95f)]
    [Tooltip(
        "Incoming damage reduction applied to an attack when the player had " +
        "shield at the start of that attack. 0.25 means 25 percent."
    )]
    private float shieldDamageReduction = 0.25f;

    [Header("Runtime Debug Information")]
    [SerializeField]
    private PlayerDefinition definition;

    [SerializeField]
    private int maximumHealth;

    [SerializeField]
    private int currentHealth;

    [SerializeField]
    private int currentShield;

    [SerializeField]
    private int revivalCount;

    [SerializeField]
    private bool isInitialized;

    [SerializeField]
    private bool isDefeated;

    public event Action<PlayerActor> Initialized;

    public event Action<
        PlayerActor,
        PlayerDamageContext
    > BeforeDamage;

    public event Action<PlayerActor, int>
        DamageTaken;

    public event Action<PlayerActor, int>
        ShieldDamaged;

    public event Action<PlayerActor, int, int>
        ShieldChanged;

    public event Action<PlayerActor, int>
        Healed;

    public event Action<PlayerActor, int, int>
        HealthChanged;

    public event Action<PlayerActor, int, int>
        MaximumHealthChanged;

    public event Action<PlayerActor>
        Defeated;

    public event Action<PlayerActor, int>
        Revived;

    public PlayerDefinition Definition =>
        definition;

    public CharacterAbilityDefinition ActiveAbility =>
        definition != null
            ? definition.ActiveAbility
            : null;

    public CharacterPassiveDefinition PassiveAbility =>
        definition != null
            ? definition.PassiveAbility
            : null;

    public int CurrentHealth =>
        currentHealth;

    public int MaximumHealth =>
        maximumHealth;

    public int CurrentShield =>
        currentShield;

    public int MaximumShield =>
        maximumShield;

    public float ShieldDamageReduction =>
        shieldDamageReduction;

    public bool HasShield =>
        currentShield > 0;

    public int RevivalCount =>
        revivalCount;

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
                maximumHealth <= 0)
            {
                return 0f;
            }

            return Mathf.Clamp01(
                (float)currentHealth /
                maximumHealth
            );
        }
    }

    public float ShieldNormalized
    {
        get
        {
            if (!isInitialized ||
                maximumShield <= 0)
            {
                return 0f;
            }

            return Mathf.Clamp01(
                (float)currentShield /
                maximumShield
            );
        }
    }

    private void Start()
    {
        if (!initializeOnStart)
        {
            return;
        }

        PlayerDefinition definitionToInitialize =
            PlayerDefinitionRegistry.ResolveSelectedOrFallback(
                startingDefinition
            );

        if (definitionToInitialize != null)
        {
            Initialize(
                definitionToInitialize
            );
        }
    }

    public void Initialize(
        PlayerDefinition playerDefinition,
        int maximumHealthOverride = 0)
    {
        if (playerDefinition == null)
        {
            Debug.LogError(
                "PlayerActor cannot initialize without " +
                "a PlayerDefinition.",
                this
            );

            return;
        }

        definition = playerDefinition;

        maximumHealth =
            maximumHealthOverride > 0
                ? maximumHealthOverride
                : definition.BaseMaxHealth;

        maximumHealth =
            Mathf.Max(1, maximumHealth);

        maximumShield =
            Mathf.Max(1, maximumShield);

        currentHealth = maximumHealth;
        currentShield = 0;
        revivalCount = 0;
        isDefeated = false;
        isInitialized = true;

        gameObject.name =
            $"PlayerRuntime_{definition.DisplayName}";

        Initialized?.Invoke(this);

        MaximumHealthChanged?.Invoke(
            this,
            maximumHealth,
            maximumHealth
        );

        HealthChanged?.Invoke(
            this,
            currentHealth,
            maximumHealth
        );

        ShieldChanged?.Invoke(
            this,
            currentShield,
            maximumShield
        );

        Debug.Log(
            $"Initialized player " +
            $"{definition.DisplayName}. " +
            $"HP: {currentHealth}/{maximumHealth}.",
            this
        );
    }

    public bool TryTakeDamage(
        int amount,
        UnityEngine.Object source = null)
    {
        if (!CanReceiveDamage ||
            amount <= 0)
        {
            return false;
        }

        PlayerDamageContext context =
            new PlayerDamageContext(
                amount,
                source
            );

        BeforeDamage?.Invoke(
            this,
            context
        );

        if (context.IsCancelled)
        {
            return false;
        }

        int finalDamage =
            Mathf.Max(0, context.Amount);

        if (finalDamage == 0)
        {
            return false;
        }

        bool shieldWasActive =
            currentShield > 0;

        if (shieldWasActive)
        {
            finalDamage =
                Mathf.Max(
                    1,
                    Mathf.CeilToInt(
                        finalDamage *
                        (1f - shieldDamageReduction)
                    )
                );
        }

        int shieldDamage =
            Mathf.Min(
                currentShield,
                finalDamage
            );

        if (shieldDamage > 0)
        {
            currentShield -= shieldDamage;

            ShieldDamaged?.Invoke(
                this,
                shieldDamage
            );

            ShieldChanged?.Invoke(
                this,
                currentShield,
                maximumShield
            );
        }

        int healthDamage =
            finalDamage - shieldDamage;

        int actualHealthDamage = 0;

        if (healthDamage > 0)
        {
            int previousHealth =
                currentHealth;

            currentHealth =
                Mathf.Max(
                    0,
                    currentHealth - healthDamage
                );

            actualHealthDamage =
                previousHealth - currentHealth;

            if (actualHealthDamage > 0)
            {
                DamageTaken?.Invoke(
                    this,
                    actualHealthDamage
                );

                HealthChanged?.Invoke(
                    this,
                    currentHealth,
                    maximumHealth
                );
            }
        }

        if (currentHealth == 0)
        {
            HandleDefeat();
        }

        return shieldDamage > 0 ||
               actualHealthDamage > 0;
    }

    public int GrantShield(int amount)
    {
        if (!isInitialized ||
            isDefeated ||
            amount <= 0)
        {
            return 0;
        }

        int previousShield =
            currentShield;

        currentShield =
            Mathf.Min(
                maximumShield,
                currentShield + amount
            );

        int actualShieldGranted =
            currentShield - previousShield;

        if (actualShieldGranted <= 0)
        {
            return 0;
        }

        ShieldChanged?.Invoke(
            this,
            currentShield,
            maximumShield
        );

        return actualShieldGranted;
    }

    public int Heal(int amount)
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
                maximumHealth,
                currentHealth + amount
            );

        int actualHealing =
            currentHealth - previousHealth;

        if (actualHealing <= 0)
        {
            return 0;
        }

        Healed?.Invoke(
            this,
            actualHealing
        );

        HealthChanged?.Invoke(
            this,
            currentHealth,
            maximumHealth
        );

        return actualHealing;
    }

    public void RestoreToFullHealth()
    {
        if (!isInitialized ||
            isDefeated)
        {
            return;
        }

        Heal(
            maximumHealth - currentHealth
        );
    }

    public bool TryRevive(int restoredHealth)
    {
        if (!isInitialized ||
            !isDefeated ||
            restoredHealth <= 0)
        {
            return false;
        }

        isDefeated = false;

        currentHealth =
            Mathf.Clamp(
                restoredHealth,
                1,
                maximumHealth
            );

        currentShield = 0;
        revivalCount++;

        Revived?.Invoke(
            this,
            revivalCount
        );

        HealthChanged?.Invoke(
            this,
            currentHealth,
            maximumHealth
        );

        ShieldChanged?.Invoke(
            this,
            currentShield,
            maximumShield
        );

        return true;
    }

    public void IncreaseMaximumHealth(
        int amount,
        bool healAddedAmount = true)
    {
        if (!isInitialized ||
            amount <= 0)
        {
            return;
        }

        int previousMaximum =
            maximumHealth;

        maximumHealth += amount;

        if (healAddedAmount &&
            !isDefeated)
        {
            currentHealth =
                Mathf.Min(
                    maximumHealth,
                    currentHealth + amount
                );
        }

        MaximumHealthChanged?.Invoke(
            this,
            previousMaximum,
            maximumHealth
        );

        HealthChanged?.Invoke(
            this,
            currentHealth,
            maximumHealth
        );
    }

    private void HandleDefeat()
    {
        if (isDefeated)
        {
            return;
        }

        isDefeated = true;

        if (currentShield > 0)
        {
            currentShield = 0;

            ShieldChanged?.Invoke(
                this,
                currentShield,
                maximumShield
            );
        }

        Defeated?.Invoke(this);
    }

    private void OnValidate()
    {
        maximumHealth =
            Mathf.Max(0, maximumHealth);

        currentHealth =
            Mathf.Clamp(
                currentHealth,
                0,
                maximumHealth
            );

        maximumShield =
            Mathf.Max(1, maximumShield);

        currentShield =
            Mathf.Clamp(
                currentShield,
                0,
                maximumShield
            );

        shieldDamageReduction =
            Mathf.Clamp(
                shieldDamageReduction,
                0f,
                0.95f
            );

        revivalCount =
            Mathf.Max(0, revivalCount);
    }

    [ContextMenu("Prototype/Take 25 Damage")]
    private void DebugTakeDamage()
    {
        if (Application.isPlaying)
        {
            TryTakeDamage(25);
        }
    }

    [ContextMenu("Prototype/Grant 30 Shield")]
    private void DebugGrantShield()
    {
        if (Application.isPlaying)
        {
            GrantShield(30);
        }
    }

    [ContextMenu("Prototype/Heal 20 HP")]
    private void DebugHeal()
    {
        if (Application.isPlaying)
        {
            Heal(20);
        }
    }

    [ContextMenu("Prototype/Revive With 50 HP")]
    private void DebugRevive()
    {
        if (Application.isPlaying)
        {
            TryRevive(50);
        }
    }
}
