using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

public sealed class ForestFoundationTests
{
    [Test] public void SaveProfilesAreExplicitAndUnknownVersionsAreRejected()
    {
        Assert.That(RunContinuation.SupportsSnapshot(new RunCombatSnapshot{version=1}),Is.True);
        var legacy=JsonUtility.FromJson<RunCombatSnapshot>(JsonUtility.ToJson(new RunCombatSnapshot{version=1}));
        Assert.That(RunContinuation.SupportsSnapshot(legacy),Is.True,"JsonUtility may materialize a null nested clock; schema 1 remains legacy");
        var saved=new RunCombatSnapshot{version=2,clock=new CombatClockSnapshot()};
        Assert.That(RunContinuation.SupportsSnapshot(saved),Is.True);
        saved.clock.profile="future";Assert.That(RunContinuation.SupportsSnapshot(saved),Is.False);
        saved.clock.profile=CombatClockSnapshot.MoveProfile;saved.clock.actions.Accept(1);
        Assert.That(RunContinuation.SupportsSnapshot(saved),Is.False,"in-flight consequences use accepted-input replay");
        saved.clock.actions.Commit(1);saved.clock.zoneId="missing";
        Assert.That(RunContinuation.SupportsSnapshot(saved),Is.False);
        saved.clock.zoneId="magical-forest";saved.clock.actions.nextActorId=3;
        saved.enemies.Add(new EnemyCombatSnapshot{persistentId=1});saved.enemies.Add(new EnemyCombatSnapshot{persistentId=1});
        Assert.That(RunContinuation.SupportsSnapshot(saved),Is.False,"slot reuse cannot alias persistent actors");
        saved.enemies[1].persistentId=2;Assert.That(RunContinuation.SupportsSnapshot(saved),Is.True);
    }
    [Test] public void ApprovedImportsKeepNativePixelsAndIndependentTaxonomy()
    {
        var zone=Resources.Load<ZoneDefinition>("Zones/magical-forest");Assert.That(zone,Is.Not.Null);
        Assert.That(zone.eligibleForLiveTravel,Is.False);Assert.That(zone.enemies.Length,Is.EqualTo(6));
        Assert.That(zone.developmentEncounters.SelectMany(e=>e.members).Distinct().Count(),Is.EqualTo(6));
        foreach(var enemy in zone.enemies)
        {
            string path=AssetDatabase.GetAssetPath(enemy.FallbackVisualSprite);
            string source="ArtSource/Forest/Approved/"+enemy.name+".png";
            CollectionAssert.AreEqual(File.ReadAllBytes(source),File.ReadAllBytes(path),enemy.name);
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);
            Assert.That(importer.filterMode,Is.EqualTo(FilterMode.Point));Assert.That(importer.mipmapEnabled,Is.False);
            Assert.That(importer.textureCompression,Is.EqualTo(TextureImporterCompression.Uncompressed));
            Assert.That(enemy.AnimationControllerOverride,Is.Not.Null);
            foreach(string state in new[]{"Idle","AutoAttack","Hit","Death"})
                Assert.That(enemy.AnimationControllerOverride.animationClips.Any(c=>c.name.EndsWith("_"+state)),Is.True,enemy.name+" "+state);
            Assert.That(enemy.Race,Is.Not.Empty);Assert.That(enemy.Faction,Is.Not.Empty);
            Assert.That(enemy.EligibleZones,Does.Contain("magical-forest"));Assert.That(enemy.FirstAttackMoves,Is.GreaterThan(0));
            Assert.That(enemy.FallbackVisualSprite.rect.width,Is.EqualTo(enemy.name=="Briar_Matriarch"?96:64));
        }
        foreach(var cell in zone.theme.boardCells) Assert.That(cell.rect.size,Is.EqualTo(new Vector2(64,64)));
        Assert.That(zone.theme.panelBackground,Is.Not.EqualTo(zone.theme.generalBackground));
    }
    [Test] public void ResonanceUsesOnlyCurrentGemAndEligiblePlayerSources()
    {
        var go=new GameObject("ZoneAttributionTest");
        try
        {
            var zone=go.AddComponent<ZoneRuntimeContext>();
            typeof(ZoneRuntimeContext).GetProperty("Definition").SetValue(zone,Resources.Load<ZoneDefinition>("Zones/magical-forest"));
            foreach(BoardClearSource source in Enum.GetValues(typeof(BoardClearSource)))
                foreach(GemType color in Enum.GetValues(typeof(GemType)))
                {
                    var clear=new BoardClearContext(color,3,2,source);
                    bool eligible=color==GemType.Emerald && (clear.IsMatchClear||clear.IsSpecialClear||source==BoardClearSource.Ability);
                    Assert.That(zone.EligibleDamageMultiplier(clear),Is.EqualTo(eligible?1.15f:1f));
                }
            Assert.That(zone.EligibleDamageMultiplier(new BoardClearContext(GemType.Emerald,0,0,BoardClearSource.Match)),Is.EqualTo(1));
        }
        finally { UnityEngine.Object.DestroyImmediate(go); }
    }
}
