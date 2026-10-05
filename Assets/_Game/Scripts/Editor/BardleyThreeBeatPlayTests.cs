using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

public sealed partial class ForestFoundationPlayTests
{
    [UnityTest] public IEnumerator BardleyThreeBeatsKeepOneBoardHoldAndNoBubbleDelivery()
    {
        yield return Launch();
        PrepareSafeMove();
        var definition = AssetDatabase.LoadAssetAtPath<CrackedGemsAbilityDefinition>(
            "Assets/_Game/Data/Player Abilities/Ability_CrackedGems.asset");
        Assert.That(definition.TargetGemCount, Is.EqualTo(3));
        Assert.That(definition.EnergyCost, Is.EqualTo(80));
        Assert.That(EditorUtility.audioMasterMute, Is.True);
        var board = Run.Board;
        int bubbleEvents = 0, move = board.CompletedValidPlayerMoves;
        var beats = new List<float>();
        var order = new List<int>();
        board.CrackedGemTargetsSelected += (_, __, ___) => bubbleEvents++;
        board.PrimaryExplosionPresented += index =>
        {
            order.Add(index); beats.Add(Time.time);
            Assert.That(board.IsBusy, Is.True);
            int holes = 0;
            for (int y = 0; y < board.Height; y++) for (int x = 0; x < board.Width; x++)
                if (board.GetGem(x, y) == null) holes++;
            Assert.That(holes, Is.GreaterThan(0), "no refill between primary beats");
        };
        bool finished = false;
        Assert.That(board.TryActivateCrackedGems(new GemType[0], 3, 20,
            10, 10, .05f, 1.08f, .01f, () => finished = true), Is.True);
        yield return Until(() => finished && !board.IsBusy, "three primary explosions settle");
        CollectionAssert.AreEqual(new[] { 0, 1, 2 }, order);
        Assert.That(beats[1] - beats[0], Is.GreaterThanOrEqualTo(.16f));
        Assert.That(beats[2] - beats[1], Is.GreaterThanOrEqualTo(.16f));
        Assert.That(bubbleEvents, Is.Zero);
        Assert.That(board.CompletedValidPlayerMoves, Is.EqualTo(move), "ability remains a free action");
    }
}
