using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class RunUpgradeRuntime : MonoBehaviour
{
    private sealed class OwnedUpgrade
    {
        public RunUpgradeDefinition Definition;
        public int Stacks;
    }

    private readonly Dictionary<string, OwnedUpgrade> ownedUpgrades =
        new Dictionary<string, OwnedUpgrade>(StringComparer.Ordinal);

    private RunUpgradeCatalog catalog;
    private PlayerActor playerActor;
    private WaveController waveController;
    private System.Random draftRandom;
    private int cardSeed;
    private int baseMaximumHealth;
    private bool observedPlayerInitialization;

    public static RunUpgradeRuntime Current { get; private set; }

    public event Action<RunUpgradeDefinition, int> UpgradeChanged;

    public RunUpgradeCatalog Catalog => catalog;
    public int CardSeed => cardSeed;

    [RuntimeInitializeOnLoadMethod(
        RuntimeInitializeLoadType.SubsystemRegistration
    )]
    private static void ResetStaticState()
    {
        Current = null;
    }

    private void OnEnable()
    {
        if (Current != null && Current != this)
        {
            Debug.LogError(
                "Only one RunUpgradeRuntime may own a scene run.",
                this
            );
            enabled = false;
            return;
        }

        Current = this;
        SubscribeToPlayer();
    }

    private void OnDisable()
    {
        UnsubscribeFromPlayer();

        if (Current == this)
        {
            Current = null;
        }
    }

    public void Configure(
        RunUpgradeCatalog upgradeCatalog,
        PlayerActor runPlayer,
        WaveController waves)
    {
        UnsubscribeFromPlayer();

        catalog = upgradeCatalog;
        playerActor = runPlayer;
        waveController = waves;

        ResetRun();
        SubscribeToPlayer();

        if (playerActor != null && playerActor.IsInitialized)
        {
            baseMaximumHealth = playerActor.MaximumHealth;
            observedPlayerInitialization = true;
        }

        if (catalog != null)
        {
            catalog.ValidateCatalog(this);
        }
    }

    public void ResetRun()
    {
        ownedUpgrades.Clear();
        draftRandom = null;
        cardSeed = 0;
        baseMaximumHealth = 0;
        observedPlayerInitialization = false;
    }

    public int GetStackCount(string upgradeId)
    {
        if (string.IsNullOrWhiteSpace(upgradeId) ||
            !ownedUpgrades.TryGetValue(upgradeId, out OwnedUpgrade owned))
        {
            return 0;
        }

        return owned.Stacks;
    }

    public int GetStackCount(RunUpgradeDefinition definition)
    {
        return definition == null
            ? 0
            : GetStackCount(definition.UpgradeId);
    }

    public bool IsEligible(
        RunUpgradeDefinition definition,
        PlayerActor runPlayer,
        int currentWave)
    {
        if (definition == null ||
            string.IsNullOrWhiteSpace(definition.UpgradeId) ||
            currentWave < definition.MinimumWave ||
            (definition.MaximumWave > 0 &&
             currentWave > definition.MaximumWave) ||
            GetStackCount(definition) >= definition.MaxStacks)
        {
            return false;
        }

        PlayerDefinition playerDefinition =
            runPlayer != null ? runPlayer.Definition : null;
        CharacterAbilityDefinition abilityDefinition =
            runPlayer != null ? runPlayer.ActiveAbility : null;

        if (!string.IsNullOrEmpty(definition.RequiredPlayerId) &&
            (playerDefinition == null ||
             !string.Equals(
                 definition.RequiredPlayerId,
                 playerDefinition.PlayerId,
                 StringComparison.Ordinal)))
        {
            return false;
        }

        if (!string.IsNullOrEmpty(definition.RequiredAbilityId) &&
            (abilityDefinition == null ||
             !string.Equals(
                 definition.RequiredAbilityId,
                 abilityDefinition.AbilityId,
                 StringComparison.Ordinal)))
        {
            return false;
        }

        IReadOnlyList<string> prerequisites =
            definition.PrerequisiteUpgradeIds;

        for (int index = 0; index < prerequisites.Count; index++)
        {
            if (string.IsNullOrEmpty(prerequisites[index]) ||
                GetStackCount(prerequisites[index]) == 0)
            {
                return false;
            }
        }

        IReadOnlyList<string> exclusions = definition.ExcludedUpgradeIds;

        for (int index = 0; index < exclusions.Count; index++)
        {
            if (!string.IsNullOrEmpty(exclusions[index]) &&
                GetStackCount(exclusions[index]) > 0)
            {
                return false;
            }
        }

        foreach (OwnedUpgrade owned in ownedUpgrades.Values)
        {
            IReadOnlyList<string> ownedExclusions =
                owned.Definition.ExcludedUpgradeIds;

            for (int index = 0; index < ownedExclusions.Count; index++)
            {
                if (string.Equals(
                        ownedExclusions[index],
                        definition.UpgradeId,
                        StringComparison.Ordinal))
                {
                    return false;
                }
            }
        }

        return true;
    }

    public bool TryApply(
        RunUpgradeDefinition definition,
        int currentWave)
    {
        if (!IsEligible(definition, playerActor, currentWave))
        {
            return false;
        }

        EnsureBaseMaximumHealth();

        int maximumHealthBefore =
            RunUpgradeResolver.ResolveMaximumHealth(
                baseMaximumHealth,
                this
            );

        if (!ownedUpgrades.TryGetValue(
                definition.UpgradeId,
                out OwnedUpgrade owned))
        {
            owned = new OwnedUpgrade
            {
                Definition = definition,
                Stacks = 0
            };
            ownedUpgrades.Add(definition.UpgradeId, owned);
        }

        owned.Stacks++;

        int maximumHealthAfter =
            RunUpgradeResolver.ResolveMaximumHealth(
                baseMaximumHealth,
                this
            );

        int addedMaximumHealth =
            Mathf.Max(0, maximumHealthAfter - maximumHealthBefore);

        if (addedMaximumHealth > 0 && playerActor != null)
        {
            playerActor.IncreaseMaximumHealth(
                addedMaximumHealth,
                healAddedAmount: true
            );
        }

        UpgradeChanged?.Invoke(definition, owned.Stacks);
        return true;
    }

    public List<RunUpgradeDefinition> GetOwnedDefinitions()
    {
        List<RunUpgradeDefinition> definitions =
            new List<RunUpgradeDefinition>(ownedUpgrades.Count);

        foreach (OwnedUpgrade owned in ownedUpgrades.Values)
        {
            if (owned != null && owned.Definition != null && owned.Stacks > 0)
            {
                definitions.Add(owned.Definition);
            }
        }

        definitions.Sort(
            (first, second) => string.CompareOrdinal(
                first.UpgradeId,
                second.UpgradeId
            )
        );

        return definitions;
    }

    public bool HasMechanic(RunUpgradeMechanic mechanic)
    {
        if (mechanic == RunUpgradeMechanic.None)
        {
            return false;
        }

        foreach (OwnedUpgrade owned in ownedUpgrades.Values)
        {
            IReadOnlyList<RunUpgradeMechanicGrant> grants =
                owned.Definition.MechanicGrants;

            for (int index = 0; index < grants.Count; index++)
            {
                if (grants[index] != null &&
                    grants[index].Mechanic == mechanic)
                {
                    return true;
                }
            }
        }

        return false;
    }

    public System.Random GetDraftRandom()
    {
        if (draftRandom != null)
        {
            return draftRandom;
        }

        int encounterSeed =
            waveController != null ? waveController.EncounterSeed : 0;

        if (encounterSeed == 0)
        {
            encounterSeed = Environment.TickCount;
        }

        cardSeed = unchecked(
            (encounterSeed * 486187739) ^ (int)0x6D2B79F5
        );

        if (cardSeed == 0)
        {
            cardSeed = 1;
        }

        draftRandom = new System.Random(cardSeed);
        return draftRandom;
    }

    private void EnsureBaseMaximumHealth()
    {
        if (baseMaximumHealth <= 0 &&
            playerActor != null &&
            playerActor.IsInitialized)
        {
            baseMaximumHealth = playerActor.MaximumHealth;
        }
    }

    private void SubscribeToPlayer()
    {
        if (playerActor == null)
        {
            return;
        }

        playerActor.Initialized -= HandlePlayerInitialized;
        playerActor.Initialized += HandlePlayerInitialized;
    }

    private void UnsubscribeFromPlayer()
    {
        if (playerActor != null)
        {
            playerActor.Initialized -= HandlePlayerInitialized;
        }
    }

    private void HandlePlayerInitialized(PlayerActor initializedPlayer)
    {
        if (initializedPlayer == playerActor)
        {
            if (observedPlayerInitialization)
            {
                ResetRun();
            }

            baseMaximumHealth = initializedPlayer.MaximumHealth;
            observedPlayerInitialization = true;
        }
    }
}
