using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(EnemyActor))]
public sealed class CourtMageEnemyAbility :
    MonoBehaviour,
    IEnemySpecialAbilityRuntime
{
    private const int MaximumOwnedFrozenGems = 3;

    [Header("Runtime Debug Information")]
    [SerializeField]
    private int ownedFrozenGemCount;

    private EnemyActor enemyActor;
    private BoardController boardController;
    private EnemySpecialActionAvailability specialActionAvailability;
    private int ownerInstanceId;
    private bool releaseQueued;

    public void InitializeSpecialAbility(
        EnemyActor initializedEnemy,
        BoardController initializedBoard,
        IReadOnlyList<EnemyActor> activeEnemies)
    {
        specialActionAvailability?.Dispose();
        Unsubscribe();

        enemyActor = initializedEnemy;
        boardController = initializedBoard;
        ownerInstanceId =
            enemyActor != null
                ? enemyActor.GetInstanceID()
                : 0;
        releaseQueued = false;

        RefreshOwnedFrozenGemCount();

        if (enemyActor == null ||
            boardController == null)
        {
            Debug.LogError(
                "CourtMageEnemyAbility requires an initialized EnemyActor " +
                "and BoardController.",
                this
            );
            return;
        }

        specialActionAvailability =
            new EnemySpecialActionAvailability(
                this,
                enemyActor,
                boardController,
                TryFreezeGem
            );

        Subscribe();
    }

    private void Subscribe()
    {
        enemyActor.SpecialBecameReady -= HandleSpecialBecameReady;
        enemyActor.SpecialBecameReady += HandleSpecialBecameReady;
        enemyActor.Defeated -= HandleEnemyDefeated;
        enemyActor.Defeated += HandleEnemyDefeated;

        boardController.ValidPlayerMoveCompleted -=
            HandleValidPlayerMoveCompleted;
        boardController.ValidPlayerMoveCompleted +=
            HandleValidPlayerMoveCompleted;
    }

    private void Unsubscribe()
    {
        if (enemyActor != null)
        {
            enemyActor.SpecialBecameReady -= HandleSpecialBecameReady;
            enemyActor.Defeated -= HandleEnemyDefeated;
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
        if (readyEnemy == enemyActor &&
            readyEnemy != null &&
            !readyEnemy.IsDefeated)
        {
            specialActionAvailability?.RequestExecution();
        }
    }

    private void HandleValidPlayerMoveCompleted(
        int completedMoveNumber)
    {
        if (enemyActor != null &&
            !enemyActor.IsDefeated &&
            enemyActor.IsSpecialReady)
        {
            specialActionAvailability?.RequestExecution();
        }
    }

    private bool TryFreezeGem()
    {
        if (enemyActor == null ||
            boardController == null ||
            enemyActor.IsDefeated ||
            !enemyActor.IsSpecialReady)
        {
            return false;
        }

        RefreshOwnedFrozenGemCount();

        if (ownedFrozenGemCount >= MaximumOwnedFrozenGems)
        {
            /*
             * Reaching the three-gem cap consumes this cadence instead of
             * stockpiling an instant fourth freeze. Once ice is cleared, the
             * Mage must charge a fresh cast.
             */
            enemyActor.ResetSpecialCounter();
            return true;
        }

        bool queued =
            boardController.TryQueueFreezeRandomGem(
                enemyActor,
                MaximumOwnedFrozenGems
            );

        if (!queued)
        {
            return false;
        }

        enemyActor.ResetSpecialCounter();
        return true;
    }

    private void HandleEnemyDefeated(
        EnemyActor defeatedEnemy)
    {
        specialActionAvailability?.Dispose();
        QueueOwnedFreezeRelease();
        Unsubscribe();
    }

    private void QueueOwnedFreezeRelease()
    {
        if (releaseQueued ||
            boardController == null ||
            ownerInstanceId == 0)
        {
            return;
        }

        RefreshOwnedFrozenGemCount();

        if (ownedFrozenGemCount <= 0)
        {
            return;
        }

        releaseQueued = true;
        boardController.QueueReleasePinnedGems(ownerInstanceId);
    }

    private void RefreshOwnedFrozenGemCount()
    {
        ownedFrozenGemCount =
            boardController != null &&
            ownerInstanceId != 0
                ? boardController.GetFrozenGemCountForOwner(
                    ownerInstanceId)
                : 0;
    }

    private void OnDestroy()
    {
        specialActionAvailability?.Dispose();
        QueueOwnedFreezeRelease();
        Unsubscribe();
    }
}
