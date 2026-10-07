using UnityEngine;

public sealed partial class EnemyActor
{
    public void ResumeContinuationReadiness()
    {
        if(isSpecialReady && !IsDefeated) SpecialBecameReady?.Invoke(this);
    }
    public EnemyCombatSnapshot CaptureContinuation(int slot) => new EnemyCombatSnapshot
    {
        definition=definition.name, slot=slot, weakness=assignedGemType, persistentId=PersistentId,
        health=currentHealth, shield=currentShield, specialTurns=currentSpecialTurnCount,
        specialRequirement=specialTurnRequirementOverride,fortifiedStacks=FortifiedStacks
    };
    public void RestoreContinuation(EnemyCombatSnapshot saved)
    {
        if (saved.persistentId > 0) PersistentId=saved.persistentId;
        currentHealth=Mathf.Min(CombatAmounts.Health(saved.health),MaxHealth);
        currentShield=Mathf.Min(CombatAmounts.Round(saved.shield),MaximumShield);
        fortifiedStacks=Mathf.Clamp(saved.fortifiedStacks,0,2);
        if(fortifiedStacks>0 && GetComponent<EnemyFortifiedView>()==null)gameObject.AddComponent<EnemyFortifiedView>();
        currentSpecialTurnCount=Mathf.Max(0,saved.specialTurns);
        specialTurnRequirementOverride=saved.specialRequirement;
        isSpecialReady=HasSpecialAbility && currentSpecialTurnCount>=SpecialTurnRequirement;
        HealthChanged?.Invoke(this,currentHealth,MaxHealth);
        ShieldChanged?.Invoke(this,currentShield,MaximumShield);
        SpecialCounterChanged?.Invoke(this,currentSpecialTurnCount,SpecialTurnRequirement);
    }
}
