using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object=UnityEngine.Object;

[NonParallelizable]
public sealed class BombRefinementTests
{
    const BindingFlags Flags=BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public;
    static object Call(object o,string n,params object[] a)=>o.GetType().GetMethod(n,Flags).Invoke(o,a);
    static void Set(object o,string n,object v)=>o.GetType().GetField(n,Flags).SetValue(o,v);
    [Test] public void NativeSequencesArePointFilteredFullRectAndComplete()
    {
        foreach(var family in new[]{"FireHead","FireTrail","StarGlint"})
        {
            var frames=Resources.LoadAll<Sprite>("VFX/BombRefinement/"+family);
            Assert.That(frames.Length,Is.EqualTo(family=="StarGlint"?8:6));
            Assert.That(frames.Select(s=>s.name).Distinct().Count(),Is.EqualTo(frames.Length));
            foreach(var s in frames)
            {
                Assert.That(s.rect.size,Is.EqualTo(new Vector2(family=="StarGlint"?32:64,32)));
                Assert.That(s.pixelsPerUnit,Is.EqualTo(64));Assert.That(s.vertices.Length,Is.EqualTo(4));
                Assert.That(s.texture.filterMode,Is.EqualTo(FilterMode.Point));Assert.That(s.texture.mipmapCount,Is.EqualTo(1));
            }
        }
    }
    [TestCase(GemSpecialType.RowBomb,0)] [TestCase(GemSpecialType.ColumnBomb,90)]
    public void FireTravelsBothWaysAtNativeScaleAndRespectsTheBoardMask(GemSpecialType type,float angle)
    {
        var go=new GameObject("LineTest");bool had=PlayerPrefs.HasKey(PresentationPreferences.ReducedMotionKey);int old=PlayerPrefs.GetInt(PresentationPreferences.ReducedMotionKey);
        try
        {
            PlayerPrefs.SetInt(PresentationPreferences.ReducedMotionKey,0);
            var line=go.AddComponent<BombLineVFX>();line.ConfigureRendering(null,"Gems",4);
            line.Play(type,new Vector3(2,-1,0),10,1,.36f,null);
            Assert.That(go.transform.localPosition,Is.EqualTo(new Vector3(2,-1,0)));
            Assert.That(go.transform.localEulerAngles.z,Is.EqualTo(angle).Within(.01));
            Call(line,"ApplyVisualState",.36f);
            Assert.That(go.transform.localScale,Is.EqualTo(Vector3.one));
            Assert.That(go.transform.Find("FireFront0").localPosition.x,Is.EqualTo(-2.5f));
            Assert.That(go.transform.Find("FireFront1").localPosition.x,Is.EqualTo(2.5f));
            Call(line,"ApplyVisualState",.72f);
            Assert.That(go.transform.Find("FireFront1").localPosition.x,Is.EqualTo(5));
            foreach(var r in go.GetComponentsInChildren<SpriteRenderer>())Assert.That(r.maskInteraction,Is.EqualTo(SpriteMaskInteraction.VisibleInsideMask));
            PlayerPrefs.SetInt(PresentationPreferences.ReducedMotionKey,1);Call(line,"ApplyVisualState",.2f);
            Assert.That(go.transform.Find("FireFront1").GetComponent<SpriteRenderer>().enabled,Is.False);
            line.StopImmediately();Assert.That(go.GetComponentsInChildren<SpriteRenderer>(true).All(r=>!r.enabled),Is.True);
        }
        finally{Object.DestroyImmediate(go);if(had)PlayerPrefs.SetInt(PresentationPreferences.ReducedMotionKey,old);else PlayerPrefs.DeleteKey(PresentationPreferences.ReducedMotionKey);}
    }
    [Test] public void StarStartsAtSourceArrivesBeforeFlashAndDoesNotFollowMovedTargets()
    {
        var go=new GameObject("StarTest");bool had=PlayerPrefs.HasKey(PresentationPreferences.ReducedMotionKey);int old=PlayerPrefs.GetInt(PresentationPreferences.ReducedMotionKey);
        try
        {
            PlayerPrefs.SetInt(PresentationPreferences.ReducedMotionKey,0);var star=go.AddComponent<ColorCrystalGlintVFX>();star.ConfigureRendering(null,"Gems",6);
            star.Play(null,new Vector3(3,2,0),.34f,.24f,.12f,.05f,32,Color.white,null,new Vector3(-3,-2,0),1);
            Assert.That(go.transform.localPosition,Is.EqualTo(new Vector3(-3,-2,0)));
            Set(star,"elapsedTime",.048f);Call(star,"ApplyVisualState");Assert.That(go.transform.localPosition,Is.EqualTo(Vector3.zero));
            Set(star,"elapsedTime",.10f);Call(star,"ApplyVisualState");Assert.That(go.transform.localPosition,Is.EqualTo(new Vector3(3,2,0)));
            Assert.That(go.transform.localScale,Is.EqualTo(Vector3.one));Assert.That(go.transform.localRotation,Is.EqualTo(Quaternion.identity));
            star.StopImmediately();Assert.That(go.activeSelf,Is.False);
        }
        finally{Object.DestroyImmediate(go);if(had)PlayerPrefs.SetInt(PresentationPreferences.ReducedMotionKey,old);else PlayerPrefs.DeleteKey(PresentationPreferences.ReducedMotionKey);}
    }
}

[NonParallelizable]
public sealed class BombRefinementPlayTests
{
    const string Root=".utmp/BombRefinement";
    IDisposable profile,selection;int oldCapture,oldMotion;bool hadMotion,captured;
    static IEnumerator Until(Func<bool> ready){float limit=Time.realtimeSinceStartup+100;while(!ready()){Assert.That(Time.realtimeSinceStartup,Is.LessThan(limit));yield return null;}}
    [UnitySetUp] public IEnumerator Setup()
    {
        typeof(GameplayPixelLayoutTests).GetMethod("SetGameViewSize",BindingFlags.NonPublic|BindingFlags.Static).Invoke(null,new object[]{new Vector2Int(720,1280)});
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);yield return new EnterPlayMode();
        oldCapture=Time.captureFramerate;hadMotion=PlayerPrefs.HasKey(PresentationPreferences.ReducedMotionKey);oldMotion=PlayerPrefs.GetInt(PresentationPreferences.ReducedMotionKey);captured=true;PlayerPrefs.SetInt(PresentationPreferences.ReducedMotionKey,0);
        string path=Path.GetFullPath(Root+"/Profiles/"+Guid.NewGuid()+".json");Directory.CreateDirectory(Path.GetDirectoryName(path));File.WriteAllText(path,JsonUtility.ToJson(new AccountSave{potions=3,bombs=3,equipPotions=true,equipBombs=true}));
        profile=AccountProgression.UseDisposableProfile(path);selection=CharacterSelectionSettings.UseTemporarySelection("skeleton");RunLaunchOptions.StartingZone="dungeon";
        SceneManager.LoadScene("Game");yield return null;yield return Until(()=>RunSession.Current?.Continuation?.CanCapture==true&&RunSession.Current.Waves.IsWaveActive);
        yield return Until(()=>RunSession.Current.Waves.ActiveEnemies.All(e=>e.GetComponent<EnemyLifecycleVFX>()?.IsSpawning!=true));
    }
    [UnityTearDown] public IEnumerator Cleanup()
    {
        Time.timeScale=1;Time.captureFramerate=oldCapture;if(captured){if(hadMotion)PlayerPrefs.SetInt(PresentationPreferences.ReducedMotionKey,oldMotion);else PlayerPrefs.DeleteKey(PresentationPreferences.ReducedMotionKey);}
        profile?.Dispose();selection?.Dispose();RunLaunchOptions.StartingZone=null;if(Application.isPlaying)yield return new ExitPlayMode();
    }
    void Prepare(BoardController board,GemSpecialType special)
    {
        var sprites=(Sprite[])typeof(BoardController).GetField("gemSprites",BindingFlags.NonPublic|BindingFlags.Instance).GetValue(board);
        for(int y=0;y<board.Height;y++)for(int x=0;x<board.Width;x++){var g=board.GetGem(x,y);if(g==null)continue;g.SetSpecialType(GemSpecialType.None);int color=(x+2*y)%6;g.SetType((GemType)color,sprites[color]);}
        if(special==GemSpecialType.ColorCrystal){board.GetGem(3,3).SetSpecialType(special);return;}
        foreach(var xy in new[]{new Vector2Int(2,3),new Vector2Int(3,3),new Vector2Int(4,2)})board.GetGem(xy.x,xy.y).SetType(GemType.Ruby,sprites[0]);
        board.GetGem(4,3).SetType(GemType.Sapphire,sprites[4]);board.GetGem(3,3).SetSpecialType(special);
    }
    IEnumerator Capture(GemSpecialType special)
    {
        var board=RunSession.Current.Board;Prepare(board,special);yield return null;
        string dir=Path.GetFullPath(Root+"/"+special);Directory.CreateDirectory(dir);
        int moves=board.CompletedValidPlayerMoves,cues=0,burstCount=0;board.BombVFXRequested+=_=>cues++;board.TileBurstVFXRequested+=_=>burstCount++;
        Time.captureFramerate=60;
        for(int f=0;f<100;f++)
        {
            if(f==8){if(special==GemSpecialType.ColorCrystal)board.ReplayPlayerSwap(3,3,4,3);else board.ReplayPlayerSwap(4,2,4,3);}
            ScreenCapture.CaptureScreenshot(dir+"/Frame"+f.ToString("D3")+".png");yield return null;
        }
        Time.captureFramerate=oldCapture;yield return Until(()=>File.Exists(dir+"/Frame099.png"));
        yield return Until(()=>board.CompletedValidPlayerMoves>moves&&RunSession.Current.Continuation.CanCapture);
        Assert.That(board.CompletedValidPlayerMoves,Is.EqualTo(moves+1));
        if(special!=GemSpecialType.ColorCrystal){Assert.That(cues,Is.GreaterThan(0));Assert.That(burstCount,Is.Zero,"Directional activation must not duplicate generic tile explosions");}
        Assert.That(board.GetComponentsInChildren<BombLineVFX>().All(v=>!v.IsPlaying),Is.True);
    }
    [UnityTest] public IEnumerator RowBombActualSwap()=>Capture(GemSpecialType.RowBomb);
    [UnityTest] public IEnumerator ColumnBombActualSwap()=>Capture(GemSpecialType.ColumnBomb);
    [UnityTest] public IEnumerator ColorStarActualSwap()=>Capture(GemSpecialType.ColorCrystal);
}
