using System.Collections;
using System.IO;
using System.Linq;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.TestTools;

public sealed partial class ForestFoundationPlayTests
{
    private void AssertRosterCountersClearControls()
    {
        var ui=Run.GetComponent<RunControlsUI>();
        var buttons=ui.GameplayCanvas.GetComponentsInChildren<RectTransform>()
            .Where(r=>r.name=="Settings"||r.name=="CombatGuide").ToArray();
        foreach(var actor in Run.Waves.ActiveEnemies.Where(e=>e!=null&&!e.IsDefeated))
        {
            var slot=actor.GetComponentInParent<EnemySlotUI>();
            var labels=slot.GetComponentsInChildren<TMP_Text>().Where(t=>t.enabled &&
                (t.name=="SpecialAbilityCounter"||t.name=="MoveIntent"||t.name=="EnemyBuffs")).ToArray();
            foreach(var label in labels)
            {
                label.ForceMeshUpdate();var bounds=RenderedTextBounds(label);
                Assert.That(label.isTextOverflowing,Is.False,label.name);
                Assert.That(label.preferredWidth,Is.LessThanOrEqualTo(label.rectTransform.rect.width+.1f),label.name+" native glyph width");
                Assert.That(label.canvasRenderer.cull,Is.False,label.name);
                if(label.name=="EnemyBuffs")
                {
                    var gem=slot.GetComponentInChildren<EnemyWeaknessIndicatorUI>().transform as RectTransform;
                    float edge=gem.TransformPoint(new Vector3(gem.rect.xMax,0,0)).x;
                    Assert.That(bounds.xMin,Is.GreaterThan(edge),"Buff must not cover weakness pixels");
                }
                foreach(var button in buttons)
                {
                    var corners=new Vector3[4];button.GetWorldCorners(corners);
                    var rect=Rect.MinMaxRect(corners[0].x,corners[0].y,corners[2].x,corners[2].y);
                    Assert.That(bounds.Overlaps(rect),Is.False,$"{actor.Definition.EnemyId} {label.name} {bounds} overlaps {button.name} {rect}");
                }
            }
        }
    }
    [UnityTest] public IEnumerator RosterFinalCourtPortraits() {yield return FinalCourtPortraits();}
    private IEnumerator FinalCourtPortraits()
    {
        yield return TributeFixture("shellback_porter","reef_spearman","queen_nacre");
        foreach(var enemy in Run.Waves.ActiveEnemies)enemy.GrantFortified(2);
        var board=Run.Board;
        string folder=Path.GetFullPath(".utmp/RosterEndless/Court");Directory.CreateDirectory(folder);
        var layout=UnityEngine.Object.FindFirstObjectByType<GameplayPixelLayoutController>();
        Time.timeScale=0;
        try
        {
            var sizes=new[]{new Vector2Int(720,1280),new Vector2Int(1080,1920),new Vector2Int(1080,2400),new Vector2Int(1080,2400)};
            for(int view=0;view<sizes.Length;view++)
            {
                var size=sizes[view];bool inset=view==3;
                typeof(GameplayPixelLayoutTests).GetMethod("SetGameViewSize",System.Reflection.BindingFlags.Static|System.Reflection.BindingFlags.NonPublic).Invoke(null,new object[]{size});
                GameplayPixelLayoutController.ValidationSafeArea=inset?new Rect(32,72,1016,2240):(Rect?)null;
                yield return Until(()=>Screen.width==size.x&&Screen.height==size.y,"Court final portrait size");
                layout.Refresh(true);for(int i=0;i<8;i++)yield return null;
                Assert.That(GameplayPixelLayoutValidator.Validate(layout,out string report),Is.Empty,report);
                ScreenCapture.CaptureScreenshot(Path.Combine(folder,"tribute-fortified-"+(inset?"safe-inset":size.x+"x"+size.y)+".png"));
                yield return null;yield return null;
                AssertRosterCountersClearControls();
                foreach(var label in UnityEngine.Object.FindObjectsByType<TMP_Text>(FindObjectsSortMode.None).Where(t=>t.name=="EnemyBuffs"&&t.gameObject.activeInHierarchy))
                {Assert.That(label.text,Is.EqualTo("FORTIFIED 2"));Assert.That(label.textInfo.characterInfo.Count(c=>c.isVisible),Is.EqualTo(10));}
            }
        }
        finally{GameplayPixelLayoutController.ValidationSafeArea=null;Time.timeScale=1;}
    }
    [UnityTest] public IEnumerator RosterFinalCofferWakeCaptures() {yield return FinalCofferWake();}
    private IEnumerator FinalCofferWake()
    {
        yield return RoyalCofferFixture();var board=Run.Board;
        string folder=Path.GetFullPath(".utmp/RosterEndless/Court");Directory.CreateDirectory(folder);
        ScreenCapture.CaptureScreenshot(Path.Combine(folder,"royal-armored.png"));yield return null;yield return null;
        var coffer=board.Aquatic.coffer;
        var hit=new[]{Vector2Int.left,Vector2Int.right,Vector2Int.up,Vector2Int.down}
            .Select(d=>board.GetGem(coffer.x+d.x,coffer.y+d.y)).First(g=>g!=null);
        bool moved=false;board.CofferMoving+=(_,__,___)=>moved=true;
        Assert.That(board.TryClearPlayerArea(hit,0,()=>true),Is.True);
        yield return Until(()=>moved,"coffer begins rotation");
        string motion=Path.Combine(folder,"Motion");Directory.CreateDirectory(motion);
        for(int frame=0;frame<18;frame++)
        {
            ScreenCapture.CaptureScreenshot(Path.Combine(motion,frame.ToString("D2")+".png"));
            yield return new WaitForSeconds(.025f);
        }
        yield return Stable();
        ScreenCapture.CaptureScreenshot(Path.Combine(folder,"royal-exposed.png"));yield return null;yield return null;
        var caption=board.GetComponentsInChildren<TextMeshPro>().Single(t=>t.name=="coffer");
        Assert.That(caption.textBounds.size.y*caption.transform.localScale.y,Is.LessThanOrEqualTo(board.CellSize*.21f));
    }
}
