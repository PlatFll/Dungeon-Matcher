using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

public sealed partial class ForestFoundationPlayTests
{
    [UnityTest] public IEnumerator StatusPanelFitsPortraitsAndSafeAreaWithAllEightEffects()
    {
        yield return Launch(0, true);
        foreach (var enemy in Run.Waves.ActiveEnemies) enemy.GetComponent<EnemyAutoAttack>().StopAttacking();
        Time.timeScale = 0;
        foreach (var data in Resources.LoadAll<PlayerStatusDefinition>("PlayerStatuses"))
            Assert.That(Run.Player.Statuses.Apply(data, Run.Waves.ActiveEnemies[0]), Is.True);
        Run.Player.GrantShield(30);
        string output = Path.GetFullPath(".utmp/StatusRevision/Visual"); Directory.CreateDirectory(output);
        try
        {
            int variant = 0;
            foreach (var size in new[] { new Vector2Int(720,1280), new Vector2Int(1080,1920), new Vector2Int(1080,2400), new Vector2Int(1080,1920) })
            {
                bool inset = variant++ == 3;
                typeof(GameplayPixelLayoutTests).GetMethod("SetGameViewSize", BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,new object[]{size});
                GameplayPixelLayoutController.ValidationSafeArea = inset ? new Rect(32,60,1016,1772) : (Rect?)null;
                yield return Until(() => Screen.width == size.x && Screen.height == size.y, "status viewport");
                for (int frame=0; frame<8; frame++) yield return null;
                ScreenCapture.CaptureScreenshot(Path.Combine(output,"debug-statuses-"+variant+".png")); yield return null; yield return null;
                var view = Object.FindFirstObjectByType<PlayerStatusPanel>(); var root = view.StatusRoot;
                Assert.That(root.childCount, Is.EqualTo(8));
                var parent = (RectTransform)view.transform;
                var corners = new Vector3[4]; root.GetWorldCorners(corners);
                foreach (var corner in corners) Assert.That(parent.rect.Contains(parent.InverseTransformPoint(corner)), Is.True,
                    "Status icons stay inside player panel: " + parent.InverseTransformPoint(corner) + " in " + parent.rect);
                var bar = (RectTransform)parent.Find("PlayerHPBarBackground"); var hp = new Vector3[4]; bar.GetWorldCorners(hp);
                Assert.That(corners[1].y, Is.LessThan(hp[0].y - ShieldBarUI.ReservedSpaceBelowHealthBar), "Shield track has reserved space");
                foreach (var label in root.GetComponentsInChildren<TMP_Text>())
                { Assert.That(label.font, Is.SameAs(GameUi.TmpFont)); Assert.That(label.text, Is.EqualTo("2").Or.EqualTo("3")); }
                foreach (var icon in root.GetComponentsInChildren<Image>()) Assert.That(icon.sprite, Is.Not.Null);
                var errors = GameplayPixelLayoutValidator.Validate(Object.FindFirstObjectByType<GameplayPixelLayoutController>(), out string report);
                string name = "statuses-" + size.x + "x" + size.y + (inset ? "-safe" : "");
                File.WriteAllText(Path.Combine(output,name+".txt"),report+"\n"+string.Join("\n",errors));
                Assert.That(errors, Is.Empty);
                ScreenCapture.CaptureScreenshot(Path.Combine(output,name+".png")); yield return null; yield return null;
            }
        }
        finally { GameplayPixelLayoutController.ValidationSafeArea=null; Time.timeScale=1; }
    }
    [UnityTest] public IEnumerator GenericStatusCasterAppliesAtAcceptedContactWithoutProductionAssignment()
    {
        yield return Launch(0, true);
        var actor = Run.Waves.ActiveEnemies.First(e => !e.HasSpecialAbility);
        foreach (var enemy in Run.Waves.ActiveEnemies) enemy.GetComponent<EnemyAutoAttack>().StopAttacking();
        var original = actor.Definition; var fixture = Object.Instantiate(original);
        try
        {
            Set(fixture,"hasSpecialAbility",true); Set(fixture,"specialAbilityKind",EnemySpecialAbilityKind.ApplyPlayerStatus);
            fixture.appliedPlayerStatus=Resources.Load<PlayerStatusDefinition>("PlayerStatuses/Sapped");
            Set(actor,"definition",fixture); actor.SetSpecialTurnRequirement(1);
            EnemySpecialAbilityRuntimeFactory.CreateAndInitialize(EnemySpecialAbilityKind.ApplyPlayerStatus,actor.gameObject,actor,Run.Board,Run.Waves.ActiveEnemies);
            int casts=0; actor.SpecialAbilityUsed += _ => casts++;
            Assert.That(Run.Player.Statuses.Has(PlayerStatusKind.Sapped), Is.False);
            yield return Move();
            Assert.That(casts, Is.EqualTo(1));
            Assert.That(Run.Player.Statuses.Remaining(PlayerStatusKind.Sapped), Is.EqualTo(3), "Cast does not spend its birth action");
            Assert.That(CombatGuide.Enemy(actor), Does.Contain("Sapped"));
        }
        finally { Set(actor,"definition",original); Object.Destroy(fixture); }
    }
}

public sealed class PlayerStatusAssetTests
{
    [Test] public void NativeStatusIconsAndApprovedCasterAssignmentsMatchProduction()
    {
        var definitions = Resources.LoadAll<PlayerStatusDefinition>("PlayerStatuses");
        CollectionAssert.AreEquivalent(System.Enum.GetValues(typeof(PlayerStatusKind)), definitions.Select(data => data.kind).ToArray());
        foreach (var data in definitions)
        {
            int native = data.kind == PlayerStatusKind.Rattled ? 32 : 16;
            Assert.That(data.icon, Is.Not.Null, data.name);
            Assert.That(data.icon.rect.size, Is.EqualTo(new Vector2(native, native)), data.name);
            Assert.That(data.durationMoves, Is.EqualTo(data.kind == PlayerStatusKind.Rattled ? 2 : 3), data.name);
            var importer = (TextureImporter)AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(data.icon));
            Assert.That(importer.filterMode, Is.EqualTo(FilterMode.Point), data.name);
            Assert.That(importer.mipmapEnabled, Is.False, data.name);
            Assert.That(importer.textureCompression, Is.EqualTo(TextureImporterCompression.Uncompressed), data.name);
        }

        var enemies = AssetDatabase.FindAssets("t:EnemyDefinition", new[] { "Assets/_Game" })
            .Select(guid => AssetDatabase.LoadAssetAtPath<EnemyDefinition>(AssetDatabase.GUIDToAssetPath(guid))).ToArray();
        var casters = enemies.Where(enemy => enemy.appliedPlayerStatus != null ||
            enemy.SpecialAbilityKind == EnemySpecialAbilityKind.ApplyPlayerStatus ||
            enemy.SpecialAbilityKind == EnemySpecialAbilityKind.Faultline).ToArray();
        CollectionAssert.AreEquivalent(new[] { "frostblade_reaver", "seismic_smith" },
            casters.Select(enemy => enemy.EnemyId).ToArray());

        var frostbite = definitions.Single(data => data.kind == PlayerStatusKind.Frostbite);
        var reaver = casters.Single(enemy => enemy.EnemyId == "frostblade_reaver");
        Assert.That(reaver.HasSpecialAbility, Is.True);
        Assert.That(reaver.SpecialAbilityKind, Is.EqualTo(EnemySpecialAbilityKind.ApplyPlayerStatus));
        Assert.That(reaver.appliedPlayerStatus, Is.SameAs(frostbite));

        var smith = casters.Single(enemy => enemy.EnemyId == "seismic_smith");
        Assert.That(smith.HasSpecialAbility, Is.True);
        Assert.That(smith.SpecialAbilityKind, Is.EqualTo(EnemySpecialAbilityKind.Faultline));
        Assert.That(smith.appliedPlayerStatus, Is.SameAs(definitions.Single(data => data.kind == PlayerStatusKind.Rattled)));

        var sharedIce = Resources.Load<FrostwakeRules>("Zones/FrostwakeRules");
        Assert.That(sharedIce, Is.Not.Null);
        Assert.That(sharedIce.frostbite, Is.SameAs(frostbite), "Bifrost and Glacier Cleave use the shared Frostbite definition.");
        Assert.That(Resources.Load<ZoneDefinition>("Zones/frostwake-peaks").frostwakeRules, Is.SameAs(sharedIce));
    }
}
