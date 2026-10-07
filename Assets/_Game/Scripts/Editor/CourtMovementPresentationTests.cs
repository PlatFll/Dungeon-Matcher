using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

public sealed partial class ForestFoundationPlayTests
{
    [UnityTest] public IEnumerator CourtMotionActualSwapFallsPauseAndDrain() {yield return CourtMotionMove();}
    private IEnumerator CourtMotionMove()
    {
        yield return LaunchCourt("shellback_porter");yield return Move();
        var board=Run.Board;board.Aquatic.StartFlood(18,Run.MoveClock.Tick);
        var fx=board.GetComponent<CourtBoardEffects>();Assert.That(fx,Is.Not.Null);
        int swaps=0,falls=0;board.GemMotionPresented+=(g,d,delay,gravity)=>{if(gravity)falls++;else swaps++;};
        PrepareSafeMove();int tick=board.CompletedValidPlayerMoves;
        board.StartCoroutine((IEnumerator)Call(board,"TrySwap",board.GetGem(safeMoveFrom.x,safeMoveFrom.y),board.GetGem(safeMoveTo.x,safeMoveTo.y)));
        yield return Until(()=>board.GetComponentsInChildren<SpriteRenderer>().Any(r=>r.name=="CourtParticle"),"actual wet swap bubbles");
        Assert.That(board.GetComponentsInChildren<SpriteRenderer>().Any(r=>r.name=="CourtRipple"),Is.True);
        Time.timeScale=0;yield return null;
        var particles=board.GetComponentsInChildren<SpriteRenderer>().Where(r=>r.name=="CourtParticle").ToArray();
        var positions=particles.Select(r=>r.transform.position).ToArray();
        yield return new WaitForSecondsRealtime(.15f);
        Assert.That(particles.Select(r=>r.transform.position),Is.EqualTo(positions));
        Time.timeScale=1;yield return Until(()=>board.CompletedValidPlayerMoves==tick+1&&Run.Continuation.CanCapture,"wet move settles");
        Assert.That(swaps,Is.GreaterThanOrEqualTo(2));Assert.That(falls,Is.GreaterThan(0));
        Assert.That(((IList)Get(fx,"particles")).Count,Is.LessThanOrEqualTo(24));
        board.Aquatic.phase=TidePhase.Dry;yield return null;yield return null;
        Assert.That(((IList)Get(fx,"particles")),Is.Empty);Assert.That(((IList)Get(fx,"trails")),Is.Empty);
        yield return Move();Assert.That(((IList)Get(fx,"particles")),Is.Empty);
    }
    [UnityTest] public IEnumerator CourtMotionRoyalWakeReducedMotionAndMissingArt() {yield return CourtMotionRoyal();}
    private IEnumerator CourtMotionRoyal()
    {
        yield return RoyalCofferFixture();var board=Run.Board;var fx=board.GetComponent<CourtBoardEffects>();
        var theme=GameplayThemeSkin.Current;var frames=theme.waterRippleFrames;var bubble=theme.waterMicroBubble;
        bool reduced=PresentationPreferences.ReducedMotion;
        try
        {
            PresentationPreferences.SetReducedMotion(false);
            var c=board.Aquatic.coffer;
            var hit=new[]{Vector2Int.left,Vector2Int.right,Vector2Int.up,Vector2Int.down}
                .Select(d=>board.GetGem(c.x+d.x,c.y+d.y)).First(g=>g!=null);
            bool sliding=false;board.CofferMoving+=(_,__,___)=>sliding=true;
            Assert.That(board.TryClearPlayerArea(hit,0,()=>true),Is.True);
            yield return Until(()=>sliding,"royal slide presentation event");yield return null;
            Assert.That(((IList)Get(fx,"trails")).Cast<object>().Any(t=>(bool)Get(t,"coffer")),Is.True);
            Assert.That(board.GetComponentsInChildren<SpriteRenderer>().Any(r=>r.name=="CourtRipple"),Is.True);
            yield return Stable();
            yield return Until(()=>((IList)Get(fx,"particles")).Count==0&&((IList)Get(fx,"trails")).Count==0,"prior slide and settle effects finish");
            PresentationPreferences.SetReducedMotion(true);
            Call(fx,"CofferMoved",board.transform.position,board.transform.position+Vector3.right,.24f);
            yield return null;yield return null;
            Assert.That(board.GetComponentsInChildren<SpriteRenderer>().Any(r=>r.name=="CourtRipple"),Is.False);
            Assert.That(((IList)Get(fx,"particles")).Count,Is.InRange(1,2));
            yield return new WaitForSeconds(.5f);
            theme.waterRippleFrames=null;theme.waterMicroBubble=null;
            yield return Move();Assert.That(Run.Continuation.CanCapture,Is.True);
            Assert.That(((IList)Get(fx,"particles")),Is.Empty);
            for(int i=0;i<100;i++)Call(fx,"CofferMoved",Vector3.zero,Vector3.right,.24f);
            Assert.That(((IList)Get(fx,"trails")).Count,Is.LessThanOrEqualTo(10));
            fx.enabled=false;Assert.That(((IList)Get(fx,"trails")),Is.Empty);
        }
        finally {theme.waterRippleFrames=frames;theme.waterMicroBubble=bubble;PresentationPreferences.SetReducedMotion(reduced);}
    }
}

public sealed class CourtMovementAssetTests
{
    [Test] public void CourtRippleNativeFramesUseCrispImports()
    {
        var theme=Resources.Load<GameplayThemeDefinition>("Zones/DrownedCourtTheme");
        Assert.That(theme.waterRippleFrames.Length,Is.EqualTo(3));
        foreach(var sprite in theme.waterRippleFrames)
        {
            Assert.That(sprite.rect.size,Is.EqualTo(new Vector2(32,16)));
            var path=AssetDatabase.GetAssetPath(sprite);var importer=(TextureImporter)AssetImporter.GetAtPath(path);
            Assert.That(importer.filterMode,Is.EqualTo(FilterMode.Point));Assert.That(importer.mipmapEnabled,Is.False);
            Assert.That(importer.textureCompression,Is.EqualTo(TextureImporterCompression.Uncompressed));
            Assert.That(System.IO.File.ReadAllBytes(path),Is.EqualTo(System.IO.File.ReadAllBytes("ArtSource/DrownedCourt/RosterRevision/"+sprite.name+".png")));
        }
    }
}
