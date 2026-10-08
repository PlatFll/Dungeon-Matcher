using System.Collections;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

public sealed partial class ForestFoundationPlayTests
{
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
