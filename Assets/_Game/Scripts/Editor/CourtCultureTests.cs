using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

public sealed class CourtCultureTests
{
    [Serializable] private sealed class Recipes { public Recipe[] recipes; }
    [Serializable] private sealed class Recipe { public string id,name; public string[] members; public int first,last,tide,weight; }
    [Test] public void SerializedCulturesAndRecipesMatchReviewedSource()
    {
        var zone=Resources.Load<ZoneDefinition>("Zones/drowned-court");
        var additions = new[] { "nacre_duelist", "nacre_halberdier" };
        CollectionAssert.AreEquivalent(DrownedCourtImporter.Ids.Concat(additions), zone.enemies.Select(e => e.EnemyId));
        var original = zone.enemies.Where(e => DrownedCourtImporter.Ids.Contains(e.EnemyId)).ToArray();
        Assert.That(original.Length, Is.EqualTo(14));
        Assert.That(original.Count(e => e.Faction == "Reef Clans"), Is.EqualTo(10));
        Assert.That(original.Count(e => e.Faction == "Nacre Court"), Is.EqualTo(4));
        foreach (var enemy in original)
            Assert.That(enemy.Faction, Is.EqualTo(DrownedCourtImporter.CultureFor(enemy.EnemyId)), enemy.EnemyId);
        foreach (var id in additions)
            Assert.That(zone.enemies.Single(e => e.EnemyId == id).Faction, Is.EqualTo("Nacre Court"), id);
        var source=JsonUtility.FromJson<Recipes>(File.ReadAllText("ArtSource/DrownedCourt/Production/Selected/encounters.json")).recipes;
        Assert.That(zone.liveEncounters.Length,Is.EqualTo(source.Length));
        for(int i=0;i<source.Length;i++)
        {
            var actual=zone.liveEncounters[i];var expected=source[i];
            Assert.That(actual.label,Is.EqualTo(expected.id+": "+expected.name));
            CollectionAssert.AreEqual(expected.members,actual.members.Select(e=>e.EnemyId));
            Assert.That(actual.firstLocalWave,Is.EqualTo(expected.first));Assert.That(actual.lastLocalWave,Is.EqualTo(expected.last));
            Assert.That(actual.requiredTide,Is.EqualTo(expected.tide));Assert.That(actual.weight,Is.EqualTo(expected.weight));
        }
        var mixed=zone.liveEncounters.Where(r=>r.members.Select(e=>e.Faction).Distinct().Count()>1).ToArray();
        Assert.That(mixed.Length,Is.EqualTo(2));Assert.That(mixed.All(r=>r.weight==1),Is.True);
        foreach(var recipe in zone.liveEncounters.Where(r=>r.members.Contains(zone.apexEnemy)))
            Assert.That(recipe.members.All(e=>e.Faction=="Nacre Court"),Is.True);
        Assert.That(zone.apexEnemy.EnemyId,Is.EqualTo("queen_nacre"));
        Assert.That(zone.apexEnemy.aquaticSummon.EnemyId,Is.EqualTo("skittercrab"));
    }
    [Test] public void EveryTeachingBandRetainsDryAndWetFormationCoverage()
    {
        var zone=Resources.Load<ZoneDefinition>("Zones/drowned-court");
        for(int wave=1;wave<=25;wave++) foreach(int tide in new[]{1,2})
            Assert.That(zone.liveEncounters.Any(r=>r.firstLocalWave<=wave && r.lastLocalWave>=wave &&
                (r.requiredTide==0 || r.requiredTide==tide)),Is.True,"wave "+wave+" tide "+tide);
        Assert.That(zone.liveEncounters.Count(r=>r.members.All(e=>e.Faction=="Reef Clans")),Is.EqualTo(24));
        Assert.That(zone.liveEncounters.Count(r=>r.members.All(e=>e.Faction=="Nacre Court")),Is.EqualTo(6));
    }
}
