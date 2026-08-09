using UnityEngine;

public static class EnemySpecialAbilityRuntimeFactory
{
    public static IEnemySpecialAbilityRuntime
        CreateAndInitialize(
            EnemySpecialAbilityKind abilityKind,
            GameObject enemyObject,
            EnemyActor enemyActor,
            BoardController boardController)
    {
        if (abilityKind ==
            EnemySpecialAbilityKind.None)
        {
            return null;
        }

        if (enemyObject == null ||
            enemyActor == null ||
            boardController == null)
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
            boardController
        );

        return runtime;
    }
}
