using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public sealed partial class ForestFoundationPlayTests
{
    [UnityTest] public IEnumerator UnifiedPacingDungeon() => UnifiedMeasuredPlay("dungeon",7301);
    [UnityTest] public IEnumerator UnifiedPacingForest() => UnifiedMeasuredPlay("magical-forest",7302);
    [UnityTest] public IEnumerator UnifiedPacingCourt() => UnifiedMeasuredPlay("drowned-court",7303);
    [UnityTest] public IEnumerator UnifiedPacingMine() => UnifiedMeasuredPlay("ironvein-excavation",7304);
    private IEnumerator UnifiedMeasuredPlay(string zone,int seed)
    {
        UnityEngine.Random.InitState(seed);yield return LaunchUnified(zone);
        Set(Run.Waves,"encounterRandom",new SavedRandom(seed));
        const BindingFlags binding=BindingFlags.Static|BindingFlags.NonPublic;
        var helper=typeof(BalancePacingValidation);var runField=helper.GetField("run",binding);object previous=runField.GetValue(null);
        runField.SetValue(null,Run);var move=helper.GetMethod("Move",binding);
        float started=Time.time,wallStart=Time.realtimeSinceStartup,waveStart=Time.time,waveWall=Time.realtimeSinceStartup;
        float next=Time.time+2.5f,watchdog=Time.realtimeSinceStartup;int waveMove=0,completed=0,accepted=0;
        // A lethal cascade can finish the wave before the clock's settle event.
        Run.Board.ValidPlayerMoveAccepted+=_=>accepted++;
        var rows=new List<string>{"wave,formation,accepted_moves,game_seconds,wall_seconds"};
        Run.Waves.WaveStarted+=_=>{waveStart=Time.time;waveWall=Time.realtimeSinceStartup;};
        Run.Waves.WaveCompleted+=wave=>
        {
            completed++;rows.Add(FormattableString.Invariant($"{wave},{string.Join("+",Run.Waves.OriginalEncounterDefinitions.Select(d=>d.EnemyId))},{accepted-waveMove},{Time.time-waveStart:0.###},{Time.realtimeSinceStartup-waveWall:0.###}"));
            waveMove=accepted;
        };
        try
        {
            Time.timeScale=6;
            while(!Run.IsFinished && completed<6 && Run.MoveClock.Tick<80 && Time.time-started<900)
            {
                Assert.That(Time.realtimeSinceStartup-watchdog,Is.LessThan(100),"pacing input remains responsive: "+zone);
                var draft=Run.Waves.GetComponent<RunUpgradeCoordinator>();
                if(draft.IsBlockingWaveProgression && Run.Continuation.CanCapture)
                {
                    var choices=Run.Continuation.Capture().draft;
                    if(choices.Count>0){Assert.That(draft.SelectRecordedCard(choices[0]),Is.True);Time.timeScale=6;watchdog=Time.realtimeSinceStartup;}
                }
                if(Time.time>=next && !Run.Board.IsBusy && !Run.Board.IsExternalInputBlocked && Run.Waves.IsWaveActive && !Run.MoveClock.IsResolving)
                {
                    var ability=Run.Player.GetComponent<PlayerAbilityController>();
                    bool acted=Run.Player.CurrentHealth<Run.Player.MaximumHealth*.45f && Run.TryUsePotion();
                    if(!acted && ability.CanActivate)acted=ability.TryActivate();
                    if(!acted)acted=(bool)move.Invoke(null,new object[]{Run.Board,true});
                    if(acted){next=Time.time+2.5f;watchdog=Time.realtimeSinceStartup;}
                }
                yield return null;
            }
            Assert.That(Run.MoveClock.Tick,Is.GreaterThan(0));
        }
        finally
        {
            Time.timeScale=1;runField.SetValue(null,previous);Directory.CreateDirectory(".utmp/UnifiedCombat/Pacing");
            File.WriteAllLines(".utmp/UnifiedCombat/Pacing/"+zone+".csv",rows);
            File.WriteAllText(".utmp/UnifiedCombat/Pacing/"+zone+".txt",
                "Actual Unity production combat, disposable level-1 Skeleton, seed="+seed+". No HP/damage/cadence overrides.\n"+
                "Automated greedy swaps, free ability when ready, potion under 45%, first offered card, 2.5 game-second think interval, 6x simulation. This is NOT human pacing.\n"+
                FormattableString.Invariant($"Outcome={(Run.IsFinished?"defeated":"observation limit")}; completed={completed}; moves={accepted}; completed_clock_moves={Run.MoveClock.Tick}; game_seconds={Time.time-started:0.###}; wall_seconds={Time.realtimeSinceStartup-wallStart:0.###}; hp={Run.Player.CurrentHealth}\n"));
        }
    }
}
