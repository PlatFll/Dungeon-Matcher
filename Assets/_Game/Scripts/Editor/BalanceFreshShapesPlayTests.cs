using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

public sealed class BalanceFreshShapesPlayTests
{
    private const BindingFlags Flags=BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic;
    [UnityTest] public IEnumerator CharacterResetAndSharedMasteryWorkThroughRealMenus()
    {
        if(SystemInfo.graphicsDeviceType!=UnityEngine.Rendering.GraphicsDeviceType.Null)
            typeof(GameplayPixelLayoutTests).GetMethod("SetGameViewSize",BindingFlags.Static|BindingFlags.NonPublic)
                .Invoke(null,new object[]{new Vector2Int(720,1280)});
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);yield return new EnterPlayMode();
        string path=Path.GetFullPath(".utmp/ShapeProfiles/"+Guid.NewGuid().ToString("N")+".json");
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        File.WriteAllText(path,JsonUtility.ToJson(new AccountSave{gold=10000}));
        using(AccountProgression.UseDisposableProfile(path))
        using(CharacterSelectionSettings.UseTemporarySelection("skeleton"))
        using(GemMasterySettings.UseTemporaryLoadout(GemMasteryLoadout.Default))
        {
            SceneManager.LoadScene("MainMenu");yield return null;yield return null;
            var menu=UnityEngine.Object.FindFirstObjectByType<MainMenuController>();
            menu.ShowCharacterSelect();yield return null;
            var characters=UnityEngine.Object.FindFirstObjectByType<CharacterSelectMenuController>();
            Assert.That(Button(characters,"ResetLevel").interactable,Is.False,"already level 1");
            yield return CaptureMenu("01-level-one");
            for(int level=1;level<=7;level++)
            {
                menu.ShowHome();menu.ShowCharacterSelect();
                if(level>1)Button(characters,"LevelUp").onClick.Invoke();
                Assert.That(AccountProgression.Current.Level("skeleton"),Is.EqualTo(level));
                Assert.That(AccountProgression.Current.Level("bardley"),Is.EqualTo(1));
                menu.ShowHome();menu.ShowGemMastery();yield return null;
                var rewards=(Button[])typeof(GemMasteryMenuController).GetField("rewardButtons",Flags)
                    .GetValue(UnityEngine.Object.FindFirstObjectByType<GemMasteryMenuController>());
                Assert.That(rewards[0].interactable,Is.True);
                Assert.That(rewards[1].interactable,Is.EqualTo(level>=3));
                Assert.That(rewards[2].interactable,Is.EqualTo(level>=7));
                Assert.That(rewards[3].interactable,Is.EqualTo(level>=5));
                if(level==1)yield return CaptureMenu("02-mastery-locked");
                if(level==7)
                {
                    rewards[2].onClick.Invoke();
                    Assert.That(GemMasterySettings.GetReward(GemMasteryShape.StraightFive),Is.EqualTo(GemMasteryReward.ShieldBomb));
                    yield return CaptureMenu("03-mastery-unlocked");
                }
            }
            menu.ShowHome();menu.ShowCharacterSelect();yield return null;
            Button(characters,"ResetLevel").onClick.Invoke();
            yield return CaptureMenu("04-confirm-reset");
            Button(characters,"CancelResetLevel").onClick.Invoke();
            Assert.That(AccountProgression.Current.Level("skeleton"),Is.EqualTo(7));
            Button(characters,"ResetLevel").onClick.Invoke();
            Button(characters,"ConfirmResetLevel").onClick.Invoke();
            Assert.That(AccountProgression.Current.Level("skeleton"),Is.EqualTo(1));
            Assert.That(Button(characters,"ResetLevel").interactable,Is.False);
            yield return CaptureMenu("05-reset-complete");
            Button(characters,"LevelUp").onClick.Invoke();Button(characters,"LevelUp").onClick.Invoke();
            Assert.That(characters.transform.Find("UnlockFeedback").GetComponent<Text>().text,Does.Not.Contain("Unlocked for"));
            ((Button)typeof(CharacterSelectMenuController).GetField("bardleyButton",Flags).GetValue(characters)).onClick.Invoke();
            Button(characters,"LevelUp").onClick.Invoke();
            Button(characters,"ResetLevel").onClick.Invoke();Button(characters,"ConfirmResetLevel").onClick.Invoke();
            Assert.That(AccountProgression.Current.Level("bardley"),Is.EqualTo(1));
            Assert.That(AccountProgression.Current.Level("skeleton"),Is.EqualTo(3),"Bardley reset leaves Rattlebones intact");
            Assert.That(GemMasterySettings.GetReward(GemMasteryShape.StraightFive),Is.EqualTo(GemMasteryReward.ShieldBomb));
            menu.ShowHome();
        }
        yield return new ExitPlayMode();
    }
    private static Button Button(Component root,string name)=>root.GetComponentsInChildren<Button>(true).Single(button=>button.name==name);
    private static IEnumerator CaptureMenu(string name)
    {
        if(SystemInfo.graphicsDeviceType==UnityEngine.Rendering.GraphicsDeviceType.Null)yield break;
        string directory=Path.GetFullPath(".utmp/CharacterResetMenu");Directory.CreateDirectory(directory);
        yield return null;yield return null;
        ScreenCapture.CaptureScreenshot(Path.Combine(directory,name+".png"));
        yield return null;yield return null;
    }
    [UnityTest] public IEnumerator FreshRowColumnAndCrystalShapesResolveThroughRealSwaps()
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);yield return new EnterPlayMode();
        for(int scenario=0;scenario<3;scenario++)
        {
            int length=scenario==1?5:4;
            bool column=scenario==2;
            string path=Path.GetFullPath(".utmp/ShapeProfiles/"+Guid.NewGuid().ToString("N")+".json");Directory.CreateDirectory(Path.GetDirectoryName(path));
            var save=new AccountSave();save.characters.Add(new CharacterProgress{id="skeleton",level=1});File.WriteAllText(path,JsonUtility.ToJson(save));
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
                for(int i=0;i<length;i++)Color(column?grid[7,i]:grid[i,7],0,sprites);
                var from=column?grid[6,2]:grid[2,6];var to=column?grid[7,2]:grid[2,7];
                Color(to,3,sprites);Color(from,0,sprites);Color(column?grid[7,length]:grid[length,7],4,sprites);
                int rewarded=0;board.BoardClearResolved+=clear=>{if(clear.Source==BoardClearSource.Match&&clear.CascadeDepth==0)rewarded+=clear.GemCount;};
                int moves=board.CompletedValidPlayerMoves;
                var expected=scenario==1?GemSpecialType.ColorCrystal:column?GemSpecialType.ColumnBomb:GemSpecialType.RowBomb;
                bool created=false;
                board.StartCoroutine((IEnumerator)typeof(BoardController).GetMethod("TrySwap",Flags).Invoke(board,new object[]{from,to}));
                yield return Until(()=>
                {
                    // A later refill cascade may legitimately activate the new
                    // bomb. Observe its creation while the real action resolves.
                    foreach(var gem in grid) if(gem!=null&&gem.SpecialType==expected) created=true;
                    return board.CompletedValidPlayerMoves==moves+1&&!board.IsBusy&&!board.HasPendingBoardMutation;
                },"accepted fixture swap settles");
                Assert.That(created,Is.True,"level 1 shape creates its special: "+expected);
                Assert.That(rewarded,Is.GreaterThanOrEqualTo(length-1),"cleared gems still reward normally");
                Assert.That(RunSession.Current.ExitTo("MainMenu"),Is.True);yield return null;
            }
        }
        File.WriteAllText(".utmp/balance-fresh-shapes-play.txt","PASS: actual settled swaps create Row Bomb, Column Bomb and Color Crystal on fresh level-1 profiles. Disposable saves and temporary loadouts; original preferences unchanged.\n");
        yield return new ExitPlayMode();
    }
    private static void Color(Gem gem,int type,Sprite[] sprites)=>gem.SetType((GemType)type,sprites[type]);
    private static IEnumerator Until(Func<bool> condition,string name)
    {float end=Time.realtimeSinceStartup+35;while(!condition()){Assert.That(Time.realtimeSinceStartup,Is.LessThan(end),name);yield return null;}}
}
