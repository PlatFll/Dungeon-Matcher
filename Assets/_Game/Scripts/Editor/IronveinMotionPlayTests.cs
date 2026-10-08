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
        for(int start=0;start<IronveinArtImporter.Ids.Length;start+=3)
        {
            // A new fixture is a fresh attempt. Loading Game alone correctly
            // resumes the previous saved formation, which would miss this batch.
            if(start>0)
            {
                Assert.That(Run.ExitTo("MainMenu"),Is.True);yield return null;
            }
            var ids=IronveinArtImporter.Ids.Skip(start).Take(3).ToArray();
            yield return LaunchMineKit(ids);QuietKitFixture();
            foreach(string id in ids)
            {
                var actor=Enemy(id);var attack=actor.GetComponent<EnemyAutoAttack>();
                var animator=actor.transform.Find("VisualRoot").GetComponent<Animator>();
                var portrait=animator.GetComponent<Image>();
                var clip=actor.Definition.AnimationControllerOverride.animationClips.Single(c=>c.name==id+"_AutoAttack");
                var impact=AnimationUtility.GetAnimationEvents(clip).Single(e=>e.functionName=="AutoAttackImpact").time;
                var expected=AnimationUtility.GetObjectReferenceCurve(clip,AnimationUtility.GetObjectReferenceCurveBindings(clip).Single())
                    .Last(k=>k.time<=impact+.001f).value;
                int hits=0;
                attack.AttackResolved+=(_,__,___)=>{hits++;Assert.That(portrait.sprite,Is.EqualTo(expected),id+" actual contact sprite");};
                Run.Player.RestoreToFullHealth();
                Assert.That(attack.PerformAttackImmediately(),Is.True,id);
                yield return null;
                Time.timeScale=0;var pose=portrait.sprite;
                yield return new WaitForSecondsRealtime(.08f);
                Assert.That(portrait.sprite,Is.EqualTo(pose),id+" pauses with combat");
                Assert.That(hits,Is.Zero,id+" paused windup cannot hit");Time.timeScale=1;
                yield return Until(()=>!attack.IsAttackSequenceInProgress,id+" native basic completes");
                Assert.That(hits,Is.EqualTo(1),id);
                // Completion is emitted 10 ms before the clip end; two Editor
                // frames need not span that interval on a fast test machine.
                float recoveryDeadline=Time.time+.25f;
                while(!animator.GetCurrentAnimatorStateInfo(0).IsName("Idle") && Time.time<recoveryDeadline)
                    yield return null;
                Assert.That(animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"),Is.True,id+" returns after authored recovery");
            }
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
        Assert.That(attack.PerformAttackImmediately(),Is.True);
        yield return Until(()=>!attack.IsAttackSequenceInProgress,"powered native clip completes");
        Assert.That(hits,Is.EqualTo(1));Assert.That(ore.IsPowered,Is.False);
        yield return null;yield return null;
        Assert.That(animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"),Is.True);
        expected="AutoAttack";frame=3;
        Assert.That(attack.PerformAttackImmediately(),Is.True);
        yield return Until(()=>!attack.IsAttackSequenceInProgress,"ordinary native clip completes");
        Assert.That(hits,Is.EqualTo(2));
        var layout=Object.FindFirstObjectByType<GameplayPixelLayoutController>();
        Assert.That(GameplayPixelLayoutValidator.Validate(layout,out var report),Is.Empty,report);
    }
}
