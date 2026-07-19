using System;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class PlayerAbilityEnergy : MonoBehaviour
{
    [Header("Energy")]
    [SerializeField, Min(1)]
    private int maximumEnergy = 100;

    [SerializeField, Min(0)]
    private int startingEnergy = 0;

    public event Action<int, int> EnergyChanged;

    public int CurrentEnergy { get; private set; }

    public int MaximumEnergy => maximumEnergy;

    public bool IsFull =>
        CurrentEnergy >= maximumEnergy;

    [Header("Energy Gain")]
    [SerializeField, Min(0)]
    private int energyPerMatch = 10;

    [SerializeField, Min(0)]
    private int bonusEnergyPerCascadeDepth = 3;

    [SerializeField, Min(0)]
    private int matchingGemTypeBonus = 2;

    [Header("Match Source")]
    [SerializeField]
    private CombatController combatController;

    [SerializeField]
    private GemType characterGemType =
        GemType.Emerald;

    private void Awake()
    {
        CurrentEnergy =
            Mathf.Clamp(
                startingEnergy,
                0,
                maximumEnergy
            );
    }

    public void AddEnergy(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        SetEnergy(
            CurrentEnergy + amount
        );
    }

    public void GainEnergyFromMatch(
    GemType matchedGemType,
    int cascadeDepth,
    GemType characterGemType)
    {
        int gainedEnergy =
            energyPerMatch;

        gainedEnergy +=
            Mathf.Max(0, cascadeDepth) *
            bonusEnergyPerCascadeDepth;

        if (matchedGemType == characterGemType)
        {
            gainedEnergy +=
                matchingGemTypeBonus;
        }

        AddEnergy(gainedEnergy);
    }

    public bool TrySpendEnergy(int amount)
    {
        if (amount <= 0 ||
            CurrentEnergy < amount)
        {
            return false;
        }

        SetEnergy(
            CurrentEnergy - amount
        );

        return true;
    }

    public void ResetEnergy()
    {
        SetEnergy(0);
    }

    private void SetEnergy(int newEnergy)
    {
        int clampedEnergy =
            Mathf.Clamp(
                newEnergy,
                0,
                maximumEnergy
            );

        if (clampedEnergy == CurrentEnergy)
        {
            return;
        }

        CurrentEnergy = clampedEnergy;

        EnergyChanged?.Invoke(
            CurrentEnergy,
            maximumEnergy
        );
    }

    private void OnValidate()
    {
        maximumEnergy =
            Mathf.Max(
                1,
                maximumEnergy
            );

        startingEnergy =
            Mathf.Clamp(
                startingEnergy,
                0,
                maximumEnergy
            );
    }
}