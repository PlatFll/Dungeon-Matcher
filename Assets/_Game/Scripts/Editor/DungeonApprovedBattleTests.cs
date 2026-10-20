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

public sealed class DungeonApprovedBattleTests
{
    public const string Evidence = "ArtSource/DungeonEnvironment/BattleIntegration";
    [Serializable] public class BoundsRecord { public string name; public Rect bounds; }
    [Serializable] public class LayoutRecord { public BoundsRecord[] items; public Vector3 boardPosition, boardScale; public float canvasScale; }
    [UnityTest, Explicit("Capture only on the pre-integration main revision; never refresh approved baselines on this branch.")]
    public IEnumerator CaptureProductionBaseline() => Capture(false);
    [UnityTest] public IEnumerator ApprovedArtKeepsLiveBattleLayoutAndInput() => Capture(true);

    [UnityTearDown] public IEnumerator Cleanup()
    {
        Time.timeScale=1;RunLaunchOptions.StartingZone=null;GameplayPixelLayoutController.ValidationSafeArea=null;
        if(Application.isPlaying)yield return new ExitPlayMode();
    }
    private static void Size(Vector2Int size) => typeof(GameplayPixelLayoutTests)
        .GetMethod("SetGameViewSize",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,new object[]{size});
    private static IEnumerator Until(Func<bool> ready)
    {float end=Time.realtimeSinceStartup+45;while(!ready()){Assert.That(Time.realtimeSinceStartup,Is.LessThan(end));yield return null;}}
    private static string Hierarchy(Transform t) => t.parent==null?t.name:Hierarchy(t.parent)+"/"+t.name;
    private static LayoutRecord Measure(GameplayPixelLayoutController layout)
    {
        var roots=new HashSet<string>{"PlayerSection","EnemySection","PlayerCharacter","PlayerHealthBar","WaveTracker","BattleFloorAnchor"};
        var rects=layout.GetComponentsInChildren<RectTransform>(true)
            .Where(r=>roots.Contains(r.name)||r.GetComponent<EnemySlotUI>()!=null||r.GetComponent<Button>()!=null||r==layout.TopHud||r==layout.BoardArea||r==layout.BottomHud)
            .Distinct().OrderBy(r=>Hierarchy(r)).ToArray();
        return new LayoutRecord{items=rects.Select(r=>new BoundsRecord{name=Hierarchy(r),bounds=GameplayPixelLayoutController.ScreenRect(r)}).ToArray(),
            boardPosition=RunSession.Current.Board.transform.position,boardScale=RunSession.Current.Board.transform.localScale,
            canvasScale=layout.GetComponentInParent<Canvas>().rootCanvas.scaleFactor};
    }
    private static void Compare(LayoutRecord before,LayoutRecord after,string label)
    {
        CollectionAssert.AreEqual(before.items.Select(r=>r.name),after.items.Select(r=>r.name),label);
        for(int i=0;i<before.items.Length;i++)
        {
            var a=before.items[i].bounds;var b=after.items[i].bounds;
            Assert.That(Vector4.Distance(new Vector4(a.x,a.y,a.width,a.height),new Vector4(b.x,b.y,b.width,b.height)),Is.LessThan(.1f),label+" "+before.items[i].name);
        }
        Assert.That(Vector3.Distance(before.boardPosition,after.boardPosition),Is.LessThan(.0001f));
        Assert.That(Vector3.Distance(before.boardScale,after.boardScale),Is.LessThan(.0001f));
        Assert.That(after.canvasScale,Is.EqualTo(before.canvasScale));
    }
    public static void AssertApprovedLayers(GameplayPixelLayoutController layout)
    {
        if(DungeonPixelLabBattleArt.Current) {DungeonPixelPipelineStage16Tests.AssertNativeLayers(layout);return;}
        var art = ApprovedDungeonBattleArt.Current;
        Assert.That(art, Is.Not.Null);
        var scenery = GameObject.Find("ApprovedDungeonScenery").transform;
        Assert.That(scenery.parent, Is.SameAs(layout.TopHud));
        Assert.That(scenery.GetSiblingIndex(), Is.Zero, "Actors and live HUD must draw over the scenery.");
        Rect standingStrip = GameplayPixelLayoutController.ScreenRect(
            (RectTransform)scenery.Find("ApprovedDungeonStandingStripA"));
        foreach (var slot in layout.TopHud.GetComponentsInChildren<EnemySlotUI>())
        {
            var visual = slot.EnemySpawnAnchor.GetComponentsInChildren<Image>()
                .FirstOrDefault(image => image.name == "VisualRoot" && image.sprite != null);
            if (visual == null) continue;
            float ratio = GameplayPixelLayoutController.ScreenRect(visual.rectTransform).height / visual.sprite.rect.height;
            float ground = GameplayPixelLayoutController.ScreenRect(slot.EnemySpawnAnchor).center.y - 32 * ratio;
            Assert.That(ground, Is.InRange(standingStrip.yMin, standingStrip.yMax),
                "The authored enemy reference floor must land on the shallow standing strip.");
        }
        Assert.That(GameObject.Find("ReferenceKitFarWall"), Is.Null);
        Assert.That(GameObject.Find("ReferenceKitLeftBorderDressing"), Is.Null);
        var pieces = RunSession.Current.Board.transform.Find("BoardFrame").GetComponentsInChildren<SpriteRenderer>();
        Assert.That(pieces.Length, Is.EqualTo(8));
        Assert.That(pieces.Select(p => p.sprite.name).Distinct().Count(), Is.EqualTo(8));
        var props = GameObject.Find("ApprovedDungeonSurround").GetComponentsInChildren<SpriteRenderer>();
        Assert.That(props.Length, Is.GreaterThan(0));
        foreach (var prop in props)
        {
            Assert.That(prop.sprite, Is.SameAs(art.Sprite(prop.sprite.name)));
            Assert.That(prop.maskInteraction, Is.EqualTo(SpriteMaskInteraction.None), "Do not trim padded scenery.");
            Assert.That(SortingLayer.GetLayerValueFromID(prop.sortingLayerID),
                Is.LessThan(SortingLayer.GetLayerValueFromID(pieces[0].sortingLayerID)), "Extended art stays behind the board.");
        }
        var wall = GameObject.Find("DungeonSurroundingMasonry").GetComponent<SpriteRenderer>();
        Assert.That(wall.sprite, Is.SameAs(art.Sprite("BackWallReference")));
        Assert.That(wall.drawMode, Is.EqualTo(SpriteDrawMode.Tiled));
        Assert.That(Object.FindObjectsByType<Image>(FindObjectsSortMode.None)
            .Where(i => i.name.StartsWith("ApprovedDungeon")).All(i => !i.raycastTarget), Is.True);
    }

    [Test] public void BindingReusesEveryApprovedNativeModule()
    {
        var art = Resources.Load<ApprovedDungeonBattleArt>("UI/BattlePresentation/ApprovedDungeon");
        Assert.That(art, Is.Not.Null);
        Assert.That(art.modules.Length, Is.EqualTo(119));
        Assert.That(art.modules.Distinct().Count(), Is.EqualTo(119));
        foreach (var sprite in art.modules)
        {
            string path = AssetDatabase.GetAssetPath(sprite);
            Assert.That(path, Does.StartWith("Assets/_Game/Art/DungeonEnvironment/SideViewRebuild/"));
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            Assert.That(importer.filterMode, Is.EqualTo(FilterMode.Point));
            Assert.That(importer.mipmapEnabled, Is.False);
            Assert.That(importer.textureCompression, Is.EqualTo(TextureImporterCompression.Uncompressed));
            Assert.That(importer.spritePixelsPerUnit, Is.EqualTo(64));
        }
    }

    private static IEnumerator Capture(bool integrated)
    {
        EditorUtility.audioMasterMute=true;
        Size(new Vector2Int(1080,1920));EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        yield return new EnterPlayMode();
        integrated=TestContext.CurrentContext.Test.Name.StartsWith("Approved");
        string phase=integrated?"After":"Before";
        string output=Path.GetFullPath(integrated ? "ArtSource/DungeonPixelLab/Diagnostics/Stage16/Regression" : Evidence+"/"+phase);Directory.CreateDirectory(output);
        using(CharacterSelectionSettings.UseTemporarySelection("skeleton"))
        foreach(string zone in new[]{"dungeon","magical-forest","drowned-court"})
        {
            string profile=Path.GetFullPath(".utmp/DungeonApprovedBattle/"+Guid.NewGuid()+".json");Directory.CreateDirectory(Path.GetDirectoryName(profile));
            File.WriteAllText(profile,JsonUtility.ToJson(new AccountSave{potions=3,bombs=3,equipPotions=true,equipBombs=true}));
            using(AccountProgression.UseDisposableProfile(profile))
            {
                UnityEngine.Random.InitState(3101);RunLaunchOptions.StartingZone=zone;SceneManager.LoadScene("Game");
                yield return Until(()=>RunSession.Current?.Continuation?.CanCapture==true&&RunSession.Current.Waves.IsWaveActive);
                if(!integrated) Assert.That(ApprovedDungeonBattleArt.Current,Is.Null,"Baseline must be captured before the integration exists.");
                foreach(var size in new[]{new Vector2Int(1080,1920),new Vector2Int(720,1280),new Vector2Int(1080,2400)})
                {
                    Size(size);GameplayPixelLayoutController.ValidationSafeArea=size.y==2400?new Rect(0,80,1080,2200):(Rect?)null;
                    yield return new WaitForSecondsRealtime(.7f);Canvas.ForceUpdateCanvases();
                    var layout=Object.FindFirstObjectByType<GameplayPixelLayoutController>();
                    var errors=GameplayPixelLayoutValidator.Validate(layout,out string report);
                    string name=zone+"-"+size.x+"x"+size.y;
                    File.WriteAllText(output+"/"+name+"-validation.txt",report+"\n"+string.Join("\n",errors));
                    Assert.That(errors,Is.Empty,name);
                    var geometry=Measure(layout);File.WriteAllText(output+"/"+name+"-layout.json",JsonUtility.ToJson(geometry,true));
                    if(integrated)
                    {
                        var before=JsonUtility.FromJson<LayoutRecord>(File.ReadAllText(Evidence+"/Before/"+name+"-layout.json"));
                        if(DungeonPixelLabBattleArt.Current)
                        {
                            // Native scenery/character pixels and UI skins are intentionally replaced.
                            // The user's existing board footprint and major responsive sections are retained.
                            foreach(string section in new[]{"TopHUD","GameArea","BottomHUD"})
                            {
                                var a=before.items.Single(i=>i.name.EndsWith("/"+section)).bounds;
                                var b=geometry.items.Single(i=>i.name.EndsWith("/"+section)).bounds;
                                Assert.That(Vector2.Distance(a.position,b.position),Is.LessThan(.1),name+section);
                                Assert.That(Vector2.Distance(a.size,b.size),Is.LessThan(.1),name+section);
                            }
                        }
                        else Compare(before,geometry,name);
                    }
                    ScreenCapture.CaptureScreenshot(output+"/"+name+".png");yield return new WaitForSecondsRealtime(.25f);
                    if(integrated&&zone=="dungeon")
                    {
                        AssertApprovedLayers(layout);
                    }
                }
                if(integrated)
                {
                    var board=RunSession.Current.Board;int moves=board.CaptureContinuation(id=>id).moves;
                    var response=board.GetImmediateResponses().First();board.ReplayPlayerSwap(response.Source.Column,response.Source.Row,response.Target.Column,response.Target.Row);
                    yield return Until(()=>RunSession.Current.Continuation.CanCapture);
                    Assert.That(board.CaptureContinuation(id=>id).moves,Is.EqualTo(moves+1));
                    var controls=Object.FindFirstObjectByType<RunControlsUI>();controls.OpenSettings();yield return null;controls.Close();yield return null;
                }
                SceneManager.LoadScene("MainMenu");yield return null;yield return null;
                GameplayPixelLayoutController.ValidationSafeArea=null;Size(new Vector2Int(1080,1920));
            }
        }
        yield return new ExitPlayMode();
    }
}
