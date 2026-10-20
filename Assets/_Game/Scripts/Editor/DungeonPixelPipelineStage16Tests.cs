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
using Object=UnityEngine.Object;

public sealed class DungeonPixelPipelineStage16Tests
{
    const string Root="ArtSource/DungeonPixelLab";
    static void Size(int w,int h)=>typeof(GameplayPixelLayoutTests).GetMethod("SetGameViewSize",BindingFlags.NonPublic|BindingFlags.Static).Invoke(null,new object[]{new Vector2Int(w,h)});
    static IEnumerator Until(Func<bool> f)
    {float end=Time.realtimeSinceStartup+45;while(!f()){Assert.That(Time.realtimeSinceStartup,Is.LessThan(end));yield return null;}}
    public static void AssertNativeLayers(GameplayPixelLayoutController layout)
    {
        var art=DungeonPixelLabBattleArt.Current;Assert.That(art,Is.Not.Null);
        Assert.That(art.modules.Count(s=>AssetDatabase.GetAssetPath(s).StartsWith("Assets/_Game/Art/DungeonPixelLab/")),Is.EqualTo(102));
        Assert.That(art.boardCell64.rect.size,Is.EqualTo(new Vector2(64,64)));
        var world=GameObject.Find("DungeonPixelLabEnvironment").GetComponent<Canvas>();
        Assert.That(world.renderMode,Is.EqualTo(RenderMode.WorldSpace));Assert.That(world.sortingLayerName,Is.EqualTo("Default"));
        Assert.That(world.sortingOrder,Is.LessThan(0));
        Assert.That(GameObject.Find("ApprovedDungeonScenery"),Is.Null);
        Assert.That(GameObject.Find("ApprovedDungeonSurround"),Is.Null);
        Assert.That(RunSession.Current.Board.transform.Find("BoardFrame").GetComponentsInChildren<SpriteRenderer>(),Is.Empty);
        var frame=layout.BoardArea.Find("DungeonPixelLabBoardFrame");Assert.That(frame,Is.Not.Null);
        foreach(var im in world.GetComponentsInChildren<Image>())
        {
            Assert.That(im.raycastTarget,Is.False);Assert.That(im.sprite.texture.filterMode,Is.EqualTo(FilterMode.Point));
            var bounds=GameplayPixelLayoutController.ScreenRect(im.rectTransform);
            Assert.That(bounds.width/im.sprite.rect.width,Is.EqualTo(DungeonPixelLabBattleArt.PhysicalPixelScale).Within(.01));
            Assert.That(bounds.height/im.sprite.rect.height,Is.EqualTo(DungeonPixelLabBattleArt.PhysicalPixelScale).Within(.01));
        }
        var portrait=layout.TopHud.Find("GeneratedTopBattleLayout/PlayerSection/PixelLabPortraitBackdrop/PortraitMask");
        Assert.That(portrait.GetComponent<RectMask2D>(),Is.Not.Null);
        var backing=portrait.GetComponentInChildren<Image>().rectTransform;
        Assert.That(backing.offsetMin.x,Is.LessThan(0));Assert.That(backing.offsetMax.x,Is.GreaterThan(0));
    }

    [UnityTearDown] public IEnumerator Cleanup()
    {
        Time.timeScale=1;RunLaunchOptions.StartingZone=null;GameplayPixelLayoutController.ValidationSafeArea=null;
        if(Application.isPlaying)yield return new ExitPlayMode();
        LogAssert.ignoreFailingMessages=false;
    }
    [UnityTest,Explicit("Stage16 actual Game-scene capture and integration checks.")]
    public IEnumerator DungeonCombatAndPhoneScreens()
    {
        EditorUtility.audioMasterMute=true;
        Size(1080,1920);EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        yield return new EnterPlayMode();
        // Runtime errors and per-resolution layout failures both fail this integration gate.
        LogAssert.ignoreFailingMessages=false;
        var errors=new List<string>();
        string profile=Path.GetFullPath(".utmp/Stage16/"+Guid.NewGuid()+".json");Directory.CreateDirectory(Path.GetDirectoryName(profile));
        File.WriteAllText(profile,JsonUtility.ToJson(new AccountSave{potions=3,bombs=3,equipPotions=true,equipBombs=true}));
        using(CharacterSelectionSettings.UseTemporarySelection("skeleton"))
        using(AccountProgression.UseDisposableProfile(profile))
        {
            UnityEngine.Random.InitState(3101);RunLaunchOptions.StartingZone="dungeon";SceneManager.LoadScene("Game");
            yield return Until(()=>RunSession.Current?.Continuation?.CanCapture==true&&RunSession.Current.Waves.IsWaveActive);
            // Fill the remaining live slots using the existing wave summoning API.
            // This is a disposable visual/combat fixture; no production encounter data changes.
            foreach(string id in new[]{"pan_villager","knight"})
                Assert.That(RunSession.Current.Waves.TrySummonEnemy(Resources.Load<ZoneDefinition>("Zones/dungeon").enemies.First(e=>e.EnemyId==id),out _),Is.True);
            yield return new WaitForSecondsRealtime(1);
            foreach(var test in new[]{new Vector3Int(720,1280,0),new Vector3Int(1080,1920,0),new Vector3Int(1080,2400,0),new Vector3Int(1080,2400,1),new Vector3Int(360,640,0)})
            {
                var size=new Vector2Int(test.x,test.y);Size(size.x,size.y);
                GameplayPixelLayoutController.ValidationSafeArea=test.z==1?new Rect(0,84,1080,2200):(Rect?)null;
                yield return new WaitForSecondsRealtime(1.5f);Canvas.ForceUpdateCanvases();
                var layout=Object.FindFirstObjectByType<GameplayPixelLayoutController>();
                string name=test.z==1?"Dungeon-safe-1080x2400":"Dungeon-"+size.x+"x"+size.y;
                var found=GameplayPixelLayoutValidator.Validate(layout,out string report);
                foreach(string error in found)errors.Add(name+": "+error);
                File.WriteAllText(Root+"/Diagnostics/Stage16/"+name+".txt",report+"\nERRORS:\n"+string.Join("\n",found));
                AssertNativeLayers(layout);
                Assert.That(RunSession.Current.Waves.ActiveEnemies.Count,Is.EqualTo(3));
                var cellArt=RunSession.Current.Board.GetComponentsInChildren<SpriteRenderer>().Where(r=>r.name.StartsWith("CellTile_")).ToArray();
                Assert.That(cellArt.Length,Is.EqualTo(64));Assert.That(cellArt.All(r=>r.sprite==DungeonPixelLabBattleArt.Current.boardCell64),Is.True);
                foreach(var enemy in RunSession.Current.Waves.ActiveEnemies)
                {
                    var intent=enemy.GetComponent<EnemyUnifiedIntentView>().ActionRoot;
                    Assert.That(GameplayPixelLayoutController.ScreenRect(layout.TopHud).Contains(GameplayPixelLayoutController.ScreenRect(intent).center),Is.True);
                    Assert.That(enemy.GetComponentInParent<EnemySlotUI>().CombatBarRect.gameObject.activeInHierarchy,Is.True);
                }
                Assert.That(Object.FindFirstObjectByType<DungeonSideViewArtPreview>(),Is.Null,"Production may not instantiate the art-only preview.");
                Assert.That(GameObject.Find("DungeonPixelLabEnvironment"),Is.Not.Null);
                Assert.That(RunSession.Current.Board.GetComponentsInChildren<Gem>().Length,Is.EqualTo(64));
                string output=Path.GetFullPath(Root+"/Previews/Stage16/"+name+".png");if(File.Exists(output))File.Delete(output);
                ScreenCapture.CaptureScreenshot(output);yield return Until(()=>File.Exists(output));yield return null;
                foreach(var text in layout.TopHud.GetComponentsInChildren<TMPro.TMP_Text>().Where(t=>t.name=="PlayerHPText"||t.name=="EnemyHPText"))
                { text.ForceMeshUpdate();Assert.That(text.textInfo.lineCount,Is.LessThanOrEqualTo(1),name+" health number wrapping"); }
                var liveControls=Object.FindFirstObjectByType<RunControlsUI>();
                liveControls.OpenGuide(CombatGuide.Basics);yield return null;yield return null;
                var resume=GameObject.Find("ResumeGuide").GetComponent<Button>();
                var resumeBounds=GameplayPixelLayoutController.ScreenRect((RectTransform)resume.transform);
                Assert.That(layout.Current.Safe.Contains(resumeBounds.min)&&layout.Current.Safe.Contains(resumeBounds.max),Is.True,name+" guide resume fits safe area");
                resume.onClick.Invoke();yield return null;Assert.That(liveControls.IsModalOpen,Is.False);

            }
            Size(1080,1920);GameplayPixelLayoutController.ValidationSafeArea=null;yield return new WaitForSecondsRealtime(.5f);
            var board=RunSession.Current.Board;int moves=board.CaptureContinuation(id=>id).moves;
            var response=board.GetImmediateResponses().First();board.ReplayPlayerSwap(response.Source.Column,response.Source.Row,response.Target.Column,response.Target.Row);
            yield return Until(()=>RunSession.Current.Continuation.CanCapture);
            Assert.That(board.CaptureContinuation(id=>id).moves,Is.EqualTo(moves+1));
            var controls=Object.FindFirstObjectByType<RunControlsUI>();controls.OpenSettings();yield return null;Assert.That(controls.IsModalOpen,Is.True);controls.Close();yield return null;
            File.WriteAllText(Root+"/Diagnostics/Stage16/Gameplay.txt","Actual Game scene, disposable profile. Rattlebones selected. Active wave, 64 gems, accepted swap increments one move, settings opens/closes.\n"+string.Join("\n",errors));
        }
        Assert.That(errors,Is.Empty);
        yield return new ExitPlayMode();
    }
}
