using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

public sealed partial class ForestFoundationPlayTests
{
    private BoardCasterSigilView Sigils=>Run.Board.GetComponent<BoardCasterSigilView>();
    private EnemyActor SlotActor(int slot)=>Run.Waves.ContinuationEnemy(slot);
    private static void SetThreatProperty(object threat,string property,object value)=>
        threat.GetType().GetProperty(property).SetValue(threat,value);
    private string[] CasterTargetSignature()
    {
        var targets=new List<CasterBoardTarget>();Run.Board.CollectCasterTargets(targets);
        return targets.Select(t=>$"{Run.Waves.ContinuationSlot(t.Owner)}:{t.Kind}:{t.Gem?.BoardIdentity??0}:{t.Cell}:{t.Lane}").OrderBy(s=>s).ToArray();
    }
    private void AssertSlotBadges()
    {
        for(int slot=0;slot<3;slot++)
        {
            var actor=SlotActor(slot);if(actor==null || actor.IsDefeated)continue;
            var view=actor.GetComponentInParent<EnemySlotUI>().GetComponent<EnemySlotSigilView>();
            Assert.That(view.SlotIndex,Is.EqualTo(slot));
            var icon=view.transform.Find("CasterSlotSigil").GetComponent<Image>();
            Assert.That(icon.enabled,Is.True);Assert.That(icon.sprite,Is.SameAs(CasterSigilArt.ForSlot(slot)));
        }
    }

    [UnityTest] public IEnumerator SigilsStackFollowGemFallKeepCellsAndLanesAndResumeAcrossPortraits()
    {
        yield return LaunchCourt("reef_spearman","hammerhead_bruiser","needlefin_skirmisher");
        var board=Run.Board;
        BoardController.GemSetThreat left=null,middle=null;
        BoardController.CellResponseThreat right=null;BoardController.LaneThreat lanes=null;
        Assert.That(board.TryQueueMarkGemSet(SlotActor(0),1,3,false,t=>left=t,null),Is.True);yield return Stable();
        Assert.That(board.TryQueueMarkGemSet(SlotActor(1),1,3,true,t=>middle=t,null),Is.True);yield return Stable();
        Assert.That(board.TryQueueCellResponse(SlotActor(2),1,3,false,t=>right=t),Is.True);yield return Stable();
        Assert.That(board.TryQueueMarkLanes(SlotActor(2),3,t=>lanes=t,null),Is.True);yield return Stable();
        Assert.That(left,Is.Not.Null);Assert.That(middle,Is.Not.Null);Assert.That(right,Is.Not.Null);Assert.That(lanes,Is.Not.Null);
        // Real targeting usually avoids overlap. Exercise the supported overlap
        // case without weakening that target-selection fairness rule.
        var gem=board.GetGem(2,4);var fixedCell=new Vector2Int(2,4);
        left.Targets.Clear();left.Targets.Add(gem);middle.Targets.Clear();middle.Targets.Add(gem);
        right.Cells.Clear();right.Cells.Add(fixedCell);
        SetThreatProperty(lanes,"Row",2);SetThreatProperty(lanes,"Column",5);
        yield return null;yield return null;
        Assert.That(Sigils.ActiveCount,Is.EqualTo(5));AssertSlotBadges();
        var stacked=Sigils.Images.Where(r=>r.enabled).Take(3).ToArray();
        Assert.That(stacked.Select(r=>r.name),Is.EqualTo(new[]{"CasterSigil_0","CasterSigil_1","CasterSigil_2"}));
        Assert.That(stacked[0].transform.localPosition.x,Is.LessThan(stacked[1].transform.localPosition.x));
        Assert.That(stacked[1].transform.localPosition.x,Is.LessThan(stacked[2].transform.localPosition.x));

        // Open a gap below the marked gem and use production gravity, including
        // its in-flight transform. The cell target must not follow that fall.
        var grid=(Gem[,])Get(board,"gems");var removed=grid[2,0];grid[2,0]=null;Object.Destroy(removed.gameObject);
        bool fallen=false;var fall=Run.StartCoroutine(FinishFall());
        IEnumerator FinishFall(){yield return board.StartCoroutine((IEnumerator)Call(board,"CollapseAndRefillBoard"));fallen=true;}
        while(!fallen)
        {
            yield return null;
            var slotZero=Sigils.Images.First(r=>r.enabled && r.name=="CasterSigil_0");
            var world=gem.transform.position+board.transform.TransformVector(Vector3.up*board.CellSize*.32f);
            float targetY=Camera.main.WorldToScreenPoint(world).y;
            Assert.That(GameplayPixelLayoutController.ScreenRect(slotZero.rectTransform).center.y,Is.EqualTo(targetY).Within(4),"sigil follows the falling gem");
        }
        yield return fall;yield return null;
        Assert.That(gem.Row,Is.EqualTo(3));Assert.That(right.Cells,Does.Contain(fixedCell));
        var cellPosition=board.CasterTargetLocalPosition(CasterBoardTarget.OnCell(SlotActor(2),fixedCell));
        Assert.That(cellPosition,Is.EqualTo(board.GetCellLocalPosition(2,4)));
        Assert.That(Sigils.ActiveCount,Is.EqualTo(5));

        // Rejoin the stack for the portrait evidence; all seven status cells and
        // the separate player shield track are present at the same time.
        right.Cells.Clear();right.Cells.Add(new Vector2Int(gem.Column,gem.Row));
        // Rebuild WHEN views after the fixture's forced target overlap. Normal
        // gameplay selects coordinates before creating its telegraphs.
        Object.Destroy(board.GetComponent<BoardTelegraphVFX>());yield return null;
        var telegraph=board.gameObject.AddComponent<BoardTelegraphVFX>();
        Call(telegraph,"ShowMarks",left);Call(telegraph,"ShowMarks",middle);
        Call(telegraph,"ShowCellResponse",right);Call(telegraph,"ShowLanes",lanes);
        foreach(var data in Resources.LoadAll<PlayerStatusDefinition>("PlayerStatuses"))Run.Player.Statuses.Apply(data,SlotActor(0));
        Run.Player.GrantShield(30);
        float settled=Time.time+1.5f;yield return Until(()=>Time.time>=settled,"transient numbers finish");
        Time.timeScale=0;
        string output=Path.GetFullPath(".utmp/StatusRevision/Visual");Directory.CreateDirectory(output);
        try
        {
            int variant=0;
            foreach(var size in new[]{new Vector2Int(720,1280),new Vector2Int(1080,1920),new Vector2Int(1080,2400),new Vector2Int(1080,1920)})
            {
                bool inset=variant++==3;
                typeof(GameplayPixelLayoutTests).GetMethod("SetGameViewSize",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,new object[]{size});
                GameplayPixelLayoutController.ValidationSafeArea=inset?new Rect(32,60,1016,1772):(Rect?)null;
                yield return Until(()=>Screen.width==size.x && Screen.height==size.y,"sigil viewport");
                for(int frame=0;frame<10;frame++)yield return null;
                Assert.That(Sigils.ActiveCount,Is.EqualTo(5));AssertSlotBadges();
                foreach(var r in Sigils.Images.Where(r=>r.enabled))
                {
                    var bounds=GameplayPixelLayoutController.ScreenRect(r.rectTransform);
                    Assert.That(bounds.xMin,Is.GreaterThanOrEqualTo(0));Assert.That(bounds.xMax,Is.LessThanOrEqualTo(Screen.width));
                    Assert.That(bounds.yMin,Is.GreaterThanOrEqualTo(0));Assert.That(bounds.yMax,Is.LessThanOrEqualTo(Screen.height));
                    var layer=r.GetComponentInParent<Canvas>();
                    Assert.That(layer.overrideSorting,Is.True);Assert.That(layer.sortingOrder,Is.GreaterThan(layer.rootCanvas.sortingOrder),"outside lane sigils draw above the frame");
                }
                var layout=Object.FindFirstObjectByType<GameplayPixelLayoutController>();
                var errors=GameplayPixelLayoutValidator.Validate(layout,out string report);Assert.That(errors,Is.Empty);
                string name="sigils-"+size.x+"x"+size.y+(inset?"-safe":"");
                File.WriteAllText(Path.Combine(output,name+".txt"),report);
                ScreenCapture.CaptureScreenshot(Path.Combine(output,name+".png"));yield return null;yield return null;
            }
        }
        finally {GameplayPixelLayoutController.ValidationSafeArea=null;Time.timeScale=1;}
        yield return Stable();var signature=CasterTargetSignature();
        yield return ResumeCourtCheckpoint();yield return null;yield return null;
        Assert.That(CasterTargetSignature(),Is.EqualTo(signature));Assert.That(Sigils.ActiveCount,Is.EqualTo(5));AssertSlotBadges();
        Assert.That(Run.Board.GetComponents<BoardCasterSigilView>().Length,Is.EqualTo(1));
    }

    [UnityTest] public IEnumerator SigilsCancelOnDeathReplacementInheritsSlotAndResolutionFinishes()
    {
        yield return LaunchCourt("reef_spearman","hammerhead_bruiser","needlefin_skirmisher");
        var board=Run.Board;var middle=SlotActor(1);var definition=middle.Definition;
        var middleSlot=middle.GetComponentInParent<EnemySlotUI>();
        BoardController.GemSetThreat set=null;BoardController.GemPairThreat pair=null;
        board.TryQueueMarkGemSet(middle,3,2,false,t=>set=t,null);yield return Stable();
        board.TryQueueMarkGemPair(SlotActor(0),2,t=>pair=t);yield return Stable();
        yield return null;Assert.That(Sigils.ActiveCount,Is.EqualTo(5));
        middle.ResolveDirectDamage(99999);yield return Stable();yield return null;
        Assert.That(Sigils.Images.Where(r=>r.enabled).All(r=>r.name=="CasterSigil_0"),Is.True);
        yield return Until(()=>middleSlot.CurrentEnemy==null,"dead slot releases");
        Assert.That(Run.Waves.TrySummonEnemy(definition,out var replacement),Is.True);yield return Stable();
        replacement.GetComponent<EnemyAutoAttack>().SetActionPaused(this,true);
        Assert.That(Run.Waves.ContinuationSlot(replacement),Is.EqualTo(1));
        board.CancelGemPairThreat(pair);
        board.TryQueueMarkGemSet(replacement,3,2,false,t=>set=t,null);yield return Stable();
        yield return null;AssertSlotBadges();Assert.That(Sigils.ActiveCount,Is.EqualTo(3));
        SetThreatProperty(set,"DueMove",board.CompletedValidPlayerMoves);
        bool done=false;
        Assert.That(board.TryQueueResolveGemSet(set,null,_=>done=true,null),Is.True);
        bool sawConsumed=false;
        while(!done)
        {
            yield return null;
            if(!set.Ended || !set.Targets.Any(board.IsEnvironmentalOrdinaryGem))continue;
            sawConsumed=true;var targets=new List<CasterBoardTarget>();board.CollectCasterTargets(targets);
            Assert.That(targets.Any(t=>t.Owner==replacement),Is.True,"consumed warning keeps identity throughout sequential clear");
        }
        yield return Stable();yield return null;
        Assert.That(sawConsumed,Is.True);Assert.That(Sigils.ActiveCount,Is.Zero);
    }

    [UnityTest] public IEnumerator SigilsCourtChannelTargetsInterruptAndRestoreWithoutNewIdentity()
    {
        yield return LaunchCourt("queen_nacre","shellback_porter");
        for(int i=0;i<8;i++)yield return Move();
        var queen=Enemy("queen_nacre");var ability=queen.GetComponent<AquaticEnemyAbility>();
        Assert.That(ability.CastName,Is.EqualTo("DEPTHS"));yield return null;
        Assert.That(Sigils.ActiveCount,Is.EqualTo(2));
        var positions=Sigils.Images.Where(r=>r.enabled).Select(r=>r.transform.localPosition).ToArray();
        yield return ResumeCourtCheckpoint();yield return null;
        queen=Enemy("queen_nacre");ability=queen.GetComponent<AquaticEnemyAbility>();
        Assert.That(Sigils.Images.Where(r=>r.enabled).Select(r=>r.transform.localPosition),Is.EqualTo(positions));
        queen.GetComponent<EnemyStagger>().ApplyStagger(2,2);yield return null;yield return null;
        Assert.That(ability.IsPreparing,Is.False);Assert.That(Sigils.ActiveCount,Is.Zero);
    }
}
