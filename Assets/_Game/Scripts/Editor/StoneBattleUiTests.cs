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
using Object=UnityEngine.Object;

public sealed class StoneBattleUiTests
{
    const string Root="ArtSource/StoneAbilityUI/Preview";
    static void Size(int w,int h)=>typeof(GameplayPixelLayoutTests).GetMethod("SetGameViewSize",BindingFlags.NonPublic|BindingFlags.Static).Invoke(null,new object[]{new Vector2Int(w,h)});
    static IEnumerator Until(Func<bool> ready){float end=Time.realtimeSinceStartup+60;while(!ready()){Assert.That(Time.realtimeSinceStartup,Is.LessThan(end));yield return null;}}
    static IEnumerator Wait(float seconds){float end=Time.realtimeSinceStartup+seconds;yield return Until(()=>Time.realtimeSinceStartup>=end);}
    [UnityTearDown]public IEnumerator Cleanup(){SceneManager.sceneLoaded-=SeedScene;Time.timeScale=1;RunLaunchOptions.StartingZone=null;GameplayPixelLayoutController.ValidationSafeArea=null;if(Application.isPlaying)yield return new ExitPlayMode();}
    [UnityTest,Explicit]public IEnumerator Before()=>Capture(false);
    [UnityTest,Explicit]public IEnumerator After()=>Capture(true);
    static IEnumerator Capture(bool verify)
    {
        SessionState.SetBool("StoneUiVerify",verify);EditorUtility.audioMasterMute=true;Size(1080,2340);
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);yield return new EnterPlayMode();
        verify=SessionState.GetBool("StoneUiVerify",false);string folder=Root+(verify?"/After":"/Before");Directory.CreateDirectory(folder);
        string profile=Path.GetFullPath(".utmp/StoneUI/"+Guid.NewGuid()+".json");Directory.CreateDirectory(Path.GetDirectoryName(profile));
        File.WriteAllText(profile,JsonUtility.ToJson(new AccountSave{potions=3,bombs=3,equipPotions=true,equipBombs=true}));
        using(CharacterSelectionSettings.UseTemporarySelection("skeleton"))
        {
            foreach(string zone in new[]{"ironvein-excavation","dungeon","magical-forest","drowned-court","frostwake-peaks"})
            {
                // A separate disposable save prevents a prior zone continuation from
                // overriding the next zone under test.
                profile=Path.GetFullPath(".utmp/StoneUI/"+Guid.NewGuid()+".json");
                File.WriteAllText(profile,JsonUtility.ToJson(new AccountSave{potions=3,bombs=3,equipPotions=true,equipBombs=true}));
                using(AccountProgression.UseDisposableProfile(profile))
                {
                RunLaunchOptions.StartingZone=zone;SceneManager.sceneLoaded+=SeedScene;
                SceneManager.LoadScene("Game");yield return null;SceneManager.sceneLoaded-=SeedScene;
                yield return Until(()=>RunSession.Current?.Continuation?.CanCapture==true&&RunSession.Current.Waves.IsWaveActive);
                var visuals=Object.FindFirstObjectByType<BoardVisuals>();
                typeof(BoardVisuals).GetField("randomizeTileLayoutEachRun",BindingFlags.NonPublic|BindingFlags.Instance).SetValue(visuals,false);
                typeof(BoardVisuals).GetField("tileLayoutSeed",BindingFlags.NonPublic|BindingFlags.Instance).SetValue(visuals,12345);visuals.RefreshPresentationFrame();
                var waves=RunSession.Current.Waves;
                if(zone=="ironvein-excavation")foreach(string id in new[]{"rivet_gunner","stonewright"})
                    if(waves.ActiveEnemies.Count<3)Assert.That(waves.TrySummonEnemy(RunSession.Current.Zone.Definition.enemies.Single(e=>e.EnemyId==id),out _),Is.True);
                yield return Until(()=>RunSession.Current.Continuation.CanCapture&&waves.ActiveEnemies.All(e=>e.GetComponent<EnemyLifecycleVFX>()?.IsSpawning!=true));
                var sizes=new[]{new Vector3Int(1080,2340,0),new Vector3Int(720,1280,0),new Vector3Int(360,640,0),new Vector3Int(1080,2400,1)};
                foreach(var size in sizes)
                {
                    GameplayPixelLayoutController.ValidationSafeArea=null;Size(size.x,size.y);yield return Until(()=>UnityEngine.Device.Screen.width==size.x&&UnityEngine.Device.Screen.height==size.y);
                    GameplayPixelLayoutController.ValidationSafeArea=size.z==1?new Rect(24,96,size.x-48,size.y-216):(Rect?)null;
                    yield return Wait(.4f);Canvas.ForceUpdateCanvases();
                    var layout=Object.FindFirstObjectByType<GameplayPixelLayoutController>();var board=RunSession.Current.Board;
                    string key=zone+"-"+size.x+"x"+size.y+(size.z==1?"-safe":"");
                    var errors=GameplayPixelLayoutValidator.Validate(layout,out var report);Assert.That(errors,Is.Empty,report);
                    File.WriteAllText(folder+"/"+key+".txt",report.TrimEnd()+"\n");
                    var world=GameObject.Find("ApprovedIronveinEnvironment");
                    var snap=new Snapshot{
                        images=layout.GetComponentsInChildren<Image>().Where(im=>!IsSharedControl(im.transform)).Select(im=>State(im)).OrderBy(s=>s).ToArray(),
                        feet=layout.TopHud.GetComponentsInChildren<EnemySlotUI>().Where(s=>s.CurrentEnemy).Select(BattleFloorAnchor.EnemyFeetScreenY).ToArray(),
                        cells=board.GetComponentsInChildren<SpriteRenderer>().Where(r=>r.name.StartsWith("CellTile")).Select(r=>r.sprite.name+":"+r.bounds).OrderBy(s=>s).ToArray(),
                        scenery=world?world.GetComponentsInChildren<Image>().Where(im=>im.name!="IronveinTopModules_CavernDepth").Select(State).OrderBy(s=>s).ToArray():Array.Empty<string>()};
                    string data=JsonUtility.ToJson(snap,true);File.WriteAllText(folder+"/"+key+".json",data);
                    string png=Path.GetFullPath(folder+"/"+key+".png");ScreenCapture.CaptureScreenshot(png);yield return Wait(.15f);Assert.That(File.Exists(png),Is.True);
                    if(verify)
                    {
                        var before=JsonUtility.FromJson<Snapshot>(File.ReadAllText(Root+"/Before/"+key+".json"));
                        Assert.That(snap.feet,Is.EqualTo(before.feet),"Enemy floor remains exact");
                        Assert.That(snap.cells,Is.EqualTo(before.cells),"Board and cell geometry remain exact");
                        Assert.That(snap.scenery,Is.EqualTo(before.scenery),"Every existing environment placement remains exact");
                        Assert.That(snap.images.Where(x=>!IsResponsiveIntent(x)),Is.EqualTo(before.images.Where(x=>!IsResponsiveIntent(x))),"All non-control/non-responsive-intent UI placement and artwork remain exact");
                        // EnemyUnifiedIntentView already clamps to the live wave plaque.
                        // Replacing the old tall stretched Court plaque legitimately releases
                        // that clamp. Require identical art/color and test its actual clearance
                        // below, rather than treating the old plaque's bounds as game rules.
                        Assert.That(snap.images.Where(IsResponsiveIntent).Select(IntentArt),Is.EqualTo(before.images.Where(IsResponsiveIntent).Select(IntentArt)),"Intent artwork and colors unchanged");
                        ValidateSharedControls(layout);
                        if(world)ValidateCoverage(world);
                        else Assert.That(IronveinBattleAssembly.Current,Is.Null,"Other zones retain their own presentation");
                    }
                }
                if(verify)
                {
                    var controls=Object.FindFirstObjectByType<RunControlsUI>();controls.SettingsButton.onClick.Invoke();yield return null;Assert.That(Time.timeScale,Is.Zero);controls.Close();yield return null;Assert.That(controls.GameplayCanvas.GetComponentsInChildren<Button>(true).Any(b=>b.name=="CombatGuide"),Is.False);controls.GameplayCanvas.GetComponentsInChildren<Button>().First(b=>b.name=="InspectEnemy").onClick.Invoke();yield return null;Assert.That(controls.IsModalOpen,Is.True);controls.Close();yield return null;
                    var run=RunSession.Current;var all=controls.GameplayCanvas.GetComponentsInChildren<Button>();
                    var bomb=all.Single(b=>b.name=="Bomb");Assert.That(bomb.interactable,Is.True);int bombs=run.Charges(ConsumableKind.Bomb);
                    bomb.onClick.Invoke();yield return null;Assert.That(run.Board.IsSelectingTarget,Is.True);bomb.onClick.Invoke();yield return null;Assert.That(run.Board.IsSelectingTarget,Is.False);Assert.That(run.Charges(ConsumableKind.Bomb),Is.EqualTo(bombs));
                    run.Player.TryTakeDamage(25);yield return null;
                    var potion=all.Single(b=>b.name=="HealthPotion");Assert.That(potion.interactable,Is.True);int potions=run.Charges(ConsumableKind.HealthPotion);potion.onClick.Invoke();yield return null;Assert.That(run.Charges(ConsumableKind.HealthPotion),Is.EqualTo(potions-1));
                    var energy=run.Player.GetComponent<PlayerAbilityEnergy>();energy.AddEnergy(1000);yield return Wait(.7f);
                    var ability=all.Single(b=>b.name=="AbilityButton");Assert.That(ability.interactable,Is.True);var stone=ability.GetComponent<StoneAbilityButtonView>();Assert.That(stone.IsRaised,Is.True);Assert.That(stone.Glow.color.a,Is.GreaterThan(.5f));int beforeEnergy=energy.CurrentEnergy;ability.onClick.Invoke();yield return null;Assert.That(energy.CurrentEnergy,Is.LessThan(beforeEnergy));
                    yield return Until(()=>!run.Board.IsBusy);
                }
                }
            }
        }
        yield return new ExitPlayMode();
    }
    static bool IsSharedControl(Transform t)
    {
        for(var p=t;p;p=p.parent)
            if(new[]{"WaveTracker","CombatGuide","Settings","HealthPotion","Bomb","AbilityButton","ConsumableHint","ZoneResonance"}.Contains(p.name))return true;
        return false;
    }
    static bool IsResponsiveIntent(string state)=>state.StartsWith("ImpactUnderline::") || state.StartsWith("Sword:Assets/_Game/Resources/UI/CombatStatuses/Sword.png:") || state.StartsWith("Slot:Assets/_Game/Resources/UI/CasterSigils/");
    static string IntentArt(string state)=>System.Text.RegularExpressions.Regex.Replace(state,@"\(x:[^)]+\)","<responsive position>");
    static void ValidateSharedControls(GameplayPixelLayoutController layout)
    {
        var all=layout.GetComponentsInChildren<Image>();
        string[] names={"Settings","HealthPotion","Bomb","AbilityButton","EnergyFrame"};
        string[] sprites={"Stone/Settings","Stone/Supply","Stone/Supply","Stone/AbilitySocket","ReferenceDungeon/SharedEnergy"};
        Assert.That(all.Any(im=>im.name=="CombatGuide"),Is.False,"Guide button is removed in every zone");
        var bounds=new System.Collections.Generic.List<Rect>();
        var tiles=RunSession.Current.Board.GetComponentsInChildren<SpriteRenderer>().Where(x=>x.name.StartsWith("CellTile")).ToArray();
        Assert.That(tiles,Is.Not.Empty);
        foreach(var (name,index) in names.Select((n,i)=>(n,i)))
        {
            var im=all.Single(x=>x.name==name);var rect=GameplayPixelLayoutController.ScreenRect(im.rectTransform);
            Assert.That(AssetDatabase.GetAssetPath(im.sprite),Is.EqualTo("Assets/_Game/Resources/UI/BattlePresentation/"+sprites[index]+".png"));
            Assert.That(rect.width/im.sprite.rect.width,Is.EqualTo(layout.Current.Scale).Within(.01),name+" native x scale");
            Assert.That(rect.height/im.sprite.rect.height,Is.EqualTo(layout.Current.Scale).Within(.01),name+" native y scale");
            Assert.That(im.GetComponent<ReferenceKitSurface>()?.enabled??false,Is.False);
            Assert.That(im.GetComponent<NativePixelUiSurface>()?.enabled??false,Is.False);
            Assert.That(im.GetComponentsInChildren<ResponsiveModularFrameFitter>(),Is.Empty,"No old frame over the shared plate");
            foreach(var tile in tiles)
            {
                var lo=Camera.main.WorldToScreenPoint(tile.bounds.min);var hi=Camera.main.WorldToScreenPoint(tile.bounds.max);
                Assert.That(rect.Overlaps(Rect.MinMaxRect(lo.x,lo.y,hi.x,hi.y)),Is.False,name+" covers a playable tile");
            }
            bounds.Add(rect);
        }
        for(int a=0;a<bounds.Count;a++)for(int b=a+1;b<bounds.Count;b++)Assert.That(bounds[a].Overlaps(bounds[b]),Is.False,names[a]+" overlaps "+names[b]);
        var wave=layout.TopHud.Find("WaveTracker").GetComponent<Image>();
        Assert.That(wave.type,Is.EqualTo(Image.Type.Tiled));Assert.That(wave.sprite.border,Is.EqualTo(new Vector4(11,10,11,10)));
        Assert.That(wave.pixelsPerUnitMultiplier,Is.EqualTo(1));
        Assert.That(wave.transform.Find("SharedWavePendant"),Is.Not.Null);
        var wr=GameplayPixelLayoutController.ScreenRect(wave.rectTransform);
        Assert.That(wr.Overlaps(bounds[0]),Is.False,"Wave overlaps top buttons");
        var text=wave.GetComponentInChildren<TMPro.TMP_Text>();string saved=text.text;
        foreach(var value in new[]{"WAVE 1","WAVE 99999"}){text.text=value;PixelTextFitter.ApplyNativeMenu(text,40);text.ForceMeshUpdate();Assert.That(text.isTextOverflowing,Is.False,value);Assert.That(text.textBounds.size.x,Is.LessThanOrEqualTo(text.rectTransform.rect.width+.1f));Assert.That(text.textInfo.characterInfo.Take(text.textInfo.characterCount).Where(c=>c.isVisible).Max(c=>c.textElement.glyph.metrics.height*c.scale),Is.LessThanOrEqualTo(21),"Native glyph height stays inside dark plaque inset (excluding transparent TMP mesh padding)");}
        text.text=saved;PixelTextFitter.ApplyNativeMenu(text,40);text.ForceMeshUpdate();
        Assert.That(text.fontSize*layout.Current.Scale/16,Is.EqualTo(Mathf.Round(text.fontSize*layout.Current.Scale/16)).Within(.01),"Whole physical glyph pixels");
        foreach(var actor in RunSession.Current.Waves.ActiveEnemies)
        {
            var view=actor.GetComponent<EnemyUnifiedIntentView>();Assert.That(view,Is.Not.Null);
            var ar=GameplayPixelLayoutController.ScreenRect(view.ActionRoot);
            var slot=GameplayPixelLayoutController.ScreenRect((RectTransform)view.ActionRoot.parent);
            Assert.That(ar.xMin,Is.GreaterThanOrEqualTo(slot.xMin-.01f));Assert.That(ar.xMax,Is.LessThanOrEqualTo(slot.xMax+.01f));
            Assert.That(ar.yMax,Is.LessThan(wr.yMin),"Counters stay below the live header");
            Assert.That(GameplayPixelLayoutController.ScreenRect(view.Statuses.Rect).yMax,Is.LessThan(wr.yMin),"Statuses stay below the live header");
            foreach(var button in bounds.Take(1))Assert.That(ar.Overlaps(button),Is.False,"Counters never overlap Settings");
        }
    }
    static void SeedScene(Scene scene,LoadSceneMode mode)
    {
        // Match the encounter and weakness choices across independent processes.
        // Unity's seed alone does not control WaveController's SavedRandom.
        UnityEngine.Random.InitState(3101);
        var waves=Object.FindFirstObjectByType<WaveController>();
        typeof(WaveController).GetField("encounterSeed",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(waves,3101);
        typeof(WaveController).GetField("encounterRandom",BindingFlags.Instance|BindingFlags.NonPublic).SetValue(waves,null);
    }
    static string State(Image im)=>im.name+":"+AssetDatabase.GetAssetPath(im.sprite)+":"+GameplayPixelLayoutController.ScreenRect(im.rectTransform)+":"+im.type+":"+im.color;
    static void ValidateCoverage(GameObject world)
    {
        var cave=world.GetComponentsInChildren<Image>().Single(im=>im.name=="IronveinTopModules_CavernDepth");
        var cr=GameplayPixelLayoutController.ScreenRect(cave.rectTransform);
        Assert.That(cave.raycastTarget,Is.False);Assert.That(cave.color,Is.EqualTo(Color.white));
        Assert.That(cave.sprite.texture.filterMode,Is.EqualTo(FilterMode.Point));
        Assert.That(AssetDatabase.GetAssetPath(cave.sprite),Is.EqualTo("Assets/_Game/Art/IronveinTopCoverage/CavernWide.png"));
        float sx=cr.width/cave.sprite.rect.width,sy=cr.height/cave.sprite.rect.height;
        Assert.That(sx,Is.EqualTo(sy).Within(.001),"Uniform source pixel scale");
        Assert.That(sx,Is.EqualTo(BattleFloorAnchor.PhysicalPixelScale).Within(.001),"Normal environment pixel scale; never zoom to fill");
        Assert.That(cave.sprite.rect.size,Is.EqualTo(new Vector2(480,352)),"Full native scene canvas");
        var source=new Texture2D(2,2);source.LoadImage(File.ReadAllBytes("Assets/_Game/Art/IronveinStaged/CavernDepth.png"));
        var original=source.GetPixels32();var full=cave.sprite.texture.GetPixels32();
        Assert.That(full.All(c=>c.a==255),Is.True,"Every pixel in the full cave is opaque");
        for(int y=0;y<160;y++)for(int x=0;x<240;x++)
            Assert.That(full[y*480+180+x],Is.EqualTo(original[y*240+x]),"Original cave pixel unchanged");
        Object.Destroy(source);
        Assert.That(sx,Is.EqualTo(Mathf.Round(sx)).Within(.001),"Whole physical pixels, no fractional sampling");
        Assert.That(cr.xMin,Is.LessThanOrEqualTo(.01));Assert.That(cr.xMax,Is.GreaterThanOrEqualTo(Screen.width-.01));
        Assert.That(cr.yMax,Is.GreaterThanOrEqualTo(Screen.height-.01),"Cover physical top beyond safe area");
        Assert.That(cr.xMin,Is.EqualTo(Mathf.Round(cr.xMin)).Within(.01));
        Assert.That(cr.yMin,Is.EqualTo(Mathf.Round(cr.yMin)).Within(.01));
        var floor=world.GetComponentsInChildren<Image>().Where(im=>im.name.StartsWith("Revision03_WalkingSurface")).ToArray();
        Assert.That(floor,Is.Not.Empty);
        Assert.That(cr.yMin,Is.LessThan(floor.Max(im=>GameplayPixelLayoutController.ScreenRect(im.rectTransform).yMax)),"Cave continues behind floor without a gap");

    }
    [Serializable]class Snapshot{public string[] images,cells,scenery;public float[] feet;}
}
