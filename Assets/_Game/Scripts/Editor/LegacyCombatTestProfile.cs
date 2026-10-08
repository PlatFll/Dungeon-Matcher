using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;

// Existing seconds-profile scenarios remain compatibility tests. Load a real
// version-one checkpoint so bootstrap, restore and runtime guards all agree.
internal static class LegacyCombatTestProfile
{
    public static IEnumerator Load()
    {
        var run = RunSession.Current;
        Assert.That(run.Continuation.CanCapture, Is.True);
        var saved = run.Continuation.Capture();
        saved.version = 1;
        saved.clock = null;
        Assert.That(AccountProgression.Current.StoreCheckpoint(run.RunId, saved), Is.True);
        SceneManager.LoadScene("Game");
        yield return null;
        float deadline = Time.realtimeSinceStartup + 40;
        while (RunSession.Current?.InitialStateReady != true ||
               RunSession.Current.Continuation.IsRestoring ||
               RunSession.Current.GetComponent<RunControlsUI>() == null)
        {
            Assert.That(Time.realtimeSinceStartup, Is.LessThan(deadline), "legacy checkpoint restores");
            yield return null;
        }
        RunSession.Current.GetComponent<RunControlsUI>().Close();
        while (!RunSession.Current.Continuation.CanCapture)
        {
            Assert.That(Time.realtimeSinceStartup, Is.LessThan(deadline), "legacy board settles");
            yield return null;
        }
        Assert.That(CombatMoveClock.Active, Is.False, "explicit version-one seconds profile");
    }
}
