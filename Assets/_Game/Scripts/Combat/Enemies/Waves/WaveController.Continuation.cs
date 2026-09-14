using System;
using System.Collections.Generic;
using UnityEngine;

public sealed partial class WaveController
{
    public void PrepareContinuation() { spawnWaveOnStart=false; }
    public bool CanCaptureContinuation
    {
        get
        {
            if(isSpawningWave || pendingDeathEffects>0 || waitingForDeathEffects) return false;
            foreach(var enemy in activeEnemies)
                if(enemy==null || enemy.IsDefeated || enemy.HasAnimationActionInProgress ||
                    (enemy.GetComponent<EnemyAutoAttack>() is EnemyAutoAttack attack && !attack.CanCaptureContinuation)) return false;
            return true;
        }
    }
    public int ContinuationSlot(EnemyActor enemy)
    {
        if(enemy==null) return -1;
        for(int i=0;i<enemySlots.Length;i++) if(enemySlots[i]?.CurrentEnemy==enemy) return i;
        return -1;
    }
    public EnemyActor ContinuationEnemy(int slot) => slot>=0 && slot<enemySlots.Length ? enemySlots[slot]?.CurrentEnemy : null;
    public int ContinuationOwnerSlot(int id)
    {
        foreach(var enemy in activeEnemies) if(enemy!=null && enemy.GetInstanceID()==id) return ContinuationSlot(enemy);
        return -1;
    }
    public void CaptureContinuation(RunCombatSnapshot saved)
    {
        if(!CanCaptureContinuation) throw new InvalidOperationException("Encounter has an action in flight.");
        saved.wave=currentWave; saved.waveActive=IsWaveActive; saved.plan=CurrentPlan?.SourceRuleName;
        saved.encounterRandom=((SavedRandom)EncounterRandom).State; saved.encounterSeed=encounterSeed;
        saved.previousRecipe=previousRecipeId;
        foreach(var data in seenMilestoneLeaders) saved.seenEnemies.Add(data.name);
        foreach(var data in originalEncounterDefinitions) saved.originalEnemies.Add(data.name);
        foreach(var data in previousEncounterLeaders) saved.previousLeaders.Add(data.name);
        // Preserve authoritative roster order as well as slot identity.
        foreach(var enemy in activeEnemies)
        {
            var value=enemy.CaptureContinuation(ContinuationSlot(enemy));
            enemy.GetComponent<EnemyAutoAttack>()?.CaptureContinuation(value);
            enemy.GetComponent<EnemyStagger>()?.CaptureContinuation(value);
            enemy.GetComponent<EnemyPoisonStatus>()?.CaptureContinuation(value);
            foreach(var owner in enemy.GetComponents<IEnemyContinuationOwner>()) owner.CaptureContinuation(value,ContinuationSlot);
            saved.enemies.Add(value);
        }
    }
    private EnemyDefinition ContinuationDefinition(string id)
    {
        foreach(var data in enemyDatabase.Enemies) if(data!=null && data.name==id) return data;
        throw new InvalidOperationException("Saved enemy definition is unavailable: "+id);
    }
    public void RestoreContinuationActors(RunCombatSnapshot saved)
    {
        if(activeEnemies.Count!=0) throw new InvalidOperationException("Cannot restore over an active encounter.");
        currentWave=saved.wave; IsWaveActive=saved.waveActive; encounterSeed=saved.encounterSeed;
        encounterRandom=new SavedRandom(saved.encounterRandom); previousRecipeId=saved.previousRecipe;
        seenMilestoneLeaders.Clear(); originalEncounterDefinitions.Clear(); previousEncounterLeaders.Clear();
        foreach(string id in saved.seenEnemies) seenMilestoneLeaders.Add(ContinuationDefinition(id));
        var categories=new List<EnemyCategory>();
        foreach(string id in saved.originalEnemies) { var data=ContinuationDefinition(id); originalEncounterDefinitions.Add(data); categories.Add(data.Category); }
        foreach(string id in saved.previousLeaders) previousEncounterLeaders.Add(ContinuationDefinition(id));
        CurrentPlan=new WaveSpawnPlan(currentWave,saved.plan,categories);
        foreach(var value in saved.enemies)
        {
            var enemy=CreateEnemy(ContinuationDefinition(value.definition),enemySlots[value.slot],value.weakness);
            if(enemy==null) throw new InvalidOperationException("Could not restore enemy slot "+value.slot);
            activeEnemies.Add(enemy); EnemySpawned?.Invoke(enemy);
        }
    }
    public void RestoreContinuationState(RunCombatSnapshot saved)
    {
        foreach(var value in saved.enemies)
        {
            var enemy=ContinuationEnemy(value.slot);
            enemy.RestoreContinuation(value);
            enemy.GetComponent<EnemyAutoAttack>()?.RestoreContinuation(value);
            enemy.GetComponent<EnemyStagger>()?.RestoreContinuation(value);
            var poison=enemy.GetComponent<EnemyPoisonStatus>();
            if(value.poisoned && poison==null) poison=enemy.gameObject.AddComponent<EnemyPoisonStatus>();
            poison?.RestoreContinuation(value);
        }
        foreach(var value in saved.enemies)
            foreach(var owner in ContinuationEnemy(value.slot).GetComponents<IEnemyContinuationOwner>())
                owner.RestoreContinuation(value,ContinuationEnemy);
        foreach(var cell in saved.board.cells) if(cell.banner)
            RoyalBannerAuraRuntime.Install(boardController,activeEnemies,cell.bannerId,1.2f);
    }
    public void ResumeContinuationProgression()
    {
        if(!IsWaveActive && advanceWaveCoroutine==null && advanceWavesAutomatically)
            advanceWaveCoroutine=StartCoroutine(AdvanceToNextWaveWhenReady(currentWave));
    }
}
