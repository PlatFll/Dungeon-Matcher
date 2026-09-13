using System;
using UnityEngine;

// Editor-only test double. It deliberately leaves component availability to
// PlayerAbilityController so tests detect missing coordinator guards.
public sealed class PlayerAbilityLifecycleProbe : MonoBehaviour, IPlayerAbilityRuntime
{
    public event Action StateChanged;
    public bool IsActive { get; private set; }
    public bool RejectActivation { get; set; }
    public int ActivationAttempts { get; private set; }
    public int AcceptedActivations { get; private set; }
    public int Cancellations { get; private set; }

    public bool Supports(CharacterAbilityDefinition definition)
    {
        return definition is RoyalDecreeAbilityDefinition;
    }

    public bool CanActivate(CharacterAbilityDefinition definition)
    {
        return Supports(definition) && !IsActive;
    }

    public bool TryActivate(CharacterAbilityDefinition definition)
    {
        ActivationAttempts++;
        if (RejectActivation || !CanActivate(definition)) return false;
        IsActive = true;
        AcceptedActivations++;
        NotifyStateChanged();
        return true;
    }

    public void Cancel()
    {
        if (!IsActive) return;
        IsActive = false;
        Cancellations++;
        NotifyStateChanged();
    }

    public void Complete()
    {
        if (!IsActive) return;
        IsActive = false;
        NotifyStateChanged();
    }

    public void NotifyStateChanged()
    {
        StateChanged?.Invoke();
    }

    private void OnDisable()
    {
        Cancel();
    }
}
