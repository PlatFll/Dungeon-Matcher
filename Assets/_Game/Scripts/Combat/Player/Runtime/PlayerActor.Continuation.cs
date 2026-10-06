using UnityEngine;

public sealed partial class PlayerActor
{
    public PlayerCombatSnapshot CaptureContinuation() => new PlayerCombatSnapshot
    {
        statuses=Statuses.Capture(),
        health=currentHealth, maximumHealth=maximumHealth, shield=currentShield,
        maximumShield=maximumShield, revivalCount=revivalCount, lastDamage=LastDamageSummary,
        energy=GetComponent<PlayerAbilityEnergy>()?.CurrentEnergy ?? 0
    };

    public void RestoreContinuation(PlayerCombatSnapshot saved)
    {
        maximumHealth=CombatAmounts.Health(saved.maximumHealth);
        maximumShield=CombatAmounts.Round(saved.maximumShield);
        currentHealth=Mathf.Min(CombatAmounts.Health(saved.health),maximumHealth);
        currentShield=Mathf.Min(CombatAmounts.Round(saved.shield),maximumShield);
        revivalCount=Mathf.Max(0,saved.revivalCount);
        LastDamageSummary=saved.lastDamage ?? "";
        isDefeated=false;
        var energy=GetComponent<PlayerAbilityEnergy>();
        if(energy!=null) { energy.ResetEnergy(); energy.AddEnergy(saved.energy); }
        MaximumHealthChanged?.Invoke(this,currentHealth,maximumHealth);
        HealthChanged?.Invoke(this,currentHealth,maximumHealth);
        ShieldChanged?.Invoke(this,currentShield,maximumShield);
    }
}
