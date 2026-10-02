using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public sealed class CombatPolishPlayTests
{
    private const BindingFlags Flags=BindingFlags.Instance|BindingFlags.NonPublic;

    [UnityTest] public IEnumerator KingFormationContinuesAndSettlesOnlyOnDeath()
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        yield return new EnterPlayMode();
        string path=Path.GetFullPath(".utmp/CombatPolish/endless-"+Guid.NewGuid().ToString("N")+".json");
        using(AccountProgression.UseDisposableProfile(path))
        using(CharacterSelectionSettings.UseTemporarySelection("skeleton"))
        {
            SceneManager.LoadScene("Game");
            yield return Until(()=>RunSession.Current!=null && RunSession.Current.Waves.IsWaveActive && RunSession.Current.Continuation.CanCapture);
            var run=RunSession.Current; var waves=run.Waves;
            // Fast-forward a disposable fixture to the real, data-selected King formation.
            waves.ClearCurrentWave(); yield return null;
            for(int completed=1;completed<30;completed++)
                Assert.That(AccountProgression.Current.RecordWave(run.RunId,completed,false,false),Is.True);
            var seen=(HashSet<EnemyDefinition>)typeof(WaveController).GetField("seenMilestoneLeaders",Flags).GetValue(waves);
            var database=(EnemyDatabase)typeof(WaveController).GetField("enemyDatabase",Flags).GetValue(waves);
            // Seed only this run's roster. Unreleased zone references must not
            // enter a dungeon save through broad Editor asset discovery.
            foreach(var definition in database.Enemies)
            {
                if(definition.Category!=EnemyCategory.Boss) seen.Add(definition);
            }
            typeof(WaveController).GetField("currentWave",Flags).SetValue(waves,30);
            waves.SpawnCurrentWave(); yield return Until(()=>waves.IsWaveActive && waves.CurrentWave==30 && waves.ActiveEnemies.Count==2 && run.Continuation.CanCapture);
            Assert.That(waves.OriginalEncounterDefinitions.Any(e=>e.Category==EnemyCategory.Boss),Is.True);
            string id=run.RunId; int gold=AccountProgression.Current.Gold;
            foreach(var enemy in waves.ActiveEnemies.ToArray())
            {
                enemy.GetComponent<EnemyAutoAttack>()?.StopAttacking();
                if(enemy.HasShield) enemy.TryTakeDamage(1000000);
                enemy.TryTakeDamage(1000000);
            }
            yield return Until(()=>waves.CurrentWave==31 && waves.IsWaveActive && run.Continuation.CanCapture);
            Assert.That(run.IsFinished,Is.False); Assert.That(run.IsVictory,Is.False);
            Assert.That(AccountProgression.Current.ActiveRun.id,Is.EqualTo(id));
            Assert.That(AccountProgression.Current.ActiveRun.kingCleared,Is.True);
            Assert.That(AccountProgression.Current.Gold,Is.EqualTo(gold),"King milestone is journaled, not paid early");
            foreach(var enemy in waves.ActiveEnemies) enemy.GetComponent<EnemyAutoAttack>()?.StopAttacking();
            Assert.That(run.Continuation.SaveNow(),Is.True);
            Assert.That(run.SuspendToMenu(),Is.True);yield return null;
            SceneManager.LoadScene("Game");
            yield return Until(()=>RunSession.Current!=null && RunSession.Current.Waves.CurrentWave==31 && RunSession.Current.Waves.IsWaveActive && !RunSession.Current.Continuation.IsRestoring);
            run=RunSession.Current;run.GetComponent<RunControlsUI>().Close();
            Assert.That(run.RunId,Is.EqualTo(id)); Assert.That(AccountProgression.Current.ActiveRun.kingCleared,Is.True);
            if(run.Player.HasShield)run.Player.TryTakeDamage(1000000);
            run.Player.TryTakeDamage(1000000);
            yield return Until(()=>run.IsFinished && run.RewardFinalized);
            Assert.That(run.IsVictory,Is.False);Assert.That(AccountProgression.Current.HasClearedKing,Is.True);
            int paid=AccountProgression.Current.Gold; Assert.That(paid,Is.GreaterThan(gold));
            Assert.That(run.RetrySave(),Is.True);Assert.That(AccountProgression.Current.Gold,Is.EqualTo(paid));
            SceneManager.LoadScene("MainMenu");yield return null;
        }
        Time.timeScale=1;yield return new ExitPlayMode();
    }

    private static IEnumerator Until(Func<bool> ready)
    {
        float deadline=Time.realtimeSinceStartup+25;
        while(!ready()){Assert.That(Time.realtimeSinceStartup,Is.LessThan(deadline),"runtime fixture timed out");yield return null;}
    }
}
