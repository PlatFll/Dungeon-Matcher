using System.Collections;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

public sealed partial class ForestFoundationPlayTests
{
    [UnityTest] public IEnumerator IronveinWholeRosterContactsAndPauseUseTheirNativeFrames()
    {
        // Each native basic is observed on the first turn of its own encounter.
        // A specialist already preparing a cast must legitimately withhold its basic.
        foreach(string id in IronveinArtImporter.Ids)
        {
            if(Run!=null)
            {
                Assert.That(Run.ExitTo("MainMenu"),Is.True);yield return null;
            }
            yield return LaunchMineKit(id);
            var actor=Enemy(id);var attack=actor.GetComponent<EnemyAutoAttack>();
            var animator=actor.transform.Find("VisualRoot").GetComponent<Animator>();
            var portrait=animator.GetComponent<Image>();
            var clip=actor.Definition.AnimationControllerOverride.animationClips.Single(c=>c.name==id+"_AutoAttack");
            var impact=AnimationUtility.GetAnimationEvents(clip).Single(e=>e.functionName=="AutoAttackImpact").time;
            var expected=AnimationUtility.GetObjectReferenceCurve(clip,AnimationUtility.GetObjectReferenceCurveBindings(clip).Single())
                .Last(k=>k.time<=impact+.001f).value;
            int hits=0;
            void ObserveContact(EnemyAutoAttack source,int damage,bool followup)
            {
                hits++;Assert.That(portrait.sprite,Is.EqualTo(expected),id+" actual contact sprite");
            }
            attack.AttackResolved+=ObserveContact;
            Run.Player.RestoreToFullHealth();
            actor.SetSpecialTurnRequirement(999);
            actor.ResetSpecialCounter();
            Set(attack,"unifiedRemaining",1);
            Assert.That(attack.PerformAttackImmediately(),Is.False,id+" waits for a player move");
            PrepareSafeMove();
            var board=Run.Board;
            int previousMoves=board.CompletedValidPlayerMoves;
            var from=board.GetGem(safeMoveFrom.x,safeMoveFrom.y);
            var to=board.GetGem(safeMoveTo.x,safeMoveTo.y);
            board.StartCoroutine((IEnumerator)Call(board,"TrySwap",from,to));
            yield return Until(()=>attack.IsAttackSequenceInProgress ||
                (board.CompletedValidPlayerMoves==previousMoves+1 && Run.Continuation.CanCapture),id+" accepts the diagnostic move");
            Assert.That(attack.IsAttackSequenceInProgress,Is.True,
                $"{id} native windup: move {board.CompletedValidPlayerMoves}, hits {hits}, remaining {attack.RemainingAttackTime}, running {attack.IsRunning}, stagger {attack.IsPausedByStagger}, action pause {attack.IsPausedByAction}");
            Time.timeScale=0;var pose=portrait.sprite;
            yield return new WaitForSecondsRealtime(.08f);
            Assert.That(portrait.sprite,Is.EqualTo(pose),id+" pauses with combat");
            Assert.That(hits,Is.Zero,id+" paused windup cannot hit");Time.timeScale=1;
            // This fixture owns the one settlement waiter, including all actors in the move.
            yield return Until(()=>Run.Board.CompletedValidPlayerMoves==previousMoves+1 &&
                Run.Continuation.CanCapture && !Run.MoveClock.IsResolving,id+" player move and all actors settle");
            attack.AttackResolved-=ObserveContact;
            Assert.That(hits,Is.EqualTo(1),id);
            // Completion is emitted 10 ms before the clip end; two Editor
            // frames need not span that interval on a fast test machine.
            float recoveryDeadline=Time.time+.25f;
            while(!animator.GetCurrentAnimatorStateInfo(0).IsName("Idle") && Time.time<recoveryDeadline)
                yield return null;
            Assert.That(animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"),Is.True,id+" returns after authored recovery");
        }
    }

    [UnityTest] public IEnumerator IronveinPoweredSpriteContactHitsOnceThenReturnsToNormal()
    {
        yield return LaunchMineKit("pickaxe_delver","ore_hauler","packbeetle");QuietKitFixture();
        var actor=Enemy("pickaxe_delver");var attack=actor.GetComponent<EnemyAutoAttack>();
        var animator=actor.transform.Find("VisualRoot").GetComponent<Animator>();
        var portrait=animator.GetComponent<Image>();var ore=actor.GetComponent<EnemyOrePower>();
        var dir=Path.GetFullPath(".utmp/Ironvein/Captures");Directory.CreateDirectory(dir);
        int hits=0;string expected="OreChargedAutoAttack";int frame=4;
        attack.AttackResolved+=(_,amount,__)=>
        {
            hits++;
            Assert.That(animator.GetCurrentAnimatorStateInfo(0).IsName(expected),Is.True);
            Assert.That(portrait.sprite.name,Is.EqualTo("pickaxe_delver_"+expected+"_"+frame.ToString("00")),"damage follows the displayed contact pose");
            ScreenCapture.CaptureScreenshot(Path.Combine(dir,"phase08-"+expected+"-contact.png"));
        };
        Assert.That(ore.Grant(),Is.True);
        actor.SetSpecialTurnRequirement(999);
        Set(attack,"unifiedRemaining",1);
        yield return EnvironmentMove();
        Assert.That(attack.IsAttackSequenceInProgress,Is.False,"powered native clip completes");
        Assert.That(hits,Is.EqualTo(1));Assert.That(ore.IsPowered,Is.False);
        yield return Until(()=>animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"),"powered recovery returns to Idle");
        expected="AutoAttack";frame=3;
        Set(attack,"unifiedRemaining",1);
        yield return EnvironmentMove();
        Assert.That(attack.IsAttackSequenceInProgress,Is.False,"ordinary native clip completes");
        Assert.That(hits,Is.EqualTo(2));
        var layout=Object.FindFirstObjectByType<GameplayPixelLayoutController>();
        Assert.That(GameplayPixelLayoutValidator.Validate(layout,out var report),Is.Empty,report);
    }
}
