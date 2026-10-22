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

public sealed class DungeonHybridIntegrationTests
{
    [Serializable] public class Geometry {public Rect top,board,bottom;public float ratio;public Rect[] health;}
    const string Root="ArtSource/DungeonHybrid/Integration";
    const string Phase="Final";
    static void Size(int w,int h)=>typeof(GameplayPixelLayoutTests).GetMethod("SetGameViewSize",BindingFlags.NonPublic|BindingFlags.Static).Invoke(null,new object[]{new Vector2Int(w,h)});
    static IEnumerator Until(Func<bool> f)
    {float end=Time.realtimeSinceStartup+45;while(!f()){Assert.That(Time.realtimeSinceStartup,Is.LessThan(end));yield return null;}}
    public static void AssertNativeLayers(GameplayPixelLayoutController layout)
    {
        var art=DungeonPixelLabBattleArt.Current;Assert.That(art,Is.Not.Null);
        Assert.That(art.UsesHybrid,Is.True);
        Assert.That(art.hybridModules.Length,Is.EqualTo(55));
        Assert.That(art.hybridModules.All(a=>a && AssetDatabase.GetAssetPath(a).StartsWith("Assets/_Game/Art/DungeonHybrid/")),Is.True);
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
            if(im.type==Image.Type.Tiled)
            {
                Assert.That(im.pixelsPerUnit*im.pixelsPerUnitMultiplier,Is.EqualTo(1).Within(.001f));
                continue; // A repeated surface has a variable extent, never stretched source pixels.
            }
            Assert.That(bounds.width/im.sprite.rect.width,Is.EqualTo(DungeonPixelLabBattleArt.PhysicalPixelScale).Within(.01));
            Assert.That(bounds.height/im.sprite.rect.height,Is.EqualTo(DungeonPixelLabBattleArt.PhysicalPixelScale).Within(.01));
        }
        Assert.That(frame.GetComponentInChildren<RectMask2D>(),Is.Null,"No opaque overlay above SpriteRenderer gems");
        var backingObject=world.transform.Find("Hybrid board backing");
        Assert.That(backingObject,Is.Not.Null);
        Assert.That(backingObject.GetComponentInParent<Canvas>(),Is.EqualTo(world));
        foreach(var prop in world.transform.Find("Approved hybrid props").GetComponentsInChildren<Image>())
        {
            var r=GameplayPixelLayoutController.ScreenRect(prop.rectTransform);
            Assert.That(layout.Current.Viewport.Contains(r.min)&&layout.Current.Viewport.Contains(r.max),Is.True,prop.name+" must fit completely");
        }
        Assert.That(world.transform.Find("Approved hybrid gate").childCount,Is.EqualTo(10));
        var portrait=layout.TopHud.Find("GeneratedTopBattleLayout/PlayerSection/PixelLabPortraitBackdrop/PortraitMask");
        Assert.That(portrait.GetComponent<RectMask2D>(),Is.Not.Null);
        var backing=portrait.GetComponentInChildren<Image>().rectTransform;
        Assert.That(backing.offsetMin.x,Is.LessThan(0));Assert.That(backing.offsetMax.x,Is.GreaterThan(0));
    }

    // Board-state counts do not prove visibility: an overlay can hide all gems.
    // Check the actual captured pixels, independently of scene component counts.
    static void AssertVisibleGems(string path,Rect board)
    {
        var png=new Texture2D(2,2,TextureFormat.RGBA32,false);
        try
        {
            Assert.That(png.LoadImage(File.ReadAllBytes(path)),Is.True);
            int bright=0,total=0;var pixels=png.GetPixels32();
            for(int y=Mathf.CeilToInt(board.yMin+board.height*.08f);y<board.yMax-board.height*.08f;y++)
            for(int x=Mathf.CeilToInt(board.xMin+board.width*.08f);x<board.xMax-board.width*.08f;x++)
            {
                var c=pixels[y*png.width+x];total++;
                int max=Mathf.Max(c.r,Mathf.Max(c.g,c.b)),min=Mathf.Min(c.r,Mathf.Min(c.g,c.b));
                if(max>90&&max-min>55)bright++;
            }
            Assert.That((float)bright/total,Is.GreaterThan(.15f),"Actual board pixels must show bright gem faces, not an opaque overlay");
        }
        finally {Object.DestroyImmediate(png);}
    }

    [UnityTearDown] public IEnumerator Cleanup()
    {
        Time.timeScale=1;RunLaunchOptions.StartingZone=null;GameplayPixelLayoutController.ValidationSafeArea=null;
        if(Application.isPlaying)yield return new ExitPlayMode();
        LogAssert.ignoreFailingMessages=false;
    }
    [UnityTest,Explicit("Hybrid actual Game-scene capture and integration checks.")]
    public IEnumerator DungeonCombatAndPhoneScreens()
    {
        EditorUtility.audioMasterMute=true;
        Size(1080,1920);EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        yield return new EnterPlayMode();
        // Runtime errors and per-resolution layout failures both fail this integration gate.
        LogAssert.ignoreFailingMessages=false;
        var errors=new List<string>();
        string profile=Path.GetFullPath(".utmp/HybridIntegration/"+Guid.NewGuid()+".json");Directory.CreateDirectory(Path.GetDirectoryName(profile));
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
                File.WriteAllText(Root+"/Diagnostics/"+Phase+"/"+name+".txt",report+"\nERRORS:\n"+string.Join("\n",found));
                AssertNativeLayers(layout);
                var geometry=new Geometry { top=GameplayPixelLayoutController.ScreenRect(layout.TopHud),
                    board=GameplayPixelLayoutController.ScreenRect(layout.BoardArea), bottom=GameplayPixelLayoutController.ScreenRect(layout.BottomHud),
                    ratio=layout.Current.BoardTexelRatio,
                    health=layout.TopHud.GetComponentsInChildren<EnemySlotUI>().Where(s=>s.CurrentEnemy).Select(s=>GameplayPixelLayoutController.ScreenRect(s.CombatBarRect)).ToArray() };
                File.WriteAllText(Root+"/Diagnostics/"+Phase+"/"+name+"-geometry.json",JsonUtility.ToJson(geometry,true));

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
                string output=Path.GetFullPath(Root+"/Previews/"+Phase+"/"+name+".png");if(File.Exists(output))File.Delete(output);
                ScreenCapture.CaptureScreenshot(output);yield return Until(()=>File.Exists(output));yield return null;
                AssertVisibleGems(output,GameplayPixelLayoutController.ScreenRect(layout.BoardArea));
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
            File.WriteAllText(Root+"/Diagnostics/"+Phase+"/Gameplay.txt","Actual Game scene, disposable profile. Rattlebones selected. Active wave, 64 gems, accepted swap increments one move, settings opens/closes.\n"+string.Join("\n",errors));
        }
        Assert.That(errors,Is.Empty);
        yield return new ExitPlayMode();
    }
}
