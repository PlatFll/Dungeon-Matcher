using System.Collections.Generic;
using UnityEngine;

public static class EnemySpecialAbilityRuntimeFactory
{
    public static IEnemySpecialAbilityRuntime
        CreateAndInitialize(
            EnemySpecialAbilityKind abilityKind,
            GameObject enemyObject,
            EnemyActor enemyActor,
            BoardController boardController,
            IReadOnlyList<EnemyActor> activeEnemies,
            IEnemySummonService summonService = null)
    {
        if (abilityKind ==
            EnemySpecialAbilityKind.None)
        {
            return null;
        }

        if (enemyObject == null ||
            enemyActor == null ||
            boardController == null ||
            activeEnemies == null)
        {
            Debug.LogError(
                "Cannot create an enemy special ability runtime " +
                "without an enemy object, EnemyActor and BoardController."
            );

            return null;
        }

        IEnemySpecialAbilityRuntime runtime;

        switch (abilityKind)
        {
            case EnemySpecialAbilityKind.KnightCaptain:
                var captain = enemyObject.GetComponent<KnightCaptainEnemyAbility>();
                if (captain == null) captain = enemyObject.AddComponent<KnightCaptainEnemyAbility>();
                runtime = captain;
                break;
            case EnemySpecialAbilityKind.Miner:
                MinerEnemyAbility minerAbility =
                    enemyObject.GetComponent<
                        MinerEnemyAbility
                    >();

                if (minerAbility == null)
                {
                    minerAbility =
                        enemyObject.AddComponent<
                            MinerEnemyAbility
                        >();
                }

                runtime = minerAbility;
                break;

            case EnemySpecialAbilityKind.CrossbowGuardBolt:
                CrossbowGuardEnemyAbility crossbowAbility =
                    enemyObject.GetComponent<
                        CrossbowGuardEnemyAbility
                    >();

                if (crossbowAbility == null)
                {
                    crossbowAbility =
                        enemyObject.AddComponent<
                            CrossbowGuardEnemyAbility
                        >();
                }

                runtime = crossbowAbility;
                break;

            case EnemySpecialAbilityKind.Barricade:
                BarricadeEnemyAbility barricadeAbility =
                    enemyObject.GetComponent<
                        BarricadeEnemyAbility
                    >();

                if (barricadeAbility == null)
                {
                    barricadeAbility =
                        enemyObject.AddComponent<
                            BarricadeEnemyAbility
                        >();
                }

                runtime = barricadeAbility;
                break;

            case EnemySpecialAbilityKind.ShieldingAllies:
                ShieldingAlliesEnemyAbility shieldingAbility =
                    enemyObject.GetComponent<
                        ShieldingAlliesEnemyAbility
                    >();

                if (shieldingAbility == null)
                {
                    shieldingAbility =
                        enemyObject.AddComponent<
                            ShieldingAlliesEnemyAbility
                        >();
                }

                runtime = shieldingAbility;
                break;

            case EnemySpecialAbilityKind.TownMarshal:
                if (summonService == null)
                {
                    Debug.LogError(
                        "Town Marshal ability requires an enemy summon service.",
                        enemyObject
                    );

                    return null;
                }

                TownMarshalEnemyAbility marshalAbility =
                    enemyObject.GetComponent<
                        TownMarshalEnemyAbility
                    >();

                if (marshalAbility == null)
                {
                    marshalAbility =
                        enemyObject.AddComponent<
                            TownMarshalEnemyAbility
                        >();
                }

                marshalAbility.ConfigureSummonService(
                    summonService
                );

                runtime = marshalAbility;
                break;

            case EnemySpecialAbilityKind.SiegeSergeant:
                SiegeSergeantEnemyAbility sergeant =
                    enemyObject.GetComponent<SiegeSergeantEnemyAbility>();
                if (sergeant == null)
                    sergeant = enemyObject.AddComponent<SiegeSergeantEnemyAbility>();
                runtime = sergeant;
                break;

            default:
                Debug.LogError(
                    $"No runtime factory is registered for " +
                    $"enemy ability kind {abilityKind}.",
                    enemyObject
                );

                return null;
        }

        runtime.InitializeSpecialAbility(
            enemyActor,
            boardController,
            activeEnemies
        );

        return runtime;
    }
}
