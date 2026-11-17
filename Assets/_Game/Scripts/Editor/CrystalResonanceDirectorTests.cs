#if UNITY_EDITOR
using System.Linq;
using NUnit.Framework;
using UnityEngine;

public sealed class CrystalResonanceDirectorTests
{
    [Test]
    public void EveryZoneAndTierHasLegalRepeatableCapstonesAfterUniquePoolExhaustion()
    {
        foreach (var zone in Resources.LoadAll<ZoneDefinition>("Zones"))
        {
            if (!zone.eligibleForLiveTravel) continue;
            for (int tier = 0; tier < 5; tier++)
            {
                var state = new CrystalResonanceSnapshot { version = 1, zoneId = zone.zoneId, visit = 4 };
                state.seenLeaders.AddRange(zone.enemies.Select(e => e.EnemyId));
                state.seenMechanics.AddRange(zone.enemies.Select(e => e.EnemyId));
                for (int seed = 1; seed <= 100; seed++)
                {
                    var recipe = WaveController.ChooseResonanceCapstone(zone, zone.resonanceTierStarts[tier], state, new SavedRandom(seed));
                    Assert.AreEqual(ZoneEncounterKind.Elite, recipe.resonanceKind, zone.zoneId);
                    Assert.IsTrue(WaveController.ResonanceFormationLegal(zone, recipe, tier, state, true));
                    Assert.IsTrue(recipe.members.All(e => zone.enemies.Contains(e)));
                }
            }
        }
    }
    [Test]
    public void KingRequiresRoyalExposureAndHasNoFixedDepthAppointment()
    {
        var zone = Resources.Load<ZoneDefinition>("Zones/dungeon");
        var state = new CrystalResonanceSnapshot { version = 1, visit = 3 };
        var king = zone.resonanceEncounters.Single(r => r.members.Any(e => e.EnemyId == "king"));
        Assert.IsFalse(WaveController.ResonanceFormationLegal(zone, king, 4, state, true));
        state.seenMechanics.AddRange(new[] { "royal_swordsman", "court_mage" });
        Assert.IsTrue(WaveController.ResonanceFormationLegal(zone, king, 4, state, true));
        bool other = false;
        for (int seed = 1; seed < 100; seed++)
            other |= WaveController.ChooseResonanceCapstone(zone, 35, state, new SavedRandom(seed)) != king;
        Assert.IsTrue(other);
    }
}
#endif
