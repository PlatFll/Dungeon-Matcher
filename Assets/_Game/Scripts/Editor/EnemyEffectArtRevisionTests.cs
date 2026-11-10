using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public sealed partial class ForestFoundationPlayTests
{
    [UnityTest] public IEnumerator HearthFlameMovesWithFixedFootingPausesAndResumes()
    {
        yield return LaunchFrostwakeKit("hearthkeeper","iceborn_raider");StopAmbientFrost();
        var owner=Enemy("hearthkeeper");var ally=Enemy("iceborn_raider");Set(ally,"currentHealth",ally.MaxHealth-25);
        yield return ReadyRevisionHearth(owner,ally);
        var flame=Run.Board.GetComponentsInChildren<HearthEmberFlameView>().Single();
        var renderer=flame.GetComponent<SpriteRenderer>();
        int hp=ally.CurrentHealth,moves=Run.Board.CompletedValidPlayerMoves;
        var position=renderer.transform.position;var scale=renderer.transform.localScale;
        bool hadPreference=PlayerPrefs.HasKey(PresentationPreferences.ReducedMotionKey);
        int preference=PlayerPrefs.GetInt(PresentationPreferences.ReducedMotionKey);
        try
        {
            PlayerPrefs.SetInt(PresentationPreferences.ReducedMotionKey,0);
            var seen=new HashSet<Sprite>();float until=Time.realtimeSinceStartup+1.1f;
            while(Time.realtimeSinceStartup<until){seen.Add(renderer.sprite);yield return null;}
            Assert.That(seen.Count,Is.GreaterThanOrEqualTo(7),"the live hearth actually cycles through authored flame cels");
            Assert.That(renderer.transform.position,Is.EqualTo(position));Assert.That(renderer.transform.localScale,Is.EqualTo(scale));
            Assert.That(ally.CurrentHealth,Is.EqualTo(hp));Assert.That(Run.Board.CompletedValidPlayerMoves,Is.EqualTo(moves));
            Time.timeScale=0;yield return null;var paused=renderer.sprite;
            yield return new WaitForSecondsRealtime(.3f);Assert.That(renderer.sprite,Is.SameAs(paused));
            Time.timeScale=1;PlayerPrefs.SetInt(PresentationPreferences.ReducedMotionKey,1);yield return null;yield return null;
            var still=Resources.Load<Sprite>("Combat/ApprovedRevisions/HearthFlame/HearthFlame_00");
            Assert.That(renderer.sprite,Is.SameAs(still));
            yield return new WaitForSecondsRealtime(.3f);Assert.That(renderer.sprite,Is.SameAs(still));
            PlayerPrefs.SetInt(PresentationPreferences.ReducedMotionKey,0);
            yield return Until(()=>renderer.sprite!=still,"flame resumes after Reduced Motion is disabled");
            // Sample the real Game renderer at authored frame times; normal
            // automatic playback and pause were independently exercised above.
            Time.timeScale=0;
            for(int i=0;i<8;i++)
            {
                Set(flame,"elapsed",i*.12f+.001f);Call(flame,"Present");
                yield return CaptureEnvironment("hearth-flame-"+i.ToString("00"));
            }
            Time.timeScale=1;
            yield return ResumeRoster();
            Assert.That(Run.Board.GetComponentsInChildren<HearthEmberFlameView>().Length,Is.EqualTo(1));
        }
        finally
        {
            Time.timeScale=1;
            if(hadPreference)PlayerPrefs.SetInt(PresentationPreferences.ReducedMotionKey,preference);
            else PlayerPrefs.DeleteKey(PresentationPreferences.ReducedMotionKey);
        }
    }

    [UnityTest] public IEnumerator MarineNetFillsOneCellAtEveryRemainingDurationAndPhoneSize()
    {
        yield return LaunchCourt("reef_netweaver","shellback_porter");QuietKitFixture();
        // Aquatic state is created by the first accepted move, as in production.
        yield return Move();PrepareSafeMove();
        var board=Run.Board;board.Aquatic.StartFlood(30,Run.MoveClock.Tick);
        // Match the existing Court expiry fixture: snares may only be placed
        // while a useful combat answer and a collectible AIR route survive.
        var types=(Sprite[])Get(board,"gemSprites");var useful=Enemy("shellback_porter").AssignedGemType;
        foreach(var point in new[]{new Vector2Int(safeMoveTo.x-2,safeMoveTo.y),new Vector2Int(safeMoveTo.x-1,safeMoveTo.y),safeMoveFrom})
            board.GetGem(point.x,point.y).SetType(useful,types[(int)useful]);
        board.Aquatic.bubbles.Clear();board.Aquatic.bubbles.Add(board.GetGem(safeMoveFrom.x,safeMoveFrom.y).BoardIdentity);
        Assert.That(board.TryApplyAquaticSnares(Enemy("reef_netweaver")),Is.True);
        var snares=board.Aquatic.snares;Assert.That(snares.Count,Is.GreaterThan(0));
        foreach(var size in new[]{new Vector2Int(1080,1920),new Vector2Int(360,640),new Vector2Int(1080,2400)})
        {
            typeof(GameplayPixelLayoutTests).GetMethod("SetGameViewSize",BindingFlags.Static|BindingFlags.NonPublic)
                .Invoke(null,new object[]{size});
            yield return null;yield return null;yield return null;
            foreach(int remaining in new[]{3,2,1})
            {
                foreach(var snare in snares)snare.expiresMove=board.CompletedValidPlayerMoves+remaining;
                yield return null;yield return null;
                foreach(var snare in snares)
                {
                    var gem=board.FindAquaticGem(snare.gemId);
                    var view=board.transform.Find("snare"+snare.gemId).GetComponent<SpriteRenderer>();
                    Assert.That(view.sprite.rect.size,Is.EqualTo(new Vector2(64,64)));
                    Assert.That(view.bounds.size.x,Is.EqualTo(board.CellSize*board.transform.lossyScale.x).Within(.0001f));
                    Assert.That(view.bounds.size.y,Is.EqualTo(board.CellSize*board.transform.lossyScale.y).Within(.0001f));
                    // World -> board-local -> world uses floating-point matrices.
                    // Keep the tolerance far below one displayed pixel.
                    Assert.That(Vector3.Distance(view.transform.position,gem.transform.position),Is.LessThan(.00001f));
                    Assert.That(view.enabled && view.isVisible,Is.True);
                }
            }
            yield return CaptureEnvironment("marine-net-full-tile-"+size.x+"x"+size.y);
        }
    }
}
