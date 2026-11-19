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
        {
            SceneManager.LoadScene("Game");
            yield return Stable();
        }

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
            yield return CaptureGalleryState(output, state);
        }

        var actors = Run.Waves.ActiveEnemies.ToArray();
        foreach (var actor in actors)
        {
            Assert.IsTrue(EnemyVisualPresenter.TryApplyFallbackOnly(actor.gameObject, actor.Definition.FallbackVisualSprite));
            Assert.IsTrue(EnemyVisualPresenter.TryApply(actor.gameObject, actor.Definition));
        }
        var specialist = actors.SingleOrDefault(actor => actor.HasSpecialAbility);
        if (specialist != null)
        {
            var ally = actors.First(actor => actor != specialist);
            bool heal = specialist.Definition.SpecialAbilityKind == EnemySpecialAbilityKind.GuildInstantHeal;
            if (heal) Assert.IsTrue(ally.ResolveDirectDamage(20).Applied);
            PrepareSafeMove();
            if (!heal)
            {
                // The upper swap leaves this lower, answerable weakness intact.
                var board = Run.Board;
                var sprites = (Sprite[])Get(board, "gemSprites");
                var color = specialist.AssignedGemType;
                var other = (GemType)(((int)color + 1) % sprites.Length);
                board.GetGem(0, 0).SetType(color, sprites[(int)color]);
                board.GetGem(1, 0).SetType(color, sprites[(int)color]);
                board.GetGem(2, 0).SetType(other, sprites[(int)other]);
                board.GetGem(2, 1).SetType(color, sprites[(int)color]);
            }
            int allyHealth = ally.CurrentHealth;
            var weakness = ally.AssignedGemType;
            specialist.SetSpecialTurnRequirement(1);
            int move = Run.MoveClock.Tick;
            Run.Board.ReplayPlayerSwap(safeMoveFrom.x, safeMoveFrom.y, safeMoveTo.x, safeMoveTo.y);
            yield return Until(() => Run.MoveClock.Tick == move + 1 && Run.Continuation.CanCapture,
                "one accepted diagnostic swap settles");
            yield return Until(() => heal ? ally.CurrentHealth != allyHealth : specialist.GuildTauntMoves > 0,
                "Guild support contact through the production action runtime");
            if (heal) Assert.AreEqual(allyHealth + 10, ally.CurrentHealth);
            else
            {
                Assert.AreEqual(3, specialist.GuildTauntMoves);
                Assert.IsTrue(ally.IsProtectedByTaunt);
                Assert.IsFalse(ally.ResolveDirectDamage(9).Applied);
                Assert.AreEqual(weakness, ally.AssignedGemType);
            }
            yield return CaptureGalleryState(output, heal ? "FieldMedicine" : "Taunt");
            yield return Until(() => !specialist.HasAnimationActionInProgress, "Guild support releases ownership");
            Assert.AreEqual(heal ? allyHealth + 10 : allyHealth, ally.CurrentHealth);
        }

        bool reduced = PresentationPreferences.ReducedMotion;
        try
        {
            PresentationPreferences.SetReducedMotion(true);
            Time.timeScale = 0f;
            var portraits = actors.Select(actor => actor.transform.Find("VisualRoot").GetComponent<Image>()).ToArray();
            var paused = portraits.Select(image => image.sprite).ToArray();
            yield return new WaitForSecondsRealtime(.2f);
            CollectionAssert.AreEqual(paused, portraits.Select(image => image.sprite));
            Time.timeScale = 1f;
            if (specialist != null && specialist.GuildTauntMoves > 0)
            {
                specialist.ResolveDirectDamage(specialist.MaxHealth + 100);
                Assert.IsFalse(actors.Any(actor => actor != specialist && actor.IsProtectedByTaunt));
            }
            var living = actors.Where(actor => !actor.IsDefeated).ToArray();
            foreach (var actor in living) Assert.IsTrue(actor.ResolveDirectDamage(1).Applied);
            yield return CaptureGalleryState(output, "Hit");
            foreach (var actor in living) actor.ResolveDirectDamage(actor.CurrentHealth + 100);
            Assert.IsTrue(actors.All(actor => actor.IsDefeated));
            yield return CaptureGalleryState(output, "Defeat");
        }
        finally
        {
            Time.timeScale = 1f;
            PresentationPreferences.SetReducedMotion(reduced);
        }
    }

    private static IEnumerator CaptureGalleryState(string output, string state)
    {
        string png = Path.Combine(output, state + "-" + System.Guid.NewGuid().ToString("N") + ".png");
        yield return null;
        ScreenCapture.CaptureScreenshot(png);
        yield return Until(() => File.Exists(png) && new FileInfo(png).Length > 0, "Game scene capture");
    }
}
