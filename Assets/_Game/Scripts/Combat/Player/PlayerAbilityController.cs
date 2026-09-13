using System;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(PlayerActor))]
[RequireComponent(typeof(PlayerAbilityEnergy))]
public sealed class PlayerAbilityController :
    MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private PlayerActor playerActor;

    [SerializeField]
    private PlayerAbilityEnergy playerAbilityEnergy;

    private IPlayerAbilityRuntime activeRuntime;

    public event Action StateChanged;

    public CharacterAbilityDefinition ActiveAbility =>
        playerActor != null
            ? playerActor.ActiveAbility
            : null;

    public int CurrentEnergy =>
        playerAbilityEnergy != null
            ? playerAbilityEnergy.CurrentEnergy
            : 0;

    public int RequiredEnergy =>
        ActiveAbility != null
            ? RunUpgradeResolver.ResolveAbilityEnergyCost(
                ActiveAbility.EnergyCost,
                ActiveAbility
            )
            : 0;

    public bool IsAbilityActive =>
        IsRuntimeAlive(activeRuntime) &&
        activeRuntime.IsActive;

    public float ChargeNormalized
    {
        get
        {
            int requiredEnergy =
                RequiredEnergy;

            if (requiredEnergy <= 0)
            {
                return 0f;
            }

            return Mathf.Clamp01(
                (float)CurrentEnergy /
                requiredEnergy
            );
        }
    }

    public bool CanActivate
    {
        get
        {
            CharacterAbilityDefinition definition =
                ActiveAbility;

            if (!isActiveAndEnabled ||
                definition == null ||
                playerActor == null ||
                playerAbilityEnergy == null ||
                !playerActor.IsInitialized ||
                playerActor.IsDefeated ||
                !IsRuntimeAvailable(activeRuntime) ||
                activeRuntime.IsActive)
            {
                return false;
            }

            int energyCost = RequiredEnergy;

            return
                playerAbilityEnergy.CurrentEnergy >=
                energyCost &&
                activeRuntime.Supports(definition) &&
                activeRuntime.CanActivate(definition);
        }
    }

    private void Awake()
    {
        ResolveReferences();
    }

    private void OnEnable()
    {
        ResolveReferences();
        SubscribeToPlayer();
        SubscribeToEnergy();
        RefreshRuntime();
    }

    private void Start()
    {
        if (!ValidateReferences())
        {
            enabled = false;
        }
    }

    private void OnDisable()
    {
        if (IsRuntimeAlive(activeRuntime) &&
            activeRuntime.IsActive)
        {
            activeRuntime.Cancel();
        }

        UnsubscribeFromRuntime();
        UnsubscribeFromPlayer();
        UnsubscribeFromEnergy();
    }

    public bool TryActivate()
    {
        CharacterAbilityDefinition definition =
            ActiveAbility;

        if (!isActiveAndEnabled ||
            definition == null ||
            playerActor == null ||
            playerAbilityEnergy == null ||
            !playerActor.IsInitialized ||
            playerActor.IsDefeated)
        {
            return false;
        }

        if (!IsRuntimeAlive(activeRuntime) ||
            !activeRuntime.Supports(definition))
        {
            RefreshRuntime();
        }

        if (!IsRuntimeAvailable(activeRuntime) ||
            activeRuntime.IsActive ||
            !activeRuntime.CanActivate(definition))
        {
            return false;
        }

        int energyCost = RequiredEnergy;

        if (playerAbilityEnergy.CurrentEnergy <
            energyCost)
        {
            return false;
        }

        bool activationSucceeded =
            activeRuntime.TryActivate(
                definition
            );

        if (!activationSucceeded)
        {
            return false;
        }

        /*
         * Energy is spent only after the ability runtime
         * confirms that activation succeeded.
         */
        if (!playerAbilityEnergy.TrySpendEnergy(
                energyCost))
        {
            activeRuntime.Cancel();
            StateChanged?.Invoke();

            return false;
        }

        StateChanged?.Invoke();

        return true;
    }

    public void CancelActiveAbility()
    {
        if (!IsRuntimeAlive(activeRuntime) ||
            !activeRuntime.IsActive)
        {
            return;
        }

        activeRuntime.Cancel();
    }

    public void RefreshRuntime()
    {
        // A disabled coordinator must not reinstall callbacks or runtimes.
        // OnEnable performs the normal discovery and subscription again.
        if (!isActiveAndEnabled)
        {
            return;
        }

        CharacterAbilityDefinition definition =
            ActiveAbility;

        IPlayerAbilityRuntime newRuntime =
            FindRuntimeFor(definition);

        if (ReferenceEquals(
                activeRuntime,
                newRuntime))
        {
            // OnDisable removed this subscription even when the runtime
            // instance survived. The idempotent helper restores it once.
            SubscribeToRuntime();
            StateChanged?.Invoke();
            return;
        }

        if (IsRuntimeAlive(activeRuntime) &&
            activeRuntime.IsActive)
        {
            activeRuntime.Cancel();
        }

        UnsubscribeFromRuntime();

        activeRuntime = newRuntime;

        SubscribeToRuntime();

        StateChanged?.Invoke();
    }

    private static bool IsRuntimeAlive(IPlayerAbilityRuntime runtime)
    {
        // Interface references do not use Unity's destroyed-object null check.
        return runtime != null &&
               (!(runtime is UnityEngine.Object unityObject) ||
                unityObject != null);
    }

    private static bool IsRuntimeAvailable(IPlayerAbilityRuntime runtime)
    {
        return IsRuntimeAlive(runtime) &&
               (!(runtime is Behaviour behaviour) ||
                behaviour.isActiveAndEnabled);
    }

    private IPlayerAbilityRuntime FindRuntimeFor(
        CharacterAbilityDefinition definition)
    {
        if (definition == null)
        {
            return null;
        }

        EnsureDeclaredRuntime(definition);

        MonoBehaviour[] components =
            GetComponents<MonoBehaviour>();

        foreach (MonoBehaviour component
                 in components)
        {
            if (component != null &&
                component is
                    IPlayerAbilityRuntime runtime &&
                runtime.Supports(definition))
            {
                return runtime;
            }
        }

        return null;
    }

    private void EnsureDeclaredRuntime(
        CharacterAbilityDefinition definition)
    {
        if (definition == null)
        {
            return;
        }

        Type runtimeType =
            definition.RuntimeType;

        if (runtimeType == null)
        {
            return;
        }

        bool isValidRuntimeType =
            typeof(MonoBehaviour)
                .IsAssignableFrom(runtimeType) &&
            typeof(IPlayerAbilityRuntime)
                .IsAssignableFrom(runtimeType);

        if (!isValidRuntimeType)
        {
            Debug.LogError(
                $"Ability '{definition.DisplayName}' declares invalid " +
                $"runtime type '{runtimeType.FullName}'. Runtime types must " +
                "derive from MonoBehaviour and implement IPlayerAbilityRuntime.",
                definition
            );

            return;
        }

        if (GetComponent(runtimeType) != null)
        {
            return;
        }

        gameObject.AddComponent(runtimeType);
    }

    private void ResolveReferences()
    {
        if (playerActor == null)
        {
            playerActor =
                GetComponent<PlayerActor>();
        }

        if (playerAbilityEnergy == null)
        {
            playerAbilityEnergy =
                GetComponent<PlayerAbilityEnergy>();
        }
    }

    private void SubscribeToPlayer()
    {
        if (playerActor == null)
        {
            return;
        }

        playerActor.Initialized -=
            HandlePlayerInitialized;

        playerActor.Initialized +=
            HandlePlayerInitialized;

        playerActor.Defeated -=
            HandlePlayerDefeated;

        playerActor.Defeated +=
            HandlePlayerDefeated;
    }

    private void UnsubscribeFromPlayer()
    {
        if (playerActor == null)
        {
            return;
        }

        playerActor.Initialized -=
            HandlePlayerInitialized;

        playerActor.Defeated -=
            HandlePlayerDefeated;
    }

    private void SubscribeToEnergy()
    {
        if (playerAbilityEnergy == null)
        {
            return;
        }

        playerAbilityEnergy.EnergyChanged -=
            HandleEnergyChanged;

        playerAbilityEnergy.EnergyChanged +=
            HandleEnergyChanged;
    }

    private void UnsubscribeFromEnergy()
    {
        if (playerAbilityEnergy == null)
        {
            return;
        }

        playerAbilityEnergy.EnergyChanged -=
            HandleEnergyChanged;
    }

    private void SubscribeToRuntime()
    {
        if (!isActiveAndEnabled || !IsRuntimeAlive(activeRuntime))
        {
            return;
        }

        activeRuntime.StateChanged -=
            HandleRuntimeStateChanged;

        activeRuntime.StateChanged +=
            HandleRuntimeStateChanged;
    }

    private void UnsubscribeFromRuntime()
    {
        if (!IsRuntimeAlive(activeRuntime))
        {
            return;
        }

        activeRuntime.StateChanged -=
            HandleRuntimeStateChanged;
    }

    private void HandlePlayerInitialized(
        PlayerActor initializedPlayer)
    {
        if (IsRuntimeAlive(activeRuntime) &&
            activeRuntime.IsActive)
        {
            activeRuntime.Cancel();
        }

        RefreshRuntime();
    }

    private void HandlePlayerDefeated(
        PlayerActor defeatedPlayer)
    {
        CancelActiveAbility();
        StateChanged?.Invoke();
    }

    private void HandleEnergyChanged(
        int currentEnergy,
        int maximumEnergy)
    {
        StateChanged?.Invoke();
    }

    private void HandleRuntimeStateChanged()
    {
        StateChanged?.Invoke();
    }

    private bool ValidateReferences()
    {
        bool isValid = true;

        if (playerActor == null)
        {
            Debug.LogError(
                "PlayerAbilityController requires " +
                "a PlayerActor.",
                this
            );

            isValid = false;
        }

        if (playerAbilityEnergy == null)
        {
            Debug.LogError(
                "PlayerAbilityController requires " +
                "PlayerAbilityEnergy.",
                this
            );

            isValid = false;
        }

        return isValid;
    }
}
