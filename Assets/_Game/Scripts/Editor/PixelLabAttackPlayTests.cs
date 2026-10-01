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
using UnityEngine.UI;
using Object = UnityEngine.Object;

public sealed class PixelLabAttackPlayTests
{
    [Serializable] private sealed class Entry { public string name; public int[] frames, durations; public int impactFrame; }
    [Serializable] private sealed class Entries { public Entry[] entries; }
    private const BindingFlags Flags=BindingFlags.Instance|BindingFlags.NonPublic;
    private readonly List<string> runtimeErrors=new List<string>();

    private void CaptureLog(string message,string trace,LogType type)
    {
        if(type!=LogType.Error&&type!=LogType.Exception&&type!=LogType.Assert)return;
        // Resizing the Game View can log the previous camera geometry during
        // reflow. Record it, then validate the settled geometry explicitly below.
        if(message.StartsWith("Pixel layout FAILED:"))
            File.AppendAllText(".utmp/EnemyAttacksReview/resize-diagnostics.txt",message+"\n");
        else runtimeErrors.Add(message+"\n"+trace);
    }
    [UnityTearDown] public IEnumerator Cleanup()
    {
        Application.logMessageReceived-=CaptureLog;LogAssert.ignoreFailingMessages=false;Time.timeScale=1;
        if(Application.isPlaying)yield return new ExitPlayMode();
    }

    [UnityTest] public IEnumerator EveryAuthoredAttackHitsOncePerStrikeAndReturnsToIdle()
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        yield return new EnterPlayMode();
        var entries=JsonUtility.FromJson<Entries>("{\"entries\":"+File.ReadAllText("ArtSource/EnemyAttacks/selected.json")+"}").entries;
        string output=Path.GetFullPath(".utmp/EnemyAttacksReview");Directory.CreateDirectory(output);
        runtimeErrors.Clear();Application.logMessageReceived+=CaptureLog;LogAssert.ignoreFailingMessages=true;
        using(AccountProgression.UseDisposableProfile(Path.Combine(output,Guid.NewGuid()+".json")))
        using(CharacterSelectionSettings.UseTemporarySelection("skeleton"))
        {
            typeof(CombatActionValidation).GetMethod("SetSize",BindingFlags.NonPublic|BindingFlags.Static).Invoke(null,new object[]{1080,1920});
            yield return new WaitForSecondsRealtime(.3f);
            SceneManager.LoadScene("Game");
            yield return Until(()=>RunSession.Current!=null&&RunSession.Current.Continuation.CanCapture);
            var run=RunSession.Current;
            if(UnityEngine.EventSystems.EventSystem.current!=null)UnityEngine.EventSystems.EventSystem.current.enabled=false;
            foreach(var controls in Object.FindObjectsByType<RunControlsUI>(FindObjectsSortMode.None)){controls.Close();controls.enabled=false;}
            foreach(var initial in run.Waves.ActiveEnemies)initial.GetComponent<EnemyAutoAttack>().StopAttacking();
            foreach(var entry in entries)
            {
                yield return Until(()=>run.Waves.HasFreeEnemySlot);
                var definition=AssetDatabase.LoadAssetAtPath<EnemyDefinition>("Assets/_Game/Data/Enemies/Enemy_"+entry.name+".asset");
                Assert.That(definition.TimeAutoAttackFromAnimation&&definition.UseAuthoredAutoAttackMotion,Is.True,entry.name);
                Assert.That(run.Waves.TrySummonEnemy(definition,out EnemyActor enemy),Is.True,entry.name);
                var attack=enemy.GetComponent<EnemyAutoAttack>();attack.StopAttacking();
                var image=enemy.transform.Find("VisualRoot").GetComponent<Image>();var animator=image.GetComponent<Animator>();
                var frames=CombatActionImporter.LoadFrames(entry.name+"_AutoAttack");
                Assert.That(frames.Length,Is.EqualTo(entry.frames.Length),entry.name);
                var clip=AssetDatabase.LoadAssetAtPath<AnimationClip>(CombatActionImporter.AnimationRoot+"/"+entry.name+"_AutoAttack.anim");
                Assert.That(clip.length,Is.EqualTo(.68f).Within(.001f));
                Assert.That(clip.events.Count(e=>e.functionName=="AutoAttackImpact"),Is.EqualTo(1));
                var importer=(TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(frames[0]));
                Assert.That(importer.filterMode,Is.EqualTo(FilterMode.Point));
                Assert.That(importer.textureCompression,Is.EqualTo(TextureImporterCompression.Uncompressed));
                yield return new WaitForSeconds(.4f);
                run.Player.RestoreToFullHealth();
                var hitPoses=new List<Sprite>();Action<PlayerActor,int> onHit=(p,d)=>hitPoses.Add(image.sprite);run.Player.DamageTaken+=onHit;
                Assert.That(attack.PerformAttackImmediately(),Is.True,entry.name);
                Assert.That(hitPoses,Is.Empty,"No early damage");
                if(entry==entries[0])
                {
                    yield return new WaitForSeconds(.1f);Time.timeScale=0;var frozen=image.sprite;
                    Assert.That(attack.ResolveAnimationImpact(),Is.False);
                    yield return new WaitForSecondsRealtime(3.2f);
                    Assert.That(image.sprite,Is.SameAs(frozen));Assert.That(hitPoses,Is.Empty);Time.timeScale=1;
                }
                yield return Until(()=>hitPoses.Count>0);
                Assert.That(attack.ResolveAnimationImpact(),Is.False,"Duplicate event must not damage twice");
                yield return Until(()=>!attack.IsAttackSequenceInProgress);
                float recoveryDeadline=Time.time+0.2f;
                while(!animator.GetCurrentAnimatorStateInfo(0).IsName("Idle")&&Time.time<recoveryDeadline)yield return null;
                Assert.That(hitPoses.Count,Is.EqualTo(enemy.FollowUpDamage>0?2:1),entry.name+" strikes");
                Assert.That(hitPoses.All(p=>p==frames[entry.impactFrame]),Is.True,entry.name+" actual impact pose");
                Assert.That(animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"),Is.True,entry.name+" return at "+animator.GetCurrentAnimatorStateInfo(0).normalizedTime+" speed "+animator.speed);
                run.Player.DamageTaken-=onHit;
                enemy.GetComponent<EnemyCombatFeedback>().enabled=false;
                foreach(int height in new[]{1920,2400})
                {
                    typeof(CombatActionValidation).GetMethod("SetSize",BindingFlags.NonPublic|BindingFlags.Static).Invoke(null,new object[]{1080,height});
                    yield return new WaitForSeconds(.3f);Time.timeScale=0;
                    animator.fireEvents=false;animator.Play("Idle",0,0);animator.Update(0);yield return null;
                    Rect baseline=ScreenRect(image);float idleScale=baseline.height/image.sprite.rect.height;
                    int elapsed=0;
                    for(int f=0;f<frames.Length;f++)
                    {
                        animator.Play("AutoAttack",0,(elapsed+entry.durations[f]*.5f)/680f);animator.Update(0);elapsed+=entry.durations[f];
                        yield return null;Canvas.ForceUpdateCanvases();
                        Rect rect=ScreenRect(image);float scale=rect.height/frames[f].rect.height;
                        Assert.That(image.sprite,Is.SameAs(frames[f]));
                        Assert.That(scale,Is.EqualTo(idleScale).Within(.01f),entry.name+" native body scale");
                        Assert.That(scale,Is.EqualTo(Mathf.Round(scale)).Within(.01f),entry.name+" integer texels");
                        Assert.That(rect.center.x,Is.EqualTo(baseline.center.x).Within(.1f));
                        Assert.That(rect.yMin,Is.EqualTo(baseline.yMin).Within(.1f),entry.name+" floor");
                        Assert.That(rect.xMin>=0&&rect.xMax<=1080&&rect.yMin>=0&&rect.yMax<=height,Is.True,entry.name+" screen bounds");
                        if(f==entry.impactFrame)
                        {
                            ScreenCapture.CaptureScreenshot(Path.Combine(output,entry.name+"-"+height+".png"));
                            yield return new WaitForSecondsRealtime(.1f);
                        }
                    }
                    animator.Play("Idle",0,0);animator.Update(0);animator.fireEvents=true;Time.timeScale=1;
                    yield return new WaitForSeconds(.15f);
                    var issues=GameplayPixelLayoutValidator.Validate(Object.FindFirstObjectByType<GameplayPixelLayoutController>(),out string layout);
                    File.WriteAllText(Path.Combine(output,entry.name+"-"+height+"-layout.txt"),layout+"\n"+string.Join("\n",issues));
                    Assert.That(issues,Is.Empty,entry.name+" settled layout: "+string.Join("; ",issues));
                    Assert.That(runtimeErrors,Is.Empty,string.Join("\n",runtimeErrors));
                }
                int lateHits=0;Action<PlayerActor,int> late=(p,d)=>lateHits++;run.Player.DamageTaken+=late;
                Assert.That(attack.PerformAttackImmediately(),Is.True);yield return new WaitForSeconds(.1f);
                if(enemy.HasShield)enemy.TryTakeDamageWithoutFeedback(1000000);
                enemy.TryTakeDamageWithoutFeedback(1000000);
                yield return new WaitForSeconds(.8f);Assert.That(lateHits,Is.Zero,entry.name+" death cancels pending strike");
                run.Player.DamageTaken-=late;
            }
            File.WriteAllText(Path.Combine(output,"validation.txt"),"PASS: "+entries.Length+" production enemy attacks; authored impact sprites, per-strike damage, duplicate guard, pause, idle return, death cancellation and every pose at both portrait sizes.\n");
            SceneManager.LoadScene("MainMenu");yield return null;
        }
        Assert.That(runtimeErrors,Is.Empty,string.Join("\n",runtimeErrors));
        Application.logMessageReceived-=CaptureLog;LogAssert.ignoreFailingMessages=false;
        Time.timeScale=1;yield return new ExitPlayMode();
    }
    private static Rect ScreenRect(Image image)
    {
        var corners=new Vector3[4];image.rectTransform.GetWorldCorners(corners);
        var camera=image.canvas.renderMode==RenderMode.ScreenSpaceOverlay?null:image.canvas.worldCamera;
        return Rect.MinMaxRect(RectTransformUtility.WorldToScreenPoint(camera,corners[0]).x,RectTransformUtility.WorldToScreenPoint(camera,corners[0]).y,
            RectTransformUtility.WorldToScreenPoint(camera,corners[2]).x,RectTransformUtility.WorldToScreenPoint(camera,corners[2]).y);
    }
    private static IEnumerator Until(Func<bool> ready)
    {
        float deadline=Time.realtimeSinceStartup+25;
        while(!ready()){Assert.That(Time.realtimeSinceStartup,Is.LessThan(deadline),"attack fixture timed out");yield return null;}
    }
}
