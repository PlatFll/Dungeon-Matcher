using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public sealed class BalanceFreshShapesPlayTests
{
    private const BindingFlags Flags=BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic;
    [UnityTest] public IEnumerator FreshAndUnlockedShapesResolveThroughRealSwaps()
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);yield return new EnterPlayMode();
        for(int scenario=0;scenario<3;scenario++)
        {
            int level=scenario==2?2:1,length=scenario==1?5:4;
            string path=Path.GetFullPath(".utmp/ShapeProfiles/"+Guid.NewGuid().ToString("N")+".json");Directory.CreateDirectory(Path.GetDirectoryName(path));
            var save=new AccountSave();save.characters.Add(new CharacterProgress{id="skeleton",level=level});File.WriteAllText(path,JsonUtility.ToJson(save));
            using(AccountProgression.UseDisposableProfile(path))
            using(CharacterSelectionSettings.UseTemporarySelection("skeleton"))
            using(GemMasterySettings.UseTemporaryLoadout(GemMasteryLoadout.Default))
            {
                SceneManager.LoadScene("Game");yield return Until(()=>RunSession.Current!=null&&RunSession.Current.Waves.IsWaveActive,"scene starts");
                var board=RunSession.Current.Board;yield return Until(()=>!board.IsBusy&&!board.HasPendingBoardMutation,"initial board settles");
                var grid=(Gem[,])typeof(BoardController).GetField("gems",Flags).GetValue(board);
                var sprites=(Sprite[])typeof(BoardController).GetField("gemSprites",Flags).GetValue(board);
                for(int x=0;x<board.Width;x++)for(int y=0;y<board.Height;y++)
                {grid[x,y].SetSpecialType(GemSpecialType.None);Color(grid[x,y],(x+2*y)%6,sprites);}
                for(int x=0;x<length;x++)Color(grid[x,7],0,sprites);
                Color(grid[2,7],3,sprites);Color(grid[2,6],0,sprites);Color(grid[length,7],4,sprites);
                var original=new List<Gem>();for(int x=0;x<length;x++)original.Add(x==2?grid[2,6]:grid[x,7]);
                int rewarded=0;board.BoardClearResolved+=clear=>{if(clear.Source==BoardClearSource.Match&&clear.CascadeDepth==0)rewarded+=clear.GemCount;};
                int moves=board.CompletedValidPlayerMoves;
                var expected=scenario==1?GemSpecialType.ColorCrystal:GemSpecialType.RowBomb;
                bool created=false;
                board.StartCoroutine((IEnumerator)typeof(BoardController).GetMethod("TrySwap",Flags).Invoke(board,new object[]{grid[2,6],grid[2,7]}));
                yield return Until(()=>
                {
                    // A later refill cascade may legitimately activate the new
                    // bomb. Observe its creation while the real action resolves.
                    foreach(var gem in grid) if(gem!=null&&gem.SpecialType==expected) created=true;
                    return board.CompletedValidPlayerMoves==moves+1&&!board.IsBusy&&!board.HasPendingBoardMutation;
                },"accepted fixture swap settles");
                if(scenario==0)
                {
                    Assert.That(rewarded,Is.GreaterThanOrEqualTo(4),"locked straight four rewards every destroyed gem");
                    foreach(var gem in original)Assert.That(gem==null,Is.True,"no locked bomb preservation");
                    foreach(var gem in grid)if(gem!=null)Assert.That(gem.SpecialType==GemSpecialType.None||gem.SpecialType==GemSpecialType.ColorCrystal,Is.True);
                }
                else
                {
                    Assert.That(created,Is.True,"shape creates its permitted special: "+expected);
                }
                Assert.That(RunSession.Current.ExitTo("MainMenu"),Is.True);yield return null;
            }
        }
        File.WriteAllText(".utmp/balance-fresh-shapes-play.txt","PASS: actual settled swaps reward all four gems on a locked fresh profile; fresh five creates a Color Crystal; independent level 2 unlock allows a Row Bomb. Disposable saves and temporary loadouts; original preferences unchanged.\n");
        yield return new ExitPlayMode();
    }
    private static void Color(Gem gem,int type,Sprite[] sprites)=>gem.SetType((GemType)type,sprites[type]);
    private static IEnumerator Until(Func<bool> condition,string name)
    {float end=Time.realtimeSinceStartup+35;while(!condition()){Assert.That(Time.realtimeSinceStartup,Is.LessThan(end),name);yield return null;}}
}
