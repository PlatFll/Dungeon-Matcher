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

    [UnityTest] public IEnumerator ExpansionSlinghunterSingleHitToHealth() => VerifySlinghunterContact(false);
    [UnityTest] public IEnumerator ExpansionSlinghunterSingleHitBreaksShield() => VerifySlinghunterContact(true);

    private IEnumerator VerifySlinghunterContact(bool shielded)
    {
        const string zoneId = "magical-forest";
        var zone = Resources.Load<ZoneDefinition>("Zones/" + zoneId);
        var definition = zone.enemies.Single(e => e.EnemyId == "orc_slinghunter");
        RunLaunchOptions.StartingZone = zoneId;
        using (ResonanceEncounterFixture.OnNextGame(new[] { definition }, "Single sling stone"))
        {
            SceneManager.LoadScene("Game");
            yield return Stable();
        }

        var orc = Run.Waves.ActiveEnemies.Single();
        Assert.That(orc.Definition, Is.EqualTo(definition));
        yield return Until(() => orc.GetComponent<EnemyLifecycleVFX>()?.IsSpawning != true, "Slinghunter settles");
        var attack = orc.GetComponent<EnemyAutoAttack>();
        var player = Run.Player;
        Assert.That(orc.FollowUpDamage, Is.Zero);
        Assert.That(player.CurrentShield, Is.Zero);
        int startingShield = shielded ? CombatAmounts.Step : 0;
        if (shielded) Assert.That(player.GrantShield(startingShield), Is.EqualTo(startingShield));
        Assert.That(player.CurrentShield, Is.EqualTo(startingShield));
        int before = player.CurrentHealth;
        int resolved = 0;
        int attributed = 0;
        string reason = null;
        int hpLoss = 0;
        int shieldLoss = 0;
        System.Action<EnemyAutoAttack, int, bool> onAttack = (source, amount, applied) => resolved++;
        System.Action<EnemyActor, string, int, int> onDamage = (source, kind, hp, shield) =>
        {
            if (source != orc) return;
            attributed++;
            reason = kind;
            hpLoss += hp;
            shieldLoss += shield;
        };
        attack.AttackResolved += onAttack;
        player.DamageResolved += onDamage;
        try
        {
            // Isolate a due attack, then let the regular accepted-swap pipeline run it.
            Set(attack, "unifiedRemaining", 1);
            yield return Move();
            yield return Until(() => resolved > 0 && !attack.IsAttackSequenceInProgress, "single shot and recovery");
            yield return new WaitForSeconds(0.4f);
            Assert.That(resolved, Is.EqualTo(1), "All attack contacts, including any blocked follow-up");
            Assert.That(attributed, Is.EqualTo(1));
            Assert.That(reason, Is.EqualTo("basic contact"));
            Assert.That(hpLoss, Is.EqualTo(shielded ? 0 : orc.Damage));
            Assert.That(shieldLoss, Is.EqualTo(startingShield));
            Assert.That(player.CurrentHealth, Is.EqualTo(before - hpLoss));
            Assert.That(player.CurrentShield, Is.Zero);
            TestContext.Out.WriteLine($"Slinghunter contacts={resolved}; attributed={attributed}; HP {before}->{player.CurrentHealth}; shield {startingShield}->{player.CurrentShield}; move={Run.MoveClock.Tick}");
        }
        finally
        {
            attack.AttackResolved -= onAttack;
            player.DamageResolved -= onDamage;
        }
    }


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
        string capture = System.Environment.GetEnvironmentVariable("DM_APPROVED_MOTION_CAPTURE");
        if (!string.IsNullOrEmpty(capture) && zoneId != "ironvein-excavation")
            yield return CaptureApprovedMotion(Path.Combine(capture, TestContext.CurrentContext.Test.Name), Run.Waves.ActiveEnemies.ToArray());

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
            StringAssert.Contains(actor.Definition.Description, CombatGuide.Enemy(actor));
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

    private static IEnumerator CaptureApprovedMotion(string folder, EnemyActor[] actors)
    {
        // Record the imported controller poses on the real Game canvas. Suppress sampling
        // events only during the visual capture; combat assertions run at normal speed.
        Directory.CreateDirectory(folder);
        var animators = actors.Select(e => e.GetComponentsInChildren<Animator>(true)
            .Single(a => a.runtimeAnimatorController == e.Definition.AnimationControllerOverride)).ToArray();
        var speeds = animators.Select(a => a.speed).ToArray();
        var fireEvents = animators.Select(a => a.fireEvents).ToArray();
        var rows = new System.Collections.Generic.List<string> { "enemy,xMin,yMin,xMax,yMax,screenWidth,screenHeight" };
        for (int i = 0; i < actors.Length; i++)
        {
            var image = animators[i].GetComponent<Image>();
            var corners = new Vector3[4];
            image.rectTransform.GetWorldCorners(corners);
            var canvas = image.canvas.rootCanvas;
            var camera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
            var points = corners.Select(c => RectTransformUtility.WorldToScreenPoint(camera, c)).ToArray();
            rows.Add(string.Join(",", actors[i].Definition.EnemyId,
                Mathf.FloorToInt(points.Min(p => p.x)), Mathf.FloorToInt(points.Min(p => p.y)),
                Mathf.CeilToInt(points.Max(p => p.x)), Mathf.CeilToInt(points.Max(p => p.y)), Screen.width, Screen.height));
        }
        File.WriteAllLines(Path.Combine(folder, "actors.csv"), rows);
        try
        {
            foreach (var animator in animators) { animator.fireEvents = false; animator.speed = 0f; }
            foreach (string state in new[] { "Idle", "AutoAttack", "Ability" })
            {
                var hash = Animator.StringToHash("Base Layer." + state);
                var clips = animators.Select(a => a.runtimeAnimatorController.animationClips
                    .FirstOrDefault(c => c.name.EndsWith("_" + state, System.StringComparison.Ordinal))).ToArray();
                if (clips.All(c => c == null)) continue;
                var times = new System.Collections.Generic.SortedSet<int> { 0 };
                foreach (var clip in clips.Where(c => c != null))
                {
                    var binding = UnityEditor.AnimationUtility.GetObjectReferenceCurveBindings(clip).Single();
                    foreach (var key in UnityEditor.AnimationUtility.GetObjectReferenceCurve(clip, binding))
                        times.Add(Mathf.RoundToInt(key.time * 1000));
                }
                foreach (int ms in times)
                {
                    for (int i = 0; i < clips.Length; i++)
                    {
                        if (clips[i] == null) continue;
                        int duration = Mathf.RoundToInt(clips[i].length * 1000);
                        float sample = (Mathf.Min(ms, duration - 1) + .25f) / duration;
                        animators[i].Play(hash, 0, sample);
                        animators[i].Update(0f);
                        Assert.That(animators[i].GetComponent<Image>().sprite.name,
                            Does.StartWith(clips[i].name + "_"), "visible sprite from the production clip");
                    }
                    yield return null;
                    string png = Path.Combine(folder, state + "-" + ms.ToString("D4") + ".png");
                    ScreenCapture.CaptureScreenshot(png);
                    yield return Until(() => File.Exists(png) && new FileInfo(png).Length > 0, "rendered Game frame");
                }
            }
        }
        finally
        {
            for (int i = 0; i < animators.Length; i++)
            {
                animators[i].Play("Base Layer.Idle", 0, 0f);
                animators[i].Update(0f);
                animators[i].speed = speeds[i];
                animators[i].fireEvents = fireEvents[i];
            }
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
