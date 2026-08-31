using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(EnemyActor))]
public sealed class BarricadeEnemyAbility :
    MonoBehaviour,
    IEnemySpecialAbilityRuntime
{
    [Header("Runtime Debug Information")]
    [SerializeField]
    private int ownedBarricadeCount;

    private EnemyActor enemyActor;
    private BoardController boardController;

    private int ownerInstanceId;
    private bool ownershipReleased;

    public void InitializeSpecialAbility(
        EnemyActor initializedEnemy,
        BoardController initializedBoard,
        IReadOnlyList<EnemyActor> activeEnemies)
    {
        Unsubscribe();

        enemyActor = initializedEnemy;
        boardController = initializedBoard;
        ownershipReleased = false;

        ownerInstanceId =
            enemyActor != null
                ? enemyActor.GetInstanceID()
                : 0;

        RefreshOwnedBarricadeCount();

        if (enemyActor == null ||
            boardController == null ||
            enemyActor.Definition == null)
        {
            Debug.LogError(
                "BarricadeEnemyAbility requires an initialized EnemyActor " +
                "with an EnemyDefinition and a BoardController.",
                this
            );

            return;
        }

        Subscribe();
    }

    private void Subscribe()
    {
        if (enemyActor == null ||
            boardController == null)
        {
            return;
        }

        enemyActor.SpecialBecameReady -=
            HandleSpecialBecameReady;

        enemyActor.SpecialBecameReady +=
            HandleSpecialBecameReady;

        enemyActor.Defeated -=
            HandleEnemyDefeated;

        enemyActor.Defeated +=
            HandleEnemyDefeated;

        boardController.ValidPlayerMoveCompleted -=
            HandleValidPlayerMoveCompleted;

        boardController.ValidPlayerMoveCompleted +=
            HandleValidPlayerMoveCompleted;
    }

    private void Unsubscribe()
    {
        if (enemyActor != null)
        {
            enemyActor.SpecialBecameReady -=
                HandleSpecialBecameReady;

            enemyActor.Defeated -=
                HandleEnemyDefeated;
        }

        if (boardController != null)
        {
            boardController.ValidPlayerMoveCompleted -=
                HandleValidPlayerMoveCompleted;
        }
    }

    private void HandleSpecialBecameReady(
        EnemyActor readyEnemy)
    {
        if (readyEnemy == null ||
            readyEnemy != enemyActor ||
            readyEnemy.IsDefeated)
        {
            return;
        }

        TryPlaceBarricades();
    }

    private void HandleValidPlayerMoveCompleted(
        int completedMoveNumber)
    {
        if (enemyActor == null ||
            enemyActor.IsDefeated ||
            !enemyActor.IsSpecialReady)
        {
            return;
        }

        /*
         * Keep a ready charge when the exact board state has no legal target.
         * A later player move may open cells that make the placement valid.
         */
        TryPlaceBarricades();
    }

    private void TryPlaceBarricades()
    {
        if (enemyActor == null ||
            boardController == null ||
            enemyActor.Definition == null ||
            enemyActor.IsDefeated ||
            !enemyActor.IsSpecialReady)
        {
            return;
        }

        EnemyDefinition definition =
            enemyActor.Definition;

        RefreshOwnedBarricadeCount();

        int maximumOwned =
            Mathf.Max(
                1,
                definition.MaximumOwnedBarricades
            );

        if (ownedBarricadeCount >=
            maximumOwned)
        {
            /*
             * Do not stockpile a hidden ready cast while the board is already
             * at this enemy's cap. Once a barricade breaks, a fresh cadence is
             * required, matching the Crossbow Guard cap policy.
             */
            enemyActor.ResetSpecialCounter();
            return;
        }

        bool queued =
            boardController.TryQueuePlaceBarricades(
                enemyActor,
                definition.BarricadesPerUse,
                maximumOwned,
                definition.BarricadeDurability,
                definition.BarricadeStyle
            );

        if (!queued)
        {
            return;
        }

        enemyActor.ResetSpecialCounter();
    }

    private void HandleEnemyDefeated(
        EnemyActor defeatedEnemy)
    {
        ReleaseOwnershipWithoutRemovingBarricades();
        Unsubscribe();
    }

    private void ReleaseOwnershipWithoutRemovingBarricades()
    {
        if (ownershipReleased ||
            boardController == null ||
            ownerInstanceId == 0)
        {
            return;
        }

        ownershipReleased = true;

        /*
         * Barricades are persistent board obstacles: defeating their creator
         * does not remove them. Orphan their ownership instead so a recycled
         * Unity instance ID can never make old barricades count against a new
         * enemy's six-barricade cap.
         */
        boardController.OrphanBarricadesForOwner(
            ownerInstanceId
        );
    }

    private void RefreshOwnedBarricadeCount()
    {
        ownedBarricadeCount =
            boardController != null &&
            ownerInstanceId != 0
                ? boardController
                    .GetBarricadeCountForOwner(
                        ownerInstanceId
                    )
                : 0;
    }

    private void OnDestroy()
    {
        ReleaseOwnershipWithoutRemovingBarricades();
        Unsubscribe();
    }
}
