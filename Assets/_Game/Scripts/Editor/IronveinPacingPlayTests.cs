using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public sealed partial class ForestFoundationPlayTests
{
    [UnityTest] public IEnumerator IronveinSeed3101UsesProductionCombatAndRecordsPacing()
    {yield return MineMeasuredPlay(3101,1);}
    [UnityTest] public IEnumerator IronveinSeed9121UsesProductionCombatAndRecordsPacing()
    {yield return MineMeasuredPlay(9121,5);}
    [UnityTest] public IEnumerator IronveinSeed10101UsesProductionCombatAndRecordsPacing()
    {yield return MineMeasuredPlay(10101,5);}

    private IEnumerator MineMeasuredPlay(int seed,int level)
    {
        // Separate disposable account; no production health, damage, cadence,
        // enemy or board overrides. Synthetic greedy input is not human evidence.
        profile.Dispose();var save=new AccountSave{potions=3,bombs=3,equipPotions=true,equipBombs=true};
        save.characters.Add(new CharacterProgress{id="skeleton",level=level});File.WriteAllText(path,JsonUtility.ToJson(save));
        profile=AccountProgression.UseDisposableProfile(path);
        UnityEngine.Random.InitState(seed);RunLaunchOptions.ForestPrototype=false;RunLaunchOptions.StartingZone="ironvein-excavation";
        SceneManager.LoadScene("Game");yield return Stable();
        Set(Run.Waves,"encounterRandom",new SavedRandom(seed));
        const BindingFlags staticPrivate=BindingFlags.NonPublic|BindingFlags.Static;
        var helper=typeof(BalancePacingValidation);var helperRun=helper.GetField("run",staticPrivate);
        object previous=helperRun.GetValue(null);helperRun.SetValue(null,Run);
        var move=helper.GetMethod("Move",staticPrivate);
        float started=Time.time,next=Time.time+2.5f,waveStart=Time.time,watchdog=Time.realtimeSinceStartup;
        int startMoves=0,drills=0,helpful=0,firstObsidian=-1,peakStones=0,samples=0,stoneSum=0,totalMoves=0;
        var seconds=new List<float>();var actions=new List<int>();var rows=new List<string>();
        string folder=".utmp/Ironvein/Pacing";Directory.CreateDirectory(folder);
        Run.Board.MineDrillFired+=(id,row,lane)=>
        {drills++;if(Run.Board.MineStoneTargets().Any(t=>row?t.Cell.y==lane:t.Cell.x==lane))helpful++;};
        Run.Waves.WaveStarted+=_=>{waveStart=Time.time;startMoves=Run.Board.CompletedValidPlayerMoves;};
        Run.Waves.WaveCompleted+=wave=>
        {
            float elapsed=Time.time-waveStart;int accepted=Run.Board.CompletedValidPlayerMoves-startMoves;
            seconds.Add(elapsed);actions.Add(accepted);
            rows.Add(wave+","+string.Join("+",Run.Waves.OriginalEncounterDefinitions.Select(e=>e.EnemyId))+","+
                elapsed.ToString("F3",CultureInfo.InvariantCulture)+","+accepted);
        };
        try
        {
            Time.timeScale=6;
            while(!Run.IsFinished && Run.Zone.Definition.maturesStone && Time.time-started<900)
            {
                totalMoves=Run.Board.CompletedValidPlayerMoves;
                Assert.That(Time.realtimeSinceStartup-watchdog,Is.LessThan(90),"seeded input remains responsive");
                var draft=Run.Waves.GetComponent<RunUpgradeCoordinator>();
                if(draft.IsBlockingWaveProgression && Run.Continuation.CanCapture)
                {
                    var choices=Run.Continuation.Capture().draft;
                    if(choices.Count>0){Assert.That(draft.SelectRecordedCard(choices[0]),Is.True);Time.timeScale=6;watchdog=Time.realtimeSinceStartup;}
                }
                if(Time.time>=next && !Run.Board.IsBusy && !Run.Board.IsExternalInputBlocked && Run.Waves.IsWaveActive && !Run.MoveClock.IsResolving)
                {
                    int count=Run.Board.MineStoneCount;peakStones=Math.Max(peakStones,count);stoneSum+=count;samples++;
                    if(firstObsidian<0 && Run.Board.MineStoneTargets().Any(s=>s.State.stage==MineStoneStage.Obsidian))firstObsidian=Run.Waves.CurrentWave;
                    var ability=UnityEngine.Object.FindFirstObjectByType<PlayerAbilityController>();
                    bool acted=Run.Player.CurrentHealth<Run.Player.MaximumHealth*.45f && Run.TryUsePotion();
                    if(!acted && ability.CanActivate)acted=ability.TryActivate();
                    if(!acted)acted=(bool)move.Invoke(null,new object[]{Run.Board,true});
                    if(acted){next=Time.time+2.5f;watchdog=Time.realtimeSinceStartup;}
                }
                yield return null;
            }
            Assert.That(totalMoves,Is.GreaterThan(0));
            Assert.That(peakStones,Is.LessThanOrEqualTo(6));
        }
        finally
        {
            Time.timeScale=1;helperRun.SetValue(null,previous);
            string N(float n)=>n.ToString("F3",CultureInfo.InvariantCulture);
            float Median(IEnumerable<float> values){var a=values.OrderBy(n=>n).ToArray();return a.Length==0?0:a.Length%2==1?a[a.Length/2]:(a[a.Length/2-1]+a[a.Length/2])*.5f;}
            File.WriteAllLines(folder+"/seed-"+seed+"-waves.csv",new[]{"wave,formation,game_seconds,accepted_moves"}.Concat(rows));
            File.WriteAllText(folder+"/seed-"+seed+".txt",
                "Actual Unity scene, 6x simulation speed; greedy match/ability, potion under 45%, first offered card, 2.5 game-second think interval.\n"+
                "No stat/cadence overrides. Disposable Skeleton level="+level+" seed="+seed+"; not human pacing.\n"+
                "Outcome="+(Run.IsFinished?"defeated":!Run.Zone.Definition.maturesStone?"travelled":"900-second observation censored")+
                "; completed encounters="+seconds.Count+"; game seconds="+N(Time.time-started)+"; accepted moves="+totalMoves+"\n"+
                "Completed-fight seconds min/median/max="+(seconds.Count==0?"unavailable":N(seconds.Min())+"/"+N(Median(seconds))+"/"+N(seconds.Max()))+"\n"+
                "Completed-fight moves min/median/max="+(actions.Count==0?"unavailable":actions.Min()+"/"+N(Median(actions.Select(a=>(float)a)))+"/"+actions.Max())+"\n"+
                "Board stone occupancy mean="+N(samples==0?0:(float)stoneSum/samples/64)+"; peak stones="+peakStones+
                "; drills="+drills+"; drills crossing a stone="+helpful+"; first Obsidian wave="+(firstObsidian<0?"not observed":firstObsidian.ToString())+"\n");
        }
    }
}
