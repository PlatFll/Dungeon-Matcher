using System;
using System.Collections;
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

public sealed class ThreeZonePresentationTests
{
    private bool priorReduced;
    [SetUp] public void Prepare(){priorReduced=PresentationPreferences.ReducedMotion;PresentationPreferences.SetReducedMotion(false);}
    [UnityTearDown] public IEnumerator Cleanup()
    {
        Time.timeScale=1;RunLaunchOptions.StartingZone=null;GameplayPixelLayoutController.ValidationSafeArea=null;
        PresentationPreferences.SetReducedMotion(priorReduced);
        if(Application.isPlaying)yield return new ExitPlayMode();
    }
    [UnityTest] public IEnumerator CaptureThreeZones()
    {
        EditorUtility.audioMasterMute = true;
        typeof(GameplayPixelLayoutTests).GetMethod("SetGameViewSize", BindingFlags.Static|BindingFlags.NonPublic)
            .Invoke(null,new object[]{new Vector2Int(1080,1920)});
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        yield return new EnterPlayMode();
        string stage = Resources.Load<Sprite>("UI/BattlePresentation/Dungeon/Plaque") == null ? "before" : "after";
        string output = Path.GetFullPath(".utmp/ThreeZonePresentation/"+stage);
        Directory.CreateDirectory(output);
        using(CharacterSelectionSettings.UseTemporarySelection("skeleton"))
        foreach(string zone in new[]{"dungeon","magical-forest","drowned-court"})
        {
            string file=Path.Combine(output,zone+"-"+Guid.NewGuid()+".json");
            File.WriteAllText(file,JsonUtility.ToJson(new AccountSave{potions=3,bombs=3,equipPotions=true,equipBombs=true}));
            using(AccountProgression.UseDisposableProfile(file))
            {
                UnityEngine.Random.InitState(3101);
                RunLaunchOptions.StartingZone=zone;
                SceneManager.LoadScene("Game");
                yield return Until(()=>RunSession.Current?.Continuation?.CanCapture==true && RunSession.Current.Waves.IsWaveActive);
                yield return new WaitForSecondsRealtime(1);
                Canvas.ForceUpdateCanvases();
                var errors=GameplayPixelLayoutValidator.Validate(Object.FindFirstObjectByType<GameplayPixelLayoutController>(),out var report);
                File.WriteAllText(Path.Combine(output,zone+"-layout.txt"),report+"\n"+string.Join("\n",errors));
                Assert.That(errors,Is.Empty);
                File.WriteAllLines(Path.Combine(output,zone+"-images.txt"),Object.FindObjectsByType<Image>(FindObjectsSortMode.None)
                    .Select(i=>$"{i.transform.parent?.name}/{i.name}: sprite={(i.sprite!=null?i.sprite.name:"none")}; rect={i.rectTransform.rect}; position={i.rectTransform.position}; type={i.type}; enabled={i.enabled}"));
                File.WriteAllLines(Path.Combine(output,zone+"-ambient.txt"),Object.FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None)
                    .Where(s=>s.transform.parent!=null && s.transform.parent.name=="AmbientDetails")
                    .Select(s=>$"{s.name}: sprite={AssetDatabase.GetAssetPath(s.sprite)}:{s.sprite?.name}; texture={AssetDatabase.GetAssetPath(s.sprite?.texture)}; material={s.sharedMaterial?.name}; shader={s.sharedMaterial?.shader?.name}; rect={s.sprite?.rect}"));
                ScreenCapture.CaptureScreenshot(Path.Combine(output,zone+"-1080.png"));
                yield return new WaitForSecondsRealtime(.4f);
                if(stage=="after")
                {
                    Assert.That(BattlePresentationSkin.UsesReferenceKit,Is.True,zone+" must use its approved kit");
                    string referenceRoot="Assets/_Game/Resources/UI/BattlePresentation/Reference"+BattlePresentationSkin.Folder+"/";
                    Assert.That(AssetDatabase.GetAssetPath(BattlePresentationSkin.Reference("BattleFloor")),Is.EqualTo(referenceRoot+"BattleFloor.png"));
                    Assert.That(AssetDatabase.GetAssetPath(BattlePresentationSkin.Shared("Ability")),Is.EqualTo("Assets/_Game/Resources/UI/BattlePresentation/ReferenceDungeon/SharedAbility.png"));
                    if(zone!="dungeon")
                    {
                        var floor=GameObject.Find("ReferenceKitBattleFloor").GetComponent<Image>();
                        Assert.That(floor.sprite.rect,Is.EqualTo(new Rect(0,10,470,77)),"Arena must exclude the ten source rows hidden by the board rim.");
                        Assert.That(floor.GetComponent<ReferenceKitSurface>().Extent.y,Is.EqualTo(288));
                    }
                    var nativeLayout=Object.FindFirstObjectByType<GameplayPixelLayoutController>();
                    Assert.That(nativeLayout.TopHud.Find("GeneratedTopBattleLayout/BattleArenaFrame"),Is.Null);
                    Assert.That(nativeLayout.BottomHud.Find("GeneratedBottomHudFrame"),Is.Null);
                    var vista=Object.FindFirstObjectByType<BattleBackgroundTilemapController>().ActiveEnvironment;
                    var map=vista.GetComponentInChildren<UnityEngine.Tilemaps.Tilemap>();
                    var used=new System.Collections.Generic.List<Sprite>();
                    foreach(var cell in map.cellBounds.allPositionsWithin)if(map.GetSprite(cell)!=null)used.Add(map.GetSprite(cell));
                    Assert.That(used.Count,Is.EqualTo(96));
                    Assert.That(used.Distinct().Count(),Is.EqualTo(96),"Scenery tiles must appear once; never clamp/repeat the last floor row.");
                    var framePieces=RunSession.Current.Board.GetComponentsInChildren<SpriteRenderer>()
                        .Where(s=>s.sprite!=null && AssetDatabase.GetAssetPath(s.sprite).EndsWith("/FrameAtlas.png")).ToArray();
                    if(BattlePresentationSkin.UsesReferenceKit)
                    {
                        Assert.That(framePieces,Is.Empty,"The previous frame must not render behind the approved kit.");
                        var approvedFrame=RunSession.Current.Board.transform.Find("BoardFrame/ReferenceKitBoardFrame");
                        Assert.That(approvedFrame,Is.Not.Null);
                        Assert.That(approvedFrame.GetComponent<SpriteRenderer>().sprite,Is.SameAs(BattlePresentationSkin.Reference("BoardFrame")));
                        Assert.That(vista.GetComponentsInChildren<UnityEngine.Tilemaps.TilemapRenderer>().All(r=>r.forceRenderingOff),Is.True);
                        Assert.That(Object.FindObjectsByType<ReferenceKitSurface>(FindObjectsSortMode.None).Length,Is.EqualTo(zone=="dungeon"?12:10));
                        foreach(var surface in Object.FindObjectsByType<ReferenceKitSurface>(FindObjectsSortMode.None))
                            Assert.That(surface.GetComponent<Image>().raycastTarget,Is.EqualTo(surface.TryGetComponent<Button>(out _)));
                    }
                    else
                    {
                        Assert.That(framePieces.Length,Is.EqualTo(28));
                        Assert.That(framePieces.Select(s=>s.sprite.name).Distinct().Count(),Is.EqualTo(28));
                    }
                    var cells=RunSession.Current.Board.GetComponentsInChildren<SpriteRenderer>()
                        .Where(s=>s.name.StartsWith("CellTile_",StringComparison.Ordinal)).ToArray();
                    Assert.That(cells.Length,Is.EqualTo(64));
                    Assert.That(cells.All(s=>s.sprite!=null && s.sprite.name.StartsWith(BattlePresentationSkin.UsesReferenceKit?"EmptyCell":"Cell",StringComparison.Ordinal)),Is.True);
                    var ambient=Object.FindFirstObjectByType<BattleAmbientPresentation>();
                    Assert.That(ambient,Is.Not.Null);
                    if(BattlePresentationSkin.UsesReferenceKit)Assert.That(ambient.DetailCount,Is.Zero,"Old positioned ambient art must not overlap reference landmarks.");
                    else Assert.That(ambient.DetailCount,Is.GreaterThanOrEqualTo(2));
                    var before=JsonUtility.ToJson(RunSession.Current.Board.CaptureContinuation(id=>id));
                    float clock=ambient.AnimationTime;
                    for(int frame=0;frame<24;frame++)
                    {
                        string sequence=Path.Combine(output,zone+"-motion");Directory.CreateDirectory(sequence);
                        ScreenCapture.CaptureScreenshot(Path.Combine(sequence,$"{frame:00}.png"));
                        yield return new WaitForSecondsRealtime(.15f);
                    }
                    Assert.That(ambient.AnimationTime,Is.GreaterThan(clock));
                    Assert.That(JsonUtility.ToJson(RunSession.Current.Board.CaptureContinuation(id=>id)),Is.EqualTo(before),"Ambience must not change the board or refill RNG.");
                    var controls=Object.FindFirstObjectByType<RunControlsUI>();controls.OpenSettings();yield return null;
                    clock=ambient.AnimationTime;yield return new WaitForSecondsRealtime(.3f);
                    Assert.That(ambient.AnimationTime,Is.EqualTo(clock));controls.Close();yield return null;
                    PresentationPreferences.SetReducedMotion(true);yield return null;clock=ambient.AnimationTime;
                    yield return new WaitForSecondsRealtime(.3f);Assert.That(ambient.AnimationTime,Is.EqualTo(clock));
                    PresentationPreferences.SetReducedMotion(false);
                    foreach(var size in new[]{new Vector2Int(720,1280),new Vector2Int(1080,2400)})
                    {
                        typeof(GameplayPixelLayoutTests).GetMethod("SetGameViewSize",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,new object[]{size});
                        GameplayPixelLayoutController.ValidationSafeArea=size.y==2400?new Rect(0,80,1080,2200):(Rect?)null;
                        yield return new WaitForSecondsRealtime(.6f);Canvas.ForceUpdateCanvases();
                        var layout=Object.FindFirstObjectByType<GameplayPixelLayoutController>();
                        errors=GameplayPixelLayoutValidator.Validate(layout,out report);
                        File.WriteAllText(Path.Combine(output,$"{zone}-{size.x}x{size.y}-layout.txt"),report+"\n"+string.Join("\n",errors));
                        Assert.That(errors,Is.Empty,$"{zone} at {size}");
                        ScreenCapture.CaptureScreenshot(Path.Combine(output,$"{zone}-{size.x}x{size.y}.png"));yield return new WaitForSecondsRealtime(.2f);
                    }
                    // Keep comparison captures in the same idle state, then exercise a real move.
                    var response=RunSession.Current.Board.GetImmediateResponses().First();
                    int moves=RunSession.Current.Board.CaptureContinuation(id=>id).moves;
                    RunSession.Current.Board.ReplayPlayerSwap(response.Source.Column,response.Source.Row,response.Target.Column,response.Target.Row);
                    yield return Until(()=>RunSession.Current.Continuation.CanCapture);
                    Assert.That(RunSession.Current.Board.CaptureContinuation(id=>id).moves,Is.EqualTo(moves+1));
                    GameplayPixelLayoutController.ValidationSafeArea=null;
                    typeof(GameplayPixelLayoutTests).GetMethod("SetGameViewSize",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,new object[]{new Vector2Int(1080,1920)});
                }
                SceneManager.LoadScene("MainMenu"); yield return null; yield return null;
                Assert.That(Object.FindFirstObjectByType<BattleAmbientPresentation>(),Is.Null);
            }
        }
        Time.timeScale=1;RunLaunchOptions.StartingZone=null;
        yield return new ExitPlayMode();
    }
    [UnityTest] public IEnumerator EveryPlayableKeepsItsAbilitySymbol()
    {
        EditorUtility.audioMasterMute=true;
        typeof(GameplayPixelLayoutTests).GetMethod("SetGameViewSize",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,new object[]{new Vector2Int(1080,1920)});
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);yield return new EnterPlayMode();
        string output=Path.GetFullPath(".utmp/ThreeZonePresentation/characters");Directory.CreateDirectory(output);
        foreach(string id in new[]{"skeleton","bardley","gideon_glass","dungeon_attender","kobold_gunner"})
        using(CharacterSelectionSettings.UseTemporarySelection(id))
        using(AccountProgression.UseDisposableProfile(Path.Combine(output,Guid.NewGuid()+".json")))
        {
            RunLaunchOptions.StartingZone="dungeon";SceneManager.LoadScene("Game");
            yield return Until(()=>RunSession.Current?.Continuation?.CanCapture==true);
            yield return new WaitForSecondsRealtime(.5f);
            var ability=Object.FindFirstObjectByType<AbilityButtonUI>();
            var icon=ability.transform.Find("AbilityIcon").GetComponent<Image>();
            Assert.That(icon.sprite,Is.Not.Null);
            var bounds=GameplayPixelLayoutController.ScreenRect(icon.rectTransform);
            var button=GameplayPixelLayoutController.ScreenRect((RectTransform)ability.transform);
            Assert.That(button.Contains(bounds.min) && button.Contains(bounds.max),Is.True,id);
            Assert.That(ability.GetComponent<Button>().image.sprite,Is.SameAs(BattlePresentationSkin.Shared("Ability")));
            var errors=GameplayPixelLayoutValidator.Validate(Object.FindFirstObjectByType<GameplayPixelLayoutController>(),out _);
            Assert.That(errors,Is.Empty,id);
            ScreenCapture.CaptureScreenshot(Path.Combine(output,id+".png"));yield return new WaitForSecondsRealtime(.2f);
            SceneManager.LoadScene("MainMenu");yield return null;yield return null;
        }
        yield return new ExitPlayMode();
    }
    [Test] public void NativeExportsAndSharedSkins()
    {
        const string root="Assets/_Game/Resources/UI/BattlePresentation";
        Assert.That(Directory.Exists(root));
        foreach(string path in Directory.GetFiles(root,"*.png",SearchOption.AllDirectories))
        {
            var importer=(TextureImporter)AssetImporter.GetAtPath(path.Replace('\\','/'));
            Assert.That(importer.filterMode,Is.EqualTo(FilterMode.Point),path);
            Assert.That(importer.mipmapEnabled,Is.False,path);
            Assert.That(importer.textureCompression,Is.EqualTo(TextureImporterCompression.Uncompressed),path);
            Assert.That(importer.spritePixelsPerUnit,Is.EqualTo(64),path);
        }
        foreach(string family in new[]{"Guide","Supply","Ability"})
        foreach(string state in new[]{"","Hover","Pressed","Disabled"})
            Assert.That(Resources.Load<Sprite>("UI/BattlePresentation/Shared/"+family+state),Is.Not.Null,family+state);
        foreach(string zone in new[]{"Dungeon","Forest","Court"})
        foreach(string path in Directory.GetFiles(root+"/Reference"+zone,"*.png"))
        {
            string name=Path.GetFileName(path);
            Assert.That(name.EndsWith("ArtOnly.png"),Is.False);
            Assert.That(name,Is.Not.EqualTo("BoardWell.png"));
            string correction="ArtSource/BattlePresentation/ReferenceIntegration/native/"+zone+"/"+name;
            string source=File.Exists(correction)?correction:"ArtSource/BattlePresentation/"+zone+"ReferenceKit/native/"+name;
            CollectionAssert.AreEqual(File.ReadAllBytes(source),File.ReadAllBytes(path));
        }
    }
    [Test] public void ReferenceCompositionKeepsNativeControlsAligned()
    {
        foreach(var size in new[]{new Vector2Int(720,1280),new Vector2Int(1080,1920),new Vector2Int(1080,2400)})
        {
            var g=GameplayPixelLayoutController.Calculate(size.x,size.y,new Rect(0,0,size.x,size.y),new Vector2(544,544),320,true);
            Assert.That(g.Fits,Is.True);
            Assert.That(g.Bottom.height%2,Is.Zero,"Even native controls need an integer-pixel center.");
            Assert.That(g.Top.height,Is.InRange(320,480));
            Assert.That(g.Board.width,Is.EqualTo(g.Board.height));
            Assert.That(g.Top.yMin-g.Board.yMax,Is.GreaterThanOrEqualTo(6));
            Assert.That(g.Board.yMin-g.Bottom.yMax,Is.GreaterThanOrEqualTo(6));
        }
    }
    private static IEnumerator Until(Func<bool> ready)
    {float deadline=Time.realtimeSinceStartup+40;while(!ready()){Assert.That(Time.realtimeSinceStartup,Is.LessThan(deadline));yield return null;}}
}
