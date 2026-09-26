using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Opt-in integration checks. Uses real scenes, UI callbacks, combat and board
// coroutines with disposable account/mastery/character state. Never saves scenes.
[InitializeOnLoad]
public static class BalancePlayValidation
{
    private const string Pending="DungeonMatcher.BalancePlayValidation";
    private const BindingFlags Flags=BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public;
    private static readonly Stack<IEnumerator> steps=new Stack<IEnumerator>();
    private static IDisposable profile,selection,mastery;
    private static string output,savePath;
    private static StringBuilder report=new StringBuilder();
    private static bool priorMusic,priorSfx;
    static BalancePlayValidation()
    {
        EditorApplication.playModeStateChanged+=state=>
        {
            if(state==PlayModeStateChange.EnteredPlayMode&&SessionState.GetBool(Pending,false))
            {
                output=Path.GetFullPath(".utmp/BalancePlay");Directory.CreateDirectory(output);
                report.Clear();steps.Clear();steps.Push(Checks());EditorApplication.update+=Tick;
            }
            if(state==PlayModeStateChange.ExitingPlayMode)
            {
                EditorApplication.update-=Tick;steps.Clear();
                if(SessionState.GetBool(Pending,false))Restore();
            }
        };
    }
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void PrepareProfile()
    {
        if(!SessionState.GetBool(Pending,false))return;
        savePath=Path.GetFullPath(".utmp/BalancePlay/profile-"+Guid.NewGuid().ToString("N")+".json");
        Directory.CreateDirectory(Path.GetDirectoryName(savePath));
        File.WriteAllText(savePath,JsonUtility.ToJson(new AccountSave{gold=500,potions=10,bombs=10}));
        profile=AccountProgression.UseDisposableProfile(savePath);
        selection=CharacterSelectionSettings.UseTemporarySelection("skeleton");
        mastery=GemMasterySettings.UseTemporaryLoadout(GemMasteryLoadout.Default);
        priorMusic=AudioPreferences.MusicMuted;priorSfx=AudioPreferences.SfxMuted;
    }
    [MenuItem("Dungeon Matcher/Validation/Balance v1 Integration")]
    public static void Run()
    {
        if(EditorApplication.isPlaying)throw new InvalidOperationException("Exit Play Mode before starting.");
        foreach(string name in new[]{"Potion","Bomb","Slot"})
            AssetDatabase.ImportAsset("Assets/_Game/Resources/UI/Consumables/"+name+".png",ImportAssetOptions.ForceUpdate);
        SetSize(new Vector2Int(720,1280));
        EditorSceneManager.OpenScene("Assets/_Game/Scenes/MainMenu.unity");
        SessionState.SetBool(Pending,true);EditorApplication.EnterPlaymode();
    }
    private static void Tick()
    {
        try
        {
            if(steps.Count==0){Finish(true);return;}
            var current=steps.Peek();
            if(!current.MoveNext()){steps.Pop();return;}
            if(current.Current is IEnumerator nested)steps.Push(nested);
        }
        catch(Exception error){report.AppendLine("FAILED: "+error);Finish(false);}
    }
    private static IEnumerator Checks()
    {
        yield return Wait(1);
        Check(AccountProgression.Current.Gold==500,"disposable profile installed before scene bootstrap");
        yield return Capture("01-main-menu");
        var menu=UnityEngine.Object.FindFirstObjectByType<MainMenuController>();
        menu.ShowCharacterSelect();yield return Capture("02-characters-fresh");
        Click("LevelUp");yield return Wait(.4f);
        Check(AccountProgression.Current.Level("skeleton")==2&&AccountProgression.Current.Gold==475,"Level Up UI updates independent level and wallet");
        Check(AccountProgression.Current.IsUnlocked(GemSpecialType.RowBomb),"directional bombs available from level 1");
        yield return Capture("03-characters-upgraded");
        menu.ShowHome();menu.ShowGemMastery();yield return Capture("04-mastery-locks");
        menu.ShowHome();menu.ShowShop();yield return Capture("05-shop");
        var shop=UnityEngine.Object.FindFirstObjectByType<ShopMenuController>();
        var buys=shop.GetComponentsInChildren<Button>().Where(b=>b.name=="Buy").ToArray();
        Check(buys.Length==2,"shop contains only the two authorized items");buys[0].onClick.Invoke();
        foreach(var equip in shop.GetComponentsInChildren<Button>().Where(b=>b.name=="Equip"))equip.onClick.Invoke();
        Check(AccountProgression.Current.Equipped(ConsumableKind.HealthPotion)&&AccountProgression.Current.Equipped(ConsumableKind.Bomb),"both item types equip together");
        yield return Capture("06-shop-equipped");menu.ShowHome();menu.PlayGame();
        yield return WaitForRun();var run=RunSession.Current;
        Check(run.Charges(ConsumableKind.HealthPotion)==3&&run.Charges(ConsumableKind.Bomb)==3,"independent three-use run snapshots");
        SetSize(new Vector2Int(540,960));yield return Wait(1);
        yield return Capture("07-hud-short");AuditControlBounds();
        SetSize(new Vector2Int(720,1600));
        GameplayPixelLayoutController.ValidationSafeArea=new Rect(0,54,720,1470);
        yield return Wait(1);yield return Capture("08-hud-tall-cutout");
        AuditControlBounds();
        Check(!run.TryUsePotion(),"full-health potion does not spend");
        Check(run.ToggleBombTargeting(),"Bomb accepts target selection");
        Check(run.ToggleBombTargeting()&&run.Charges(ConsumableKind.Bomb)==3&&run.Cooldown(ConsumableKind.Bomb)==0,"target cancellation spends nothing");
        run.Player.TryTakeDamage(30);int hp=run.Player.CurrentHealth;
        Check(run.TryUsePotion()&&run.Player.CurrentHealth>hp,"accepted potion heals through PlayerActor");
        Check(!run.TryUsePotion()&&run.Charges(ConsumableKind.HealthPotion)==2,"repeat potion rejected on cooldown");
        Check(run.ToggleBombTargeting(),"Bomb independent from potion cooldown");
        Check(run.TryUseBomb(At(run.Board,3,3)),"accepted Bomb uses authoritative board pipeline");
        yield return Settle(run.Board);yield return Capture("09-consumable-cooldowns");
        var controls=UnityEngine.Object.FindFirstObjectByType<RunControlsUI>();
        using(var otherOwner=run.Board.AcquireExternalInputBlock())
        {
            controls.OpenSettings();float remaining=run.Cooldown(ConsumableKind.HealthPotion);
            Check(Time.timeScale==0,"settings pause");yield return Wait(.7f);
            Check(Mathf.Approximately(run.Cooldown(ConsumableKind.HealthPotion),remaining),"cooldown freezes with gameplay");
            Check(!run.TryUsePotion()&&!run.ToggleBombTargeting(),"paused consumables rejected");
            Click("Music");Check(AudioPreferences.MusicMuted!=priorMusic&&AudioPreferences.SfxMuted==priorSfx,"Music control independent");
            Click("SFX");Check(AudioPreferences.SfxMuted!=priorSfx,"SFX control independent");
            yield return Capture("10-settings");controls.Close();
            Check(Time.timeScale==1&&run.Board.IsExternalInputBlocked,"closing settings preserves another owner's block");
        }
        Check(!run.Board.IsExternalInputBlocked,"input owner released");
        for(int use=1;use<3;use++)
        {
            yield return WaitForUsableBomb(run);
            Check(run.TryUseBomb(At(run.Board,3,3)),"Bomb use "+(use+1));yield return Settle(run.Board);
        }
        Check(AccountProgression.Current.Owned(ConsumableKind.Bomb)==7&&run.Charges(ConsumableKind.Bomb)==0,"ten owned / three uses leaves seven");
        Check(!run.ToggleBombTargeting(),"fourth use does not refill from stock");
        string oldId=run.RunId;
        controls.OpenSettings();Click("Retry");yield return Wait(.1f);Click("Confirm");yield return WaitForRun();
        run=RunSession.Current;Check(run.RunId!=oldId&&run.Charges(ConsumableKind.Bomb)==3&&Time.timeScale==1,"Retry loads new run charges and restores time");
        Check(AccountProgression.Current.LastReward.runId==oldId,"Retry payout attributed once");
        controls=UnityEngine.Object.FindFirstObjectByType<RunControlsUI>();controls.OpenSettings();Click("Quit");yield return Wait(.1f);Click("Confirm");yield return Wait(1);
        Check(SceneManager.GetActiveScene().name=="MainMenu"&&RunSession.Current==null,"Quit to Menu cleans run runtime");
        Check(new AccountProgression(savePath).Gold==AccountProgression.Current.Gold,"wallet persists on reload");
        Check(AudioPreferences.MusicMuted!=priorMusic&&AudioPreferences.SfxMuted!=priorSfx,"audio choices survive scene transitions");
        menu=UnityEngine.Object.FindFirstObjectByType<MainMenuController>();menu.PlayGame();yield return WaitForRun();run=RunSession.Current;
        yield return PlayToWave(run,3);
        oldId=run.RunId;int gold=AccountProgression.Current.Gold;int expected=AccountProgression.Current.PreviewReward("Defeat").Total;
        run.Player.TryTakeDamage(10000);yield return Wait(3);
        Check(AccountProgression.Current.Gold==gold+expected&&AccountProgression.Current.LastReward.runId==oldId,"death credits completed-wave payout exactly once");
        yield return Capture("11-death-rewards");Click("QuitToMenu");yield return Wait(1);
        Check(SceneManager.GetActiveScene().name=="MainMenu"&&Time.timeScale==1,"death Quit restores scene/time");
        Check(AccountProgression.Current.Gold==gold+expected,"death Quit does not duplicate payout");
    }
    private static IEnumerator WaitForUsableBomb(RunSession run)
    {
        float end=Time.realtimeSinceStartup+35;
        while(!run.CanUse(ConsumableKind.Bomb))
        {
            SelectCard();Check(Time.realtimeSinceStartup<end&&!run.IsFinished,"Bomb becomes usable after cooldown/transition");yield return null;
        }
    }
    private static IEnumerator PlayToWave(RunSession run,int target)
    {
        float end=Time.realtimeSinceStartup+180,next=0;
        while(run.Waves.CurrentWave<target)
        {
            Check(!run.IsFinished&&Time.realtimeSinceStartup<end,"reach wave "+target+" through real swaps");
            SelectCard();
            if(Time.time>=next&&TryMove(run.Board)){next=Time.time+.8f;}
            yield return null;
        }
        yield return Settle(run.Board);
    }
    private static bool TryMove(BoardController board)
    {
        if(board==null||board.IsBusy||board.IsExternalInputBlocked||!board.TryGetRandomHintMove(out var a,out var b))return false;
        board.StartCoroutine((IEnumerator)typeof(BoardController).GetMethod("TrySwap",Flags).Invoke(board,new object[]{a,b}));return true;
    }
    private static void SelectCard()
    {
        var choice=UnityEngine.Object.FindFirstObjectByType<UpgradeChoiceUI>();
        if(choice==null||!choice.IsOpen)return;
        var views=(UpgradeCardView[])typeof(UpgradeChoiceUI).GetField("cardViews",Flags).GetValue(choice);
        var button=(Button)typeof(UpgradeCardView).GetField("button",Flags).GetValue(views[0]);
        if(button.interactable)button.onClick.Invoke();
    }
    private static IEnumerator WaitForRun()
    {
        float end=Time.realtimeSinceStartup+30;
        while(RunSession.Current==null||RunSession.Current.Player==null||!RunSession.Current.Player.IsInitialized||!RunSession.Current.Waves.IsWaveActive||RunSession.Current.Board.IsBusy)
        {Check(Time.realtimeSinceStartup<end,"Game scene initializes");yield return null;}
        yield return Wait(.5f);
    }
    private static IEnumerator Settle(BoardController board)
    {float end=Time.realtimeSinceStartup+30;while(board.IsBusy||board.HasPendingBoardMutation){Check(Time.realtimeSinceStartup<end,"board settles");yield return null;}}
    private static IEnumerator Wait(float seconds)
    {float end=Time.realtimeSinceStartup+seconds;while(Time.realtimeSinceStartup<end)yield return null;}
    private static IEnumerator Capture(string name)
    {
        yield return Wait(.7f);ScreenCapture.CaptureScreenshot(Path.Combine(output,name+".png"));yield return Wait(.3f);
        report.AppendLine("Captured "+name+" "+Screen.width+"x"+Screen.height+"; visual review required.");Flush();
    }
    private static void AuditControlBounds()
    {
        Rect safe=GameplayPixelLayoutController.ValidationSafeArea??Screen.safeArea;
        foreach(string name in new[]{"Settings","HealthPotion","Bomb"})
        {
            var button=UnityEngine.Object.FindObjectsByType<Button>(FindObjectsSortMode.None).Single(b=>b.name==name);
            var corners=new Vector3[4];((RectTransform)button.transform).GetWorldCorners(corners);
            foreach(var corner in corners)Check(safe.Contains(RectTransformUtility.WorldToScreenPoint(null,corner)),name+" inside physical safe area");
        }
    }
    private static Gem At(BoardController board,int x,int y)=>((Gem[,])typeof(BoardController).GetField("gems",Flags).GetValue(board))[x,y];
    private static void Click(string name)
    {
        var buttons=UnityEngine.Object.FindObjectsByType<Button>(FindObjectsSortMode.None).Where(b=>b.name==name&&b.isActiveAndEnabled).ToArray();
        Check(buttons.Length==1&&buttons[0].interactable,"unique enabled button: "+name);buttons[0].onClick.Invoke();
    }
    private static void SetSize(Vector2Int size)=>typeof(GameplayPixelLayoutTests).GetMethod("SetGameViewSize",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,new object[]{size});
    private static readonly HashSet<string> passed = new HashSet<string>();
    private static void Check(bool success,string label){if(!success)throw new Exception(label);if(passed.Add(label)){report.AppendLine("PASS "+label);Flush();}}
    private static void Flush(){if(output!=null)File.WriteAllText(Path.Combine(output,"report.txt"),report.ToString());}
    private static void Restore()
    {
        AudioPreferences.SetMusicMuted(priorMusic);AudioPreferences.SetSfxMuted(priorSfx);
        GameplayPixelLayoutController.ValidationSafeArea=null;
        mastery?.Dispose();selection?.Dispose();profile?.Dispose();
        SessionState.SetBool(Pending,false);
    }
    private static void Finish(bool success)
    {
        EditorApplication.update-=Tick;steps.Clear();report.AppendLine(success?"INTEGRATION PASSED":"INTEGRATION FAILED");Flush();
        // Leave the scene visible for inspection. Stop the opt-in test explicitly.
        Debug.Log("Balance integration "+(success?"PASSED":"FAILED")+". Evidence: "+output);
    }
}
