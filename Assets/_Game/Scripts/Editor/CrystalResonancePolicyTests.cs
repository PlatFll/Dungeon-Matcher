using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public sealed partial class ForestFoundationPlayTests
{
    [UnityTest] public IEnumerator CrystalResonancePolicyUsesJournaledLegalInputForEveryStyle()
    {
        foreach (ResonancePlaystyle style in Enum.GetValues(typeof(ResonancePlaystyle)))
        {
            RunLaunchOptions.ClearDevelopmentLaunch();SceneManager.LoadScene("Game");yield return Stable();
            var driver=new HumanLikePlaytestDriver(style,18201);
            var logs=new List<string>();driver.ActionLog=logs.Add;
            var accepted=new List<RunActionKind>();
            Run.Continuation.ValidationActionRecorded+=(_,tape)=>accepted.Add(tape.frames.SelectMany(f=>f.actions).Last().kind);
            float deadline=Time.realtimeSinceStartup+20;
            while(driver.Swaps==0 && Time.realtimeSinceStartup<deadline){driver.Step(Run);yield return null;}
            Assert.That(driver.Swaps,Is.EqualTo(1));yield return Stable();
            var submitted=logs.Where(l=>l.StartsWith("swap:")||l=="ability"||l=="potion"||l.StartsWith("bomb:")||l.StartsWith("card:"))
                .Select(l=>l.StartsWith("swap:")?RunActionKind.Swap:l=="ability"?RunActionKind.Ability:l=="potion"?RunActionKind.Potion:
                    l.StartsWith("bomb:")?RunActionKind.Bomb:RunActionKind.ChooseCard).ToArray();
            CollectionAssert.AreEqual(submitted,accepted,"each submitted action accepted once: "+style);
            Assert.That(accepted.Count(k=>k==RunActionKind.Swap),Is.EqualTo(1),"exactly one legal swap: "+style);
            Assert.That(Run.Board.CompletedValidPlayerMoves,Is.EqualTo(1));
            string decision=logs.Single(l=>l.StartsWith("decision:"));
            Assert.That(decision,Does.Contain("noticed="));Assert.That(decision,Does.Contain("heuristicGap="));
            Assert.That(logs.Count(l=>l.StartsWith("swap:")),Is.EqualTo(1));
            Assert.That(Run.ExitTo("MainMenu"),Is.True);yield return null;yield return null;
        }
    }

    [UnityTest] public IEnumerator CrystalResonancePolicyAttentionCannotAlwaysInspectEveryAlternative()
    {
        SceneManager.LoadScene("Game");yield return Stable();
        // Attention takes only source positions, not expected reward or future state.
        var choices=new List<BoardController.ResponseOption>();
        for(int x=0;x<Run.Board.Width-1;x++)for(int y=0;y<Run.Board.Height;y++)
            choices.Add(new BoardController.ResponseOption{Source=Run.Board.GetGem(x,y),Target=Run.Board.GetGem(x+1,y)});
        var notice=typeof(HumanLikePlaytestDriver).GetMethod("Notice",BindingFlags.Instance|BindingFlags.NonPublic);
        string before=JsonUtility.ToJson(Run.Continuation.Capture());
        foreach(ResonancePlaystyle style in Enum.GetValues(typeof(ResonancePlaystyle)))
        {
            var first=(List<BoardController.ResponseOption>)notice.Invoke(new HumanLikePlaytestDriver(style,1),new object[]{choices,false});
            var repeat=(List<BoardController.ResponseOption>)notice.Invoke(new HumanLikePlaytestDriver(style,1),new object[]{choices,false});
            CollectionAssert.AreEqual(first,repeat,"same decision seed and visible alternatives");
            Assert.That(first.Count,Is.GreaterThan(0));Assert.That(first.Distinct().Count(),Is.EqualTo(first.Count));
            Assert.That(first.All(choices.Contains),Is.True);
            if(style==ResonancePlaystyle.Skilled)Assert.That(first.Count,Is.EqualTo(choices.Count));
            else Assert.That(first.Count,Is.LessThan(choices.Count),"ordinary profiles have bounded attention");
        }
        Assert.That(JsonUtility.ToJson(Run.Continuation.Capture()),Is.EqualTo(before),"observation does not mutate state/RNG");
    }

    [UnityTest] public IEnumerator CrystalResonanceVisibleFollowupUsesOnlyAlreadyVisiblePieces()
    {
        SceneManager.LoadScene("Game");yield return Stable();
        var board=Run.Board;var sprites=(Sprite[])Get(board,"gemSprites");
        for(int x=0;x<board.Width;x++)for(int y=0;y<board.Height;y++)
        {var gem=board.GetGem(x,y);gem.SetSpecialType(GemSpecialType.None);gem.SetType((GemType)((x+2*y)%6),sprites[(x+2*y)%6]);}
        void Color(int x,int y,int type)=>board.GetGem(x,y).SetType((GemType)type,sprites[type]);
        Color(0,0,0);Color(0,1,0);Color(0,2,2);Color(1,2,0);
        Color(0,3,1);Color(1,0,1);Color(2,0,1);
        var option=board.GetImmediateResponses().First(o=>
            (o.Source==board.GetGem(0,2)&&o.Target==board.GetGem(1,2))||
            (o.Target==board.GetGem(0,2)&&o.Source==board.GetGem(1,2)));
        Assert.That(option.UsesSpecial||option.CreatesSpecial,Is.False);
        string before=JsonUtility.ToJson(board.CaptureContinuation(Run.Waves.ContinuationOwnerSlot));
        Assert.That(HumanLikePlaytestDriver.VisibleFollowups(board,option),Is.GreaterThanOrEqualTo(3),"visible fourth gem falls into a bottom-row match");
        Assert.That(JsonUtility.ToJson(board.CaptureContinuation(Run.Waves.ContinuationOwnerSlot)),Is.EqualTo(before));
        Color(0,3,3);Color(1,0,4);Color(2,0,5);
        Assert.That(HumanLikePlaytestDriver.VisibleFollowups(board,option),Is.Zero,"unknown refill cannot supply predicted gems");
    }
}
