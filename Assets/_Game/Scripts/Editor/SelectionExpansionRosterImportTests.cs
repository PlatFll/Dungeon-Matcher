using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public sealed class SelectionExpansionRosterImportTests
{
    [TestCase("drowned-court", "nacre_duelist", "iceborn_axeman")]
    [TestCase("drowned-court", "nacre_halberdier", "iceborn_axeman")]
    [TestCase("frostwake-peaks", "iceborn_axeman", "nacre_duelist")]
    public void ReimportRebuildsAuthoredKitsAndPreservesOnlyRegisteredLocalAdditions(
        string zoneId, string expansionId, string foreignId)
    {
        var zone = ScriptableObject.CreateInstance<ZoneDefinition>();
        var definitions = new List<EnemyDefinition>();
        EnemyDefinition Definition(string id)
        {
            var definition = ScriptableObject.CreateInstance<EnemyDefinition>();
            definitions.Add(definition);
            JsonUtility.FromJsonOverwrite("{\"enemyId\":\"" + id + "\"}", definition);
            return definition;
        }

        try
        {
            zone.zoneId = zoneId;
            var existing = Definition(expansionId);
            var duplicate = Definition(expansionId);
            var oldBase = Definition("base_member");
            var newBase = Definition("base_member");
            var newMember = Definition("new_authored_member");
            var removed = Definition("removed_member");
            var foreign = Definition(foreignId);
            zone.enemies = new[] { existing, duplicate, oldBase, removed, foreign };
            var authored = new[] { newMember, newBase, newBase };

            zone.enemies = SelectionExpansionRosterImport.Rebuild(zone, authored);
            CollectionAssert.AreEqual(new[] { existing, newMember, newBase }, zone.enemies);
            CollectionAssert.AreEqual(zone.enemies, SelectionExpansionRosterImport.Rebuild(zone, authored));

            var nowAuthored = Definition(expansionId);
            CollectionAssert.AreEqual(new[] { newBase, nowAuthored }, SelectionExpansionRosterImport.Rebuild(
                zone, new[] { newBase, nowAuthored }));
            zone.enemies = new[] { newMember, newBase };
            CollectionAssert.AreEqual(zone.enemies, SelectionExpansionRosterImport.Rebuild(zone, authored));
        }
        finally
        {
            foreach (var definition in definitions) UnityEngine.Object.DestroyImmediate(definition);
            UnityEngine.Object.DestroyImmediate(zone);
        }
    }

}
