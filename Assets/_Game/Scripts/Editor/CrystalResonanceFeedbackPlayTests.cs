#if UNITY_EDITOR
using System.Collections;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

public sealed partial class ForestFoundationPlayTests
{
    // Injected presentation state and controlled legal boards, never natural pacing evidence.
    [UnityTest] public IEnumerator CrystalResonanceFeedbackShowsNormalExcellentAndPendingInAllZones()
    {
        string dir=Path.GetFullPath(".utmp/CrystalResonance/Refinement/Feedback");Directory.CreateDirectory(dir);
        bool reduced=PresentationPreferences.ReducedMotion;
        try
        {
            PresentationPreferences.SetReducedMotion(false);
            foreach(string zone in new[]{"dungeon","magical-forest","drowned-court","ironvein-excavation","frostwake-peaks"})
            {
                yield return LaunchUnified(zone);PrepareSafeMove();var board=Run.Board;
                var hud=Object.FindFirstObjectByType<CrystalResonanceHud>();
                var state=Run.Resonance.State.Copy();state.charge=8;Run.Resonance.Restore(state);
                int before=board.CompletedValidPlayerMoves;
                board.ReplayPlayerSwap(safeMoveFrom.x,safeMoveFrom.y,safeMoveTo.x,safeMoveTo.y);
                yield return Until(()=>board.CompletedValidPlayerMoves==before+1&&Run.Continuation.CanCapture,"normal match settles");
                Assert.That(Run.Resonance.Pending,Is.Zero,"normal three does not create deliberate mastery");
                Assert.That(hud.GetComponentsInChildren<UIPixelParticle>(true).Any(m=>m.IsPlaying),Is.False,"normal score has restrained feedback");
                ScreenCapture.CaptureScreenshot(Path.Combine(dir,zone+"-normal.png"));yield return null;yield return null;

                PrepareSafeMove();var sprites=(Sprite[])Get(board,"gemSprites");
                var type=board.GetGem(safeMoveTo.x-1,safeMoveTo.y).Type;
                board.GetGem(safeMoveTo.x+1,safeMoveTo.y).SetType(type,sprites[(int)type]);
                bool earned=false;System.Action<ResonanceMastery> record=_=>earned=true;Run.Resonance.MasteryEarned+=record;
                before=board.CompletedValidPlayerMoves;
                board.ReplayPlayerSwap(safeMoveFrom.x,safeMoveFrom.y,safeMoveTo.x,safeMoveTo.y);
                yield return Until(()=>earned,"actual intentional four emits feedback");yield return null;
                var notice=hud.transform.Find("ResonanceFeedback").GetComponent<TMPro.TMP_Text>();
                Assert.That(notice.text,Does.Contain("+1 RESONANCE"));
                Assert.That(hud.transform.Find("ChargeGlint").GetComponent<Image>().enabled,Is.True);
                Assert.That(hud.GetComponentsInChildren<UIPixelParticle>(true).Any(m=>m.IsPlaying),Is.True,"mastery has a native spark");
                ScreenCapture.CaptureScreenshot(Path.Combine(dir,zone+"-excellent-spark.png"));yield return null;yield return null;
                yield return Until(()=>board.CompletedValidPlayerMoves==before+1&&Run.Continuation.CanCapture,"four settles");
                Run.Resonance.MasteryEarned-=record;
                Assert.That(Run.Resonance.Committed,Is.EqualTo(8));Assert.That(Run.Resonance.Pending,Is.EqualTo(1));
                var pending=hud.transform.Find("ResonanceTrack/Provisional").GetComponent<Image>();
                Assert.That(pending.rectTransform.rect.width,Is.GreaterThan(0));
                Color first=pending.color;bool shimmer=false;
                float end=Time.time+.6f;
                while(Time.time<end){shimmer|=pending.color!=first;yield return null;}
                Assert.That(shimmer,Is.True,"pending fill remains distinct and shimmers");
                ScreenCapture.CaptureScreenshot(Path.Combine(dir,zone+"-pending.png"));yield return null;yield return null;
                Assert.That(hud.GetComponentsInChildren<Image>(true).All(i=>!i.raycastTarget),Is.True,"effects cannot intercept input");
                var notch=hud.transform.Find("ResonanceTrack/CardNotch12").GetComponent<Image>();
                first=notch.color;bool anticipation=false;end=Time.time+1.7f;
                while(Time.time<end){anticipation|=notch.color!=first;yield return null;}
                Assert.That(anticipation,Is.True,"nearby unearned notch anticipates the reward");
                PresentationPreferences.SetReducedMotion(true);yield return null;yield return null;
                first=pending.color;yield return new WaitForSeconds(.35f);
                Assert.That(pending.color,Is.EqualTo(first));
                Assert.That(hud.GetComponentsInChildren<UIPixelParticle>(true).Any(m=>m.IsPlaying),Is.False);
                PresentationPreferences.SetReducedMotion(false);
            }
        }
        finally{PresentationPreferences.SetReducedMotion(reduced);}
    }
}
#endif
