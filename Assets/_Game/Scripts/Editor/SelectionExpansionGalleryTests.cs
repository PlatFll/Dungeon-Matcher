using System.Collections;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

public sealed partial class ForestFoundationPlayTests
{
    [UnityTest] public IEnumerator ExpansionGalleryGuildFrontLine() =>
        CaptureExpansionFormation("dungeon", "guild_bulwark", "guild_ranger", "guild_swordsman");
    [UnityTest] public IEnumerator ExpansionGalleryGuildHealer() =>
        CaptureExpansionFormation("dungeon", "guild_chirurgeon", "guild_swordsman");
    [UnityTest] public IEnumerator ExpansionGalleryCourt() =>
        CaptureExpansionFormation("drowned-court", "nacre_duelist", "nacre_halberdier");
    [UnityTest] public IEnumerator ExpansionGalleryForest() =>
        CaptureExpansionFormation("magical-forest", "elven_pathblade", "orc_slinghunter");
    [UnityTest] public IEnumerator ExpansionGalleryFrostwake() =>
        CaptureExpansionFormation("frostwake-peaks", "iceborn_axeman");
    [UnityTest] public IEnumerator ExpansionGalleryIronvein() =>
        CaptureExpansionFormation("ironvein-excavation", "ore_ram");

    private IEnumerator CaptureExpansionFormation(string zoneId, params string[] members)
    {
        var zone = Resources.Load<ZoneDefinition>("Zones/" + zoneId);
        Assert.That(zone, Is.Not.Null, zoneId);
        var definitions = members.Select(id => zone.enemies.Single(e => e.EnemyId == id)).ToArray();
        RunLaunchOptions.StartingZone = zoneId;
        using (ResonanceEncounterFixture.OnNextGame(definitions, "Selection expansion diagnostic gallery"))
            SceneManager.LoadScene("Game");

        yield return Stable();
        Assert.That(Run.Zone.Definition.zoneId, Is.EqualTo(zoneId));
        CollectionAssert.AreEquivalent(members, Run.Waves.ActiveEnemies.Select(e => e.Definition.EnemyId));
        yield return Until(() => Run.Waves.ActiveEnemies.All(e =>
            e.GetComponent<EnemyLifecycleVFX>()?.IsSpawning != true), "new formation settles");

        string output = Path.GetFullPath(Path.Combine(".utmp", "SelectionExpansion", "Gallery", TestContext.CurrentContext.Test.Name));
        Directory.CreateDirectory(output);
        foreach (string state in new[] { "Idle", "AutoAttack", "Ability" })
        {
            bool any = false;
            foreach (var enemy in Run.Waves.ActiveEnemies)
            {
                var anim = enemy.GetComponentsInChildren<Animator>(true)
                    .Single(x => x.runtimeAnimatorController == enemy.Definition.AnimationControllerOverride);
                int hash = Animator.StringToHash("Base Layer." + state);
                if (!anim.HasState(0, hash))
                {
                    Assert.That(state, Is.EqualTo("Ability"));
                    Assert.That(enemy.Definition.HasSpecialAbility, Is.False);
                    continue;
                }
                any = true;
                anim.Play(hash, 0, state == "Idle" ? 0f : 0.5f);
                anim.Update(0f);
                Assert.That(anim.GetComponent<Image>().sprite, Is.Not.Null, enemy.Definition.EnemyId);
            }
            if (!any) continue;
            string png = Path.Combine(output, state + "-" + System.Guid.NewGuid().ToString("N") + ".png");
            yield return null;
            ScreenCapture.CaptureScreenshot(png);
            yield return Until(() => File.Exists(png) && new FileInfo(png).Length > 0, "Game scene capture");
        }
    }
}
