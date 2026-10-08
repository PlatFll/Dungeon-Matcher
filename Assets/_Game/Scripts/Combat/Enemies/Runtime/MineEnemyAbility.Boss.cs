using System;
using UnityEngine;

public enum MineBossPhase { FirstMech, PilotFoot, SecondMech }

public sealed partial class MineEnemyAbility
{
    private EnemyRuntimeStats firstSuitStats;
    private bool phaseDamageGuard;
    private int phaseFrame;
    public MineBossPhase BossPhase => state.bossPhase;
    public bool IsPilot => Kind == EnemySpecialAbilityKind.GrandDelver && state.bossPhase == MineBossPhase.PilotFoot;
    public int PilotMoves => IsPilot ? Mathf.Max(0,state.pilotDueMove-Move) : 0;
    public event Action<MineBossPhase,bool> BossPhaseChanged;
    private string BossCastName => Kind != EnemySpecialAbilityKind.GrandDelver ? null :
        state.action == "CLAIM" ? "Claim the Vein" : state.action == "STEAM" ? "Full Steam!" : "Heart of Obsidian";

    public static bool Supports(MineEnemySnapshot value) => value == null ||
        (value.version >= 0 && value.version <= 2 && Enum.IsDefined(typeof(MineBossPhase),value.bossPhase) &&
         value.pilotDueMove >= 0 && (value.bossPhase != MineBossPhase.SecondMech || value.remountUsed));

    private void InitializeBoss()
    {
        if (Kind != EnemySpecialAbilityKind.GrandDelver) return;
        firstSuitStats = actor.RuntimeStats;
        state.remountEnabled = actor.Definition.mineEnableRemount;
        actor.TryAdvanceLethalPhase = EjectPilot;
        actor.PhaseAcceptsDamage = PhaseCanTakeDamage;
    }
    private bool PlanBoss()
    {
        state.action = state.cycle % 3 == 0 ? "CLAIM" : state.cycle % 3 == 1 ? "STEAM" : "CORE";
        if (state.action != "STEAM" && board.MineStoneCount >= RunSession.Current.Zone.Definition.maximumMineStones)
            state.action = "STEAM"; // Never stall forever on a full board.
        return true;
    }
    private void LostTarget()
    {
        bool core = state.action == "CORE";
        state.cycle++; Finish();
        if (core) stagger?.ApplyStagger(2,2);
    }
    private bool EjectPilot()
    {
        if (disposed || !state.remountEnabled || state.remountUsed || state.bossPhase != MineBossPhase.FirstMech) return false;
        StopAllCoroutines(); Finish();
        state.bossPhase = MineBossPhase.PilotFoot;
        state.pilotDueMove = Math.Max(Move,CombatMoveClock.EffectAction) + Mathf.Max(1,actor.Definition.minePilotMoves);
        phaseDamageGuard = true; phaseFrame = Time.frameCount;
        ApplyBossPhase(true);
        return true;
    }
    private bool PhaseCanTakeDamage()
    {
        if (!phaseDamageGuard) return true;
        // One lethal hit cannot leak overkill/cascade callbacks into the ejection.
        // The pilot becomes targetable at the next settled input boundary.
        if (Time.frameCount <= phaseFrame || board.IsBusy || CombatMoveClock.Current?.IsBlockingWaveProgression == true) return false;
        phaseDamageGuard = false;
        return true;
    }
    private void AdvancePilot()
    {
        if (disposed || actor.IsDefeated || !state.remountEnabled || state.remountUsed || Move < state.pilotDueMove) return;
        state.remountUsed = true; state.bossPhase = MineBossPhase.SecondMech;
        state.pilotDueMove = 0; state.cycle = 0;
        ApplyBossPhase(true);
    }
    private EnemyRuntimeStats BossStats()
    {
        if (state.bossPhase == MineBossPhase.FirstMech) return firstSuitStats;
        bool foot = IsPilot;
        float health = foot ? actor.Definition.minePilotHealthFraction : actor.Definition.mineReserveHealthFraction;
        return new EnemyRuntimeStats(firstSuitStats.Wave,firstSuitStats.Level,
            CombatAmounts.Health(firstSuitStats.MaxHealth*health),
            CombatAmounts.Round(firstSuitStats.Damage*(foot?.5f:.8f)),0,
            firstSuitStats.AttackInterval*(foot?.7f:1f),firstSuitStats.SpecialTurnRequirement,firstSuitStats.DamageMultiplier);
    }
    private void ApplyBossPhase(bool restartAttack, int savedHealth = -1)
    {
        bool wasRunning = attack?.IsRunning == true;
        if (restartAttack) attack?.StopAttacking();
        var stats = BossStats(); actor.ApplyPhaseStats(stats,savedHealth >= 0 ? savedHealth : stats.MaxHealth);
        if (restartAttack)
        {
            actor.GetComponent<EnemyOrePower>()?.Discard();
            actor.ResetSpecialCounter();
            if (wasRunning) attack?.TryStartAttacking();
        }
        BossPhaseChanged?.Invoke(state.bossPhase,restartAttack);
    }
    private void RestoreBoss(int health)
    {
        if (Kind != EnemySpecialAbilityKind.GrandDelver) return;
        phaseDamageGuard = false;
        ApplyBossPhase(false,health);
    }
}
