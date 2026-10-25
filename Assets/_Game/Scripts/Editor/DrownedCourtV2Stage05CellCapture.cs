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

// Captures current cell art and geometry into data only; no production edits.
public sealed class DrownedCourtV2Stage05CellCapture
{
    const string Root="ArtSource/DrownedCourtEnvironmentV2";
    [Serializable] public class Piece {public string name,asset,sprite;public Rect rect;public Color color;public Vector2 nativeSize;}
    [Serializable] public class Snapshot {public Rect board;public Piece[] cells;public string scope="Read-only cell renderer snapshot using a disposable profile.";}
    static IEnumerator Until(Func<bool> f){float end=Time.realtimeSinceStartup+60;while(!f()){Assert.That(Time.realtimeSinceStartup,Is.LessThan(end));yield return null;}}
    [UnityTearDown] public IEnumerator Cleanup(){Time.timeScale=1;RunLaunchOptions.StartingZone=null;GameplayPixelLayoutController.ValidationSafeArea=null;if(Application.isPlaying)yield return new ExitPlayMode();}
    [UnityTest,Explicit] public IEnumerator CaptureCells()
    {
        typeof(GameplayPixelLayoutTests).GetMethod("SetGameViewSize",BindingFlags.NonPublic|BindingFlags.Static).Invoke(null,new object[]{new Vector2Int(720,1280)});
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);yield return new EnterPlayMode();
        string profile=Path.GetFullPath(".utmp/CourtV2/"+Guid.NewGuid()+".json");Directory.CreateDirectory(Path.GetDirectoryName(profile));
        File.WriteAllText(profile,JsonUtility.ToJson(new AccountSave{potions=3,bombs=3,equipPotions=true,equipBombs=true}));
        using(CharacterSelectionSettings.UseTemporarySelection("skeleton"))
        using(AccountProgression.UseDisposableProfile(profile))
        {
            RunLaunchOptions.StartingZone="drowned-court";UnityEngine.Random.InitState(3101);SceneManager.LoadScene("Game");yield return null;
            yield return Until(()=>RunSession.Current?.Continuation?.CanCapture==true&&RunSession.Current.Waves.IsWaveActive);
            yield return new WaitForSecondsRealtime(1.5f);Canvas.ForceUpdateCanvases();
            var board=RunSession.Current.Board;var camera=Camera.main;var tiles=board.transform.Find("CellTiles");Assert.That(tiles,Is.Not.Null);
            var cells=tiles.GetComponentsInChildren<SpriteRenderer>().Where(s=>s.enabled&&s.sprite).Select(s=>{
                var lo=camera.WorldToScreenPoint(s.bounds.min);var hi=camera.WorldToScreenPoint(s.bounds.max);
                return new Piece{name=s.name,asset=AssetDatabase.GetAssetPath(s.sprite),sprite=s.sprite.name,rect=Rect.MinMaxRect(lo.x,lo.y,hi.x,hi.y),color=s.color,nativeSize=s.sprite.rect.size};
            }).ToArray();Assert.That(cells.Length,Is.EqualTo(64));
            var a=camera.WorldToScreenPoint(board.transform.TransformPoint(new Vector3(-board.LocalBoardWidth/2,-board.LocalBoardHeight/2)));
            var b=camera.WorldToScreenPoint(board.transform.TransformPoint(new Vector3(board.LocalBoardWidth/2,board.LocalBoardHeight/2)));
            File.WriteAllText(Root+"/QA/Stage05/CellSnapshot.json",JsonUtility.ToJson(new Snapshot{board=Rect.MinMaxRect(a.x,a.y,b.x,b.y),cells=cells},true));
        }
        yield return new ExitPlayMode();
    }
}
