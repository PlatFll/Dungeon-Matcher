using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

[NonParallelizable]
public sealed class BoardFeel2Tests
{
    private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
    [Test] public void ImpactPromotesPastDetailButCannotSpamOrStarveDanger()
    {
        var gate = new HapticBudget();
        Assert.That(gate.TryConsume(0, 0), Is.True);
        Assert.That(gate.TryConsume(.02f, 4), Is.False, "40ms absolute floor");
        Assert.That(gate.TryConsume(.05f, 2), Is.True, "impact promotes past a detail cooldown");
        Assert.That(gate.TryConsume(.08f, 0), Is.False);
        Assert.That(gate.TryConsume(.10f, 4), Is.True, "player danger can promote");
        Assert.That(gate.TryConsume(.20f, 4), Is.False);
        Assert.That(gate.TryConsume(.24f, 4), Is.True);
    }
    [Test] public void PromotionStillHasASixPerSecondBudgetAndNoDeferredPlayback()
    {
        var gate = new HapticBudget();
        for (int i = 0; i < 6; i++) Assert.That(gate.TryConsume(i * .15f, 2), Is.True);
        Assert.That(gate.TryConsume(.91f, 4), Is.False);
        Assert.That(gate.TryConsume(1.01f, 4), Is.True);
        gate.Reset(); Assert.That(gate.TryConsume(0, 0), Is.True);
    }
    [Test] public void SoundVariantsNeverRepeatImmediatelyOrConsumeUnityRandom()
    {
        var state = UnityEngine.Random.state;
        try
        {
            UnityEngine.Random.InitState(9731);
            var before = UnityEngine.Random.state;
            var selector = new SoundVariantSelector();
            int previous = -1; var seen = new HashSet<int>();
            for (int i = 0; i < 100; i++)
            {
                int next = selector.Next(4);
                Assert.That(next, Is.InRange(0, 3)); Assert.That(next, Is.Not.EqualTo(previous));
                seen.Add(next); previous = next;
            }
            Assert.That(seen.Count, Is.EqualTo(4));
            Assert.That(UnityEngine.Random.state, Is.EqualTo(before));
            Assert.That(selector.Next(1), Is.Zero);
        }
        finally { UnityEngine.Random.state = state; }
    }
    [TestCase(3, 1f)] [TestCase(4, 1.03f)] [TestCase(9, 1.18f)] [TestCase(64, 1.18f)]
    public void ClearWeightIsBoundedAndOnlyAffectsGroupedImpacts(int count, float weight)
    {
        Assert.That(CombatSoundMix.CountWeight(CombatSoundCue.GemMatch, count), Is.EqualTo(weight).Within(.0001f));
        Assert.That(CombatSoundMix.CountWeight(CombatSoundCue.GemLand, count), Is.EqualTo(1));
        float sum = CombatSoundMix.Gain(CombatSoundCue.Explosion) * weight * CombatSoundMix.VoiceCount;
        Assert.That(sum * CombatSoundMix.HeadroomScale(sum), Is.LessThanOrEqualTo(.80001f));
    }
    [TestCase("GemMatch")] [TestCase("GemLand")] [TestCase("Explosion")] [TestCase("EnemyHit")]
    [TestCase("Swap")] [TestCase("InvalidSwap")] [TestCase("SpecialCreated")]
    [TestCase("BlockerHit")] [TestCase("BlockerBreak")]
    public void RepeatedCuesHaveFourDistinctShortMonoChoices(string name)
    {
        var clips = new List<AudioClip>();
        for (int i = 0; i < 4; i++)
        {
            var clip = Resources.Load<AudioClip>("Audio/Combat/" + name + (i == 0 ? "" : "_" + i));
            Assert.That(clip, Is.Not.Null); Assert.That(clip.channels, Is.EqualTo(1));
            Assert.That(clip.length, Is.InRange(.05f, .35f)); clips.Add(clip);
        }
        Assert.That(clips.Distinct().Count(), Is.EqualTo(4));
    }
    [Test] public void OldJournalJsonKeepsOriginalBoardTiming()
    {
        var old = JsonUtility.FromJson<RunReplayTape>("{\"frames\":[{\"delta\":0.016,\"actions\":[]}]}");
        Assert.That(old.boardFeelVersion, Is.Zero);
        var current = new RunReplayTape { boardFeelVersion = 1 };
        Assert.That(JsonUtility.FromJson<RunReplayTape>(JsonUtility.ToJson(current)).boardFeelVersion, Is.EqualTo(1));
    }
    [Test] public void PointerAcknowledgementDoesNotSelectAndCancellationRestoresScale()
    {
        var root = new GameObject("Pointer feedback fixture"); root.SetActive(false);
        try
        {
            var board = root.AddComponent<BoardController>();
            var go = new GameObject("Gem"); go.transform.SetParent(root.transform);
            var gem = go.AddComponent<Gem>(); gem.Initialize(board, 0, 0, GemType.Ruby, null, .9f);
            board.BeginPointerGesture(gem, Vector2.zero, 1);
            Assert.That(typeof(BoardController).GetField("selectedGem", Flags).GetValue(board), Is.Null);
            Assert.That(gem.GetComponent<SpriteRenderer>().color, Is.Not.EqualTo(Color.white));
            using (board.AcquireExternalInputBlock())
            {
                Assert.That(gem.transform.localScale, Is.EqualTo(Vector3.one * .9f));
                Assert.That(gem.GetComponent<SpriteRenderer>().color, Is.EqualTo(Color.white));
            }
            Assert.That(board.CompletedValidPlayerMoves, Is.Zero);
        }
        finally { UnityEngine.Object.DestroyImmediate(root); }
    }
}
