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
            ? Mathf.Max(
                1,
                ActiveAbility.EnergyCost
            )
            : 0;

    public bool IsAbilityActive =>
        activeRuntime != null &&
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

            if (definition == null ||
                playerActor == null ||
                playerAbilityEnergy == null ||
                !playerActor.IsInitialized ||
                playerActor.IsDefeated ||
                activeRuntime == null ||
                activeRuntime.IsActive)
            {
                return false;
            }

            int energyCost =
                Mathf.Max(
                    1,
                    definition.EnergyCost
                );

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
        if (activeRuntime != null &&
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

        if (definition == null ||
            playerActor == null ||
            playerAbilityEnergy == null ||
            !playerActor.IsInitialized ||
            playerActor.IsDefeated)
        {
            return false;
        }

        if (activeRuntime == null ||
            !activeRuntime.Supports(definition))
        {
            RefreshRuntime();
        }

        if (activeRuntime == null ||
            activeRuntime.IsActive ||
            !activeRuntime.CanActivate(definition))
        {
            return false;
        }

        int energyCost =
            Mathf.Max(
                1,
                definition.EnergyCost
            );

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
        if (activeRuntime == null ||
            !activeRuntime.IsActive)
        {
            return;
        }

        activeRuntime.Cancel();
    }

    public void RefreshRuntime()
    {
        CharacterAbilityDefinition definition =
            ActiveAbility;

        IPlayerAbilityRuntime newRuntime =
            FindRuntimeFor(definition);

        if (ReferenceEquals(
                activeRuntime,
                newRuntime))
        {
            StateChanged?.Invoke();
            return;
        }

        if (activeRuntime != null &&
            activeRuntime.IsActive)
        {
            activeRuntime.Cancel();
        }

        UnsubscribeFromRuntime();

        activeRuntime = newRuntime;

        SubscribeToRuntime();

        StateChanged?.Invoke();
    }

    private IPlayerAbilityRuntime FindRuntimeFor(
        CharacterAbilityDefinition definition)
    {
        if (definition == null)
        {
            return null;
        }

        MonoBehaviour[] components =
            GetComponents<MonoBehaviour>();

        foreach (MonoBehaviour component
                 in components)
        {
            if (component is
                    IPlayerAbilityRuntime runtime &&
                runtime.Supports(definition))
            {
                return runtime;
            }
        }

        return null;
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
        if (activeRuntime == null)
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
        if (activeRuntime == null)
        {
            return;
        }

        activeRuntime.StateChanged -=
            HandleRuntimeStateChanged;
    }

    private void HandlePlayerInitialized(
        PlayerActor initializedPlayer)
    {
        if (activeRuntime != null &&
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