using System;
using System.Linq;
using UnityEngine.SceneManagement;

// Explicit diagnostic formations only. Never used by the human-like pacing runner.
// Existing kit tests choose their actors; they do not test random encounter selection.
internal static class ResonanceEncounterFixture
{
    public static void Lock(RunSession run, EnemyDefinition[] members, string label)
    {
        var state=run.Resonance.State;
        state.zoneId=run.Zone.Definition.zoneId;state.visit=run.Travel.State.visit;
        state.active=false;state.pendingWave=0;
        state.capstoneId=label;state.capstoneKind=(int)ZoneEncounterKind.Elite;
        state.capstoneMembers=members.Select(e=>e.EnemyId).ToList();state.formationSeed=91;
        state.capstoneWave=run.Waves.CurrentWave;
        state.capstoneEncounterId=run.RunId+":"+state.visit+":"+state.capstoneWave;
        state.capstonePhase=ResonanceCapstonePhase.Scheduled;
    }
    public static IDisposable OnNextGame(EnemyDefinition[] members,string label)
        => new BeforeStart(members,label);
    private sealed class BeforeStart : IDisposable
    {
        private readonly EnemyDefinition[] members;private readonly string label;
        public BeforeStart(EnemyDefinition[] members,string label)
        {this.members=members;this.label=label;SceneManager.sceneLoaded+=Loaded;}
        private void Loaded(Scene scene,LoadSceneMode mode)
        {if(scene.name!="Game")return;Dispose();Lock(RunSession.Current,members,label);}
        public void Dispose()=>SceneManager.sceneLoaded-=Loaded;
    }
    public static void EarnedExit(RunSession run)
    {
        // Isolate atomic travel/cleanup, not pacing or the earning of charge.
        var recipe=run.Zone.Definition.resonanceEncounters.First(r=>r.resonanceKind!=ZoneEncounterKind.Ordinary);
        Lock(run,recipe.members,"Diagnostic earned exit: "+recipe.label);
        var state=run.Resonance.State;
        state.charge=100;state.triggeredMask=state.claimedMask=7;
        state.activeNotch=0;state.pendingNotches.Clear();state.capstonePhase=ResonanceCapstonePhase.Defeated;
    }
}
