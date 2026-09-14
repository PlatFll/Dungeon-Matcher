using UnityEngine;

public sealed partial class RunSession
{
    public void CaptureContinuation(RunCombatSnapshot saved)
    {
        var journal=account.ActiveRun;
        saved.completedWaves=journal.completedWaves; saved.milestones=journal.milestoneCount;
        saved.kingCleared=journal.kingCleared; saved.potionCharges=journal.potionCharges; saved.bombCharges=journal.bombCharges;
        saved.draftWave=journal.draftWave; saved.draft=new System.Collections.Generic.List<string>(journal.draft);
        saved.refinementUsed=journal.refinementUsed;
        saved.potionCooldown=cooldowns[0]; saved.bombCooldown=cooldowns[1];
    }
    public void RestoreContinuation(RunCombatSnapshot saved)
    {
        cooldowns[0]=saved.potionCooldown; cooldowns[1]=saved.bombCooldown;
        hasMilestone=hasKing=false;
        foreach(var data in Waves.OriginalEncounterDefinitions)
        { hasMilestone|=data.Category==EnemyCategory.Miniboss; hasKing|=data.Category==EnemyCategory.Boss; }
        waveStartedAt=Time.unscaledTime; movesAtStart=Board.CompletedValidPlayerMoves;
    }
    public bool SuspendToMenu()
    {
        if(IsFinished || transitioning || Continuation==null || !Continuation.SaveNow()) return false;
        transitioning=true; CancelTargeting();
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
        return true;
    }
}
