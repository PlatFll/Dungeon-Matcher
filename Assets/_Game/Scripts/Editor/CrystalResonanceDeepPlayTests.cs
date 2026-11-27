#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

// Explicit depth/history fixtures. These are NOT naturally played visits or pacing samples.
public sealed partial class ForestFoundationPlayTests
{
    [Serializable] private sealed class ResonanceDeepFormationEvidence
    { public string zone, recipe, members; public int depth, kind, acceptedMoves; }
    [Serializable] private sealed class ResonanceDeepEvidence
    { public bool diagnosticOnly=true;public List<ResonanceDeepFormationEvidence> formations=new List<ResonanceDeepFormationEvidence>(); }

    [UnityTest] public IEnumerator CrystalResonanceDeepDiagnosticFormationsSpawnAndAcceptInputInEveryZone()
    {
        string directory=Path.GetFullPath(".utmp/CrystalResonance/DeepFixtures");Directory.CreateDirectory(directory);
        var evidence=new ResonanceDeepEvidence();
        foreach(string zoneId in new[]{"dungeon","magical-forest","drowned-court","ironvein-excavation","frostwake-peaks"})
        foreach(bool capstone in new[]{false,true})
        {
            ZoneTestEncounter chosen=null;
            void Configure(Scene scene,LoadSceneMode mode)
            {
                if(scene.name!="Game")return;
                SceneManager.sceneLoaded-=Configure;
                Set(Run.Waves,"currentWave",45);
                Set(Run.Waves,"advanceWavesAutomatically",false);
                Run.Travel.State.visit=4;Run.Travel.State.visitStartWave=45;
                var state=Run.Resonance.State;state.visit=4;state.lastBankedWave=44;
                state.seenMechanics.AddRange(Run.Zone.Definition.enemies.Select(e=>e.EnemyId));
                if(!capstone)return;
                chosen=Run.Zone.Definition.resonanceEncounters
                    .Where(r=>r.resonanceKind!=ZoneEncounterKind.Ordinary && WaveController.ResonanceFormationLegal(Run.Zone.Definition,r,4,state,true))
                    .OrderByDescending(r=>r.resonanceKind).ThenBy(r=>r.label,StringComparer.Ordinal).First();
                ResonanceEncounterFixture.Lock(Run,chosen.members,chosen.label);
                state.capstoneKind=(int)chosen.resonanceKind;
                state.charge=100;state.triggeredMask=state.claimedMask=7;
            }
            SceneManager.sceneLoaded+=Configure;
            try {yield return LaunchUnified(zoneId);}
            finally {SceneManager.sceneLoaded-=Configure;}
            Assert.AreEqual(45,Run.Waves.CurrentWave);
            Assert.IsTrue(RunContinuation.SupportsSnapshot(Run.Continuation.Capture()));
            var actual=Run.Waves.OriginalEncounterDefinitions.ToArray();
            Assert.IsTrue(actual.All(e=>Run.Zone.Definition.enemies.Contains(e)),"same-zone actors only");
            if(chosen!=null)CollectionAssert.AreEquivalent(chosen.members,actual,"complete selected capstone including escorts");
            else
            {
                chosen=new ZoneTestEncounter {label=Run.Waves.CurrentPlan.SourceRuleName,members=actual};
                Assert.IsTrue(WaveController.ResonanceFormationLegal(Run.Zone.Definition,chosen,4,Run.Resonance.State,false));
                if(zoneId=="dungeon")Assert.IsFalse(actual.Any(e=>new[]{"farmer","basket_villager","pan_villager","miner","barricade_villager"}.Contains(e.EnemyId)));
            }
            if(capstone){Assert.IsTrue(Run.Resonance.Sealed);Assert.AreEqual(0,Run.Travel.State.stage);}
            var response=Run.Board.GetImmediateResponses().First(o=>Run.Board.IsHintMoveStillValid(o.Source,o.Target));
            int before=Run.Board.CompletedValidPlayerMoves;
            Run.Board.ReplayPlayerSwap(response.Source.Column,response.Source.Row,response.Target.Column,response.Target.Row);
            yield return Until(()=>Run.Board.CompletedValidPlayerMoves==before+1 && Run.Continuation.CanCapture,"deep diagnostic accepted swap settles");
            Assert.AreEqual(0,Run.Travel.State.stage,"one input cannot bypass an unfinished capstone");
            if(zoneId=="drowned-court" && !capstone)
            {
                // Selection-only phase fixtures use the actual Court board context.
                // Restore immediately without yielding or advancing combat in the artificial phase.
                var water=Run.Board.Aquatic;var oldPhase=water.phase;
                var exhausted=Run.Resonance.State.Copy();
                exhausted.seenLeaders.Clear();exhausted.seenLeaders.AddRange(Run.Zone.Definition.enemies.Select(e=>e.EnemyId));
                try
                {
                    foreach(var phase in new[]{TidePhase.Dry,TidePhase.Pending,TidePhase.Flooded})
                    {
                        water.phase=phase;
                        for(int tier=0;tier<5;tier++)for(int seed=1;seed<=10000;seed++)
                        {
                            int depth=Run.Zone.Definition.resonanceTierStarts[tier];
                            var random=new SavedRandom(unchecked((uint)seed*2654435761u));
                            var ordinary=WaveController.SelectResonanceOrdinary(Run.Zone.Definition,depth,exhausted,random);
                            var reserve=WaveController.ChooseResonanceCapstone(Run.Zone.Definition,depth,exhausted,random);
                            Assert.IsTrue(WaveController.ResonanceFormationLegal(Run.Zone.Definition,ordinary,tier,exhausted,false));
                            Assert.IsTrue(WaveController.ResonanceFormationLegal(Run.Zone.Definition,reserve,tier,exhausted,true));
                            Assert.AreEqual(ZoneEncounterKind.Elite,reserve.resonanceKind);
                            if(phase!=TidePhase.Dry)
                            {Assert.IsTrue(Run.Board.FormationCanFlood(ordinary.members));Assert.IsTrue(Run.Board.FormationCanFlood(reserve.members));}
                        }
                    }
                }
                finally {water.phase=oldPhase;}
            }
            evidence.formations.Add(new ResonanceDeepFormationEvidence {zone=zoneId,depth=45,recipe=chosen.label,
                kind=(int)chosen.resonanceKind,members=string.Join("+",actual.Select(e=>e.EnemyId)),acceptedMoves=1});
            ScreenCapture.CaptureScreenshot(Path.Combine(directory,zoneId+(capstone?"-capstone":"-ordinary")+".png"));
            yield return null;yield return null;
        }
        File.WriteAllText(Path.Combine(directory,"formations.json"),JsonUtility.ToJson(evidence,true));
    }
}
#endif
