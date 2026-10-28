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
using TMPro;
using System.Collections.Generic;
using Object=UnityEngine.Object;

// Captures current cell art and geometry into data only; no production edits.
public sealed class IronveinStagedContentCapture
{
    const string Root="ArtSource/IronveinStaged";
    [Serializable] public class Piece {public string name,asset,sprite;public Rect rect;public Color color;public Vector2 nativeSize;}
    [Serializable] public class Snapshot {public Rect board;public Piece[] cells,gems; public GraphicRecord[] graphics;public string scope="Read-only cell renderer snapshot using a disposable profile.";}
    static IEnumerator Until(Func<bool> f){float end=Time.realtimeSinceStartup+60;while(!f()){Assert.That(Time.realtimeSinceStartup,Is.LessThan(end));yield return null;}}
    [UnityTearDown] public IEnumerator Cleanup(){Time.timeScale=1;RunLaunchOptions.StartingZone=null;GameplayPixelLayoutController.ValidationSafeArea=null;if(Application.isPlaying)yield return new ExitPlayMode();}
    [UnityTest,Explicit] public IEnumerator CaptureContent() => CaptureZone("ironvein-excavation", "ContentSnapshot.json");
    [UnityTest,Explicit] public IEnumerator CaptureSharedPlayer() => CaptureZone("dungeon", "SharedPlayerSnapshot.json");
    IEnumerator CaptureZone(string zone,string output)
    {
        typeof(GameplayPixelLayoutTests).GetMethod("SetGameViewSize",BindingFlags.NonPublic|BindingFlags.Static).Invoke(null,new object[]{new Vector2Int(720,1280)});
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);yield return new EnterPlayMode();
        string profile=Path.GetFullPath(".utmp/CourtV2/"+Guid.NewGuid()+".json");Directory.CreateDirectory(Path.GetDirectoryName(profile));
        File.WriteAllText(profile,JsonUtility.ToJson(new AccountSave{potions=3,bombs=3,equipPotions=true,equipBombs=true}));
        using(CharacterSelectionSettings.UseTemporarySelection("skeleton"))
        using(AccountProgression.UseDisposableProfile(profile))
        {
            RunLaunchOptions.StartingZone=zone;UnityEngine.Random.InitState(3101);SceneManager.LoadScene("Game");yield return null;
            yield return Until(()=>RunSession.Current?.Continuation?.CanCapture==true&&RunSession.Current.Waves.IsWaveActive);
            while(RunSession.Current.Waves.ActiveEnemies.Count<3){
                var waves=RunSession.Current.Waves;var definition=waves.ActiveEnemies[0].Definition;
                if(zone=="ironvein-excavation")definition=new[]{"pickaxe_delver","rivet_gunner","stonewright"}.Select(id=>AssetDatabase.LoadAssetAtPath<EnemyDefinition>("Assets/_Game/Data/Enemies/Ironvein/"+id+".asset")).First(d=>d!=null&&!waves.ActiveEnemies.Any(e=>e.Definition==d));
                Assert.That(definition,Is.Not.Null);Assert.That(waves.TrySummonEnemy(definition,out _),Is.True);
            }
            yield return Until(()=>RunSession.Current.Continuation.CanCapture&&RunSession.Current.Waves.ActiveEnemies.All(e=>e.GetComponent<EnemyLifecycleVFX>()?.IsSpawning!=true));
            yield return new WaitForSecondsRealtime(1.5f);Canvas.ForceUpdateCanvases();
            Time.timeScale=0;
            var board=RunSession.Current.Board;var camera=Camera.main;var tiles=board.transform.Find("CellTiles");Assert.That(tiles,Is.Not.Null);
            var cells=tiles.GetComponentsInChildren<SpriteRenderer>().Where(s=>s.enabled&&s.sprite).Select(s=>{
                var lo=camera.WorldToScreenPoint(s.bounds.min);var hi=camera.WorldToScreenPoint(s.bounds.max);
                return new Piece{name=s.name,asset=AssetDatabase.GetAssetPath(s.sprite),sprite=s.sprite.name,rect=Rect.MinMaxRect(lo.x,lo.y,hi.x,hi.y),color=s.color,nativeSize=s.sprite.rect.size};
            }).ToArray();Assert.That(cells.Length,Is.EqualTo(64));
            var a=camera.WorldToScreenPoint(board.transform.TransformPoint(new Vector3(-board.LocalBoardWidth/2,-board.LocalBoardHeight/2)));
            var b=camera.WorldToScreenPoint(board.transform.TransformPoint(new Vector3(board.LocalBoardWidth/2,board.LocalBoardHeight/2)));
            var gems=board.GetComponentsInChildren<SpriteRenderer>().Where(v=>v.enabled&&v.sprite&&v.name.StartsWith("Gem_")).Select(v=>{var lo=camera.WorldToScreenPoint(v.bounds.min);var hi=camera.WorldToScreenPoint(v.bounds.max);return new Piece{name=v.name,asset=AssetDatabase.GetAssetPath(v.sprite),sprite=v.sprite.name,rect=Rect.MinMaxRect(lo.x,lo.y,hi.x,hi.y),color=v.color,nativeSize=v.sprite.rect.size};}).ToArray();
            var graphics=new List<GraphicRecord>();
            var layout=Object.FindFirstObjectByType<GameplayPixelLayoutController>();
            foreach(var g in layout.TopHud.GetComponentsInChildren<Graphic>()){
                var path=PathOf(g.transform);if(!g.enabled||g.color.a==0||g is TMP_SubMeshUI)continue;
                if(!(path.Contains("/PlayerCharacter")||path.Contains("/PlayerAffinityGem")||path.Contains("/PlayerHPBarBackground")||path.Contains("/PlayerShield")||g.GetComponentInParent<EnemySlotUI>()))continue;
                if(path.Contains("EnemyBase"))continue;
                var e=new GraphicRecord{path=path,rect=GameplayPixelLayoutController.ScreenRect(g.rectTransform),color=g.color};
                if(g is Image im){e.kind="image";e.asset=im.sprite?AssetDatabase.GetAssetPath(im.sprite):"";e.sprite=im.sprite?im.sprite.name:"";e.imageType=(int)im.type;e.fill=im.fillAmount;e.fillMethod=(int)im.fillMethod;e.fillOrigin=im.fillOrigin;e.preserveAspect=im.preserveAspect;e.ppuMultiplier=im.pixelsPerUnitMultiplier/im.canvas.scaleFactor;if(im.sprite&&string.IsNullOrEmpty(e.asset))continue;}
                else if(g is TMP_Text text){e.kind="text";e.asset=AssetDatabase.GetAssetPath(text.font);e.text=text.text;e.fontSize=text.fontSize*e.rect.height/text.rectTransform.rect.height;e.alignment=(int)text.alignment;e.fontStyle=(int)text.fontStyle;}else continue;graphics.Add(e);
            }
            Assert.That(gems.Length,Is.EqualTo(64));Assert.That(graphics.Any(g=>g.path.Contains("EnemyHPBarBackground")),Is.True);
            File.WriteAllText(Root+"/QA/"+output,JsonUtility.ToJson(new Snapshot{board=Rect.MinMaxRect(a.x,a.y,b.x,b.y),cells=cells,gems=gems,graphics=graphics.ToArray()},true));
            string capture=Path.GetFullPath(Root+"/Preview/Legacy-"+zone+"-720x1280.png");if(File.Exists(capture))File.Delete(capture);ScreenCapture.CaptureScreenshot(capture);yield return Until(()=>File.Exists(capture));
        }
        Time.timeScale=1;yield return new ExitPlayMode();
    }
    [Serializable] public class GraphicRecord {public string path,kind,asset,sprite,text;public Rect rect;public Color color;public int imageType,fillMethod,fillOrigin,alignment,fontStyle;public float fill,ppuMultiplier,fontSize;public bool preserveAspect;}
    static string PathOf(Transform t)=>t.parent?PathOf(t.parent)+"/"+t.name:t.name;
}
