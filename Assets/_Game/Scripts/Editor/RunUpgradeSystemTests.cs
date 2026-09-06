using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public sealed class RunUpgradeSystemTests
{
    private readonly List<GameObject> createdObjects = new List<GameObject>();

    [TearDown]
    public void TearDown()
    {
        for (int index = createdObjects.Count - 1; index >= 0; index--)
        {
            if (createdObjects[index] != null)
            {
                UnityEngine.Object.DestroyImmediate(createdObjects[index]);
            }
        }

        createdObjects.Clear();
    }

    [Test]
    public void UpgradeCadenceIsEveryFiveCompletedWaves()
    {
        for (int wave = 1; wave <= 4; wave++)
        {
            Assert.That(
                RunUpgradeCoordinator.ShouldOfferUpgradeAfterWave(wave),
                Is.False
            );
        }

        Assert.That(
            RunUpgradeCoordinator.ShouldOfferUpgradeAfterWave(5),
            Is.True
        );
        Assert.That(
            RunUpgradeCoordinator.ShouldOfferUpgradeAfterWave(10),
            Is.True
        );
    }

    [Test]
    public void DraftProducesThreeUniqueLegalChoicesWithoutEncounterRng()
    {
        Fixture fixture = CreateFixture("Players/Player_Skeleton", 71237);
        System.Random encounterRandomBefore = GetEncounterRandom(fixture.Waves);

        List<RunUpgradeDefinition> choices = UpgradeDraftGenerator.Generate(
            fixture.Runtime.Catalog,
            fixture.Runtime,
            fixture.Player,
            5,
            fixture.Runtime.GetDraftRandom()
        );

        Assert.That(choices.Count, Is.EqualTo(3));
        Assert.That(
            new HashSet<RunUpgradeDefinition>(choices).Count,
            Is.EqualTo(3)
        );
        Assert.That(
            choices.Exists(choice =>
                choice.UpgradeId == "prototype_horrible_encore"),
            Is.False
        );
        Assert.That(encounterRandomBefore, Is.Null);
        Assert.That(GetEncounterRandom(fixture.Waves), Is.Null);
    }

    [Test]
    public void AbilityEligibilityUsesStablePlayerAndAbilityIds()
    {
        Fixture skeleton = CreateFixture("Players/Player_Skeleton", 10);
        RunUpgradeDefinition encore = FindUpgrade(
            skeleton.Runtime.Catalog,
            "prototype_horrible_encore"
        );

        Assert.That(
            skeleton.Runtime.IsEligible(encore, skeleton.Player, 5),
            Is.False
        );

        TearDown();

        Fixture bardley = CreateFixture("Players/Player_Bardley", 10);
        encore = FindUpgrade(
            bardley.Runtime.Catalog,
            "prototype_horrible_encore"
        );

        Assert.That(
            bardley.Runtime.IsEligible(encore, bardley.Player, 5),
            Is.True
        );
    }

    [Test]
    public void StackCountModifiersAndMaximumHealthApplyAuthoritatively()
    {
        Fixture fixture = CreateFixture("Players/Player_Bardley", 20);
        RunUpgradeDefinition damage = FindUpgrade(
            fixture.Runtime.Catalog,
            "prototype_gem_grinder"
        );
        RunUpgradeDefinition health = FindUpgrade(
            fixture.Runtime.Catalog,
            "prototype_thicker_hide"
        );
        RunUpgradeDefinition cost = FindUpgrade(
            fixture.Runtime.Catalog,
            "prototype_efficient_casting"
        );
        int healthBefore = fixture.Player.MaximumHealth;
        int currentBefore = fixture.Player.CurrentHealth;

        Assert.That(fixture.Runtime.TryApply(damage, 5), Is.True);
        Assert.That(fixture.Runtime.TryApply(damage, 10), Is.True);
        Assert.That(
            RunUpgradeResolver.ResolveGemDamage(
                100,
                default,
                fixture.Runtime
            ),
            Is.EqualTo(130)
        );

        Assert.That(fixture.Runtime.TryApply(health, 5), Is.True);
        Assert.That(fixture.Player.MaximumHealth, Is.EqualTo(healthBefore + 20));
        Assert.That(fixture.Player.CurrentHealth, Is.EqualTo(currentBefore + 20));

        Assert.That(fixture.Runtime.TryApply(cost, 5), Is.True);
        Assert.That(
            RunUpgradeResolver.ResolveAbilityEnergyCost(
                fixture.Player.ActiveAbility.EnergyCost,
                fixture.Player.ActiveAbility,
                fixture.Runtime
            ),
            Is.EqualTo(72)
        );
    }

    [Test]
    public void MaxedUpgradeIsExcludedAndMissingRuntimePreservesBaseValues()
    {
        Assert.That(
            RunUpgradeResolver.ResolveAbilityEnergyGain(17),
            Is.EqualTo(17)
        );
        Assert.That(
            RunUpgradeResolver.ResolveAbilityEnergyCost(80, null),
            Is.EqualTo(80)
        );

        Fixture fixture = CreateFixture("Players/Player_Skeleton", 30);
        RunUpgradeDefinition siege = FindUpgrade(
            fixture.Runtime.Catalog,
            "prototype_siegebreaker"
        );

        for (int stack = 0; stack < siege.MaxStacks; stack++)
        {
            Assert.That(fixture.Runtime.TryApply(siege, 5), Is.True);
        }

        Assert.That(
            fixture.Runtime.IsEligible(siege, fixture.Player, 10),
            Is.False
        );
        Assert.That(
            RunUpgradeResolver.ResolveBarricadeDurabilityDamage(
                1,
                fixture.Runtime
            ),
            Is.EqualTo(1 + siege.MaxStacks)
        );
    }

    [Test]
    public void ExternalInputBlockUsesDisposableTokens()
    {
        GameObject boardObject = new GameObject("BoardTest");
        createdObjects.Add(boardObject);
        BoardController board = boardObject.AddComponent<BoardController>();

        IDisposable first = board.AcquireExternalInputBlock();
        IDisposable second = board.AcquireExternalInputBlock();

        Assert.That(board.IsExternalInputBlocked, Is.True);
        first.Dispose();
        Assert.That(board.IsExternalInputBlocked, Is.True);
        second.Dispose();
        Assert.That(board.IsExternalInputBlocked, Is.False);
    }

    private Fixture CreateFixture(string playerResource, int encounterSeed)
    {
        RunUpgradeCatalog catalog = Resources.Load<RunUpgradeCatalog>(
            "RunUpgrades/PrototypeRunUpgradeCatalog"
        );
        PlayerDefinition definition = Resources.Load<PlayerDefinition>(
            playerResource
        );

        Assert.That(catalog, Is.Not.Null);
        Assert.That(definition, Is.Not.Null);

        GameObject playerObject = new GameObject("UpgradeTestPlayer");
        createdObjects.Add(playerObject);
        PlayerActor player = playerObject.AddComponent<PlayerActor>();
        player.Initialize(definition);

        GameObject waveObject = new GameObject("UpgradeTestWaves");
        createdObjects.Add(waveObject);
        WaveController waves = waveObject.AddComponent<WaveController>();
        SetPrivateField(waves, "encounterSeed", encounterSeed);
        RunUpgradeRuntime runtime = waveObject.AddComponent<RunUpgradeRuntime>();
        runtime.Configure(catalog, player, waves);

        return new Fixture
        {
            Player = player,
            Waves = waves,
            Runtime = runtime
        };
    }

    private static RunUpgradeDefinition FindUpgrade(
        RunUpgradeCatalog catalog,
        string id)
    {
        for (int index = 0; index < catalog.Upgrades.Count; index++)
        {
            if (catalog.Upgrades[index] != null &&
                catalog.Upgrades[index].UpgradeId == id)
            {
                return catalog.Upgrades[index];
            }
        }

        Assert.Fail($"Upgrade '{id}' was not found in the prototype catalog.");
        return null;
    }

    private static System.Random GetEncounterRandom(WaveController waves)
    {
        return (System.Random)typeof(WaveController)
            .GetField(
                "encounterRandom",
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic
            )
            .GetValue(waves);
    }

    private static void SetPrivateField(
        object target,
        string fieldName,
        object value)
    {
        typeof(WaveController)
            .GetField(
                fieldName,
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic
            )
            .SetValue(target, value);
    }

    private sealed class Fixture
    {
        public PlayerActor Player;
        public WaveController Waves;
        public RunUpgradeRuntime Runtime;
    }
}
