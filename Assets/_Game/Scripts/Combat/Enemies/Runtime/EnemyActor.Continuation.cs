using UnityEngine;

public sealed partial class EnemyActor
{
    public void ResumeContinuationReadiness()
    {
        if(isSpecialReady && !IsDefeated) SpecialBecameReady?.Invoke(this);
    }
    public EnemyCombatSnapshot CaptureContinuation(int slot) => new EnemyCombatSnapshot
    {
        definition=definition.name, slot=slot, weakness=assignedGemType,
        health=currentHealth, shield=currentShield, specialTurns=currentSpecialTurnCount,
        specialRequirement=specialTurnRequirementOverride
    };
    public void RestoreContinuation(EnemyCombatSnapshot saved)
    {
        currentHealth=Mathf.Clamp(saved.health,1,MaxHealth);
        currentShield=Mathf.Clamp(saved.shield,0,MaximumShield);
        currentSpecialTurnCount=Mathf.Max(0,saved.specialTurns);
        specialTurnRequirementOverride=saved.specialRequirement;
        isSpecialReady=HasSpecialAbility && currentSpecialTurnCount>=SpecialTurnRequirement;
        HealthChanged?.Invoke(this,currentHealth,MaxHealth);
        ShieldChanged?.Invoke(this,currentShield,MaximumShield);
        SpecialCounterChanged?.Invoke(this,currentSpecialTurnCount,SpecialTurnRequirement);
    }
}
