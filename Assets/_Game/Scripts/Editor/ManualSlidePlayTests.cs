using System.Collections;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public sealed partial class ForestFoundationPlayTests
{
    private void SlideFixture()
    {
        var board=Run.Board;var sprites=(Sprite[])Get(board,"gemSprites");
        for(int y=0;y<board.Height;y++)for(int x=0;x<board.Width;x++)
        {
            var gem=board.GetGem(x,y);gem.SetSpecialType(GemSpecialType.None);gem.SetType((GemType)((x+2*y)%6),sprites[(x+2*y)%6]);
        }
        foreach(var enemy in Run.Waves.ActiveEnemies)
        {
            var stats=enemy.RuntimeStats;
            typeof(EnemyActor).GetProperty("RuntimeStats").SetValue(enemy,new EnemyRuntimeStats(stats.Wave,stats.Level,10000,0,0,1000,1000));
            Set(enemy,"currentHealth",10000);
        }
    }
    private void Color(int x,int y,GemType color)
    {Run.Board.GetGem(x,y).SetType(color,((Sprite[])Get(Run.Board,"gemSprites"))[(int)color]);}

    [UnityTest] public IEnumerator SlipperyPreviewRotationSpecialMovementAndSingleAcceptance()
    {
        yield return LaunchCourt("shellback_porter");yield return Move();SlideFixture();
        Color(1,6,GemType.Ruby);Color(2,6,GemType.Amber);Color(3,6,GemType.Emerald);Color(3,4,GemType.Ruby);Color(3,5,GemType.Ruby);
        Run.Board.Aquatic.StartFlood(12,Run.MoveClock.Tick);
        Run.Player.Statuses.Apply(Resources.Load<PlayerStatusDefinition>("PlayerStatuses/Slippery"));yield return null;
        var board=Run.Board;
        var a=board.GetGem(1,6);var b=board.GetGem(2,6);var c=board.GetGem(3,6);b.SetSpecialType(GemSpecialType.RowBomb);
        var plan=board.PreviewManualSwap(a,b);Assert.That(plan.IsExtended,Is.True);Assert.That(plan.IsLegal,Is.True);
        board.BeginPointerGesture(a,new Vector2(100,100),11);board.UpdatePointerGesture(a,new Vector2(180,100),11);
        int before=board.CompletedValidPlayerMoves;
        Assert.That(board.GetGem(1,6),Is.SameAs(a));
        string folder=Path.GetFullPath(".utmp/StatusRevision/Visual");Directory.CreateDirectory(folder);
        yield return null;ScreenCapture.CaptureScreenshot(Path.Combine(folder,"slippery-preview.png"));yield return null;yield return null;
        int accepted=0,completed=0;
        board.ValidPlayerMoveAccepted+=_=>
        {
            accepted++;Assert.That(board.GetGem(3,6),Is.SameAs(a));Assert.That(board.GetGem(1,6),Is.SameAs(b));Assert.That(board.GetGem(2,6),Is.SameAs(c));
        };
        board.ValidPlayerMoveCompleted+=_=>completed++;
        board.EndPointerGesture(a,new Vector2(180,100),11);
        yield return Until(()=>board.CompletedValidPlayerMoves==before+1 && Run.Continuation.CanCapture,"slipped action completes");
        Assert.That(accepted,Is.EqualTo(1));Assert.That(completed,Is.EqualTo(1));Assert.That(board.SwapPreview,Is.Null);
        Assert.That(b.SpecialType,Is.EqualTo(GemSpecialType.RowBomb));Assert.That(board.GetGem(1,6),Is.SameAs(b));
        Assert.That(Run.Player.Statuses.Remaining(PlayerStatusKind.Slippery),Is.EqualTo(2));
    }
    [UnityTest] public IEnumerator SlipperyInvalidFinalArrangementReversesWithoutTakingAMove()
    {
        yield return LaunchCourt("shellback_porter");yield return Move();SlideFixture();
        Color(2,3,GemType.Ruby);Color(2,4,GemType.Emerald);Color(2,5,GemType.Topaz);Color(1,4,GemType.Ruby);Color(3,4,GemType.Ruby);
        Color(5,1,GemType.Ruby);Color(6,1,GemType.Amber);Color(7,1,GemType.Emerald);Color(7,0,GemType.Ruby);Color(7,2,GemType.Ruby);
        var board=Run.Board;var a=board.GetGem(2,3);var b=board.GetGem(2,4);var c=board.GetGem(2,5);
        int before=board.CompletedValidPlayerMoves;
        Assert.That(board.PreviewManualSwap(a,b).IsLegal,Is.True,"Ordinary adjacent result matches");
        Run.Board.Aquatic.StartFlood(12,Run.MoveClock.Tick);Run.Player.Statuses.Apply(Resources.Load<PlayerStatusDefinition>("PlayerStatuses/Slippery"));yield return null;
        Assert.That(board.PreviewManualSwap(a,b).IsLegal,Is.False,"Only the final three-cell result counts");
        board.StartCoroutine((IEnumerator)Call(board,"TrySwap",a,b));
        yield return Until(()=>!board.IsBusy && Run.Continuation.CanCapture,"invalid slide reverses");
        Assert.That(board.CompletedValidPlayerMoves,Is.EqualTo(before));Assert.That(board.GetGem(2,3),Is.SameAs(a));Assert.That(board.GetGem(2,4),Is.SameAs(b));Assert.That(board.GetGem(2,5),Is.SameAs(c));
        Assert.That(Run.Player.Statuses.Remaining(PlayerStatusKind.Slippery),Is.EqualTo(3));
    }
    [UnityTest] public IEnumerator SlipperyRequiresFloodingAndContinuesWithoutSavingAPreview()
    {
        yield return LaunchCourt("shellback_porter");yield return Move();
        var board=Run.Board;Run.Player.Statuses.Apply(Resources.Load<PlayerStatusDefinition>("PlayerStatuses/Slippery"));
        Assert.That(board.PreviewManualSwap(board.GetGem(0,0),board.GetGem(1,0)).IsExtended,Is.False);
        board.Aquatic.StartFlood(12,Run.MoveClock.Tick);yield return null;
        yield return Until(()=>Run.Continuation.CanCapture,"movement rule settles");
        Assert.That(board.PreviewManualSwap(board.GetGem(0,0),board.GetGem(1,0)).IsExtended,Is.True);
        board.ShowManualSwapPreview(board.GetGem(0,0),board.GetGem(1,0));
        Assert.That(Run.Continuation.SaveNow(),Is.True);
        SceneManager.LoadScene("Game");yield return Stable();Run.GetComponent<RunControlsUI>().Close();
        board=Run.Board;
        Assert.That(board.SwapPreview,Is.Null);Assert.That(board.UsesExtraManualSwapStep,Is.True);
        board.Aquatic.phase=TidePhase.Dry;yield return null;
        Assert.That(board.UsesExtraManualSwapStep,Is.False);Assert.That(board.PreviewManualSwap(board.GetGem(0,0),board.GetGem(1,0)).IsExtended,Is.False);
    }
}
