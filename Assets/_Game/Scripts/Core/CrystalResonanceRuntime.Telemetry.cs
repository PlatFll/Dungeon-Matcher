using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class ResonanceTelemetryEvent
{
    public string kind, detail, runId, zone, encounter, capstone;
    public int visit, depth, moves, charge, pending, value, health, shield, capstonePhase, approach;
    public long score, receipt, sourceActor;
    public string sourceEnemy;
    public int shieldLoss, air;
    public float wall, visitSeconds, runSeconds, combatSeconds, draftSeconds, travelSeconds;
    public uint refillRandom, capstoneRandom;
    public int encounterSeed;
}
public sealed partial class CrystalResonanceRuntime
{
    // Opt-in local observers only. No adaptive balance and no production file/network writer.
    public event Action<ResonanceTelemetryEvent> Telemetry;
    private readonly List<EnemyActor> telemetryEnemies = new List<EnemyActor>();
    public void Record(string kind, string detail = "", int value = 0, EnemyActor source = null, int shieldLoss = 0)
    {
        if (!Enabled || Telemetry == null) return;
        Telemetry.Invoke(new ResonanceTelemetryEvent { kind=kind, detail=detail, value=value,
            sourceActor=source?.PersistentId ?? 0, sourceEnemy=source?.Definition?.EnemyId,
            shieldLoss=shieldLoss, air=run.Board.Aquatic?.air ?? 0,
            runId=run.RunId,zone=State.zoneId,visit=State.visit,depth=run.Waves.CurrentWave,
            encounter=State.encounterId,capstone=State.capstoneId,capstonePhase=(int)State.capstonePhase,
            approach=State.approachThreshold,charge=State.charge,pending=State.PendingMastery,
            score=State.score,receipt=State.lastReceipt,moves=run.Board.CompletedValidPlayerMoves,
            health=run.Player.CurrentHealth,shield=run.Player.CurrentShield,
            wall=Time.realtimeSinceStartup,visitSeconds=State.visitSeconds,runSeconds=State.runSeconds,
            combatSeconds=State.combatSeconds,draftSeconds=State.draftSeconds,travelSeconds=State.travelSeconds,
            refillRandom=run.Board.RefillRandomState,capstoneRandom=State.capstoneRandom,encounterSeed=run.Waves.EncounterSeed });
    }
    private void TickTelemetryTime()
    {
        if (!Enabled || run.IsFinished || run.Continuation?.IsRestoring == true || !run.InitialStateReady) return;
        float dt=Time.unscaledDeltaTime;
        bool travel=run.Travel.State.stage!=0;
        bool modal=run.GetComponent<RunControlsUI>()?.IsModalOpen==true;
        if (modal || (Time.timeScale<=0 && !travel)) { State.pausedSeconds+=dt; return; }
        State.runSeconds+=dt; State.visitSeconds+=dt;
        if (travel) State.travelSeconds+=dt;
        else if (State.activeNotch!=0) State.draftSeconds+=dt;
        else if (State.active) State.combatSeconds+=dt;
    }
    private void TelemetryMove(int move) { if (Enabled) { State.movesThisVisit++; Record("move_accepted", "", move); } }
    private void TelemetryDamage(EnemyActor source, string reason, int health, int shield)
    {
        if (!Enabled) return;
        State.damageThisVisit += health;
        Record("player_damage", reason, health, source, shield);
    }
    private void TelemetryAir(int before, int spent, int[] collected)
    {
        Record("air_receipt", "before=" + before + " collected=" + string.Join(",", collected), spent);
    }
    private void TelemetryEnemy(EnemyActor enemy)
    { if (enemy!=null) { telemetryEnemies.RemoveAll(previous=>previous==null); telemetryEnemies.Add(enemy); enemy.SpecialAbilityUsed+=TelemetrySpecial; } }
    private void TelemetrySpecial(EnemyActor enemy) => Record("enemy_special", enemy.Definition.EnemyId);
}
