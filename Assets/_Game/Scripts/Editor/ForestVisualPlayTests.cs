using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public sealed class ForestVisualPlayTests
{
    [UnityTest] public IEnumerator CaptureActualForestGameplayAndChannelIntent()
    {
        Assert.That(SystemInfo.graphicsDeviceType,Is.Not.EqualTo(UnityEngine.Rendering.GraphicsDeviceType.Null),"run with -Graphics");
        typeof(GameplayPixelLayoutTests).GetMethod("SetGameViewSize",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,new object[]{new Vector2Int(720,1280)});
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);yield return new EnterPlayMode();
        string output=Path.GetFullPath(".utmp/ForestValidation/Visual");Directory.CreateDirectory(output);
        string profile=Path.Combine(output,Guid.NewGuid()+".json");
        File.WriteAllText(profile,JsonUtility.ToJson(new AccountSave{potions=3,bombs=3,equipPotions=true,equipBombs=true}));
        using(AccountProgression.UseDisposableProfile(profile))
        using(CharacterSelectionSettings.UseTemporarySelection("skeleton"))
        {
            RunLaunchOptions.ForestPrototype=true;SceneManager.LoadScene("Game");yield return null;
            yield return Until(()=>RunSession.Current?.Continuation?.CanCapture==true &&
                RunSession.Current.Waves.IsWaveActive && RunSession.Current.Waves.ActiveEnemies.Count==3 &&
                RunSession.Current.Waves.ActiveEnemies.All(e=>e.GetComponent<EnemyLifecycleVFX>()?.IsSpawning!=true),"forest ready");
            yield return new WaitForSeconds(.5f);
            var run=RunSession.Current;Assert.That(run.Zone,Is.Not.Null);
            Assert.That(UnityEngine.Object.FindFirstObjectByType<BattleBackgroundTilemapController>().ActiveEnvironment.EnvironmentId,Is.EqualTo("forest-prototype"));
            CheckLayout(output,"720x1280");
            yield return Capture(output,"01-forest-idle");
            var caster=run.Waves.ActiveEnemies.First(e=>e.Definition.EnemyId=="elven_mender");
            var target=run.Waves.ActiveEnemies.First(e=>e.Definition.EnemyId=="orc_trailguard");
            target.ResolveDamageWithoutFeedback(30);
            caster.GetComponent<EnemyChannelRuntime>().RestoreContinuation(new EnemyCombatSnapshot{channel=new EnemyChannelSnapshot{state=1,sequence=1,targetId=target.PersistentId,deadlineMove=2}},_=>target);
            yield return Capture(output,"02-channel-intent");
            Assert.That(caster.GetComponent<EnemyChannelRuntime>().ResponseMoves,Is.EqualTo(2));
            typeof(GameplayPixelLayoutTests).GetMethod("SetGameViewSize",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,new object[]{new Vector2Int(1080,2400)});
            yield return Until(()=>Screen.width==1080 && Screen.height==2400 &&
                UnityEngine.Object.FindFirstObjectByType<GameplayPixelLayoutController>().Current.Safe.size==new Vector2(1080,2400),"resized canvas and layout agree");
            // Inspect after the new screen size has passed through LateUpdate,
            // Canvas layout, pixel camera projection and render-time snapping.
            for(int frame=0;frame<6;frame++) yield return null;
            CheckLayout(output,"1080x2400");
            yield return Capture(output,"04-forest-tall");
            run.GetComponent<RunControlsUI>().OpenSettings();yield return Capture(output,"05-settings-preserved");
            Assert.That(Time.timeScale,Is.Zero);run.GetComponent<RunControlsUI>().Close();
            Assert.That(run.SuspendToMenu(),Is.True);yield return null;yield return new WaitForSeconds(.5f);
            yield return Capture(output,"03-menu-preserved");
        }
        Time.timeScale=1;yield return new ExitPlayMode();
    }
    private static IEnumerator Capture(string output,string name)
    {yield return null;yield return null;ScreenCapture.CaptureScreenshot(Path.Combine(output,name+".png"));yield return null;yield return null;}
    private static void CheckLayout(string output,string label)
    {
        var layout=UnityEngine.Object.FindFirstObjectByType<GameplayPixelLayoutController>();
        var errors=GameplayPixelLayoutValidator.Validate(layout,out var report);
        File.WriteAllText(Path.Combine(output,"layout-"+label+".txt"),report+"\n"+string.Join("\n",errors));
        Assert.That(errors,Is.Empty,string.Join("\n",errors));
        foreach(var enemy in RunSession.Current.Waves.ActiveEnemies)
        {
            Assert.That(enemy.GetComponents<EnemyMoveIntentView>().Length,Is.EqualTo(1));
            var intent=(RectTransform)enemy.GetComponentInParent<EnemySlotUI>().transform.Find("MoveIntent");
            var visual=(RectTransform)enemy.transform.Find("VisualRoot");
            var intentCorners=new Vector3[4];var spriteCorners=new Vector3[4];
            intent.GetWorldCorners(intentCorners);visual.GetWorldCorners(spriteCorners);
            Assert.That(intentCorners[0].y,Is.GreaterThan(spriteCorners[1].y),
                enemy.name+" move intent must remain above the rendered sprite at "+label);
        }
    }
    private static IEnumerator Until(Func<bool> ready,string why)
    {float end=Time.realtimeSinceStartup+40;while(!ready()){Assert.That(Time.realtimeSinceStartup,Is.LessThan(end),why);yield return null;}}
}
