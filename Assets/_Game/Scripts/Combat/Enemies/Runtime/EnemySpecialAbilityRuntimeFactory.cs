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
            IReadOnlyList<EnemyActor> activeEnemies)
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
