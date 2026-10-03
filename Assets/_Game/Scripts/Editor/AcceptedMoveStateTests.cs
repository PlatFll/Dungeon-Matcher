#if UNITY_EDITOR
using System;
using NUnit.Framework;
using UnityEngine;

public sealed class AcceptedMoveStateTests
{
    [Test] public void OnlyAcceptedSequentialActionCanCommitOnce()
    {
        var state = new AcceptedMoveState();
        Assert.Throws<InvalidOperationException>(() => state.Commit(1));
        Assert.IsTrue(state.Accept(1));
        Assert.IsFalse(state.Accept(1));
        Assert.IsFalse(state.Accept(2));
        Assert.IsTrue(state.Commit(1));
        Assert.IsFalse(state.Commit(1));
        Assert.IsFalse(state.Accept(1));
        Assert.Throws<InvalidOperationException>(() => state.Accept(3));
        Assert.IsTrue(state.Accept(2));
        Assert.IsTrue(state.Commit(2));
        Assert.AreEqual(2, state.completed);
    }

    [Test] public void SnapshotPreservesActionAndStableActorIdentity()
    {
        var saved = new CombatClockSnapshot();
        Assert.AreEqual(1, saved.actions.AllocateActor());
        saved.actions.Accept(1); saved.actions.Commit(1);
        var restored = JsonUtility.FromJson<CombatClockSnapshot>(JsonUtility.ToJson(saved));
        Assert.AreEqual(CombatClockSnapshot.MoveProfile, restored.profile);
        Assert.AreEqual(1, restored.actions.completed);
        Assert.AreEqual(2, restored.actions.AllocateActor());
        Assert.IsFalse(restored.actions.IsPending);
        Assert.IsTrue(restored.actions.Accept(2));
    }
}
#endif
