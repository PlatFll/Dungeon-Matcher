using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

public sealed partial class ForestFoundationPlayTests
{
    [UnityTest] public IEnumerator PlayerStatusesUseCompleteActionsPauseAndResumeWithoutReplayingBurn()
    {
        yield return Launch(0, true);
        foreach (var enemy in Run.Waves.ActiveEnemies) enemy.GetComponent<EnemyAutoAttack>().StopAttacking();
        var statuses = Run.Player.Statuses;
        statuses.Apply(Resources.Load<PlayerStatusDefinition>("PlayerStatuses/Burn"));
        statuses.Apply(Resources.Load<PlayerStatusDefinition>("PlayerStatuses/Sapped"));
        statuses.Apply(Resources.Load<PlayerStatusDefinition>("PlayerStatuses/Fear"), Run.Waves.ActiveEnemies[0]);
        long feared = Run.Waves.ActiveEnemies[0].PersistentId;
        Time.timeScale = 0;
        yield return new WaitForSecondsRealtime(.2f);
        Assert.That(statuses.Remaining(PlayerStatusKind.Burn), Is.EqualTo(3));
        Time.timeScale = 1;
        yield return Move();
        Assert.That(statuses.Remaining(PlayerStatusKind.Burn), Is.EqualTo(2));
        Assert.That(Run.Player.LastDamageSummary, Does.Contain("Burn"));
        Assert.That(Run.Continuation.SaveNow(), Is.True);
        var snapshot = Run.Continuation.Capture();
        SceneManager.LoadScene("Game"); yield return Stable();
        Run.GetComponent<RunControlsUI>().Close();
        foreach (var enemy in Run.Waves.ActiveEnemies) enemy.GetComponent<EnemyAutoAttack>().StopAttacking();
        Assert.That(Run.Player.CurrentHealth, Is.EqualTo(snapshot.player.health));
        Assert.That(Run.Player.Statuses.Remaining(PlayerStatusKind.Burn), Is.EqualTo(2));
        Assert.That(Run.Player.Statuses.OutgoingMultiplier(Run.Waves.ActiveEnemies.First(e => e.PersistentId == feared)), Is.EqualTo(.75f));
        yield return Move(); yield return Move();
        Assert.That(Run.Player.Statuses.Effects, Is.Empty);
    }

    [UnityTest] public IEnumerator LegacyDungeonFearKeepsItsSourceThroughContinue()
    {
        SceneManager.LoadScene("Game"); yield return Stable();
        Assert.That(Run.MoveClock, Is.Null);
        var source = Run.Waves.ActiveEnemies[0]; long id = source.PersistentId;
        Assert.That(id, Is.GreaterThan(0));
        foreach (var enemy in Run.Waves.ActiveEnemies) enemy.GetComponent<EnemyAutoAttack>().StopAttacking();
        Assert.That(Run.Player.Statuses.Apply(Resources.Load<PlayerStatusDefinition>("PlayerStatuses/Fear"), source), Is.True);
        Assert.That(Run.Continuation.SaveNow(), Is.True);
        SceneManager.LoadScene("Game"); yield return Stable();
        Run.GetComponent<RunControlsUI>().Close();
        var restored = Run.Waves.ActiveEnemies.Single(e => e.PersistentId == id);
        Assert.That(Run.Player.Statuses.OutgoingMultiplier(restored), Is.EqualTo(.75f));
        restored.GetComponent<EnemyStagger>().ApplyStagger(2, 3);
        Assert.That(Run.Player.Statuses.Has(PlayerStatusKind.Fear), Is.False);
    }
}
