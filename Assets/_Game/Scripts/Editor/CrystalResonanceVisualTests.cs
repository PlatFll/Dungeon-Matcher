#if UNITY_EDITOR
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.TestTools;

// Explicit presentation fixtures; these states never count toward pacing evidence.
public sealed partial class ForestFoundationPlayTests
{
    [UnityTest] public IEnumerator CrystalResonanceHudFiveZonesFivePhoneLayoutsAndMotionModes()
    {
        string dir=Path.GetFullPath(".utmp/CrystalResonance/Visual");Directory.CreateDirectory(dir);
        bool reduced=PresentationPreferences.ReducedMotion;
        var sizes=new[]{new Vector2Int(720,1280),new Vector2Int(1080,1920),new Vector2Int(1080,2340),new Vector2Int(1170,2532),new Vector2Int(1080,2400)};
        try
        {
            PresentationPreferences.SetReducedMotion(false);
            foreach(string zone in new[]{"dungeon","magical-forest","drowned-court","ironvein-excavation","frostwake-peaks"})
            {
                yield return LaunchUnified(zone);
                yield return Until(()=>Run.Waves.ActiveEnemies.All(e=>e.GetComponent<EnemyLifecycleVFX>()?.IsSpawning!=true),"visual entrance settled");
                var hud=Object.FindFirstObjectByType<CrystalResonanceHud>();Assert.IsNotNull(hud);
                var state=Run.Resonance.State.Copy();state.charge=70;state.score=12345;state.triggeredMask=state.claimedMask=7;
                Run.Resonance.Restore(state);
                foreach(var size in sizes)
                {
                    typeof(GameplayPixelLayoutTests).GetMethod("SetGameViewSize",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,new object[]{size});
                    GameplayPixelLayoutController.ValidationSafeArea=new Rect(0,40,size.x,size.y-110);
                    yield return null;yield return null;yield return null;
                    var crystal=hud.transform.Find("ResonanceStoryCrystal").GetComponent<Image>();
                    float pixels=crystal.rectTransform.rect.width*crystal.canvas.rootCanvas.scaleFactor;
                    Assert.That(pixels/crystal.sprite.rect.width,Is.EqualTo(Mathf.Round(pixels/crystal.sprite.rect.width)).Within(.001f),"integer native crystal scale");
                    var track=(RectTransform)hud.transform.Find("ResonanceTrack");
                    Assert.That(track.rect.width,Is.GreaterThan(0));
                    var score=hud.GetComponentsInChildren<TMPro.TMP_Text>().First(t=>t.text.Contains("SCORE"));
                    score.ForceMeshUpdate();var corners=new Vector3[4];crystal.rectTransform.GetWorldCorners(corners);
                    float left=score.rectTransform.TransformPoint(score.textBounds.min).x;
                    Assert.That(left,Is.GreaterThanOrEqualTo(corners[3].x),"score must not overlap the crystal");
                    var root=(RectTransform)hud.transform;
                    float right=root.TransformPoint(new Vector3(root.rect.xMax-28*root.rect.height/66f,0,0)).x;
                    Assert.That(score.rectTransform.TransformPoint(score.textBounds.max).x,Is.LessThanOrEqualTo(right),"score leaves room for the right ornament");
                    ScreenCapture.CaptureScreenshot(Path.Combine(dir,zone+"-"+size.x+"x"+size.y+"-70.png"));yield return null;yield return null;
                }
                state.score=999999;Run.Resonance.Restore(state);yield return null;yield return null;
                var largeScore=hud.GetComponentsInChildren<TMPro.TMP_Text>().First(t=>t.text.Contains("SCORE"));
                largeScore.ForceMeshUpdate();
                Assert.IsFalse(largeScore.isTextOverflowing,"six-digit run score fits the plaque");
                ScreenCapture.CaptureScreenshot(Path.Combine(dir,zone+"-large-score.png"));yield return null;yield return null;
                state.score=12345;
                foreach(int charge in new[]{80,100})
                {
                    state.charge=charge;Run.Resonance.Restore(state);yield return new WaitForSeconds(.5f);
                    ScreenCapture.CaptureScreenshot(Path.Combine(dir,zone+"-"+charge+".png"));yield return null;yield return null;
                }
                if(zone=="dungeon")
                {
                    for(int frame=0;frame<18;frame++)
                    {ScreenCapture.CaptureScreenshot(Path.Combine(dir,"animation-"+frame.ToString("D2")+".png"));yield return new WaitForSeconds(.1f);}
                    var motes=hud.GetComponentsInChildren<UIPixelParticle>(true);
                    Assert.That(motes.Any(m=>m.IsPlaying),Is.True,"charged crystal emits native pixels");
                    Time.timeScale=0;var positions=motes.Select(m=>m.transform.localPosition).ToArray();
                    float until=Time.realtimeSinceStartup+.3f;while(Time.realtimeSinceStartup<until)yield return null;
                    CollectionAssert.AreEqual(positions,motes.Select(m=>m.transform.localPosition).ToArray(),"pause freezes crystal pixels");
                    Time.timeScale=1;PresentationPreferences.SetReducedMotion(true);yield return null;yield return null;
                    Assert.IsFalse(motes.Any(m=>m.IsPlaying),"reduced motion stops transient particles");
                    ScreenCapture.CaptureScreenshot(Path.Combine(dir,"dungeon-reduced-motion.png"));yield return null;yield return null;
                    PresentationPreferences.SetReducedMotion(false);
                    var crystal=hud.transform.Find("ResonanceStoryCrystal").GetComponent<Image>();var sprite=crystal.sprite;
                    crystal.sprite=null;yield return null;hud.Tick();crystal.sprite=sprite;
                }
            }
        }
        finally {Time.timeScale=1;PresentationPreferences.SetReducedMotion(reduced);GameplayPixelLayoutController.ValidationSafeArea=null;}
    }
}
#endif
