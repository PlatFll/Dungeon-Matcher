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
            Assert.That(UnityEngine.Object.FindFirstObjectByType<BattleBackgroundTilemapController>().ActiveEnvironment.EnvironmentId,Is.EqualTo("forest-woodland"));
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
            typeof(GameplayPixelLayoutTests).GetMethod("SetGameViewSize",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,new object[]{new Vector2Int(1080,1920)});
            yield return Until(()=>Screen.width==1080&&Screen.height==1920,"1080 screen");
            for(int frame=0;frame<6;frame++) yield return null;
            CheckLayout(output,"1080x1920");yield return Capture(output,"06-forest-1080");
            GameplayPixelLayoutController.ValidationSafeArea=new Rect(32,60,1016,1772);
            for(int frame=0;frame<6;frame++) yield return null;
            CheckLayout(output,"1080x1920-safe");yield return Capture(output,"07-forest-safearea");
            GameplayPixelLayoutController.ValidationSafeArea=null;
            run.GetComponent<RunControlsUI>().OpenSettings();yield return Capture(output,"05-settings-preserved");
            Assert.That(Time.timeScale,Is.Zero);run.GetComponent<RunControlsUI>().Close();
            run.GetComponent<RunControlsUI>().OpenGuide(CombatGuide.Basics);
            yield return Capture(output,"10-guide");run.GetComponent<RunControlsUI>().Close();
            // Presentation fixture uses real catalog cards without claiming an
            // earned wave choice or granting a reward.
            var choice=UnityEngine.Object.FindFirstObjectByType<UpgradeChoiceUI>();
            Assert.That(choice.Show(RunUpgradeRuntime.Current.Catalog.Upgrades.Take(3).ToArray(),_=>false),Is.True);
            Time.timeScale=0;yield return Capture(output,"11-upgrades");choice.Hide();Time.timeScale=1;
            Assert.That(run.SuspendToMenu(),Is.True);yield return null;yield return new WaitForSeconds(.5f);
            yield return Capture(output,"03-menu-preserved");
            SceneManager.LoadScene("Game");yield return null;
            yield return Until(()=>RunSession.Current?.Continuation!=null&&!RunSession.Current.Continuation.IsRestoring,"resume for loss panel");
            run=RunSession.Current;run.GetComponent<RunControlsUI>().Close();
            run.Player.TryTakeDamage(1000000);
            yield return Until(()=>GameObject.Find("GameOverPanel")!=null,"loss panel exists");
            float lossAt=Time.unscaledTime;yield return Until(()=>Time.unscaledTime>=lossAt+3,"loss panel settles");
            yield return Capture(output,"12-game-over");
            SceneManager.LoadScene("MainMenu");yield return null;
        }
        Time.timeScale=1;yield return new ExitPlayMode();
    }
    [UnityTest] public IEnumerator CaptureWardenWarning()
    { PrepareMilestone();yield return new EnterPlayMode();yield return CaptureMilestone(9,"barkhide_warden","08-warden");yield return new ExitPlayMode(); }
    [UnityTest] public IEnumerator CaptureMatriarchRitual()
    { PrepareMilestone();yield return new EnterPlayMode();yield return CaptureMilestone(11,"briar_matriarch","09-matriarch");yield return new ExitPlayMode(); }
    private static void PrepareMilestone()
    {
        Assert.That(SystemInfo.graphicsDeviceType,Is.Not.EqualTo(UnityEngine.Rendering.GraphicsDeviceType.Null));
        typeof(GameplayPixelLayoutTests).GetMethod("SetGameViewSize",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,new object[]{new Vector2Int(1080,1920)});
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
    }
    private static IEnumerator CaptureMilestone(int offset,string id,string file)
    {
        string output=Path.GetFullPath(".utmp/ForestValidation/Visual");Directory.CreateDirectory(output);
        string profile=Path.Combine(output,Guid.NewGuid()+".json");
        File.WriteAllText(profile,JsonUtility.ToJson(new AccountSave{potions=3,bombs=3,equipPotions=true,equipBombs=true}));
        using(AccountProgression.UseDisposableProfile(profile))
        using(CharacterSelectionSettings.UseTemporarySelection("gideon_glass"))
        {
            RunLaunchOptions.ForestPrototype=true;RunLaunchOptions.ForestEncounterOffset=offset;
            SceneManager.LoadScene("Game");yield return null;
            yield return Until(()=>RunSession.Current?.Continuation?.CanCapture==true&&RunSession.Current.Waves.IsWaveActive,"milestone ready");
            var run=RunSession.Current;var caster=run.Waves.ActiveEnemies.First(e=>e.Definition.EnemyId==id);
            foreach(var enemy in run.Waves.ActiveEnemies) enemy.GetComponent<EnemyAutoAttack>().SetActionPaused(typeof(ForestVisualPlayTests),true);
            var target=run.Waves.ActiveEnemies.First(e=>e!=caster);
            target.ResolveDamageWithoutFeedback(25);
            if(offset==9) run.Board.TryQueueVineWarning(caster,2,2,null,true);
            else run.Board.TryQueueVineAnchors(caster,2,null);
            yield return Until(()=>run.Continuation.CanCapture,"anchor art settles");
            caster.GetComponent<ForestMilestoneEnemyAbility>().RestoreContinuation(new EnemyCombatSnapshot{
                forestMilestone=new ForestMilestoneSnapshot{state=1,sequence=1,deadline=offset==9?1:2,targetId=offset==9?0:target.PersistentId}},_=>target);
            float began=Time.time;yield return Until(()=>Time.time>=began+1,"channel reaches hold pose");
            CheckLayout(output,file);yield return Capture(output,file);
            SceneManager.LoadScene("MainMenu");yield return null;
        }
        Time.timeScale=1;RunLaunchOptions.ForestPrototype=false;
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
            var controller=UnityEngine.Object.FindFirstObjectByType<BattleBackgroundTilemapController>();
            var camera=Camera.main;
            var footScreen=RectTransformUtility.WorldToScreenPoint(null,spriteCorners[0]);
            var floorScreen=camera.WorldToScreenPoint(controller.transform.position);
            File.AppendAllText(Path.Combine(output,"contacts-"+label+".txt"),
                enemy.name+" visual bottom screen="+footScreen+" baseline screen="+floorScreen+
                " map position="+controller.transform.position+" sprite="+visual.GetComponent<UnityEngine.UI.Image>().sprite.name+"\n");
            Assert.That(intentCorners[0].y,Is.GreaterThan(spriteCorners[1].y),
                enemy.name+" move intent must remain above the rendered sprite at "+label);
        }
    }
    private static IEnumerator Until(Func<bool> ready,string why)
    {float end=Time.realtimeSinceStartup+40;while(!ready()){Assert.That(Time.realtimeSinceStartup,Is.LessThan(end),why);yield return null;}}
}
