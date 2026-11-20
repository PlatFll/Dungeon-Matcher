using System;
using System.IO;
using NUnit.Framework;
using UnityEngine;

public sealed class AccountRecoveryEdgeTests
{
    private string directory;
    private string file;
    private IDisposable mastery;

    [SetUp]
    public void SetUp()
    {
        directory = Path.GetFullPath(Path.Combine(".utmp", "AccountRecoveryEdges", Guid.NewGuid().ToString("N")));
        Directory.CreateDirectory(directory);
        file = Path.Combine(directory, "account.json");
        mastery = GemMasterySettings.UseTemporaryLoadout(GemMasteryLoadout.Default);
    }

    [TearDown]
    public void TearDown()
    {
        mastery?.Dispose();
    }

    [Test]
    public void EndingSuspendedRunFromHomePaysEveryCompletedMilestoneExactlyOnce()
    {
        var session = new AccountProgression(file);
        string run = session.BeginRun("skeleton");
        for (int wave = 1; wave <= 50; wave++)
        {
            Assert.That(session.RecordWave(run, wave, wave % 10 == 0, false), Is.True);
        }

        var expected = session.PreviewReward("End Run");
        Assert.That(expected.milestoneGold, Is.EqualTo(5 * BalanceV1.Current.milestoneGold));
        var reopened = new AccountProgression(file);
        Assert.That(reopened.ActiveRun.milestoneCount, Is.EqualTo(5));
        Assert.That(reopened.PreviewReward("End Run").Total, Is.EqualTo(expected.Total));
        Assert.That(reopened.FinalizeRun(run, "End Run"), Is.True);
        Assert.That(reopened.FinalizeRun(run, "End Run"), Is.False);
        Assert.That(new AccountProgression(file).Gold, Is.EqualTo(expected.Total));
    }

    [Test]
    public void FirstPurchaseAfterFallbackPreservesTheLastReadableAccountBackup()
    {
        File.WriteAllText(file, JsonUtility.ToJson(new AccountSave { gold = 180 }));
        var account = new AccountProgression(file);
        Assert.That(account.TryPurchase(ConsumableKind.HealthPotion), Is.True);
        var healthyBackup = File.ReadAllBytes(file + ".bak");
        File.WriteAllText(file, "broken primary");

        var recovered = new AccountProgression(file);
        Assert.That(recovered.Gold, Is.EqualTo(180));
        Assert.That(recovered.TryPurchase(ConsumableKind.Bomb), Is.True);
        var written = new AccountProgression(file);
        Assert.That(written.Gold, Is.EqualTo(180 - BalanceV1.Current.bombPrice));
        Assert.That(written.Owned(ConsumableKind.Bomb), Is.EqualTo(1));
        CollectionAssert.AreEqual(healthyBackup, File.ReadAllBytes(file + ".bak"));

        File.WriteAllText(file, "second damaged primary");
        var recoveredAgain = new AccountProgression(file);
        Assert.That(recoveredAgain.Gold, Is.EqualTo(180));
        Assert.That(recoveredAgain.Owned(ConsumableKind.Bomb), Is.Zero);
    }

    [Test]
    public void LockedHealthyPrimaryDoesNotReopenThePreviousWallet()
    {
        if (Application.platform != RuntimePlatform.WindowsEditor && Application.platform != RuntimePlatform.WindowsPlayer)
        {
            Assert.Ignore("This case uses Windows exclusive file sharing.");
        }

        File.WriteAllText(file, JsonUtility.ToJson(new AccountSave { gold = 180 }));
        var account = new AccountProgression(file);
        Assert.That(account.TryPurchase(ConsumableKind.Bomb), Is.True);
        byte[] previousWallet = File.ReadAllBytes(file + ".bak");
        using (new FileStream(file, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
        {
            Assert.Throws<IOException>(() => new AccountProgression(file));
        }

        var reopened = new AccountProgression(file);
        Assert.That(reopened.Owned(ConsumableKind.Bomb), Is.EqualTo(1));
        Assert.That(reopened.Gold, Is.EqualTo(180 - BalanceV1.Current.bombPrice));
        CollectionAssert.AreEqual(previousWallet, File.ReadAllBytes(file + ".bak"));
    }
}
