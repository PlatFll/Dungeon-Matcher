using UnityEngine;

public sealed partial class PlayerActor
{
    public PlayerCombatSnapshot CaptureContinuation() => new PlayerCombatSnapshot
    {
        health=currentHealth, maximumHealth=maximumHealth, shield=currentShield,
        maximumShield=maximumShield, revivalCount=revivalCount, lastDamage=LastDamageSummary,
        energy=GetComponent<PlayerAbilityEnergy>()?.CurrentEnergy ?? 0
    };

    public void RestoreContinuation(PlayerCombatSnapshot saved)
    {
        maximumHealth=Mathf.Max(1,saved.maximumHealth);
        maximumShield=Mathf.Max(0,saved.maximumShield);
        currentHealth=Mathf.Clamp(saved.health,1,maximumHealth);
        currentShield=Mathf.Clamp(saved.shield,0,maximumShield);
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
