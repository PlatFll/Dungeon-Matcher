using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public sealed partial class MineEnemyAbility
{
    private IEnemySummonService summons;
    public void ConfigureSummonService(IEnemySummonService service) => summons = service;
    private EnemyActor LivingMineActor(long id) => roster.FirstOrDefault(e => e != null && !e.IsDefeated && e.PersistentId == id);
    public IEnumerable<EnemyActor> OwnedTurrets => state.turrets.Select(LivingMineActor).Where(e => e != null);
    public bool HasObsidianSlam => state.obsidianSlam;
    private string MilestoneName => state.action == "ASSEMBLE" ? "Assemble Turret" : state.action == "PRIME" ? "Prime the Turrets" :
        state.action == "DEVOUR" ? "Devour Ore" : state.action == "SLAM" ? "Hydraulic Slam" : null;

    private bool PlanMilestone()
    {
        if (Kind == EnemySpecialAbilityKind.SiegeMachinist)
        {
            state.turrets.RemoveAll(id => LivingMineActor(id) == null);
            bool build = summons?.HasFreeEnemySlot == true && actor.Definition.mineTurret != null &&
                state.turrets.Count < Mathf.Clamp(actor.Definition.maximumMineTurrets,1,2);
            if (build && (state.cycle % 2 == 0 || state.turrets.Count == 0)) state.action = "ASSEMBLE";
            else if (state.turrets.Count > 0) state.action = "PRIME";
            else if (build) state.action = "ASSEMBLE";
            else return false;
            return true;
        }
        var stones = board.MineStoneTargets().Where(t=>!t.State.isCore).ToArray();
        if (state.cycle % 2 == 0 && stones.Length > 0)
        { state.action = "DEVOUR"; state.stoneId = stones[0].State.id; }
        else state.action = "SLAM";
        return true;
    }
    private bool CommitMachinist()
    {
        if (state.action == "ASSEMBLE")
        {
            if (OwnedTurrets.Count() >= Mathf.Clamp(actor.Definition.maximumMineTurrets,1,2) ||
                summons == null || !summons.TrySummonEnemy(actor.Definition.mineTurret,out var turret)) return false;
            state.turrets.Add(turret.PersistentId); return true;
        }
        var targets = OwnedTurrets.ToArray(); if (targets.Length == 0) return false;
        foreach (var turret in targets) turret.GetComponent<EnemyOrePower>()?.Grant();
        board.TryQueueMineDrillPower(1+(state.cycle/2)%2,1); return true;
    }
    private void ApplyExtractedPower(MineStoneStage stage)
    {
        var values=actor.Definition.mineExtractionMultipliers;
        float multiplier=stage==MineStoneStage.Brittle?values.x:stage==MineStoneStage.Hardened?values.y:values.z;
        actor.GetComponent<EnemyOrePower>()?.Grant(Mathf.Max(1,multiplier));
        if (stage != MineStoneStage.Brittle) actor.GrantFortified(1,2);
        if (stage == MineStoneStage.Obsidian) state.obsidianSlam = true;
    }
    private void DrillFired(int id, bool horizontal, int lane)
    {
        if (disposed || Kind != EnemySpecialAbilityKind.ObsidianSentinel || !IsPreparing || state.action != "SLAM") return;
        StopAllCoroutines(); state.obsidianSlam = false; state.cycle++; Finish();
        stagger?.ApplyStagger(2,2); // Existing immunity/duration and presentation rules remain authoritative.
    }
    public void ResolveAfterMineDrills()
    {
        if (IsPilot) { AdvancePilot(); return; }
        if (!disposed && !pending && IsPreparing && TargetGone) { LostTarget(); return; }
        if (disposed || pending || !IsPreparing || (state.action != "SLAM" && state.action != "CORE") || Move < state.dueMove ||
            actor.IsDefeated || stagger?.IsStaggered == true || board.IsBusy || Time.timeScale <= 0 ||
            !CombatMoveClock.CanOffer(actor) || !actor.TryBeginSpecialAbilityAnimationAction()) return;
        pending=true;StartCoroutine(Release());
    }
}
