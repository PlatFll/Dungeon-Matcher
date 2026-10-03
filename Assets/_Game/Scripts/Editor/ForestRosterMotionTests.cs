using System;
using System.Collections;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

public sealed class ForestRosterMotionTests
{
    [Test] public void EveryApprovedActorHasNativeMotionAndExactlyOneBasicContact()
    {
        foreach(string name in ForestRosterImporter.Names)
        {
            var enemy=AssetDatabase.LoadAssetAtPath<EnemyDefinition>("Assets/_Game/Data/Enemies/Forest/"+name+".asset");
            var clips=enemy.AnimationControllerOverride.animationClips;
            foreach(string state in new[]{"Idle","AutoAttack","Hit","Death"})
                Assert.That(clips.Count(c=>c.name==name+"_"+state),Is.EqualTo(1));
            var attack=clips.Single(c=>c.name.EndsWith("_AutoAttack"));
            var events=AnimationUtility.GetAnimationEvents(attack);
            Assert.That(events.Count(e=>e.functionName=="AutoAttackImpact"),Is.EqualTo(1));
            Assert.That(events.Count(e=>e.functionName=="AutoAttackComplete"),Is.EqualTo(1));
            Assert.That(events.Single(e=>e.functionName=="AutoAttackImpact").time,Is.LessThan(attack.length));
            foreach(var clip in clips)
            {
                var curve=AnimationUtility.GetObjectReferenceCurve(clip,AnimationUtility.GetObjectReferenceCurveBindings(clip).Single());
                foreach(var key in curve)
                {
                    var sprite=(Sprite)key.value;
                    Assert.That(sprite.rect.size,Is.EqualTo(name=="Ancient_Treant"?new Vector2(128,112):new Vector2(96,80)));
                    Assert.That(sprite.texture.filterMode,Is.EqualTo(FilterMode.Point));
                }
            }
            if(name=="Briar_Archer"||name=="Ancient_Treant")
                foreach(string state in new[]{"ChannelStart","ChannelHold","Release"})
                    Assert.That(clips.Any(c=>c.name.EndsWith("_"+state)),Is.True,name+state);
            else if(name!="Snapvine") Assert.That(clips.Any(c=>c.name.EndsWith("_Ability")),Is.True);
        }
    }
}

public sealed partial class ForestFoundationPlayTests
{
    private IEnumerator CaptureRoster(string label)
    {
        string dir=Path.GetFullPath(".utmp/ForestValidation/RosterVisual");Directory.CreateDirectory(dir);
        float began=Time.time;yield return Until(()=>Time.time>=began+.5f,"visual transients settle");
        ScreenCapture.CaptureScreenshot(Path.Combine(dir,label+".png"));yield return null;yield return null;
    }
    [UnityTest] public IEnumerator RosterSelectedBlockersBindExactChoicesAndThornArtFacesSafeSide()
    {
        yield return Launch(13,true);PreserveRoster();PrepareSafeMove();
        var board=Run.Board;var owner=Enemy("elven_thornkeeper");
        foreach(var pair in new[]{("woodenBarricadeSprite","Wood_A"),("stoneBarricadeSprite","Stone_B"),
            ("pinnedGemOverlaySprite","Chain_A"),("thornBarricadeSprite","Thorn_A")})
            Assert.That(((Sprite)Get(board,pair.Item1)).name,Is.EqualTo(pair.Item2));
        var theme=GameplayThemeSkin.Current;
        Assert.That(theme.rootLevelOne.name,Is.EqualTo("Root1_B"));Assert.That(theme.rootLevelTwo.name,Is.EqualTo("Root2_B"));
        Assert.That(theme.vineOverlay.name,Is.EqualTo("Vines_B"));
        board.TryQueuePlaceBarricades(owner,1,2,1,EnemyBarricadeStyle.Thorn,false,true);yield return Stable();
        var barrier=board.CaptureContinuation(Run.Waves.ContinuationOwnerSlot).cells.Single(c=>c.barricade);
        var dirs=new[]{Vector2Int.left,Vector2Int.right,Vector2Int.down,Vector2Int.up};
        var safe=dirs[Enumerable.Range(0,4).Single(i=>(barrier.thornSafeSide&(1<<i))!=0)];
        var view=board.GetComponentsInChildren<SpriteRenderer>().Single(r=>r.sprite!=null&&r.sprite.name=="Thorn_A");
        Assert.That(Vector2.Dot(view.transform.up,safe),Is.GreaterThan(.99f),"smooth sprite top faces the safe side");
        foreach(var edge in view.GetComponentsInChildren<SpriteRenderer>().Where(r=>r!=view))
            Assert.That(Mathf.Max(edge.bounds.size.x,edge.bounds.size.y),Is.LessThan(board.CellSize*.7f),"edge marker stays inside one tile");
        Assert.That(board.TryQueuePlantRoots(Enemy("elven_scout"),1,2,true,true,null),Is.True);yield return Stable();
        Assert.That(board.VineCount,Is.GreaterThan(0));
        yield return CaptureRoster("selected-thorn-roots-vines");
    }
    [UnityTest] public IEnumerator RosterAuthoredThornkeeperBasic() => CheckRosterBasic(0);
    [UnityTest] public IEnumerator RosterAuthoredBerserkerBasic() => CheckRosterBasic(1);
    [UnityTest] public IEnumerator RosterAuthoredBloomcallerBasic() => CheckRosterBasic(2);
    [UnityTest] public IEnumerator RosterAuthoredSnapvineBasic() => CheckRosterBasic(3);
    [UnityTest] public IEnumerator RosterAuthoredDrummerBasic() => CheckRosterBasic(4);
    [UnityTest] public IEnumerator RosterAuthoredArcherBasic() => CheckRosterBasic(5);
    [UnityTest] public IEnumerator RosterAuthoredTreantBasic() => CheckRosterBasic(6);
    private IEnumerator CheckRosterBasic(int i)
    {
        Assert.That(SystemInfo.graphicsDeviceType,Is.Not.EqualTo(UnityEngine.Rendering.GraphicsDeviceType.Null));
            yield return Launch(13+i,true);QuietKitFixture();
            var enemy=Enemy(ForestRosterImporter.Names[i].ToLowerInvariant());
            var animator=enemy.transform.Find("VisualRoot").GetComponent<Animator>();
            yield return CaptureRoster("idle-"+enemy.Definition.EnemyId);
            int hits=0;Run.Player.DamageTaken+=(_,n)=>hits++;
            var basic=enemy.GetComponent<EnemyAutoAttack>();Set(basic,"remainingAttackTime",0f);
            yield return Until(()=>enemy.HasAnimationActionInProgress,"authored attack starts");
            yield return Until(()=>hits>0,"authored contact");
            basic.SetActionPaused(this,true);
            yield return Stable();
            Assert.That(hits,Is.EqualTo(1),enemy.name+" exactly one contact");
            float began=Time.time;yield return Until(()=>Time.time>=began+.25f,"basic recovery");
            Assert.That(animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"),Is.True,enemy.name+" recovers");
            var layout=UnityEngine.Object.FindFirstObjectByType<GameplayPixelLayoutController>();
            Assert.That(GameplayPixelLayoutValidator.Validate(layout,out var report),Is.Empty,report);
    }
    [UnityTest] public IEnumerator RosterTreantChannelOwnsBasicTimerAndRecoversAfterRelease()
    {
        yield return Launch(19,true);PreserveRoster();var enemy=Enemy("ancient_treant");
        for(int i=0;i<6;i++) yield return Move();
        var kit=enemy.GetComponent<ForestPressureAbility>();
        Assert.That(kit.IsPreparing,Is.True);Assert.That(kit.ResponseMoves,Is.EqualTo(2));
        var basic=enemy.GetComponent<EnemyAutoAttack>();float remaining=basic.RemainingAttackTime;
        var animator=enemy.transform.Find("VisualRoot").GetComponent<Animator>();
        float began=Time.time;yield return Until(()=>Time.time>=began+.3f,"hold settles");
        Assert.That(animator.GetCurrentAnimatorStateInfo(0).IsName("ChannelHold"),Is.True);
        enemy.ResolveDirectDamage(5);yield return null;
        Assert.That(animator.GetCurrentAnimatorStateInfo(0).IsName("ChannelHold"),Is.True,"ordinary hits preserve held pose");
        Assert.That(basic.RemainingAttackTime,Is.EqualTo(remaining));
        yield return CaptureRoster("treant-channel");
        yield return Move();Assert.That(kit.IsPreparing,Is.True);
        yield return Move();Assert.That(kit.IsPreparing,Is.False);
        Assert.That(basic.IsPausedByAction,Is.False);
        began=Time.time;yield return Until(()=>Time.time>=began+.3f,"release settles");
        Assert.That(animator.GetCurrentAnimatorStateInfo(0).IsName("Idle"),Is.True);
        yield return CaptureRoster("treant-recovered");
    }
}
