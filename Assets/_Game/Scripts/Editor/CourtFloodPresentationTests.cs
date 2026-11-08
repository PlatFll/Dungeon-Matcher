using System;
using System.Collections;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

public sealed partial class ForestFoundationPlayTests
{
    [UnityTest] public IEnumerator CourtFloodPresentationPreviewAndContracts()
    {
        bool hadReduced = PlayerPrefs.HasKey(PresentationPreferences.ReducedMotionKey);
        int oldReduced = PlayerPrefs.GetInt(PresentationPreferences.ReducedMotionKey, 0);
        int oldCapture = Time.captureFramerate;
        string output = Path.GetFullPath(".utmp/CourtFloodReview");
        Directory.CreateDirectory(output);
        try
        {
            PlayerPrefs.SetInt(PresentationPreferences.ReducedMotionKey, 0);
            yield return LaunchCourt("reef_spearman", "hammerhead_bruiser", "pearl_thief");
            // Production initializes the aquatic move state on its first accepted move.
            yield return Move();
            var flood = UnityEngine.Object.FindFirstObjectByType<CourtFloodGraphic>();
            Assert.That(flood, Is.Not.Null);
            Assert.That(flood.raycastTarget, Is.False);
            Assert.That(flood.GetComponent<GraphicRaycaster>(), Is.Null);
            Assert.That(flood.mainTexture.width, Is.EqualTo(512));
            Assert.That(flood.mainTexture.height, Is.EqualTo(272));
            Assert.That(flood.mainTexture.filterMode, Is.EqualTo(FilterMode.Point));
            Assert.That(((Texture2D)flood.mainTexture).mipmapCount, Is.EqualTo(1));
            int tick = Run.MoveClock.Tick, health = Run.Player.CurrentHealth;
            var state = Run.Board.Aquatic;
            Assert.That(state,Is.Not.Null);
            var layout = UnityEngine.Object.FindFirstObjectByType<GameplayPixelLayoutController>();
            foreach(var size in new[]{new Vector2Int(720,1280), new Vector2Int(1080,1920), new Vector2Int(1080,2400)})
            {
                typeof(GameplayPixelLayoutTests).GetMethod("SetGameViewSize", BindingFlags.Static|BindingFlags.NonPublic)
                    .Invoke(null,new object[]{size});
                yield return Until(()=>Screen.width==size.x && Screen.height==size.y,"water preview resolution");
                for(int i=0;i<8;i++) yield return null;
                Assert.That(GameplayPixelLayoutValidator.Validate(layout,out string report),Is.Empty,report);
                File.WriteAllText(Path.Combine(output,"Layout-"+size.x+"x"+size.y+".txt"),report);
                ScreenCapture.CaptureScreenshot(Path.Combine(output,"Dry-"+size.x+"x"+size.y+".png"));
                yield return null; yield return null;
                state.StartFlood(12,tick);
                yield return Until(()=>flood.FillAmount>=1,"cosmetic water reaches the entire screen");
                Canvas.ForceUpdateCanvases();
                var floodMesh=flood.canvasRenderer.GetMesh();
                Assert.That(floodMesh.vertexCount,Is.GreaterThan(4),"water must submit visible fill and bubble geometry");
                Assert.That(flood.canvasRenderer.cull,Is.False,"water must not be culled");
                File.WriteAllText(Path.Combine(output,"Water-draw-order.txt"),string.Join("\n",Array.ConvertAll(flood.canvas.rootCanvas.GetComponentsInChildren<Graphic>(),g=>g.name+" depth="+g.depth+" rect="+g.rectTransform.rect+" color="+g.color)));
                Assert.That(state.wetMoves,Is.EqualTo(12));
                Assert.That(state.air,Is.EqualTo(5));
                Assert.That(Run.MoveClock.Tick,Is.EqualTo(tick));
                Assert.That(Run.Player.CurrentHealth,Is.EqualTo(health));
                var corners=new Vector3[4]; flood.rectTransform.GetWorldCorners(corners);
                Assert.That(corners[0].x,Is.EqualTo(0).Within(.1));
                Assert.That(corners[0].y,Is.EqualTo(0).Within(.1));
                Assert.That(corners[2].x,Is.EqualTo(size.x).Within(.1));
                Assert.That(corners[2].y,Is.EqualTo(size.y).Within(.1));
                ScreenCapture.CaptureScreenshot(Path.Combine(output,"Flooded-"+size.x+"x"+size.y+".png"));
                yield return null; yield return null;
                state.Drain(Run.Travel.LocalWave);
                yield return Until(()=>flood.FillAmount<=0,"water drains completely");
            }
            // Safe-area padding must not create uncovered flood margins.
            GameplayPixelLayoutController.ValidationSafeArea=new Rect(32,72,1016,2240);
            for(int i=0;i<8;i++)yield return null;
            state.StartFlood(12,tick);
            yield return Until(()=>flood.FillAmount>=1,"safe area flood");
            ScreenCapture.CaptureScreenshot(Path.Combine(output,"SafeArea-1080x2400.png"));
            yield return null;yield return null;
            Time.timeScale=0; float paused=flood.AnimationTime;
            for(int i=0;i<5;i++)yield return null;
            Assert.That(flood.AnimationTime,Is.EqualTo(paused),"pause freezes cosmetic motion");
            Time.timeScale=1;
            PlayerPrefs.SetInt(PresentationPreferences.ReducedMotionKey,1);
            state.Drain(Run.Travel.LocalWave); yield return null;yield return null;
            Assert.That(flood.FillAmount,Is.Zero);
            state.StartFlood(12,tick);yield return null;yield return null;
            Assert.That(flood.FillAmount,Is.EqualTo(1));
            paused=flood.AnimationTime;
            for(int i=0;i<5;i++)yield return null;
            Assert.That(flood.AnimationTime,Is.EqualTo(paused),"reduced motion uses static tint");
            state.Drain(Run.Travel.LocalWave);yield return null;yield return null;
            PlayerPrefs.SetInt(PresentationPreferences.ReducedMotionKey,0);
            GameplayPixelLayoutController.ValidationSafeArea=null;
            typeof(GameplayPixelLayoutTests).GetMethod("SetGameViewSize",BindingFlags.Static|BindingFlags.NonPublic)
                .Invoke(null,new object[]{new Vector2Int(720,1280)});
            yield return Until(()=>Screen.width==720&&Screen.height==1280,"capture dimensions");
            for(int i=0;i<8;i++)yield return null;
            string frames=Path.Combine(output,"Frames");Directory.CreateDirectory(frames);
            Time.captureFramerate=30;
            for(int frame=0;frame<210;frame++)
            {
                if(frame==15)state.StartFlood(12,tick);
                if(frame==135)state.Drain(Run.Travel.LocalWave);
                ScreenCapture.CaptureScreenshot(Path.Combine(frames,frame.ToString("D3")+".png"));
                yield return null;
            }
            yield return null;yield return null;
            Assert.That(Run.MoveClock.Tick,Is.EqualTo(tick),"effect never spends a move");
            Assert.That(flood.FillAmount,Is.Zero);
            File.WriteAllText(Path.Combine(output,"Result.txt"),"PASS: actual Game scene; 3 phone sizes, safe area, full canvas, pause, reduced motion, no input interception, unchanged move/AIR/HP state. 210 frames at 30fps.");
        }
        finally
        {
            Time.captureFramerate=oldCapture;Time.timeScale=1;
            GameplayPixelLayoutController.ValidationSafeArea=null;
            if(hadReduced)PlayerPrefs.SetInt(PresentationPreferences.ReducedMotionKey,oldReduced);
            else PlayerPrefs.DeleteKey(PresentationPreferences.ReducedMotionKey);
        }
    }
}
